/*                              
   Copyright 2010 Nils Kopal, Viktor M.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WorkspaceManager.Model;
using System.Threading;
using System.Collections;
using Cryptool.PluginBase;
using System.Reflection;
using Gears4Net;
using System.Windows.Threading;
using System.Runtime.Remoting.Contexts;

namespace WorkspaceManager.Execution
{
    /// <summary>
    /// Engine to execute a model of the WorkspaceManager
    /// This class needs a WorkspaceManager to be instantiated
    /// To run an execution process it also needs a WorkspaceModel
    /// 
    /// This class uses Gears4Net to execute the plugins
    /// </summary>
    public class ExecutionEngine
    {
        private WorkspaceManager WorkspaceManagerEditor;
        private Scheduler[] schedulers;
        private WorkspaceModel workspaceModel;
        private volatile bool isRunning = false;

        public long ExecutedPluginsCounter { get; set; }
        public bool BenchmarkPlugins { get; set; }
        public long GuiUpdateInterval { get; set; }
        public int SleepTime { get; set; }

        /// <summary>
        /// Creates a new ExecutionEngine
        /// </summary>
        /// <param name="workspaceManagerEditor"></param>
        public ExecutionEngine(WorkspaceManager workspaceManagerEditor)
        {
            WorkspaceManagerEditor = workspaceManagerEditor;
        }

        /// <summary>
        /// Is this ExecutionEngine running?
        /// </summary>
        public bool IsRunning
        {
            get{return this.isRunning;}
            private set{this.isRunning = value;}
        }

        /// <summary>
        /// Execute the given Model
        /// </summary>
        /// <param name="workspaceModel"></param>
        public void Execute(WorkspaceModel workspaceModel)
        {
            if (!IsRunning)
            {
                IsRunning = true;
                this.workspaceModel = workspaceModel;
                int amountSchedulers = System.Environment.ProcessorCount * 2;

                //Here we create n = "ProcessorsCount * 2" Gears4Net schedulers
                //We do this, because measurements showed that we get the best performance if we
                //use this amount of schedulers
                schedulers = new Scheduler[amountSchedulers];
                for (int i = 0; i < amountSchedulers; i++)
                {
                    schedulers[i] = new WorkspaceManagerScheduler("WorkspaceManagerScheduler-" + i);
                    ((WorkspaceManagerScheduler)schedulers[i]).executionEngine = this;
                }
               
                //We have to reset all states of PluginModels, ConnectorModels and ConnectionModels:
                workspaceModel.resetStates();

                //The UpdateGuiProtocol is a kind of "daemon" which will update the view elements if necessary
                UpdateGuiProtocol updateGuiProtocol = new UpdateGuiProtocol(schedulers[0], workspaceModel, this);
                schedulers[0].AddProtocol(updateGuiProtocol);
                updateGuiProtocol.Start();

                //The BenchmarkProtocl counts the amount of executed plugins per seconds and writes this to debug
                if (this.BenchmarkPlugins)
                {
                    BenchmarkProtocol benchmarkProtocol = new BenchmarkProtocol(schedulers[0], this.workspaceModel, this);
                    schedulers[0].AddProtocol(benchmarkProtocol);
                    benchmarkProtocol.Start();
                }

                //Here we create for each PluginModel an own PluginProtocol
                //By using round-robin we give each protocol to another scheduler to gain
                //a good average load balancing of the schedulers
                //we also initalize each plugin
                //It is possible that a plugin is also a PluginProtocol 
                //if that is true we do not create a new one but use the plugin instead the created one
                int counter=0;
                foreach (PluginModel pluginModel in workspaceModel.AllPluginModels)
                {
                    PluginProtocol pluginProtocol = null;

                    if (pluginModel.Plugin is PluginProtocol)
                    {
                        pluginProtocol = (PluginProtocol)pluginModel.Plugin;
                        pluginProtocol.setExecutionEngineSettings(schedulers[counter], pluginModel, this);
                    }
                    else
                    {
                        pluginProtocol = new PluginProtocol(schedulers[counter], pluginModel, this);
                    }

                    pluginModel.Plugin.PreExecution();                    
                    pluginModel.PluginProtocol = pluginProtocol;
                    schedulers[counter].AddProtocol(pluginProtocol);

                    if (pluginProtocol.Status == ProtocolStatus.Created || pluginProtocol.Status == ProtocolStatus.Terminated)
                    {
                        pluginProtocol.Start();
                    }

                    counter = (counter + 1) % (amountSchedulers);

                    if (pluginModel.Startable)
                    {
                        MessageExecution msg = new MessageExecution();
                        msg.PluginModel = pluginModel;
                        pluginProtocol.BroadcastMessageReliably(msg);
                    }
                }

                foreach (Scheduler scheduler in schedulers)
                {
                    ((WorkspaceManagerScheduler)scheduler).startScheduler();
                }
            }
        }      
      
        /// <summary>
        /// Stop the execution process:
        /// calls shutdown on all schedulers + calls stop() on each plugin
        /// </summary>
        public void Stop()
        {
            //First stop alle plugins
            foreach (PluginModel pluginModel in workspaceModel.AllPluginModels)
            {
                pluginModel.Plugin.Stop();
                pluginModel.Plugin.PostExecution();
            }            

            IsRunning = false;
            //Secondly stop all Gears4Net Schedulers
            foreach (Scheduler scheduler in schedulers)
            {
                scheduler.Shutdown();
            }
             
        }

        /// <summary>
        /// Pause the execution
        /// </summary>
        public void Pause()
        {
            //not implemented yet
        }

        /// <summary>
        /// Use the logger of the WorkspaceManagerEditor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        public void GuiLogMessage(string message, NotificationLevel level)
        {           
            WorkspaceManagerEditor.GuiLogMessage(message, level);
        }            
    }
 
    /// <summary>
    /// Message send to scheduler for a Plugin to trigger the Execution
    /// </summary>
    public class MessageExecution : MessageBase
    {
        public PluginModel PluginModel;
    }

    /// <summary>
    /// A Protocol for updating the GUI in time intervals
    /// </summary>
    public class UpdateGuiProtocol : ProtocolBase
    {
        private WorkspaceModel workspaceModel;
        private ExecutionEngine executionEngine;      

        /// <summary>
        /// Create a new protocol. Each protocol requires a scheduler which provides
        /// a thread for execution.
        /// </summary>
        /// <param name="scheduler"></param>
        public UpdateGuiProtocol(Scheduler scheduler, WorkspaceModel workspaceModel, ExecutionEngine executionEngine)
            : base(scheduler)
        {
            this.workspaceModel = workspaceModel;
            this.executionEngine = executionEngine;            
        }

        /// <summary>
        /// The main function of the protocol
        /// </summary>
        /// <param name="stateMachine"></param>
        /// <returns></returns>
        public override System.Collections.Generic.IEnumerator<ReceiverBase> Execute(AbstractStateMachine stateMachine)
        {
            while (this.executionEngine.IsRunning)
            {
                yield return Timeout(this.executionEngine.GuiUpdateInterval, HandleUpdateGui);
            }
        }

        /// <summary>
        /// Handler function for a message.
        /// This handler must not block, because it executes inside the thread of the scheduler.
        /// </summary>
        /// <param name="msg"></param>
        private void HandleUpdateGui()
        {
            //Get the gui Thread
            this.workspaceModel.WorkspaceManagerEditor.Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                foreach (PluginModel pluginModel in workspaceModel.AllPluginModels)
                {
                    if (pluginModel.GuiNeedsUpdate)
                    {
                        pluginModel.GuiNeedsUpdate = false;
                        pluginModel.paint();
                        if (pluginModel.UpdateableView != null)
                        {
                            pluginModel.UpdateableView.update();
                        }
                    }
                }
                foreach (ConnectionModel connectionModel in workspaceModel.AllConnectionModels)
                {
                    if (connectionModel.GuiNeedsUpdate)
                    {
                        if (connectionModel.UpdateableView != null)
                        {
                            connectionModel.UpdateableView.update();
                        }
                    }
                }
            }
            , null);
        }
    }
   
    /// <summary>
    /// A Protocol for benchmarking
    /// </summary>
    public class BenchmarkProtocol : ProtocolBase
    {
        private WorkspaceModel workspaceModel;
        private ExecutionEngine executionEngine;

        /// <summary>
        /// Create a new protocol. Each protocol requires a scheduler which provides
        /// a thread for execution.
        /// </summary>
        /// <param name="scheduler"></param>
        public BenchmarkProtocol(Scheduler scheduler, WorkspaceModel workspaceModel, ExecutionEngine executionEngine)
            : base(scheduler)
        {
            this.workspaceModel = workspaceModel;
            this.executionEngine = executionEngine;
        }

        /// <summary>
        /// The main function of the protocol
        /// </summary>
        /// <param name="stateMachine"></param>
        /// <returns></returns>
        public override System.Collections.Generic.IEnumerator<ReceiverBase> Execute(AbstractStateMachine stateMachine)
        {
            while (this.executionEngine.IsRunning)
            {
                yield return Timeout(1000, HandleBenchmark);
            }
        }

        /// <summary>
        /// Handler function for a message.
        /// This handler must not block, because it executes inside the thread of the scheduler.
        /// </summary>
        /// <param name="msg"></param>
        private void HandleBenchmark()
        {
            this.workspaceModel.WorkspaceManagerEditor.GuiLogMessage("Executing at about " + this.executionEngine.ExecutedPluginsCounter + " Plugins/s", NotificationLevel.Debug);
            this.executionEngine.ExecutedPluginsCounter = 0;
        }

    }

    /// <summary>
    /// A Protocol for a PluginModel
    /// </summary>
    public class PluginProtocol : ProtocolBase
    {        
        public PluginModel PluginModel;
        private ExecutionEngine executionEngine;

        /// <summary>
        /// Create a new protocol. Each protocol requires a scheduler which provides
        /// a thread for execution.
        /// </summary>
        /// <param name="scheduler"></param>
        public PluginProtocol(Scheduler scheduler, PluginModel pluginModel,ExecutionEngine executionEngine)
            : base(scheduler)
        {
            this.PluginModel = pluginModel;
            this.executionEngine = executionEngine;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="pluginModel"></param>
        /// <param name="executionEngine"></param>
        public void setExecutionEngineSettings(Scheduler scheduler, PluginModel pluginModel, ExecutionEngine executionEngine)
        {
            this.Scheduler = scheduler;
            this.PluginModel = pluginModel;
            this.executionEngine = executionEngine;
        }


        /// <summary>
        /// The main function of the protocol     
        /// </summary>
        /// <param name="stateMachine"></param>
        /// <returns></returns>
        public override System.Collections.Generic.IEnumerator<ReceiverBase> Execute(AbstractStateMachine stateMachine)
        {
            while (this.executionEngine.IsRunning)
            {
                yield return Receive<MessageExecution>(null, HandleExecute);
            }
        }

        /// <summary>
        /// Handle an execution of a plugin
        /// </summary>
        /// <param name="msg"></param>
        private void HandleExecute(MessageExecution msg)
        {
            // 1. Check if Plugin may Execute
            if (!mayExecute(PluginModel))
            {
                return;
            }

            //2. Fill all Inputs of the plugin, if this fails, stop executing the plugin
            if (!fillInputs())
            {
                return;
            }

            //3. Execute the Plugin -> call the IPlugin.Execute()
            try
            {
                PluginModel.Plugin.Execute();
            }
            catch (Exception ex)
            {
                this.PluginModel.WorkspaceModel.WorkspaceManagerEditor.GuiLogMessage("An error occured while executing  \"" + PluginModel.Name + "\": " + ex.Message, NotificationLevel.Error);
                this.PluginModel.State = PluginModelState.Error;
                this.PluginModel.GuiNeedsUpdate = true;
                return;
            }

            //4. Count for the benchmark
            if (this.executionEngine.BenchmarkPlugins)
            {
                this.executionEngine.ExecutedPluginsCounter++;
            }

            //5. If the user wants to, sleep some time
            if (this.executionEngine.SleepTime > 0)
            {
                Thread.Sleep(this.executionEngine.SleepTime);
            }

            //6. Clear all used inputs
            //clearInputs();

            //7. Send execute messages to possible executable next plugins
            //runNextPlugins();
        }

        /// <summary>
        /// Send execute messages to possible executable next plugins
        /// </summary>
        public void runNextPlugins()
        {            
            foreach (ConnectorModel connectorModel in PluginModel.InputConnectors)
            {
                foreach (ConnectionModel connectionModel in connectorModel.InputConnections)
                {
                    if (!connectionModel.From.PluginModel.Startable ||
                        (connectionModel.From.PluginModel.Startable && connectionModel.From.PluginModel.RepeatStart))
                    {
                        if (mayExecute(connectionModel.From.PluginModel))
                        {
                            MessageExecution message_exec = new MessageExecution();
                            message_exec.PluginModel = connectionModel.From.PluginModel;
                            connectionModel.From.PluginModel.PluginProtocol.BroadcastMessageReliably(message_exec);
                        }
                    }
                }
            }
        }
      
        /// <summary>
        /// Delete all input data of inputs of the plugin
        /// </summary>
        public void clearInputs()
        {
            foreach (ConnectorModel connectorModel in PluginModel.InputConnectors)
            {
                if (connectorModel.HasData)
                {
                    connectorModel.Data = null;
                    connectorModel.HasData = false;
                    connectorModel.GuiNeedsUpdate = true;

                    foreach (ConnectionModel connectionModel in connectorModel.InputConnections)
                    {
                        connectionModel.Active = false;
                        connectorModel.GuiNeedsUpdate = true;
                    }
                }
            }
        }

        /// <summary>
        /// Fill all inputs of the plugin
        /// </summary>
        /// <returns></returns>
        public bool fillInputs()
        {
            //Fill the plugins inputs with data
            foreach (ConnectorModel connectorModel in PluginModel.InputConnectors)
            {
                try
                {
                    if (connectorModel.HasData && connectorModel.Data.value != null)
                    {
                        if (connectorModel.IsDynamic)
                        {
                            MethodInfo propertyInfo = PluginModel.Plugin.GetType().GetMethod(connectorModel.DynamicSetterName);
                            propertyInfo.Invoke(PluginModel.Plugin, new object[] { connectorModel.PropertyName, connectorModel.Data.value });
                        }
                        else
                        {
                            PropertyInfo propertyInfo = PluginModel.Plugin.GetType().GetProperty(connectorModel.PropertyName);
                            propertyInfo.SetValue(PluginModel.Plugin, connectorModel.Data.value, null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.PluginModel.WorkspaceModel.WorkspaceManagerEditor.GuiLogMessage("An error occured while setting value of connector \"" + connectorModel.Name + "\" of \"" + PluginModel.Name + "\": " + ex.Message, NotificationLevel.Error);
                    this.PluginModel.State = PluginModelState.Error;
                    this.PluginModel.GuiNeedsUpdate = true;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool mayExecute()
        {
            return mayExecute(this.PluginModel);
        }

        /// <summary>
        /// Check if the PluginModel may execute
        /// </summary>
        /// <param name="pluginModel"></param>
        /// <returns></returns>
        private bool mayExecute(PluginModel pluginModel)
        {
            if (!pluginModel.WorkspaceModel.WorkspaceManagerEditor.isExecuting())
            {
                return false;
            }

            //Check if all necessary inputs are set
            foreach (ConnectorModel connectorModel in pluginModel.InputConnectors)
            {
                if (!connectorModel.IControl &&
                    (connectorModel.IsMandatory || connectorModel.InputConnections.Count > 0) && (!connectorModel.HasData ||
                    connectorModel.Data == null))
                {
                    return false;
                }
            }

            //Check if all outputs are free
            foreach (ConnectorModel connectorModel in pluginModel.OutputConnectors)
            {
                if (!connectorModel.IControl)
                {
                    foreach (ConnectionModel connectionModel in connectorModel.OutputConnections)
                    {
                        if (connectionModel.To.HasData)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public ExecutionEngine ExecutionEngine
        {
            get { return this.executionEngine; }            
        }
    }

    /// <summary>
    /// Gears4Net Scheduler
    /// </summary>
    public class WorkspaceManagerScheduler : Scheduler
    {
        private System.Threading.AutoResetEvent wakeup = new System.Threading.AutoResetEvent(false);
        private bool shutdown = false;
        private System.Threading.Thread thread;
        private Context currentContext;
        public ExecutionEngine executionEngine = null;

		public WorkspaceManagerScheduler() : this(String.Empty)
		{

		}

        public WorkspaceManagerScheduler(string name)
            : base()
        {
            this.currentContext = Thread.CurrentContext;

            thread = new System.Threading.Thread(this.Start);
            thread.SetApartmentState(System.Threading.ApartmentState.MTA);
			thread.Name = name;
            
        }

        public void startScheduler()
        {
            thread.Start();
        }

        private void Start()
        {
            if (this.currentContext != Thread.CurrentContext){
                this.currentContext.DoCallBack(Start);}
            
            this.executionEngine.GuiLogMessage("Scheduler " + this.thread.Name + " up and running", NotificationLevel.Debug);

            // Loop forever
            while (true)
            {
                this.wakeup.WaitOne();

                // Loop while there are more protocols waiting
                while (true)
                {
                    // Should the scheduler stop?
                    if (this.shutdown)
                    {
                        this.executionEngine.GuiLogMessage("Scheduler " + this.thread.Name + " terminated", NotificationLevel.Debug);
                        return;
                    }
                    
                    ProtocolBase protocol = null;
                    lock (this)
                    {
                        // No more protocols? -> Wait
                        if (this.waitingProtocols.Count == 0)
                            break;
                    }

                    try
                    {
                        protocol = this.waitingProtocols.Dequeue();
                        ProtocolStatus status = protocol.Run();

                        lock (this)
                        {
                            switch (status)
                            {
                                case ProtocolStatus.Created:
                                    System.Diagnostics.Debug.Assert(false);
                                    break;
                                case ProtocolStatus.Ready:
                                    this.waitingProtocols.Enqueue(protocol);
                                    break;
                                case ProtocolStatus.Waiting:
                                    break;
                                case ProtocolStatus.Terminated:
                                    System.Diagnostics.Debug.Assert(!this.waitingProtocols.Contains(protocol));
                                    this.RemoveProtocol(protocol);
                                    break;
                            }
                        }
                    }
                    catch (Exception ex) 
                    {
                        System.Diagnostics.Debug.Fail("Error during scheduling: " + ex.Message + " - " + ex.InnerException);
                    }
                }
            }
        }

        /// <summary>
        /// Removes a protocol from the internal queue
        /// </summary>
        /// <param name="protocol"></param>
        public override void RemoveProtocol(ProtocolBase protocol)
        {
            lock (this)
            {
                this.protocols.Remove(protocol);
                if (this.protocols.Count == 0)
                    this.Shutdown();
            }
        }

        /// <summary>
        /// Adds a protocol to the internal queue
        /// </summary>
        /// <param name="protocol"></param>
        public override void AddProtocol(ProtocolBase protocol)
        {
            lock (this)
            {
                this.protocols.Add(protocol);
            }
        }

        /// <summary>
        /// Wakeup this scheduler
        /// </summary>
        /// <param name="protocol"></param>
        public override void Wakeup(ProtocolBase protocol)
        {
            lock (this)
            {
                if (!this.waitingProtocols.Contains(protocol))
                    this.waitingProtocols.Enqueue(protocol);
                this.wakeup.Set();
            }
        }

        /// <summary>
        /// Terminates the scheduler
        /// </summary>
        public override void Shutdown()
        {
            this.shutdown = true;
            this.wakeup.Set();
        }
    }
}
