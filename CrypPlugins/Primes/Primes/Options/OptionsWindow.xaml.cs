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
using System.Windows.Shapes;

namespace Primes.Options
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        private static OptionsWindow m_OptionsWindow;
        private static bool m_CanClose;

        public bool CanClose
        {
            get { return m_CanClose; }
            set { m_CanClose = value; }
        }

        private OptionsWindow()
        {
            InitializeComponent();
        }

        public static void ShowOptionsDialog()
        {
            m_OptionsWindow = null;
            m_OptionsWindow = new OptionsWindow();
            m_CanClose = false;
            m_OptionsWindow.ShowDialog();
        }

        public static void ForceClosing()
        {
            if (m_OptionsWindow != null)
            {
                m_OptionsWindow.CanClose = true;
                m_OptionsWindow.Close();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //if (!CanClose) e.Cancel = true;
            base.OnClosing(e);
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnOk)
            {
                CanClose = options.Save();
            }
            else if (sender == btnCancel)
            {
                CanClose = true;
            }
            else if (sender == btnSave)
            {
            }

            Close();
        }
    }
}
