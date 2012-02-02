/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.IO;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.IO;

namespace Cryptool.Plugins.CypherMatrix
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Michael Schäfer", "michael.schaefer@rub.de", null, null)]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("CypherMatrix", "CypherMatrix En/Decryption", "CypherMatrix/doc.xml", new[] { "CrypWin/images/default.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class CypherMatrix : ICrypComponent
    {
        #region Private variables and public constructor

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly CypherMatrixSettings settings;
        private CStreamWriter outputStreamWriter;
        private StreamWriter debugDataWriter;
        private CStreamReader inputStreamReader;
        private List<byte> cipherChars;
        private List<byte> blockKey;
        private byte[] matrixKey;
        private byte[] cm1;
        private byte[] cm3;
        private bool stop = false;

        public CypherMatrix()
        {
            this.settings = new CypherMatrixSettings();
            cm1 = new byte[256];
            cm3 = new byte[256];
        }

        #endregion

        #region Data Properties

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>

        [PropertyInfo(Direction.InputData, "Input Data", "Data to be processed by the CypherMatrix cipher", true)]
        public ICryptoolStream InputStream
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "Passwort", "Input the passwords bytes to be used by the CypherMatrix cipher", true)]
        public byte[] InputByteArray
        {
            get;
            set;
        }

        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>
        //[PropertyInfo(Direction.OutputData, "Text Output", "The string after processing with the CypherMatrix cipher")]
        //public string OutputString
        //{
        //    get;
        //    set;
        //}

        [PropertyInfo(Direction.OutputData, "Output Data", "Data after processing with CypherMatrix", true)]
        public ICryptoolStream OutputStream
        {
            get
            {
                return outputStreamWriter;
            }
            set
            {
                // empty
            }
        }

        //[PropertyInfo(Direction.OutputData, "Debug Data", "internal state of CypherMatrix", false)]
        //public ICryptoolStream OutputDebug
        //{
        //    get
        //    {
        //        return debugDataWriter;
        //    }
        //    set
        //    {
        //        // empty
        //    }
        //}

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            stop = false;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            // HOWTO: Use this to show the progress of a plugin algorithm execution in the editor.
            ProgressChanged(0, 1);
            Stopwatch sw = new Stopwatch();

            try
            {
                if (InputStream == null || InputStream.Length == 0)
                {
                    GuiLogMessage("No input data, aborting now", NotificationLevel.Error);
                    return;
                }
                if (InputByteArray.Length == 0)
                {
                    GuiLogMessage("No password bytes, aborting now", NotificationLevel.Error);
                    return;
                }
                inputStreamReader = InputStream.CreateReader();
                //inputStreamReader.WaitEof();
                outputStreamWriter = new CStreamWriter();
                debugDataWriter = new StreamWriter("CypherMatrixDebug.log", false);  // sollte die Datei schon vorhanden sein, sie überschrieben
                sw.Start();

                switch (settings.Action)
                {
                    case CypherMatrixSettings.CypherMatrixMode.Encrypt:
                        {
                            //GuiLogMessage("Starting encryption.", NotificationLevel.Debug);
                            Encrypt();
                            break;
                        }
                    case CypherMatrixSettings.CypherMatrixMode.Decrypt:
                        {
                            //GuiLogMessage("Starting decryption.", NotificationLevel.Debug);
                            Decrypt();
                            break;
                        }
                    case CypherMatrixSettings.CypherMatrixMode.Hash:
                        {
                            sw.Stop();
                            outputStreamWriter.Close();
                            throw new NotImplementedException();
                            //break;
                        }
                    default:
                        {
                            sw.Stop();
                            outputStreamWriter.Close();
                            throw new NotImplementedException();
                            //break;
                        }
                }

                sw.Stop();
                GuiLogMessage(string.Format("Compution in {0} ms completed.", sw.ElapsedMilliseconds), NotificationLevel.Info);
                GuiLogMessage(string.Format("Achieved data throughput: {0:N} kB/s", (double) InputStream.Length / sw.ElapsedMilliseconds), NotificationLevel.Info);
                outputStreamWriter.Flush();
                outputStreamWriter.Close();
                debugDataWriter.Flush();
                debugDataWriter.Close();
                if(settings.Debug)
                    GuiLogMessage(String.Format("Debug data has been written to {0}\\CypherMatrixDebug.log.", Environment.CurrentDirectory), NotificationLevel.Info);
                OnPropertyChanged("OutputStream");
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
            
            //// HOWTO: After you have changed an output property, make sure you announce the name of the changed property to the CT2 core.
            //SomeOutput = SomeInput - settings.SomeParameter;
            //OnPropertyChanged("SomeOutput");

            // HOWTO: You can pass error, warning, info or debug messages to the CT2 main window.
            //if (settings.SomeParameter < 0)
            //    GuiLogMessage("SomeParameter is negative", NotificationLevel.Debug);

            // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            this.stop = true;
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            settings.Initialize();
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {

        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion

        #region CypherMatrix

        private void Encrypt()
        {
            int length = settings.BlockKeyLen, bytesRead = 0;
            int roundMax = (int)(inputStreamReader.Length / length) + 1;
            cipherChars = new List<byte>(128);
            blockKey = new List<byte>(length);
            List<byte> xor = new List<byte>();
            List<uint> index = new List<uint>();
            List<byte> ciphertext = new List<byte>();
            matrixKey = new byte[settings.MatrixKeyLen];
            byte[] plaintext = new byte[length];
            int startseqLen = InputByteArray.Length < settings.MatrixKeyLen ? settings.MatrixKeyLen : InputByteArray.Length; // als Länge wird immer der größere Wert genommen
            byte[] startseq = new byte[startseqLen];
            Buffer.BlockCopy(InputByteArray, 0, startseq, 0, InputByteArray.Length);
            int round = 1;
            
            while ((bytesRead = inputStreamReader.ReadFully(plaintext)) > 0)
            {
                if (bytesRead < length)
                    // in der letzten Runde Padding durch hinzufügen von Leerzeichen bis der Puffer voll ist
                    for(int i = bytesRead; i < plaintext.Length; i++)
                        plaintext[i] = 0x20;

                // Schlüssel generieren
                //GuiLogMessage(string.Format("Encryption round {0} started.", round), NotificationLevel.Debug);
                Generator(startseq, startseqLen, round);

                // Verschlüsseln
                // 1. Klartext XOR Blockschlüssel
                for (int i = 0; i < length; i++)
                    xor.Add((byte)(plaintext[i] ^ blockKey[i]));

                // bit conversation
                long puffer = 0;
                int bitCount = 0;
                for (int i = 0; i < length; i++)
                {
                    puffer <<= 8;       // mache für die nächsten 8 Bits Platz
                    puffer |= xor[i];   // schreibe die nächsten 8 Bits in den Puffer
                    bitCount += 8;             // bitCount als Zähler für die Bits im Puffer, erhöhe um 8
                    index.Add((byte)(puffer >> (bitCount - 7) & 0x7F));    // lies die obersten 7 Bits aus
                    bitCount -= 7;             // verringere um 7, da 7 Bits ausgelesen wurden
                    // aus Performancegründen werden die gelesenen Bits nicht gelöscht
                    if (bitCount == 7)
                    {
                        index.Add((byte)(puffer & 0x7F));       // haben sich 7 Bits angesammelt, so ließ sie aus
                        bitCount = 0;          // 7 Bits gelesen, 7 - 7 = 0
                    }
                }
                switch (bitCount)
                {
                    case 1: index.Add((byte)(puffer & 0x01)); break;
                    case 2: index.Add((byte)(puffer & 0x03)); break;
                    case 3: index.Add((byte)(puffer & 0x07)); break;
                    case 4: index.Add((byte)(puffer & 0x0F)); break;
                    case 5: index.Add((byte)(puffer & 0x1F)); break;
                    case 6: index.Add((byte)(puffer & 0x3F)); break;
                    default: break;
                }

                // Abbildung auf Chiffre-Alphabet
                for (int i = 0; i < index.Count; i++)
                    outputStreamWriter.WriteByte(cipherChars[(byte)index[i]]);

                // Vorbereitungen für die nächste Runde
                cipherChars.Clear();
                blockKey.Clear();
                xor.Clear();
                index.Clear();
                startseqLen = matrixKey.Length;
                Buffer.BlockCopy(matrixKey, 0, startseq, 0, startseqLen);

                if (stop)
                {
                    GuiLogMessage("Aborted!", NotificationLevel.Warning);
                    stop = false;
                    break;
                }

                roundMax = (int)(inputStreamReader.Length / length) + 1;
                ProgressChanged(round, roundMax);
                round++;
            }
        }

        private void Decrypt()
        {
            // zur Berechnung der Länge eines Chiffreblocks wird mathematisch gerundet

            int length = settings.BlockKeyLen, bytesRead = 0, len7 = (int)Math.Round((double)settings.BlockKeyLen * 8 / 7);
            int roundMax = (int)(inputStreamReader.Length / length) + 1;
            cipherChars = new List<byte>(128);
            blockKey = new List<byte>(length);
            List<byte> xor = new List<byte>();
            List<uint> index = new List<uint>();
            matrixKey = new byte[settings.MatrixKeyLen];
            byte[] cipherBlock = new byte[len7];
            int startseqLen = InputByteArray.Length < settings.MatrixKeyLen ? settings.MatrixKeyLen : InputByteArray.Length; // als Länge wird immer der größere Wert genommen
            byte[] startseq = new byte[startseqLen];
            Buffer.BlockCopy(InputByteArray, 0, startseq, 0, InputByteArray.Length);
            int round = 1;

            while ((bytesRead = inputStreamReader.ReadFully(cipherBlock)) > 0)
            {
                if (bytesRead < len7)
                    len7 = cipherBlock.Length;      // in der letzten Runde ist der Klartext warscheinlich nicht einen Block breit

                // Schlüssel generieren
                Generator(startseq, startseqLen, round);

                // Analyse Chiffrat, char zu 7-Bit Index
                foreach (byte b in cipherBlock)
                    index.Add((uint)cipherChars.IndexOf(b));

                // Bit-Konversion, 7-Bit Index zu 8-Bit Werten
                long puffer = 0;
                int bitCount = 0;
                for (int i = 0; i < index.Count; i++)
                {
                    puffer <<= 7;           // mache für die nächsten 7 Bits Platz
                    puffer |= index[i];     // schreibe die nächsten 7 Bits in den Puffer
                    bitCount += 7;          // bitCount als Zähler für die Bits im Puffer, erhöhe um 7
                    // aus Performancegründen werden die gelesenen Bits nicht gelöscht
                    if (bitCount > 7)
                    {
                        xor.Add((byte)(puffer >> (bitCount - 8) & 0xFF));       // haben sich 8 Bits angesammelt, so ließ sie aus
                        bitCount -= 8;          // 8 Bits gelesen
                    }
                }
                switch (bitCount)
                {
                    case 1: xor.Add((byte)(puffer & 0x01)); break;
                    case 2: xor.Add((byte)(puffer & 0x03)); break;
                    case 3: xor.Add((byte)(puffer & 0x07)); break;
                    case 4: xor.Add((byte)(puffer & 0x0F)); break;
                    case 5: xor.Add((byte)(puffer & 0x1F)); break;
                    case 6: xor.Add((byte)(puffer & 0x3F)); break;
                    case 7: xor.Add((byte)(puffer & 0x7F)); break;
                    default: break;
                }

                // XOR mit Blockschlüssel, Rückgewinnung des Klartextes
                for (int i = 0; i < xor.Count; i++)
                    //plaintext.Append((char)(xor[i] ^ blockKey[i]));
                    outputStreamWriter.WriteByte((byte)(xor[i] ^ blockKey[i]));

                // Vorbereitungen für die nächste Runde
                cipherChars.Clear();
                blockKey.Clear();
                xor.Clear();
                index.Clear();
                for (int i = 0; i < matrixKey.Length; i++)
                    startseq[i] = matrixKey[i];
                startseqLen = matrixKey.Length;
                Buffer.BlockCopy(matrixKey, 0, startseq, 0, startseqLen);

                if (stop)
                {
                    GuiLogMessage("Aborted!", NotificationLevel.Warning);
                    stop = false;
                    break;
                }

                roundMax = (int)(inputStreamReader.Length / length) + 1;
                ProgressChanged(round, roundMax);
                round++;
            }
        }

        // base function 
        private void Generator(byte[] startseq, int startseqLen, int r)
        {
            // das Ergebnis der Mod Operation kann negativ sein! Im Skript wird immer von einer positiven Zahl ausgegangen
            // Initialliserung der Variablen
            int H_k = 0;
            int i, j, k, l;
            long H_p = 0, s_i = 0;
            List<int> d = new List<int>();
            
            int n = startseqLen;
            int C_k = n * (n - 2) + settings.Code;
            
            for (i = 1; i <= n; i++)
                H_k += (startseq[i - 1] + 1) * (i + C_k);   // i-1, da das Array von 0 bis n-1 läuft, im Paper von 1 bis n

            // Berechnung der Hashfunktionsfolge
            for (i = 1; i <= n; i++)
            {
                s_i = (((long)startseq[i - 1] + 1) * i * H_k + (i + settings.Code + r));    // i-1, da das Array von 0 bis n-1 läuft, im Paper von 1 bis n; Erhöhung der Präzision durch cast auf long, wichtig!
                LongToBaseX(s_i, d, settings.Basis);
                H_p += s_i;
            }

            long H_ges = H_p + H_k;
            List<int> tmp = new List<int>(d);
            LongToBaseX(H_ges, d, settings.Basis);
            tmp.Reverse();
            d.AddRange(tmp);
            tmp.Clear();
            //folgende Zeile findet sich im Basic Code 
            //d.RemoveAll(new System.Predicate<int>(delegate(int val) { return (val == 8); }));   // ANSI 0x08: Backspace Steuerzeichen

            // Berechnung der Parameter
            int variante = (H_k % 11) + 1;
            int Alpha = (int)(H_ges % 255 + 1);
            int Beta = H_k % 169 + 1;
            int Gamma = (int)((H_p + settings.Code) % 196 + 1);
            int Delta = (int)(H_ges % 155 + settings.Code);
            int Theta = H_k % 32 + 1;            

            // Generierung der Basis-Variation
            k = 0;
            for (int e = 0; k < 256; k++)
            {
                e = (BaseXToInt(d, k + variante - 1, 3, settings.Basis + 1) - Theta) % 256;    // k + variante - 1, weil array d bei 0 anfängt
                cm1[k] = (byte)e;
                //Logik zum testen ob ein Wert schon im Array vorhanden ist
                for (i = 0; i < k; i++)
                {
                    if (cm1[i] == e)
                    {
                        e++;
                        if (e >= 256)
                            e = e - 256;    // Reduziere e falls es zu groß wird
                        cm1[k] = (byte)e;
                        i = -1;     // das Array soll von Null an abgesucht werden; i wird als nächstes direkt um 1 erhöht
                    }
                }
            }

            // 3-fach Permutation der Basis-Variation
            //Perm. Variante B
            i = 1; k = 0; l = 0;
            for (int pos = Alpha - 1; i <= 16; i++)
            {
                for (j = 1; j <= 16; j++)
                {
                    k = i - j;
                    if (k <= 0)
                        k += 16;
                    l = k - j;
                    if (l <= 0)
                        l += 16;
                    cm3[(k - 1) * 16 + (l - 1)] = cm1[pos];
                    pos++;
                    if (pos > 255)
                        pos = 0;
                }
            }

            // Entnahme Chiffre-Alphabet
            cipherChars.AddRange(cm3);
            cipherChars.AddRange(cm3);
            cipherChars.RemoveRange(0, Alpha - 1);
            cipherChars.RemoveAll(CharFilter);
            if (cipherChars.Count > 128)
                cipherChars.RemoveRange(128, cipherChars.Count - 128);
            // Block-Schlüssel
            for (i = 0; i < settings.BlockKeyLen; i++)
                blockKey.Add((byte)cm3[Beta - 1 + i]);
            
            // Matrix-Schlüssel
            for (i = 0; i < settings.MatrixKeyLen; i++)
                matrixKey[i] = cm3[Gamma - 1 + i];

            // Debugdaten schreiben
            if (settings.Debug)
            {
                //debugDataWriter.Write(String.Format("\r\nInputByteArray (hex): \r\n "));
                //foreach (byte b in InputByteArray)
                //    debugDataWriter.Write(String.Format(" {0:X2}", b));
                //debugDataWriter.WriteLine("\r\n");
                debugDataWriter.WriteLine(String.Format("Data of round {0}\r\n", r));
                debugDataWriter.WriteLine(String.Format("code = {0}", settings.Code));
                debugDataWriter.WriteLine(String.Format("basis = {0}", settings.Basis));
                debugDataWriter.WriteLine(String.Format("blockKeyLen = {0}", settings.BlockKeyLen));
                debugDataWriter.WriteLine(String.Format("matrixKeyLen = {0}", settings.MatrixKeyLen));
                debugDataWriter.WriteLine(String.Format("hashBlockLen = {0}", settings.HashBlockLen));
                debugDataWriter.Write(String.Format("\r\nstartSequence (hex): \r\n "));
                for(i = 0; i < startseqLen; i++)
                    debugDataWriter.Write(String.Format(" {0:X2}", startseq[i]));
                debugDataWriter.WriteLine("\r\n");
                debugDataWriter.WriteLine(String.Format("n = {0}", n));
                debugDataWriter.WriteLine(String.Format("C_k = {0}", C_k));
                debugDataWriter.WriteLine(String.Format("H_k = {0}", H_k));
                debugDataWriter.WriteLine(String.Format("H_p = {0}", H_p));
                debugDataWriter.Write(String.Format("\r\nd (hex): \r\n "));
                foreach (int v in d)
                    debugDataWriter.Write(String.Format(" {0:X2}", v));
                debugDataWriter.WriteLine("\r\n");
                debugDataWriter.WriteLine(String.Format("variante = {0}", variante));
                debugDataWriter.WriteLine(String.Format("Alpha = {0}", Alpha));
                debugDataWriter.WriteLine(String.Format("Beta = {0}", Beta));
                debugDataWriter.WriteLine(String.Format("Gamma = {0}", Gamma));
                debugDataWriter.WriteLine(String.Format("Delta = {0}", Delta));
                debugDataWriter.WriteLine(String.Format("Theta = {0}", Theta));
                debugDataWriter.Write(String.Format("\r\ncm1 (hex): \r\n "));
                for (i = 0; i < 256; )
                {
                    for (j = 0; j < 16; j++)
                    {
                        debugDataWriter.Write(String.Format(" {0:X2}", cm1[i]));
                        i++;
                    }
                    debugDataWriter.Write("\r\n ");
                }
                debugDataWriter.Write(String.Format("\r\ncm3 (hex): \r\n "));
                for (i = 0; i < 256; )
                {
                    for (j = 0; j < 16; j++)
                    {
                        debugDataWriter.Write(String.Format(" {0:X2}", cm3[i]));
                        i++;
                    }
                    debugDataWriter.Write("\r\n ");
                }
                debugDataWriter.Write(String.Format("\r\ncipherChars (hex): \r\n "));
                foreach (byte b in cipherChars)
                    debugDataWriter.Write(String.Format(" {0:X2}", b));
                debugDataWriter.WriteLine();
                debugDataWriter.Write(String.Format("\r\nblockKey (hex): \r\n "));
                foreach (byte b in blockKey)
                    debugDataWriter.Write(String.Format(" {0:X2}", b));
                debugDataWriter.WriteLine();
                debugDataWriter.Write(String.Format("\r\nmatrixKey (hex): \r\n "));
                foreach (byte b in matrixKey)
                    debugDataWriter.Write(String.Format(" {0:X2}", b));
                debugDataWriter.WriteLine();
                debugDataWriter.WriteLine(String.Format("\r\n>>>>>>>>>> END OF ROUND <<<<<<<<<<\r\n\r\n"));
            }
        }

        // function for changing to a choosen base
        private void LongToBaseX(long number, List<int> list, int x)
        {
            List<int> a = new List<int>();
            while (number != 0)
            {
                a.Add((int)(number % x));
                number = number / x;
            }
            for (int i = a.Count - 1; i >= 0; i--)
            {
                list.Add(a[i]);
            }
        }

        // function for changing back to the base 10
        static int BaseXToInt(List<int> list, int start, int length, int Base)
        {
            int b = 1, a = 0;
            for (int i = start + length - 1; i >= start; i--)
            {
                a += list[i] * b;
                b *= Base;
            }
            return a;
        }

        // function to filter certain chars
        private bool CharFilter(byte i)
        {
            return i < 33 || i == 34 || i == 44 || i == 176 || i == 177 || i == 178 || i == 213 || i == 219 || i == 220 || i == 221 || i == 222 || i == 223;
        }

        #endregion
    }
}
