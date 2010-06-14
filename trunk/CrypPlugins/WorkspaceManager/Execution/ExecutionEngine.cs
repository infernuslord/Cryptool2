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
        public long CheckInterval { get; set; }
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
                for(int i=0;i<System.Environment.ProcessorCount*2;i++){
                    schedulers[i] = new STAScheduler("Scheduler" + i);
                }

                //We have to reset all states of PluginModels, ConnectorModels and ConnectionModels:
                workspaceModel.resetStates();

                //The UpdateGuiProtocol is a kind of "daemon" which will update the view elements if necessary
                UpdateGuiProtocol updateGuiProtocol = new UpdateGuiProtocol(schedulers[0], workspaceModel, this);
                schedulers[0].AddProtocol(updateGuiProtocol);
                updateGuiProtocol.Start();

                //The CheckExecutableProtocl is also a kind of "daemon" which will check from time to time if a
                //plugin can be executed again
                CheckExecutableProtocol checkExecutableProtocol = new CheckExecutableProtocol(schedulers[0], workspaceModel, this);
                schedulers[0].AddProtocol(checkExecutableProtocol);
                checkExecutableProtocol.Start();

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
                int counter=0;
                foreach (PluginModel pluginModel in workspaceModel.AllPluginModels)
                {
                    PluginProtocol pluginProtocol = new PluginProtocol(schedulers[counter], pluginModel,this);
                    pluginModel.PluginProtocol = pluginProtocol;
                    schedulers[counter].AddProtocol(pluginProtocol);
                    pluginModel.checkExecutable(pluginProtocol);
                    pluginProtocol.Start();
                    counter = (counter + 1) % (System.Environment.ProcessorCount*2);
                }
            }
        }      
      
        /// <summary>
        /// Stop the execution process:
        /// calls shutdown on all schedulers + calls stop() on each plugin
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
            //First stop all Gears4Net Schedulers
            foreach (Scheduler scheduler in schedulers)
            {
                scheduler.Shutdown();
            }
            //Secondly stop alle plugins
            foreach(PluginModel pluginModel in workspaceModel.AllPluginModels)
            {
                pluginModel.Plugin.Stop();
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
    /// A Protocol for checking if plugins are executable in time intervals
    /// </summary>
    public class CheckExecutableProtocol : ProtocolBase
    {
        private WorkspaceModel workspaceModel;
        private ExecutionEngine executionEngine;
     
        /// <summary>
        /// Create a new protocol. Each protocol requires a scheduler which provides
        /// a thread for execution.
        /// </summary>
        /// <param name="scheduler"></param>
        public CheckExecutableProtocol(Scheduler scheduler, WorkspaceModel workspaceModel, ExecutionEngine executionEngine)
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
                yield return Timeout(this.executionEngine.CheckInterval, HandleCheckExecutable);
            }
        }

        /// <summary>
        /// Handler function for a message.
        /// This handler must not block, because it executes inside the thread of the scheduler.
        /// </summary>
        /// <param name="msg"></param>
        private void HandleCheckExecutable()
        {
            foreach (PluginModel pluginModel in workspaceModel.AllPluginModels)
            {
                pluginModel.checkExecutable(pluginModel.PluginProtocol);
            }
        }
        
    }

    /// <summary>
    /// A Protocol for checking if plugins are executable in time intervals
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
        private PluginModel pluginModel;
        private ExecutionEngine executionEngine;

        /// <summary>
        /// Create a new protocol. Each protocol requires a scheduler which provides
        /// a thread for execution.
        /// </summary>
        /// <param name="scheduler"></param>
        public PluginProtocol(Scheduler scheduler, PluginModel pluginModel,ExecutionEngine executionEngine)
            : base(scheduler)
        {
            this.pluginModel = pluginModel;
            this.executionEngine = executionEngine;
        }

        /// <summary>
        /// The main function of the protocol
        /// 
        /// states are here:
        /// 
        ///     PreExecution -> Execution -> PostExecution
        ///        /\                           |
        ///         |---------------------------|
        ///         
        /// </summary>
        /// <param name="stateMachine"></param>
        /// <returns></returns>
        public override System.Collections.Generic.IEnumerator<ReceiverBase> Execute(AbstractStateMachine stateMachine)
        {
            while (this.executionEngine.IsRunning)
            {
                yield return Receive<MessageExecution>(null, this.HandleExecute);             
            }
        }

        /// <summary>
        /// Call the execution function of the wrapped IPlugin
        /// </summary>
        /// <param name="msg"></param>
        private void HandleExecute(MessageExecution msg)
        {
            msg.PluginModel.Plugin.PreExecution();

            //executionEngine.GuiLogMessage("HandleExecute for \"" + msg.PluginModel.Name + "\"", NotificationLevel.Debug);
            //Fill the plugins Inputs with data
            foreach (ConnectorModel connectorModel in pluginModel.InputConnectors)
            {
                if (connectorModel.HasData)
                {
                    PropertyInfo propertyInfo = pluginModel.Plugin.GetType().GetProperty(connectorModel.PropertyName);
                    propertyInfo.SetValue(pluginModel.Plugin, connectorModel.Data, null);
                    connectorModel.HasLastData = true;
                    connectorModel.LastData = connectorModel.Data;
                    connectorModel.Data = null;
                    connectorModel.HasData = false;
                    connectorModel.InputConnection.Active = false;                    
                }
            }
            
            msg.PluginModel.Plugin.Execute();

            msg.PluginModel.Plugin.PostExecution();

            if (this.executionEngine.BenchmarkPlugins)
            {
                this.executionEngine.ExecutedPluginsCounter++;                                
            }
        }
      
    }
}
