using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Analysis;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using Cryptool.PluginBase.IO;
using System.Collections.ObjectModel;
using Cryptool.MonoalphabeticAnalysis;
using System.Collections;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Runtime.CompilerServices;
using Cryptool.PluginBase.Miscellaneous;

using System.Runtime.Remoting.Contexts;


namespace Cryptool.MonoalphabeticAnalysis
{
    [Author("Georgi Angelov", "angelov@cryptool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo(false,
    "MonoalphabeticAnalysis",
    "Proposes Alphabet for the substitution cypher.",
    "",
    "MonoalphabeticAnalysis/icon.png")]



    public partial class MonoalphabeticAnalysis : IStatistic
    {
        private string stringOutput = "";
        private string statisticTextInput;
        private string letterFrequencyInput;
        
        
       // private int[] arrayOutput;

        // public static DataSource Data = new DataSource();



        #region Properties (Inputs/Outputs)

        [PropertyInfo(Direction.Input, "Statistic text used for analysis", "Statistic Text Input", "", true, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string StatisticTextInput
        {
            get
            {
                return statisticTextInput;
            }
            set { statisticTextInput = value; OnPropertyChanged("StatisticTextInput"); }
        }



        [PropertyInfo(Direction.Input, "Letter Frequency From FrequencyTest Plug-in", "Letter Frequency Input", "", true, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string LetterFrequencyInput
        {
            get
            {
                return letterFrequencyInput;
            }
            set { letterFrequencyInput = value; OnPropertyChanged("LetterFrequencyInput"); }
        }


        [PropertyInfo(Direction.Output, "String output", "Proposal Alphabets", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string StringOutput
        {
            get { return stringOutput; }
            set
            {
                StringOutput = value;
                OnPropertyChanged("StringOutput");

            }
        }
        #endregion


        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private MonoalphabeticAnalysisSettings settings;
        public ISettings Settings
        {
            get { return settings; }
            set { settings = (MonoalphabeticAnalysisSettings)value; }
        }
        // private FrequencyTestPresentation presentation;
        public MonoalphabeticAnalysis()
        {
            settings = new MonoalphabeticAnalysisSettings();
            // presentation = new FrequencyTestPresentation(this);
            // Presentation = presentation;
            //  QuickWatchPresentation = presentation;
        }
        public UserControl Presentation { get; private set; }


        public UserControl QuickWatchPresentation { get; private set; }

        public void PreExecution()
        {
            //throw new NotImplementedException();
        }

        public void Execute()
        {
            if (letterFrequencyInput != null)
            {


                string alphabet = "";
                
                ArrayList mapping1 = InitialMapping(LetterFrequencyInput, statisticTextInput);

                for (int i = 0; i < mapping1.Count; i++)
                {
                    CollectionElement a1 = (CollectionElement)mapping1[i];
                    alphabet += a1.Mapping;
                }

                alphabet=alphabet.ToUpper();

               stringOutput = alphabet;

                             

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


        #region CUSTOM METHODS

        public ArrayList ReturnSortedList(string str,SortElements.SortElemetsBy sort_type)        // The Method returns a sorted list from the FrequenscyTest string output.
        {
            
            str = str.Replace(Environment.NewLine, ":");

            char[] splitter = { ':' };

            
            string[] splitfrequencies = str.Split(splitter);
            // splitfrequencies.

            ArrayList sortingfrequencyList = new ArrayList();

                       
                for (int i = 0; i < splitfrequencies.Length - 1; i = i + 3)
                {
                    CollectionElement row = new CollectionElement(splitfrequencies[i], System.Convert.ToDouble(splitfrequencies[i + 2]),"");
                    sortingfrequencyList.Add(row);
                }
             
                                 

            SortElements mySortingType = new SortElements(sort_type);
            sortingfrequencyList.Sort(mySortingType);

            return sortingfrequencyList;
        }


        ArrayList InitialMapping(string cyString, string statString)
        {

            ArrayList cypherTextFrequencies = ReturnSortedList(cyString, SortElements.SortElemetsBy.byFrequency);
            ArrayList shogunFrequencies = ReturnSortedList(statString, SortElements.SortElemetsBy.byFrequency);

            ArrayList mappings = new ArrayList();

            if (cypherTextFrequencies.Count == shogunFrequencies.Count)
            {
                for (int i = 0; i < cypherTextFrequencies.Count ; i++)
                {
                    CollectionElement a1 = (CollectionElement)cypherTextFrequencies[i];
                    CollectionElement a2 = (CollectionElement)shogunFrequencies[i];
                    double frequencyDifference = Math.Abs(a1.Frequency - a2.Frequency);
                    CollectionElement row = new CollectionElement(a1.Caption, frequencyDifference, a2.Caption);
                    mappings.Add(row);

                }


            }
            SortElements mySortingType = new SortElements(SortElements.SortElemetsBy.byString);
            mappings.Sort(mySortingType);

            return mappings;
        }
        
        #endregion CUSTOM METHODS

    }

       




    

   
}










