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
    "Proposes Alphabet for the substitution cipher.",
    "",
    "MonoalphabeticAnalysis/icon.png")]



    public partial class MonoalphabeticAnalysis : IStatistic
    {
        private string stringOutput = "";
        private string textStatisticInput_Monograms = ""; //Generated from FrequencyTest. Contains monogram/letter frequencies of  arbitrary text. Used for initial mapping and proposal of alphabet.
        private string textStatisticInput_Digrams="";//Generated from FrequencyTest. It is Used in the Cost Function for each proposed alphabet to generate the alphabetGoodness number.
        private string cipherTextFrequencyInput_Monograms = ""; //Generated from FrequencyTest. Compared with textStatisticInput_Monograms and used for initial mapping of letters and first proposal alphabet.
        private string decipherAttempt_Digrams = ""; //Generated from FrequencyTest. After the initial alphabet is proposed and decryption attempt from substitution plug-in is generated, the result is analysed with Frequencytest set on digrams and fed to MonoalphabeticAnalysis as input. It is used on the second pass through the execute method. 
        public string GoodAlphabet = null;
        public string NextAlphabet = null;
        private double alphabetGoodnes = 0;//result of the cost function. Used in ReturnAlphabetGoodness method.
        public int AlphabetCounterA = 1;
        public int AlphabetcounterB = 1;
        



        #region Properties (Inputs/Outputs)

        [PropertyInfo(Direction.Input, "TextStatisticInput_Monograms", "TextStatisticInput_Monograms", "", false, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string TextStatisticInput_Monograms
        {
            get
            {
                return textStatisticInput_Monograms;
            }
            set { textStatisticInput_Monograms = value; OnPropertyChanged("textStatisticInput_Monograms"); }
        }


        [PropertyInfo(Direction.Input, "StatisticTextFrequencyInput_Digrams", "StatisticTextFrequencyInput_Digrams", "", false, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string StatisticTextFrequencyInput_Digrams
        {
            get
            {
                return textStatisticInput_Digrams;
            }
            set { textStatisticInput_Digrams = value; OnPropertyChanged("StatisticTextFrequencyInput_Digrams"); }
        }


        [PropertyInfo(Direction.Input, "CipherTextFrequencyInput_Monograms", "CipherTextFrequencyInput_Monograms", "", true, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string CipherTextFrequencyInput_Monograms
        {
            get
            {
                return cipherTextFrequencyInput_Monograms;
            }
            set { cipherTextFrequencyInput_Monograms = value; OnPropertyChanged("CipherTextFrequencyInput_Monograms"); }
        }

        [PropertyInfo(Direction.Input, "DecipherAttempt_Digrams", "DecipherAttempt_Digrams", "", false, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string DecipherAttempt_Digrams
        {
            get
            {
                return decipherAttempt_Digrams;
            }
            set { decipherAttempt_Digrams = value; OnPropertyChanged("DecipherAttempt_Digrams"); }
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
        //EXECUTE!!!//    //IMPLEMENTATION ATTEMPT OF THE ALGORITHM: Jakobsen, Thomas(1995)'A FAST METHOD FOR CRYPTANALYSIS OF SUBSTITUTION CIPHERS',Cryptologia,19:3,265 — 274
        //////////////
      
        public void Execute() 
        {
            

            if (cipherTextFrequencyInput_Monograms != "") 
            {
                   //if Optional Statistical input not used -> use variables from pre-analysed Text (James Clavel's Shogun in our case)  
                if (TextStatisticInput_Monograms == "") textStatisticInput_Monograms = ShogunStatistics.ShogunMonograms;


                 if (GoodAlphabet == null) //first pass
                 {
                     //GoodAlphabet equals null means initial alphabet should be generated
                    string alphabet = "";
                    ArrayList mapping1 = InitialMapping(cipherTextFrequencyInput_Monograms, textStatisticInput_Monograms);

                    alphabet = AlphabetFromMapping(mapping1);

                    GoodAlphabet = alphabet;
                    stringOutput = MapDecryptKeyAlphabet(alphabet);
                    OnPropertyChanged("StringOutput");

                 }

                

                 if (GoodAlphabet != null && decipherAttempt_Digrams!="" && AlphabetcounterB < GoodAlphabet.Length)
                 {

                     double goodness = ReturnAlphabetGoodness();
                     if (alphabetGoodnes == 0) //secondpass
                     {

                         if (settings.FastAproach==1)
                         {
                             alphabetGoodnes = goodness;
                             FastAproach();


                         }


                         else
                         {
                             alphabetGoodnes = goodness;
                             string alphabet1 = GenerateNextAlphabet();
                             NextAlphabet = alphabet1;
                             stringOutput = MapDecryptKeyAlphabet(NextAlphabet);
                             OnPropertyChanged("StringOutput");
                         }
                         



                     } 
                     
                     if (alphabetGoodnes > goodness && settings.FastAproach==0) //third pass and on 
                     {
                         GoodAlphabet = NextAlphabet;
                         alphabetGoodnes = goodness;
                         
                         string alphabet1 = GenerateNextAlphabet();
                         NextAlphabet = alphabet1;
                         stringOutput = MapDecryptKeyAlphabet(NextAlphabet);
                         OnPropertyChanged("StringOutput");
                     }

                     if (alphabetGoodnes < goodness && settings.FastAproach == 0)
                     {
                         string alphabet1 = GenerateNextAlphabet();
                         NextAlphabet = alphabet1;
                         stringOutput = MapDecryptKeyAlphabet(NextAlphabet);
                         OnPropertyChanged("StringOutput");
                     }

                     if (AlphabetcounterB == GoodAlphabet.Length)
                     {
                         stringOutput = MapDecryptKeyAlphabet(GoodAlphabet);
                         OnPropertyChanged("StringOutput");
                     }

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
            if (textStatisticInput_Monograms == "") textStatisticInput_Monograms = ShogunStatistics.ShogunMonograms;
            ArrayList cipherTextFrequencies = ReturnSortedList(cyString, SortElements.SortElemetsBy.byFrequency);
            ArrayList statisticFrequencies = ReturnSortedList(statString, SortElements.SortElemetsBy.byFrequency);

            ArrayList mappings = new ArrayList();

            if (cipherTextFrequencies.Count == statisticFrequencies.Count)
            {
                for (int i = 0; i < cipherTextFrequencies.Count ; i++)
                {
                    CollectionElement a1 = (CollectionElement)cipherTextFrequencies[i];
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

     
       //RETURN ALPHABET GOODNESS// 

        
      public double ReturnAlphabetGoodness() 
      {
          if (textStatisticInput_Digrams == "") textStatisticInput_Digrams = ShogunStatistics.ShogunDigrams;//if Optional Statistical input not used -> use variables from pre-analysed Text (James Clavel's Shogun)

          double[,] statisticDigramSquare = GetDigramFrequencySquare(StatisticTextFrequencyInput_Digrams);    
               
         
          double[,] decipherAttemptDigrams = GetDigramFrequencySquare(DecipherAttempt_Digrams);
          
          double goodness=new double();


          for (int i = 0; i < GoodAlphabet.Length; i++)
          {
              for (int n = 0; n < GoodAlphabet.Length; n++)
              {
                  goodness +=Math.Abs( statisticDigramSquare[i, n] - decipherAttemptDigrams[i, n]);
              }
          }


          return goodness;

      }


       //GENERATE NEXT ALPHABET//


      public string GenerateNextAlphabet ()
      {
          //string alphabet = GoodAlphabet;
          
          if (AlphabetcounterB != GoodAlphabet.Length)  
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
          else { return GoodAlphabet; } 

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


        //FAST APROACH//  



      public void FastAproach() 
      {
          if (textStatisticInput_Digrams == "") StatisticTextFrequencyInput_Digrams = ShogunStatistics.ShogunDigrams;//if Optional Statistical input not used -> use variables from pre-analysed Text (James Clavel's Shogun)

          double[,] statisticDigramSquare = GetDigramFrequencySquare(StatisticTextFrequencyInput_Digrams);
          double[,] digramFrequencyAttemptSquare = new double[GoodAlphabet.Length, GoodAlphabet.Length];
          digramFrequencyAttemptSquare=GetDigramFrequencySquare(decipherAttempt_Digrams);



          double[,] swappingSquare = new double[GoodAlphabet.Length, GoodAlphabet.Length];
          swappingSquare.Initialize();
         
          
          while (AlphabetcounterB < GoodAlphabet.Length-1)
          {

              for (int q = 0; q < GoodAlphabet.Length; q++)
              {
                  for (int z = 0; z < GoodAlphabet.Length; z++)
                  {
                      swappingSquare[q, z] = digramFrequencyAttemptSquare[q, z];
                  }
              }

              char[] orderedAlphabet = GoodAlphabet.ToCharArray();
              Array.Sort(orderedAlphabet);
              int swapIndex1 = Array.IndexOf(orderedAlphabet,GoodAlphabet[AlphabetCounterA-1]);                         //Find Corespondant places in ordered alphabet
              int swapIndex2 = Array.IndexOf(orderedAlphabet, GoodAlphabet[AlphabetCounterA + AlphabetcounterB - 1]);
              
              
              //Swap two rows          
              
              for (int i = 0; i < swappingSquare.GetLength(0); i++)
              {
                  double holder1 = swappingSquare[swapIndex1, i];
                  swappingSquare[swapIndex1 , i] = swappingSquare[swapIndex2, i];
                  swappingSquare[swapIndex2, i] = holder1;


              }

              int breakpoint = 0;

              //Swap two columns
              for (int n = 0; n < swappingSquare.GetLength(0); n++)
              {
                  double holder2 = swappingSquare[n, swapIndex1];
                  swappingSquare[n,swapIndex1] = swappingSquare[n, swapIndex2];
                  swappingSquare[n, swapIndex2] = holder2;
              
              }

                           
              
              double goodness1 = 0;


              for (int m = 0; m < swappingSquare.GetLength(0); m++)
              {
                  for (int g = 0; g < swappingSquare.GetLength(0); g++)
                  {
                      goodness1 += Math.Abs(statisticDigramSquare[m, g] - swappingSquare[m, g]);
                  }
              }

              NextAlphabet = GenerateNextAlphabet();

              if (goodness1 < alphabetGoodnes) 
              {
                  GoodAlphabet = NextAlphabet;
                  alphabetGoodnes = goodness1;

                  for (int q = 0; q < GoodAlphabet.Length; q++)
                  {
                      for (int z = 0; z < GoodAlphabet.Length; z++)
                      {
                          digramFrequencyAttemptSquare[q, z] = swappingSquare[q, z];
                      }
                  }
              }
                           

                      


          }

          stringOutput = MapDecryptKeyAlphabet(GoodAlphabet);
          OnPropertyChanged("StringOutput");


      }

      
        //MAPDECRYPTALPHABET//
        
        public string MapDecryptKeyAlphabet(string alph) 
        {


             string alphabet1 = "";
          
             char[] AlphabetCharArray = alph.ToCharArray();
             char[] orderedAlphabet = alph.ToCharArray();
            Array.Sort(orderedAlphabet);
          
          
            for (int i = 0; i < alph.Length; i++)
            {
                 int mapIndex = Array.IndexOf(AlphabetCharArray, orderedAlphabet[i]);
                 alphabet1 += orderedAlphabet[mapIndex];
             }

             return alphabet1;
      
        }



        #endregion CUSTOM METHODS

    }

       




    

   
}










