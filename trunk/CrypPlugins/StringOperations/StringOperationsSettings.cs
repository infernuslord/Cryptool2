/*                              
   Copyright 2011 Nils Kopal, Uni Duisburg-Essen

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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Cryptool.PluginBase;
using System;

namespace StringOperations
{
    class StringOperationsSettings : ISettings
    {
        private StringOperationType _stringOperationType;
        private int _blockSize = 5;
        private int _order = 0;
        private readonly Dictionary<StringOperationType, List<string>> _operationVisibility = new Dictionary<StringOperationType, List<string>>();
        private readonly List<string> _operationList = new List<string>();  

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public StringOperationsSettings()
        {
            foreach (StringOperationType name in Enum.GetValues(typeof(StringOperationType)))
            {
                _operationVisibility[name] = new List<string>();
            }
            _operationList.Add("Blocksize");
            _operationList.Add("Order");
            _operationVisibility[StringOperationType.Block].Add("Blocksize");
            _operationVisibility[StringOperationType.Sort].Add("Order");
            UpdateTaskPaneVisibility();
        }

        [TaskPane("OperationCaption", "OperationCaptionToolTip", null, 1, false, ControlType.ComboBox,
            new[] { "OperationList1", "OperationList2", "OperationList3", "OperationList4", "OperationList5", "OperationList6", "OperationList7", "OperationList8", "OperationList9", "OperationList10", "OperationList11", "OperationList12", "OperationList13", "OperationList14", "OperationList15", "OperationList16" })]
        public StringOperationType Operation
        {
            get
            {
                return _stringOperationType;
            }
            set
            {
                if (_stringOperationType != value)
                {
                    _stringOperationType = value;
                    UpdateTaskPaneVisibility();
                    OnPropertyChanged("Operation");
                }
            }
        }
        
        [TaskPane("BlocksizeCaption", "BlocksizeTooltip", null, 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int Blocksize
        {
            get
            {
                return _blockSize;
            }
            set
            {
                _blockSize = value;
                OnPropertyChanged("Blocksize");
            }
        }

        [TaskPane("OrderCaption", "OrderTooltip", null, 4, false, ControlType.ComboBox, new[] { "Ascending", "Descending" })]
        public int Order
        {
            get
            {
                return _order;
            }
            set
            {
                _order = value;
                OnPropertyChanged("Order");
            }
        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
                return;

            foreach (var tpac in _operationList.Select(operation => new TaskPaneAttribteContainer(operation, (_operationVisibility[Operation].Contains(operation)) ? Visibility.Visible : Visibility.Collapsed)))
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(tpac));
            }
        }

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
    }

    enum StringOperationType
    {
        Concatenate,
        Substring,
        ToLowercase,
        ToUppercase,
        Length,
        CompareTo,
        Trim,
        IndexOf,
        Equals,
        Replace,
        RegexReplace,
        Split,
        Block,
        Reverse,
        Sort,
        Distinct
    }
}
