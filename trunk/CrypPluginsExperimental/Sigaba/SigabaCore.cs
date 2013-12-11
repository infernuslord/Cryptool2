using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;


namespace Sigaba
{
    public class SigabaCore
    {

        private readonly Sigaba _facade;
        private readonly SigabaSettings _settings;
        private readonly SigabaPresentation _sigpa;

        public Rotor[] ControlRotors { get; set; }
        public Rotor[] CipherRotors { get; set; }
        public Rotor[] IndexRotors { get; set; }

        public Rotor[] CodeWheels { get; set; }

        public int[,] PresentationLetters = new int[5,20];

        public System.Timers.Timer aTimer = new System.Timers.Timer();

       
        public Boolean b2 = true;

        public SigabaCore(Sigaba facade, SigabaPresentation sigpa)
        {
            _sigpa = sigpa;
            _facade = facade;
            _settings = (SigabaSettings)_facade.Settings;

            CodeWheels = new Rotor[15];

            CipherRotors = new Rotor[5];
            ControlRotors = new Rotor[5];
            IndexRotors = new Rotor[5];
            

        }

        public object[] Encrypt(String cipher)
        {
            StringBuilder repeat = new StringBuilder();
            List<int[]> repeatList = new List<int[]>();

            foreach (char c in cipher)
            {
                //StringBuilder s = new StringBuilder();

                //repeat = String.Concat(repeat, (char)(Cipher(c - 65) + 65) + "");

                repeat.Append((char) (Cipher(c - 65) + 65));

                int[] controlarr = Control().Distinct().ToArray();

                repeatList.Add(controlarr);

                foreach (int i in Control().Distinct().ToArray())
                {
                    //s.Append(i);
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

            foreach (int[] intse in repeatList)
            {
                foreach (int i in intse)
                {
                    Console.Write(i+",");        
                }
                Console.Write("}, new int[] {");        
            }
            Console.WriteLine();
            UpdateSettings();



            return new object[] {repeat.ToString(), repeatList.ToArray()} ;
        }

        public string EncryptPresentation(String cipher)
        {
            Boolean b2 = true;
            _sigpa.stopPresentation = false;
            string repeat = "";

            _sigpa.SetCipher(cipher);

            List<int[]> repeatList = new List<int[]>();

            foreach (char c in cipher)
            {
                if(!b2 || _sigpa.stopPresentation)
                {
                    break;
                }
                string s = "";

                repeat = String.Concat(repeat, (char)(CipherPresentation(c - 65) + 65) + "");

                int[] controlarr = Control().Distinct().ToArray();

                repeatList.Add(controlarr);

                foreach (int i in ControlPresentation().Distinct().ToArray())
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

                _sigpa.FillPresentation(PresentationLetters);
                _sigpa.Callback = true;
                while(b2 && _sigpa.Callback)
                {

                }
                UpdateSettings();
                
            }

            foreach (int[] intse in repeatList)
            {
                foreach (int i in intse)
                {
                    Console.Write(i + ",");
                }
                Console.Write("}, new int[] {");
            }
            Console.WriteLine();
           
            return repeat;
        }

        private void UpdateSettings()
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();

            _settings.CipherKey = CipherRotors.Aggregate("", (current, r) => String.Concat(current, (char) (r.Position + 65)));
            _settings.ControlKey = ControlRotors.Reverse().Aggregate("", (current, r) => String.Concat(current, (char)(r.Position + 65)));
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

            ControlRotors[4].Position = _settings.ControlKey[0]-65; 
            ControlRotors[3].Position = _settings.ControlKey[1]-65;
            ControlRotors[2].Position = _settings.ControlKey[2]-65;
            ControlRotors[1].Position = _settings.ControlKey[3]-65;
            ControlRotors[0].Position = _settings.ControlKey[4]-65;
        }



