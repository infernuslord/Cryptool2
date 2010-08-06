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
using System.Reflection;
using System.IO;

namespace PKCS1.OnlineHelp
{
    public delegate void Close();

    /// <summary>
    /// Interaktionslogik für WindowOnlineHelp.xaml
    /// </summary>
    public partial class WindowOnlineHelp : Window
    {
        private static readonly string HELPPROTOCOL = "help://";
        //private static readonly string IMGREGEX = "<img src=\"(.+)\".+>";
        //private static readonly string IMGSRCREGEX = "src=\".+\" ";

        private int m_actualPage;
        IList<string> m_History;
        private System.Windows.Forms.WebBrowser m_Browser = null;
        public event Close OnClose;


        public WindowOnlineHelp()
        {
            InitializeComponent();
            m_Browser = new System.Windows.Forms.WebBrowser();
            //m_Browser.Dock = DockStyle.Fill;
            m_Browser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(m_Browser_Navigating);
            webbrowserhost.Child = m_Browser;
            m_actualPage = -1;
            m_History = new List<string>();
        }

        void m_Browser_Navigating(object sender, System.Windows.Forms.WebBrowserNavigatingEventArgs e)
        {
            string url = e.Url.OriginalString;
            if (!string.IsNullOrEmpty(url))
            {
                if (url.StartsWith(HELPPROTOCOL))
                {
                    url = url.Substring(HELPPROTOCOL.Length, url.Length - HELPPROTOCOL.Length);
                    if (url.EndsWith("/"))
                        url = url.Substring(0, url.Length - 1);
                    NavigateTo(url);
                    e.Cancel = true;
                }

            }
        }

        public void NavigateTo(string action)
        {
            string text = OnlineHelpAccess.HelpResourceManager.GetString(action);
            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    ShowHelp(text);
                    m_actualPage++;
                    m_History.Insert(m_actualPage, action);
                    for (int i = 2; i < m_History.Count; i++)
                        m_History.RemoveAt(i);
                }
                catch { }
            }
            SetEnableNavigationButtons();
        }

        private void ShowHelp(string text)
        {
            string htmltemplate = PKCS1.Properties.Resources.template;
            text = htmltemplate.Replace("#content#", text);
            //text = SetImages(text);
            m_Browser.DocumentText = text;
            this.Show();
            this.Activate();
        }

        private void btnHistoryBack_Click(object sender, RoutedEventArgs e)
        {
            if (m_actualPage > -1)
            {
                string text = OnlineHelpAccess.HelpResourceManager.GetString(m_History[m_actualPage - 1]);
                if (!string.IsNullOrEmpty(text))
                {
                    ShowHelp(text);
                    m_actualPage--;
                }
            }
            SetEnableNavigationButtons();
        }

        private void btnHistoryForward_Click(object sender, RoutedEventArgs e)
        {
            if (m_actualPage < m_History.Count - 1)
            {
                string text = OnlineHelpAccess.HelpResourceManager.GetString(m_History[m_actualPage + 1]);
                if (!string.IsNullOrEmpty(text))
                {
                    ShowHelp(text);
                    m_actualPage++;
                }
            }
            SetEnableNavigationButtons();
        }

        private void SetEnableNavigationButtons()
        {
            btnHistoryBack.IsEnabled = m_History.Count > 0 && m_actualPage > 0;
            btnHistoryForward.IsEnabled = m_History.Count == 2 && m_actualPage == 0;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (OnClose != null) OnClose();
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape) this.Close();
        }
    }
}
