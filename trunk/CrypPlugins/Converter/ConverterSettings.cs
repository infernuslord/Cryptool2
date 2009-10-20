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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using Cryptool.PluginBase;

namespace Cryptool.Plugins.Converter
{
    public class ConverterSettings : ISettings
    {
        #region private variables
        private int converter = 9; // 0 = String, 1 = int, 2 = short, 3 = byte, 4 = double, 5 = bigInteger, 6= Int[] , 7=Byte[], 8=CryptoolStream, 9 = default
        private bool hasChanges;
        private bool numeric = false;
        private bool formatAmer = false;
        #endregion

        #region taskpane
     //   [ContextMenu("Numeric Interpretation", "Choose whether inputs are treated as numeric values if possible", 1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "numeric", "non numeric")]
        [TaskPane("Converter", "Choose the output type", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "string", "int", "short", "byte", "double", "BigInteger", "int[]", "byte[]","Cryptoolstream" })]
        
      
        public int Converter
        {
            get { return this.converter; }
            set
            {
                if (value != this.converter)
                {
                    this.converter = value;
                    if (TaskPaneAttributeChanged != null)
                    {
                        switch (Converter)
                        {
                            case 0:
                                {
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Numeric", Visibility.Collapsed)));
                                    //TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Format", Visibility.Collapsed)));
                                    break;
                                }
                            case 1:
                                {
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Numeric", Visibility.Collapsed)));
                                    //  TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Format", Visibility.Collapsed)));
                                    break;
                                }
                            case 2:
                                {
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Numeric", Visibility.Collapsed)));
                                    //TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Format", Visibility.Collapsed)));
                                    break;
                                }
                            case 3:
                                {
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Numeric", Visibility.Collapsed)));
                                    //TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Format", Visibility.Collapsed)));
                                    break;
                                }
                            case 4:
                                {
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Numeric", Visibility.Collapsed)));
                                    // TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Format", Visibility.Collapsed)));
                                    break;
                                }
                            case 5:
                                {
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Numeric", Visibility.Collapsed)));
                                    //TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Format", Visibility.Collapsed)));
                                    break;
                                }
                            case 6:
                                {
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Numeric", Visibility.Collapsed)));
                                    //TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Format", Visibility.Collapsed)));
                                    break;
                                }
                            case 7:
                                {
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Numeric", Visibility.Visible)));
                                    //TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Format", Visibility.Collapsed)));
                                    break;
                                }
                            case 8:
                                {
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Numeric", Visibility.Collapsed)));
                                    //TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Format", Visibility.Collapsed)));
                                    break;
                                }
                        }
                    }
                    OnPropertyChanged("Converter");
                    HasChanges = true;

                   ChangePluginIcon(converter+1);
                }
            }
        }
        [TaskPane("Numeric", "Choose whether inputs are interpreted as numeric values if possible", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "no", "yes" })]
        public bool Numeric
        {
            get { return this.numeric; }
            set
            {
                if (value != this.numeric)
                {
                    this.numeric = value;
                    OnPropertyChanged("Numeric");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("Format", "Choose whether double values are recognized via german or american syntax. German: \"123.345.34,34\" American: \"123,345,34.34 ", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "german", "american" })]
        public bool FormatAmer
        {
            get { return this.formatAmer; }
            set
            {
                if (value != this.formatAmer)
                {
                    this.formatAmer = value;
                    OnPropertyChanged("Format");
                    HasChanges = true;
                }
            }
        }
        #endregion

        #region ISettings Member

        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }

        }

        #endregion

        #region INotifyPropertyChanged Member

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {

        }
        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIcon(int iconIndex)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, iconIndex));
        }
        #endregion
    }
}
