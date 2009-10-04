using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web.Services.Description;
using System.Xml.Schema;
using System.IO;
using System.Data;
using System.Windows.Controls;
using System.Collections;

namespace WebService
{
    public class SecurityPolicy
    {
        private XmlDocument doc, soap;
        private WebService webService;
        private XmlElement policy, signedElements,signedParts, encryptedElements,contentEncryptedElements ;
        private ArrayList encryptedElementsList, XPathArray;
        private string XPath;
        public SecurityPolicy(WebService webService)
        {
            doc = new XmlDocument();
            policy = doc.CreateElement("wsp", "Policy", "http://www.w3.org/ns/ws-policy");
            XmlAttribute securityPolicyNamespace = doc.CreateAttribute("xmlns:sp");
            securityPolicyNamespace.Value = "http://docs.oasis-open.org/ws-sx/ws-securitypolicy/200802";
            policy.Attributes.Append(securityPolicyNamespace);
            doc.AppendChild(policy);

            signedElements = doc.CreateElement("sp", "SignedElements", "http://docs.oasis-open.org/ws-sx/ws-securitypolicy/200802");
            encryptedElements = doc.CreateElement("sp", "EncryptedElements", "http://docs.oasis-open.org/ws-sx/ws-securitypolicy/200802");
            signedParts = doc.CreateElement("sp", "SignedParts", "http://docs.oasis-open.org/ws-sx/ws-securitypolicy/200802");
            this.webService = webService;
            XPath = "";
            XPathArray = new ArrayList();

            
            encryptedElementsList = new ArrayList();
        }
        public ArrayList getEncryptedElementsList()
        {
            return this.getEncryptedElementsList();


        }

        public void loadWSDL(string filename)
        {

       

            ServiceDescription t = this.webService.serviceDescription;

            // Initialize a service description importer.

            ServiceDescription serviceDescription = t.Services[0].ServiceDescription;
            Types types = serviceDescription.Types;
            PortTypeCollection portTypes = serviceDescription.PortTypes;
            MessageCollection messages = serviceDescription.Messages;
            XmlSchema schema = types.Schemas[0];
            PortType porttype = portTypes[0];
            Operation operation = porttype.Operations[0];
            OperationInput input = operation.Messages[0].Operation.Messages.Input;
            Message message = messages[input.Message.Name];

            MessagePart messagePart = message.Parts[0];
            //        XmlSchemaObject fsdf = types.Schemas[0].Elements[messagePart.Element];
            XmlSchema fsdf = types.Schemas[0];

            if (fsdf == null)
            {
                Console.WriteLine("Test");
            }
            StringWriter twriter = new StringWriter();
            //  TextWriter writer= new TextWriter(twriter);
            fsdf.Write(twriter);


            DataSet set = new DataSet();
            StringReader sreader = new StringReader(twriter.ToString());
            XmlTextReader xmlreader = new XmlTextReader(sreader);
            set.ReadXmlSchema(xmlreader);


           

            soap = new XmlDocument();
            //    XmlNode node,envelope,header, body, securityHeader;

           XmlNode node = soap.CreateXmlDeclaration("1.0", "ISO-8859-1", "yes");

            soap.AppendChild(node);
           XmlElement envelope = soap.CreateElement("s:Envelope", "http://www.w3.org/2001/12/soap-envelope");

            
            soap.AppendChild(envelope);

            XmlElement body = soap.CreateElement("s:Body", "http://www.w3.org/2001/12/soap-envelope");
            XmlNode eingabe = soap.CreateElement("tns",set.Tables[0].ToString(), set.Tables[0].Namespace);
            DataTable table = set.Tables[0];
            foreach (DataColumn tempColumn in table.Columns)
            {
                XmlNode neu = soap.CreateElement("tns",tempColumn.ColumnName, set.Tables[0].Namespace);
                eingabe.AppendChild(neu);
            }
            body.AppendChild(eingabe);
            envelope.AppendChild(body);

            StringWriter tim = new StringWriter();
            XmlTextWriter jan = new XmlTextWriter(tim);
            jan.Formatting = Formatting.Indented;
            soap.WriteContentTo(jan);
            // presentation.textBox1.Text =   tim.ToString();
          //  reader.Close();

            XmlNode rootElement = soap.SelectSingleNode("/*");
           this.webService.presentation.inboundPolicy  = new System.Windows.Controls.TreeViewItem();


            if (rootElement.Attributes != null)
            {
            }
         //   presentation.origSoapItem.IsExpanded = true;

            StackPanel panel1 = new StackPanel();
            StackPanel origSoapPanel = new StackPanel();
            StackPanel origSoapPanel2 = new StackPanel();

            panel1.Orientation = System.Windows.Controls.Orientation.Horizontal;
            origSoapPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            origSoapPanel2.Orientation = System.Windows.Controls.Orientation.Horizontal;

            TextBlock elem1 = new TextBlock();
            elem1.Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
            TextBlock origSoapElem = new TextBlock();
            origSoapElem.Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
            TextBlock origSoapElem2 = new TextBlock();
            origSoapElem2.Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";


            panel1.Children.Insert(0, elem1);
            origSoapPanel.Children.Insert(0, origSoapElem);
            origSoapPanel2.Children.Insert(0, origSoapElem2);

            webService.presentation.inboundPolicy.Header = panel1;
            this.webService.presentation.namespacesTable.Clear();
            this.webService.presentation.CopyXmlToTreeView(rootElement, ref this.webService.presentation.inboundPolicy,true);


           // TreeViewItem tr = (TreeViewItem) this.webService.presentation.policyTreeview.Items[0];


         //   this.webService.presentation.treeView2.Items.Add(this.webService.presentation.inboundPolicy);
            this.webService.presentation.inboundPolicy.IsExpanded = true;
          //  this.webService.presentation.treeView2.Items.Refresh();
         //  string visible= this.webService.presentation.policyTreeview.Visibility.;
         //   this.webService.presentation.policyTreeview.Items.Add(tr); ;
             //   (this.webService.presentation.inboundPolicy);
         
          // this.InputString = this.soap;



        }
        public void addSignedElementsAssertion(string elementName)
        {
            string xpath = this.getXPathForElement(elementName);
            XmlElement xpathElement = doc.CreateElement( "sp:XPath", "http://docs.oasis-open.org/ws-sx/ws-securitypolicy/200802");
            xpathElement.InnerText = xpath;
            signedElements.AppendChild(xpathElement);
            policy.AppendChild(signedElements);


        }
        public void addSignedPartsAssertion(string name, string nameSpace)
        {
            XmlElement sigendPartsAssertion = doc.CreateElement("sp", "Header", "http://docs.oasis-open.org/ws-sx/ws-securitypolicy/200802");
            XmlAttribute nameAttribute = doc.CreateAttribute("Name");
            nameAttribute.Value = name;
            XmlAttribute nameSpaceAttribute = doc.CreateAttribute("Namespace");
            nameSpaceAttribute.Value = nameSpace;
            sigendPartsAssertion.Attributes.Append(nameAttribute);
            sigendPartsAssertion.Attributes.Append(nameSpaceAttribute);
            signedParts.AppendChild(sigendPartsAssertion);
            policy.AppendChild(signedParts);
            
        }
        public void addSignedPartsAssertion(string nameSpace)
        {
            XmlElement sigendPartsAssertion = doc.CreateElement("sp", "Header", "http://docs.oasis-open.org/ws-sx/ws-securitypolicy/200802");
           
            XmlAttribute nameSpaceAttribute = doc.CreateAttribute("Namespace");
            nameSpaceAttribute.Value = nameSpace;
 
            sigendPartsAssertion.Attributes.Append(nameSpaceAttribute);
            signedParts.AppendChild(sigendPartsAssertion);
            policy.AppendChild(signedParts);

        }

