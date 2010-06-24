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

        public long ExecutedPluginsCounter { get; set; }
        public bool BenchmarkPlugins { get; set; }
        public long GuiUpdateInterval { get; set; }

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
            get;
            private set;
        }

        /// <summary>
        /// Execute the given Model
        /// </summary>
        /// <param name="workspaceModel"></param>
        public void Execute(WorkspaceModel workspaceModel)
        {
            this.workspaceModel = workspaceModel;

            if (!IsRunning)
            {
                IsRunning = true;

                //Here we create n = "ProcessorsCount * 2" Gears4Net schedulers
                //We do this, because measurements showed that we get the best performance if we
                //use this amount of schedulers
                schedulers = new Scheduler[System.Environment.ProcessorCount*2];
                for(int i=0;i< System.Environment.ProcessorCount*2;i++){
                    schedulers[i] = new WorkspaceManagerScheduler("Scheduler" + i);                    
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
                int counter=0;
                foreach (PluginModel pluginModel in workspaceModel.AllPluginModels)
                {
                    pluginModel.Plugin.PreExecution();
                    PluginProtocol pluginProtocol = new PluginProtocol(schedulers[counter], pluginModel,this);
                    pluginModel.PluginProtocol = pluginProtocol;
                    schedulers[counter].AddProtocol(pluginProtocol);
                   
                    pluginProtocol.Start();
                    counter = (counter + 1) % (System.Environment.ProcessorCount*2);

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
                        //executionEngine.GuiLogMessage("UpdateGui for \"" + pluginModel.Name + "\"", NotificationLevel.Debug);
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
        /// The main function of the protocol     
        /// </summary>
        /// <param name="stateMachine"></param>
        /// <returns></returns>
        public override System.Collections.Generic.IEnumerator<ReceiverBase> Execute(AbstractStateMachine stateMachine)
        {
            while (this.executionEngine.IsRunning)
            {
                yield return Receive<MessageExecution>(null, this.HandleExecute);
                //yield return Parallel(1,new PluginWaitReceiver()) & Receive<MessageExecution>(null, this.HandleExecute);
                //yield return new PluginWaitReceiver() + Receive<MessageExecution>(null, this.HandleExecute);
                //yield return Parallel(1,new PluginWaitReceiver()) + Receive<MessageExecution>(null, this.HandleExecute);
            }
        }

        /// <summary>
        /// Call the execution function of the wrapped IPlugin
        /// </summary>
        /// <param name="msg"></param>
        private void HandleExecute(MessageExecution msg)
        {
            //executionEngine.GuiLogMessage("HandleExecute for \"" + msg.PluginModel.Name + "\"", NotificationLevel.Debug);
            //Fill the plugins Inputs with data
            foreach (ConnectorModel connectorModel in PluginModel.InputConnectors)
            {
                if (connectorModel.HasData)
                {                   
                    try
                    {
                        if (connectorModel.IsDynamic)
                        {
                            MethodInfo propertyInfo = PluginModel.Plugin.GetType().GetMethod(connectorModel.DynamicSetterName);
                            propertyInfo.Invoke(PluginModel.Plugin, new object[] { connectorModel.PropertyName, connectorModel.Data });
                        }
                        else
                        {
                            PropertyInfo propertyInfo = PluginModel.Plugin.GetType().GetProperty(connectorModel.PropertyName);
                            propertyInfo.SetValue(PluginModel.Plugin, connectorModel.Data, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.PluginModel.WorkspaceModel.WorkspaceManagerEditor.GuiLogMessage("An error occured while setting value of connector \"" + connectorModel.Name + "\" of \"" + PluginModel + "\": " + ex.Message, NotificationLevel.Error);
                        this.PluginModel.State = PluginModelState.Error;
                        this.PluginModel.GuiNeedsUpdate = true;
                        return;
                    }
                }
            }
            
            msg.PluginModel.Plugin.Execute();            

            if (this.executionEngine.BenchmarkPlugins)
            {
                this.executionEngine.ExecutedPluginsCounter++;                                
            }
        }
      
    }

    public class WorkspaceManagerScheduler : Scheduler
    {
        private System.Threading.AutoResetEvent wakeup = new System.Threading.AutoResetEvent(false);
        private bool shutdown = false;
        private System.Threading.Thread thread;
        private Context currentContext;

		public WorkspaceManagerScheduler() : this(String.Empty)
		{

		}

        public WorkspaceManagerScheduler(string name)
            : base()
        {
            this.currentContext = Thread.CurrentContext;

            thread = new System.Threading.Thread(this.Start);
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
			thread.Name = name;
            
        }

        public void startScheduler()
        {
            thread.Start();
        }

        private void Start()
        {
            if (this.currentContext != Thread.CurrentContext)
                this.currentContext.DoCallBack(Start);

            // Loop forever
            while (true)
            {
                this.wakeup.WaitOne();

                // Loop while there are more protocols waiting
                while (true)
                {
                    // Should the scheduler stop?
                    if (this.shutdown)
                        return;
                    
                    bool donotrun = false;
                    ProtocolBase protocol = null;
                    lock (this)
                    {
                        // No more protocols? -> Wait
                        if (this.waitingProtocols.Count == 0)
                            break;
                        protocol = this.waitingProtocols.Dequeue();

                        if (protocol is PluginProtocol)
                        {
                            PluginProtocol pluginProtocol = (PluginProtocol)protocol;
                            foreach (ConnectorModel outputConnector in pluginProtocol.PluginModel.OutputConnectors)
                            {
                                foreach (ConnectionModel connection in outputConnector.OutputConnections)
                                {
                                    
                                    if (connection.To.PluginModel.PluginProtocol.QueueLength > 0 &&
                                        connection.To.PluginModel != pluginProtocol.PluginModel && 
                                        donotrun == false)
                                    {                                            
                                        this.waitingProtocols.Enqueue(protocol);
                                        donotrun = true;
                                    }
                                 
                                }
                            }               
                        }

                    }

                    if (donotrun == false)
                    {
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
                }
            }
        }

        public override void RemoveProtocol(ProtocolBase protocol)
        {
            lock (this)
            {
                this.protocols.Remove(protocol);
                if (this.protocols.Count == 0)
                    this.Shutdown();
            }
        }

        public override void AddProtocol(ProtocolBase protocol)
        {
            lock (this)
            {
                this.protocols.Add(protocol);
            }
        }

        public override void Wakeup(ProtocolBase protocol)
        {
            lock (this)
            {
                if (!this.waitingProtocols.Contains(protocol))
                    this.waitingProtocols.Enqueue(protocol);
                this.wakeup.Set();
            }
        }

        public override void Shutdown()
        {
            this.shutdown = true;
            this.wakeup.Set();
        }
    }
}
