/*                              
   Copyright 2010 Nils Kopal

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
using WorkspaceManager.Model;
using System.Threading;
using Cryptool.PluginBase;
using System.Windows.Threading;
using Cryptool.PluginBase.Editor;


namespace WorkspaceManager.Execution
{
    /// <summary>
    /// Engine to execute a model of the WorkspaceManager
    /// This class needs a WorkspaceManager to be instantiated
    /// To run an execution process it also needs a WorkspaceModel
    /// </summary>
    public class ExecutionEngine
    {
        private IEditor Editor;
        private WorkspaceModel workspaceModel;
        private Thread guiUpdateThread = null;

        public volatile int ExecutedPluginsCounter = 0;
        public bool BenchmarkPlugins = false;
        public int GuiUpdateInterval = 0;
        public int SleepTime = 0;
        public int ThreadPriority = 0;
        public int MaxStopWaitingTime = 1000;

        public List<Thread> threads;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        /// <summary>
        /// Creates a new ExecutionEngine
        /// </summary>
        /// <param name="editor"></param>
        public ExecutionEngine(IEditor editor)
        {
            Editor = editor;
            threads = new List<Thread>();
        }

        /// <summary>
        /// Is this ExecutionEngine running?
        /// </summary>
        public bool IsRunning()
        {
            foreach (var pluginModel in workspaceModel.AllPluginModels)
            {
                if(pluginModel.Stop == false)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Execute the given Model
        /// </summary>
        /// <param name="workspaceModel"></param>
        public void Execute(WorkspaceModel workspaceModel, bool updateGuiElements = true)
        {
            try
            {
                Stopped = false;
                workspaceModel.ExecutionEngine = this;
                workspaceModel.IsBeingExecuted = true;
                ExecutionCounter = 0;
                this.workspaceModel = workspaceModel;
                workspaceModel.resetStates();

                if (updateGuiElements)
                {
                    guiUpdateThread = new Thread(CheckGui);
                    threads.Add(guiUpdateThread);
                    guiUpdateThread.Name = "WorkspaceManager_GUIUpdateThread";
                    guiUpdateThread.IsBackground = true;
                    guiUpdateThread.Start();
                }

                benchmarkTimer = new System.Timers.Timer(1000);
                benchmarkTimer.Elapsed += BenchmarkTimeout;
                benchmarkTimer.AutoReset = true;
                benchmarkTimer.Enabled = true;

                int i = 0;
                foreach (var pluginModel in workspaceModel.AllPluginModels)
                {
                    var thread = new Thread(new ParameterizedThreadStart(pluginModel.Execute))
                                     {Name = "WorkspaceManager_Thread-" + i};
                    threads.Add(thread);
                    thread.IsBackground = true;
                    i++;
                    thread.Start(this);
                }

                foreach (var pluginModel in workspaceModel.AllPluginModels)
                {
                    if (pluginModel.InputConnectors.Count == 0)
                    {
                        pluginModel.resetEvent.Set();
                    }
                }
            }
            catch(Exception ex)
            {
                GuiLogMessage("Exception occured during startup of Workspace: " + ex.Message, NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Called by the BenchmarkTimer to display plugins per seconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BenchmarkTimeout(object sender, EventArgs e)
        {
            if(BenchmarkPlugins)
            {
                GuiLogMessage(string.Format("Executing at {0:0,0} Plugins\\sec.",ExecutionCounter),NotificationLevel.Debug);
            }
            ExecutionCounter = 0;
            benchmarkTimer.Start();
        }

        private System.Timers.Timer benchmarkTimer = null;
        private bool Stopped { get; set; }

        internal volatile int ExecutionCounter = 0;

        /// <summary>
        /// Called by the GUI-Updater Thread
        /// </summary>
        private void CheckGui()
        {
            try
            {
                while (true)
                {
                    if(Stopped)
                    {
                        return;
                    }

                    if (Editor.Presentation.IsVisible)
                    {

                        Editor.Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            foreach (var pluginModel in workspaceModel.AllPluginModels)
                            {
                                if (pluginModel.GuiNeedsUpdate)
                                {
                                    if (pluginModel.UpdateableView != null)
                                        pluginModel.UpdateableView.update();
                                    pluginModel.GuiNeedsUpdate = false;
                                }
                            }

                            //in the current version line updates are not possible
                            //so i out commented the code
                            //nils kopal
                            /*foreach (var connectionModel in workspaceModel.AllConnectionModels)
                            {
                                if (connectionModel.GuiNeedsUpdate)
                                {
                                    if (connectionModel.UpdateableView != null)
                                        connectionModel.UpdateableView.update();
                                    connectionModel.GuiNeedsUpdate = false;
                                }
                            }

                            foreach (var connectorModel in workspaceModel.AllConnectorModels)
                            {
                                if (connectorModel.GuiNeedsUpdate)
                                {
                                    if (connectorModel.UpdateableView != null)
                                        connectorModel.UpdateableView.update();
                                    connectorModel.GuiNeedsUpdate = false;
                                }
                            }*/
                        }
                        , null);
                    }
                    Thread.Sleep(GuiUpdateInterval);
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage("Exception occured during update of GUI of Workspace: " + ex.Message, NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Stop the execution process:
        /// </summary>
        public void Stop()
        {
            try
            {
                GuiLogMessage("Start stopping ExecutionEngine", NotificationLevel.Info);
                Stopped = true;
                foreach (var pluginModel in workspaceModel.AllPluginModels)
                {
                    pluginModel.Stop = true;
                    pluginModel.Plugin.Stop();
                    pluginModel.resetEvent.Set();
                }
                benchmarkTimer.Enabled = false;
                workspaceModel.IsBeingExecuted = false;

                GuiLogMessage("Waiting for all threads to stop", NotificationLevel.Debug);
                foreach(var t in threads)
                {
                    try
                    {
                        t.Join(MaxStopWaitingTime);
                    }
                    catch(Exception ex)
                    {
                        GuiLogMessage(string.Format("Exception during waiting for thread '{0}' to stop: {1}",t.Name,ex.Message),NotificationLevel.Error);
                        GuiLogMessage(string.Format("Aborting '{0}' now", t.Name), NotificationLevel.Debug);
                        t.Abort();
                    }
                }
                                
                GuiLogMessage("All threads stopped", NotificationLevel.Debug);

                workspaceModel.resetStates();

                GuiLogMessage("WorkspaceModel states resetted",NotificationLevel.Debug);

                GuiLogMessage("ExecutionEngine successfully stopped", NotificationLevel.Info);
            }
            catch(Exception ex)
            {
                GuiLogMessage("Exception occured during stopping of Workspace: " + ex.Message, NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Loggs a gui message
        /// Sender will be the editor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        internal void GuiLogMessage(string message, NotificationLevel level)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                GuiLogEventArgs args = new GuiLogEventArgs(message, Editor, level);
                args.Title = "-";
                OnGuiLogNotificationOccured(Editor, args);
            }
        }
    }    
}
