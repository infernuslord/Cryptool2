using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Analysis;
using System.ComponentModel;

namespace FriedmanTest
{
    [Author("Georgi Angelov & Danail Vazov", "vazov@cryptool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo(false,
            "Friedman Test",
            "Calculates the probable keylenght of a polyalphabetic substitusion cipher",
            "URL",
            "FriedmanTest/icon.png")]
    public class FriedmanTest : IStatistic
    {public FriedmanTest()
        {
            settings = new FriedmanTestSettings();

        }
    #region Private Variables
    private int integerValue;
    private string stringInput="";
    private string stringOutput="";
    #endregion


    #region Properties (Inputs/Outputs)

    [PropertyInfo(Direction.Input, "The string to be analyzed", "Caution: Aplaying a string, other than the one outputed from the FrequencyTest plug-in, will result in ilogical results", "",false, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public string StringInput
    {
        get
        {
            return stringInput;
        }
        set { stringInput = value; OnPropertyChanged("StringInput"); }
    }
    [PropertyInfo(Direction.Output,"Probable key length.", "For greater accuracy, please refer to the string output.", "",false , false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public int IntegerValue
    {
        get { return integerValue; }
        set
        {
            if (value != integerValue)
            {
                integerValue = value;
                OnPropertyChanged("IntegerValue");
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
            
            if (stringInput != null)
            {
                //Edit the input from Frequency Test in order to extract the observed letter frequencies
                string string1 = stringInput;
                string string2 = string1.Replace(Environment.NewLine, ":");
               
               
                string[] split = null;



                
                split = string2.Split(':');
                
                //Now we put the needed absolute values of the letter frequencies into an array
                int[] absolutevalue = new int[Convert.ToInt32(split.Length / 3)];
               
                int j = 0;
                for (int i = 1; i <= split.Length-2; i=i+3)
                { 
                absolutevalue[j] = Convert.ToInt32(split[i]);
                j++;

                }
               
                //Now we begin calculation of the arithmetic sum of the frequencies
                int[] summ=new int [absolutevalue.Length];
                for (int d = 0; d < absolutevalue.Length; d++)
                    {
                        summ[d] = absolutevalue[d] * (absolutevalue[d] - 1);
                        
                    }
                int summ1 = 0;
                foreach (int z in summ) 
                {
                     summ1 += z;
                     
                }
                //outputString = Convert.ToString(summ1);
                //OnPropertyChanged("OutputString");

                //Now we calculate the length of text from the observed letter frequencies
                int texLen = 0;
                foreach (int y in absolutevalue)
                {
                    texLen += y;
                }
                //outputString = Convert.ToString(texLen);
                //OnPropertyChanged("OutputString");
                double normTexLen = texLen * (texLen - 1); //Normalize the text length in order to calculate the observed index of coincidence
                //outputString = Convert.ToString(Convert.ToDecimal(normTexLen));
                //OnPropertyChanged("OutputString");
                double obIC = summ1/normTexLen; //Calculates the observed index of coincidence
                //outputString = Convert.ToString(Convert.ToDecimal(obIC));
                //OnPropertyChanged("OutputString");
                double Kr = 0.038; //Kappa "random" - expected coincidence rate for a uniform distribution of the alphabet. In this case 1/26, hence we should have a 26 letter alphabet on the input. 
                double Kp = 0.065; //Kappa "plain-text" 
                double keyLen = 0.027 * texLen / (((texLen - 1) * obIC) - (Kr * texLen) + Kp);
                stringOutput = Convert.ToString(keyLen);
                OnPropertyChanged("OutputString");
                if (OnPluginProgressChanged != null)
                {
                    OnPluginProgressChanged(this, new PluginProgressEventArgs(texLen, texLen));
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
