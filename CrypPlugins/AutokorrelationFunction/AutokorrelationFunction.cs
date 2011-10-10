/* HOWTO: Change year, author name and organization.
   Copyright 2010 Your Name, University of Duckburg

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Windows.Media;
using Cryptool.PluginBase.Utils.Graphics.Diagrams.Histogram;

namespace Cryptool.Plugins.AutokorrelationFunction
{
    [Author("Dennis Nolte", "nolte@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("AutokorrelationFunction.Properties.Resources", false, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "AutokorrelationFunction/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class AutokorrelationFunction : ICrypComponent
    {
        #region Private Variables

        private readonly AutokorrelationFunctionSettings settings;
        private AutocorrelationPresentation presentation;

        private String cipher = "";                                     //The cipher to be analysed
        private int probablelength = 0;                                 //estimated keylength
        private double probablekorr = -999999.999999;                   //initialized probable korrelation of the length
        private String alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";         //used alphabet
        private double same;                                            //Found same letter counter
        private double[] ak;                                            // Autokorrelation Values
        private HistogramElement bar;                                   
        private HistogramDataSource data;

        #endregion

        #region Data Properties

        /// <summary>
        /// The input for the ciphertext 
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputCipherCaption", "InputCipherTooltip", "", true, false, QuickWatchFormat.Text, null)]
        public String InputCipher
        {
            get
            {
                return cipher;
            }
            set
            {
                this.cipher = value;
                OnPropertyChanged("InputCipher");
            }
        }

        /// <summary>
        /// The output for the found shift value (most probable keylength) 
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputLengthCaption", "OutputLengthTooltip", "")]
        public int OutputLength
        {
            get
            {
                return probablelength;
            }
            set
            {
                this.probablelength = value;
                OnPropertyChanged("OutputLength");
            }
        }

        #endregion

        #region IPlugin Members

        public AutokorrelationFunction()
        {
            settings = new AutokorrelationFunctionSettings();
            presentation = new AutocorrelationPresentation();
            HistogramElement bar = new HistogramElement(0, 0, "");
            data = new HistogramDataSource();

        }
        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return presentation; }
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {

//START------------------------------------------------------------------------------------------------------------
//Preparations for the Analyse-------------------------------------------------------------------------------------

            if (InputCipher != null)                                //Starts only if a ciphertext is set
            {
                ProgressChanged(0, 1);

                cipher = InputCipher;                               //initialising the ciphertext
                cipher = prepareForAnalyse(cipher);                 //and prepare it for the analyse (-> see private methods section)

                ak = new double[cipher.Length];                     //initialise ak[]...there are n possible shifts where n is cipher.length

                presentation.histogram.SetBackground(Brushes.Beige);              //sets the background colour for the quickwatch
                presentation.histogram.SetHeadline( typeof(AutokorrelationFunction).GetPluginStringResource("Autocorrelation_matches") );    //sets its title

                //-----------------------------------------------------------------------------------------------------------------
                //Analyse----------------------------------------------------------------------------------------------------------
                //-----------------------------------------------------------------------------------------------------------------		

                //for each possible shift value...
                for (int t = 0; t < cipher.Length; t++)
                {
                    same = 0;

                    //...calculate how often the letters match...
                    for (int x = 0; x < cipher.Length - t; x++)
                    {
                        if (cipher[x] == cipher[x + t])
                        {
                            same++;
                        }
                    }

                    try
                    {
                        //...and save the count for the matches at the shift position
                        ak[t] = same;
                    }
                    catch
                    {
                    }
                }

                data.ValueCollection.Clear();

                //for all observed shifts...
                for (int y = 1; y < ak.Length; y++)
                {
                    //find the one with the highest match count...
                    if (ak[y] > probablekorr)
                    {
                        probablekorr = ak[y];
                        probablelength = y;                 //...and remember this shift value
                    }
                }

                //find the top 13 matches...
                if (ak.Length > 11)
                {
                    ak = findTopThirteen(ak);
                }

                for (int y = 1; y < ak.Length; y++)
                {
                    if (ak[y] > -1)                         //Adds a bar into the presentation if it is higher then the average matches
                    {
                        bar = new HistogramElement(ak[y], ak[y], "" + y);
                        data.ValueCollection.Add(bar);
                    }
                }

                
                presentation.histogram.SetHeadline( String.Format( typeof(AutokorrelationFunction).GetPluginStringResource("Highest_match_count_with_shift"), probablekorr, probablelength ));

                if (data != null)
                {
                    presentation.histogram.ShowData(data);
                }

                OutputLength = probablelength;              //sending the keylength via output
                OnPropertyChanged("OutputLength");
            }  


            ProgressChanged(1, 1);

//EXECUTE END------------------------------------------------------------------------------------------------------
        
        }

        public void PostExecution()
        {
        }

        public void Pause()
        {
        }

        public void Stop()
        {
            presentation.histogram.SetBackground(Brushes.LightGray);
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        #endregion

        #region Private Methods

//PREPARE PART---------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Remove spaces and symbols not provided by the alphabet from the text
        /// </summary>
        private String prepareForAnalyse(String c)
        {
            String prepared = "";

            c = c.ToUpper();

            for (int x = 0; x < c.Length; x++)
            {
                if (getPos(c[x]) != -1)
                {
                    prepared = prepared + c[x];
                }
            }
            return prepared;
        }


//---------------------------------------------------------------------------------------------------------------------------------------
//LETTER TO NUMBER----------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Convert a the letter to an int-value that resembles his position in the given alphabet
        /// </summary>
        private int getPos(char c)
        {
            int pos = -1;
            for (int i = 0; i < alphabet.Length; i++)
            {
                if (alphabet[i] == c)
                {
                    pos = i;
                }
            }
            return pos;
        }


//---------------------------------------------------------------------------------------------------------------------------------------
//FIND TOP 13----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Thirteen possible shift values with the highest match count are enough information
        /// </summary>
        private double[] findTopThirteen(double[] ak)
        {
            double[] top = ak;
            int thrownaway = 0;

            for(int match=0; match < probablekorr; match++)
            {
                for(int x=0;x<ak.Length;x++)
                {
                    if(top[x] == match)
                    {
                        top[x] = -1;
                        thrownaway++;
                    }
                    if(thrownaway == (ak.Length)-13)
                    {
                        return top;
                    }

                }
            }
            return top;
        }





//---------------------------------------------------------------------------------------------------------------------------------------

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
