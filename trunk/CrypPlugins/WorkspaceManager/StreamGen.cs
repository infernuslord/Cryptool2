using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using WorkspaceManager.Execution;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using Gears4Net;
using WorkspaceManager.Model;
using System.Threading;

namespace WorkspaceManager
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(true, "StreamGen", "StreamGen", null, "AnotherEditor/icon.png")]
    class StreamGen : PluginProtocol, IPlugin, IInput
    {
        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private CStream outputStream;
        [PropertyInfo(Direction.OutputData, "OutputStream", "The Outputstream", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public CStream OutputStream
        {
            get { return outputStream; }
            set
            {
                if (value != outputStream)
                {
                    outputStream = value;                    
                }
            }
        }

        public StreamGen()
            : base(new WorkspaceManagerScheduler(), null, null)
        {
        }

        /// <summary>
        /// The main function of the protocol     
        /// </summary>
        /// <param name="stateMachine"></param>
        /// <returns></returns>
        public override System.Collections.Generic.IEnumerator<ReceiverBase> Execute(AbstractStateMachine stateMachine)
        {
            yield return Receive<MessageExecution>();
            while (!this.mayExecute() && this.PluginModel.WorkspaceModel.WorkspaceManagerEditor.isExecuting())
            {
                yield return Receive<MessageExecution>();
            }

            this.fillInputs();
            int ops = 100;

            while (this.PluginModel.WorkspaceModel.WorkspaceManagerEditor.isExecuting() && ops >0)
            {
                CStreamWriter writer = new CStreamWriter();
                OutputStream = writer.CStream;
                String time = DateTime.Now.ToLongTimeString();
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                writer.Write(enc.GetBytes(time));
                writer.Close();
                GuiLogMessage("Write " + time, NotificationLevel.Debug);
                OnPropertyChanged("OutputStream");                
                this.runNextPlugins();
                
                ops--;
                this.ExecutionEngine.ExecutedPluginsCounter++;
                yield return Receive<MessageExecution>();                
            }
            
        }

        public ISettings Settings
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            this.PluginModel.RepeatStart = true;
        }

        public void Execute()
        {            
        }

        public void PostExecution()
        {
            
        }

        public void Pause()
        {
            
        }

        public void Stop()
        {
            
        }

        public void Initialize()
        {
            
        }

        public void Dispose()
        {
            
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }
    }
}
