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

namespace Cryptool.Plugins.BigNumber
{
    class BigNumberOperationSettings : ISettings
    {
        #region private variables

        private int operat;

        #endregion
        #region taskpane

        /// <summary>
        /// The checkbox with its options.
        /// 
        /// Based on the option chosen, the icon for this plug-in will also change.
        /// </summary>
        [TaskPane("OperatCaption", "OperatTooltip", null, 1, false, ControlType.ComboBox, new string[] { "x+y","x-y","x*y","x/y","x^y","GCD"})]
        public int Operat
        {
            get { return this.operat; }
            set
            {
                if ((int)value != this.operat)
                {
                    this.operat = (int)value;
                    OnPropertyChanged("Operat");
                    HasChanges = true;

                    changeToCorrectIcon(operat);
                }
            }
        }

        /// <summary>
        /// Changes the plugins icon to the icon fitting to actual selected arithmetic function
        /// </summary>
        /// <param name="operat"></param>
        public void changeToCorrectIcon(int operat)
        {
            switch (operat)
            {
                //x+y
                case 0:
                    ChangePluginIcon(0);
                    break;
                //x-y
                case 1:
                    ChangePluginIcon(1);
                    break;
                //x*y
                case 2:
                    ChangePluginIcon(2);
                    break;
                //x/y
                case 3:
                    ChangePluginIcon(3);
                    break;
                //x^y
                case 4:
                    ChangePluginIcon(4);
                    break;
                //gcd(x,y)
                case 5:
                    ChangePluginIcon(5);
                    break;
            }
        }

        private void ChangePluginIcon(int p)
        {
            OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, p));
        }
        #endregion
        #region ISettings Members

        private bool hasChanges;
        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(p));
        }
        public event StatusChangedEventHandler OnPluginStatusChanged;

        #endregion
    }
}
