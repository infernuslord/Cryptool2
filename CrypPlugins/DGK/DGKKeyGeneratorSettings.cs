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


namespace Cryptool.Plugins.DGK
{
    /// <summary>
    /// Settings class for the DGKKeyGenerator plugin
    /// </summary>
    class DGKKeyGeneratorSettings : ISettings
    {

        #region private members

        private int bitSizeK = 200;
        private int bitSizeT = 80;
        private int limitL = 40;
        private bool hasChanges = false;
        
        #endregion

        #region events
        
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region public

        /// <summary>
        /// Getter/Setter for the parameter k
        /// </summary>
        [TaskPane("BitSizeKCaption", "BitSizeKTooltip", null, 1, false, ControlType.TextBox, ValidationType.RegEx, "[0-9][0-9]*")]
        public int BitSizeK
        {
            get { return this.bitSizeK; }
            set
            {
                if (value != this.bitSizeK)
                {
                    this.bitSizeK = value;
                    OnPropertyChanged("BitSizeK");
                    HasChanges = true;
                }
            }
        }

        /// <summary>
        /// Getter/Setter for the parameter t
        /// </summary>
        [TaskPane("BitSizeTCaption", "BitSizeTTooltip", null, 1, false, ControlType.TextBox, ValidationType.RegEx, "[0-9][0-9]*")]
        public int BitSizeT
        {
            get { return this.bitSizeT; }
            set
            {
                if (value != this.bitSizeT)
                {
                    this.bitSizeT = value;
                    OnPropertyChanged("BitSizeT");
                    HasChanges = true;
                }
            }
        }

        /// <summary>
        /// Getter/Setter for the parameter l
        /// </summary>
        [TaskPane("BitSizeLCaption", "BitSizeLTooltip", null, 1, false, ControlType.TextBox, ValidationType.RegEx, "[0-9][0-9]*")]
        public int LimitL
        {
            get { return this.limitL; }
            set
            {
                if (value != this.limitL)
                {
                    this.limitL = value;
                    OnPropertyChanged("BitSizeL");
                    HasChanges = true;
                }
            }
        }

        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
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

    }//end DGKKeyGeneratorSettings

}//end Cryptool.Plugins.DGK
