/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using System.ComponentModel;
using Cryptool.PluginBase;
using System.Collections.ObjectModel;
using System.Windows;
using Cryptool.PluginBase.Miscellaneous;

using System.Windows.Forms;

namespace Cryptool.Plugins.CostFunction
{
    class CostFunctionSettings : ISettings
    {
        #region private variables
        private bool hasChanges = false;
        private int functionType;
        private String bytesToUse = "256";
        private int bytesToUseInteger = 256;
        #endregion

        public void Initialize()
        {
            UpdateTaskPaneVisibility();
        }

        [TaskPane( "FunctionTypeCaption", "FunctionTypeTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Index of coincidence", "Entropy", "Bigrams: log 2", "Bigrams: Sinkov", "Bigrams: Percentaged", "Regular Expression", "Weighted Bigrams/Trigrams"})]
        public int FunctionType
        {
            get { return this.functionType; }
            set
            {
                this.functionType = (int)value;
                UpdateTaskPaneVisibility();
                OnPropertyChanged("FunctionType");
            }
        }

        [TaskPane( "BytesToUseCaption", "BytesToUseTooltip", null, 4, false, ControlType.TextBox)]
        public String BytesToUse
        {
            get
            {
                return bytesToUse;
            }
            set
            {
                var old = bytesToUseInteger;
                if (!int.TryParse(value, out bytesToUseInteger))
                {
                    bytesToUseInteger = old;
                }
                else
                    bytesToUse = value;
                
                OnPropertyChanged("BytesToUse");
            }
        }

        public int BytesToUseInteger
        {
            get { return bytesToUseInteger; }
        }


        public string customFilePath;
        public int statisticscorpus = 0;
        [TaskPane( "StatisticsCorpusCaption", "StatisticsCorpusTooltip", null, 7, false, ControlType.ComboBox, new string[] { "Text corpus (DE)", "Text corpus (EN)", "Custom (...)" })]
        public int StatisticsCorpus
        {
            get
            {
              return statisticscorpus;
            }
            set
            {
                statisticscorpus = value;
                if (statisticscorpus == 2)
                {
                    

                    OpenFileDialog openCorpusFile = new OpenFileDialog();
                    openCorpusFile.Title = "Select text corpus file";
                    openCorpusFile.CheckFileExists = true;
                    openCorpusFile.CheckPathExists = true;
                    openCorpusFile.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                    if (openCorpusFile.ShowDialog() == DialogResult.OK)
                    {
                        customFilePath = openCorpusFile.FileName;
                    }
                    else
                    {
                        statisticscorpus = 0; // Fall back to default
                    }
                }
                UpdateTaskPaneVisibility();
                OnPropertyChanged("StatisticsCorpus");
            }
        }
        public int entropyselect;
        [TaskPane( "entropyCaption", "entropyTooltip", null, 9, false, ControlType.ComboBox, new string[] { "External (NativeCryptography, default)", "Internal" })]
        public int entropy
        {
            get
            {
                return entropyselect;
            }

            set
            {
                entropyselect = value;
                OnPropertyChanged("entropy");
            }
        }

        public string customfwtpath;
        public int fwt = 0; //fwt = fitness weight table
        [TaskPane( "weighttableCaption", "weighttableTooltip", null, 8, false, ControlType.ComboBox, new string[] { "Matthews", "Toemeh-Arumugam", "Custom Weight table (*.csv)" })]
        public int weighttable
        {
            get
            {
                return fwt;
            }
            set
            {
                fwt = value;
                if (fwt == 2)
                {
                    OpenFileDialog openfwt = new OpenFileDialog();
                    openfwt.Title = "Select fitness weight table";
                    openfwt.CheckFileExists = true;
                    openfwt.CheckPathExists = true;
                    openfwt.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                    if (openfwt.ShowDialog() == DialogResult.OK)
                    {
                        customfwtpath = openfwt.FileName;
                    }
                    else
                    {
                        fwt = 0; // Fall back to default
                    }
                }
                OnPropertyChanged("weighttable");
            }
        }
        

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
            {
                return;
            }

            if (functionType.Equals(5))
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BytesToUse", Visibility.Visible)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RegEx", Visibility.Visible)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RegExHex", Visibility.Visible)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CaseInsensitiv", Visibility.Visible)));
            }
            else
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BytesToUse", Visibility.Visible)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RegEx", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RegExHex", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CaseInsensitiv", Visibility.Collapsed)));
            }

            if (functionType.Equals(4) || functionType.Equals(2) || functionType.Equals(3) || functionType.Equals(6))
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("StatisticsCorpus", Visibility.Visible)));
                
            }
            else
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("StatisticsCorpus", Visibility.Collapsed)));
                
                
            }
            if (functionType.Equals(6))
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("weighttable", Visibility.Visible)));

            }
            else
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("weighttable", Visibility.Collapsed)));

            }
            if (functionType.Equals(1))
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("entropy", Visibility.Visible)));
            }
            else
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("entropy", Visibility.Collapsed)));
            }
        }


        private string regExText;
        [TaskPane( "RegExCaption", "RegExTooltip", null, 5, false, ControlType.TextBox)]
        public String RegEx
        {
            get
            {
                return regExText;
            }
            set
            {
                regExText = value;
                OnPropertyChanged("RegEx");

                regExHex = convertTextToHexString(regExText);
                OnPropertyChanged("RegExHex");
            }
        }

        private static string convertTextToHexString(string text)
        {
            if (text == null)
                return null;

            StringBuilder sb = new StringBuilder();
            foreach(byte b in Encoding.ASCII.GetBytes(text))
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        private bool caseInsensitiv;
        [TaskPane( "CaseInsensitivCaption", "CaseInsensitivTooltip", null, 6, false, ControlType.CheckBox)]
        public bool CaseInsensitiv
        {
            get { return caseInsensitiv; }
            set
            {
                if (value != caseInsensitiv)
                {
                    caseInsensitiv = value;
                    hasChanges = true;
                    OnPropertyChanged("CaseInsensitiv");
                }
            }
        }

        private string regExHex;
        [TaskPane( "RegExHexCaption", "RegExHexTooltip", null, 7, false, ControlType.TextBox)]
        public String RegExHex
        {
            get
            {
                return regExHex;
            }
            set
            {
                regExHex = value;
                OnPropertyChanged("RegExHex");

                regExText = convertHexStringToText(regExHex);
                OnPropertyChanged("RegEx");
            }
        }

        private static string convertHexStringToText(string hexString)
        {
            if (hexString == null)
                return null;

            StringBuilder cleanHexString = new StringBuilder();

            //cleanup the input
            foreach (char c in hexString)
            {
                if (Uri.IsHexDigit(c))
                    cleanHexString.Append(c);
            }

            int numberChars = cleanHexString.Length % 2 == 0 ? cleanHexString.Length : cleanHexString.Length - 1;

            byte[] bytes = new byte[numberChars / 2];

            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(cleanHexString.ToString().Substring(i, 2), 16);
            }
            return Encoding.ASCII.GetString(bytes);
        }

        #region ISettings Members

        public bool HasChanges
        {
            get
            {
                return hasChanges;
            }
            set
            {
                hasChanges = value;
            }
        }



        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #endregion

        #region testing
        public void changeFunctionType(int type)
        {
                      this.functionType = type;
                UpdateTaskPaneVisibility();
                OnPropertyChanged("FunctionType");
            
        
        }
#endregion
    }
    
    
}
