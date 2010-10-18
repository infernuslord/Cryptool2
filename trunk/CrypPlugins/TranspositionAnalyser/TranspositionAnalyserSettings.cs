using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Windows;
namespace TranspositionAnalyser
{
    class TranspositionAnalyserSettings : ISettings
    {
        private bool has_Changes = false;

        #region ISettings Member

      

        private int selected_method = 0;

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
                return;

            switch (selected_method)
            {
                case 0: TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MaxLength", Visibility.Visible)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RowColumnColumn", Visibility.Visible)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RowColumnRow", Visibility.Visible)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ColumnColumnRow", Visibility.Visible)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ColumnColumnColumn", Visibility.Visible)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("KeySize", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Repeatings", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Iterations", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CribSearchKeylength", Visibility.Hidden)));
                        break;
                case 1: TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MaxLength", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RowColumnColumn", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RowColumnRow", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ColumnColumnRow", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ColumnColumnColumn", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("KeySize", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Repeatings", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Iterations", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CribSearchKeylength", Visibility.Visible)));
                        break;
                case 2: TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MaxLength", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RowColumnColumn", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RowColumnRow", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ColumnColumnRow", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ColumnColumnColumn", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("KeySize", Visibility.Visible)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Repeatings", Visibility.Visible)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Iterations", Visibility.Visible)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CribSearchKeylength", Visibility.Hidden)));
                        break;
            }
        }
        //[PropertySaveOrder(1)]
        [TaskPane("Analysis Method", "Select the Analysis Method", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Bruteforce Analysis", "Analysis with Crib", "Genetic algorithm" })]
        public int Analysis_method
        {
            get
            {
                return this.selected_method;
            }
            set
            {
                if (value != selected_method) HasChanges = true;
                this.selected_method = value;
                UpdateTaskPaneVisibility();
                OnPropertyChanged("Analysis_method");
                
            }

        }

        // FIX: REGEX 
        private int bruteforce_length = 12;
        [PropertySaveOrder(2)]
        [TaskPaneAttribute("Transposition Bruteforce length", "Enter the max length to be bruteforced (max: 20)", null, 2, true, DisplayLevel.Beginner, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int MaxLength
        {
            get { return bruteforce_length; }
            set
            {
                bruteforce_length = value;
                
            }
        }

        private int keysize = 8;
        [PropertySaveOrder(3)]
        [TaskPaneAttribute("Keysize for genetic analysis.", "Enter the keysize to be searched", null, 2, true, DisplayLevel.Beginner, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int KeySize
        {
            get { return keysize; }
            set
            {
                keysize = value;

            }
        }

        private Boolean row_colum_column = true;
        [PropertySaveOrder(4)]
        [ContextMenu("R-C-C", "Bruteforce this transposition settings: Read in by row. Permute by column. Read out by Column", 4, DisplayLevel.Beginner, ContextMenuControlType.CheckBox, null, "Row-Column-Column")]
        [TaskPane("R-C-C", "Bruteforce this transposition settings: Read in by row. Permute by column. Read out by Column", null, 4, false, DisplayLevel.Expert, ControlType.CheckBox, "")]
        public bool RowColumnColumn
        {
            get { return this.row_colum_column; }
            set
            {
                if (value != this.row_colum_column) HasChanges = true;
                this.row_colum_column= value;
                OnPropertyChanged("RowColumnColumn");
            }
        }

        private Boolean row_colum_row = true;
        [PropertySaveOrder(5)]
        [ContextMenu("R-C-R", "Bruteforce this transposition settings: Read in by row. Permute by column. Read out by Row", 4, DisplayLevel.Beginner, ContextMenuControlType.CheckBox, null, "Row-Column-Column")]
        [TaskPane("R-C-R", "Bruteforce this transposition settings: Read in by row. Permute by column. Read out by Row", null, 4, false, DisplayLevel.Expert, ControlType.CheckBox, "")]
        public bool RowColumnRow
        {
            get { return this.row_colum_row; }
            set
            {
                if (value != this.row_colum_row) HasChanges = true;
                this.row_colum_row = value;
                OnPropertyChanged("RowColumnRow");
            }
        }


        private Boolean column_colum_row = true;
        [PropertySaveOrder(6)]
        [ContextMenu("C-C-R", "Bruteforce this transposition settings: Read in by column. Permute by column. Read out by Row", 4, DisplayLevel.Beginner, ContextMenuControlType.CheckBox, null, "Row-Column-Column")]
        [TaskPane("C-C-R", "Bruteforce this transposition settings: Read in by column. Permute by column. Read out by Row", null, 4, false, DisplayLevel.Expert, ControlType.CheckBox, "")]
        public bool ColumnColumnRow
        {
            get { return this.column_colum_row; }
            set
            {
                if (value != this.column_colum_row) HasChanges = true;
                this.column_colum_row = value;
                OnPropertyChanged("ColumnColumnRow");
            }
        }

        private Boolean column_colum_column = true;
        [PropertySaveOrder(7)]
        [ContextMenu("C-C-C", "Bruteforce this transposition settings: Read in by column. Permute by column. Read out by column", 4, DisplayLevel.Beginner, ContextMenuControlType.CheckBox, null, "Row-Column-Column")]
        [TaskPane("C-C-C", "Bruteforce this transposition settings: Read in by column. Permute by column. Read out by column", null, 4, false, DisplayLevel.Expert, ControlType.CheckBox, "")]
        public bool ColumnColumnColumn
        {
            get { return this.column_colum_column; }
            set
            {
                if (value != this.column_colum_column) HasChanges = true;
                this.column_colum_column = value;
                OnPropertyChanged("ColumnColumnColumn");
            }
        }

        private int repeatings = 10;
        [PropertySaveOrder(8)]
        [TaskPaneAttribute("Numbers of repeatings for genetic analysis.", "Enter the maximum number of repeations of iterations for genetic analysis.", null, 2, true, DisplayLevel.Beginner, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int Repeatings
        {
            get { return repeatings; }
            set
            {
                if (value != this.repeatings) HasChanges = true;
                repeatings= value;
                OnPropertyChanged("Repeatings");
            }
        }

        private int iterations = 5000;
        [PropertySaveOrder(9)]
        [TaskPaneAttribute("Numbers of iterations for genetic analysis.", "Enter the maximum number of iterations for genetic analysis.", null, 2, true, DisplayLevel.Beginner, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int Iterations
        {
            get { return iterations; }
            set
            {
                if (value != this.iterations) HasChanges = true;
                iterations = value;
                OnPropertyChanged("Iterations");
            }
        }

        private int cribSearchKeylength = 12;
        [PropertySaveOrder(10)]
        [TaskPaneAttribute("Maximum keylength for crib analysis", "Enter the maximum keylength for the crib based analysis.", null, 2, true, DisplayLevel.Beginner, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int CribSearchKeylength
        {
            get { return cribSearchKeylength; }
            set
            {
                if (value != this.cribSearchKeylength) HasChanges = true;
                cribSearchKeylength = value;
                OnPropertyChanged("CribSearchKeylength");
            }
        }


        public bool HasChanges
        {
            get
            {
                return has_Changes;
            }
            set
            {
                has_Changes = true;
            }
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region Events
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
        #endregion


        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
