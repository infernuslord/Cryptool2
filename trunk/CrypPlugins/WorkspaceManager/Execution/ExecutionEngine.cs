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

namespace WorkspaceManager.Execution
{
    /// <summary>
    /// Engine to execute a model of the WorkspaceManager
    /// </summary>
    public class ExecutionEngine
    {
        private Hashtable RunningPlugins = Hashtable.Synchronized(new Hashtable());
        private WorkspaceManager WorkspaceManagerEditor;
        private Mutex mutex = new Mutex();

        /// <summary>
        /// Enable/Disable the Debug Mode
        /// </summary>
        public bool DebugMode { get; set; }

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
            RunningPlugins.Clear();
            GuiLogMessage("ExecutionEngine starts", NotificationLevel.Debug);
            ThreadPool.SetMaxThreads(1, 1);
            ThreadPool.QueueUserWorkItem(new WaitCallback(Schedule), workspaceModel);
        }

        /// <summary>
        /// Scheduler Thread
        /// </summary>
        /// <param name="stateInfo"></param>
        public void Schedule(object stateInfo)
        {
            WorkspaceModel workspaceModel = (WorkspaceModel)stateInfo;
            if (!IsRunning)
            {
                IsRunning = true;
                workspaceModel.resetStates();

                //Initially check if a plugin can be executed
                foreach (PluginModel pluginModel in workspaceModel.AllPluginModels)
                {
                    pluginModel.checkExecutable();
                }

                //Now start Plugins if they are executable
                while (IsRunning)
                {
                    foreach (PluginModel pluginModel in workspaceModel.AllPluginModels)
                    {
                        
                        //execute the plugin in a new Thread only if it is executable
                        if (IsRunning && pluginModel.ExecutionState == PluginModelState.Executable && !RunningPlugins.Contains(pluginModel))
                        {
                            AddRunningPlugin(pluginModel);
                            ThreadPool.QueueUserWorkItem(new WaitCallback(runPlugin), pluginModel);
                        }
                        Thread.Sleep(50);
                    }
                }
                GuiLogMessage("ExecutionEngine ends", NotificationLevel.Debug);
            }
        }

        /// <summary>
        /// Runs a Plugin
        /// </summary>
        /// <param name="stateInfo"></param>
        private void runPlugin(object stateInfo)
        {   
         
            PluginModel pluginModel = (PluginModel)stateInfo;            
            GuiLogMessage("Running Plugin " + pluginModel.Name + " now!", NotificationLevel.Debug);
            
            //Fill the plugins Inputs with data
            foreach (ConnectorModel connectorModel in pluginModel.InputConnectors)
            {
                if (connectorModel.HasData)
                {                    
                    PropertyInfo propertyInfo = pluginModel.Plugin.GetType().GetProperty(connectorModel.PropertyName);
                    GuiLogMessage("Setting for " + connectorModel.PluginModel.Name + " the value of property " + propertyInfo.Name + " to \"" + connectorModel.Data +"\"", NotificationLevel.Debug);
                    propertyInfo.SetValue(pluginModel.Plugin, connectorModel.Data, null);    
                    connectorModel.Data = null;
                    connectorModel.HasData = false;
                    connectorModel.InputConnection.Active = false;
                }
            }

            //Run execution
            try
            {
                pluginModel.ExecutionState = PluginModelState.PreExecuting;
                pluginModel.Plugin.PreExecution();
                pluginModel.ExecutionState = PluginModelState.Executing;
                pluginModel.Plugin.Execute();
                pluginModel.ExecutionState = PluginModelState.PostExecuting;
                pluginModel.Plugin.PostExecution();
                pluginModel.ExecutionState = PluginModelState.Terminated;
            }
            catch (Exception ex)
            {
                pluginModel.ExecutionState = PluginModelState.Error;
                GuiLogMessage("Error during Execution of Plugin " + pluginModel.Name + ": " + ex.Message, NotificationLevel.Error);
            }
            //Remove plugin from being-executed-list so that it can be
            //executed again
            if (IsRunning)
            {
                //we only remove the plugin if we are in state running;otherwise
                //the Stop() method will remove it
                RemoveRunningPlugin(pluginModel);
            }
            GuiLogMessage("Stopped Plugin " + pluginModel.Name, NotificationLevel.Debug);
            Thread.Sleep(15);
        }

        /// <summary>
        /// Stop the execution
        /// </summary>
        public void Stop()
        {
            mutex.WaitOne();
            IsRunning = false;
            foreach (PluginModel pluginModel in RunningPlugins.Keys)
            {
                pluginModel.Plugin.Stop();              
            }
            RunningPlugins.Clear();
            mutex.ReleaseMutex();
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
        private void GuiLogMessage(string message, NotificationLevel level)
        {
            if (DebugMode || level.Equals(NotificationLevel.Error) || level.Equals(NotificationLevel.Warning))
            {
                WorkspaceManagerEditor.GuiLogMessage(message, level);
            }
        }

        /// <summary>
        /// Add a running plugin
        /// </summary>
        /// <param name="pluginModel"></param>
        private void AddRunningPlugin(PluginModel pluginModel)
        {
            mutex.WaitOne();
            if (IsRunning)
            {
                RunningPlugins.Add(pluginModel, pluginModel);
            }
            mutex.ReleaseMutex();
        }

        /// <summary>
        /// Remove a running plugin
        /// </summary>
        /// <param name="pluginModel"></param>
        private void RemoveRunningPlugin(PluginModel pluginModel)
        {
            mutex.WaitOne();
            if(RunningPlugins.Contains(pluginModel))
            {
                RunningPlugins.Remove(pluginModel);
            }
            mutex.ReleaseMutex();
        }

    }
}
