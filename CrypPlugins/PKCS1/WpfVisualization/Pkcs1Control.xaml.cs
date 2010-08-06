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
using System.Threading;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Tool;
using PKCS1.WpfControls;
using PKCS1.WpfControls.Start;
using PKCS1.WpfControls.SigGen;
using PKCS1.WpfControls.SigGenFake;
using PKCS1.WpfControls.SigVal;
using PKCS1.WpfControls.RsaKeyGen;
using PKCS1.Library;
using PKCS1.WpfVisualization.Navigation;
using PKCS1.WpfVisualization;


namespace PKCS1.WpfVisualization
{
    /// <summary>
    /// Interaktionslogik für pkcs1control.xaml
    /// </summary>
    public partial class Pkcs1Control : UserControl
    {
        private IPkcs1UserControl m_ActualControl = null;
        private IPkcs1UserControl m_RsaKeyGenControl = null;
        private IPkcs1UserControl m_StartControl = null;
        private IPkcs1UserControl m_SigGenControl = null;
        private IPkcs1UserControl m_SigGenFakeBleichenbControl = null;
        private IPkcs1UserControl m_SigGenFakeShortControl = null;
        private IPkcs1UserControl m_SigValControl = null;

        //public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public Pkcs1Control()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            //ControlHandler.Dispatcher = this.Dispatcher;
            navigator.OnNavigate += Navigate;

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                Navigate(NavigationCommandType.Start);
            }, null);
            //(m_StartControl as StartControl).OnStartpageLinkClick += new Navigate(Navigate);
            //this.MouseRightButtonDown += new MouseButtonEventHandler(PrimesControl_MouseRightButtonDown);
        }

        private void Navigate(NavigationCommandType type)
        {
            if (m_ActualControl != null)
            {
                m_ActualControl.Dispose();
            }
            SetTitle(type);

            switch (type)
            {
                case NavigationCommandType.RsaKeyGen:
                    if (m_RsaKeyGenControl == null) m_RsaKeyGenControl = new RsaKeyGenControl();
                    SetUserControl(m_RsaKeyGenControl);
                    break;
                case NavigationCommandType.SigGen:
                    if (m_SigGenControl == null) m_SigGenControl = new SigGenPkcs1Control();
                    SetUserControl(m_SigGenControl);
                    break;
                case NavigationCommandType.SigGenFakeBleichenb:
                    if (m_SigGenFakeBleichenbControl == null) m_SigGenFakeBleichenbControl = new SigGenFakeBleichenbControl();
                    SetUserControl(m_SigGenFakeBleichenbControl);
                    break;
                case NavigationCommandType.SigGenFakeShort:
                    if (m_SigGenFakeShortControl == null) m_SigGenFakeShortControl = new SigGenFakeShortControl();
                    SetUserControl(m_SigGenFakeShortControl);
                    break;
                case NavigationCommandType.SigVal:
                    if (m_SigValControl == null) m_SigValControl = new SigValControl();
                    SetUserControl(m_SigValControl);
                    break;
                case NavigationCommandType.Start:
                    if (m_StartControl == null) m_StartControl = new StartControl();
                    SetUserControl(m_StartControl);
                    break;
            }

        }

        private void SetTitle(NavigationCommandType type)
        {
            switch (type)
            {
                case NavigationCommandType.RsaKeyGen:
                    lblTitel.Content = "RSA-Schlüssel generieren";
                    break;
                case NavigationCommandType.Start:
                    lblTitel.Content = "Startseite";
                    break;
                case NavigationCommandType.SigGen:
                    lblTitel.Content = "PKCS #1-Signatur generieren";
                    break;
                case NavigationCommandType.SigGenFakeBleichenb:
                    lblTitel.Content = "Bleichenbacher Angriff";
                    break;
                case NavigationCommandType.SigGenFakeShort:
                    lblTitel.Content = "Angriff mit kürzeren Schlüsseln";
                    break;
                case NavigationCommandType.SigVal:
                    lblTitel.Content = "PKCS #1-Signatur validieren";
                    break;
            }
        }

        private void SetUserControl(IPkcs1UserControl control)
        {
            SetUserControl(control, 0);
        }

        private void SetUserControl(IPkcs1UserControl control, int tab)
        {
            /*if (tab >= 0)
            {
                try
                {
                    control.SetTab(tab);
                }
                catch { }
            }*/
            (control as UserControl).HorizontalAlignment = HorizontalAlignment.Stretch;
            (control as UserControl).VerticalAlignment = VerticalAlignment.Stretch;
            ContentArea.Content = control as UserControl;
            
            m_ActualControl = control;
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //kein code
        }

        internal void Dispose()
        {
            if (m_RsaKeyGenControl != null) m_RsaKeyGenControl.Dispose();
            if (m_StartControl != null) m_StartControl.Dispose();
            if (m_SigGenControl != null) m_SigGenControl.Dispose();
            if (m_SigGenFakeBleichenbControl != null) m_SigGenFakeBleichenbControl.Dispose();
            if (m_SigGenFakeShortControl != null) m_SigGenFakeShortControl.Dispose();
            if (m_SigValControl != null) m_SigValControl.Dispose();

            m_RsaKeyGenControl = null;
            m_StartControl = null;
            m_SigGenControl = null;
            m_SigGenFakeBleichenbControl = null;
            m_SigGenFakeShortControl = null;
            m_SigValControl = null;
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PKCS1.OnlineHelp.OnlineHelpActions action = PKCS1.OnlineHelp.OnlineHelpActions.StartControl;

            if (m_ActualControl.GetType() == typeof(RsaKeyGenControl))
            {
                action = PKCS1.OnlineHelp.OnlineHelpActions.KeyGen;
            }
            else if (m_ActualControl.GetType() == typeof(SigGenPkcs1Control))
            {
                action = PKCS1.OnlineHelp.OnlineHelpActions.SigGen;
            }
            else if (m_ActualControl.GetType() == typeof(SigGenFakeBleichenbControl))
            {
                action = PKCS1.OnlineHelp.OnlineHelpActions.SigGenFakeBleichenbacher;
            }
            else if (m_ActualControl.GetType() == typeof(SigGenFakeShortControl))
            {
                action = PKCS1.OnlineHelp.OnlineHelpActions.SigGenFakeKuehn;
            }
            else if (m_ActualControl.GetType() == typeof(SigValControl))
            {
                action = PKCS1.OnlineHelp.OnlineHelpActions.SigVal;
            }

            e.Handled = true;
            OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(action);
        }
    }
}
