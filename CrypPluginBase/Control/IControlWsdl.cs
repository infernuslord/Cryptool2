using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Cryptool.PluginBase.Control
{
    public interface IControlWsdl : IControl
    {
        XmlDocument Wsdl
        {
            set;
            get;
        }

        void setWsdl(XmlDocument wsdlDocument);
    }
}
