using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Cryptool;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.Miscellaneous;


namespace ClassLibrary1
{

    [Author("Daniel Kohnen", "kohnen@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "Transposition", "", "", "Transposition/icon.PNG")]
    [EncryptionType(EncryptionType.Classic)]
    public class Transposition : IEncryption
    {
        # region Private variables

        private String keyword;
        private String input;
        private String output;
        private TranspositionSettings settings;

        # endregion

        public Transposition()
        {
            this.settings = new TranspositionSettings();
            keyword = "";
            input = "";
            output = "";
        }

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (TranspositionSettings)value; }
        }

        # region Properties

        [PropertyInfo(Direction.InputData, "Input", "input", "", DisplayLevel.Beginner)]
        public string Input
        {
            get
            {
                return this.input;
            }

            set
            {
                this.input = value;
                OnPropertyChange("Input");
            }
        }

        [PropertyInfo(Direction.InputData, "Keyword", "keyword", "", DisplayLevel.Beginner)]
        public string Keyword
        {
            get
            {
                return this.keyword;
            }

            set
            {
                this.keyword = value;
                OnPropertyChange("Keyword");
            }
        }

        [PropertyInfo(Direction.OutputData, "Output", "output", "", DisplayLevel.Beginner)]
        public string Output
        {
            get
            {
                return this.output;
            }
            set
            {
                this.output = value;
                OnPropertyChange("Output");
            }
        }

        private void OnPropertyChange(String propertyname)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(propertyname));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        # endregion

        #region IPlugin Member

        public void Dispose()
        {

        }

        public void Execute()
        {
            ProcessTransposition();
        }

        public void Initialize()
        {

        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public void Pause()
        {

        }

        public void PostExecution()
        {

        }

        public void PreExecution()
        {

        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void Stop()
        {

        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        # region Private Methods

        private void ProcessTransposition()
        {

            Output = encrypt_text(input, keyword);

            ProgressChanged(1, 1);
        }

        private String encrypt_text(String input, String keyword)
        {
            int[] keys = new int[keyword.Length];

            for (int i = 1; i <= keyword.Length; i++)
            {
                for (int j = 0; j < keyword.Length; j++)
                {
                    if ((int)Char.GetNumericValue(keyword[j]) == i)
                    {
                        keys[i - 1] = j;
                    }
                }
            }

            String enc = "";

            for (int j = 0; j < keyword.Length; j++)
            {
                for (int i = 0; i <= input.Length / keyword.Length; i++)
                {
                    int tmp = keys[j] + i * keyword.Length;

                    if (tmp < input.Length)
                    {
                        enc += input[tmp];
                    }
                }
            }

            return enc;
        }

        private String decrypt_text(String input, String keyword)
        {
            int input_pos = 0;

            int breite = keyword.Length;
            int hoehe = input.Length / keyword.Length;
            int offs = input.Length % keyword.Length;
            if (offs != 0) { hoehe++; }

            char[,] matrix = new char[breite, hoehe];

            for (int i = 1; i <= keyword.Length; i++)
            {
                int pos = -1;

                for (int j = 0; j < keyword.Length; j++)
                {
                    if (i == (int)Char.GetNumericValue(keyword[j]))
                    {
                        pos = j;
                    }
                }


                if (offs != 0)
                {
                    if (pos < offs)
                    {
                        for (int j = 0; j < hoehe; j++)
                        {
                            matrix[pos, j] = input[input_pos];
                            input_pos++;
                        }
                    }
                    else
                    {
                        for (int j = 0; j < hoehe - 1; j++)
                        {
                            matrix[pos, j] = input[input_pos];
                            input_pos++;
                        }
                    }
                }

                else
                {
                    for (int j = 0; j < hoehe; j++)
                    {
                        matrix[pos, j] = input[input_pos];
                        input_pos++;
                    }
                }
            }
            String dec = "";

            // Spaltenweise auslesen
            // Read by line
            for (int j = 0; j < hoehe; j++)
            {
                for (int i = 0; i < breite; i++)
                {
                    dec += matrix[i, j];
                }
            }
            // Ende Spaltenweise auslesen
            return dec;
        }

        private char[,] enc_read_in_by_row(String input, int keyword_length)
        {
            int size = input.Length / keyword_length;

            if (input.Length % keyword_length != 0)
            {
                size++;
            }

            int pos = 0;

            char[,] matrix = new char[keyword_length, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < keyword_length; j++)
                {
                    if (pos < input.Length)
                    {
                        matrix[j, i] = input[pos];
                        pos++;
                    }                    
                }
            }

            return matrix;
        }

        private char[,] enc_read_in_by_column(String input, int keyword_length)
        {
            int size = input.Length / keyword_length;
            int offs = input.Length % keyword_length;
            if (offs != 0)
            {
                size++;
            }

            int pos = 0;

            char[,] matrix = new char[keyword_length, size];
            for (int i = 0; i < keyword_length; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (pos < input.Length)
                    {
                        if (offs > 0 && j == size - 1 && i >= offs) { }
                        else
                        {
                            matrix[i, j] = input[pos];
                            pos++;
                        }
                    }
                }
            }

            return matrix;
        }

        private char[,] dec_read_in_by_row(String input, int[] keyword)
        {
            int size = input.Length / keyword.Length;

            int offs = input.Length % keyword.Length;
            if (offs != 0)
            {
                size++;
            }

            char[,] matrix = new char[keyword.Length, size];
            int pos = 0;

            for (int i = 0; i < keyword.Length; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (pos < input.Length)
                    {
                        if ((!offs.Equals(0)) && j.Equals(size - 1))
                        {
                            bool ok = false;

                            for (int k = 0; k < offs; k++)
                            {
                                Console.WriteLine(k);
                                if((keyword[k]-1).Equals(i))
                                {
                                    ok = true;
                                }
                            }

                            if (ok)
                            {
                                matrix[i, j] = input[pos];
                                pos++;
                            }

                        }
                        else
                        {
                            matrix[i, j] = input[pos];
                            pos++;
                        }
                    }
                }
            }


            return matrix;
        }

