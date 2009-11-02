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
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Windows;

namespace Cryptool.Plugins.RSA
{
    /// <summary>
    /// Settings class for the RSAKeyGenerator plugin
    /// </summary>
    class RSAKeyGeneratorSettings : ISettings
    {

        #region private members

        private int source;
        private String p = "23";
        private String q = "13";
        private String n = "299";
        private String e = "23";
        private String d = "23";
        private string certificateFile;
        private String password = "";
        private bool hasChanges = false;
        
        #endregion

        #region events
        
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region public

        /// <summary>
        /// Getter/Setter for the source of the Key Data
        /// </summary>
        [TaskPane("Source", "Select the source of the key data", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Manual enter primes", "Manual enter keys", "Random generated", "X.509 Certificate" })]
        public int Source
        {
            get { return this.source; }
            set
            {
                if (((int)value) != source) hasChanges = true;
                this.source = (int)value;

                if (TaskPaneAttributeChanged != null)
                    switch (source)
                    {
                        case 0:
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CertificateFile", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CloseFile", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Password", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("P", Visibility.Visible)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Q", Visibility.Visible)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("E", Visibility.Visible)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("D", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("N", Visibility.Collapsed)));
                            break;
                        case 1:
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CertificateFile", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CloseFile", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Password", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("P", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Q", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("E", Visibility.Visible)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("D", Visibility.Visible)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("N", Visibility.Visible)));
                            break;
                        case 2:
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CertificateFile", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CloseFile", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Password", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("P", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Q", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("E", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("D", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("N", Visibility.Collapsed)));
                            break;
                        case 3:
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CertificateFile", Visibility.Visible)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CloseFile", Visibility.Visible)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Password", Visibility.Visible)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("P", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Q", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("E", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("D", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("N", Visibility.Collapsed)));
                            break;
                    }

                OnPropertyChanged("Source");
            }
        }

        /// <summary>
        /// Getter/Setter for prime P
        /// </summary>
        [TaskPane("P", "P", null, 2, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public String P
        {
            get
            {
                return p;
            }
            set
            {
                p = value;
                OnPropertyChanged("P");
            }
        }
        
        /// <summary>
        /// Getter/Setter for the prime Q
        /// </summary>
        [TaskPane("Q", "Q", null, 3, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public String Q
        {
            get
            {
                return q;
            }
            set
            {
                q = value;
                OnPropertyChanged("Q");
            }
        }

        /// <summary>
        /// Getter/Setter for the N
        /// </summary>
        [TaskPane("N", "N", null, 4, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public String N
        {
            get
            {
                return n;
            }
            set
            {
                n = value;
                OnPropertyChanged("N");
            }
        }

        /// <summary>
        /// Getter/Setter for the e
        /// </summary>
        [TaskPane("e", "e", null, 5, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public String E
        {
            get
            {
                return e;
            }
            set
            {
                e = value;
                OnPropertyChanged("E");
            }
        }

        /// <summary>
        /// Getter/Setter for the D
        /// </summary>
        [TaskPane("d", "d", null, 6, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public String D
        {
            get
            {
                return d;
            }
            set
            {
                d = value;
                OnPropertyChanged("D");
            }
        }

        /// <summary>
        /// Getter/Setter for the certificate file
        /// </summary>
        [TaskPane("Open X.509 Certificate", "Select the X.509 certificate you want to open.", null, 5, false, DisplayLevel.Beginner, ControlType.OpenFileDialog, FileExtension = "X.509 certificates (*.cer)|*.cer")]
        public string CertificateFile
        {
            get { return certificateFile; }
            set
            {
                if (value != certificateFile)
                {
                    certificateFile = value;
                    HasChanges = true;
                    OnPropertyChanged("CertificateFile");
                }
            }
        }

        /// <summary>
        /// Getter/Setter for the password of the certificate
        /// </summary>
        [DontSave]
        [TaskPane("Password", "Password", null, 4, false, DisplayLevel.Beginner, ControlType.TextBoxHidden)]
        public String Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
                OnPropertyChanged("Password");
            }
        }

        /// <summary>
        /// Button to "close" the certificate file. That means it will not appear any more in the text field
        /// </summary>
        [TaskPane("Close file", "Close file", null, 6, false, DisplayLevel.Beginner, ControlType.Button)]
        public void CloseFile()
        {
            CertificateFile = null;
        }

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

        #region private

        /// <summary>
        /// The property p changed
        /// </summary>
        /// <param name="p">p</param>
        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #endregion

    }//end RSAKeyGeneratorSettings

}//end Cryptool.Plugins.RSA
