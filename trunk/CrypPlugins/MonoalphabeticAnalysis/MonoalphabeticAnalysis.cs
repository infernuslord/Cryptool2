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
        private string statisticTextFrequencyInput_Monograms="";
        private string statisticTextFrequencyInput_Digrams="";
        private string cypherTextFrequencyInput_Monograms="";
        private string decypherAttempt_Digrams=""; //After the initial alphabet is proposed and decrypt from substitution plug-in is generated, the result is analysed with Frequencytest concerning digrams and fed back to MonoalphabeticAnalysis as input.
        public string GoodAlphabet = null;
        public string NextAlphabet = null;
        private double alphabetGoodnes = 0;
        public int AlphabetCounterA = 1;
        public int AlphabetcounterB = 1;
        



        #region Properties (Inputs/Outputs)

        [PropertyInfo(Direction.Input, "StatisticTextFrequencyInput_Monograms", "StatisticTextFrequencyInput_Monograms", "", false, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string StatisticTextFrequencyInput_Monograms
        {
            get
            {
                return statisticTextFrequencyInput_Monograms;
            }
            set { statisticTextFrequencyInput_Monograms = value; OnPropertyChanged("StatisticTextFrequencyInput_Monograms"); }
        }


        [PropertyInfo(Direction.Input, "StatisticTextFrequencyInput_Digrams", "StatisticTextFrequencyInput_Digrams", "", false, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string StatisticTextFrequencyInput_Digrams
        {
            get
            {
                return statisticTextFrequencyInput_Digrams;
            }
            set { statisticTextFrequencyInput_Digrams = value; OnPropertyChanged("StatisticTextFrequencyInput_Digrams"); }
        }


        [PropertyInfo(Direction.Input, "CypherTextFrequencyInput_Monograms", "CypherTextFrequencyInput_Monograms", "", true, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string CypherTextFrequencyInput_Monograms
        {
            get
            {
                return cypherTextFrequencyInput_Monograms;
            }
            set { cypherTextFrequencyInput_Monograms = value; OnPropertyChanged("CypherTextFrequencyInput_Monograms"); }
        }

        [PropertyInfo(Direction.Input, "DecypherAttempt_Digrams", "DecypherAttempt_Digrams", "", false, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string DecypherAttempt_Digrams
        {
            get
            {
                return decypherAttempt_Digrams;
            }
            set { decypherAttempt_Digrams = value; OnPropertyChanged("DecypherAttempt_Digrams"); }
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



        [PropertyInfo(Direction.Output, "Alphabet goodness", "Alphabet goodness", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string AlphabetGoodness
        {
            get { return alphabetGoodnes.ToString(); }
            set
            {
                StringOutput = value;
                OnPropertyChanged("Alphabetgoodness");

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
        
        
        
        //////////////
        //EXECUTE!!!//
        //////////////
      
        public void Execute()
        {
            

            if (cypherTextFrequencyInput_Monograms != "")
            {
                   //if Optional Statistical input not used -> use variables from pre-analysed Text (James Clavel's Shogun)  
                 if (statisticTextFrequencyInput_Monograms == "") statisticTextFrequencyInput_Monograms = ShogunStatistics.ShogunMonograms;


                 if (GoodAlphabet == null)
                 {
                    //decypherAttempt_Digrams equals null means initial alphabet should be generated
                    string alphabet = "";
                    ArrayList mapping1 = InitialMapping(cypherTextFrequencyInput_Monograms, statisticTextFrequencyInput_Monograms);

                    alphabet = AlphabetFromMapping(mapping1);

                    GoodAlphabet = alphabet;
                    stringOutput = alphabet;
                    OnPropertyChanged("StringOutput");

                 }



                 if (GoodAlphabet != null && decypherAttempt_Digrams!="")
                 {

                     double goodness = ReturnAlphabetGoodness();
                     if (alphabetGoodnes == 0) //secondpass
                     { 
                         alphabetGoodnes = goodness;
                     } 
                     if (alphabetGoodnes > goodness) //third pass and on if condition applies
                     {
                         GoodAlphabet = NextAlphabet;
                         alphabetGoodnes = goodness;
                     }

                     string alphabet1 = GenerateNextAlphabet();
                     NextAlphabet = alphabet1;
                     stringOutput = NextAlphabet;
                     OnPropertyChanged("StringOutput");




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
            //statisticTextFrequencyInput_Monograms = "";
            //OnPropertyChanged("StatisticTextFrequencyInput_Monograms");
           // statisticTextFrequencyInput_Digrams = "";
            //OnPropertyChanged("StatisticTextFrequencyInput_Digrams");
            
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




        //RETURN SORTED LIST//  // The Method returns a sorted list from the FrequenscyTest string output.
        
        public ArrayList ReturnSortedList(string str,SortElements.SortElemetsBy sort_type)        
        {
            
            str = str.Replace(Environment.NewLine, ":");

            char[] splitter = { ':' };

            
            string[] splitFrequencies = str.Split(splitter);
            

            ArrayList sortingFrequencyList = new ArrayList();

                       
                for (int i = 0; i < splitFrequencies.Length - 1; i = i + 3)
                {
                    CollectionElement row = new CollectionElement(splitFrequencies[i], System.Convert.ToDouble(splitFrequencies[i + 2]),"");
                    sortingFrequencyList.Add(row);
                }
             
                                 

            SortElements mySortingType = new SortElements(sort_type);
            sortingFrequencyList.Sort(mySortingType);

            return sortingFrequencyList;
        }



        //INITIAL MAPPING//

       public ArrayList InitialMapping(string cyString, string statString)
        {
            if (statisticTextFrequencyInput_Monograms == "") statisticTextFrequencyInput_Monograms = ShogunStatistics.ShogunMonograms;
            ArrayList cypherTextFrequencies = ReturnSortedList(cyString, SortElements.SortElemetsBy.byFrequency);
            ArrayList statisticFrequencies = ReturnSortedList(statString, SortElements.SortElemetsBy.byFrequency);

            ArrayList mappings = new ArrayList();

            if (cypherTextFrequencies.Count == statisticFrequencies.Count)
            {
                for (int i = 0; i < cypherTextFrequencies.Count ; i++)
                {
                    CollectionElement a1 = (CollectionElement)cypherTextFrequencies[i];
                    CollectionElement a2 = (CollectionElement)statisticFrequencies[i];
                    double frequencyDifference = Math.Abs(a1.Frequency - a2.Frequency);
                    CollectionElement row = new CollectionElement(a1.Caption, frequencyDifference, a2.Caption);
                    mappings.Add(row);

                }


            }
            SortElements mySortingType = new SortElements(SortElements.SortElemetsBy.byString);
            mappings.Sort(mySortingType);

            return mappings;
        }




        //ALPHABET FROM MAPPING//

      public string AlphabetFromMapping(ArrayList mapping)
        {
            string alphabet = "";
            
            for (int i = 0; i < mapping.Count; i++)
            {
                CollectionElement a1 = (CollectionElement)mapping[i];
                alphabet += a1.Mapping;
            }

            alphabet = alphabet.ToUpper();
            
            return alphabet;
 
        }

     
       //RETURN ALPHABET GOODNESS// Revision Needed!!!
        
      public double ReturnAlphabetGoodness() 
      {
          if (statisticTextFrequencyInput_Digrams == "") StatisticTextFrequencyInput_Digrams = ShogunStatistics.ShogunDigrams;//if Optional Statistical input not used -> use variables from pre-analysed Text (James Clavel's Shogun)

          double[,] statisticDigramSquare = GetDigramFrequencySquare(StatisticTextFrequencyInput_Digrams);
          double[,] decypherAttemptDigrams = GetDigramFrequencySquare(DecypherAttempt_Digrams);
         // double[,] digramFrequencyDifferences = new double[GoodAlphabet.Length, GoodAlphabet.Length];
          double goodness=new double();


          for (int i = 0; i < GoodAlphabet.Length; i++)
          {
              for (int n = 0; n < GoodAlphabet.Length; n++)
              {
                  goodness +=Math.Abs( statisticDigramSquare[i, n] - decypherAttemptDigrams[i, n]);
              }
          }


          return goodness;





         /* ArrayList statisticList_Digrams = ReturnSortedList(statisticTextFrequencyInput_Digrams, SortElements.SortElemetsBy.byFrequency);

          ArrayList decypherAttemptList_Digrams = ReturnSortedList(digramsFrequencies, SortElements.SortElemetsBy.byFrequency);

          ArrayList digramFrequencyDifference = statisticList_Digrams;

          for (int i = 0; i < decypherAttemptList_Digrams.Count; i++)
          {
              CollectionElement a1 = (CollectionElement)decypherAttemptList_Digrams[i];
              string name1 = a1.Caption;

              for (int n = 0; n < statisticList_Digrams.Count; n++)
              {
                  CollectionElement a2 = (CollectionElement)statisticList_Digrams[n];
                  string name2 = a2.Caption;
                  if (name1 == name2)
                  {
                      double difference = a2.Frequency - a1.Frequency;
                      CollectionElement row = new CollectionElement(name1, difference, "");
                      digramFrequencyDifference[n] = (Object)row;
                  }
                  if (name1[0] != name2[0]) break;
              }


          }
          double sumOfDigramFrequencyDifferences = 0;

          for (int m = 0; m < digramFrequencyDifference.Count; m++)
          {
              CollectionElement z = (CollectionElement)digramFrequencyDifference[m];
              sumOfDigramFrequencyDifferences = sumOfDigramFrequencyDifferences + z.Frequency;
          }

          return Math.Abs(sumOfDigramFrequencyDifferences); */

         /* decypherAttemptList_Digrams.Reverse();
          statisticList_Digrams.Reverse();
          int hits=0;

          //CollectionElement[,] darray = new CollectionElement[GoodAlphabet.Length,GoodAlphabet.Length];
          for (int i = 0; i < 30; i++)
          {
              CollectionElement a1 = (CollectionElement)decypherAttemptList_Digrams[i];
              CollectionElement a2 = (CollectionElement)statisticList_Digrams[i];



              if (a1.Caption == a2.Caption) hits += GoodAlphabet.Length * GoodAlphabet.Length - i * GoodAlphabet.Length;
          }

          return hits; */

      }


       //GENERATE NEXT ALPHABET//

      public string GenerateNextAlphabet ()
      {
          //string alphabet = GoodAlphabet;
          
          if (AlphabetcounterB != GoodAlphabet.Length)  //TO DO:OTHER WAY TO REALY EXIT
          {
              char[] alphabet=GoodAlphabet.ToCharArray();

              char holder = alphabet[AlphabetCounterA - 1];
              alphabet[AlphabetCounterA - 1] = alphabet[AlphabetCounterA + AlphabetcounterB - 1];
              alphabet[AlphabetCounterA + AlphabetcounterB - 1] = holder;
              AlphabetCounterA++;
              if (AlphabetCounterA + AlphabetcounterB > alphabet.Length)
              {
                  AlphabetCounterA = 1;
                  AlphabetcounterB++;
              }

              string alphabet1=null;
              alphabet1 = new string(alphabet);
              return alphabet1;
          }
          else { return GoodAlphabet; } //TO DO: OTHER WAY TO REALY EXIT

      }


     
        
        //GET DIGRAM FREQUENCY SQUARE//

      
        
      public double[,] GetDigramFrequencySquare(string digrams) 
      {
          double[,] digramFrequencySquare = new double[GoodAlphabet.Length, GoodAlphabet.Length];
          char[] alphabetCharacters = GoodAlphabet.ToCharArray();
          ArrayList digramfrequencyList = ReturnSortedList(digrams,SortElements.SortElemetsBy.byString);
          
          
          Array.Sort(alphabetCharacters);

          foreach (CollectionElement item in digramfrequencyList)
          {
              int firstLetterIndex = Array.IndexOf(alphabetCharacters,item.Caption.ToUpper()[0]);
              int secondLetterIndex = Array.IndexOf(alphabetCharacters, item.Caption.ToUpper()[1]);
              digramFrequencySquare[firstLetterIndex,secondLetterIndex]=item.Frequency;     
              
          }


          return digramFrequencySquare;
                 
      }


        #endregion CUSTOM METHODS

    }

       




    

   
}










