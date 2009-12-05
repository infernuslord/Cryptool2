using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using Cryptool.MD5Collider.Algorithm;

namespace Cryptool.MD5Collider
{
    [Author("Holger Pretzsch", "mail@holger-pretzsch.de", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(true, "MD5Collider", "MD5 hash collider", "MD5Collider/DetailedDescription/Description.xaml", "MD5Collider/MD5Collider.png")]
    [EncryptionType(EncryptionType.Classic)]
    class MD5Collider : ICryptographicHash
    {
        private MD5ColliderSettings settings = new MD5ColliderSettings();
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();

        public event Cryptool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;
        public event Cryptool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event Cryptool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public Cryptool.PluginBase.ISettings Settings { get { return settings; } }
        public System.Windows.Controls.UserControl Presentation { get { return null; } }
        public System.Windows.Controls.UserControl QuickWatchPresentation { get { return null; } }

        public void PreExecution() { Dispose(); }
        public void PostExecution() { Dispose(); }
        public void Pause() { }
        public void Stop() { }
        public void Initialize() { }

        private byte[] outputData1;
        [PropertyInfo(Direction.OutputData, "First colliding data block", "First colliding data block as byte array", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public byte[] OutputData1
        {
            get { return this.outputData1; }
            set
            {
                outputData1 = value;
                OnPropertyChanged("OutputData1");
                OnPropertyChanged("OutputDataStream1");
            }
        }

        [PropertyInfo(Direction.OutputData, "First colliding data block", "First colliding data block as Stream", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public CryptoolStream OutputDataStream1
        {
            get
            {
                if (outputData1 != null)
                {
                    CryptoolStream stream = new CryptoolStream();
                    listCryptoolStreamsOut.Add(stream);
                    stream.OpenRead(outputData1);
                    return stream;
                }
                else
                    return null; ;
            }
        }

        private byte[] outputData2;
        [PropertyInfo(Direction.OutputData, "Second colliding data block", "Second colliding data block as byte array", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public byte[] OutputData2
        {
            get { return this.outputData2; }
            set
            {
                outputData2 = value;
                OnPropertyChanged("OutputData2");
                OnPropertyChanged("OutputDataStream2");
            }
        }

        [PropertyInfo(Direction.OutputData, "Second colliding data block", "Second colliding data block as Stream", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public CryptoolStream OutputDataStream2
        {
            get
            {
                if (outputData2 != null)
                {
                    CryptoolStream stream = new CryptoolStream();
                    listCryptoolStreamsOut.Add(stream);
                    stream.OpenRead(outputData2);
                    return stream;
                }
                else
                    return null; ;
            }
        }

        public void Execute()
        {
            ProgressChanged(0.5, 1.0);
            MD5TunnelCollider collider = new MD5TunnelCollider();
            collider.FindCollision();
            OutputData1 = collider.FirstCollidingData;
            OutputData2 = collider.SecondCollidingData;
            ProgressChanged(1.0, 1.0);
        }

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public void Dispose()
        {
            foreach (CryptoolStream stream in listCryptoolStreamsOut)
                stream.Close();
            listCryptoolStreamsOut.Clear();
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
    }
}
