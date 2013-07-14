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
        #region Global Variables

        // Delegates
        private UpdateOutputCiphertext updateOutput;
        private RestartSearch restartSearch;

        // Data Source for DataGrid
        private List<LetterPair> data;

        // Switch Letters
        private DataGridCell swapCell = null;
        private DataGridRow swapRow = null;
        private Style swapStyle = null;

        #endregion

        #region Properties

        public List<LetterPair> Data
        {
            get { return this.data; }
            set { this.data = value; }
        }

        #endregion

        #region constructor

        public AssignmentPresentation()
        {
            InitializeComponent();
        }

        #endregion

        #region Main Methods

        public void RefreshGUI()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                dataGrid1.Items.Refresh();
            }));
        }

        public void ConnectDataSource()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                dataGrid1.ItemsSource = this.data;
            }));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.DoWork += new DoWorkEventHandler(updateOutput_DoWork);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(updateOutput_RunWorkerCompleted);
            bgWorker.RunWorkerAsync();
        }

        private void updateOutput_DoWork(object sender, DoWorkEventArgs e)
        {
            DisableGUI();
            updateOutput(this.data);
        }

        private void updateOutput_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EnableGUI();
            RefreshGUI();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.DoWork += new DoWorkEventHandler(restartSearch_DoWork);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(restartSearch_RunWorkerCompleted);
            bgWorker.RunWorkerAsync();
        }

        private void restartSearch_DoWork(object sender, DoWorkEventArgs e)
        {
            DisableGUI();
            restartSearch();
        }

        private void restartSearch_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EnableGUI();
            RefreshGUI();
        }

        public void SetUpdateOutputCiphertext(UpdateOutputCiphertext method)
        {
            this.updateOutput = method;
        }

        public void SetRestartSearch(RestartSearch method)
        {
            this.restartSearch = method;
        }

        public void DisableGUI()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                btnRestart.IsEnabled = false;
                btnUpdate.IsEnabled = false;
                dataGrid1.IsEnabled = false;
            }));
        }

        public void EnableGUI()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                btnRestart.IsEnabled = true;
                btnUpdate.IsEnabled = true;
                dataGrid1.IsEnabled = true;
            }));
        }

        #endregion

        #region Swap Letters

        private void dataGrid1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            
            while ((dep != null) && !(dep is DataGridCell))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
            {
                return;
            }

            DataGridCell cell = dep as DataGridCell;

            if (this.swapCell == null && cell.Column.DisplayIndex == 1)
            {
                // Store cell infos
                this.swapCell = cell;
                while ((dep != null) && !(dep is DataGridRow))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }
                this.swapRow = dep as DataGridRow;
                this.swapStyle = cell.Style;
                
                // Set new style
                Style style = new Style(typeof(DataGridCell));
                Setter setter = new Setter(DataGridCell.BackgroundProperty, Brushes.LightGreen);
                style.Setters.Add(setter);
                cell.Style = style;
                
            }
            else if (this.swapCell != null && cell.Column.DisplayIndex == 1)
            {
                // Get object of second cell
                while ((dep != null) && !(dep is DataGridRow))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }

                DataGridRow row = dep as DataGridRow;
                
                LetterPair p1 = this.swapRow.Item as LetterPair;
                LetterPair p2 = row.Item as LetterPair;

                string h = p1.Plaintext_letter;
                p1.Plaintext_letter = p2.Plaintext_letter;
                p2.Plaintext_letter = h;

                // Reset after swapped finished
                this.swapCell.Style = this.swapStyle;
                this.swapCell = null;

                RefreshGUI();
            }
            

        }

        #endregion
    }
}
