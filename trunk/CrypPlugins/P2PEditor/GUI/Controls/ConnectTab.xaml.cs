using System;
using System.Windows;
using Cryptool.P2P;

namespace Cryptool.P2PEditor.GUI.Controls
{
    public partial class ConnectTab
    {
        public static readonly DependencyProperty IsP2PConnectingProperty =
            DependencyProperty.Register("IsP2PConnecting",
                                        typeof(
                                            Boolean),
                                        typeof(
                                            ConnectTab), new PropertyMetadata(false));

        public Boolean IsP2PConnecting
        {
            get { return (Boolean)GetValue(IsP2PConnectingProperty); }
            set { SetValue(IsP2PConnectingProperty, value); }
        }

        public ConnectTab()
        {
            InitializeComponent();
        }

        private void ConnectButtonClick(object sender, RoutedEventArgs e)
        {
            if (!P2PManager.IsConnected)
                P2PManager.Connect();

            ((P2PEditorPresentation) P2PEditor.Presentation).UpdateConnectionState();
        }
    }
}
