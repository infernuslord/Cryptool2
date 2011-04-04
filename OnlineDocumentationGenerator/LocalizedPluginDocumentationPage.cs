using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Cryptool.PluginBase;

namespace OnlineDocumentationGenerator
{
    public class LocalizedPluginDocumentationPage
    {
        private readonly Type _pluginType;
        private readonly XElement _xml;
        private readonly string _lang;
        private bool _startable;
        private readonly string _name;
        private readonly string _toolTip;

        public string ToolTip
        {
            get { return _toolTip; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Lang
        {
            get { return _lang; }
        }
        
        public LocalizedPluginDocumentationPage(Type pluginType, XElement xml, string lang)
        {
            _pluginType = pluginType;
            _xml = xml;
            _lang = lang;

            CultureInfo cultureInfo = new CultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            _startable = pluginType.GetPluginInfoAttribute().Startable;
            _name = pluginType.GetPluginInfoAttribute().Caption;
            _toolTip = pluginType.GetPluginInfoAttribute().ToolTip;
        }
    }
}
