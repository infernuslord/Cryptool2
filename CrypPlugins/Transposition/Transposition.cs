using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;
using System.Runtime.InteropServices;

using Cryptool;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.Control;

namespace Transposition
{
    [Author("Daniel Kohnen, Julian Weyers, Simon Malischewski, Armin Wiefels", "kohnen@cryptool.org, weyers@cryptool.org, malischewski@cryptool.org, wiefels@cryptool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Transposition.Properties.Resources", false, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "Transposition/Images/icon.png", "Transposition/Images/encrypt.png", "Transposition/Images/decrypt.png")]
    [EncryptionType(EncryptionType.Classic)]
    public class Transposition : IEncryption
    {
        # region Private variables

        private String keyword = "";
        private byte[] input;
        private byte[] output;
        private TranspositionSettings settings;
        private TranspositionPresentation myPresentation;
        private byte[,] read_in_matrix;
        private byte[,] permuted_matrix;
        private int[] key;
        private AutoResetEvent ars;
        private Boolean b;

        # endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public Transposition()
        {
            this.settings = new TranspositionSettings();
            myPresentation = new TranspositionPresentation();
            Presentation = myPresentation;
            ars =new AutoResetEvent(false);
            myPresentation.feuerEnde += new EventHandler(presentation_finished);
            myPresentation.updateProgress += new EventHandler(update_progress);
            this.settings.PropertyChanged += settings_OnPropertyChange;
            b = true;
        }

        private void update_progress(object sender, EventArgs e) 
        {
            TranspositionPresentation myhelp = new TranspositionPresentation();
            myhelp = (TranspositionPresentation)sender;
            ProgressChanged(myhelp.progress, 3000);
        }

        private void presentation_finished(object sender, EventArgs e)
        {
           
            Output = output;
            ProgressChanged(1, 1);
            ars.Set(); 
        }

