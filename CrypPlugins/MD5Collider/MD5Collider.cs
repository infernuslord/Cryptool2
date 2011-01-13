using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using Cryptool.Plugins.MD5Collider.Algorithm;
using Cryptool.Plugins.MD5Collider.Presentation;
using System.Windows.Controls;

namespace Cryptool.Plugins.MD5Collider
{
    [Author("Holger Pretzsch", "mail@holger-pretzsch.de", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "MD5Collider", "MD5 hash collider", "MD5Collider/DetailedDescription/Description.xaml", "MD5Collider/MD5Collider.png")]
    class MD5Collider : ICryptographicHash
    {
        private MD5ColliderSettings settings = new MD5ColliderSettings();
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();
        private QuickWatchPresentationContainer quickWatchPresentation = new QuickWatchPresentationContainer();

        private IMD5ColliderAlgorithm _collider;
        private IMD5ColliderAlgorithm Collider
        {
            get { return _collider; }
            set { _collider = value; quickWatchPresentation.Collider = value; }
        }

        public event Cryptool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;
        public event Cryptool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event Cryptool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public Cryptool.PluginBase.ISettings Settings { get { return settings; } }
        public System.Windows.Controls.UserControl Presentation { get { return null; } }
        public System.Windows.Controls.UserControl QuickWatchPresentation { get { return quickWatchPresentation; } }

        public void PreExecution() { Dispose(); }
        public void PostExecution() { Dispose(); }
        public void Pause() { }
        public void Stop() { Collider.Stop(); }
        public void Initialize() { }

        public MD5Collider()
        {
            Collider = new MultiThreadedMD5Collider<StevensCollider>();
            //Collider.Status = "Waiting";
        }

        private byte[] outputData1;
        [PropertyInfo(Direction.OutputData, "First colliding data block", "First colliding data block as byte array", "", false, false, QuickWatchFormat.Hex, null)]
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

        [PropertyInfo(Direction.OutputData, "First colliding data block", "First colliding data block as Stream", "", false, false, QuickWatchFormat.Hex, null)]
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
        [PropertyInfo(Direction.OutputData, "Second colliding data block", "Second colliding data block as byte array", "", false, false, QuickWatchFormat.Hex, null)]
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

        [PropertyInfo(Direction.OutputData, "Second colliding data block", "Second colliding data block as Stream", "", false, false, QuickWatchFormat.Hex, null)]
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

        private byte[] randomSeed;
        [PropertyInfo(Direction.InputData, "Random seed", "Data used for initialization of RNG", "", false, false, QuickWatchFormat.Hex, null)]
        public byte[] RandomSeed
        {
            get { return randomSeed; }
            set { this.randomSeed = value; OnPropertyChanged("RandomSeed"); }
        }

        private byte[] prefix;
        [PropertyInfo(Direction.InputData, "Prefix", "Common prefix for colliding blocks", "", false, false, QuickWatchFormat.Hex, null)]
        public byte[] Prefix
        {
            get { return prefix; }
            set { this.prefix = value; OnPropertyChanged("Prefix"); }
        }

        public void Execute()
        {
            ProgressChanged(0.5, 1.0);

            Collider.RandomSeed = RandomSeed;

            if (Prefix != null)
            {
                if (Prefix.Length % 64 != 0)
                {
                    GuiLogMessage("Prefixed data must be a multiple of 64 bytes long!", NotificationLevel.Error);
                    return;
                }

                Collider.IHV = new IHVCalculator(Prefix).GetIHV();
            }

            Collider.FindCollision();

            OutputData1 = Collider.FirstCollidingData;
            OutputData2 = Collider.SecondCollidingData;

            ProgressChanged(1.0, 1.0);
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
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
