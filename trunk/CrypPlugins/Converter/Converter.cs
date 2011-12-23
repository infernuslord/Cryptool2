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
    public enum OutputTypes { StringType = 0, IntType, ShortType, ByteType, DoubleType, BigIntegerType, ByteArrayType, CryptoolStreamType };
    
    [Author("Raoul Falk, Dennis Nolte", "falk@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Converter.Properties.Resources", "PluginCaption", "PluginTooltip", "Converter/DetailedDescription/doc.xml", "Converter/icons/icon.png", "Converter/icons/tostring.png", "Converter/icons/toint.png", "Converter/icons/toshort.png", "Converter/icons/tobyte.png", "Converter/icons/todouble.png", "Converter/icons/tobig.png", /*"Converter/icons/tointarray.png",*/ "Converter/icons/tobytearray.png", "Converter/icons/tocryptoolstream.png")]
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
            this.settings.PropertyChanged += settings_PropertyChanged;
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

        [PropertyInfo(Direction.InputData, "InputOneCaption", "InputOneTooltip", true)]
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



        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip", true)]
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
                    if (!settings.ReverseOrder) return (byte[])output;
                    byte[] temp = new byte[((byte[])output).Length];
                    Buffer.BlockCopy((byte[])output, 0, temp, 0, ((byte[])output).Length);
                    Array.Reverse(temp);
                    return temp;
                }
                else if (settings.Converter == OutputTypes.CryptoolStreamType)
                {
                    byte[] streamData = null;

                    if (inputOne is ICryptoolStream)
                    {
                        if (!settings.ReverseOrder) return (ICryptoolStream)inputOne;
                        streamData = ICryptoolStreamToByteArray((ICryptoolStream)inputOne);
                    } 
                    else if (inputOne is byte[])
                        streamData = (byte[])inputOne;
                    else if (inputOne is byte)
                        streamData = new byte[] { (byte)inputOne };
                    else if (inputOne is Boolean)
                        streamData = new byte[] { (byte)(((bool)InputOne) ? 1 : 0) };
                    else if (inputOne is String)
                        streamData = GetBytesForEncoding((String)inputOne, settings.OutputEncoding);
                    else if (inputOne is BigInteger)
                        streamData = ((BigInteger)inputOne).ToByteArray();

                    if (streamData != null)
                    {
                        if (!settings.ReverseOrder) return new CStreamWriter(streamData);
                        byte[] temp = new byte[streamData.Length];
                        Buffer.BlockCopy(streamData, 0, temp, 0, streamData.Length);
                        Array.Reverse(temp);
                        return new CStreamWriter(temp);
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

        private Type GetType(OutputTypes t)
        {
            switch (t)
            {
                case OutputTypes.StringType: return typeof(String);
                case OutputTypes.IntType: return typeof(int);
                case OutputTypes.ShortType: return typeof(short);
                case OutputTypes.ByteType: return typeof(byte);
                case OutputTypes.DoubleType: return typeof(double);
                case OutputTypes.BigIntegerType: return typeof(BigInteger);
                //case OutputTypes.IntArrayType: return typeof(int[]);
                case OutputTypes.ByteArrayType: return typeof(byte[]);
                case OutputTypes.CryptoolStreamType: return typeof(ICryptoolStream);
                default: return null;
            }
        }

        private byte[] GetBytesForEncoding(string s, ConverterSettings.EncodingTypes encoding)
        {
            if (s == null) return null;

            switch (encoding)
            {
                case ConverterSettings.EncodingTypes.UTF16:
                    return Encoding.Unicode.GetBytes(s);

                case ConverterSettings.EncodingTypes.UTF7:
                    return Encoding.UTF7.GetBytes(s);

                case ConverterSettings.EncodingTypes.UTF8:
                    return Encoding.UTF8.GetBytes(s);

                case ConverterSettings.EncodingTypes.UTF32:
                    return Encoding.UTF32.GetBytes(s);

                case ConverterSettings.EncodingTypes.ASCII:
                    return Encoding.ASCII.GetBytes(s);
                    
                case ConverterSettings.EncodingTypes.ISO8859_15:
                    return Encoding.GetEncoding("iso-8859-15").GetBytes(s);

                case ConverterSettings.EncodingTypes.Windows1252:
                    return Encoding.GetEncoding(1252).GetBytes(s);

                default:    // should never be reached
                    return Encoding.Default.GetBytes(s);
            }
        }

        private string GetStringForEncoding(byte[] bytes, ConverterSettings.EncodingTypes encoding)
        {
            if (bytes == null) return null;

            switch (encoding)
            {
                case ConverterSettings.EncodingTypes.UTF16:
                    return Encoding.Unicode.GetString(bytes);

                case ConverterSettings.EncodingTypes.UTF7:
                    return Encoding.UTF7.GetString(bytes);

                case ConverterSettings.EncodingTypes.UTF8:
                    return Encoding.UTF8.GetString(bytes);

                case ConverterSettings.EncodingTypes.UTF32:
                    return Encoding.UTF32.GetString(bytes);

                case ConverterSettings.EncodingTypes.ASCII:
                    return Encoding.ASCII.GetString(bytes);

                case ConverterSettings.EncodingTypes.ISO8859_15:
                    return Encoding.GetEncoding("iso-8859-15").GetString(bytes);

                case ConverterSettings.EncodingTypes.Windows1252:
                    return Encoding.GetEncoding(1252).GetString(bytes);

                default:
                    return Encoding.Default.GetString(bytes);
            }
        }

        public bool ConvertToOutput(object input)
        {
            if (input == null) return false;

            GuiLogMessage("Converting from " + input.GetType() + " to " + GetType(settings.Converter), NotificationLevel.Debug);

            #region ConvertFromTypes

            #region ConvertFromICryptoolStream
            if (input is ICryptoolStream)
            {
                switch (this.settings.Converter)
                {
                    case OutputTypes.CryptoolStreamType:
                        {
                            Output = (ICryptoolStream)input;
                            break;
                        }

                    case OutputTypes.StringType:
                        {
                            byte[] buffer = ICryptoolStreamToByteArray((ICryptoolStream)input);
                            Output = GetStringForEncoding(buffer, settings.InputEncoding);
                            break;
                        }

                    case OutputTypes.ByteArrayType:
                        {
                            Output = ICryptoolStreamToByteArray((ICryptoolStream)input);
                            break;
                        }

                    case OutputTypes.BigIntegerType:
                        {
                            byte[] buffer = ICryptoolStreamToByteArray((ICryptoolStream)input);
                            Output = ByteArrayToBigInteger(buffer);
                            break;
                        }

                    default:
                        //GuiLogMessage("Conversion from ICryptoolStream to the chosen type is not implemented", NotificationLevel.Error);
                        GuiLogMessage("Conversion from " + input.GetType() + " to " + GetType(settings.Converter) + " is not implemented", NotificationLevel.Error);
                        return false;
                }

                return true;
            }
            #endregion
            #region ConvertFromIntArray
            else if (input is int[])
            {
                GuiLogMessage("Conversion from int[] to the chosen type is not implemented", NotificationLevel.Error);
                return false;
            }
            #endregion
            #region ConvertFromByteArray
            else if (input is byte[])
            {
                switch (this.settings.Converter)
                {
                    case OutputTypes.BigIntegerType: // byte[] to BigInteger
                        {
                            byte[] temp = (byte[])input;
                            if (settings.Endianness) Array.Reverse(temp);
                            Output = ByteArrayToBigInteger(temp);
                            return true;
                        }
                    case OutputTypes.IntType: // byte[] to int
                        {
                            try
                            {
                                byte[] temp = new byte[4];
                                Array.Copy((byte[])input, temp, 4);
                                if (settings.Endianness) Array.Reverse(temp);
                                Output = BitConverter.ToInt32(temp, 0);
                                return true;
                            }
                            catch (Exception e)
                            {
                                GuiLogMessage("Could not convert byte[] to integer: " + e.Message, NotificationLevel.Error);
                                return false;
                            }
                        }
                    case OutputTypes.ShortType: // byte[] to short
                        {
                            try
                            {
                                byte[] temp = new byte[2];
                                Array.Copy((byte[])input, temp, 2);
                                if (settings.Endianness) Array.Reverse(temp);
                                Output = BitConverter.ToInt16(temp, 0);
                                return true;
                            }
                            catch (Exception e)
                            {
                                GuiLogMessage("Could not convert byte[] to short: " + e.Message, NotificationLevel.Error);
                                return false;
                            }
                        }
                    case OutputTypes.ByteType: // byte[] to byte
                        {
                            try
                            {
                                Output = ((byte[])input)[0];
                                return true;
                            }
                            catch (Exception e)
                            {
                                GuiLogMessage("Could not convert byte[] to byte: " + e.Message, NotificationLevel.Error);
                                return false;
                            }
                        }
                    case OutputTypes.StringType: // byte[] to String
                        {
                            Output = GetStringForEncoding((byte[])input, settings.InputEncoding);
                            return true;
                        }
                    case OutputTypes.ByteArrayType: // byte[] to byte[]
                        {
                            Output = (byte[])input;
                            return true;
                        }
                    //default:
                    //    {
                    //        GuiLogMessage("Could not convert from byte[] to chosen type: ", NotificationLevel.Error);
                    //        return false;
                    //    }
                }
            }
            #endregion
            #region ConvertFromBigInteger
            else if (input is BigInteger)
            {
                if (this.settings.Converter == OutputTypes.ByteArrayType)
                {
                    Output = ((BigInteger)input).ToByteArray();
                    return true;
                }
            }
            #endregion
            #region  ConvertFromInt
            else if (input is int)
            {
                if (this.settings.Converter == OutputTypes.ByteArrayType)
                {
                    Output = BitConverter.GetBytes((int)input);
                    return true;
                }
            }
            #endregion
            #region ConvertFromShort
            else if (input is short)
            {
                if (this.settings.Converter == OutputTypes.ByteArrayType)
                {
                    Output = BitConverter.GetBytes((short)input);
                    return true;
                }
            }
            #endregion
            #region ConvertFromByte
            else if (input is byte)
            {
                if (this.settings.Converter == OutputTypes.ByteArrayType)
                {
                    Output = new byte[] { (byte)input };
                    return true;
                }
            }
            #endregion
            #region ConvertFromDouble
            else if (input is Double)
            {
                if (this.settings.Converter == OutputTypes.ByteArrayType)
                {
                    Output = BitConverter.GetBytes((Double)input);
                    return true;
                }
            }
            #endregion
            #region ConvertFromBool
            else if (input is bool)
            {
                switch (this.settings.Converter)
                {
                    case OutputTypes.StringType:
                            Output = input.ToString();
                            return true;

                    case OutputTypes.IntType:
                            Output = (int)((bool)input ? 1 : 0);
                            return true;

                    case OutputTypes.ShortType:
                            Output = (short)((bool)input ? 1 : 0);
                            return true;

                    case OutputTypes.ByteType:
                            Output = (byte)((bool)input ? 1 : 0);
                            return true;

                    case OutputTypes.ByteArrayType:
                            Output = new byte[] { (byte)(((bool)input) ? 1 : 0) };
                            return true;

                    case OutputTypes.BigIntegerType:
                            Output = (BigInteger)((bool)input ? 1 : 0);
                            return true;

                    case OutputTypes.DoubleType:
                            Output = (Double)((bool)input ? 1 : 0);
                            return true;

                    case OutputTypes.CryptoolStreamType:
                            Output = new byte[] { (byte)(((bool)input) ? 1 : 0) };
                            return true;

                    default:
                            GuiLogMessage("Could not convert from bool to chosen type: ", NotificationLevel.Error);
                            return false;
                }
            }
            #endregion

            #endregion

            // the string representation is used for all upcoming operations
            string inpString = Convert.ToString(input);

            #region ConvertFromString

            switch (this.settings.Converter) // convert to what?
            {
                #region ConvertToString
                case OutputTypes.StringType:
                    {
                        if (settings.Numeric)
                        {
                            try // can be read as parseable expression?
                            {
                                Output = BigIntegerHelper.ParseExpression(inpString).ToString();
                                return true;
                            }
                            catch (Exception) { }
                        }

                        Output = inpString;
                        return true;
                    }
                #endregion
                #region ConvertToInt
                case OutputTypes.IntType:
                    {
                        try // can be read as int from decimal string?
                        {
                            Output = Convert.ToInt32(inpString);
                            return true;
                        }
                        catch (Exception e)
                        {
                        }

                        try // can be read as int from hexadecimal string?
                        {
                            Output = Convert.ToInt32(inpString, 16);
                            return true;
                        }
                        catch (Exception e)
                        {
                            GuiLogMessage("Could not convert input to integer: " + e.Message, NotificationLevel.Error);
                            return false;
                        }
                    }
                #endregion
                #region ConvertToShort
                case OutputTypes.ShortType:
                    {
                        try // can be read as short from decimal string?
                        {
                            Output = Convert.ToInt16(inpString);
                            return true;
                        }
                        catch (Exception e)
                        {
                        }

                        try // can be read as short from hexadecimal string?
                        {
                            Output = Convert.ToInt16(inpString, 16);
                            return true;
                        }
                        catch (Exception e)
                        {
                            GuiLogMessage("Could not convert input to short: " + e.Message, NotificationLevel.Error);
                            return false;
                        }
                    }
                #endregion
                #region ConvertToByte
                case OutputTypes.ByteType:
                    {
                        try // can be read as byte from decimal string?
                        {
                            Output = Convert.ToByte(inpString);
                            return true;
                        }
                        catch (Exception e)
                        {
                        }

                        try // can be read as byte hexadecimal string?
                        {
                            Output = Convert.ToByte(inpString, 16);
                            return true;
                        }
                        catch (Exception e)
                        {
                            GuiLogMessage("Could not convert input to byte: " + e.Message, NotificationLevel.Error);
                            return false;
                        }
                    }
                #endregion
                #region ConvertToDouble
                case OutputTypes.DoubleType:
                    {
                        try // can be read as double?
                        {
                            String cleanInputString = DoubleCleanup(inpString); // apply user settings concerning input format
                            Output = Convert.ToDouble(cleanInputString);

                            GuiLogMessage("Converting String to double is not safe. Digits may have been cut off  ", NotificationLevel.Warning);
                            return true;
                        }
                        catch (Exception e)
                        {
                            GuiLogMessage("Could not convert input to double: " + e.Message, NotificationLevel.Error);
                            return false;
                        }
                    }
                #endregion
                #region ConvertToBigInteger
                case OutputTypes.BigIntegerType:
                    {
                        try // can be read as parseable expression?
                        {
                            Output = BigIntegerHelper.ParseExpression(inpString);
                            return true;
                        }
                        catch (Exception) { }

                        // remove all non-hex characters and parse as hexstring
                        byte[] result = TryMatchHex(inpString);
                        if (result != null)
                        {
                            Output = ByteArrayToBigInteger(result, settings.Endianness);
                            return true;
                        }

                        GuiLogMessage("Could not convert input to BigInteger", NotificationLevel.Error);
                        return false;
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
                        if (settings.Numeric) // apply user setting concerning numeric interpretation of input (else input is read as string)
                        {
                            //inpString = setText(Encoding.UTF8.GetBytes(inpString), settings.Presentation);

                            //try // can be read as BigInteger?
                            //{
                            //    Output = BigInteger.Parse(inpString).ToByteArray();
                            //    return true;
                            //}
                            //catch(Exception){}

                            try // can be read as parseable expression?
                            {
                                Output = BigIntegerHelper.ParseExpression(inpString).ToByteArray();
                                return true;
                            }
                            catch (Exception) { }

                            try // can be read as Hexstring?
                            {
                                byte[] result = TryMatchHex(inpString);
                                if (result != null)
                                {
                                    Output = result;
                                    return true;
                                }
                            }
                            catch (Exception) { }

                            try // can be read as double
                            {
                                double tempDouble = Convert.ToDouble(DoubleCleanup(inpString));
                                byte[] temp = BitConverter.GetBytes(tempDouble);
                                Output = temp;

                                double test = BitConverter.ToDouble(temp, 0);
                                GuiLogMessage("Converting String to double is not safe. Digits may have been cut off " + test.ToString(), NotificationLevel.Warning);

                                return true;
                            }
                            catch (Exception) { }
                        }

                        // numeric interpretation NOT selected:
                        Output = GetBytesForEncoding(inpString, settings.OutputEncoding);
                        return true;
                    }
                #endregion
                #region ConvertToCryptoolStream
                case OutputTypes.CryptoolStreamType:
                    {
                        GuiLogMessage("Conversion from " + input.GetType().Name, NotificationLevel.Info);

                        if (input is byte[] || input is byte || input is BigInteger || input is String)
                        {
                            OnPropertyChanged("Output"); 
                            return true;
                        }
                        else
                        {
                            GuiLogMessage("Conversion from " + input.GetType().Name + " to CryptoolStream is not yet implemented", NotificationLevel.Error);
                            return false;
                        }
                    }
                #endregion
                #region ConvertToAnythingLeft
                default:
                    return false;
                #endregion
            }

            #endregion
        }

        public void Execute()
        {
            ProgressChanged(0, 100);
            if( ConvertToOutput(InputOne) ) ProgressChanged(100, 100);
        }

        private String setText( byte[] bytes, ConverterSettings.PresentationFormat presentation )
        {
            switch (presentation)
            {
                case ConverterSettings.PresentationFormat.Text:
                    return GetStringForEncoding(bytes,settings.OutputEncoding);

                case ConverterSettings.PresentationFormat.Hex:
                    return BitConverter.ToString(bytes).Replace("-", "");

                case ConverterSettings.PresentationFormat.Base64:
                    return Convert.ToBase64String(bytes);

                case ConverterSettings.PresentationFormat.Decimal:
                    return string.Join(" ", Array.ConvertAll(bytes, item => item.ToString()));

                default:
                    return null;
            }
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

        public event StatusChangedEventHandler OnPluginStatusChanged;

        void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(this, args);
        }

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //GuiLogMessage("settings_PropertyChanged: "+e.PropertyName, NotificationLevel.Debug);
                      
            //if ( e.PropertyName == "OutputEncoding" )
            {
                ConvertToOutput(InputOne);
            }
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