        private void settings_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            myPresentation.UpdateSpeed(this.settings.PresentationSpeed);
        }
        /// <summary>
        /// Get or set all settings for this algorithm.
        /// </summary>
        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (TranspositionSettings)value; }
        }

        # region getter methods

        /// <summary>
        /// Get read in matrix.
        /// </summary>
        public byte[,] Read_in_matrix
        {
            get
            {
                return read_in_matrix;
            }
        }

        /// <summary>
        /// Get permuted matrix.
        /// </summary>
        public byte[,] Permuted_matrix
        {
            get
            {
                return permuted_matrix;
            }
        }

        /// <summary>
        /// Get numerical key order.
        /// </summary>
        public int[] Key
        {
            get
            {
                return key;
            }
        }
        # endregion

        # region Properties

        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip", "", false, false, QuickWatchFormat.Text, null)]
        public Byte[] Input
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

        [PropertyInfo(Direction.InputData, "KeywordCaption", "KeywordTooltip", "", false, false, QuickWatchFormat.Text, null)]
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

        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip", "")]
        public byte[] Output
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
            
            Transposition_LogMessage("execute tr", NotificationLevel.Debug);
            ProcessTransposition();
            if (controlSlave is object && Input is object)
            {
                ((TranspositionControl)controlSlave).onStatusChanged();
            }

            if(b)
            if (Presentation.IsVisible)
            {
                b = false;
                    Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                   {
                       myPresentation.main(Read_in_matrix, Permuted_matrix, key, Keyword, Input, output, this.settings.Permutation, this.settings.ReadIn, this.settings.ReadOut, this.settings.Action, this.settings.Number);
                   }
                   , null);

                ars.WaitOne();
                Thread.Sleep(1000);
                b = true;
            }
            else
            {
                Output = output;
                ProgressChanged(1, 1);
            }
            
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
            get;
            private set;
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return Presentation; }
        }

        public void Stop()
        {
            ars.Set();
            myPresentation.my_Stop(this, EventArgs.Empty);
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        # region Private Methods

        private void ProcessTransposition()
        {
            try
            {
                if (keyword.Contains(','))
                {
                    key = get_Keyword_Array(keyword);
                }

                else
                {
                    key = sortKey(keyword);
                }
                
                switch (settings.Action)
                {
                    case 0:
                        output = encrypt(input, key);
                        break;
                    case 1:
                        output = decrypt(input, key);
                        break;
                    default:
                        break;
                }
                
            }

            catch (Exception)
            {
                Transposition_LogMessage("Keyword is not valid", NotificationLevel.Error);
                Output = null;
            }
        }

        private byte[] encrypt(byte[] input, int[] key)
        {
            if (key != null && input != null && key.Length > 0)
            {
                if (is_Valid_Keyword(key))
                {
                    byte[] encrypted = null;

                    if (((TranspositionSettings.PermutationMode)settings.Permutation).Equals(TranspositionSettings.PermutationMode.byRow))
                    {
                        switch ((TranspositionSettings.ReadInMode)settings.ReadIn)
                        {
                            case TranspositionSettings.ReadInMode.byRow:
                                read_in_matrix = enc_read_in_by_row_if_row_perm(input, key.Length); break;
                            case TranspositionSettings.ReadInMode.byColumn:
                                read_in_matrix = enc_read_in_by_column_if_row_perm(input, key.Length); break;
                            default:
                                break;
                        }

                        permuted_matrix = enc_permute_by_row(read_in_matrix, key);

                        switch ((TranspositionSettings.ReadOutMode)settings.ReadOut)
                        {
                            case TranspositionSettings.ReadOutMode.byRow:
                                encrypted = read_out_by_row_if_row_perm(permuted_matrix, key.Length); break;
                            case TranspositionSettings.ReadOutMode.byColumn:
                                encrypted = read_out_by_column_if_row_perm(permuted_matrix, key.Length); break;
                            default:
                                break;
                        }
                    }

                    // permute by column:
                    else
                    {
                        switch ((TranspositionSettings.ReadInMode)settings.ReadIn)
                        {
                            case TranspositionSettings.ReadInMode.byRow:
                                read_in_matrix = enc_read_in_by_row(input, key.Length); break;
                            case TranspositionSettings.ReadInMode.byColumn:
                                read_in_matrix = enc_read_in_by_column(input, key.Length); break;
                            default:
                                break;
                        }

                        permuted_matrix = enc_permut_by_column(read_in_matrix, key);

                        switch ((TranspositionSettings.ReadOutMode)settings.ReadOut)
                        {
                            case TranspositionSettings.ReadOutMode.byRow:
                                encrypted = read_out_by_row(permuted_matrix, key.Length); break;
                            case TranspositionSettings.ReadOutMode.byColumn:
                                encrypted = read_out_by_column(permuted_matrix, key.Length); break;
                            default:
                                break;
                        }
                    }
                    return encrypted;
                }
                else
                {
                    Transposition_LogMessage("Keyword is not valid", NotificationLevel.Error);
                    return null;
                }
            }
            else
            {
                // 2do: Anzeige "Kein gültiges Keyword
                return null;
            }
        }

        public byte[] decrypt(byte[] input, int[] new_key)
        {
            //Transposition_LogMessage("hier normales decrypt: " + new_key[0] + " / " +input[0], NotificationLevel.Debug);

            if (new_key != null && input != null && new_key.Length > 0)
            {
                if (is_Valid_Keyword(new_key))
                {
                    byte[] decrypted = null;
                    if (((TranspositionSettings.PermutationMode)settings.Permutation).Equals(TranspositionSettings.PermutationMode.byRow))
                    {
                        switch ((TranspositionSettings.ReadOutMode)settings.ReadOut)
                        {
                            case TranspositionSettings.ReadOutMode.byRow:
                                read_in_matrix = dec_read_in_by_row_if_row_perm(input, new_key); break;
                            case TranspositionSettings.ReadOutMode.byColumn:
                                read_in_matrix = dec_read_in_by_column_if_row_perm(input, new_key); break;
                            default:
                                break;
                        }

                        permuted_matrix = dec_permut_by_row(read_in_matrix, new_key);

                        switch ((TranspositionSettings.ReadInMode)settings.ReadIn)
                        {
                            case TranspositionSettings.ReadInMode.byRow:
                                decrypted = read_out_by_row_if_row_perm(permuted_matrix, new_key.Length); break;
                            case TranspositionSettings.ReadInMode.byColumn:
                                decrypted = read_out_by_column_if_row_perm(permuted_matrix, new_key.Length); break;
                            default:
                                break;
                        }
                    }

                    // permute by column:
                    else
                    {
                        switch ((TranspositionSettings.ReadOutMode)settings.ReadOut)
                        {
                            case TranspositionSettings.ReadOutMode.byRow:
                                read_in_matrix = dec_read_in_by_row(input, new_key); break;
                            case TranspositionSettings.ReadOutMode.byColumn:
                                read_in_matrix = dec_read_in_by_column(input, new_key); break;
                            default:
                                break;
                        }

                        permuted_matrix = dec_permut_by_column(read_in_matrix, new_key);

                        switch ((TranspositionSettings.ReadInMode)settings.ReadIn)
                        {
                            case TranspositionSettings.ReadInMode.byRow:
                                decrypted = read_out_by_row(permuted_matrix, new_key.Length); break;
                            case TranspositionSettings.ReadInMode.byColumn:
                                decrypted = read_out_by_column(permuted_matrix, new_key.Length); break;
                            default:
                                break;
                        }
                    }
                    return decrypted;
                }
                else
                {
                    Transposition_LogMessage("Keyword is not valid", NotificationLevel.Error);
                    return null;
                }
            }
            else
            {
                // 2do: Anzeige "Kein gültiges Keyword
                return null;
            }
        }

        private byte[,] enc_read_in_by_row(byte[] input, int keyword_length)
        {
            int size = input.Length / keyword_length;

            if (input.Length % keyword_length != 0)
            {
                size++;
            }

            int pos = 0;
            byte[,] matrix = new byte[keyword_length, size];

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

        private byte[,] enc_read_in_by_column(byte[] input, int keyword_length)
        {
            int size = input.Length / keyword_length;
            int offs = input.Length % keyword_length;
            if (offs != 0)
            {
                size++;
            }

            int pos = 0;

            byte[,] matrix = new byte[keyword_length, size];
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

        private byte[,] enc_read_in_by_row_if_row_perm(byte[] input, int keyword_length)
        {
            int height = keyword_length;
            int length = input.Length / keyword_length;
            int offs = input.Length % keyword_length;
            if (offs != 0)
            {
                length++;
            }

            byte[,] matrix = new byte[length, height];
            int pos = 0;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    if (pos < input.Length)
                    {
                        if (j.Equals(length - 1) && offs != 0)
                        {
                            if (i < offs)
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

        private byte[,] enc_read_in_by_column_if_row_perm(byte[] input, int keyword_length)
        {
            int height = keyword_length;
            int length = input.Length / keyword_length;
            int offs = input.Length % keyword_length;
            if (offs != 0)
            {
                length++;
            }

            byte[,] matrix = new byte[length, height];
            int pos = 0;

            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (pos < input.Length)
                    {
                        matrix[i, j] = input[pos];
                        pos++;
                    }
                }
            }
            return matrix;
        }

        private byte[,] dec_read_in_by_column(byte[] input, int[] keyword)
        {
            int size = input.Length / keyword.Length;
            int offs = input.Length % keyword.Length;
            if (offs != 0)
            {
                size++;
            }

            byte[,] matrix = new byte[keyword.Length, size];
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

        private byte[,] dec_read_in_by_column_if_row_perm(byte[] input, int[] keyword)
        {
            int size = input.Length / keyword.Length;
            int offs = input.Length % keyword.Length;
            if (offs != 0)
            {
                size++;
            }

            byte[,] matrix = new byte[size, keyword.Length];
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

        private byte[,] dec_read_in_by_row(byte[] input, int[] keyword)
        {
            int size = input.Length / keyword.Length;
            int offs = input.Length % keyword.Length;
            if (offs != 0)
            {
                size++;
            }

            byte[,] matrix = new byte[keyword.Length, size];
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

        private byte[,] dec_read_in_by_row_if_row_perm(byte[] input, int[] keyword)
        {
            int size = input.Length / keyword.Length;
            int offs = input.Length % keyword.Length;
            if (offs != 0)
            {
                size++;
            }

            byte[,] matrix = new byte[size, keyword.Length];
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

        private byte[,] enc_permut_by_column(byte[,] readin_matrix, int[] keyword)
        {
            int x = keyword.Length;
            int y = readin_matrix.Length / keyword.Length;
            byte[,] matrix = new byte[x, y];
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

        private byte[,] enc_permute_by_row(byte[,] readin_matrix, int[] keyword)
        {
            int y = keyword.Length;
            int x = readin_matrix.Length / keyword.Length;
            byte[,] matrix = new byte[x, y];
            int pos = 0;

            for (int i = 1; i <= y; i++)
            {
                for (int j = 0; j < keyword.Length; j++)
                {
                    if (keyword[j].Equals(i))
                    {
                        pos = j;
                    }
                }

                for (int j = 0; j < x; j++)
                {
                    matrix[j, i - 1] = readin_matrix[j, pos];
                }
            }
            return matrix;
        }

        private byte[,] dec_permut_by_column(byte[,] readin_matrix, int[] keyword)
        {
            int x = keyword.Length;
            int y = readin_matrix.Length / keyword.Length;
            byte[,] matrix = new byte[x, y];

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    matrix[i, j] = readin_matrix[keyword[i] - 1, j];
                }
            }
            return matrix;
        }

        private byte[,] dec_permut_by_row(byte[,] readin_matrix, int[] keyword)
        {
            int x = keyword.Length;
            int y = readin_matrix.Length / keyword.Length;
            byte[,] matrix = new byte[y, x];

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    matrix[j, i] = readin_matrix[j, keyword[i] - 1];
                }
            }
            return matrix;
        }

        private byte[] read_out_by_row(byte[,] matrix, int keyword_length)
        {
            int x = keyword_length;
            int y = matrix.Length / keyword_length;
            byte empty_byte = new byte();
            int count_empty = 0;

            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    byte tmp = matrix[j, i];
                    if (tmp.Equals(empty_byte))
                    {
                        count_empty++;
                    }
                }
            }
            byte[] enc = new byte[matrix.Length - count_empty];

            int pos = 0;
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    byte tmp = matrix[j, i];
                    if (!tmp.Equals(empty_byte))
                    {
                        enc[pos] = tmp;
                        pos++;
                    }
                }
            }
            return enc;
        }

        private byte[] read_out_by_row_if_row_perm(byte[,] matrix, int keyword_length)
        {
            int y = keyword_length;
            int x = matrix.Length / keyword_length;

            byte empty_byte = new byte();
            int empty_count = 0;
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    byte tmp = matrix[j, i];
                    if (tmp.Equals(empty_byte))
                    {
                        empty_count++;
                    }
                }
            }

            byte[] enc = new byte[matrix.Length - empty_count];
            int pos = 0;

            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    byte tmp = matrix[j, i];
                    if (!tmp.Equals(empty_byte))
                    {
                        enc[pos] = tmp;
                        pos++;
                    }
                }
            }
            return enc;
        }

        private byte[] read_out_by_column(byte[,] matrix, int keyword_length)
        {
            int x = keyword_length;
            int y = matrix.Length / keyword_length;

            byte empty_byte = new byte();
            int empty_count = 0;

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    byte tmp = matrix[i, j];
                    if (tmp.Equals(empty_byte))
                    {
                        empty_count++;
                    }
                }
            }

            byte[] enc = new byte[matrix.Length - empty_count];
            int pos = 0;
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    byte tmp = matrix[i, j];
                    if (!tmp.Equals(empty_byte) || tmp.Equals(null))
                    {
                        enc[pos] = tmp;
                        pos++;  
                    }
                }
            }
            return enc;
        }

        private byte[] read_out_by_column_if_row_perm(byte[,] matrix, int keyword_length)
        {
            int y = keyword_length;
            int x = matrix.Length / keyword_length;
            
            byte empty_byte = new byte();
            int empty_count = 0;
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    byte tmp = matrix[i, j];
                    if (tmp.Equals(empty_byte))
                    {
                        empty_count++;
                    }
                }
            }

            byte[] enc = new byte[matrix.Length - empty_count];
            int pos = 0;
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    byte tmp = matrix[i, j];
                    if (!tmp.Equals(empty_byte))
                    {
                        enc[pos] = tmp;
                        pos++;
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

        public int[] sortKey(String input)
        {
            if (input != null && !input.Equals(""))
            {
                String key = input;
                Char[] keyChars = key.ToCharArray();
                Char[] orgChars = key.ToCharArray();
                int[] rank = new int[keyChars.Length];
                Array.Sort(keyChars);

                for (int i = 0; i < orgChars.Length; i++)
                {
                    rank[i] = (Array.IndexOf(keyChars, orgChars[i])) + 1;
                    keyChars[Array.IndexOf(keyChars, orgChars[i])] = (char)0;
                }
                return rank;
            }
            return null;
        }

        public void Transposition_LogMessage(string msg, NotificationLevel loglevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(msg, this, loglevel));
        }

        public void changeSettings(string setting, object value)
        {
            if (setting.Equals("ReadIn")) settings.ReadIn = (int)value;
            else if (setting.Equals("Permute")) settings.Permutation = (int)value;
            else if (setting.Equals("ReadOut")) settings.ReadOut = (int)value;

        }
        # endregion

        private IControlEncryption controlSlave;
        [PropertyInfo(Direction.ControlSlave, "ControlSlaveCaption", "ControlSlaveTooltip", "")]
        public IControlEncryption ControlSlave
        {
            get
            {
                if (controlSlave == null)
                    controlSlave = new TranspositionControl(this);
                return controlSlave;
            }
        }
    }

    public class TranspositionControl : IControlEncryption
    {
        private Transposition plugin;


        public TranspositionControl(Transposition plugin)
        {
            this.plugin = plugin;
        }

        #region IControlEncryption Member
        
        public byte[] Decrypt(byte[] key, int blocksize)
        {
            throw new NotImplementedException();
        }

        public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] IV, int bytesToUse)
        {
            return Decrypt(ciphertext, key, IV);
        }

        public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] IV)
        {
            if (plugin.Input != ciphertext)
            {
                plugin.Input = ciphertext;
            }

            int[] k = new int[key.Length];
            for(int i=0; i<key.Length; i++)
            {
                k[i] = key[i];
            }

            //plugin.Transposition_LogMessage("hier decrypt von control: " + k[0] + " / " +plugin.Input[0], NotificationLevel.Debug);
            return plugin.decrypt(plugin.Input, k);
        }

        public byte[] Encrypt(byte[] key, int blocksize)
        {
            return null;
        }

        public IControlEncryption clone()
        {
            return null;
        }

        public string GetKeyPattern()
        {
            return null;
        }

        public void onStatusChanged()
        {
            if (OnStatusChanged != null)
                OnStatusChanged(this, true);
        }

        public string GetOpenCLCode(int decryptionLength, byte[] iv)
        {
            return null;
        }

        public void changeSettings(string setting, object value)
        {
            plugin.changeSettings(setting, value);
        }

        public IKeyTranslator GetKeyTranslator()
        {
            throw new NotImplementedException();
        }

        public event KeyPatternChanged keyPatternChanged;

        #endregion

        #region IControl Member

        public event IControlStatusChangedEventHandler OnStatusChanged;

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion
    }
}
