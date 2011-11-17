using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Windows;

namespace Cryptool.FrequencyTest
{
    public class FrequencyTestSettings : ISettings
    {

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion



        #region Private variables

        private int unknownSymbolHandling = 0;
        //private int trimInputString=0;
        private int caseSensitivity = 0;
        private int grammLength = 1;
        private int boundaryFragments = 0;
        private bool autozoom = true;
        private int chartHeight = 160;
        private int scale = 10000; // = 1 , factor of 10000

        #endregion

        #region Private helper methods

        private void showSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
            }
        }

        private void hideSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
            }
        }

        #endregion

        #region Public events and methods

        /// <summary>
        /// This event is needed in order to render settings elements visible/invisible
        /// </summary>
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        #endregion

        #region Visible settings
        
        /// <summary>
        /// Visible setting how to deal with alphabet case. 0 = case insentive, 1 = case sensitive
        /// </summary>
        [PropertySaveOrder(1)]
        [ContextMenu( "CaseSensitivityCaption", "CaseSensitivityTooltip", 7, ContextMenuControlType.ComboBox, null, new string[] { "CaseSensitivityList1", "CaseSensitivityList2" })]
        [TaskPane( "CaseSensitivityCaption", "CaseSensitivityTooltip", "", 7,false, ControlType.ComboBox, new string[] { "CaseSensitivityList1", "CaseSensitivityList2" })]
        public int CaseSensitivity
        {
            get { return this.caseSensitivity; }
            set
            {
                if (value != caseSensitivity)
                {
                    caseSensitivity = value;
                    OnPropertyChanged("CaseSensitivity");
                }
            }
        }

        [PropertySaveOrder(2)]
        [TaskPane( "GrammLengthCaption", "GrammLengthTooltip", "", 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int GrammLength
        {
            get { return this.grammLength; }
            set
            {
                if (value != grammLength)
                {
                    grammLength = value;
                    OnPropertyChanged("GrammLength");
                }
            }
        }

        [PropertySaveOrder(3)]
        [ContextMenu( "ProcessUnknownSymbolsCaption", "ProcessUnknownSymbolsTooltip", 4, ContextMenuControlType.ComboBox, null, new string[] { "ProcessUnknownSymbolsList1", "ProcessUnknownSymbolsList2" })]
        [TaskPane( "ProcessUnknownSymbolsCaption", "ProcessUnknownSymbolsTooltip", null, 4, false, ControlType.ComboBox, new string[] { "ProcessUnknownSymbolsList1", "ProcessUnknownSymbolsList2" })]
        public int ProcessUnknownSymbols
        {
            get { return this.unknownSymbolHandling; }
            set
            {
                if (value != unknownSymbolHandling)
                {
                    unknownSymbolHandling = value;
                    OnPropertyChanged("ProcessUnknownSymbols");
                }
            }
        }

        /// <summary>
        /// This option is to choose whether additional n-grams shall be used at word boundary for n-grams with n>=2.
        /// Example trigrams for the word "cherry":
        /// che
        /// her
        /// err
        /// rry
        /// The following fragments at word boundary may be included optionally:
        /// __c
        /// _ch
        /// ry_
        /// y__
        /// The underline char represents a whitespace.
        /// </summary>
        [PropertySaveOrder(4)]
        [TaskPane("BoundaryFragmentsCaption", "BoundaryFragmentsTooltip", "", 10, false, ControlType.ComboBox, new string[] { "BoundaryFragmentsList1", "BoundaryFragmentsList2" })]
        public int BoundaryFragments
        {
            get { return this.boundaryFragments; }
            set
            {
                if (value != boundaryFragments)
                {
                    boundaryFragments = value;
                    OnPropertyChanged("BoundaryFragments");
                }
            }
        }

        [PropertySaveOrder(5)]
        [TaskPane("AutozoomCaption", "AutozoomTooltip", "PresentationGroup", 20, true, ControlType.CheckBox)]
        public bool Autozoom
        {
            get { return this.autozoom; }
            set
            {
                if (value != autozoom)
                {
                    autozoom = value;

                    if (autozoom)
                        hideSettingsElement("ChartHeight");
                    else
                        showSettingsElement("ChartHeight");


                    OnPropertyChanged("Autozoom");
                }
            }
        }


        [PropertySaveOrder(6)]
        [TaskPane("ChartHeightCaption", "ChartHeightTooltip", "PresentationGroup", 21, true, ControlType.NumericUpDown, ValidationType.RangeInteger, 10, 1000)]
        public int ChartHeight
        {
            get { return this.chartHeight; }
            set
            {
                if (value != chartHeight)
                {
                    chartHeight = value;
                    OnPropertyChanged("ChartHeight");
                }
            }
        }


        [PropertySaveOrder(7)]
        [TaskPane("ScaleCaption", "ScaleTooltip", "PresentationGroup", 22, true, ControlType.Slider, 5, 20000)]
        public int Scale
        {
            get { return scale; }
            set
            {
                if (scale != value)
                {
                    scale = value;
                    OnPropertyChanged("Scale");   
                }
            }
        }

        #endregion
        
    }
}
