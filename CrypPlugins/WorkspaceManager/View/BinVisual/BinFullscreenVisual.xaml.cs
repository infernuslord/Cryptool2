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
using System.Collections.ObjectModel;
using WorkspaceManager.View.Base;
using System.ComponentModel;
using WorkspaceManager.Model;

namespace WorkspaceManager.View.BinVisual
{
    /// <summary>
    /// Interaction logic for BinFullscreenVisual.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("WorkspaceManager.Properties.Resources")]
    public partial class BinFullscreenVisual : UserControl, INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Close;
        #endregion

        #region Properties

        public UIElement ActivePresentation
        {
            get
            {
                if (ActiveComponent == null)
                    return null;


                UIElement o = null;

                if (ActiveComponent.State == BinComponentState.Min &&
                    ActiveComponent.LastState == null &&
                    ActiveComponent.HasComponentPresentation)
                {
                    ActiveComponent.Presentations.TryGetValue(BinComponentState.Presentation, out o);
                    return o;
                }

                if (ActiveComponent.State == BinComponentState.Min &&
                    ActiveComponent.LastState == null &&
                    !ActiveComponent.HasComponentPresentation)
                {
                    ActiveComponent.Presentations.TryGetValue(BinComponentState.Log, out o);
                    return o;
                }

                if (ActiveComponent.State == BinComponentState.Min)
                {
                    ActiveComponent.Presentations.TryGetValue((BinComponentState)ActiveComponent.LastState, out o);
                    return o;
                }

                ActiveComponent.Presentations.TryGetValue(ActiveComponent.State, out o);
                return o;
            }
        }



        public bool HasComponentPresentation
        {
            get
            {
                if (ActiveComponent == null)
                    return false;

                UIElement e = null;
                ActiveComponent.Presentations.TryGetValue(BinComponentState.Presentation, out e);
                return e == null ? false : true;
            }
        }

        public bool HasComponentSetting
        {
            get
            {
                if (ActiveComponent == null)
                    return false;

                UIElement e = null;
                ActiveComponent.Presentations.TryGetValue(BinComponentState.Setting, out e);
                return e == null ? false : true;
            }
        }

        private BinComponentVisual lastActiveComponent;
        public BinComponentVisual LastActiveComponent
        {
            set
            {
                lastActiveComponent = value;
            }
            get
            {
                return lastActiveComponent;
            }
        }

        #endregion

        #region DependencyProperties
        public static readonly DependencyProperty ComponentCollectionProperty = DependencyProperty.Register("ComponentCollection",
            typeof(ObservableCollection<BinComponentVisual>), typeof(BinFullscreenVisual), new FrameworkPropertyMetadata(null, null));

        public ObservableCollection<BinComponentVisual> ComponentCollection
        {
            get
            {
                return (ObservableCollection<BinComponentVisual>)base.GetValue(ComponentCollectionProperty);
            }
            set
            {
                base.SetValue(ComponentCollectionProperty, value);
            }
        }

        public static readonly DependencyProperty ActiveComponentProperty = DependencyProperty.Register("ActiveComponent",
            typeof(BinComponentVisual), typeof(BinFullscreenVisual), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnActiveComponentChanged)));

        public BinComponentVisual ActiveComponent
        {
            get
            {
                return (BinComponentVisual)base.GetValue(ActiveComponentProperty);
            }
            set
            {
                base.SetValue(ActiveComponentProperty, value);
            }
        }

        //public static readonly DependencyProperty IsOverviewOpenProperty = DependencyProperty.Register("IsOverviewOpen",
        //    typeof(bool), typeof(BinFullscreenVisual), new FrameworkPropertyMetadata(false, null));

        //public bool IsOverviewOpen
        //{
        //    get
        //    {
        //        return (bool)base.GetValue(IsOverviewOpenProperty);
        //    }
        //    set
        //    {
        //        base.SetValue(IsOverviewOpenProperty, value);
        //    }
        //}

        public static readonly DependencyProperty IsFullscreenOpenProperty = DependencyProperty.Register("IsFullscreenOpen",
            typeof(bool), typeof(BinFullscreenVisual), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsFullscreenOpenChanged)));


        public bool IsFullscreenOpen
        {
            get
            {
                return (bool)base.GetValue(IsFullscreenOpenProperty);
            }
            set
            {
                base.SetValue(IsFullscreenOpenProperty, value);
            }
        }
        #endregion

        #region Constructors
        public BinFullscreenVisual()
        {
            InitializeComponent();
        }
        #endregion

        #region protected
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region EventHandler

        private void CloseClickHandler(object sender, RoutedEventArgs e)
        {
            IsFullscreenOpen = false;
            if (Close != null)
                Close.Invoke(this, new EventArgs());
        }

        private void ActionHandler(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b == null)
                return;

            if (b.Content is BinComponentState && ActiveComponent != null)
            {
                ActiveComponent.State = (BinComponentState)b.Content;
                OnPropertyChanged("ActivePresentation");
                return;
            }
            
            e.Handled = true;
        }

        private void ToggleClickOverviewHandler(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private static void OnActiveComponentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BinFullscreenVisual f = (BinFullscreenVisual)d;
            BinComponentVisual newBin = (BinComponentVisual)e.NewValue;
            BinComponentVisual oldBin = (BinComponentVisual)e.OldValue;
            if (newBin != null)
            {
                newBin.IsFullscreen = true;
            }

            if (oldBin != null)
            {
                f.LastActiveComponent = oldBin;
                if(oldBin != newBin)
                    oldBin.IsFullscreen = false;
            }

            f.OnPropertyChanged("HasComponentPresentation");
            f.OnPropertyChanged("HasComponentSetting");
            f.OnPropertyChanged("ActivePresentation");
        }

        private static void OnIsFullscreenOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BinFullscreenVisual f = (BinFullscreenVisual)d;
            if ((bool)e.NewValue)
            {
                if(f.LastActiveComponent != null)
                    f.LastActiveComponent.IsFullscreen = true;
            }
            else
            {
                if (f.LastActiveComponent != null)
                    f.LastActiveComponent.IsFullscreen = false;
                f.ActiveComponent = null;
            }
            f.OnPropertyChanged("ActivePresentation");
        }
        #endregion



    }
}
