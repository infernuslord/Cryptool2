/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows;

namespace Cryptool.Plugins.SATAttack
{
    public class SATAttackSettings : ISettings
    {
        #region Private Variables

        private int attackMode;
        private int inputSelection;
        private string openFilename;
        private string inputHashValue;
        private string inputMessage;
        private bool showFileInputSelection = false;
        private bool showPreimageAttackSettings = true;
        private bool showSecondPreimageAttackSettings;
        private bool showOtherAttackSettings;
        private string mainFunctionName;

        #endregion

        #region TaskPane Settings

        #region General Settings

        [TaskPane("AttackModeCaption", "AttackModeTooltip", null, 1, false, ControlType.ComboBox,
            new string[] { "PreimageAttack", "SecondPreimageAttack", "OtherAttack" })]
        public int AttackMode
        {
            get
            {
                return attackMode;
            }
            set
            {
                if (attackMode != value)
                {
                    attackMode = value;
                    OnPropertyChanged("AttackMode");

                    if (attackMode == 0) // Preimage Attack
                    {
                        showPreimageAttackSettings = true;
                        showSecondPreimageAttackSettings = false;
                        showOtherAttackSettings = false;
                    }
                    else if (attackMode == 1) // Second Preimage Attack
                    {
                        showPreimageAttackSettings = false;
                        showSecondPreimageAttackSettings = true;
                        showOtherAttackSettings = false;
                    }
                    else if (attackMode == 2) // Other Attack
                    {
                        showPreimageAttackSettings = false;
                        showSecondPreimageAttackSettings = false;
                        showOtherAttackSettings = true;
                    }

                    UpdateTaskPaneVisibility();
                }
            }
        }

        [TaskPane("InputSelectionCaption", "InputSelectionTooltip", null, 2, false, ControlType.ComboBox,
            new String[] { "InputSelectionTextInput", "InputSelectionFileInput" })]
        public int InputSelection
        {
            get
            {
                return inputSelection;
            }
            set
            {
                if (inputSelection != value)
                {
                    inputSelection = value;
                    OnPropertyChanged("InputSelection");

                    if (inputSelection == 0) // Text Input
                    {
                        showFileInputSelection = false;
                    }
                    else if (inputSelection == 1) // File Input
                    {
                        showFileInputSelection = true;
                    }

                    UpdateTaskPaneVisibility();
                }
            }
        }

        [TaskPane("FileInputCaption", "FileInputTooltip", null, 3, false, ControlType.OpenFileDialog,
            FileExtension = "All Files (*.*)|*.*")]
        public string InputFile
        {
            get { return openFilename; }
            set
            {
                if (value != openFilename)
                {
                    openFilename = value;
                    OnPropertyChanged("InputFile");
                }
            }
        }

        [TaskPane("MainFunctionCaption", "MainFunctionTooltip", null, 4, false, ControlType.TextBox)]
        public string MainFunctionName
        {
            get
            {
                return mainFunctionName;
            }
            set
            {
                if (value != mainFunctionName)
                {
                    mainFunctionName = value;
                    OnPropertyChanged("MainFunctionName");
                }
            }
        }

        #endregion

        #region Preimage Attack Settings

        [TaskPane("InputHashValueCaption", "InputHashValueTooltip", "PreimageAttackOptions", 1, false, ControlType.TextBox)]
        public string InputHashValue
        {
            get
            {
                return inputHashValue;
            }
            set
            {
                if (value != inputHashValue)
                {
                    inputHashValue = value;
                    OnPropertyChanged("InputHashValue");
                }
            }
        }

        #endregion

        #region Second-Preimage Attack Settings

        [TaskPane("InputMessageCaption", "InputMessageTooltip", "SecondPreimageAttackOptions", 1, false, ControlType.TextBox)]
        public string InputMessage
        {
            get
            {
                return inputMessage;
            }
            set
            {
                if (value != inputMessage)
                {
                    inputMessage = value;
                    OnPropertyChanged("InputMessage");
                }
            }
        }

        #endregion

        #endregion

        #region Events

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        private void settingChanged(string setting, Visibility vis)
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
        }

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
                return;

            if (showPreimageAttackSettings)
            {
                settingChanged("InputHashValue", Visibility.Visible);
                settingChanged("InputMessage", Visibility.Collapsed);
            }
            else if (showSecondPreimageAttackSettings)
            {
                settingChanged("InputHashValue", Visibility.Visible);
                settingChanged("InputMessage", Visibility.Visible);
            }
            else if (showOtherAttackSettings)
            {
                settingChanged("InputHashValue", Visibility.Collapsed);
                settingChanged("InputMessage", Visibility.Collapsed);
            }

            if (showFileInputSelection)
            {
                settingChanged("InputFile", Visibility.Visible);
            }
            else if (!showFileInputSelection)
            {
                settingChanged("InputFile", Visibility.Collapsed);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
        }

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
