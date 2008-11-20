using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Analysis;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Cryptool.FrequencyTest;
using System.Collections;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Cryptool.FrequencyTest
{
    [PluginInfo(false,
    "Frequency Test",
    "Calculates the frequency of letters of a string.",
    "URL",
    "FrequencyTest/icon.png")]    
    
     
    
    public partial class FrequencyTest : IStatistic
    {
        private string stringOutput = "";
        private string stringInput;

        public static DataSource Data = new DataSource();
        
        

        #region Properties (Inputs/Outputs)

        [PropertyInfo(Direction.Input, "The string to be analyzed", "blablah", "",true, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string StringInput
        {
            get
            {
                return stringInput ;
            }
            set { stringInput = value; OnPropertyChanged("StringInput"); }
        }

        [PropertyInfo(Direction.Output, "Text output", "The string after processing with the Frequency test", "",false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string StringOutput
        {
            get { return stringOutput; }
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

        private FrequencyTestSettings settings;
        public ISettings Settings
        {
            get { return settings; }
            set { settings = (FrequencyTestSettings)value; }
        }
        private FrequencyTestPresentation presentation;
        public FrequencyTest()
        {
        settings = new FrequencyTestSettings();
        presentation = new FrequencyTestPresentation(this);
        Presentation = presentation;
        }
        public UserControl Presentation { get; private set; }


        public UserControl QuickWatchPresentation
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

                string workstring = stringInput;
                string workstring2 = "";
                if (settings.CaseSensitivity == 0)
                {
                    workstring = workstring.ToLower();
                }
                if (settings.unknownSymbolHandling == 1)
                {
                    char[] validchars = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
                    string strValidChars = new string(validchars);
                    StringBuilder workstring1 = new StringBuilder();
                    foreach (char c in workstring.ToCharArray())
                    {
                        if (strValidChars.IndexOf(c) >= 0)
                        {
                            workstring1.Append(c);
                        }
                    }

                    workstring2 = workstring1.ToString();
                }
                else 
                {
                    workstring2 = workstring;
                }
               
                
                string tempString="";
                int l = 0;
                ArrayList gramms = new ArrayList();
                while (l >= 0 & l <= workstring2.Length - 1)
                {

                    tempString+=workstring2[l];
                    l++;
                    if (l % settings.GrammLength == 0 & l > 0)
                    {
                        gramms.Add(tempString);
                        tempString = "";
                    }

                }
                
                gramms.Sort();
                ArrayList amountCharacters=new ArrayList();
                ArrayList percentageCharacters = new ArrayList();
                ArrayList countedGramms= new ArrayList();
                int tempInt=0;
                Data.ValueCollection.Clear();

                for (int n = 0; n < gramms.Count; n++)
                {
                    
                    tempInt=gramms.LastIndexOf(gramms[n]) - gramms.IndexOf(gramms[n]) + 1;
                    amountCharacters.Add(tempInt);
                    Data.AddtoCollection(tempInt);
                    percentageCharacters.Add(Math.Round(Convert.ToDouble(tempInt) * settings.GrammLength / Convert.ToDouble(StringInput.Length) * 100, 3));
                    countedGramms.Add(gramms[n]);
                    n = gramms.LastIndexOf( gramms[n]);
                  
                }

                //OUTPUT
                stringOutput = "";
                for (int x = 0; x < countedGramms.Count; x++)
                  {
                     
                    stringOutput += countedGramms[x] + ":" + amountCharacters[x] + ":" + percentageCharacters[x] + Environment.NewLine;
                  }
                 OnPropertyChanged("StringOutput");
                 if (OnPluginProgressChanged != null)
                      {
                         OnPluginProgressChanged(this, new PluginProgressEventArgs(l, l));
                      }
                 presentation.OpenPresentationFile();
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

    
  public  class DataSource 
    {
    
       private  ObservableCollection<int> valueCollection;

        public  ObservableCollection<int> ValueCollection
        {
            get { return valueCollection; }
            set { valueCollection = value; }
        }

        public void AddtoCollection(int i)
        {
            valueCollection.Add(i);     
        }
       
       public DataSource(){
        
           valueCollection = new ObservableCollection<int>();
           //valueCollection.Add(200);
                                  
            


        }



       
    }


  
  
}
