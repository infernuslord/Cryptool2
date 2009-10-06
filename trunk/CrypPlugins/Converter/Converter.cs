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
using Cryptool.PluginBase.IO;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;
using System.Runtime.CompilerServices;
using Cryptool.Plugins.Converter;

namespace Cryptool.Plugins.Converter
{
    [Author("Raoul Falk, Dennis Nolte", "falk@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "Converter", "Converts input to another type", "", "Converter/icons/icon.png")]

    class Converter : IThroughput
    {
        #region private variables

        private ConverterSettings settings = new ConverterSettings();
        private object inputOne;
        private object output;

        #endregion

        #region public interfaces

        public Converter()
        {
           // this.settings = new ConverterSettings();
            this.settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (ConverterSettings)value; }
        }

        private void Converter_LogMessage(string msg, NotificationLevel loglevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(msg, this, loglevel));
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        [PropertyInfo(Direction.InputData, "Input one", "Input one.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public object InputOne
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return inputOne; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (value != inputOne)
                {
                    inputOne = value;
                    OnPropertyChanged("InputOne");
                }
            }
        }

     

        [PropertyInfo(Direction.OutputData, "Output", "Output.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public object Output
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return output;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                this.output = value;
                OnPropertyChanged("Output");
            }
        }

        #endregion

        #region IPlugin members

        public void Dispose()
        {

        }

        public void Execute()
        {
            
                if (!(InputOne is int[] || InputOne is byte[] || InputOne is CryptoolStream))
                {
                    //Wenn der Input ein Bool ist
                    if (InputOne is bool)
                    {
                        switch (this.settings.Converter)
                        {
                            case 0: // bool -> String
                                {
                                    string inpString = Convert.ToString(InputOne);
                                    Output = inpString;
                                    ProgressChanged(100, 100);
                                    break;
                                }
                            case 1: //int
                                {
                                    try // bool -> int Läuft
                                    {
                                        int inpInt;

                                        if ((bool)InputOne)
                                        {
                                            inpInt = 1;
                                        }
                                        else
                                        {
                                            inpInt = 0;
                                        }
                                        GuiLogMessage("cast klappt", NotificationLevel.Info);
                                        Output = inpInt;
                                        ProgressChanged(100, 100);
                                    }
                                    catch (Exception e)
                                    {
                                        GuiLogMessage("The given Inputs are not convertable: " + e.Message, NotificationLevel.Error);
                                    }
                                    break;
                                }
                            default: GuiLogMessage("The given Inputs are not convertable: ", NotificationLevel.Error); break;
                        }
                    }
                    else
                    {
                        string inpString = Convert.ToString(InputOne);

                        

                        switch (this.settings.Converter)
                        {
                            case 0: //String
                                {
                                    Output = inpString;
                                    ProgressChanged(100, 100);
                                    break;
                                }
                            case 1: //int
                                {
                                    try // string -> int Läuft
                                    {
                                        double temp = Convert.ToDouble(inpString);
                                        int temp2 = (int)temp;
                                        GuiLogMessage("cast klappt", NotificationLevel.Info);
                                        Output = temp2;
                                        ProgressChanged(100, 100);
                                    }
                                    catch (Exception e)
                                    {
                                        GuiLogMessage("The given Inputs are not convertable: " + e.Message, NotificationLevel.Error);
                                    }
                                    break;
                                }
                            case 2: //short
                                {

                                    try // string -> short Läuft
                                    {
                                        int temp = Convert.ToInt32(inpString);
                                        short temp2 = (short)temp;
                                        GuiLogMessage("cast klappt", NotificationLevel.Info);
                                        Output = temp2;
                                        ProgressChanged(100, 100);
                                    }
                                    catch (Exception e)
                                    {
                                        GuiLogMessage("The given Inputs are not convertable: " + e.Message, NotificationLevel.Error);
                                    }
                                    break;
                                }
                            case 3: //byte
                                {
                                    try // string -> byte Läuft
                                    {
                                        byte temp = Convert.ToByte(inpString);
                                        GuiLogMessage("cast klappt", NotificationLevel.Info);
                                        Output = temp;
                                        ProgressChanged(100, 100);
                                    }
                                    catch (Exception e)
                                    {
                                        GuiLogMessage("The given Inputs are not convertable: " + e.Message, NotificationLevel.Error);
                                    }
                                    break;
                                }
                            case 4: //double
                                {
                                    try // string -> double Läuft
                                    {
                                        if (inpString.Contains(".") && (inpString.IndexOf(".") == inpString.LastIndexOf(".")) && !inpString.Contains(","))
                                        {
                                            string stringtemp=inpString.Replace(".", ",");  
                                            double temp = Convert.ToDouble(stringtemp);
                                            
                                            Output = temp;
                                            ProgressChanged(100, 100);
                                        }
                                        else
                                        {
                                            double temp = Convert.ToDouble(inpString);
                                           
                                            Output = temp;
                                            ProgressChanged(100, 100);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        GuiLogMessage("The given Inputs are not convertable: " + e.Message, NotificationLevel.Error);
                                    }
                                    break;

                                }
                            case 5: //bigint
                                {
                                    try // string -> bigint Läuft
                                    {
                                        string temp1 = (string)inpString;
                                        BigInteger temp = BigInteger.parseExpression(temp1);
                                        GuiLogMessage("cast klappt", NotificationLevel.Info);
                                        Output = temp;
                                        ProgressChanged(100, 100);
                                    }
                                    catch (Exception e)
                                    {
                                        GuiLogMessage("The given Inputs are not convertable: " + e.Message, NotificationLevel.Error);
                                    }
                                    break;
                                }
                            case 6: // int[]
                                {
                                    GuiLogMessage("Conversion from String to int[] not defined: ", NotificationLevel.Error);
                                    break;
                                }
                            case 7: // byte[]
                                {
                                    GuiLogMessage("Conversion from String to byte[] not defined: ", NotificationLevel.Error);
                                    break;
                                }
                            case 8: //cryptoolstream
                                {
                                    GuiLogMessage("redundant", NotificationLevel.Error);
                                    break;
                                }

                        }
                    }
                }
                else
                {
                    GuiLogMessage("array and cryptoolstream input not yet implemented", NotificationLevel.Error);
                }

           
        }


        public void Initialize()
        {

        }

        public void Pause()
        {

        }

        public void PostExecution()
        {

        }

        public void PreExecution()
        {

        }

        public void Stop()
        {

        }

        #endregion

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region event handling

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(this, args);
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion
    }
}
