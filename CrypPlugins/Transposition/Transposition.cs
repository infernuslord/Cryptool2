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

namespace Transposition
{
    [Author("Daniel Kohnen, Julian Weyers, Simon Malischewski, Armin Wiefels", "kohnen@cryptool.org, weyers@cryptool.org, malischewski@cryptool.org, wiefels@cryptool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "Transposition", "", "", "Transposition/Images/icon.png", "Transposition/Images/encrypt.png", "Transposition/Images/decrypt.png")]
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

        [PropertyInfo(Direction.InputData, "Input", "input", "Text to be encrypted.", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
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

        [PropertyInfo(Direction.InputData, "Keyword", "keyword", "Keyword used for encryption", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
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
            switch (settings.Action)
            {
                case 0:
                    Output = encrypt();
                    break;
                case 1:
                    Output = decrypt();
                    break;
                default:
                    break;
            }


            ProgressChanged(1, 1);
        }

        private String encrypt()
        {
            int[] key = get_Keyword_Array(keyword);
            if (key != null && input != null)
            {
                if (is_Valid_Keyword(key))
                {
                    char[,] matrix = null;

                    switch ((TranspositionSettings.ReadInMode)settings.ReadIn)
                    {
                        case TranspositionSettings.ReadInMode.byRow:
                            matrix = enc_read_in_by_row(input, key.Length); break;
                        case TranspositionSettings.ReadInMode.byColumn:
                            matrix = enc_read_in_by_column(input, key.Length); break;
                        default:
                            break;
                    }

                    switch ((TranspositionSettings.PermutationMode)settings.Permutation)
                    {
                        case TranspositionSettings.PermutationMode.byColumn:
                            matrix = enc_permut_by_column(matrix, key); break;

                        // Permute by row still to do
                        case TranspositionSettings.PermutationMode.byRow:
                            matrix = enc_permut_by_column(matrix, key); break;
                        default:
                            break;
                    }

                    String encrypted = "";

                    switch ((TranspositionSettings.ReadOutMode)settings.ReadOut)
                    {
                        case TranspositionSettings.ReadOutMode.byRow:
                            encrypted = read_out_by_row(matrix, key.Length); break;
                        case TranspositionSettings.ReadOutMode.byColumn:
                            encrypted = read_out_by_column(matrix, key.Length); break;
                        default:
                            break;
                    }

                    return encrypted;
                }
                else
                {
                    Transposition_LogMessage("Keyword is not valid", NotificationLevel.Error);
                    return "";
                }
            }
            else
            {
                // 2do: Anzeige "Kein gültiges Keyword
                return "";
            }
        }

        private String decrypt()
        {
            int[] key = get_Keyword_Array(keyword);
            if (key != null && input != null)
            {
                if (is_Valid_Keyword(key))
                {
                    char[,] matrix = null;

                    switch ((TranspositionSettings.ReadOutMode)settings.ReadOut)
                    {
                        case TranspositionSettings.ReadOutMode.byRow:
                            matrix = dec_read_in_by_column(input, key); break;
                        case TranspositionSettings.ReadOutMode.byColumn:
                            matrix = dec_read_in_by_row(input, key); break;
                        default:
                            break;
                    }

                    switch ((TranspositionSettings.PermutationMode)settings.Permutation)
                    {
                        case TranspositionSettings.PermutationMode.byRow:
                            matrix = dec_permut_by_column(matrix, key); break;

                        // Permute by row still to do
                        case TranspositionSettings.PermutationMode.byColumn:
                            matrix = dec_permut_by_column(matrix, key); break;
                        default:
                            break;
                    }

                    String decrypted = "";

                    switch ((TranspositionSettings.ReadInMode)settings.ReadIn)
                    {
                        case TranspositionSettings.ReadInMode.byRow:
                            decrypted = read_out_by_row(matrix, key.Length); break;
                        case TranspositionSettings.ReadInMode.byColumn:
                            decrypted = read_out_by_column(matrix, key.Length); break;
                        default:
                            break;
                    }

                    return decrypted;
                }
                else
                {
                    Transposition_LogMessage("Keyword is not valid", NotificationLevel.Error);
                    return "";
                }
            }
            else
            {
                // 2do: Anzeige "Kein gültiges Keyword
                return "";
            }
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
                                if ((keyword[k] - 1).Equals(i))
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

            char[,] matrix = new char[x, y];

            int pos = 0;

            for (int i = 1; i <= keyword.Length; i++)
            {
                for (int j = 0; j < keyword.Length; j++)
                {
                    if (i.Equals(keyword[j]))
                    {
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

        private char[,] dec_permut_by_column(char[,] readin_matrix, int[] keyword)
        {
            int x = keyword.Length;
            int y = readin_matrix.Length / keyword.Length;

            char[,] matrix = new char[x, y];

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    matrix[i, j] = readin_matrix[keyword[i] - 1, j];
                }
            }

            return matrix;
        }

        private String read_out_by_row(char[,] matrix, int keyword_length)
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
                    if (!tmp.Equals(empty_char))
                    {
                        enc += tmp;
                    }
                }
            }

            return enc;
        }

        private String read_out_by_column(char[,] matrix, int keyword_length)
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
            try
            {
                int length = 1;
                char komma = ',';

                for (int i = 0; i < keyword.Length; i++)
                {
                    if (keyword[i].Equals(komma))
                    {
                        length++;
                    }
                }

                int[] keys = new int[length];

                String tmp = "";
                int pos = 0;
                for (int i = 0; i < keyword.Length; i++)
                {
                    if (i.Equals(keyword.Length - 1))
                    {
                        tmp += keyword[i];
                        keys[pos] = Convert.ToInt32(tmp);
                    }

                    else
                    {
                        if (keyword[i].Equals(komma))
                        {
                            keys[pos] = Convert.ToInt32(tmp);
                            tmp = "";
                            pos++;
                        }
                        else
                        {
                            tmp += keyword[i];
                        }
                    }
                }

                return keys;
            }
            catch (FormatException)
            {
                return null;
            }
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

        private void Transposition_LogMessage(string msg, NotificationLevel loglevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(msg, this, loglevel));

        }
      
        # endregion
    }
}
