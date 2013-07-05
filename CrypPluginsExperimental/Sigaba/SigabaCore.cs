using System;
using System.Linq;


namespace Sigaba
{
    class SigabaCore
    {

        private readonly Sigaba _facade;
        private readonly SigabaSettings _settings;

        private Rotor[] ControlRotors { get; set; }
        private Rotor[] CipherRotors { get; set; }
        private Rotor[] IndexRotors { get; set; }

        public SigabaCore(Sigaba facade)
        {
            _facade = facade;
            _settings = (SigabaSettings)_facade.Settings;
            CipherRotors = new Rotor[5];
            ControlRotors = new Rotor[5];
            IndexRotors = new Rotor[5];

        }

        public string Encrypt(String cipher)
        {
            string repeat = "";

            foreach (char c in cipher)
            {
                string s = "";

                repeat = String.Concat(repeat, (char)(Cipher(c - 65) + 65) + "");

                foreach (int i in Control().Distinct().ToArray())
                {
                    s = String.Concat(s, i);
                    CipherRotors[i].IncrementPosition();
                }

                if (ControlRotors[2].Position == 14)
                {
                    if (ControlRotors[1].Position == 14)
                    {
                        ControlRotors[3].IncrementPosition();
                    }
                    ControlRotors[1].IncrementPosition();
                }
                ControlRotors[2].IncrementPosition();
            }

            UpdateSettings();
            return repeat;
        }

        private void UpdateSettings()
        {
            _settings.CipherKey = CipherRotors.Aggregate("", (current, r) => String.Concat(current, (char) (r.Position + 65)));
            _settings.ControlKey = ControlRotors.Aggregate("", (current, r) => String.Concat(current, (char)(r.Position + 65)));
        }

        public void SetKeys()
        {
            CipherRotors[0].Position = _settings.CipherKey[0]-65;
            CipherRotors[1].Position = _settings.CipherKey[1]-65;
            CipherRotors[2].Position = _settings.CipherKey[2]-65;
            CipherRotors[3].Position = _settings.CipherKey[3]-65;
            CipherRotors[4].Position = _settings.CipherKey[4]-65;
            
            IndexRotors[0].Position = _settings.IndexKey[0]-48;
            IndexRotors[1].Position = _settings.IndexKey[1]-48;
            IndexRotors[2].Position = _settings.IndexKey[2]-48;
            IndexRotors[3].Position = _settings.IndexKey[3]-48;
            IndexRotors[4].Position = _settings.IndexKey[4]-48;

            ControlRotors[0].Position = _settings.ControlKey[0]-65; 
            ControlRotors[1].Position = _settings.ControlKey[1]-65;
            ControlRotors[2].Position = _settings.ControlKey[2]-65;
            ControlRotors[3].Position = _settings.ControlKey[3]-65;
            ControlRotors[4].Position = _settings.ControlKey[4]-65;
        }

        public void SetInternalConfig()
        {
            CipherRotors[0] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.CipherRotor1], _settings.CipherKey[0] - 65, _settings.CipherRotor1Reverse);
            CipherRotors[1] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.CipherRotor2], _settings.CipherKey[1] - 65, _settings.CipherRotor2Reverse);
            CipherRotors[2] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.CipherRotor3], _settings.CipherKey[2] - 65, _settings.CipherRotor3Reverse);
            CipherRotors[3] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.CipherRotor4], _settings.CipherKey[3] - 65, _settings.CipherRotor4Reverse);
            CipherRotors[4] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.CipherRotor5], _settings.CipherKey[4] - 65, _settings.CipherRotor5Reverse);

            ControlRotors[0] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.ControlRotor1], _settings.ControlKey[0] - 65, _settings.ControlRotor1Reverse);
            ControlRotors[1] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.ControlRotor2], _settings.ControlKey[1] - 65, _settings.ControlRotor2Reverse);
            ControlRotors[2] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.ControlRotor3], _settings.ControlKey[2] - 65, _settings.ControlRotor3Reverse);
            ControlRotors[3] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.ControlRotor4], _settings.ControlKey[3] - 65, _settings.ControlRotor4Reverse);
            ControlRotors[4] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.ControlRotor5], _settings.ControlKey[4] - 65, _settings.ControlRotor5Reverse);

            IndexRotors[0] = new Rotor(SigabaConstants.IndexRotors[_settings.IndexRotor1], _settings.IndexKey[0] - 48, _settings.IndexRotor1Reverse);
            IndexRotors[1] = new Rotor(SigabaConstants.IndexRotors[_settings.IndexRotor1], _settings.IndexKey[1] - 48, _settings.IndexRotor2Reverse);
            IndexRotors[2] = new Rotor(SigabaConstants.IndexRotors[_settings.IndexRotor1], _settings.IndexKey[2] - 48, _settings.IndexRotor3Reverse);
            IndexRotors[3] = new Rotor(SigabaConstants.IndexRotors[_settings.IndexRotor1], _settings.IndexKey[3] - 48, _settings.IndexRotor4Reverse);
            IndexRotors[4] = new Rotor(SigabaConstants.IndexRotors[_settings.IndexRotor1], _settings.IndexKey[4] - 48, _settings.IndexRotor5Reverse);
        }

        public int[] Control()
        {
            int tempf = 'F' - 65;
            int tempg = 'G' - 65;
            int temph = 'H' - 65;
            int tempi = 'I' - 65;

            foreach (var rotor in ControlRotors)
            {
                tempf = rotor.DeCiph(tempf);
                tempg = rotor.DeCiph(tempg);
                temph = rotor.DeCiph(temph);
                tempi = rotor.DeCiph(tempi);
            }

            tempf = SigabaConstants.Transform[tempf];
            tempg = SigabaConstants.Transform[tempg];
            temph = SigabaConstants.Transform[temph];
            tempi = SigabaConstants.Transform[tempi];

            

            foreach (var rotor in IndexRotors)
            {
                if (tempf != -1)
                    tempf = rotor.Ciph(tempf);
                if (tempg != -1)
                    tempg = rotor.Ciph(tempg);
                if (temph != -1)
                    temph = rotor.Ciph(temph);
                if (tempi != -1)
                    tempi = rotor.Ciph(tempi);
            }

            //   Console.WriteLine(tempf +""+ tempg +""+ temph + "" + tempi + "test");

            tempf = SigabaConstants.Transform2[tempf];
            tempg = SigabaConstants.Transform2[tempg];
            temph = SigabaConstants.Transform2[temph];
            tempi = SigabaConstants.Transform2[tempi];

            int[] back = { tempf, tempg, temph, tempi };

            return back;
        }

        public int Cipher(int c)
        {
            int temp = c;

            if (_settings.Action == 0)
            {
                temp = CipherRotors.Aggregate(temp, (current, rotor) => rotor.Ciph(current));
            }
            if (_settings.Action == 1)
            {
                /*for (int i = _cipherRotors.Count() - 1; i > -1; i--)
                {
                    temp = _cipherRotors[i].DeCiph(temp);
                }*/
                temp = CipherRotors.Reverse().Aggregate(temp, (current, rotor) => rotor.DeCiph(current));
            }

            return temp;
        }
    }
}