        private char[,] dec_read_in_by_column(String input, int[] keyword)
        {
            int size = input.Length / keyword.Length;

            int offs = input.Length % keyword.Length;
            if (offs != 0)
            {
                size++;
            }

            char[,] matrix = new char[keyword.Length, size];
            int pos = 0;

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < keyword.Length; j++)
                {
                    if (pos < input.Length)
                    {
                        if ((!offs.Equals(0)) && i.Equals(size - 1))
                        {
                            bool ok = false;

                            for (int k = 0; k < offs; k++)
                            {
                                Console.WriteLine(k);
                                if ((keyword[k] - 1).Equals(j))
                                {
                                    ok = true;
                                }
                            }

                            if (ok)
                            {
                                matrix[j, i] = input[pos];
                                pos++;
                            }

                        }
                        else
                        {
                            matrix[j, i] = input[pos];
                            pos++;
                        }
                    }
                }
            }


            return matrix;
        }

        private char[,] enc_permut_by_column(char[,] readin_matrix, int[] keyword)
        {
            int x = keyword.Length;
            int y = readin_matrix.Length / keyword.Length;

            char[,] matrix = new char[x,y];

            int pos = 0;

            for (int i = 1; i <= keyword.Length; i++)
            {
                for (int j = 0; j < keyword.Length; j++)
                {
                    if(i.Equals(keyword[j]))
                    {
                        Console.WriteLine("true");
                        pos = j;
                    }
                }

                for (int j = 0; j < y; j++)
                {
                    matrix[i - 1, j] = readin_matrix[pos, j];
                }
            }

            return matrix;
        }

        private String enc_read_out_by_row(char[,] matrix, int keyword_length)
        {
            int x = keyword_length;
            int y = matrix.Length / keyword_length;

            String enc = "";
            char empty_char = new char();

            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    char tmp = matrix[j, i];
                    if (! tmp.Equals(empty_char))
                    {
                        enc += tmp;
                    }
                }
            }

            return enc;
        }

        private String enc_read_out_by_column(char[,] matrix, int keyword_length)
        {
            int x = keyword_length;
            int y = matrix.Length / keyword_length;

            String enc = "";
            char empty_char = new char();

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    char tmp = matrix[i, j];
                    if (!tmp.Equals(empty_char))
                    {
                        enc += tmp;
                    }
                }
            }

            return enc;
        }

        private int[] get_Keyword_Array(String keyword)
        {
            int length = keyword.Length/2 + 1;
            int[] keys = new int[length];

            for (int i = 0; i < keyword.Length; i = i + 2)
            {
                keys[i / 2] = (int)Char.GetNumericValue(keyword[i]);
            }

            return keys;
        }

        private bool is_Valid_Keyword(int[] keyword)
        {
            for (int i = 1; i <= keyword.Length; i++)
            {
                bool exists = false;

                for (int j = 0; j < keyword.Length; j++)
                {
                    if (i.Equals(keyword[j]))
                    {
                        exists = true;
                    }
                }

                if (!exists)
                {
                    return false;
                }
            }

            return true;
        }
    }
        # endregion
    }
}
