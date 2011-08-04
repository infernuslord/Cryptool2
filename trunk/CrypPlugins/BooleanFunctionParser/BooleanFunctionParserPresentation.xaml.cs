/*
   Copyright 2009 Sören Rinne, Ruhr-Universität Bochum, Germany

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
using System.Windows.Threading;
using System.Threading;

namespace Cryptool.BooleanFunctionParser
{
  /// <summary>
    /// Interaction logic for BooleanFunctionParserPresentation.xaml
  /// </summary>
  [Cryptool.PluginBase.Attributes.Localization("Cryptool.BooleanFunctionParser.Properties.Resources")]
  public partial class BooleanFunctionParserPresentation : UserControl
  {
    public BooleanFunctionParserPresentation()
    {
      InitializeComponent();
      Height = double.NaN;
      Width = double.NaN;
    }

      public string getTextBoxInputFunctionText() {
          return textBoxInputFunction.Text;
      }

      public void setMemoryBit(string value)
      {
          Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
          {
              labelMemoryBit.Content = "Memory Bit: " + value;
          }, null);
      }

      public void SwitchCubeView(bool switchView) {
          GridLengthConverter myGridLengthConverter = new GridLengthConverter();
          if (!switchView)
          {
              GridLength col0Length = (GridLength)myGridLengthConverter.ConvertFromString("1*");
              GridLength col1Length = (GridLength)myGridLengthConverter.ConvertFromString("0");
              if (col0 != null)
                  Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                  {
                      col0.Width = col0Length;
                  }, null);

              if (col1 != null)
                  Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                  {
                      col1.Width = col1Length;
                  }, null);
              if (colm != null)
                  Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                  {
                      colm.Width = col1Length;
                  }, null);
          }
          else
          {
              GridLength colLength = (GridLength)myGridLengthConverter.ConvertFromString("1*");
              //GridLength colmLength = (GridLength)myGridLengthConverter.ConvertFromString("5");
              GridLength col0Length = (GridLength)myGridLengthConverter.ConvertFromString("0");
              if (col0 != null)
                  Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                  {
                      col0.Width = col0Length;
                  }, null);

              if (col1 != null)
                  Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                  {
                      col1.Width = colLength;
                  }, null);
              /*if (colm != null)
                  Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                  {
                      colm.Width = colmLength;
                  }, null);
               */
          }
      }

  }
}
