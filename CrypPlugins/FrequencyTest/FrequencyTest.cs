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
               
        
        public FrequencyTest()
        {
           
            settings = new FrequencyTestSettings();
            
        }
       

       
       
        
      
        
        
        
        
        private string outputString = "";
        private string stringInput;

        public static DataSource Data = new DataSource();
        FrequencyTestPresentation presentation =new FrequencyTestPresentation();
        

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
        public string OutputString
        {
            get { return outputString; }
            set
            {
                outputString = value;
                OnPropertyChanged("OutputString");
               
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

        public UserControl Presentation
        {
            get {  return presentation;}
            set { this.presentation=(FrequencyTestPresentation)value;}
        }

        public UserControl QuickWatchPresentation
        {
            get { return presentation; }
        }

        public void PreExecution()
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Here comes a comment about this method.
        /// </summary>
        
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
                    
                    workstring2 =  workstring1.ToString();
                }

               
                
                string tempString="";
                int l = 0;
                ArrayList gramms = new ArrayList();
                
                //  if (OnGuiLogNotificationOccured != null)
                //{
                //    OnGuiLogNotificationOccured(this, new GuiLogEventArgs("Creating list", this, NotificationLevel.Info));
                //}

                while (l >= 0 & l <= StringInput.Length - 1)
                {

                    tempString+=StringInput[l];
                    l++;
                    if (l % settings.GrammLength == 0 & l > 0)
                    {
                        gramms.Add(tempString);
                        tempString = "";
                    }

                  //  if (OnPluginProgressChanged != null)
                   // {
                    //    OnPluginProgressChanged(this, new PluginProgressEventArgs(l, StringInput.Length - 1));
                   // }
                }
                
                 gramms.Sort();
                //DataSource Data = new DataSource();
                ArrayList amountCharacters=new ArrayList();
                ArrayList percentageCharacters = new ArrayList();
                ArrayList countedGramms= new ArrayList();
                int tempInt=0;
                Data.ValueCollection.Clear();

                //FrequencyTestPresentation presentation1 = new FrequencyTestPresentation();
                for (int n = 0; n < gramms.Count; n++)
                {
                    
                    tempInt=gramms.LastIndexOf(gramms[n]) - gramms.IndexOf(gramms[n]) + 1;
                    amountCharacters.Add(tempInt);
                    Data.AddtoCollection(tempInt);
                    percentageCharacters.Add(Math.Round(Convert.ToDouble(tempInt) * settings.GrammLength / Convert.ToDouble(StringInput.Length) * 100, 3));
                    countedGramms.Add(gramms[n]);
                    //presentation.UpdateData(Data);
                    //presentation.
                    
                    
                    //outputString += gramms[n] + ":" + amountCharacters + ":" + percentageCharacters + Environment.NewLine;
                    //OnPropertyChanged("OutputString");

                    
                    n = gramms.LastIndexOf( gramms[n]);
                                     
                    
                    
                    
                }

                
                //Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Send, (ThreadStart)delegate
               // {
                    
                //   Presentation= presentation;
                 //  Presentation.UpdateLayout();
                //});
                
                //OUTPUT
                OutputString = "";
                //DataSource present=new DataSource();
                for (int x = 0; x < countedGramms.Count; x++)
                  {
                     
                    OutputString += countedGramms[x] + ":" + amountCharacters[x] + ":" + percentageCharacters[x] + '\n';
                  }


               
               
                
              //  Thread.Sleep(500);

                
                    
                    
                    
                   
               
                
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
