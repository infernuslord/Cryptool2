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
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Threading;

namespace Cryptool.Plugins.AnalysisMonoalphabeticSubstitution
{
    /// <summary>
    /// Interaktionslogik für AssignmentPresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("AnalysisMonoalphabeticSubstitution.Properties.Resources")]
    public partial class AssignmentPresentation : UserControl
    {

        public ObservableCollection<ResultEntry> entries = new ObservableCollection<ResultEntry>();
        //public event EventHandler doppelClick;

        #region Variables

        private UpdateOutput updateOutputFromUserChoice;

        #endregion

        #region Properties

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
            this.DataContext = entries;

        }

        #endregion

        #region Main Methods

        public void DisableGUI()
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                this.ListView.IsEnabled = false;
            }, null);
        }

        public void EnableGUI()
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                this.ListView.IsEnabled = true;
            }, null);
        }

        #endregion

        #region Helper Methods

        public void HandleDoubleClick(Object sender, EventArgs eventArgs)
        {
            ListViewItem lvi = sender as ListViewItem;
            ResultEntry r = lvi.Content as ResultEntry;

            if (r != null)
            {
                this.updateOutputFromUserChoice(r.Key, r.Text);
            }
        }

        public void HandleSingleClick(Object sender, EventArgs eventArgs)
        {
            //this.updateOutputFromUserChoice(0);
        }

        #endregion
    }
}
