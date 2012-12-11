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
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.Control;

namespace Transposition
{
    [Author("Daniel Kohnen, Julian Weyers, Simon Malischewski, Armin Wiefels", "kohnen@cryptool.org, weyers@cryptool.org, malischewski@cryptool.org, wiefels@cryptool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Transposition.Properties.Resources", "PluginCaption", "PluginTooltip", "Transposition/DetailedDescription/doc.xml", "Transposition/Images/icon.png", "Transposition/Images/encrypt.png", "Transposition/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Transposition : ICrypComponent
    {
        # region Private variables

        private String keyword = "";
        private ICryptoolStream input;
        private ICryptoolStream outputvalue;
        private char[] output;
        private TranspositionSettings settings;
        private TranspositionPresentation myPresentation;
        private char[,] read_in_matrix;
        private char[,] permuted_matrix;
        private int[] key;
        
        private bool running = false;
        private bool stopped = false;

        # endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public Transposition()
        {
            this.settings = new TranspositionSettings();
            myPresentation = new TranspositionPresentation();
            Presentation = myPresentation;
            
            myPresentation.feuerEnde += new EventHandler(presentation_finished);
            myPresentation.updateProgress += new EventHandler(update_progress);
            this.settings.PropertyChanged += settings_OnPropertyChange;
        }

        private void update_progress(object sender, EventArgs e) 
        {
            //TranspositionPresentation myhelp = new TranspositionPresentation();
            //myhelp = (TranspositionPresentation)sender;
            ProgressChanged(myPresentation.progress, 3000);
        }

        private void presentation_finished(object sender, EventArgs e)
        {
            if(!myPresentation.Stop)
            Output = CharacterArrayToCStream(this.output);
            ProgressChanged(1, 1);
            
            running = false;
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
        public char[,] Read_in_matrix
        {
            get
            {
                return read_in_matrix;
            }
        }

        /// <summary>
        /// Get permuted matrix.
        /// </summary>
        public char[,] Permuted_matrix
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

        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip", false)]
        public ICryptoolStream Input 
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

        public Char[] InputToCharacterArray
        {
            get
            {
                byte[] streamData = ICryptoolStreamToByteArray(Input);
                switch (settings.InternalNumber)
                {
                    case 0:
                        var sUtf = Encoding.UTF8.GetString(streamData);
                        return sUtf.ToCharArray();
                    case 1:
                        var chary = new char[streamData.Length];
                        for (int i = 0; i < streamData.Length; i++)
                        {
                            chary[i] = (char)(streamData[i]);
                        }
                        return chary;
                    default:
                        return null;
                }
            }
        }

        private byte[] CStreamReaderToByteArray(CStreamReader stream)
        {
            stream.WaitEof();
            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            stream.ReadFully(buffer);
            return buffer;
        }

        private ICryptoolStream CharacterArrayToCStream(char[] b)
        {
            var csw = new CStreamWriter();
            switch (settings.InternalNumber)
                {
                    case 0: 
                        var bUtf = UnicodeEncoding.UTF8.GetBytes(b); 
                        csw.Write(bUtf);            
                        break;
                    case 1:
                        var chary = new byte[b.Length];
                        for(int i=0;i< b.Length;i++) 
                        {
                            chary[i] = (byte)(b[i]);
                        }
                        csw.Write(chary);   
                        break;
                    default:
                        return null;
                }            
            csw.Close();
            return csw;
        }

        private byte[] ICryptoolStreamToByteArray(ICryptoolStream stream)
        {
            return  CStreamReaderToByteArray(stream.CreateReader());
        }

        [PropertyInfo(Direction.InputData, "KeywordCaption", "KeywordTooltip", false)]
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

        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip")]
        public ICryptoolStream Output
        {
            get
            {
                return outputvalue;
            }
            
            set
            {
                this.outputvalue = value;
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
            
            while(running)
            {
                myPresentation.my_Stop(this, EventArgs.Empty);
                if (stopped)
                    return;
            }

            running = true;
            
            ProcessTransposition();
            if (controlSlave is object && Input is object)
            {
                ((TranspositionControl)controlSlave).onStatusChanged();
            }

            if (Presentation.IsVisible && key.Count() != 0)
            {
                    Transposition_LogMessage(Read_in_matrix.GetLength(0) +" " + Read_in_matrix.GetLength(1) +" " + Input.Length  , NotificationLevel.Debug);        
                    Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                   {
                       myPresentation.main(Read_in_matrix, Permuted_matrix, key, Keyword, InputToCharacterArray, this.output, this.settings.Permutation, this.settings.ReadIn, this.settings.ReadOut, this.settings.Action, this.settings.Number, this.settings.PresentationSpeed);
                   }
                   , null);

                //ars.WaitOne();
            }
            else
            {
                Output = CharacterArrayToCStream(this.output);
                ProgressChanged(1, 1);
            }
            
        }

        public void Initialize()
        {
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public void PostExecution()
        {
            
        }

        public void PreExecution()
        {
            running = false;
            stopped = false;
            
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get;
            private set;
        }

        public void Stop()
        {
            
            stopped = true;

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
                        this.output = encrypt(InputToCharacterArray, key);
                        break;
                    case 1:
                        this.output = decrypt(InputToCharacterArray, key);
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

        private char[] encrypt(char[] input, int[] key)
        {
            if (key != null && input != null && key.Length > 0)
            {
                if (is_Valid_Keyword(key))
                {
                    char[] encrypted = null;

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

        public char[] decrypt(char[] input, int[] new_key)
        {
            //Transposition_LogMessage("hier normales decrypt: " + new_key[0] + " / " +input[0], NotificationLevel.Debug);

            if (new_key != null && input != null && new_key.Length > 0)
            {
                if (is_Valid_Keyword(new_key))
                {
                    char[] decrypted = null;
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

        public byte[] byteDecrypt(byte[] input, int[] new_key)
        {
            //Transposition_LogMessage("hier normales decrypt: " + new_key[0] + " / " +input[0], NotificationLevel.Debug);



            char[] c =  decrypt(System.Text.Encoding.ASCII.GetString(input).ToCharArray(), new_key);

            return ASCIIEncoding.ASCII.GetBytes(c);
            

        }

        private char[,] enc_read_in_by_row(char[] input, int keyword_length)
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

        private char[,] enc_read_in_by_column(char[] input, int keyword_length)
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

        private char[,] enc_read_in_by_row_if_row_perm(char[] input, int keyword_length)
        {
            int height = keyword_length;
            int length = input.Length / keyword_length;
            int offs = input.Length % keyword_length;
            if (offs != 0)
            {
                length++;
            }

            char[,] matrix = new char[length, height];
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

        private char[,] enc_read_in_by_column_if_row_perm(char[] input, int keyword_length)
        {
            int height = keyword_length;
            int length = input.Length / keyword_length;
            int offs = input.Length % keyword_length;
            if (offs != 0)
            {
                length++;
            }

            char[,] matrix = new char[length, height];
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

        private char[,] dec_read_in_by_column(char[] input, int[] keyword)
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

        private char[,] dec_read_in_by_column_if_row_perm(char[] input, int[] keyword)
        {
            int size = input.Length / keyword.Length;
            int offs = input.Length % keyword.Length;
            if (offs != 0)
            {
                size++;
            }

            char[,] matrix = new char[size, keyword.Length];
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

        private char[,] dec_read_in_by_row(char[] input, int[] keyword)
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

        private char[,] dec_read_in_by_row_if_row_perm(char[] input, int[] keyword)
        {
            int size = input.Length / keyword.Length;
            int offs = input.Length % keyword.Length;
            if (offs != 0)
            {
                size++;
            }

            char[,] matrix = new char[size, keyword.Length];
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

        private char[,] enc_permute_by_row(char[,] readin_matrix, int[] keyword)
        {
            int y = keyword.Length;
            int x = readin_matrix.Length / keyword.Length;
            char[,] matrix = new char[x, y];
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

        private char[,] dec_permut_by_row(char[,] readin_matrix, int[] keyword)
        {
            int x = keyword.Length;
            int y = readin_matrix.Length / keyword.Length;
            char[,] matrix = new char[y, x];

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    matrix[j, i] = readin_matrix[j, keyword[i] - 1];
                }
            }
            return matrix;
        }

        private char[] read_out_by_row(char[,] matrix, int keyword_length)
        {
            int x = keyword_length;
            int y = matrix.Length / keyword_length;
            char empty_byte = new char();
            int count_empty = 0;

            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    char tmp = matrix[j, i];
                    if (tmp.Equals(empty_byte))
                    {
                        count_empty++;
                    }
                }
            }
            char[] enc = new char[matrix.Length - count_empty];

            int pos = 0;
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    char tmp = matrix[j, i];
                    if (!tmp.Equals(empty_byte))
                    {
                        enc[pos] = tmp;
                        pos++;
                    }
                }
            }
            return enc;
        }

        private char[] read_out_by_row_if_row_perm(char[,] matrix, int keyword_length)
        {
            int y = keyword_length;
            int x = matrix.Length / keyword_length;

            char empty_byte = new char();
            int empty_count = 0;
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    char tmp = matrix[j, i];
                    if (tmp.Equals(empty_byte))
                    {
                        empty_count++;
                    }
                }
            }

            char[] enc = new char[matrix.Length - empty_count];
            int pos = 0;

            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    char tmp = matrix[j, i];
                    if (!tmp.Equals(empty_byte))
                    {
                        enc[pos] = tmp;
                        pos++;
                    }
                }
            }
            return enc;
        }

        private char[] read_out_by_column(char[,] matrix, int keyword_length)
        {
            int x = keyword_length;
            int y = matrix.Length / keyword_length;

            char empty_byte = new char();
            int empty_count = 0;

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    char tmp = matrix[i, j];
                    if (tmp.Equals(empty_byte))
                    {
                        empty_count++;
                    }
                }
            }

            char[] enc = new char[matrix.Length - empty_count];
            int pos = 0;
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    char tmp = matrix[i, j];
                    if (!tmp.Equals(empty_byte) || tmp.Equals(null))
                    {
                        enc[pos] = tmp;
                        pos++;  
                    }
                }
            }
            return enc;
        }

        private char[] read_out_by_column_if_row_perm(char[,] matrix, int keyword_length)
        {
            int y = keyword_length;
            int x = matrix.Length / keyword_length;
            
            char empty_byte = new char();
            int empty_count = 0;
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    char tmp = matrix[i, j];
                    if (tmp.Equals(empty_byte))
                    {
                        empty_count++;
                    }
                }
            }

            char[] enc = new char[matrix.Length - empty_count];
            int pos = 0;
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    char tmp = matrix[i, j];
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

        private IControlTranspoEncryption controlSlave;
        [PropertyInfo(Direction.ControlSlave, "ControlSlaveCaption", "ControlSlaveTooltip")]
        public IControlTranspoEncryption ControlSlave
        {
            get
            {
                if (controlSlave == null)
                    controlSlave = new TranspositionControl(this);
                return controlSlave;
            }
        }
    }

    public class TranspositionControl : IControlTranspoEncryption
    {
        private Transposition plugin;

        public TranspositionControl(Transposition plugin)
        {
            this.plugin = plugin;
        }

        public byte[] Decrypt(byte[] ciphertext, byte[] key)
        {
            //if (plugin.InputToCharacterArray != ciphertext)
            //{
            //    plugin.InputToCharacterArray = ciphertext;
            //}

            int[] k = new int[key.Length];
            for(int i=0; i<key.Length; i++)
            {
                k[i] = key[i];
            }

            //plugin.Transposition_LogMessage("hier decrypt von control: " + k[0] + " / " +plugin.Input[0], NotificationLevel.Debug);
            return plugin.byteDecrypt(ciphertext, k);
        }

        public void onStatusChanged()
        {
            if (OnStatusChanged != null)
                OnStatusChanged(this, true);
        }

        public void changeSettings(string setting, object value)
        {
            plugin.changeSettings(setting, value);
        }

        public event IControlStatusChangedEventHandler OnStatusChanged;

        public void Dispose()
        {

        }
    }
}
