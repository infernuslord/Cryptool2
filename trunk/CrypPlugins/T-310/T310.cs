/*
   Copyright 1995 - 2011 Jörg Drobick

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
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Windows.Controls;
using System.IO;

namespace Cryptool.Plugins.T310
{
    [Author("Jörg Drobick, Matthäus Wander", "ct2contact@cryptool.org", "", "")]
    [PluginInfo("T_310.Properties.Resources", false, "PluginCaption", "PluginTooltip", "T-310/DetailedDescription/doc.xml", "T-310/Images/t310.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class T310 : ICrypComponent
    {
        #region Private Variables

        private readonly T310Settings settings = new T310Settings();
        private Random rand = new Random();

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip", null)]
        public byte[] InputData
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", null, false, false, QuickWatchFormat.Hex, null)]
        public byte[] InputKey
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", null)]
        public byte[] OutputData
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputKeyCaption", "OutputKeyTooltip", null)]
        public byte[] OutputKey
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
            bool[] s1_bit, s2_bit;
            byte[] keyBinary;

            ProgressChanged(0, 1);

            if (InputKey != null && InputKey.Length > 0)
            {
                keyBinary = InputKey;
            }
            else
            {
                GuiLogMessage("No input key given, generating new one", NotificationLevel.Debug);
                keyBinary = generateKey();
            }

            if (!KeyCheck(keyBinary, out s1_bit, out s2_bit))
            {
                GuiLogMessage("Invalid key", NotificationLevel.Error);
                return;
            }

            OutputKey = keyBinary;
            OnPropertyChanged("OutputKey");

            if (settings.Mode == ModeEnum.Encrypt)
            {
                encrypt(InputData, ref s1_bit, ref s2_bit);
            }
            else
            {
                decrypt(InputData, ref s1_bit, ref s2_bit);
            }

            ProgressChanged(1, 1);
        }

        /* Schlüsselkarte erzeugen */
        private byte[] generateKey()
        {
            byte[] keyBinary = new byte[30];
            uint ui_Keycheck;

            do
            {
                keyBinary[0] = (byte)rand.Next(255);
                keyBinary[1] = (byte)rand.Next(255);
                keyBinary[2] = (byte)rand.Next(255);
                ui_Keycheck = (uint) keyBinary[2];
                ui_Keycheck <<= 8;
                ui_Keycheck += (uint) keyBinary[1];
                ui_Keycheck <<= 8;
                ui_Keycheck += (uint) keyBinary[0];
            } while (!KeyParity(ui_Keycheck));
            do
            {
                keyBinary[3] = (byte)rand.Next(255);
                keyBinary[4] = (byte)rand.Next(255);
                keyBinary[5] = (byte)rand.Next(255);
                ui_Keycheck = (uint)keyBinary[5];
                ui_Keycheck <<= 8;
                ui_Keycheck += (uint)keyBinary[4];
                ui_Keycheck <<= 8;
                ui_Keycheck += (uint)keyBinary[3];
            } while (!KeyParity(ui_Keycheck));
            do
            {
                keyBinary[6] = (byte)rand.Next(255);
                keyBinary[7] = (byte)rand.Next(255);
                keyBinary[8] = (byte)rand.Next(255);
                ui_Keycheck = (uint)keyBinary[8];
                ui_Keycheck <<= 8;
                ui_Keycheck += (uint)keyBinary[7];
                ui_Keycheck <<= 8;
                ui_Keycheck += (uint)keyBinary[6];
            } while (!KeyParity(ui_Keycheck));
            do
            {
                keyBinary[9] = (byte)rand.Next(255);
                keyBinary[10] = (byte)rand.Next(255);
                keyBinary[11] = (byte)rand.Next(255);
                ui_Keycheck = (uint)keyBinary[11];
                ui_Keycheck <<= 8;
                ui_Keycheck += (uint)keyBinary[10];
                ui_Keycheck <<= 8;
                ui_Keycheck += (uint)keyBinary[9];
            } while (!KeyParity(ui_Keycheck));
            do
            {
                keyBinary[12] = (byte)rand.Next(255);
                keyBinary[13] = (byte)rand.Next(255);
                keyBinary[14] = (byte)rand.Next(255);
                ui_Keycheck = (uint)keyBinary[14];
                ui_Keycheck <<= 8;
                ui_Keycheck += (uint)keyBinary[13];
                ui_Keycheck <<= 8;
                ui_Keycheck += (uint)keyBinary[12];
            } while (!KeyParity(ui_Keycheck));

            do
            {
                keyBinary[15] = (byte)rand.Next(255);
                keyBinary[16] = (byte)rand.Next(255);
                keyBinary[17] = (byte)rand.Next(255);
                ui_Keycheck = (uint)keyBinary[17];
                ui_Keycheck <<= 8;
                ui_Keycheck += (uint)keyBinary[16];
                ui_Keycheck <<= 8;
                ui_Keycheck += (uint)keyBinary[15];
            } while (!KeyParity(ui_Keycheck));
            do
            {
                keyBinary[18] = (byte)rand.Next(255);
                keyBinary[19] = (byte)rand.Next(255);
                keyBinary[20] = (byte)rand.Next(255);
                ui_Keycheck = (uint)keyBinary[20];
                ui_Keycheck <<= 8;
                ui_Keycheck += (uint)keyBinary[19];
                ui_Keycheck <<= 8;
                ui_Keycheck += (uint)keyBinary[18];
            } while (!KeyParity(ui_Keycheck));
            do
            {
                keyBinary[21] = (byte)rand.Next(255);
                keyBinary[22] = (byte)rand.Next(255);
                keyBinary[23] = (byte)rand.Next(255);
                ui_Keycheck = (uint)keyBinary[23];
                ui_Keycheck <<= 8;
                ui_Keycheck += (uint)keyBinary[22];
                ui_Keycheck <<= 8;
                ui_Keycheck += (uint)keyBinary[21];
            } while (!KeyParity(ui_Keycheck));
            do
            {
                keyBinary[24] = (byte)rand.Next(255);
                keyBinary[25] = (byte)rand.Next(255);
                keyBinary[26] = (byte)rand.Next(255);
                ui_Keycheck = (uint)keyBinary[26];
                ui_Keycheck <<= 8;
                ui_Keycheck += (uint)keyBinary[25];
                ui_Keycheck <<= 8;
                ui_Keycheck += (uint)keyBinary[24];
            } while (!KeyParity(ui_Keycheck));
            do
            {
                keyBinary[27] = (byte)rand.Next(255);
                keyBinary[28] = (byte)rand.Next(255);
                keyBinary[29] = (byte)rand.Next(255);
                ui_Keycheck = (uint)keyBinary[29];
                ui_Keycheck <<= 8;
                ui_Keycheck += (uint)keyBinary[28];
                ui_Keycheck <<= 8;
                ui_Keycheck += (uint)keyBinary[27];
            } while (!KeyParity(ui_Keycheck));

            return keyBinary;

//            string s_Kenngruppe = "";
//            for (byte b_FsKg = 0; b_FsKg < 5; ++b_FsKg)
//            {
//                byte b_FsRand = 0;
//                do
//                {
//                    b_FsRand = (byte)(0x1f & (byte)rand_myZufall.Next());
//                } while (b_FsRand > 0x18);
//                s_Kenngruppe += (char)(0x61 + b_FsRand);
//            }
//            mTB_Kenngruppe.Text = s_Kenngruppe;
        }

        private bool KeyCheck(byte[] keyBinary, out bool[] s1_bit, out bool[] s2_bit)
        {
            s1_bit = new bool[120];
            s2_bit = new bool[120];

            uint[] key1Quinary = new uint[5];
            uint[] key2Quinary = new uint[5];

            uint i_Keycheck = (uint) keyBinary[2];
            i_Keycheck <<= 8;
            i_Keycheck += (uint) keyBinary[1];
            i_Keycheck <<= 8;
            i_Keycheck += (uint) keyBinary[0];
            if (!KeyParity(i_Keycheck)) return false;
            key1Quinary[0] = i_Keycheck;
            // anzahl der 1 "H" ungerade!
            i_Keycheck = (uint) keyBinary[5];
            i_Keycheck <<= 8;
            i_Keycheck += (uint) keyBinary[4];
            i_Keycheck <<= 8;
            i_Keycheck += (uint) keyBinary[3];
            if (!KeyParity(i_Keycheck)) return false;
            // anzahl der 1 "H" ungerade!
            key1Quinary[1] = i_Keycheck;
            i_Keycheck = (uint) keyBinary[8];
            i_Keycheck <<= 8;
            i_Keycheck += (uint) keyBinary[7];
            i_Keycheck <<= 8;
            i_Keycheck += (uint) keyBinary[6];
            if (!KeyParity(i_Keycheck)) return false;
            // anzahl der 1 "H" ungerade!
            key1Quinary[2] = i_Keycheck;
            i_Keycheck = (uint) keyBinary[11];
            i_Keycheck <<= 8;
            i_Keycheck += (uint) keyBinary[10];
            i_Keycheck <<= 8;
            i_Keycheck += (uint) keyBinary[9];
            if (!KeyParity(i_Keycheck)) return false;
            // anzahl der 1 "H" ungerade!
            key1Quinary[3] = i_Keycheck;
            i_Keycheck = (uint) keyBinary[14];
            i_Keycheck <<= 8;
            i_Keycheck += (uint) keyBinary[13];
            i_Keycheck <<= 8;
            i_Keycheck += (uint) keyBinary[12];
            if (!KeyParity(i_Keycheck)) return false;
            // anzahl der 1 "H" ungerade!
            key1Quinary[4] = i_Keycheck;
            S_fuellen(ref s1_bit, key1Quinary[0], 1, 1);
            S_fuellen(ref s1_bit, key1Quinary[1], 1, 2);
            S_fuellen(ref s1_bit, key1Quinary[2], 1, 3);
            S_fuellen(ref s1_bit, key1Quinary[3], 1, 4);
            S_fuellen(ref s1_bit, key1Quinary[4], 1, 5);
            i_Keycheck = (uint) keyBinary[17];
            i_Keycheck <<= 8;
            i_Keycheck += (uint) keyBinary[16];
            i_Keycheck <<= 8;
            i_Keycheck += (uint) keyBinary[15];
            if (!KeyParity(i_Keycheck)) return false;
            // anzahl der 1 "H" ungerade!
            key2Quinary[0] = i_Keycheck;
            i_Keycheck = (uint) keyBinary[20];
            i_Keycheck <<= 8;
            i_Keycheck += (uint) keyBinary[19];
            i_Keycheck <<= 8;
            i_Keycheck += (uint) keyBinary[18];
            if (!KeyParity(i_Keycheck)) return false;
            // anzahl der 1 "H" ungerade!
            key2Quinary[1] = i_Keycheck;
            i_Keycheck = (uint) keyBinary[23];
            i_Keycheck <<= 8;
            i_Keycheck += (uint) keyBinary[22];
            i_Keycheck <<= 8;
            i_Keycheck += (uint) keyBinary[21];
            if (!KeyParity(i_Keycheck)) return false;
            // anzahl der 1 "H" ungerade!
            key2Quinary[2] = i_Keycheck;
            i_Keycheck = (uint) keyBinary[26];
            i_Keycheck <<= 8;
            i_Keycheck += (uint) keyBinary[25];
            i_Keycheck <<= 8;
            i_Keycheck += (uint) keyBinary[24];
            if (!KeyParity(i_Keycheck)) return false;
            // anzahl der 1 "H" ungerade!
            key2Quinary[3] = i_Keycheck;
            i_Keycheck = (uint) keyBinary[29];
            i_Keycheck <<= 8;
            i_Keycheck += (uint) keyBinary[28];
            i_Keycheck <<= 8;
            i_Keycheck += (uint) keyBinary[27];
            if (!KeyParity(i_Keycheck)) return false;
            // anzahl der 1 "H" ungerade!
            key2Quinary[4] = i_Keycheck;
            S_fuellen(ref s2_bit, key2Quinary[0], 1, 1);
            S_fuellen(ref s2_bit, key2Quinary[1], 1, 2);
            S_fuellen(ref s2_bit, key2Quinary[2], 1, 3);
            S_fuellen(ref s2_bit, key2Quinary[3], 1, 4);
            S_fuellen(ref s2_bit, key2Quinary[4], 1, 5);
//            if (mTB_Kenngruppe.Text.Length < 5) return false;
//            for (byte b_text = 0; b_text < 5; ++b_text)
//                if ((mTB_Kenngruppe.Text[b_text] > 0x7a) || (mTB_Kenngruppe.Text[b_text] < 0x61))
//                {                                     // wenn kleiner 'A' oder größer 'Z'
//                    mTB_Kenngruppe.Clear();
//                    return false;
//                }
            return true;
        }

        /* Key1-5, 6-10 ... ungerade */
        private bool KeyParity(uint i_TeilKey)
        {
            int i_shiftRegister = 1;                         // Maske
            bool bo_MaskSum = false;                         // speichern des Zwischenergebnisses

            for (byte b_count = 0; b_count < 24; ++b_count)  // die 24 bit des Schlüssels
            {
                if ((i_TeilKey & i_shiftRegister) > 0)      //Schlüsselbit AND Maske
                {
                    if (bo_MaskSum)                         // war das erbnis schon true?  ( XOR Funktion einmal anders )
                        bo_MaskSum = false;
                    else
                        bo_MaskSum = true;
                }
                i_shiftRegister <<= 1;                       // schieben der Maske in die entsprechende Position
            }
            return bo_MaskSum;
        }

        /* S1 und S2 Bitfeld füllen */
        private void S_fuellen(ref bool[] keyBits, uint ui_Teilschluessel, byte b_Snummer, byte b_Teilnr)
        {
            byte b_index;
            int i_temp;
            uint ui_schiebe = 1;

            for (b_index = 0; b_index < 24; ++b_index)
            {
                i_temp = (b_Teilnr - 1) * 24 + b_index;
                keyBits[i_temp] = (ui_Teilschluessel & ui_schiebe) > 0 ? true : false;
                ui_schiebe <<= 1;
            }
        }

        private void encrypt(byte[] by_array_eingabe, ref bool[] s1_bit, ref bool[] s2_bit)
        {
            using (MemoryStream streamAusgabe = new MemoryStream())
            {
                for (byte temp = 0; temp < 4; ++temp)
                {
                    streamAusgabe.WriteByte(0x19);    // 4*b
                }
                ulong ul_temp = Zufall(); //Speicher erste Zufallsfolge der Synchronfolge, Speicher Random-folge
                ulong ul_lese = ul_temp;
                for (byte temp = 0; temp < 13; ++temp)
                {
                    byte b_lese;
                    b_lese = (byte)(ul_lese & 0x1f); // nur das erste Byte lesen
                    streamAusgabe.WriteByte(b_lese); // speichern
                    ul_lese = ul_lese >> 5;          // nächsten 5 bit holen
                }
                ulong ul_syncronfolge = Syncronfolge(ul_temp); // Speicher Synchronfolge
                ul_lese = ul_syncronfolge;                             // hole Synchronfolge
                for (byte temp = 0; temp < 13; ++temp)
                {
                    byte b_lese;
                    b_lese = (byte)(ul_lese & 0x1f); // nur das erste Byte lesen
                    streamAusgabe.WriteByte(b_lese); // speichern
                    ul_lese = ul_lese >> 5;          // nächsten 5 bit holen
                }
                for (byte temp = 0; temp < 4; ++temp)
                {
                    streamAusgabe.WriteByte(0x0f);   // 4*k
                }
                bool[] bo_U_Vektor = UVektorInit(); // U-Vektor auf Anfang setzen!

                int int_zaehler = 0; // hier jetzt Klartext holen und Chiffrieren
                byte b_temp;
                while (int_zaehler < (by_array_eingabe.Length - 1))
                {
                    System.Threading.Thread.Sleep(10);
                    b_temp = (byte)((0xf8 & by_array_eingabe[int_zaehler]) >> 3);                   // byte No. 1
                    streamAusgabe.WriteByte(verschluesseln(Wurm(ref bo_U_Vektor, ref ul_syncronfolge, ref s1_bit, ref s2_bit), b_temp));  // bit 7 ... 3

                    b_temp = (byte)((0x07 & by_array_eingabe[int_zaehler]) << 2);                    // bit 2 ... 0 
                    ++int_zaehler;                                             // byte No. 2
                    if (int_zaehler == by_array_eingabe.Length) int_zaehler = by_array_eingabe.Length - 1;
                    b_temp |= (byte)(by_array_eingabe[int_zaehler] >> 6);                   // bit 7, 6
                    streamAusgabe.WriteByte(verschluesseln(Wurm(ref bo_U_Vektor, ref ul_syncronfolge, ref s1_bit, ref s2_bit), b_temp));

                    b_temp = (byte)((0x3e & by_array_eingabe[int_zaehler]) >> 1);
                    streamAusgabe.WriteByte(verschluesseln(Wurm(ref bo_U_Vektor, ref ul_syncronfolge, ref s1_bit, ref s2_bit), b_temp));

                    b_temp = (byte)((0x01 & by_array_eingabe[int_zaehler]) << 4);
                    ++int_zaehler;                                              // byte No. 3
                    if (int_zaehler == by_array_eingabe.Length) int_zaehler = by_array_eingabe.Length - 1;
                    b_temp |= (byte)((0xf0 & by_array_eingabe[int_zaehler]) >> 4);
                    streamAusgabe.WriteByte(verschluesseln(Wurm(ref bo_U_Vektor, ref ul_syncronfolge, ref s1_bit, ref s2_bit), b_temp));

                    b_temp = (byte)((0x0f & by_array_eingabe[int_zaehler]) << 1);
                    ++int_zaehler;                                             // byte 4;
                    if (int_zaehler == by_array_eingabe.Length) int_zaehler = by_array_eingabe.Length - 1;
                    b_temp |= (byte)(by_array_eingabe[int_zaehler] >> 7);
                    streamAusgabe.WriteByte(verschluesseln(Wurm(ref bo_U_Vektor, ref ul_syncronfolge, ref s1_bit, ref s2_bit), b_temp));

                    b_temp = (byte)((0x7c & by_array_eingabe[int_zaehler]) >> 2);
                    streamAusgabe.WriteByte(verschluesseln(Wurm(ref bo_U_Vektor, ref ul_syncronfolge, ref s1_bit, ref s2_bit), b_temp));

                    b_temp = (byte)((0x03 & by_array_eingabe[int_zaehler]) << 3);
                    ++int_zaehler;                                              // byte 5
                    if (int_zaehler == by_array_eingabe.Length) int_zaehler = by_array_eingabe.Length - 1;
                    b_temp |= (byte)((0xe0 & by_array_eingabe[int_zaehler]) >> 5);
                    streamAusgabe.WriteByte(verschluesseln(Wurm(ref bo_U_Vektor, ref ul_syncronfolge, ref s1_bit, ref s2_bit), b_temp));

                    b_temp = (byte)(0x1f & by_array_eingabe[int_zaehler]);
                    streamAusgabe.WriteByte(verschluesseln(Wurm(ref bo_U_Vektor, ref ul_syncronfolge, ref s1_bit, ref s2_bit), b_temp));
                    ++int_zaehler;                                              // byte 6 erhöhe für schleif
                    // Warten auf Anzeigenaktualisierung
                    // Application.DoEvents();
                    // Progressbar anzeigen
                    ProgressChanged(int_zaehler, by_array_eingabe.Length);
                }

                OutputData = streamAusgabe.ToArray();
                OnPropertyChanged("OutputData");
            }
        }

        private void decrypt(byte[] by_array_eingabe, ref bool[] s1_bit, ref bool[] s2_bit)
        {
            using (MemoryStream streamAusgabe = new MemoryStream())
            {
                ulong ul_startfolge = 0ul;
                ulong ul_syncronfolge = 0ul;

                if (! (by_array_eingabe.Length > 0x21 ) &&
                !((by_array_eingabe[0] == 0x19) & (by_array_eingabe[1] == 0x19) & (by_array_eingabe[2] == 0x19) &
                (by_array_eingabe[3] == 0x19) & (by_array_eingabe[0x1e] == 0x0f) & (by_array_eingabe[0x1f] == 0x0f) &
                (by_array_eingabe[0x20] == 0x0f) & (by_array_eingabe[0x21] == 0x0f)))
                {
                    GuiLogMessage("Can't decrypt, invalid input sequence", NotificationLevel.Error);

//                    pB_c.Visible = false;
//                    MessageBox.Show();
//                    bt_Loe_Click(sender, e);
//                    streamAusgabe.Close();
//                    button_action(true);

                    return;
                }

                for (byte temp = 16; temp > 3; --temp)
                {
                    ul_startfolge <<= 5; // 8;
                    ul_startfolge |= by_array_eingabe[temp];
                }
                for (byte temp = 29; temp > 16; --temp)
                {
                    ul_syncronfolge <<= 5;
                    ul_syncronfolge |= by_array_eingabe[temp];
                }
                if (ul_syncronfolge != Syncronfolge(ul_startfolge))
                {
                    GuiLogMessage("Can't decrypt, invalid input sequence", NotificationLevel.Error);

//                    pB_c.Visible = false;
//                    MessageBox.Show("Synchronfolge falsch");
//                    bt_Loe_Click(sender, e);
//                    streamAusgabe.Close();
//                    button_action(true);

                    return;
                }

                // pB_c.Visible = true;

                bool[] bo_U_Vektor = UVektorInit(); // U-Vektor auf Anfang setzen!

                int int_ent = 0x22; // hier jetzt Bytes holen und dechiffrieren
                byte b_temp0, b_temp1, b_temp2, b_temp3, b_temp4, b_temp5, b_temp6, b_temp7;
                while (int_ent < by_array_eingabe.Length - 1)
                {
                    b_temp0 = entschluesseln(Wurm(ref bo_U_Vektor, ref ul_syncronfolge, ref s1_bit, ref s2_bit), by_array_eingabe[int_ent]);
                    ++int_ent;
                    b_temp1 = entschluesseln(Wurm(ref bo_U_Vektor, ref ul_syncronfolge, ref s1_bit, ref s2_bit), by_array_eingabe[int_ent]);
                    ++int_ent;
                    b_temp2 = entschluesseln(Wurm(ref bo_U_Vektor, ref ul_syncronfolge, ref s1_bit, ref s2_bit), by_array_eingabe[int_ent]);
                    ++int_ent;
                    b_temp3 = entschluesseln(Wurm(ref bo_U_Vektor, ref ul_syncronfolge, ref s1_bit, ref s2_bit), by_array_eingabe[int_ent]);
                    ++int_ent;
                    b_temp4 = entschluesseln(Wurm(ref bo_U_Vektor, ref ul_syncronfolge, ref s1_bit, ref s2_bit), by_array_eingabe[int_ent]);
                    ++int_ent;
                    b_temp5 = entschluesseln(Wurm(ref bo_U_Vektor, ref ul_syncronfolge, ref s1_bit, ref s2_bit), by_array_eingabe[int_ent]);
                    ++int_ent;
                    b_temp6 = entschluesseln(Wurm(ref bo_U_Vektor, ref ul_syncronfolge, ref s1_bit, ref s2_bit), by_array_eingabe[int_ent]);
                    ++int_ent;
                    b_temp7 = entschluesseln(Wurm(ref bo_U_Vektor, ref ul_syncronfolge, ref s1_bit, ref s2_bit), by_array_eingabe[int_ent]);
                    ++int_ent;

                    // ProgressBar anzeigen
                    ProgressChanged(int_ent, by_array_eingabe.Length);

                    b_temp0 <<= 3;
                    b_temp0 |= (byte)((b_temp1 & 0x1c) >> 2); ;
                    streamAusgabe.WriteByte(b_temp0);  // erstes Byte

                    b_temp1 <<= 6;
                    b_temp1 |= (byte)((b_temp2 << 1) | ((b_temp3 & 0x10) >> 4));
                    streamAusgabe.WriteByte(b_temp1);  // zweites Byte

                    b_temp3 <<= 4;
                    b_temp3 |= (byte)((b_temp4 & 0x1e) >> 1);
                    streamAusgabe.WriteByte(b_temp3);  // drittes Byte

                    b_temp4 <<= 7;
                    b_temp4 |= (byte)((b_temp6 >> 3) | (b_temp5 << 2));
                    streamAusgabe.WriteByte(b_temp4);  // viertes Byte

                    b_temp6 <<= 5;
                    b_temp6 |= b_temp7;
                    streamAusgabe.WriteByte(b_temp6);  // fünftes Byte
                    //                            Application.DoEvents();
                }
                //                        bt_Loe_Click(sender, e);

                OutputData = streamAusgabe.ToArray();
                OnPropertyChanged("OutputData");
            }
        }

        private ulong Zufall()
        {
            ulong ul_kg = 0;                                    // variable Kenngruppe
            ulong ul_kg2 = 0;                                   // temporäre zweite Variable für Kenngruppe

            while (ul_kg == 0ul)
            {
                ul_kg = (ulong)rand.Next();           // die ersten 32 bit
                ul_kg *= 0x100000000ul;                        // Schift Left 32, "<<" nicht bei ULONG
                ul_kg2 = (ulong)rand.Next();          // die nächsten 32 bit
                ul_kg |= ul_kg2;                               // die letzten 32 bit
                ul_kg &= 0x1fffffffffffffff;                   // Maskieren auf 61 Bit
            }
            return ul_kg;
        }

        /* U-Vektor initialisieren, immer beim Starten des Chiffrierens und Dechiffrierens */
        private bool[] UVektorInit()
        {
            return new bool[37]{           // der U-Vektor ist die Startbedingung der 4bitigen schieberegister
            false, false,true,true,false, true,false,false,
            true, true,true,false,false, false,true,true, 
            true, true,true,false,false, true,false,false,
            false, false,true,false,true, true,false,true,
            false, false,false,true,true };     // U-Vektor x0110 1001 1100 0111 1100 1000 0101 1010 0011, Startbedinung, u[0] wird nicht verwendet
        } 

        /* Verschlüsselungsroutine der T310
         * Übergabe Wurmreihe (Additionsreihe)
         * Übergabe Klartext
         * Rückgramme Chiffrat, Geheimtexteinheit
         *******************************************/
        static byte verschluesseln(uint ui_key, byte b_klartext)
        {
            byte b_SRV2, b_SRV3; // Schritt 1 ... 5, Schieberegister aufbauen
            b_SRV2 = 0;
            b_SRV3 = 0x01f;
            int i_tempKey = (int)ui_key & 0x01f;  // bit 0 ... bit 4 == 1-5 des Schlüssels
            b_SRV2 = (byte)i_tempKey; // Schritt 1 ... 5, Schritt 6, Symetriewert bilden, rekursion
            while ((b_SRV2 != 0x1f) & (b_SRV2 != 0)) // wenn 11111, 0x1f ODER 0 abbrechen
            {
                b_SRV2 = Rekursion(b_SRV2);
                b_SRV3 = Rekursion(b_SRV3);
            }
            // Schritt 6  *****************************   Bit 6 bleibt ungenutzt!

            // Schritt 7 ... 11
            b_SRV2 = b_SRV3;                           // b_SRV3 nach b_SRV2 kopieren
            i_tempKey = (int)(ui_key & 0x07c0) >> 6;   // bit 6 ... 10 == 7-11 des keys
            b_SRV3 = (byte)i_tempKey;          // copy Bitt 7 ... 11 ins SRV3
            b_SRV3 ^= b_klartext;                      // Additionsreihe 7 ... 11 XOR Ktxt (5bit) 
            // Schritt 7 ... 11

            // Schritt 12  *****************************   Bit 12 bleibt ungenutzt!

            // Schritt 13
            while ((b_SRV2 != 0x1f) & (b_SRV2 != 0)) // wenn 11111 ( 0x1f oder 31 ) ODER 0 abbrechen
            {
                b_SRV2 = Rekursion(b_SRV2);
                b_SRV3 = Rekursion(b_SRV3);
            }
            // Schritt 13

            // Schritt 14; neues Zeichen holen und neue Schlüssel und beginn bei Schritt 1
            return b_SRV3;   // Ausgabe Chiffriertext 
        }

        /* Entschlüsselungroutine der T310, sie is kürzer als die Verschlüsselungsroutine
         * Übergabe Wurm-,Additionsreihe gebildet aus der Synchronisationseinheit
         * Übergabe Chiffrat, Geheimtexteinheit
         * Rückgabe Klartext
         *************************************************************************************/
        static byte entschluesseln(uint ui_key, byte b_crypt)
        {
            byte b_SRV2, b_SRV3;

            b_SRV3 = b_crypt;
            b_SRV2 = (byte)(ui_key & 0x1f);
            while ((b_SRV2 != 0x1f) & (b_SRV2 != 0))  // Rekursion der Additionsreihe in SRV2 und des GTX in SRV3
            {
                b_SRV2 = Rekursion(b_SRV2);
                b_SRV3 = Rekursion(b_SRV3);             // Z gebildet
            }
            b_SRV3 ^= (byte)((ui_key & 0x07c0) >> 6);   // (0x0f80)> 6; XOR Additionsreihe 7 ... 11 mit dem Zwischentext Z
            return b_SRV3;
        }

        /* Erzeuge Synchronfolge */
        private ulong Syncronfolge(ulong ul_Parameter)
        {
            bool bo_m2;                                                     // ergebnis des XOR bit 0,1,2,5

            for (int int_i = 1; int_i < 65; ++int_i) // Umformung durch Bit 0 xor 1 xor 2 xor 5
            {
                bo_m2 = ((ul_Parameter & 0x1ul) == 0ul ? false : true)
                ^ ((ul_Parameter & 0x2ul) == 0ul ? false : true)
                ^ ((ul_Parameter & 0x4ul) == 0ul ? false : true)
                ^ ((ul_Parameter & 0x20ul) == 0ul ? false : true);

                ul_Parameter >>= 1;                                              // Shift rechts 1, von 60 nach 0
                ul_Parameter |= bo_m2 == true ? 0x1000000000000000ul : 0x0ul;    // entspricht Set/Reset Bit
            }
            return ul_Parameter;
        }

        /* rotiere f-Folge */
        private void rot_Ffolge(ref ulong ul_syncronfolge)
        {
            bool bo_m2;                                                     // ergebnis des XOR bit 0,1,2,5

            // Umformung durch Bit 0 xor 1 xor 2 xor 5

            bo_m2 = ((ul_syncronfolge & 0x1ul) == 0ul ? false : true)
            ^ ((ul_syncronfolge & 0x2ul) == 0ul ? false : true)
            ^ ((ul_syncronfolge & 0x4ul) == 0ul ? false : true)
            ^ ((ul_syncronfolge & 0x20ul) == 0ul ? false : true);

            ul_syncronfolge >>= 1;                                              // Shift rechts 1, von 60 nach 0
            ul_syncronfolge |= bo_m2 == true ? 0x1000000000000000ul : 0x0ul;    // entspricht Set/Reset Bit
        }

        /* Rekursionsregister 5 bit realisiert
         * Übergabe Registerwert
         * Rückgabe Rekursionsergebniss
         ***********************************************/
        static byte Rekursion(byte b_Register)
        {
            const byte b_Bit3 = 0x04;
            const byte b_Bit5 = 0x10;
            const byte b_5Bitmaske = 0x1f; // Bitmaske aller werte innerhalb von 0 ... 31

            Boolean bo_xor = false;        // ergebnis des bitweisen XOR  

            byte b_ret = b_Register;       // übergabe der Eingabe an das Ausgabebyte

            bo_xor = ((b_Bit3 & b_ret) == 0 ? false : true) ^ ((b_Bit5 & b_ret) == 0 ? false : true);
            b_ret <<= 1;                   // bo_XOR bildet rekusrionsbit aus Bit 1 und Bit 5
            b_ret |= (byte)(bo_xor == true ? 0x01 : 0);
            b_ret &= b_5Bitmaske;
            return b_ret;
        }

        /* Schlüsselgenerator ablauf, Schrittkette, Automat, Batch, im Start-Stopp Betrieb */
        private uint Wurm(ref bool[] bo_U_Vektor, ref ulong ul_syncronfolge, ref bool[] s1_bit, ref bool[] s2_bit)
        {
            byte b_fsBit = 0;
            uint ui_wurmBit = 0;
            uint ui_wurm = 0;
            byte b_zeiger = 0;
            bool[] bo_T = new bool[9]; // Bool Array der T-Funktion

            while (b_fsBit < 13)
            {   // 5 bit sammeln aus 127ten runde * bit des Fs Zeichen, 127 - 254 - 381 - 508 - 635 -neues FsZeichen 762 ...
                for (byte b_runde = 0; b_runde < 127; ++b_runde)
                {
                    // S1 und S2 Schieben
                    bool bo_tempS1 = s1_bit[b_zeiger];
                    bool bo_tempS2 = s2_bit[b_zeiger];


                    // T-Funktion mit LSZ 31
                    bo_T[0] = (ul_syncronfolge & 0x01) == 1 ? true : false;  // > 0 ?; hole Bit 0 aus der synchronfolge
                    bo_T[1] = bo_T[0] ^ Z(bo_tempS2, bo_U_Vektor[7], bo_U_Vektor[4], bo_U_Vektor[33], bo_U_Vektor[30], bo_U_Vektor[18]);
                    bo_T[2] = bo_T[1] ^ bo_U_Vektor[36];
                    bo_T[3] = bo_T[2] ^ Z(bo_U_Vektor[5], bo_U_Vektor[35], bo_U_Vektor[9], bo_U_Vektor[16], bo_U_Vektor[23], bo_U_Vektor[26]);
                    bo_T[4] = bo_T[3] ^ bo_U_Vektor[32];
                    bo_T[5] = bo_T[4] ^ bo_U_Vektor[5] ^ Z(bo_U_Vektor[12], bo_U_Vektor[21], bo_U_Vektor[1], bo_U_Vektor[13], bo_U_Vektor[25], bo_U_Vektor[20]);
                    bo_T[6] = bo_T[5] ^ bo_U_Vektor[8];
                    bo_T[7] = bo_T[6] ^ Z(bo_U_Vektor[24], bo_U_Vektor[15], bo_U_Vektor[22], bo_U_Vektor[29], bo_U_Vektor[10], bo_U_Vektor[28]);
                    bo_T[8] = bo_T[7] ^ bo_U_Vektor[6];

                    // U-Vektoren schieben
                    bo_U_Vektor[36] = bo_U_Vektor[35];
                    bo_U_Vektor[35] = bo_U_Vektor[34];
                    bo_U_Vektor[34] = bo_U_Vektor[33];
                    bo_U_Vektor[33] = bo_U_Vektor[0] ^ bo_U_Vektor[19]; //  UD9;

                    bo_U_Vektor[32] = bo_U_Vektor[31];
                    bo_U_Vektor[31] = bo_U_Vektor[30];
                    bo_U_Vektor[30] = bo_U_Vektor[29];
                    bo_U_Vektor[29] = bo_U_Vektor[1] ^ bo_U_Vektor[35]; // UD8;

                    bo_U_Vektor[28] = bo_U_Vektor[27];
                    bo_U_Vektor[27] = bo_U_Vektor[26];
                    bo_U_Vektor[26] = bo_U_Vektor[25];
                    bo_U_Vektor[25] = bo_U_Vektor[2] ^ bo_U_Vektor[31]; //  UD7;

                    bo_U_Vektor[24] = bo_U_Vektor[23];
                    bo_U_Vektor[23] = bo_U_Vektor[22];
                    bo_U_Vektor[22] = bo_U_Vektor[21];
                    bo_U_Vektor[21] = bo_U_Vektor[3] ^ bo_U_Vektor[27]; //  UD6;

                    bo_U_Vektor[20] = bo_U_Vektor[19];
                    bo_U_Vektor[19] = bo_U_Vektor[18];
                    bo_U_Vektor[18] = bo_U_Vektor[17];
                    bo_U_Vektor[17] = bo_U_Vektor[4] ^ bo_U_Vektor[11]; // UD5;

                    bo_U_Vektor[16] = bo_U_Vektor[15];
                    bo_U_Vektor[15] = bo_U_Vektor[14];
                    bo_U_Vektor[14] = bo_U_Vektor[13];
                    bo_U_Vektor[13] = bo_U_Vektor[5] ^ bo_U_Vektor[23]; // UD4;

                    bo_U_Vektor[12] = bo_U_Vektor[11];
                    bo_U_Vektor[11] = bo_U_Vektor[10];
                    bo_U_Vektor[10] = bo_U_Vektor[9];
                    bo_U_Vektor[9] = bo_U_Vektor[6] ^ bo_U_Vektor[3];  // UD3;

                    bo_U_Vektor[8] = bo_U_Vektor[7];
                    bo_U_Vektor[7] = bo_U_Vektor[6];
                    bo_U_Vektor[6] = bo_U_Vektor[5];
                    bo_U_Vektor[5] = bo_U_Vektor[7] ^ bo_U_Vektor[15]; // UD2;

                    bo_U_Vektor[4] = bo_U_Vektor[3];
                    bo_U_Vektor[3] = bo_U_Vektor[2];
                    bo_U_Vektor[2] = bo_U_Vektor[1];
                    bo_U_Vektor[1] = bo_T[8] ^ bo_tempS1; // UD1 D = 0, jetzt Kommutatorkarte abbilden 29 Eingänge > 58 Ausgänge

                    // Zeiger auf nächstes Bit
                    ++b_zeiger;
                    if (b_zeiger > 119) b_zeiger = 0;   // Zeiger entspricht das rotieren des S1, S2 schlüssels
                    rot_Ffolge(ref ul_syncronfolge); // rotiere F-Folge die Syncronfolge

                } // 127 Runden!
                ui_wurm = (uint)(bo_U_Vektor[23] == true ? 0x01 : 0x00);
                ui_wurm <<= b_fsBit; // schiebe ergebnis in die entsprechende Bitstelle des FsBytes, Bit 0 bis bit 12
                ui_wurmBit |= ui_wurm;      // Maskiere und setze gebildetes bit
                ++b_fsBit;                    // nächstes bit
            }
            ui_wurmBit &= 0x1fff;         // Maskieren
            return ui_wurmBit; // Rückgabe 13 bit Wurm
        }

        /* Z-Funktion */
        private bool Z(bool b_1, bool b_2, bool b_3, bool b_4, bool b_5, bool b_6)
        {
            bool bo_temp = true ^ b_1 ^ b_5 ^ b_6 ^ (b_1 && b_4);
            bo_temp ^= (b_2 && b_3) ^ (b_2 && b_5) ^ (b_4 && b_5) ^ (b_5 && b_6);
            bo_temp ^= (b_1 && b_3 && b_4) ^ (b_1 && b_3 && b_6) ^ (b_1 && b_4 && b_5);
            bo_temp ^= (b_2 && b_3 && b_6) ^ (b_2 && b_4 && b_6) ^ (b_3 && b_5 && b_6);
            bo_temp ^= (b_1 && b_2 && b_3 && b_4) ^ (b_1 && b_2 && b_3 && b_5);
            bo_temp ^= (b_1 && b_2 && b_5 && b_6) ^ (b_2 && b_3 && b_4 && b_6);
            bo_temp ^= (b_1 && b_2 && b_3 && b_4 && b_5) ^ (b_1 && b_3 && b_4 && b_5 && b_6);
            return (bo_temp);
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
    }
}
