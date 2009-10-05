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
    class RSAKeyGeneratorSettings : ISettings
    {
        public RSAKeyGeneratorSettings()
        {
            Source = 0; //doesn't work
        }

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        private int source;
        [ContextMenu("Source", "Select the source of the Key Data", 1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, new int[] { 1, 2, 3 }, "Manual", "Random", "Certificate")]
        [TaskPane("Source", "Select the source of the Key Data", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Manual", "Random", "Certificate" })]
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
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Password", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("P", Visibility.Visible)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Q", Visibility.Visible)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("E", Visibility.Visible)));
                            break;
                        case 1:
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CertificateFile", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Password", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("P", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Q", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("E", Visibility.Collapsed)));
                            break;
                        case 2:
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CertificateFile", Visibility.Visible)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Password", Visibility.Visible)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("P", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Q", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("E", Visibility.Collapsed)));
                            break;
                    }

                OnPropertyChanged("Source");
            }
        }

        private String p = "11";
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

        private String q = "13";
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

        private String e = "23";
        [TaskPane("E", "E", null, 4, false, DisplayLevel.Beginner, ControlType.TextBox)]
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


        private string certificateFile;
        [TaskPane("Open X.509 Certificate", "Select the X.509 certificate you want to open.", null, 5, false, DisplayLevel.Beginner, ControlType.OpenFileDialog, "X.509 certificates (*.cer)|*.cer")]
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

        private String password = "";
        [TaskPane("Password", "Password", null, 4, false, DisplayLevel.Beginner, ControlType.TextBox)]
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
 
        #region ISettings Members

        private bool hasChanges = false;
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
    }
}
