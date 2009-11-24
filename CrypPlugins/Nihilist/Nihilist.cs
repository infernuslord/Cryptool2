/*                              
   Copyright 2009 Fabian Enkler

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


using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Cryptography;
using System.Windows;

namespace Nihilist
{
    [Author("Fabian Enkler", "enkler@cryptool.org", "", "")]
    [PluginInfo(false, "Nihilist", "Nihilist -- classic digraph polyalphabetic substitution cipher substituting each letter by a 2-digit-number", "", "Nihilist/icon.png")]
    [EncryptionType(EncryptionType.Classic)]
    public class Nihilist : IEncryption
    {
        private readonly NihilistSettings settings = new NihilistSettings();

        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
#pragma warning restore
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        private byte[] input = new byte[] { };
        [PropertyInfo(Direction.InputData, "Input byte array", "This is the byte array to be processed by Nihilist cipher.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public byte[] Input
        {
            get { return input; }
            set
            {
                input = value;
                OnPropertyChanged("Input");
            }
        }

        private byte[] output = new byte[] { };
        [PropertyInfo(Direction.OutputData, "Output byte array", "This is the byte array processed by Nihilist cipher", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public byte[] Output
        {
            get { return output; }
            set
            {
                output = value;
                OnPropertyChanged("Output");
            }
        }

        public void PreExecution()
        {

        }

        public void Execute()
        {
            char[,] KeyArray;
            var CryptMatrix = CreateCryptMatrix(out KeyArray);
            var secondKeyByteArray = new byte[settings.SecondKeyWord.Length];
            int Counter = 0;
            foreach (var c in settings.SecondKeyWord.ToLower())
            {
                byte tmpByte = byte.Parse(CryptMatrix[c].X.ToString() + CryptMatrix[c].Y.ToString());
                secondKeyByteArray[Counter] = tmpByte;
                Counter++;
            }
            output = new byte[input.Length];
            int max = input.Length;
            if (settings.Action == 0)
            {
                int i = 0;
                int j = 0;
                string inputString = ByteArrayToString(input);

                foreach (var c in inputString.ToLower())
                {
                    var c1 = c;
                    if (c1 == 'j')
                        c1 = 'i';

                    byte tmpByte = CryptMatrix.ContainsKey(c1) ? byte.Parse(CryptMatrix[c1].X.ToString() + CryptMatrix[c1].Y.ToString()) : HandleUnknownChar();
                    tmpByte += secondKeyByteArray[j];
                    output[i] = tmpByte;
                    i++;
                    j++;
                    if (j >= secondKeyByteArray.Length)
                        j = 0;

                    OnProgressChanged(i+1,max);
                }
            }
            else
            {
                int i = 0;
                int j = 0;
                foreach (byte b in input)
                {
                    byte tmpbyte = b;
                    tmpbyte -= secondKeyByteArray[j];
                    string tmpString = tmpbyte.ToString();
                    if (tmpbyte < 255)
                    {
                        if (tmpbyte < 10)
                            tmpString = "0" + tmpString;
                        int Index1 = int.Parse(tmpString[0].ToString());
                        int Index2 = int.Parse(tmpString[1].ToString());
                        char DecryptedChar = KeyArray[Index1, Index2];
                        output[i] = (byte)DecryptedChar;
                    }
                    else
                    {
                        output[i] = (byte)'?';
                    }

                    i++;
                    j++;
                    if (j >= secondKeyByteArray.Length)
                        j = 0;
                    OnProgressChanged(i+1, max);
                }
            }
            OnPropertyChanged("Output");
        }

        private static byte HandleUnknownChar()
        {
            return 255;
        }

        private static string ByteArrayToString(ICollection<byte> arr)
        {
            var builder = new StringBuilder(arr.Count);
            foreach (var b in arr)
            {
                builder.Append((char)b);
            }
            return builder.ToString();
        }

        private Dictionary<char, Vector> CreateCryptMatrix(out char[,] KeyArr)
        {
            var KeyArray = new char[5, 5];
            var CharDic = new HashSet<char>();
            int Row = 0;
            int Col = 0;
            foreach (var c in settings.KeyWord.ToLower() + "abcdefghiklmnopqrstuvwxyz")
            {
                if (!CharDic.Contains(c))
                {
                    if (Row < KeyArray.GetLength(1))
                        KeyArray[Row, Col] = c;
                    CharDic.Add(c);
                    Col++;
                    if (Col >= KeyArray.GetLength(0))
                    {
                        Col = 0;
                        Row++;
                    }
                }
            }
            KeyArr = KeyArray;
            var CharPosDic = new Dictionary<char, Vector>();
            for (int i = 0; i < KeyArray.GetLength(0); i++)
            {
                for (int j = 0; j < KeyArray.GetLength(1); j++)
                {
                    CharPosDic.Add(KeyArray[i, j], new Vector(i, j));
                }
            }
            return CharPosDic;
        }

        public void PostExecution()
        {

        }

        public void Pause()
        {

        }

        public void Stop()
        {

        }

        public void Initialize()
        {

        }

        public void Dispose()
        {

        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        private void OnProgressChanged(int value, int max)
        {
            if (OnPluginProgressChanged != null)
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
        }
    }
}
