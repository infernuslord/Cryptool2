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
using System.Text;

namespace Cryptool.Plugins.CypherMatrix
{
    [Author("Michael Schäfer", "michael.schaefer@rub.de", null, null)]
    [PluginInfo("CypherMatrix.Properties.Resources", "PluginCaption", "PluginTooltip", "CypherMatrix/doc.xml", new[] { "CrypWin/images/default.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class CypherMatrix : ICrypComponent
    {
        #region Private variables and public constructor

        private readonly CypherMatrixSettings settings;
        private CStreamWriter outputStreamWriter;
        private CStreamWriter debugDataWriter;
        private CStreamReader inputStreamReader;
        private List<byte> cipherChars;
        private List<byte> blockKey;
        private byte[] matrixKey;
        private byte[] cm1;
        private byte[] cm3;
        private bool stop = false;
        private Encoding encoding = Encoding.UTF8;

        public CypherMatrix()
        {
            this.settings = new CypherMatrixSettings();
            cm1 = new byte[256];
            cm3 = new byte[256];
            cipherChars = new List<byte>(128);
        }
        #endregion

        #region Data Properties and private writers

        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip", true)]
        public ICryptoolStream InputStream
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputPasswordCaption", "InputPasswordTooltip", true)]
        public byte[] InputByteArray
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", true)]
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

        [PropertyInfo(Direction.OutputData, "DebugDataCaption", "DebugDataTooltip", false)]
        public ICryptoolStream OutputDebug
        {
            get
            {
                return debugDataWriter;
            }
            set
            {
                // empty
            }
        }

        /// <summary>
        /// Function to write data to the OutputStream.
        /// </summary>
        /// <param name="value">the value that should be written</param>
        private void WriteOutput(ulong value)
        {
            outputStreamWriter.Write(encoding.GetBytes(value.ToString()));
        }

        /// <summary>
        /// Function to write data to the DebugStream.
        /// </summary>
        /// <param name="str">the string that should be written</param>
        private void WriteDebug(string str)
        {
            debugDataWriter.Write(encoding.GetBytes(str));
        }

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
            ProgressChanged(0, 1);

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
                //debugDataWriter = new StreamWriter("CypherMatrixDebug.log", false);  // sollte die Datei schon vorhanden sein, wird sie überschrieben
                debugDataWriter = new CStreamWriter();
                blockKey = new List<byte>(settings.BlockKeyLen);
                matrixKey = new byte[settings.MatrixKeyLen];
                Stopwatch sw = new Stopwatch();
                sw.Start();

                switch (settings.Action)
                {
                    case CypherMatrixSettings.CypherMatrixMode.Encrypt:
                        {
                            Encrypt();
                            break;
                        }
                    case CypherMatrixSettings.CypherMatrixMode.Decrypt:
                        {
                            Decrypt();
                            break;
                        }
                    case CypherMatrixSettings.CypherMatrixMode.Hash:
                        {
                            Hash();
                            break;
                        }
                    default:
                        {
                            sw.Stop();
                            outputStreamWriter.Close();
                            throw new NotImplementedException("Unkown execution mode!");
                            //break;
                        }
                }
                sw.Stop();
                if (!stop)
                {
                    GuiLogMessage(string.Format("Processed {0:N} kB data in {1} ms.", (double)InputStream.Length / 1024, sw.ElapsedMilliseconds), NotificationLevel.Info);
                    GuiLogMessage(string.Format("Achieved data throughput: {0:N} kB/s", (double)InputStream.Length / sw.ElapsedMilliseconds), NotificationLevel.Info);
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
            finally
            {
                if (stop)
                {
                    GuiLogMessage("Aborted!", NotificationLevel.Warning);
                    stop = false;
                }
                if (!settings.Debug)
                    WriteDebug("You have to enable the debug logging to see the internal variables here.");
                //GuiLogMessage(String.Format("Debug data has been written to {0}\\CypherMatrixDebug.log.", Environment.CurrentDirectory), NotificationLevel.Info);

                outputStreamWriter.Flush();
                outputStreamWriter.Close();
                debugDataWriter.Flush();
                debugDataWriter.Close();
                OnPropertyChanged("OutputStream");
                OnPropertyChanged("OutputDebug");
            }

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            // lösche die angefallenen Werte
            cm1 = new byte[256];
            cm3 = new byte[256];
            cipherChars = new List<byte>(128);
            blockKey = null;
            matrixKey = null;
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
            List<byte> xor = new List<byte>();
            List<uint> index = new List<uint>();
            List<byte> ciphertext = new List<byte>();
            byte[] plaintext = new byte[length];
            byte[] startseq;
            if (InputByteArray.Length < settings.MatrixKeyLen)
                startseq = new byte[settings.MatrixKeyLen];
            else
                startseq = new byte[InputByteArray.Length];
            int startseqLen = InputByteArray.Length;
            Buffer.BlockCopy(InputByteArray, 0, startseq, 0, InputByteArray.Length);
            int round = 1;
            while ((bytesRead = inputStreamReader.ReadFully(plaintext)) > 0)
            {
                if (bytesRead < length)
                    // in der letzten Runde Padding durch hinzufügen von Leerzeichen bis der Puffer voll ist
                    for (int i = bytesRead; i < plaintext.Length; i++)
                        plaintext[i] = 0x20;


                // Schlüssel generieren
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
            List<byte> xor = new List<byte>();
            List<uint> index = new List<uint>();
            byte[] cipherBlock = new byte[len7];
            byte[] startseq;
            if (InputByteArray.Length < settings.MatrixKeyLen)
                startseq = new byte[settings.MatrixKeyLen];
            else
                startseq = new byte[InputByteArray.Length];
            int startseqLen = InputByteArray.Length;
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
            int[] perm = new int[16];   // Permutationsarray bei Permutation mit Variate C

            int n = startseqLen;
            int C_k = n * (n - 2) + settings.Code;

            for (i = 1; i <= n; i++)
                //!!Die alte Variante ist nur für Testzwecke gedacht!!
                //neue Variante
                H_k += (startseq[i - 1] + 1) * (i + C_k + r);   // i-1, da das Array von 0 bis n-1 läuft, im Paper von 1 bis n
            //alte Variante
            //H_k += (startseq[i - 1] + 1) * (i + C_k);   // i-1, da das Array von 0 bis n-1 läuft, im Paper von 1 bis n

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
            if (256 < d.Count - 3)
                j = 256;
            else
                j = d.Count - 3;    // es sollen im 3 Elemente ausgelesen werden; es würde ein Fehler geworfen werden, wenn das 3. Element nicht mehr gelesen werden kann
            for (byte e = 0; k < j; k++)
            {
                e = (byte)(BaseXToInt(d, k + variante - 1, 3, settings.Basis + 1) - Theta);    // k + variante - 1, weil array d bei 0 anfängt; beim byte-cast wird automatisch mod 256 gerechnet
                cm1[k] = (byte)e;
                //Logik zum testen ob ein Wert schon im Array vorhanden ist
                for (i = 0; i < k; i++)
                {
                    if (cm1[i] == e)
                    {
                        e++;
                        //if (e >= 256)
                        //    e = e - 256;    // Reduziere e falls es zu groß wird
                        cm1[k] = e;
                        i = -1;     // das Array soll von Null an abgesucht werden; i wird als nächstes direkt um 1 erhöht
                    }
                }
            }
            // 2. for-Schleife für den Fall, dass d zu wenig Elemente enthällt
            for (byte e = 0; k < 256; k++)
            {
                e = (byte)(BaseXToIntSafe(d, k + variante - 1, 3, settings.Basis + 1) - Theta);    // k + variante - 1, weil array d bei 0 anfängt; beim byte-cast wird automatisch mod 256 gerechnet
                cm1[k] = e;
                //Logik zum testen ob ein Wert schon im Array vorhanden ist
                for (i = 0; i < k; i++)
                {
                    if (cm1[i] == e)
                    {
                        e++;
                        //if (e >= 256)
                        //    e = e - 256;    // Reduziere e falls es zu groß wird
                        cm1[k] = e;
                        i = -1;     // das Array soll von Null an abgesucht werden; i wird als nächstes direkt um 1 erhöht
                    }
                }
            }

            // 3-fach Permutation der Basis-Variation
            switch (settings.Perm)
            {
                case CypherMatrixSettings.Permutation.B:
                    {
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
                        break;
                    }
                case CypherMatrixSettings.Permutation.C:
                    {
                        throw new NotImplementedException("NYI! Please use permutation function B!");
                        int x = Delta - 1, a = 0;
                        for (k = 0; k < 16; k++)
                        {
                            x++;
                            if (x > 256)
                                x -= 256;
                            a = (cm1[x] + Theta) % 16;
                            perm[k] = a;

                            //Logik um doppelte Werte zu vermeiden
                            for (i = 0; i < k; i++)
                            {
                                if (perm[i] == a)
                                {
                                    a++;
                                    if (a >= 16)
                                        a = a - 16;    // Reduziere e falls es zu groß wird
                                    perm[k] = a;
                                    i = -1;     // das Array soll von Null an abgesucht werden; i wird als nächstes direkt um 1 erhöht
                                }
                            }
                        }

                        i = 1; k = 0; l = 0;
                        for (int pos = Alpha - 1; i <= 16; i++)
                        {
                            for (j = 1; j <= 16; j++)
                            {
                                k = i - j;
                                if (k <= 0)
                                    k += 16;
                                l = perm[j - 1];    // l ist ein Wert mod 16
                                cm3[(k - 1) * 16 + l] = cm1[pos];   // l ist ein Wert mod 16, weshalb er nicht um 1 reduziert werden muss
                                pos++;
                                if (pos > 255)
                                    pos = 0;
                            }
                        }

                        break;
                    }
                default: throw new NotImplementedException("Unknown permutation function!");
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
                //WriteDebug(String.Format("\r\nInputByteArray (hex): \r\n "));
                //foreach (byte b in InputByteArray)
                //    WriteDebug(String.Format(" {0:X2}", b));
                //WriteDebug("\r\n");
                WriteDebug(String.Format("Data of round {0}\r\n\r\n", r));
                WriteDebug(String.Format("code = {0}\r\n", settings.Code));
                WriteDebug(String.Format("basis = {0}\r\n", settings.Basis));
                WriteDebug(String.Format("blockKeyLen = {0}\r\n", settings.BlockKeyLen));
                WriteDebug(String.Format("matrixKeyLen = {0}\r\n", settings.MatrixKeyLen));
                WriteDebug(String.Format("hashBlockLen = {0}\r\n", settings.HashBlockLen));
                WriteDebug(String.Format("\r\nstartSequence (hex): \r\n "));
                for (i = 0; i < startseqLen; i++)
                    WriteDebug(String.Format(" {0:X2}", startseq[i]));
                WriteDebug(String.Format("\r\n\r\nn = {0}\r\n", n));
                WriteDebug(String.Format("C_k = {0}\r\n", C_k));
                WriteDebug(String.Format("H_k = {0}\r\n", H_k));
                WriteDebug(String.Format("H_p = {0}\r\n", H_p));
                WriteDebug("\r\nd (hex): \r\n ");
                foreach (int v in d)
                    WriteDebug(String.Format(" {0:X2}", v));
                WriteDebug(String.Format("\r\n\r\nvariante = {0}\r\n", variante));
                WriteDebug(String.Format("Alpha = {0}\r\n", Alpha));
                WriteDebug(String.Format("Beta = {0}\r\n", Beta));
                WriteDebug(String.Format("Gamma = {0}\r\n", Gamma));
                WriteDebug(String.Format("Delta = {0}\r\n", Delta));
                WriteDebug(String.Format("Theta = {0}\r\n", Theta));
                WriteDebug("\r\ncm1 (hex): \r\n");
                for (i = 0; i < 256; )
                {
                    for (j = 0; j < 16; j++)
                    {
                        WriteDebug(String.Format(" {0:X2}", cm1[i]));
                        i++;
                    }
                    WriteDebug("\r\n");
                }
                if (settings.Perm == CypherMatrixSettings.Permutation.C)
                {
                    WriteDebug("\r\nperm: \r\n");
                    for (j = 0; j < 16; j++)
                    {
                        WriteDebug(String.Format(" {0}", perm[j]));
                    }
                    WriteDebug("\r\n");
                }

                WriteDebug("\r\ncm3 (hex): \r\n");
                for (i = 0; i < 256; )
                {
                    for (j = 0; j < 16; j++)
                    {
                        WriteDebug(String.Format(" {0:X2}", cm3[i]));
                        i++;
                    }
                    WriteDebug("\r\n");
                }
                WriteDebug("\r\ncipherChars (hex): \r\n ");
                foreach (byte b in cipherChars)
                    WriteDebug(String.Format(" {0:X2}", b));
                WriteDebug("\r\n\r\nblockKey (hex): \r\n ");
                foreach (byte b in blockKey)
                    WriteDebug(String.Format(" {0:X2}", b));
                WriteDebug("\r\n\r\nmatrixKey (hex): \r\n ");
                foreach (byte b in matrixKey)
                    WriteDebug(String.Format(" {0:X2}", b));
                WriteDebug("\r\n\r\n>>>>>>>>>> END OF ROUND <<<<<<<<<<\r\n\r\n\r\n");
            }
        }

        // Hashfunktion, step
        private ulong HashStep(byte[] data, int dataSize, int r)
        {
            ulong H_k = 0, H_p = 0;

            Generator(data, dataSize, r);

            int n = cm3.Length;
            int C_k = n * (n - 2) + settings.Code;

            for (int i = 1; i <= n; i++)
                H_k += ((uint)cm3[i - 1] + 1) * (ulong)(i + C_k + r);   // i-1, da das Array von 0 bis n-1 läuft, im Paper von 1 bis n

            for (uint i = 1; i <= n; i++)
            {
                H_p += (((ulong)cm3[i - 1] + 1) * i * H_k + (uint)(i + settings.Code + r));    // i-1, da das Array von 0 bis n-1 läuft, im Paper von 1 bis n; Erhöhung der Präzision durch cast auf long, wichtig!
            }

            return H_p;
        }

        private ulong Hash_SMX()
        {
            ulong hashPart = 0, hashSum = 0;
            int round = 1, bytesRead = 0;

            byte[] dataBlock = new byte[settings.HashBlockLen];

            while ((bytesRead = inputStreamReader.ReadFully(dataBlock)) > 0)
            {
                hashPart = HashStep(dataBlock, bytesRead, round);
                hashSum += hashPart;

                // Vorbereitungen für nächste Runde
                round++;
            }

            return hashSum;
        }

        private void Hash()
        {
            ulong hash = 0;
            switch (settings.HashMode)
            {
                case CypherMatrixSettings.CypherMatrixHashMode.SMX:
                    {
                        hash = Hash_SMX();
                        break;
                    }
                case CypherMatrixSettings.CypherMatrixHashMode.FMX:
                //{
                //    throw new NotImplementedException("NYI! At the moment only encryption and decryption are possible.");
                //    break;
                //}
                case CypherMatrixSettings.CypherMatrixHashMode.LCX:
                    {
                        throw new NotImplementedException("NYI! Please choose an other hash mode.");
                        //break;
                    }
                default:
                    {
                        throw new NotImplementedException("Unknown hash function!");
                    }
            }
            WriteOutput(hash);
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

        // function for changing back to the base 10, without errors
        static int BaseXToIntSafe(List<int> list, int start, int length, int Base)
        {
            if (list.Count < start + length)
            {
                if (list.Count < start)
                    return 0;   // start zeigt in nicht von list genutzten Speicher
                length = list.Count - start;    // Berechne length neu
            }
            return BaseXToInt(list, start, length, Base);
        }

        // function to filter certain chars
        private bool CharFilter(byte i)
        {
            return i < 33 || i == 34 || i == 44 || i == 176 || i == 177 || i == 178 || i == 213 || i == 219 || i == 220 || i == 221 || i == 222 || i == 223;
        }

        #endregion
    }
}
