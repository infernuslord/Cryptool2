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
                        break;
                case 1: TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MaxLength", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RowColumnColumn", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RowColumnRow", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ColumnColumnRow", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ColumnColumnColumn", Visibility.Hidden)));
                        break;
            }
        }
        //[PropertySaveOrder(1)]
        [TaskPane("Analysis Method", "Select the Analysis Method", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Bruteforce Analysis", "Analysis with Crib" })]
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
        private int bruteforce_length = 8;
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

        private Boolean row_colum_column = true;
        [PropertySaveOrder(3)]
        [ContextMenu("Bruteforce Row-Column-Column", "Bruteforce this transposition settings: Read in by row. Permute by column. Read out by Column", 4, DisplayLevel.Beginner, ContextMenuControlType.CheckBox, null, "Row-Column-Column")]
        [TaskPane("Bruteforce Row-Column-Column", "Bruteforce this transposition settings: Read in by row. Permute by column. Read out by Column", null, 4, false, DisplayLevel.Expert, ControlType.CheckBox, "")]
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
        [PropertySaveOrder(4)]
        [ContextMenu("Bruteforce Row-Column-Row", "Bruteforce this transposition settings: Read in by row. Permute by column. Read out by Row", 4, DisplayLevel.Beginner, ContextMenuControlType.CheckBox, null, "Row-Column-Column")]
        [TaskPane("Bruteforce Row-Column-Row", "Bruteforce this transposition settings: Read in by row. Permute by column. Read out by Row", null, 4, false, DisplayLevel.Expert, ControlType.CheckBox, "")]
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
        [PropertySaveOrder(5)]
        [ContextMenu("Bruteforce Column-Column-Row", "Bruteforce this transposition settings: Read in by column. Permute by column. Read out by Row", 4, DisplayLevel.Beginner, ContextMenuControlType.CheckBox, null, "Row-Column-Column")]
        [TaskPane("Bruteforce Column-Column-Row", "Bruteforce this transposition settings: Read in by column. Permute by column. Read out by Row", null, 4, false, DisplayLevel.Expert, ControlType.CheckBox, "")]
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
        [PropertySaveOrder(6)]
        [ContextMenu("Bruteforce Column-Column-Column", "Bruteforce this transposition settings: Read in by column. Permute by column. Read out by column", 4, DisplayLevel.Beginner, ContextMenuControlType.CheckBox, null, "Row-Column-Column")]
        [TaskPane("Bruteforce Column-Column-Column", "Bruteforce this transposition settings: Read in by column. Permute by column. Read out by column", null, 4, false, DisplayLevel.Expert, ControlType.CheckBox, "")]
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
