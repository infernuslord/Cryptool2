using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace Cryptool.Plugins.AnalysisMonoalphabeticSubstitution
{
    /// <summary>
    /// Interaktionslogik für AssignmentPresentation.xaml
    /// </summary>
    public partial class AssignmentPresentation : UserControl
    {
        #region Variables

        // Delegates
        private UpdateOutput updateOutputFromUserChoice;

        // Data Source for DataGrid
        private List<KeyCandidate> keyCandidates;
        private List<PlainDisplay> keyCandidatePlaintexts;

        #endregion

        #region Properties

        public List<KeyCandidate> KeyCandidates
        {
            get { return this.keyCandidates; }
            set { this.keyCandidates = value; }
        }

        public UpdateOutput UpdateOutputFromUserChoice
        {
            get { return this.updateOutputFromUserChoice; }
            set { this.updateOutputFromUserChoice = value; }
        }

        #endregion

        #region constructor

        public AssignmentPresentation()
        {
            InitializeComponent();
            this.keyCandidatePlaintexts = new List<PlainDisplay>();
            this.ConnectDataSource();
        }

        #endregion

        #region Main Methods

        public void RefreshGUI()
        {
            if (!(this.keyCandidates == null))
            {
                this.keyCandidatePlaintexts.RemoveRange(0, this.keyCandidatePlaintexts.Count);
                for (int i = 0; i < this.keyCandidates.Count; i++)
                {
                    PlainDisplay pd = new PlainDisplay();
                    pd.Rank = i;
                    pd.Plaintext = this.keyCandidates[i].Plaintext;
                    this.keyCandidatePlaintexts.Add(pd);
                }
                this.Dispatcher.Invoke((Action)(() =>
                {
                    dataGrid1.Items.Refresh();
                }));
            }
        }

        public void DisableGUI()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                dataGrid1.IsEnabled = false;
            }));
        }

        public void EnableGUI()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                dataGrid1.IsEnabled = true;
            }));
        }

        #endregion

        #region Helper Methods

        private void ConnectDataSource()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                dataGrid1.ItemsSource = this.keyCandidatePlaintexts;
            }));
        }

        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = 0;
            this.Dispatcher.Invoke((Action)(() =>
            {
                DataGrid dg = (DataGrid)sender;
                DataGridRow row = (DataGridRow)dg.ItemContainerGenerator.ContainerFromItem(dg.SelectedItem);
                if (row != null)
                {
                    index = row.GetIndex();
                }
            }));

            updateOutputFromUserChoice(index);
        }

        #endregion

        class PlainDisplay
        {
            public int Rank
            {
                get;
                set;
            }

            public String Plaintext
            {
                get;
                set;
            }
        }

    }
}
