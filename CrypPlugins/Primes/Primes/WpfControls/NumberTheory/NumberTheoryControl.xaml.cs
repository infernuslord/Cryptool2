/*
   Copyright 2008 Timo Eckhardt, University of Siegen

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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Primes.WpfControls.NumberTheory
{
    /// <summary>
    /// Interaction logic for NumberTheoryControl.xaml
    /// </summary>
    public partial class NumberTheoryControl : UserControl, IPrimeMethodDivision
    {
        public NumberTheoryControl()
        {
            InitializeComponent();
        }

        #region IPrimeMethodDivision Members

        public void Dispose()
        {
            power.CleanUp();
        }

        public void Init()
        {
        }

        public void SetTab(int i)
        {
            if (i >= 0 && i < tbctrl.Items.Count)
            {
                tbctrl.SelectedIndex = i;
            }
        }

        #endregion

        private void TabItem_HelpButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender == tabItemNTFunctions)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.PrimitivRoot_PrimitivRoot);
            }
            else if (sender == tabItemPower)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.PrimitivRoot_PrimitivRoot);
            }
            else if (sender == tabItemPRoots)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.PrimitivRoot_PrimitivRoot);
            }
        }
    }
}
