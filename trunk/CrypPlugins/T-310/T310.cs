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
using System.Windows.Controls;
using System.IO;

namespace Cryptool.Plugins.T310
{
    [Author("Jörg Drobick, Matthäus Wander", "ct2contact@cryptool.org", "", "")]
    [PluginInfo("T_310.Properties.Resources", "PluginCaption", "PluginTooltip", "T-310/DetailedDescription/doc.xml", "T-310/Images/t310.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class T310 : ICrypComponent
    {
        #region Private Variables

        private readonly T310Settings settings = new T310Settings();
        private Random rand = new Random();

        // Zeitschlüssel, in der T-310 per Lochkarte eingelesen
        private bool[] S1 = new bool[120];
        private bool[] S2 = new bool[120];

        // Langzeitschlüssel 31, LZ-31
        int[] D = new int[] { 0, 15, 3, 23, 11, 27, 31, 35, 19 };
        int[] P = new int[] { 7, 4, 33, 30, 18, 36, 5, 35, 9, 16, 23, 26, 32, 12, 21, 1, 13, 25, 20, 8, 24, 15, 22, 29, 10, 28, 6 };
        int alpha = 23;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputDataCaption", "InputDataTooltip")]
        public byte[] InputData
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", false)]
        public byte[] InputKey
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip")]
        public byte[] OutputData
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputKeyCaption", "OutputKeyTooltip")]
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

            if (!KeyCheck(keyBinary))
            {
                GuiLogMessage("Invalid key", NotificationLevel.Error);
                return;
            }

            OutputKey = keyBinary;
            OnPropertyChanged("OutputKey");

            if (settings.Mode == ModeEnum.Encrypt)
            {
                encrypt(InputData);
            }
            else
            {
                decrypt(InputData);
            }

            ProgressChanged(1, 1);
        }

        /* Schlüsselkarte erzeugen */
        private byte[] generateKey()
        {
            byte[] keyBinary = new byte[30];

            rand.NextBytes(keyBinary);

            for (int i = 0; i < 30; i += 3)
                if (!KeyParity(keyBinary, i)) keyBinary[i] ^= 1;

            return keyBinary;
        }

        private bool KeyCheck(byte[] keyBinary)
        {
            for (int i = 0; i < 30; i += 3)
                if (!KeyParity(keyBinary,i)) return false;

            S_fuellen(ref S1, keyBinary, 0);
            S_fuellen(ref S2, keyBinary, 15);

            return true;
        }

        private bool KeyParity(byte[] b, int i )
        {
            return ByteParity(b[i]) ^ ByteParity(b[i+1]) ^ ByteParity(b[i+2]);
        }

        private bool ByteParity( byte b )
        {
            uint i = b;
            i ^= (i >> 4);
            i ^= (i >> 2);
            i ^= (i >> 1);
            return ((i&1)>0);
        }

        /* S1 und S2 Bitfeld füllen */
        private void S_fuellen(ref bool[] keyBits, byte[] keyBinary, int ofs)
        {
            int k = 0;
            for (int i = 0; i < 15; i++)
            {
                byte b = keyBinary[ofs + i];
                for (int j = 0; j < 8; j++)
                {
                    keyBits[k++] = ((b & 1) > 0);
                    b >>= 1;
                }
            }
        }

        private byte[] map8to5( byte[] b )
        {
            byte[] res = new byte[5];

            res[0] = (byte)((b[0] << 3) | (b[1] >> 2));
            res[1] = (byte)((b[1] << 6) | (b[2] << 1) | (b[3] >> 4));
            res[2] = (byte)((b[3] << 4) | (b[4] >> 1));
            res[3] = (byte)((b[4] << 7) | (b[6] >> 3) | (b[5] << 2));
            res[4] = (byte)((b[6] << 5) | b[7] );

            return res;
        }
        
        private byte[] map5to8( byte[] b, int i )
        {
            byte[] res = new byte[8];

            res[0] = (byte)((b[i] >> 3) & 0x1f);

            res[1] = (byte)((0x07 & b[i]) << 2);
            if (i < b.Length - 1) i++;
            res[1] |= (byte)(b[i] >> 6);           

            res[2] = (byte)((0x3e & b[i]) >> 1);

            res[3] = (byte)((0x01 & b[i]) << 4);
            if (i < b.Length - 1) i++;
            res[3] |= (byte)((0xf0 & b[i]) >> 4);

            res[4] = (byte)((0x0f & b[i]) << 1);
            if (i < b.Length - 1) i++;
            res[4] |= (byte)(b[i] >> 7);

            res[5] = (byte)((0x7c & b[i]) >> 2);

            res[6] = (byte)((0x03 & b[i]) << 3);
            if (i < b.Length - 1) i++;
            res[6] |= (byte)((0xe0 & b[i]) >> 5);

            res[7] = (byte)(b[i] & 0x1f);

            return res;
        }
        
        private void encrypt(byte[] by_array_eingabe)
        {
            using (MemoryStream streamAusgabe = new MemoryStream())
            {
                // Header mit 61-Bit-Initialisierungsvektor und dessen Iteration schreiben
                // "bbbb" synchronfolge_init (13 bytes) synchronfolge (13 bytes) "kkkk" ciphertext

                ulong synchronfolge_init = Zufall();
                ulong synchronfolge = Synchronfolge(synchronfolge_init);

                for (int i = 0; i < 4; i ++)
                    streamAusgabe.WriteByte(0x19);  // bbbb

                for (int i = 0; i < 61; i += 5)
                    streamAusgabe.WriteByte((byte)((synchronfolge_init >> i) & 0x1f));

                for (int i = 0; i < 61; i += 5)
                    streamAusgabe.WriteByte((byte)((synchronfolge >> i) & 0x1f));

                for (int i = 0; i < 4; i++)
                    streamAusgabe.WriteByte(0x0f);  // kkkk

                bool[] bo_U_Vektor = UVektorInit(); // U-Vektor auf Anfang setzen!

                // Ciphertext erzeugen und schreiben

                for (int i = 0; i < by_array_eingabe.Length - 1; i += 5 )
                {
                    byte[] b_out = map5to8(by_array_eingabe, i);

                    for (int j = 0; j < 8; j++)
                        streamAusgabe.WriteByte(verschluesseln(Wurm(ref bo_U_Vektor, ref synchronfolge), b_out[j]));

                    ProgressChanged(i, by_array_eingabe.Length - 1);
                }

                OutputData = streamAusgabe.ToArray();
                OnPropertyChanged("OutputData");
            }
        }

        private void decrypt(byte[] by_array_eingabe)
        {
            using (MemoryStream streamAusgabe = new MemoryStream())
            {
                // Header lesen und auf Korrektheit prüfen

                if (! (by_array_eingabe.Length > 0x21 ) &&
                    !((by_array_eingabe[0] == 0x19) & (by_array_eingabe[1] == 0x19) & (by_array_eingabe[2] == 0x19) & (by_array_eingabe[3] == 0x19) &
                    (by_array_eingabe[0x1e] == 0x0f) & (by_array_eingabe[0x1f] == 0x0f) &(by_array_eingabe[0x20] == 0x0f) & (by_array_eingabe[0x21] == 0x0f)))
                {
                    GuiLogMessage("Can't decrypt, invalid input sequence", NotificationLevel.Error);
                    return;
                }

                ulong synchronfolge_init = 0;
                for (int i = 16; i > 3; i--)
                    synchronfolge_init = (synchronfolge_init << 5) | by_array_eingabe[i];

                ulong synchronfolge = 0;
                for (int i = 29; i > 16; i--)
                    synchronfolge = (synchronfolge << 5) | by_array_eingabe[i];

                if (synchronfolge != Synchronfolge(synchronfolge_init))
                {
                    GuiLogMessage("Can't decrypt, invalid input sequence", NotificationLevel.Error);
                    return;
                }

                bool[] bo_U_Vektor = UVektorInit(); // U-Vektor auf Anfang setzen!

                byte[] b_in = new byte[8];
                for (int i = 0x22; i < by_array_eingabe.Length - 1; i+=8)   // hier jetzt Bytes holen und dechiffrieren
                {
                    for (int j = 0; j < 8; j++)
                        b_in[j] = entschluesseln(Wurm(ref bo_U_Vektor, ref synchronfolge), by_array_eingabe[i+j]);

                    streamAusgabe.Write(map8to5(b_in), 0, 5);

                    ProgressChanged(i, by_array_eingabe.Length-1);
                }

                OutputData = streamAusgabe.ToArray();
                OnPropertyChanged("OutputData");
            }
        }

        private ulong Zufall()
        {
           ulong r;
           byte[] bytes = new byte[8];

           while(true)
           {
               rand.NextBytes(bytes);
               r = BitConverter.ToUInt64(bytes, 0) & 0x1fffffffffffffff;    // erzeuge 61 Zufallsbits
               if (r != 0ul) return r;
           }
        }

        /* U-Vektor initialisieren, immer beim Starten des Chiffrierens und Dechiffrierens */
        private bool[] UVektorInit()
        {
            return new bool[37] {           // der U-Vektor ist die Startbedingung der 4bitigen Schieberegister
                false, false,true,true,false, true,false,false,
                true, true,true,false,false, false,true,true, 
                true, true,true,false,false, true,false,false,
                false, false,true,false,true, true,false,true,
                false, false,false,true,true };     // U-Vektor x0110 1001 1100 0111 1100 1000 0101 1010 0011, Startbedingung, u[0] wird nicht verwendet
        } 

        /* Verschlüsselungsroutine der T310
         * Übergabe Wurmreihe (Additionsreihe)
         * Übergabe Klartext
         * Rückgramme Chiffrat, Geheimtexteinheit
         *******************************************/
        static byte verschluesseln(uint ui_key, byte b_klartext)
        {
            byte b_SRV2 = (byte)(ui_key & 0x1f); // Schritt 1 ... 5, Schritt 6, Symmetriewert bilden, Rekursion
            byte b_SRV3 = 0x01f;

            while ((b_SRV2 != 0x1f) & (b_SRV2 != 0)) // wenn 11111, 0x1f ODER 0 abbrechen
            {
                b_SRV2 = Rekursion(b_SRV2);
                b_SRV3 = Rekursion(b_SRV3);
            }
            // Schritt 6  *****************************   Bit 6 bleibt ungenutzt!

            // Schritt 7 ... 11
            b_SRV2 = b_SRV3;                           // b_SRV3 nach b_SRV2 kopieren
            b_SRV3 = (byte)((ui_key >> 6) & 0x1f);          // copy Bitt 7 ... 11 ins SRV3
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

        /* Entschlüsselungroutine der T310, sie ist kürzer als die Verschlüsselungsroutine
         * Übergabe Wurm-,Additionsreihe gebildet aus der Synchronisationseinheit
         * Übergabe Chiffrat, Geheimtexteinheit
         * Rückgabe Klartext
         *************************************************************************************/
        static byte entschluesseln(uint a, byte cipher)
        {
            byte b_SRV2 = (byte)(a & 0x1f);
            byte b = (byte)((a >> 6) & 0x1f);

            while ((b_SRV2 != 0x1f) & (b_SRV2 != 0))  // Rekursion der Additionsreihe in SRV2 und des GTX in SRV3
            {
                b_SRV2 = Rekursion(b_SRV2);
                cipher = Rekursion(cipher);             // Z gebildet
            }

            return (byte)(cipher ^ b);
        }

        /* Erzeuge Synchronfolge */
	    private ulong Synchronfolge(ulong f)
        {
            for (int i = 0; i < 64; i++)
                f = rot_Ffolge(f);

	        return f;
	    }
	    
        /* rotiere f-Folge */
	    private ulong rot_Ffolge(ulong f)
	    {
            ulong bit = (f ^ (f>>1) ^ (f>>2) ^ (f>>5)) & 1;
            return (f >> 1) | (bit << 60);
	    }

        /* Rekursionsregister 5 bit realisiert
         * Übergabe Registerwert
         * Rückgabe Rekursionsergebniss
         ***********************************************/
        static byte Rekursion(byte b_Register)
        {
            int b = b_Register;
            return (byte)(((b<<1) | ((b>>4)^(b>>2)) & 1) & 0x1f);
        }

        /* Schlüsselgenerator Ablauf, Schrittkette, Automat, Batch, im Start-Stopp Betrieb */
        private uint Wurm(ref bool[] U, ref ulong ul_synchronfolge)
	    {
	        uint ui_wurmBit = 0;
	        int b_zeiger = 0;
	        bool[] T = new bool[10]; // Bool Array der T-Funktion
            bool[] Uold = new bool[U.Length];

            for (byte b_fsBit = 0; b_fsBit < 13; b_fsBit++ )
            {   // 5 bit sammeln aus 127ten runde * bit des Fs Zeichen, 127 - 254 - 381 - 508 - 635 -neues FsZeichen 762 ...
                for (byte b_runde = 0; b_runde < 127; b_runde++)
                { // S1 und S2 Schieben                                 
                    T[1] = ((ul_synchronfolge & 1) == 1);  // hole Bit 0 aus der synchronfolge, T1 == f
                    T[2] = T[1] ^ Z(S2[b_zeiger], U[P[0]], U[P[1]], U[P[2]], U[P[3]], U[P[4]]);
                    T[3] = T[2] ^ U[P[5]];
                    T[4] = T[3] ^ Z(U[P[6]], U[P[7]], U[P[8]], U[P[9]], U[P[10]], U[P[11]]);
                    T[5] = T[4] ^ U[P[12]];
                    T[6] = T[5] ^ S2[b_zeiger] ^ Z(U[P[13]], U[P[14]], U[P[15]], U[P[16]], U[P[17]], U[P[18]]);
                    T[7] = T[6] ^ U[P[19]];
                    T[8] = T[7] ^ Z(U[P[20]], U[P[21]], U[P[22]], U[P[23]], U[P[24]], U[P[25]]);
                    T[9] = T[8] ^ U[P[26]];

                    //Beachtung des zeitlichen Ablaufes der elektronischen Schaltung!
                    U.CopyTo(Uold, 0);
                    Uold[0] = S1[b_zeiger];

                    // U-Vektoren schieben
                    for (int j=1; j<=9; j++)
                    {
                        U[4 * j - 3] = Uold[D[j - 1]] ^ T[10 - j];
                        U[4 * j - 2] = Uold[4 * j - 3];
                        U[4 * j - 1] = Uold[4 * j - 2];
                        U[4 * j    ] = Uold[4 * j - 1];
                    }         

                    b_zeiger = (b_zeiger + 1) % 120;    // Zeiger auf nächstes Bit, entspricht der Rotiation des S1-, S2-Schlüssels
                    ul_synchronfolge = rot_Ffolge(ul_synchronfolge); // rotiere F-Folge die Synchronfolge einmal
                }                                         // 127 Runden!

                ui_wurmBit |= (U[alpha] ? 1u : 0u) << b_fsBit;
            }

	        return ui_wurmBit & 0x1ffff;                  // Rückgabe 13 bit Wurm
	    }


        /* Z-Funktion */
        private bool Z(bool b_1, bool b_2, bool b_3, bool b_4, bool b_5, bool b_6)
        {
            bool res = true
                ^ b_1 ^ b_5 ^ b_6 
                ^ (b_1 & b_4) ^ (b_2 & b_3) ^ (b_2 & b_5) ^ (b_4 & b_5) ^ (b_5 & b_6)
                ^ (b_1 & b_3 & b_4) ^ (b_1 & b_3 & b_6) ^ (b_1 & b_4 & b_5) ^ (b_2 & b_3 & b_6) ^ (b_2 & b_4 & b_6) ^ (b_3 & b_5 & b_6)
                ^ (b_1 & b_2 & b_3 & b_4) ^ (b_1 & b_2 & b_3 & b_5) ^ (b_1 & b_2 & b_5 & b_6) ^ (b_2 & b_3 & b_4 & b_6)
                ^ (b_1 & b_2 & b_3 & b_4 & b_5) ^ (b_1 & b_3 & b_4 & b_5 & b_6);
            return res;
        }

        public void PostExecution()
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
