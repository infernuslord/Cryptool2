using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Collections;
using Cryptool.PluginBase.Miscellaneous;

namespace FriedmanTest
{
    [Author("Georgi Angelov, Danail Vazov", "vazov@cryptool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("FriedmanTest.Properties.Resources", "PluginCaption", "PluginTooltip", "FriedmanTest/DetailedDescription/doc.xml", "FriedmanTest/friedman.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class FriedmanTest : ICrypComponent
    {
        public FriedmanTest()
        {
            settings = new FriedmanTestSettings();
        }

        #region Private Variables

        private FriedmanTestSettings settings;
        private double keyLength = 0;
        private double kappaCiphertext = 0;
        private string stringOutput="";
        private int [] arrayInput;

        #endregion

        #region Private methods

        private void ShowStatusBarMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void ShowProgress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion

        #region Properties (Inputs/Outputs)

        [PropertyInfo(Direction.OutputData, "StringOutputCaption", "StringOutputTooltip", false)]
        public string StringOutput
        {
            get { return this.stringOutput; }
            set { }
        }

        [PropertyInfo(Direction.OutputData, "KeyLengthCaption", "KeyLengthTooltip", false)]
        public double KeyLength
        {
            get { return keyLength; }
            set { }
        }

        [PropertyInfo(Direction.OutputData, "KappaCiphertextCaption", "KappaCiphertextTooltip", false)]
        public double KappaCiphertext
        {
            get { return kappaCiphertext; }
            set { }
        }

        [PropertyInfo(Direction.InputData, "ArrayInputCaption", "ArrayInputTooltip", true)]
        public int [] ArrayInput
        {
            get { return arrayInput; }
            set
            {
                arrayInput = value;
                OnPropertyChanged("ArrayInput");
            }
        }
        #endregion

        #region IPlugin Members

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        
        public ISettings Settings
        {
            get { return settings; }
            set { settings = (FriedmanTestSettings)value; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            //nothing to do
        }

        public void Execute()
        {
            if (arrayInput != null)
            {
                double Kp; //Kappa "language"
				long cipherTextLength = 0; //n
				long countDoubleCharacters = 0;
                string ciphermode = "monoalphabetic/cleartext";

				//Now we set the Kappa plain-text coefficient. Default is English.
                switch (settings.Kappa)
                {
                    case 1: Kp = 0.0762; break;
                    case 2: Kp = 0.0778; break;
                    case 3: Kp = 0.0770; break;
                    case 4: Kp = 0.0738; break;
                    case 5: Kp = 0.0745; break;
                    default: Kp = 0.0667; break;
                }

                ShowStatusBarMessage("Using IC = " + Kp.ToString() + " for analysis...", NotificationLevel.Debug);


                if (arrayInput.Length < 2)
                {
                    // error, only one letter?
                    ShowStatusBarMessage("Error - cannot analyze an array of a single letter.", NotificationLevel.Error);
                    return;
                }

				for (int i = 0; i < arrayInput.Length; i++)
				{
					cipherTextLength += arrayInput[i];
					countDoubleCharacters += (arrayInput[i] * (arrayInput[i] - 1));

                    // show some progress
                    ShowProgress(i, (arrayInput.Length + 1));
				}

                ShowStatusBarMessage(String.Format("Input analyzed: Got {0} different letters in a text of total length {1}.",arrayInput.Length,cipherTextLength), NotificationLevel.Debug);

				kappaCiphertext = ((double)countDoubleCharacters / (double)(cipherTextLength * (cipherTextLength - 1)));
                keyLength = 0.0377 * cipherTextLength / (((cipherTextLength - 1) * kappaCiphertext) - (0.0385 * cipherTextLength) + Kp);

                if (Math.Abs(Kp - kappaCiphertext) > 0.01)
                {
                    ciphermode = "polyalphabetic";
                }

                stringOutput = String.Format("KeyLen = {0}\nIC_analyzed = {1}\nIC_provided = {2}\nMode = {3}", keyLength.ToString("0.00000"), kappaCiphertext.ToString("0.00000"), Kp,ciphermode);

            
                OnPropertyChanged("StringOutput");
                OnPropertyChanged("KeyLength");
                OnPropertyChanged("KappaCiphertext");
                // final step of progress
                ShowProgress(100, 100);
            }

        }

        public void PostExecution()
        {
            //throw new NotImplementedException();
        }

        public void Stop()
        {
            //throw new NotImplementedException();
        }

        public void Initialize()
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
