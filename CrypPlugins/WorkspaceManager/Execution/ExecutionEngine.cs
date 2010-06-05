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

namespace WorkspaceManager.Execution
{
    /// <summary>
    /// Engine to execute a model of the WorkspaceManager
    /// </summary>
    public class ExecutionEngine
    {
        private WorkspaceManager WorkspaceManagerEditor;
        private Scheduler scheduler;
      
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
        /// Execute the current Model
        /// </summary>
        /// <param name="workspaceModel"></param>
        public void Execute(WorkspaceModel workspaceModel)
        {
            if (!IsRunning)
            {
                IsRunning = true;
                scheduler = new WinFormsScheduler("Scheduler");
                workspaceModel.resetStates();
               
                foreach (PluginModel pluginModel in workspaceModel.AllPluginModels)
                {
                    PluginProtocol pluginProtocol = new PluginProtocol(scheduler, pluginModel,this);
                    pluginModel.PluginProtocol = pluginProtocol;
                    scheduler.AddProtocol(pluginProtocol);
                    pluginModel.checkExecutable(pluginProtocol);
                    pluginProtocol.Start();
                }
            }
        }      
      
        /// <summary>
        /// Stop the execution
        /// </summary>
        public void Stop()
        {
            scheduler.Shutdown();
            IsRunning = false;   
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
    /// Message send to scheduler for a Plugin to trigger the PreExecution
    /// </summary>
    public class MessagePreExecution : MessageBase
    {
        public PluginModel PluginModel;
    }

    /// <summary>
    /// Message send to scheduler for a Plugin to trigger the Execution
    /// </summary>
    public class MessageExecution : MessageBase
    {
        public PluginModel PluginModel;
    }

    /// <summary>
    /// Message send to scheduler for a Plugin to trigger the PostExecution
    /// </summary>
    public class MessagePostExecution : MessageBase
    {
        public PluginModel PluginModel;
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
        /// </summary>
        /// <param name="stateMachine"></param>
        /// <returns></returns>
        public override System.Collections.Generic.IEnumerator<ReceiverBase> Execute(AbstractStateMachine stateMachine)
        {
            yield return Receive<MessagePreExecution>(null, this.HandlePreExecute);
            MessageExecution msg_exec = new MessageExecution();
            msg_exec.PluginModel = this.pluginModel;
            this.BroadcastMessageReliably(msg_exec);
            yield return Receive<MessageExecution>(null, this.HandleExecute);
            MessagePostExecution msg_post = new MessagePostExecution();
            msg_post.PluginModel = this.pluginModel;
            this.BroadcastMessageReliably(msg_post);
            yield return Receive<MessagePostExecution>(null, this.HandlePostExecute);
        }

        /// <summary>
        /// Handler function for a message.
        /// This handler must not block, because it executes inside the thread of the scheduler.
        /// </summary>
        /// <param name="msg"></param>
        private void HandlePreExecute(MessagePreExecution msg)
        {
            executionEngine.GuiLogMessage("HandlePreExecute for \"" + msg.PluginModel.Name + "\"", NotificationLevel.Debug);
            msg.PluginModel.Plugin.PreExecution();
        }

        /// <summary>
        /// Handler function for a message.
        /// This handler must not block, because it executes inside the thread of the scheduler.
        /// </summary>
        /// <param name="msg"></param>
        private void HandleExecute(MessageExecution msg)
        {
            executionEngine.GuiLogMessage("HandleExecute for \"" + msg.PluginModel.Name + "\"", NotificationLevel.Debug);
            //Fill the plugins Inputs with data
            foreach (ConnectorModel connectorModel in pluginModel.InputConnectors)
            {
                if (connectorModel.HasData)
                {
                    PropertyInfo propertyInfo = pluginModel.Plugin.GetType().GetProperty(connectorModel.PropertyName);
                    propertyInfo.SetValue(pluginModel.Plugin, connectorModel.Data, null);
                    connectorModel.Data = null;
                    connectorModel.HasData = false;
                    connectorModel.InputConnection.Active = false;
                }
            }
            msg.PluginModel.Plugin.Execute();
        }

        /// <summary>
        /// Handler function for a message.
        /// This handler must not block, because it executes inside the thread of the scheduler.
        /// </summary>
        /// <param name="msg"></param>
        private void HandlePostExecute(MessagePostExecution msg)
        {
            executionEngine.GuiLogMessage("HandlePostExecute for \"" + msg.PluginModel.Name + "\"", NotificationLevel.Debug);
            msg.PluginModel.Plugin.PostExecution();
        }
    }
}
