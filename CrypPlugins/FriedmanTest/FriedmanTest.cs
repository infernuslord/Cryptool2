using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Analysis;
using System.ComponentModel;
using System.Collections;

namespace FriedmanTest
{
    [Author("Georgi Angelov & Danail Vazov", "vazov@cryptool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo(false,
            "Friedman Test",
            "Calculates the probable key lenght of a polyalphabetic substitution cipher.",
            "FriedmanTest/DetailedDescription/Description.xaml",
            "FriedmanTest/friedman.png")]
    public class FriedmanTest : IStatistic
    {public FriedmanTest()
        {
            settings = new FriedmanTestSettings();

        }
    #region Private Variables
    private double keyLength;
    private string stringOutput="";
    private int [] arrayInput;
    #endregion


    #region Properties (Inputs/Outputs)

    [PropertyInfo(Direction.Output,"Probable key length.", "For greater accuracy, please refer to the string output.", "",false , false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public double KeyLength
    {
        get { return keyLength; }
        set
        {
            if (value != keyLength)
            {
                keyLength = value;
                OnPropertyChanged("KeyLength");
            }
        }
    }

    [PropertyInfo(Direction.Output, "Probable key length", "If the key length result seems to be ilogical...", "", false,false, DisplayLevel.Beginner, QuickWatchFormat.Text,null)]
    public string StringOutput
    {
        get { return this.stringOutput; }
        set
        {
            stringOutput = value;
            OnPropertyChanged("StringOutput");
        }
    }
    [PropertyInfo(Direction.Input, "List input", "absolute frequency of the letter, as calculated by FrequencyTest", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
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

    public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private FriedmanTestSettings settings;
        public ISettings Settings
        {
            get { return settings; }
            set { settings = (FriedmanTestSettings)value; }
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
            //throw new NotImplementedException();
        }

        public void Execute()
        {
            
            if (arrayInput != null)
            {
                double Kp; //Kappa "language"
				long cipherTextLength = 0; //n
				long countDoubleCharacters = 0;

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
 
				for (int i = 0; i < arrayInput.Length; i++)
				{
					cipherTextLength += arrayInput[i];
					countDoubleCharacters += (arrayInput[i] * (arrayInput[i] - 1));
				}

				double kappaCiphertext = ((double)countDoubleCharacters / (double)(cipherTextLength * (cipherTextLength - 1)));
				double keyLen = 0.0377 * cipherTextLength / (((cipherTextLength - 1) * kappaCiphertext) - (0.0385 * cipherTextLength) + Kp);
                
				stringOutput = Convert.ToString(keyLen);
                keyLength = keyLen;
                OnPropertyChanged("OutputString");
                OnPropertyChanged("KeyLength");
                if (OnPluginProgressChanged != null)
                {
					OnPluginProgressChanged(this, new PluginProgressEventArgs(cipherTextLength, cipherTextLength));
                }
            }

        }

        public void PostExecution()
        {
            //throw new NotImplementedException();
        }

        public void Pause()
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
