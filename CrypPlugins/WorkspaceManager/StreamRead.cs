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
    [PluginInfo(false, "StreamRead", "StreamRead", null, "AnotherEditor/icon.png")]
    class StreamRead : PluginProtocol, IPlugin, IOutput
    {
        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private CStream inputStream;
        [PropertyInfo(Direction.InputData, "InputStream", "The inputStream", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public CStream InputStream
        {
            get { return inputStream; }
            set
            {
                if (value != inputStream)
                {
                    inputStream = value;                    
                }
            }
        }

        public StreamRead()
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

            while (this.PluginModel.WorkspaceModel.WorkspaceManagerEditor.isExecuting())
            {
                CStreamReader reader = InputStream.CreateReader();
                int i = 0;
                int c = 0;
                byte[] buffer = new byte[256];
                while (i!=-1)
                {
                    i = reader.ReadByte();
                    if (i != -1)
                    {
                        buffer[c] = (byte)i;
                    }
                    c++;
                }                
                GuiLogMessage("Read " + System.Text.ASCIIEncoding.ASCII.GetString(buffer,0,c-1), NotificationLevel.Debug);
                this.clearInputs();
                this.runNextPlugins();
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
