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
using Cryptool.PluginBase.Analysis;

namespace Cryptool.Plugins.AutokorrelationFunction
{
    [Author("Dennis Nolte", "nolte@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "AutokorrelationFunction", "Calculates the Autokorrelation of a cipher", null, "AutokorrelationFunction/icon.png")]

    public class AutokorrelationFunction : IStatistic
    {
        #region Private Variables

        private readonly AutokorrelationFunctionSettings settings = new AutokorrelationFunctionSettings();

        private String cipher = "";                                     //The cipher to be analysed
        private int probablelength = 0;                                 //estimated keylength
        private double probablekorr = -999999.999999;                   //initialized probable korrelation of the length
        private String alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";         //used alphabet
        private double same;                                            //Found same letter counter
        private double[] ak;                                            // Autokorrelation Values

        #endregion

        #region Data Properties

        /// <summary>
        /// The input for the ciphertext 
        /// </summary>
        [PropertyInfo(Direction.InputData, "Cipher Input", "Enter your cipher here", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
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
        [PropertyInfo(Direction.OutputData, "Keylength Output", "The most probable keylength for the analysed ciphertext", "", DisplayLevel.Beginner)]
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

        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {

//START------------------------------------------------------------------------------------------------------------
//Preparations for the Analyse-------------------------------------------------------------------------------------
           
            ProgressChanged(0, 1);

            cipher = InputCipher;                               //initialising the ciphertext
            cipher = prepareForAnalyse(cipher);                 //and prepare it for the analyse (-> see private methods section)

            ak = new double[cipher.Length];                     //initialise ak[]...there are n possible shifts where n is cipher.length

//-----------------------------------------------------------------------------------------------------------------
//Analyse----------------------------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------------------------------		

            //for each possible shift value...
		    for(int t=0; t<cipher.Length; t++)
		    {
			    same=0;
			
                //...calculate how often the letters match...
			    for(int x=0; x<cipher.Length-t;x++)
			    {
				    if(cipher[x] == cipher[x+t])
				    {
					    same++;
				    }
			    }
			
                //...and save the count for the matches at the shift position
			    ak[t] = same;
		    }
		
            //For all observed shifts...
		    for(int y=1;y<ak.Length;y++)
		    {
                //find the one with the highest match count...
			    if(ak[y] > probablekorr)
			    {
				    probablekorr = ak[y];
                    probablelength = y;                 //...and remember this shift value
			    }
		    }

            OutputLength = probablelength;              //sending the keylength via output
            OnPropertyChanged("OutputLength");		
		   


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
