using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using System.Windows.Controls;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugin.BinaryConstant
{
    [Author("Holger Pretzsch", "holger-pretzsch@stud.uni-due.de", "Universität Duisburg-Essen", "http://www.vs.uni-due.de")]
    [PluginInfo(true, "Binary Constant", "Provides a binary constant", null, "CrypWin/images/default.png")]
    public class BinaryConstant : IInput
    {
        #region Private Variables

        private BinaryConstantSettings settings = new BinaryConstantSettings();

        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.OutputData, "Output", "Constant as byte array", null, DisplayLevel.Beginner)]
        public byte[] Output { get { return settings.ConstantValue; } }


        [PropertyInfo(Direction.OutputData, "Output stream", "Constant as stream", null, DisplayLevel.Beginner)]
        public CryptoolStream OutputStream
        {
            get
            {
                if (Output == null)
                    return null;

                CryptoolStream stream = new CryptoolStream();
                listCryptoolStreamsOut.Add(stream);
                stream.OpenRead(Output);
                return stream;
            }
        }

        [PropertyInfo(Direction.OutputData, "Output length", "Length of the constant data in bytes", null, DisplayLevel.Beginner)]
        public int OutputLength
        {
            get
            {
                if (Output == null)
                    return 0;
                else
                    return Output.Length;
            }
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings { get { return settings; } }

        public UserControl Presentation { get { return null; } }

        public UserControl QuickWatchPresentation { get { return null; } }

        public void PreExecution() { }

        public void Execute()
        {
            ProgressChanged(1, 1);
            OnPropertyChanged("Output");
            OnPropertyChanged("OutputStream");
            OnPropertyChanged("OutputLength");
        }

        public void PostExecution() { }

        public void Pause() { }

        public void Stop() { }

        public void Initialize() { }

        public void Dispose()
        {
            foreach (CryptoolStream stream in listCryptoolStreamsOut)
            {
                stream.Close();
            }
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
