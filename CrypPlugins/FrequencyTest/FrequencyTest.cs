﻿using System;
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

        [PropertyInfo(Direction.Output, "Text output", " letter:absolute frequency of the letter:relative frequency of the letter (in %)  ", "",false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
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
                    char[] validchars = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z','A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z' };
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
                double bigestheight=0;
                for (int n = 0; n < gramms.Count; n++)
                {

                    tempInt = gramms.LastIndexOf(gramms[n]) - gramms.IndexOf(gramms[n]) + 1;
                    amountCharacters.Add(tempInt);
                    percentageCharacters.Add(Math.Round(Convert.ToDouble(tempInt) * settings.GrammLength / Convert.ToDouble(StringInput.Length) * 100, 3));
                    countedGramms.Add(gramms[n]);
                    
                   // if (workstring2.Length <= 50)
                   // { height = tempInt * 20; }
                  //  if (workstring2.Length <= 100&&workstring2.Length>50)
                   // { height = tempInt * 10; }
                  //  if (workstring2.Length <= 200 && workstring2.Length > 100)
                  //  { height = tempInt * 5; }
                  //  if (workstring2.Length <= 300 && workstring2.Length > 200)
                  //  { height = tempInt * 3; }
                  //  if (workstring2.Length <= 400 && workstring2.Length > 300)
                  //  { height = tempInt * 2; }
                  //  if (workstring2.Length > 400)
                  //  { height = tempInt; }
                  //  else
                  //  {
                    if (bigestheight< (double)percentageCharacters[percentageCharacters.Count-1])
                    {
                        bigestheight = (double)percentageCharacters[percentageCharacters.Count - 1]; 
                    }
                   // }
                   // CollectionElement row = new CollectionElement(height, (double)percentageCharacters[percentageCharacters.Count - 1],(string)countedGramms[countedGramms.Count - 1] );

                   // Data.ValueCollection.Add(row);
                    n = gramms.LastIndexOf(gramms[n]);

                }

                //percentageCharacters.
                for (int n = 0; n < countedGramms.Count; n++)
                {
                    int height = Convert.ToInt32((double)percentageCharacters[n] * (180 / bigestheight));
                    CollectionElement row = new CollectionElement(height, (double)percentageCharacters[n], (string)countedGramms[n]);
                    Data.ValueCollection.Add(row);
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

   
  
}





