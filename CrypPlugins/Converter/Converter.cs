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
using System.Numerics;
using System.Text.RegularExpressions;

namespace Cryptool.Plugins.Converter
{
    public enum OutputTypes { StringType = 0, IntType, ShortType, ByteType, DoubleType, BigIntegerType, /*IntArrayType,*/ ByteArrayType, CryptoolStreamType };

    [Author("Raoul Falk, Dennis Nolte", "falk@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Converter.Properties.Resources", false, "PluginCaption", "PluginTooltip", "Converter/DetailedDescription/doc.xml", "Converter/icons/icon.png", "Converter/icons/tostring.png", "Converter/icons/toint.png", "Converter/icons/toshort.png", "Converter/icons/tobyte.png", "Converter/icons/todouble.png", "Converter/icons/tobig.png", "Converter/icons/tointarray.png", "Converter/icons/tobytearray.png", "Converter/icons/tocryptoolstream.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    class Converter : ICrypComponent
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

        [PropertyInfo(Direction.InputData, "InputOneCaption", "InputOneTooltip", "", true, false, QuickWatchFormat.Text, null)]
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



        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip", "", true, false, QuickWatchFormat.Text, null)]
        public object Output
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                if (inputOne == null)
                    return null;

                if (settings.Converter == OutputTypes.ByteArrayType)
                {
                    if (output == null) return null;
                    byte[] temp = (byte[])output;
                    if (settings.ReverseOrder) Array.Reverse(temp);
                    return temp;
                }
                else if (settings.Converter == OutputTypes.CryptoolStreamType)
                {
                    byte[] streamData = null;

                    if (inputOne is byte[])
                        streamData = (byte[])inputOne;
                    else if (inputOne is byte)
                        streamData = new byte[] { (byte)inputOne };
                    else if (inputOne is Boolean)
                        streamData = new byte[] { (byte)(((bool)InputOne) ? 1 : 0) };
                    else if (inputOne is String)
                        streamData = Encoding.UTF8.GetBytes((String)inputOne);
                    else if (inputOne is BigInteger)
                    {
                        streamData = ((BigInteger)inputOne).ToByteArray();
                        //Array.Reverse(streamData); // Display MSB first
                    }

                    if (streamData != null)
                    {
                        if (settings.ReverseOrder) Array.Reverse(streamData);
                        ICryptoolStream cStream = new CStreamWriter(streamData);
                        return cStream;
                    }
                    else
                    {
                        GuiLogMessage("Conversion from " + inputOne.GetType().Name + " to Cryptoolstream is not yet implemented", NotificationLevel.Error);
                        return null;
                    }
                }
                else
                {
                    return output;
                }
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

        private byte[] CStreamReaderToByteArray(CStreamReader stream)
        {
            stream.WaitEof();
            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            stream.ReadFully(buffer);
            return buffer;
        }

        private byte[] ICryptoolStreamToByteArray(ICryptoolStream stream)
        {
            return CStreamReaderToByteArray(stream.CreateReader());
        }

        private BigInteger ByteArrayToBigInteger(byte[] buffer, bool msb=false)
        {
            if (msb)
            {
                byte[] temp = new byte[buffer.Length];
                buffer.CopyTo(temp, 0);
                Array.Reverse(temp);

                return new BigInteger(temp);
            }
            else
            {
                return new BigInteger(buffer);
            }
        }

        private byte[] HexstringToByteArray(String hex)
        {
            if (hex.Length % 2 == 1) hex = "0" + hex;
            byte[] bytes = new byte[hex.Length / 2];

            for (int i = 0; i < hex.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            return bytes;
        }

        private byte[] TryMatchHex(string s)
        {
            byte[] result = null;

            Match match = Regex.Match(s, "[a-fA-F0-9]+");

            if (match.Success)  // includes hex characters?
            {
                s = Regex.Replace(s, "0[xX]", "");  // remove hex specifiers
                s = Regex.Replace(s, "[^a-fA-F0-9]", "");   // remove all non-hex characters
                result = HexstringToByteArray(s);
            }

            return result;
        }

        public void Execute()
        {
            if (InputOne != null)
                GuiLogMessage("Laufe! " + InputOne.ToString(), NotificationLevel.Debug);
            
            #region ConvertFromTypes

            #region ConvertFromICryptoolStream
            if (InputOne is ICryptoolStream)
            {
                switch (this.settings.Converter)
                {
                    case OutputTypes.StringType:
                        {
                            byte[] buffer = ICryptoolStreamToByteArray((ICryptoolStream)InputOne);
                            Output = Encoding.UTF8.GetString(buffer);
                            break;
                        }

                    case OutputTypes.ByteArrayType:
                        {
                            Output = ICryptoolStreamToByteArray((ICryptoolStream)InputOne);
                            break;
                        }

                    case OutputTypes.BigIntegerType:
                        {
                            byte[] buffer = ICryptoolStreamToByteArray((ICryptoolStream)InputOne);
                            Output = ByteArrayToBigInteger(buffer);
                            break;
                        }

                    default:
                        GuiLogMessage("Conversion from ICryptoolStream to the chosen type is not implemented", NotificationLevel.Error);
                        return;
                }

                ProgressChanged(100, 100);
                return;
            }
            #endregion
            #region ConvertFromIntArray
            else if (InputOne is int[])
            {
                GuiLogMessage("Conversion from int[] to the chosen type is not implemented", NotificationLevel.Error);
                return;
            }
            #endregion
            #region ConvertFromByteArray
            else if (InputOne is byte[])
            {
                switch (this.settings.Converter)
                {
                    case OutputTypes.BigIntegerType: // byte[] to BigInteger
                        {
                            byte[] temp = (byte[])InputOne;
                            if (settings.Endianness) Array.Reverse(temp);
                            Output = ByteArrayToBigInteger(temp);                                
                            ProgressChanged(100, 100);
                            return;
                        }
                    case OutputTypes.IntType: // byte[] to int
                        {
                            try
                            {
                                byte[] temp = new byte[4];
                                Array.Copy((byte[])InputOne, temp, 4);
                                if (settings.Endianness) Array.Reverse(temp);
                                Output = BitConverter.ToInt32(temp, 0);
                                ProgressChanged(100, 100);
                                return;
                            }
                            catch (Exception e)
                            {
                                GuiLogMessage("Could not convert byte[] to integer: " + e.Message, NotificationLevel.Error);
                                return;
                            }
                        }
                    case OutputTypes.ShortType: // byte[] to short
                        {
                            try
                            {
                                byte[] temp = new byte[2];
                                Array.Copy((byte[])InputOne, temp, 2);
                                if (settings.Endianness) Array.Reverse(temp);
                                Output = BitConverter.ToInt16(temp, 0);
                                ProgressChanged(100, 100);
                                return;
                            }
                            catch (Exception e)
                            {
                                GuiLogMessage("Could not convert byte[] to short: " + e.Message, NotificationLevel.Error);
                                return;
                            }
                        }
                    case OutputTypes.ByteType: // byte[] to byte
                        {
                            try
                            {
                                Output = ((byte[])InputOne)[0];
                                ProgressChanged(100, 100);
                                return;
                            }
                            catch (Exception e)
                            {
                                GuiLogMessage("Could not convert byte[] to byte: " + e.Message, NotificationLevel.Error);
                                return;
                            }
                        }
                    case OutputTypes.StringType: // byte[] to String
                        {
                            Output = Encoding.UTF8.GetString((byte[])InputOne);
                            ProgressChanged(100, 100);
                            return;
                        }
                    case OutputTypes.ByteArrayType: // byte[] to byte[]
                        {
                            Output = (byte[])InputOne;
                            ProgressChanged(100, 100);
                            return;
                        }
                    //default:
                    //    {
                    //        GuiLogMessage("Could not convert from byte[] to chosen type: ", NotificationLevel.Error);
                    //        break;
                    //    }
                }
            }
            #endregion
            #region ConvertFromBigInteger
            else if (InputOne is BigInteger)
            {
                if (this.settings.Converter == OutputTypes.ByteArrayType)
                {
                    Output = ((BigInteger)InputOne).ToByteArray();

                    ProgressChanged(100, 100);
                    return;
                }
            }
            #endregion
            #region  ConvertFromInt
            else if (InputOne is int)
            {
                if (this.settings.Converter == OutputTypes.ByteArrayType)
                {
                    Output = BitConverter.GetBytes((int)InputOne);

                    ProgressChanged(100, 100);
                    return;
                }
            }
            #endregion
            #region ConvertFromShort
            else if (InputOne is short)
            {
                if (this.settings.Converter == OutputTypes.ByteArrayType)
                {
                    Output = BitConverter.GetBytes((short)InputOne);

                    ProgressChanged(100, 100);
                    return;
                }
            }
            #endregion
            #region ConvertFromByte
            else if (InputOne is byte)
            {
                if (this.settings.Converter == OutputTypes.ByteArrayType)
                {
                    Output = new byte[] { (byte)InputOne };

                    ProgressChanged(100, 100);
                    return;
                }
            }
            #endregion
            #region ConvertFromDouble
            else if (InputOne is Double)
            {
                if (this.settings.Converter == OutputTypes.ByteArrayType)
                {
                    Output = BitConverter.GetBytes((Double)InputOne);

                    ProgressChanged(100, 100);
                    return;
                }
            }
            #endregion
            #region ConvertFromBool
            else if (InputOne is bool)
            {
                switch (this.settings.Converter)
                {
                    case OutputTypes.StringType:
                        {
                            Output = InputOne.ToString();
                            break;
                        }
                    case OutputTypes.IntType:
                        {
                            Output = (int)((bool)InputOne ? 1 : 0);
                            break;
                        }
                    case OutputTypes.ShortType:
                        {
                            Output = (short)((bool)InputOne ? 1 : 0);
                            break;
                        }
                    case OutputTypes.ByteType:
                        {
                            Output = (byte)((bool)InputOne ? 1 : 0);
                            break;
                        }
                    case OutputTypes.ByteArrayType:
                        {
                            Output = new byte[] { (byte)(((bool)InputOne) ? 1 : 0) };
                            break;
                        }
                    case OutputTypes.BigIntegerType:
                        {
                            Output = (BigInteger)((bool)InputOne ? 1 : 0);
                            break;
                        }
                    case OutputTypes.DoubleType:
                        {
                            Output = (Double)((bool)InputOne ? 1 : 0);
                            break;
                        }
                    case OutputTypes.CryptoolStreamType:
                        {
                            Output = new byte[] { (byte)(((bool)InputOne) ? 1 : 0) };
                            break;
                        }
                    default:
                        {
                            GuiLogMessage("Could not convert from bool to chosen type: ", NotificationLevel.Error);
                            return;
                        }
                }

                ProgressChanged(100, 100);
                return;
            }
            #endregion

            #endregion

            // the string representation is used for all upcoming operations
            string inpString = Convert.ToString(InputOne);

            #region ConvertFromString

            switch (this.settings.Converter) // convert to what?
            {
                #region ConvertToString
                case OutputTypes.StringType:
                    {
                        Output = inpString;
                        ProgressChanged(100, 100);
                        break;
                    }
                #endregion
                #region ConvertToInt
                case OutputTypes.IntType:
                    {
                        try // can be read as int from decimal string?
                        {
                            Output = Convert.ToInt32(inpString);
                            ProgressChanged(100, 100);
                            return;
                        }
                        catch (Exception e)
                        {
                        }

                        try // can be read as int from hexadecimal string?
                        {
                            Output = Convert.ToInt32(inpString,16);
                            ProgressChanged(100, 100);
                            return;
                        }
                        catch (Exception e)
                        {
                            GuiLogMessage("Could not convert input to integer: " + e.Message, NotificationLevel.Error);
                        }

                        break;
                    }
                #endregion
                #region ConvertToShort
                case OutputTypes.ShortType:
                    {
                        try // can be read as short from decimal string?
                        {
                            Output = Convert.ToInt16(inpString);
                            ProgressChanged(100, 100);
                            return;
                        }
                        catch (Exception e)
                        {
                        }

                        try // can be read as short from hexadecimal string?
                        {
                            Output = Convert.ToInt16(inpString,16);
                            ProgressChanged(100, 100);
                            return;
                        }
                        catch (Exception e)
                        {
                            GuiLogMessage("Could not convert input to short: " + e.Message, NotificationLevel.Error);
                        }

                        break;
                    }
                #endregion
                #region ConvertToByte
                case OutputTypes.ByteType:
                    {
                        try // can be read as byte from decimal string?
                        {
                            Output = Convert.ToByte(inpString);
                            ProgressChanged(100, 100);
                            return;
                        }
                        catch (Exception e)
                        {
                        }

                        try // can be read as byte hexadecimal string?
                        {
                            Output = Convert.ToByte(inpString,16);
                            ProgressChanged(100, 100);
                            return;
                        }
                        catch (Exception e)
                        {
                            GuiLogMessage("Could not convert input to byte: " + e.Message, NotificationLevel.Error);
                        }

                        break;
                    }
                #endregion
                #region ConvertToDouble
                case OutputTypes.DoubleType:
                    {
                        try // can be read as double?
                        {
                            String cleanInputString = DoubleCleanup(inpString); // apply user settings concerning input format
                            Output = Convert.ToDouble(cleanInputString);
                            ProgressChanged(100, 100);

                            GuiLogMessage("Converting String to double is not safe. Digits may have been cut off  ", NotificationLevel.Warning);
                        }
                        catch (Exception e)
                        {
                            GuiLogMessage("Could not convert input to double: " + e.Message, NotificationLevel.Error);
                        }
                        break;
                    }
                #endregion
                #region ConvertToBigInteger
                case OutputTypes.BigIntegerType:
                    {
                        try // can be read as parseable expression?
                        {
                            Output = BigIntegerHelper.ParseExpression(inpString);
                            ProgressChanged(100, 100);
                            return;
                        }
                        catch (Exception) { }

                        // remove all non-hex characters and parse as hexstring
                        byte[] result = TryMatchHex(inpString);
                        if (result != null)
                        {
                            Output = ByteArrayToBigInteger(result,settings.Endianness);
                            ProgressChanged(100, 100);
                            return;
                        }

                        GuiLogMessage("Could not convert input to BigInteger", NotificationLevel.Error);
                        break;
                    }
                #endregion
                #region ConvertToIntArray
                //case OutputTypes.IntArrayType:
                //    {
                //        GuiLogMessage("Conversion to int[] not yet defined: ", NotificationLevel.Error);
                //        break;
                //    }
                #endregion
                #region ConvertToByteArray
                case OutputTypes.ByteArrayType:
                    {
                        inpString = setText(inpString);

                        if (settings.Numeric) // apply user setting concerning numeric interpretation of input (else input is read as string)
                        {
                            //try // can be read as BigInteger?
                            //{
                            //    Output = BigInteger.Parse(inpString).ToByteArray();
                            //    ProgressChanged(100, 100);
                            //    return;
                            //}
                            //catch(Exception){}

                            try // can be read as parseable expression?
                            {
                                Output = BigIntegerHelper.ParseExpression(inpString).ToByteArray();
                                ProgressChanged(100, 100);
                                return;
                            }
                            catch (Exception) {}

                            try // can be read as Hexstring?
                            {
                                byte[] result = TryMatchHex(inpString);
                                if (result != null)
                                {
                                    Output = result;
                                    ProgressChanged(100, 100);
                                    return;
                                }
                            }
                            catch (Exception) {}

                            try // can be read as double
                            {
                                double tempDouble = Convert.ToDouble(DoubleCleanup(inpString));
                                byte[] temp = BitConverter.GetBytes(tempDouble);
                                Output = temp;

                                double test = BitConverter.ToDouble(temp, 0);
                                GuiLogMessage("Converting String to double is not safe. Digits may have been cut off " + test.ToString(), NotificationLevel.Warning);

                                ProgressChanged(100, 100);
                                return;
                            }
                            catch (Exception) {}

                            System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
                            Output = enc.GetBytes(inpString);

                            ProgressChanged(100, 100);
                            break;
                        }
                        else // numeric interpretation NOT selected:
                        {
                            switch (settings.Encoding) //apply user settings concerning encoding
                            {
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
                                case ConverterSettings.EncodingTypes.Default:
                                default:
                                    Output = Encoding.Default.GetBytes(inpString.ToCharArray());
                                    break;
                            }
                            
                            ProgressChanged(100, 100);
                            break;
                        }
                    }
                #endregion
                #region ConvertToCryptoolStream
                case OutputTypes.CryptoolStreamType:
                    {
                        GuiLogMessage("Conversion from " + inputOne.GetType().Name , NotificationLevel.Info);
                        
                        if (inputOne is byte[] || inputOne is byte || inputOne is BigInteger || inputOne is String)
                        {
                            OnPropertyChanged("Output");
                            ProgressChanged(100, 100);
                        }
                        else
                        {
                            GuiLogMessage("Conversion from " + inputOne.GetType().Name + " to CryptoolStream is not yet implemented", NotificationLevel.Error);
                        }
                        break;
                    }
                #endregion
                #region ConvertToAnythingLeft
                default:
                    {
                        break;
                    }
                #endregion
            }

            #endregion
        }

        private String setText(string temp) //apply user selected presentation format
        {
            if (temp != null)
            {

                switch (settings.Presentation)
                {
                    case ConverterSettings.PresentationFormat.Text:
                        // nothin to do here
                        break;
                    case ConverterSettings.PresentationFormat.Hex:
                        byte[] byteValues = Encoding.UTF8.GetBytes(temp.ToCharArray());
                        temp = BitConverter.ToString(byteValues, 0, byteValues.Length).Replace("-", "");
                        break;
                    case ConverterSettings.PresentationFormat.Base64:
                        temp = Convert.ToBase64String(Encoding.UTF8.GetBytes(temp.ToCharArray()));
                        break;
                    default:
                        break;
                }
                return temp;
            }
            return temp;
        }

        public String DoubleCleanup(String inpString) //apply user selected input format
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
            settings.UpdateTaskPaneVisibility();
            settings.UpdateIcon();
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
