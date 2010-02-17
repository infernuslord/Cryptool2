/*
   Copyright 2009 Matthäus Wander, Universität Duisburg-Essen

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
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace Cryptool.Plugin.BinaryConstant
{
    public class BinaryConstantSettings : ISettings
    {
        [TaskPane("Load from file", "Assign constant from file content", null, 2, false, DisplayLevel.Beginner, ControlType.Button)]
        public void setConstant()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All files (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                String fileName = dialog.FileName;
                if (fileName != null && File.Exists(fileName))
                {
                    try
                    {
                        ConstantValue = File.ReadAllBytes(fileName);
                        MessageBox.Show("Assigned file contents to constant.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Could not initialize from file:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private byte[] constantValue;
        public byte[] ConstantValue
        {
            get { return constantValue; }
            set { constantValue = value; OnPropertyChanged("ConstantValue"); HasChanges = true; }
        }

        public bool HasChanges { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