        public void addEncryptedElementsAssertion(string elementName)
        {
            string xpath = this.getXPathForElement(elementName);
            XmlElement xpathElement = doc.CreateElement("sp", "XPath", "http://docs.oasis-open.org/ws-sx/ws-securitypolicy/200802");
            xpathElement.InnerText = xpath;
            encryptedElements.AppendChild(xpathElement);
            policy.AppendChild(encryptedElements);
            XmlElement b =(XmlElement) this.soap.GetElementsByTagName("s:Body")[0];
            XmlNamespaceManager manager = new XmlNamespaceManager(this.soap.NameTable);
            manager.AddNamespace("s", b.NamespaceURI);
            manager.AddNamespace("tns", "http://tempuri.org/");
            XmlElement t = (XmlElement)soap.SelectSingleNode(xpath,manager);

            this.encryptedElementsList.Add(t);
            this.webService.presentation.namespacesTable.Clear();
            this.webService.presentation.resetPolicyItem();
            
          
          
            this.loadWSDL("");
            

        }
        public void addEncryptedPartsAssertion()
        {

        }
        public void addContentEncryptedElementsAssertion(string xpath)
        {
            XmlElement xpathElement = doc.CreateElement("sp", "XPath", "http://docs.oasis-open.org/ws-sx/ws-securitypolicy/200802");
            xpathElement.InnerText = xpath;
            contentEncryptedElements.AppendChild(xpathElement);
            policy.AppendChild(contentEncryptedElements);
        }
        public string getXPathForElement(string elementName)
        {
            if (elementName.Equals("Envelope"))
            {
                return "/s:Envelope";
            }
            if (elementName.Equals("Body"))
            {
                return "/s:Envelope/s:Body";
            }
           return  getXPath(elementName,"http://tempuri.org/");
        }
        private string getXPath(string elementName, string nameSpace)
    {
        XmlElement element = (XmlElement)this.soap.GetElementsByTagName(elementName, nameSpace)[0];
        this.XPath = "";
        this.XPathArray.Clear();
       string t= this.rekursivXPathSearch(element);
        return t;
    }
        public string rekursivXPathSearch(XmlElement element)
        {

            string XPathString = "";
            if (!element.Name.Contains("Envelope"))
            {
                if (this.XPath.Equals(""))
                {
                    XPathArray.Add(element.Name);
                    rekursivXPathSearch((XmlElement)element.ParentNode);
                }
            }
            else
            {
                if (this.XPath.Equals(""))
                {
                    XPathArray.Add(element.Name);
                }
            } if (this.XPath.Equals(""))
            {
                for (int i = XPathArray.Count - 1; i >= 0; i--)
                {
                    this.XPath += "/" + XPathArray[i];
                }
            }
                    XPathString = this.XPath;
               
                
                return XPathString;
             
             

        }
        public XmlDocument getPolicy()
        
        {
            return this.doc;
        }
    }

}
