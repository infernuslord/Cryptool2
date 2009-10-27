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
    [PluginInfo(false, "Converter", "Converts input to another type", "Converter/DetailedDescription/Description.xaml", "Converter/icons/icon.png", "Converter/icons/tostring.png", "Converter/icons/toint.png", "Converter/icons/toshort.png", "Converter/icons/tobyte.png", "Converter/icons/todouble.png", "Converter/icons/tobig.png", "Converter/icons/tointarray.png", "Converter/icons/tobytearray.png", "Converter/icons/tocryptoolstream.png")]

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
                if (inputOne is bool)
                {
                    switch (this.settings.Converter)
                    {
                        case 0:
                            {
                                Output = inputOne.ToString();
                                ProgressChanged(100, 100);
                                break;
                            }
                        case 1:
                            {
                                if ((bool)inputOne)
                                {
                                    Output = 1;
                                    ProgressChanged(100, 100);
                                    break;
                                }
                                else
                                {
                                    Output = 0;
                                    ProgressChanged(100, 100);
                                    break;
                                }
                            }
                        default:
                            {
                                GuiLogMessage("Could not convert from bool to chosen type: ", NotificationLevel.Error);
                                break;

                            }
                    }
                    return;
                }

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
                                int temp = Convert.ToInt32(inpString);
                                //int temp2 = (int)temp;
                                GuiLogMessage("int erkannt", NotificationLevel.Info);
                                Output = temp;
                                ProgressChanged(100, 100);
                            }
                            catch (Exception e)
                            {
                                GuiLogMessage("Could not convert input to integer: " + e.Message, NotificationLevel.Error);
                            }
                            break;
                        }
                    case 2: //short
                        {

                            try // string -> short Läuft
                            {
                                short temp = Convert.ToInt16(inpString);
                                // short temp2 = (short)temp;
                                GuiLogMessage("short erkannt ", NotificationLevel.Info);
                                Output = temp;
                                ProgressChanged(100, 100);
                            }
                            catch (Exception e)
                            {
                                GuiLogMessage("Could not convert input to short: " + e.Message, NotificationLevel.Error);
                            }
                            break;
                        }
                    case 3: //byte
                        {
                            try // string -> byte Läuft
                            {
                                byte temp = Convert.ToByte(inpString);
                                GuiLogMessage("byte erkannt ", NotificationLevel.Info);
                                Output = temp;
                                ProgressChanged(100, 100);
                            }
                            catch (Exception e)
                            {
                                GuiLogMessage("Could not convert input to byte: " + e.Message, NotificationLevel.Error);
                            }
                            break;
                        }
                    case 4: //double
                        {
                            try // string -> double Läuft
                            {
                                String cleanInputString = DoubleCleanup(inpString);

                                double temp = Convert.ToDouble(cleanInputString);
                                GuiLogMessage("Converting String to double is not safe. Digits may have been cut off  ", NotificationLevel.Warning);
                                Output = temp;
                                ProgressChanged(100, 100);
                            }

                            catch (Exception e)
                            {
                                GuiLogMessage("Could not convert input to double: " + e.Message, NotificationLevel.Error);
                            }
                            break;

                        }
                    case 5: //bigint
                        {
                            try // string -> bigint Läuft
                            {

                                BigInteger temp = BigInteger.parseExpression(inpString);
                                GuiLogMessage("big int erkannt ", NotificationLevel.Info);
                                Output = temp;
                                ProgressChanged(100, 100);
                            }
                            catch (Exception e)
                            {
                                GuiLogMessage("Could not convert input to biginteger: " + e.Message, NotificationLevel.Error);
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
                            if (settings.Numeric)
                            {
                                try // lässt sich als int verstehen?
                                {
                                    int tempint = Convert.ToInt32(inpString);
                                    byte[] temp = new byte[4];
                                    temp = BitConverter.GetBytes(tempint);

                                    int test = BitConverter.ToInt32(temp, 0);
                                    GuiLogMessage("int erkannt " + test.ToString(), NotificationLevel.Info);

                                    Output = temp;

                                    ProgressChanged(100, 100);
                                    break;
                                }
                                catch (Exception e)
                                {

                                }

                                try // lässt sich als bigint verstehen?
                                {
                                    BigInteger tempbigint = new BigInteger(inpString, 10);

                                    int numBits = tempbigint.bitCount();

                                    int numBytes = numBits >> 3;
                                    if ((numBits & 0x7) != 0)
                                        numBytes++;
                                    byte[] temp = new byte[numBytes];
                                    temp = tempbigint.getBytes();

                                    BigInteger test = new BigInteger(temp);
                                    GuiLogMessage("bigint erkannt " + test.ToString(), NotificationLevel.Info);
                                    Output = temp;

                                    ProgressChanged(100, 100);
                                    break;
                                }
                                catch (Exception e)
                                {

                                }
                                try // lässt sich als double verstehen?
                                {   
                                    
                                    
                                    
                                        double tempDouble = Convert.ToDouble(DoubleCleanup(inpString));
                                        byte[] temp = BitConverter.GetBytes(tempDouble);

                                        double test = BitConverter.ToDouble(temp, 0);
                                        GuiLogMessage("Converting String to double is not safe. Digits may have been cut off " + test.ToString(), NotificationLevel.Warning);

                                        Output = temp;

                                        ProgressChanged(100, 100);
                                        break;
                                    
                                  
                                  
                                }
                                catch (Exception e)
                                {

                                }
                                System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
                                Output = enc.GetBytes(inpString);


                                GuiLogMessage("byte[] wiederherstellung " + Output.ToString(), NotificationLevel.Info);
                                ProgressChanged(100, 100);
                                break;
                               
                                
                                
                            }
                            else
                            {
                                switch (settings.Encoding)
                                {
                                    case ConverterSettings.EncodingTypes.Default:
                                        Output = Encoding.Default.GetBytes(inpString.ToCharArray());
                                        break;
                                    case ConverterSettings.EncodingTypes.Unicode:
                                        Output = Encoding.Unicode.GetBytes(inpString.ToCharArray());
                                        break;
                                    case ConverterSettings.EncodingTypes.UTF7:
                                        Output = Encoding.UTF7.GetBytes(inpString.ToCharArray());
                                        break;
                                    case ConverterSettings.EncodingTypes.UTF8:
                                        Output = Encoding.UTF8.GetBytes(inpString.ToCharArray());
                                        break;
                                    case ConverterSettings.EncodingTypes.UTF32:
                                        Output = Encoding.UTF32.GetBytes(inpString.ToCharArray());
                                        break;
                                    case ConverterSettings.EncodingTypes.ASCII:
                                        Output = Encoding.ASCII.GetBytes(inpString.ToCharArray());
                                        break;
                                    case ConverterSettings.EncodingTypes.BigEndianUnicode:
                                        Output = Encoding.BigEndianUnicode.GetBytes(inpString.ToCharArray());
                                        break;
                                    default:
                                        Output = Encoding.Default.GetBytes(inpString.ToCharArray());
                                        break;
                                }
                               


                                //GuiLogMessage("byte[] wiederherstellung " + Output.ToString(), NotificationLevel.Info);
                                ProgressChanged(100, 100);
                                break;
                            }
                            
                        }
                    case 8: //cryptoolstream
                        {
                            GuiLogMessage("redundant", NotificationLevel.Error);
                            break;
                        }
                    default:
                        {
                            GuiLogMessage("kein fall getriggert ", NotificationLevel.Error);
                            break;
                        }
                }

            }
            else
            {
                GuiLogMessage("not yet implemented", NotificationLevel.Error);
            }


        }

        public String DoubleCleanup(String inpString)
        {
            if (this.settings.FormatAmer)
            {
                String temp1 = inpString.Replace(",", "");
                if (!(temp1.IndexOf(".") == temp1.LastIndexOf(".")))
                {
                    String tempXY = temp1.Insert(0, "X");
                    return tempXY;
                }
                if (temp1.Contains(".") && temp1.IndexOf(".") == temp1.LastIndexOf("."))
                {
                    String temp2 = temp1.Replace(".", ",");
                    return temp2;
                }
                else
                {
                    String temp3 = inpString.Replace(".", "");
                    return temp3;
                }
            }
            else
            {
                return inpString;
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
