/*
   Copyright 2008 Thomas Schmid, University of Siegen

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
using Contains.Aho_Corasick;
using System.Threading;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Cryptool.PluginBase;

namespace Contains
{
  /// <summary>
  /// Interaction logic for ContainsPresentation.xaml
  /// </summary>
  [Cryptool.PluginBase.Attributes.Localization("Contains.Properties.Resources")]
  public partial class ContainsPresentation : UserControl
  {
    // private ObservableCollection<StringSearchResult> collection = new ObservableCollection<StringSearchResult>();
    private bool canResetListView = false;

    public ContainsPresentation()
    {
      InitializeComponent();
      this.Width = double.NaN;
      this.Height = double.NaN;
      // listView.ItemsSource = collection;
    }

    public void SetHits(int value)
    {
      Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
      {
          hits.Content = typeof(Contains).GetPluginStringResource("LabelHits1") + value;
        //hits.Content = "Hits: " + value;
        //if (canResetListView)
        //{
        //  listView.ItemsSource = null;
        //  canResetListView = false;
        //}
      }, value);
    }

    public int TargetHits { get; set; }

    // [MethodImpl(MethodImplOptions.Synchronized)]
    public void SetData(StringSearchResult[] arr)
    {
      Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
      {
        if (arr != null)
        {          
          //collection.Clear();
          //foreach (StringSearchResult item in arr)
          //{
          //  collection.Add(item);
          //}

          //hits.Content = "Hits: " + arr.Length + " (target: "+ TargetHits.ToString() + ")";
            hits.Content = typeof(Contains).GetPluginStringResource("LabelHits1") + arr.Length + " (" + typeof(Contains).GetPluginStringResource("LabelTarget") + " " + TargetHits.ToString() + ")";
          StringBuilder sb = new StringBuilder();
          foreach (StringSearchResult item in arr)
	        {
            sb.Append("- [" + item.Keyword + "], [" + item.Index + "]\n");
          }
          textBox.Text = sb.ToString();
          //listView.ItemsSource = arr;
          //canResetListView = true;
          //foreach (GridViewColumn gvc in ((GridView)listView.View).Columns)
          //{
          //  gvc.Width = gvc.ActualWidth;
          //  gvc.Width = Double.NaN;
          //}
        }
        else
            hits.Content = typeof(Contains).GetPluginStringResource("LabelHits");
      }, arr);
    }
  }
}
