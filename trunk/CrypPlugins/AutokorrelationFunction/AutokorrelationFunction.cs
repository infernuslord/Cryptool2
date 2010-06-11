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
        private double a;                                               //Found same letter counter
        private double[] ak;                                            // Autokorrelation Values

        #endregion

        #region Data Properties

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

        /// <summary>
        /// HOWTO: You can provide a custom (tabbed) presentation to visualize your algorithm.
        /// Return null if you don't provide one.
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

        /// <summary>
        /// HOWTO: You can provide custom (quickwatch) presentation to visualize your algorithm.
        /// Return null if you don't provide one.
        /// </summary>
        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        /// <summary>
        /// HOWTO: Enter the algorithm you'd like to implement in this method.
        /// </summary>
        public void Execute()
        {
            
            ProgressChanged(0, 1);

//-------------------------------------------------------------------------------------------------------------------------

            cipher = InputCipher;
            cipher = prepareForAnalyse(cipher);

            ak = new double[cipher.Length];
		
		    for(int t=0; t<cipher.Length; t++)
		    {
			    a=0;
			
			    for(int x=0; x<cipher.Length-t;x++)
			    {
				    if(cipher[x] == cipher[x+t])
				    {
					    a++;
				    }
			    }
			
			    ak[t] = a;
		    }
		
		    for(int y=1;y<ak.Length;y++)
		    {
			    if(ak[y] > probablekorr)
			    {
				    probablekorr = ak[y];
				    probablelength = y;
			    }
		    }

            OutputLength = probablelength;
            OnPropertyChanged("OutputLength");		
		   
//-------------------------------------------------------------------------------------------------------------------------

            ProgressChanged(1, 1);
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
