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
using System.IO;
using System.Diagnostics;

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
        private Scheduler scheduler;
        private WorkspaceModel workspaceModel;
        private volatile bool isRunning = false;
        private BenchmarkProtocol BenchmarkProtocol = null;

        public volatile int ExecutedPluginsCounter = 0;
        public bool BenchmarkPlugins = false;
        public int GuiUpdateInterval = 0;
        public int SleepTime = 0;
        public int ThreadPriority = 0;

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
        public void Execute(WorkspaceModel workspaceModel, int amountThreads)
        {
            if (!IsRunning)
            {
                IsRunning = true;
                this.workspaceModel = workspaceModel;

                if (amountThreads <= 0)
                {
                    amountThreads = 1;
                }

                scheduler = new WorkspaceManagerScheduler("WorkspaceManagerScheduler", amountThreads,this);
               
                //We have to reset all states of PluginModels, ConnectorModels and ConnectionModels:
                workspaceModel.resetStates();

                //The UpdateGuiProtocol is a kind of "daemon" which will update the view elements if necessary
                UpdateGuiProtocol updateGuiProtocol = new UpdateGuiProtocol(scheduler, workspaceModel, this);
                scheduler.AddProtocol(updateGuiProtocol);
                updateGuiProtocol.Start();

                //The BenchmarkProtocl counts the amount of executed plugins per seconds and writes this to debug
                if (this.BenchmarkPlugins)
                {
                    BenchmarkProtocol = new BenchmarkProtocol(scheduler, this.workspaceModel, this);
                    scheduler.AddProtocol(BenchmarkProtocol);
                    BenchmarkProtocol.Start();
                }

                //Here we create for each PluginModel an own PluginProtocol
                //By using round-robin we give each protocol to another scheduler to gain
                //a good average load balancing of the schedulers
                //we also initalize each plugin
                //It is possible that a plugin is also a PluginProtocol 
                //if that is true we do not create a new one but use the plugin instead the created one                
                foreach (PluginModel pluginModel in workspaceModel.AllPluginModels)
                {
                    PluginProtocol pluginProtocol = new PluginProtocol(scheduler, pluginModel, this);
                    MessageExecution message = new MessageExecution();
                    message.PluginModel = pluginModel;
                    pluginModel.MessageExecution = message;

                    pluginModel.Plugin.PreExecution();                    
                    pluginModel.PluginProtocol = pluginProtocol;
                    scheduler.AddProtocol(pluginProtocol);

                    if (pluginProtocol.Status == ProtocolStatus.Created || pluginProtocol.Status == ProtocolStatus.Terminated)
                    {
                        pluginProtocol.Start();
                    }
                  
                    if (pluginModel.Startable)
                    {
                        pluginProtocol.BroadcastMessage(pluginModel.MessageExecution);
                    }
                }

                ((WorkspaceManagerScheduler)scheduler).startScheduling();
                
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
            }

            IsRunning = false;
            //Secondly stop all Gears4Net Schedulers
            scheduler.Shutdown();

            //call all PostExecution methods of all plugins
            foreach (PluginModel pluginModel in workspaceModel.AllPluginModels)
            {
                pluginModel.Plugin.PostExecution();
            }

            //remove the plugin protocol of each plugin model
            foreach (PluginModel pluginModel in workspaceModel.AllPluginModels)
            {
                pluginModel.PluginProtocol = null;
            }

            this.WorkspaceManagerEditor = null;
            this.workspaceModel = null;
          
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
            if (WorkspaceManagerEditor != null)
            {
                WorkspaceManagerEditor.GuiLogMessage(message, level);
            }
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
            yield return new IntervalReceiver(this.executionEngine.GuiUpdateInterval,this.executionEngine.GuiUpdateInterval, HandleUpdateGui);
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
                List<PluginModel> pluginModels = workspaceModel.AllPluginModels;
                foreach (PluginModel pluginModel in pluginModels)
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
                List<ConnectionModel> connectionModels = workspaceModel.AllConnectionModels;
                foreach (ConnectionModel connectionModel in connectionModels)
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
            yield return new IntervalReceiver(1000,1000, HandleBenchmark);            
        }

        /// <summary>
        /// Handler function for a message.
        /// This handler must not block, because it executes inside the thread of the scheduler.
        /// </summary>
        /// <param name="msg"></param>
        private void HandleBenchmark()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Executing at about ");
            sb.Append(this.executionEngine.ExecutedPluginsCounter); 
            sb.Append(" Plugins/s");

            this.workspaceModel.WorkspaceManagerEditor.GuiLogMessage(sb.ToString(), NotificationLevel.Debug);          
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
            yield return new PersistentReceiver(Receive<MessageExecution>(this.Filter, HandleExecute));                
        }

        /// <summary>
        /// Filter that checks wether the Plugin fits to the internal Plugin reference of this PluginProtocl
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool Filter(MessageExecution msg)
        {
            if (msg.PluginModel != this.PluginModel)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Handle an execution of a plugin
        /// </summary>
        /// <param name="msg"></param>
        private void HandleExecute(MessageExecution msg)
        {
            // ################
            // 1. Check if Plugin may Execute
            // ################

            if (!msg.PluginModel.WorkspaceModel.WorkspaceManagerEditor.isExecuting())
            {
                return;
            }

            //Check if all necessary inputs are set
            List<ConnectorModel> inputConnectors = msg.PluginModel.InputConnectors;
            foreach (ConnectorModel connectorModel in inputConnectors)
            {
                if (!connectorModel.IControl &&
                    (connectorModel.IsMandatory || connectorModel.InputConnections.Count > 0) && !connectorModel.HasData)
                {
                    return;
                }
            }

            //Check if all outputs are free
            List<ConnectorModel> outputConnectors = msg.PluginModel.OutputConnectors;
            foreach (ConnectorModel connectorModel in outputConnectors)
            {
                if (!connectorModel.IControl)
                {
                    List<ConnectionModel> outputConnections = connectorModel.OutputConnections;
                    foreach (ConnectionModel connectionModel in outputConnections)
                    {
                        if (connectionModel.To.HasData)
                        {
                            return;
                        }
                    }
                }
            }

            // ################
            //2. Fill all Inputs of the plugin, if this fails, stop executing the plugin
            // ################

            //Fill the plugins inputs with data
            foreach (ConnectorModel connectorModel in inputConnectors)
            {
                try
                {
                    if (connectorModel.HasData && connectorModel.Data != null)
                    {
                        if (connectorModel.IsDynamic)
                        {
                        
                            if(connectorModel.method == null)
                            {
                                connectorModel.method = PluginModel.Plugin.GetType().GetMethod(connectorModel.DynamicSetterName);
                            }
                            connectorModel.method.Invoke(PluginModel.Plugin, new object[] { connectorModel.PropertyName, connectorModel.Data });
                        }
                        else
                        {
                            if (connectorModel.property == null)
                            {
                                connectorModel.property = PluginModel.Plugin.GetType().GetProperty(connectorModel.PropertyName);
                            }
                            connectorModel.property.SetValue(PluginModel.Plugin, connectorModel.Data, null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.PluginModel.WorkspaceModel.WorkspaceManagerEditor.GuiLogMessage("An error occured while setting value of connector \"" + connectorModel.Name + "\" of \"" + PluginModel.Name + "\": " + ex.Message, NotificationLevel.Error);
                    this.PluginModel.State = PluginModelState.Error;
                    this.PluginModel.GuiNeedsUpdate = true;
                    return;
                }
            }

            // ################
            //3. Execute the Plugin -> call the IPlugin.Execute()
            // ################

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

            // ################
            //4. Count for the benchmark
            // ################

            if (this.executionEngine.BenchmarkPlugins)
            {
                this.executionEngine.ExecutedPluginsCounter++;                
            }

            // ################
            //5. "Consume" all inputs
            // ################

            foreach (ConnectorModel connectorModel in inputConnectors)
            {
                try
                {
                    connectorModel.HasData = false;
                    connectorModel.Data = null;
                }
                catch (Exception ex)
                {
                    this.PluginModel.WorkspaceModel.WorkspaceManagerEditor.GuiLogMessage("An error occured while 'consuming' value of connector \"" + connectorModel.Name + "\" of \"" + PluginModel.Name + "\": " + ex.Message, NotificationLevel.Error);
                    this.PluginModel.State = PluginModelState.Error;
                    this.PluginModel.GuiNeedsUpdate = true;
                    return;
                }
            }
            
            // ################
            //6. If the user wants to, sleep some time
            // ################

            if (this.executionEngine.SleepTime > 0)
            {
                Thread.Sleep(this.executionEngine.SleepTime);
            }


            // ################
            //6. check if an "input" plugin may execute
            // ################

            foreach (ConnectorModel connectorModel in inputConnectors)
            {
                List<ConnectionModel> inputConnections = connectorModel.InputConnections;
                foreach (ConnectionModel connectionModel in inputConnections)
                {
                    connectionModel.Active = false;
                    connectionModel.GuiNeedsUpdate = true;
                }
                foreach (ConnectionModel connectionModel in inputConnections)
                {
                    if (!connectionModel.From.PluginModel.Startable ||
                        (connectionModel.From.PluginModel.Startable && connectionModel.From.PluginModel.RepeatStart))
                    {
                        connectionModel.From.PluginModel.PluginProtocol.BroadcastMessage(connectionModel.From.PluginModel.MessageExecution);
                    }
                }
            }
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
        private Thread[] threads;
        private volatile int runningThreads = 0;

        public ExecutionEngine executionEngine = null;		

        public WorkspaceManagerScheduler(string name, int amountThreads, ExecutionEngine executionEngine)
            : base()
        {
            threads = new Thread[amountThreads];
            this.executionEngine = executionEngine;

            for (int i = 0; i < threads.Length;i++ )
            {
                threads[i] = new Thread(this.DoScheduling);
                threads[i].SetApartmentState(ApartmentState.MTA);
                threads[i].Name = name + "-Thread-" + i;

                switch (this.executionEngine.ThreadPriority)
                {
                    case 0:
                        threads[i].Priority = ThreadPriority.AboveNormal;
                        break;
                    case 1:
                        threads[i].Priority = ThreadPriority.BelowNormal;
                        break;
                    case 2:
                        threads[i].Priority = ThreadPriority.Highest;
                        break;
                    case 3:
                        threads[i].Priority = ThreadPriority.Lowest;
                        break;
                    case 4:
                        threads[i].Priority = ThreadPriority.Normal;
                        break;
                }
            }
            
        }

        public void startScheduling()
        {
            foreach (Thread thread in threads)
            {
                thread.Start();
                lock (this)
                {
                    runningThreads++;
                }
            }
        }
       
        private void DoScheduling()
        {           

            this.executionEngine.GuiLogMessage(Thread.CurrentThread.Name + " up and running", NotificationLevel.Debug);
            Queue<ProtocolBase> waitingProtocols = this.waitingProtocols;
            
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
                        this.executionEngine.GuiLogMessage(Thread.CurrentThread.Name + " terminated", NotificationLevel.Debug);
                        lock (this)
                        {
                            runningThreads--;
                        }                                            
                        return;
                    }

                    try
                    {
                        ProtocolBase protocol = null;
                        lock (this)
                        {
                            if (waitingProtocols.Count == 0)
                                break;
                            protocol = waitingProtocols.Dequeue();
                        }
			
                        ProtocolStatus status = protocol.Run();

                        lock (this)
                        {
                            switch (status)
                            {
                                case ProtocolStatus.Created:
                                    System.Diagnostics.Debug.Assert(false);
                                    break;
                                case ProtocolStatus.Ready:
                                    waitingProtocols.Enqueue(protocol);
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
                        this.executionEngine.GuiLogMessage("Error during scheduling: " + ex.Message + " - " + ex.InnerException,NotificationLevel.Error);
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

            this.executionEngine.GuiLogMessage("Waiting for all scheduler threads to stop", NotificationLevel.Debug);
            while (runningThreads > 0)
            {
                Thread.Sleep(50);
                this.wakeup.Set();
            }
            this.executionEngine.GuiLogMessage("All scheduler threads stopped", NotificationLevel.Debug);

            this.waitingProtocols.Clear();
            this.protocols.Clear();            
        }
    }
}