        public void SetInternalConfig()
        {
            CipherRotors[0] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.CipherRotor1], _settings.CipherKey[0] - 65, _settings.CipherRotor1Reverse);
            CipherRotors[1] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.CipherRotor2], _settings.CipherKey[1] - 65, _settings.CipherRotor2Reverse);
            CipherRotors[2] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.CipherRotor3], _settings.CipherKey[2] - 65, _settings.CipherRotor3Reverse);
            CipherRotors[3] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.CipherRotor4], _settings.CipherKey[3] - 65, _settings.CipherRotor4Reverse);
            CipherRotors[4] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.CipherRotor5], _settings.CipherKey[4] - 65, _settings.CipherRotor5Reverse);

            ControlRotors[4] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.ControlRotor5], _settings.ControlKey[0] - 65, _settings.ControlRotor5Reverse);
            ControlRotors[3] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.ControlRotor4], _settings.ControlKey[1] - 65, _settings.ControlRotor4Reverse);
            ControlRotors[2] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.ControlRotor3], _settings.ControlKey[2] - 65, _settings.ControlRotor3Reverse);
            ControlRotors[1] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.ControlRotor2], _settings.ControlKey[3] - 65, _settings.ControlRotor2Reverse);
            ControlRotors[0] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.ControlRotor1], _settings.ControlKey[4] - 65, _settings.ControlRotor1Reverse);

            IndexRotors[0] = new Rotor(SigabaConstants.IndexRotors[_settings.IndexRotor1], _settings.IndexKey[0] - 48, _settings.IndexRotor1Reverse);
            IndexRotors[1] = new Rotor(SigabaConstants.IndexRotors[_settings.IndexRotor2], _settings.IndexKey[1] - 48, _settings.IndexRotor2Reverse);
            IndexRotors[2] = new Rotor(SigabaConstants.IndexRotors[_settings.IndexRotor3], _settings.IndexKey[2] - 48, _settings.IndexRotor3Reverse);
            IndexRotors[3] = new Rotor(SigabaConstants.IndexRotors[_settings.IndexRotor4], _settings.IndexKey[3] - 48, _settings.IndexRotor4Reverse);
            IndexRotors[4] = new Rotor(SigabaConstants.IndexRotors[_settings.IndexRotor5], _settings.IndexKey[4] - 48, _settings.IndexRotor5Reverse);
        }

        public int[] Control()
        {
            int tempf = 5;
            int tempg = 6;
            int temph = 7;
            int tempi = 8;

            foreach (var rotor in ControlRotors)
            {
                tempf = rotor.DeCiph(tempf);
                tempg = rotor.DeCiph(tempg);
                temph = rotor.DeCiph(temph);
                tempi = rotor.DeCiph(tempi);
            }

            tempf = SigabaConstants.Transform[_settings.Type][tempf];
            tempg = SigabaConstants.Transform[_settings.Type][tempg];
            temph = SigabaConstants.Transform[_settings.Type][temph];
            tempi = SigabaConstants.Transform[_settings.Type][tempi];

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

            tempf = SigabaConstants.Transform2[tempf];
            tempg = SigabaConstants.Transform2[tempg];
            temph = SigabaConstants.Transform2[temph];
            tempi = SigabaConstants.Transform2[tempi];

            int[] back = { tempf, tempg, temph, tempi };

            return back;
        }

        public int[] ControlPresentation()
        {
            int tempf = 'F' - 65;
            int tempg = 'G' - 65;
            int temph = 'H' - 65;
            int tempi = 'I' - 65;

            PresentationLetters[0,0] = tempf;
            PresentationLetters[1,0] = tempg;
            PresentationLetters[2,0] = temph;
            PresentationLetters[3,0] = tempi;

            for (int i = 0; i < ControlRotors.Length; i++)
            {
                PresentationLetters[0, i+1] = tempf = ControlRotors[i].DeCiph(tempf);
                PresentationLetters[1, i+1] = tempg = ControlRotors[i].DeCiph(tempg);
                PresentationLetters[2, i+1] = temph = ControlRotors[i].DeCiph(temph);
                PresentationLetters[3, i+1] = tempi = ControlRotors[i].DeCiph(tempi);
            }

            Console.WriteLine(tempf+"  "+ tempg + "  "+ temph + "  "+ tempi);

            PresentationLetters[0, ControlRotors.Length + 2] = tempf = SigabaConstants.Transform[_settings.Type][tempf];
            PresentationLetters[1, ControlRotors.Length + 2] = tempg = SigabaConstants.Transform[_settings.Type][tempg];
            PresentationLetters[2, ControlRotors.Length + 2] = temph = SigabaConstants.Transform[_settings.Type][temph];
            PresentationLetters[3, ControlRotors.Length + 2] = tempi = SigabaConstants.Transform[_settings.Type][tempi];
            Console.WriteLine(tempf + "  " + tempg + "  " + temph + "  " + tempi);
            for (int i = 0; i < IndexRotors.Length;i++ )
            {
                if (tempf != -1)
                    PresentationLetters[0, ControlRotors.Length + i + 3] = tempf = IndexRotors[i].Ciph(tempf);
                if (tempg != -1)
                    PresentationLetters[1, ControlRotors.Length + i + 3] = tempg = IndexRotors[i].Ciph(tempg);
                if (temph != -1)
                    PresentationLetters[2, ControlRotors.Length + i + 3] = temph = IndexRotors[i].Ciph(temph);
                if (tempi != -1)
                    PresentationLetters[3, ControlRotors.Length + i + 3] = tempi = IndexRotors[i].Ciph(tempi);
                Console.WriteLine(tempf + "  " + tempg + "  " + temph + "  " + tempi);
            }
            Console.WriteLine(tempf + "  " + tempg + "  " + temph + "  " + tempi);
            PresentationLetters[0, ControlRotors.Length + IndexRotors.Length + 4] = tempf = SigabaConstants.Transform2[tempf];
            PresentationLetters[1, ControlRotors.Length + IndexRotors.Length + 4] = tempg = SigabaConstants.Transform2[tempg];
            PresentationLetters[2, ControlRotors.Length + IndexRotors.Length + 4] = temph = SigabaConstants.Transform2[temph];
            PresentationLetters[3, ControlRotors.Length + IndexRotors.Length + 4] = tempi = SigabaConstants.Transform2[tempi];

            Console.WriteLine(PresentationLetters[0, 14] + "  " + PresentationLetters[1, 14] + "  " + PresentationLetters[2, 14] + "  " + PresentationLetters[3, 14]);

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
                temp = CipherRotors.Reverse().Aggregate(temp, (current, rotor) => rotor.DeCiph(current));
            }

            return temp;
        }

        public int CipherPresentation(int c)
        {
            int temp = c;

            

            /*if (_settings.Action == 0)
            {
                temp = CipherRotors.Aggregate(temp, (current, rotor) => rotor.Ciph(current));
            }
            if (_settings.Action == 1)
            {
                temp = CipherRotors.Reverse().Aggregate(temp, (current, rotor) => rotor.DeCiph(current));
            }*/

            if (_settings.Action == 0)
            {
                PresentationLetters[4, 5] = c;
                for (int i = 0; i < CipherRotors.Length; i++)
                {
                    PresentationLetters[4, CipherRotors.Length - i -1] = temp = CipherRotors[i].Ciph(temp);
                }
            }
            if (_settings.Action == 1)
            {
                PresentationLetters[4, 0] = c;
                for (int i = CipherRotors.Length - 1; i > -1; i--)
                {
                    PresentationLetters[4, i + 1] = temp = CipherRotors[i].DeCiph(temp);
                }
            }
            return temp;
        }

        public void stop()
        {
            b2 = false;
            _sigpa.Stop();
            

        }

        public void settings_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            _sigpa.St.SetSpeedRatio((double) 50 / _settings.PresentationSpeed);
            _sigpa.SpeedRatio = (double)50 / _settings.PresentationSpeed;
        }

    }
}
