/*
   Copyright 2010 Sven Rech

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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Cryptool.Core;
using Cryptool.CrypWin.Properties;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.IO;

namespace Cryptool.CrypWin.SettingsTabs
{
    /// <summary>
    /// Interaction logic for NotificationIconSettingsTab.xaml
    /// </summary>
    [Localization("Cryptool.CrypWin.SettingsTabs.Resources.res")]
    [SettingsTab("MainSettings", "/", 1.0)]
    public partial class MainSettingsTab : UserControl
    {
        private bool initialized = false;
        private static string[] supportedCulture = new string[] {"de", "en-US"};

        public MainSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();
            
            List<CultureInfo> cultures = new List<CultureInfo>();

            foreach (var cult in supportedCulture)
            {
                cultures.Add(CultureInfo.CreateSpecificCulture(cult));
            }
            cultures.Sort((x, y) => x.DisplayName.CompareTo(y.DisplayName));

            foreach (var cultureInfo in cultures)
            {
                Culture.Items.Add(cultureInfo);
                if (cultureInfo.TextInfo.CultureName == CultureInfo.CurrentUICulture.TextInfo.CultureName)
                    Culture.SelectedItem = cultureInfo;
            }

            RecentFileListLengthBox.Text = RecentFileList.GetSingleton().ListLength.ToString();

            initialized = true;
        }

        private void Culture_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (initialized)
            {
                var selected = (CultureInfo) Culture.SelectedItem;

                Properties.Settings.Default.Culture = selected.TextInfo.CultureName;
                MainWindow.SaveSettingsSavely();

                RestartLabel.Visibility = Visibility.Visible;
            }
        }

        /***
         * Work around:
         ***/
        private bool bindingInitialized = false;
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!bindingInitialized && this.IsVisible)
            {
                Binding binding = new Binding("AvailableEditors");
                binding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(MainWindow), 1);
                binding.NotifyOnTargetUpdated = true;
                EditorSelection.SetBinding(ItemsControl.ItemsSourceProperty, binding);
                
                bindingInitialized = true;
            }
        }

        private void EditorSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Default.preferredEditor = ((Type)EditorSelection.SelectedItem).FullName;
        }

        private void EditorSelection_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (Settings.Default.preferredEditor != null)
            {
                foreach (var editor in EditorSelection.Items)
                {
                    if (((Type)editor).FullName == Settings.Default.preferredEditor)
                    {
                        EditorSelection.SelectedItem = editor;
                        break;
                    }
                }
            }
        }

        private void RecentFileListLengthBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int l;
            if (int.TryParse(RecentFileListLengthBox.Text, out l))
            {
                RecentFileList.GetSingleton().ChangeListLength(l);
            }
            else
            {
                RecentFileListLengthBox.Text = RecentFileList.GetSingleton().ListLength.ToString();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int processID = Process.GetCurrentProcess().Id;
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            string cryptoolFolderPath = Path.GetDirectoryName(exePath);
            string updaterPath = Path.Combine(DirectoryHelper.BaseDirectory, "CrypUpdater.exe");

            string parameters = "\"dummy\" " + "\"" + cryptoolFolderPath + "\" " + "\"" + exePath + "\" " + "\"" + processID + "\" -JustRestart";
            Process.Start(updaterPath, parameters);
            App.Current.Shutdown();
        }

    }

    [ValueConversion(typeof(bool), typeof(bool))]
    class NegateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
