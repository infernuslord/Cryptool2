using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Xml;
using System.Xml.Schema;
using System.Web;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.Data;
using System.IO;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Windows.Controls;
using System.Collections;
using System.Windows.Threading;
using System.Threading;
using Cryptool.PluginBase.Control;


namespace Soap
{
    [Author("Tim Podeszwa", "tim.podeszwa@student.uni-siegen.de", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo(true, "SoapMessage", "Represents a SoapMessage", "", "Soap/soap.png")]
    public class Soap:IThroughput
    {
        private ISettings settings = new SoapSettings();
        private SoapPresentation presentation; 
      
        private XmlNode node, envelope, body;
        public XmlDocument soap, inputDocument, outputString;
        public XmlDocument securedSOAP;
        private string[] signedElements;
        public Hashtable idTable;
        private bool bodySigned, methodNameSigned, bodyEncrypted, methodNameEncrypted ,secHeaderEnc,secHeaderSigned;
        private int contentCounter;
        private RSACryptoServiceProvider wsRSACryptoProv,rsaCryptoProv;
        private DSACryptoServiceProvider dsaCryptoProv;
        private string wsPublicKey;
        public bool gotKey;
        public bool wsdlLoaded,loaded;
        public string lastSessionKey;
        public bool hadHeader;
        public bool send = false;



        /// <summary>
        /// Encryption Variablen rausnehmen
        /// </summary>
        private CspParameters cspParams;
        private RSACryptoServiceProvider rsaKey;

        private struct EncryptionSettings
        {
            public string key;
            public bool content;
            public bool showsteps;
        }
        private struct SignatureSettings
        {
            public string sigAlg;
            public bool Xpath;
            public bool showsteps;
        }
        private EncryptionSettings encset;
        private SignatureSettings sigset;
        
       
        public Soap()
        {   
            soap = new XmlDocument();
            gotKey = false;
            this.presentation = new SoapPresentation(this);
            settings.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(settings_PropertyChanged);
            wsdlLoaded = false;
            idTable = new Hashtable();
            soap = new XmlDocument();
            encset = new EncryptionSettings();
            sigset = new SignatureSettings();
            cspParams = new CspParameters();
            cspParams.KeyContainerName = "XML_ENC_RSA_KEY";
            rsaKey = new RSACryptoServiceProvider(cspParams);
            rsaCryptoProv = new RSACryptoServiceProvider();
            dsaCryptoProv = new DSACryptoServiceProvider();
            wsRSACryptoProv = new RSACryptoServiceProvider();
            securedSOAP = new XmlDocument();
            soap = new XmlDocument();
            mySettings.idtable = idTable;
            rsaCryptoProv.ToXmlString(false);
            mySettings.rsacryptoProv = rsaCryptoProv.ToXmlString(true);
            mySettings.dsacryptoProv = dsaCryptoProv.ToXmlString(true);
            mySettings.wsRSAcryptoProv = wsRSACryptoProv.ToXmlString(false);
            contentCounter = 0;
            mySettings.securedsoap = xmlToString(securedSOAP);
            this.InputString = new XmlDocument();
            loaded = false;
            sigset.sigAlg = "1";
            
        }

        public bool getshowSteps()
        {
            return sigset.showsteps;
        }


        [PropertyInfo(Direction.ControlSlave, "WSDL Input", "WSDL to create the soap message",null, DisplayLevel.Beginner)]
        public XmlDocument wsdl
        {
            set
            {
                 presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                string s = xmlToString(value);
                loadWSDL(s);
                wsdlLoaded = true;
                       
                  OnPropertyChanged("wsdl");
                  createInfoMessage("Received WSDL File");
                  createInfoMessage("Created SOAP Message");
                    }, null);
            }
            get
            {
                return null;
            }
        }
        private ControlProxy control;
[PropertyInfo(Direction.ControlSlave, "WSDL Input", "WSDL to create the soap message",null, DisplayLevel.Beginner)]
        public IControlWsdl Control
{
 get
 { 
 if (control == null)
    control = new ControlProxy (this);
  return control;
 }
}


        [PropertyInfo(Direction.ControlSlave, "Public-Key input", "Encryption Key",null, DisplayLevel.Beginner)]
        public string publicKey
        {
            set
            {
                this.wsPublicKey = value;

                wsRSACryptoProv.FromXmlString(wsPublicKey);
                gotKey = true;
                mySettings.gotkey = true;
                mySettings.wsRSAcryptoProv = wsRSACryptoProv.ToXmlString(false);
                OnPropertyChanged("publicKey");
                createInfoMessage("Public Key Received");
            }
            get
            {
                return this.wsPublicKey;
            }
        }

       [PropertyInfo(Direction.OutputData, "SOAP output", "Send a SOAP Message", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, "XmlConverter")]
        public XmlDocument OutputString                                                                                                 
        {
            get {  return this.securedSOAP;  }
            set
            {

                this.securedSOAP = value;
                OnPropertyChanged("OutputString");
                send = true;

            }
        }
    
       [PropertyInfo(Direction.InputData, "SOAP input", "Input a SOAP message to be processed by the Web Service", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, "XmlOutputConverter")]
        public XmlDocument InputString       
        {
            get { return this.inputDocument; }
            set
            {
            this.inputDocument = value;

             OnPropertyChanged("InputString");
            }
        }

        public Object XmlOutputConverter(Object Data)
        {
            string test = Data.ToString();

            if (test.StartsWith("<"))
            {
                string test1 = Data.GetType().ToString();
                test1 = test1 + " " + test;
                XmlDocument doc = (XmlDocument)Data;
                StringWriter t = new StringWriter();
                Object obj = new Object();
                try
                {
                    XmlTextWriter j = new XmlTextWriter(t);
                    j.Formatting = Formatting.Indented;
                    doc.WriteContentTo(j);
                    obj = (Object)t.ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());

                }
                return obj;
            }
            return null;
         
        }

        public Object XmlConverter(Object Data)
        {
            string test = Data.ToString();
            if (test.StartsWith("<"))
            {
                XmlDocument doc = (XmlDocument)this.securedSOAP;
                StringWriter t = new StringWriter();
                Object obj = new Object();
                try
                {
                    XmlTextWriter j = new XmlTextWriter(t);
                    j.Formatting = Formatting.Indented;
                    doc.WriteContentTo(j);
                    obj = (Object)t.ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());

                }
                return obj;
            }
            return null;
        }

        public void setSignedElements(DataSet ds)
        {
            DataTable table = ds.Tables[0];
            signedElements = new string[table.Columns.Count];
        }

        public void clearSoap()
        {
            this.soap.RemoveAll();
            this.soap = new XmlDocument();
      
            this.node = null;   
        }

        public void addIdToElement(string element)
        {
            if(!idTable.ContainsKey(element))
            {
            System.Random r = new Random();
            int zufallszahl = r.Next(100000000);
                if(!idTable.ContainsValue(zufallszahl))
                {
                    System.Threading.Thread.Sleep(500);
                    zufallszahl = r.Next(100000000);
                }
                idTable.Add(element, zufallszahl);
                mySettings.idtable = idTable;
            }
        }

        private XmlNode getElementById(string id)
        {

            XmlNodeList securityHeader = securedSOAP.GetElementsByTagName("wsse:Security");
            foreach(XmlNode node in securityHeader)
            {
                foreach (XmlAttribute att in node.Attributes)
                {
                    if(att.Name.Equals("Id") && ("#"+att.Value).Equals(id))
                    {
                        return node;
                    }
                }
            }

            XmlNode body = securedSOAP.GetElementsByTagName("s:Body")[0];
            foreach (XmlAttribute att in body.Attributes)
            {
                if (att.Name.Equals("Id") && ("#" + att.Value).Equals(id))
                {
                    return body;
                }
            }
 
            foreach (XmlNode node in body.ChildNodes)
            {
                foreach (XmlAttribute att in node.Attributes)
                {
                    if (att.Name.Equals("Id") && ("#" + att.Value).Equals(id))
                    {
                        return node;
                    }
                }
                foreach(XmlNode child in node.ChildNodes)
                {
                    foreach (XmlAttribute att in child.Attributes)
                    {
                        if (att.Name.Equals("Id") && ("#" + att.Value).Equals(id))
                        {
                            return child;
                        }
                    }
                }
            }
            return null;
        }
     
        public XmlNode[] getSignedElements()
        {
            ArrayList list = new ArrayList();
            if (bodySigned)
            {
                list.Add(securedSOAP.GetElementsByTagName("s:Body")[0]);
            }
            if (methodNameSigned)
            {
                list.Add(securedSOAP.GetElementsByTagName("s:Body")[0].FirstChild);
            }
            foreach(XmlNode node in securedSOAP.GetElementsByTagName("s:Body")[0].FirstChild.ChildNodes)
            {
                if(isSigned(node))
                {
                    list.Add(node);
                }
            }
            if(secHeaderSigned)
            {
                if (isSigned(securedSOAP.GetElementsByTagName("wsse:Security")[0]));
             {
                list.Add(securedSOAP.GetElementsByTagName("wsse:Security")[0]);
            }
            }


            XmlNode[] retArray = new XmlNode[list.Count];

            for (int i = 0; i < list.Count; i++)
            {
                retArray[i] = (XmlNode)list[i];
            }
            return retArray;
        }

        public XmlNode[] getElementsToSign()
        {
            if (secHeaderEnc && secHeaderSigned)
            {
                XmlNode[] retArray = new XmlNode[0];
                return retArray;
            }
            if (secHeaderEnc)
            {
                XmlNode[] retArray = new XmlNode[1];
                retArray[0] = securedSOAP.GetElementsByTagName("wsse:Security")[0];
                return retArray;
            }

            
            ArrayList list = new ArrayList();
            if (!secHeaderSigned && (securedSOAP.GetElementsByTagName("wsse:Security").Count>0))
            {
                list.Add(securedSOAP.GetElementsByTagName("wsse:Security")[0]);
            }
            XmlNode Body = securedSOAP.GetElementsByTagName("s:Body")[0];
            XmlNode BodysChild = Body.ChildNodes[0];
            if (!bodySigned)
            {
                list.Add(Body);
                if (!bodyEncrypted)
                
                    if (!methodNameSigned) 
                    {
                        list.Add(BodysChild);
                        if(!methodNameEncrypted)
                        {
                            foreach(XmlNode childNode in BodysChild.ChildNodes)
                            {
                                bool Signed = false;
                                XmlNode[] signedElement = this.getSignedElements();
                                foreach(XmlNode sigElem in signedElement)
                                {
                                    if (childNode.Name.Equals(sigElem.Name))
                                    {
                                        Signed = true;
                                    }
                                }
                                if(!Signed)
                                {
                                    list.Add(childNode);
                                }
                            }
                        }
                    }
                }

            
            XmlNode[] retArray1 = new XmlNode[list.Count];

            for (int i = 0; i < list.Count; i++)
            {
                retArray1[i] = (XmlNode)list[i];
            }
            return retArray1;
        }

        public bool hasEncryptedContent(XmlNode node)
        {
            bool value = false;
            if (node.HasChildNodes)
            {
                if (node.ChildNodes[0].Name.Equals("xenc:EncryptedData"))
                {
                    foreach (XmlAttribute att in node.ChildNodes[0].Attributes)
                    {
                        if (att.Value.Equals(EncryptedXml.XmlEncElementContentUrl))
                        {
                            value = true;
                        }
                    }
                }
            }
            return value;
        }

        public XmlNode[] getEncryptedElements()
        {
            ArrayList list = new ArrayList();


            XmlNode header = securedSOAP.GetElementsByTagName("s:Header")[0];
            if (header != null)
            {
                foreach (XmlNode node in securedSOAP.GetElementsByTagName("s:Header")[0].ChildNodes)
                {
                    if (node.Name.Equals("wsse:Security"))
                    {
                        if (hasEncryptedContent(node))
                        {
                            list.Add(node);
                        }
                    }
                }
            }

            XmlElement body = (XmlElement)securedSOAP.GetElementsByTagName("s:Body")[0];


            if (hasEncryptedContent(body))
            {
                list.Add(body);
            }
            else
            {
                foreach (XmlNode node in body.ChildNodes)
                {
                    if (node.Name.Equals("xenc:EncryptedData"))
                    {
                        list.Add(node);
                    }
                    else
                    {
                        if(hasEncryptedContent(node))
                        {list.Add(node);
                        }
                        foreach (XmlNode nod in node.ChildNodes)
                        {
                            if (nod.Name.Equals("xenc:EncryptedData"))
                            {
                                list.Add(nod);
                            }
                            else
                            {
                                if (hasEncryptedContent(nod))
                                {
                                    list.Add(nod);
                                }
                            }
                        }
                    }
                }
            }


            XmlNode[] retArray = new XmlNode[list.Count];

            for (int i = 0; i < list.Count; i++)
            {
                retArray[i] = (XmlNode)list[i];
            }
            return retArray;
        }

        public XmlNode[] getElementsToEnc()
        {
            if (secHeaderEnc && secHeaderSigned)
            {
                XmlNode[] retArray = new XmlNode[0];
                return retArray;
            }
            if (secHeaderSigned)
            {
                XmlNode[] retArray = new XmlNode[1];
                retArray[0] = securedSOAP.GetElementsByTagName("wsse:Security")[0];
                return retArray;
            }
            else
            {

                ArrayList list = new ArrayList();
                XmlNode header = securedSOAP.GetElementsByTagName("s:Header")[0];
                if (header != null)
                {
                    foreach (XmlNode node in securedSOAP.GetElementsByTagName("s:Header")[0].ChildNodes)
                    {
                        if (node.Name.Equals("wsse:Security") && (!hasEncryptedContent(node)))
                        {
                            list.Add(node);
                        }
                    }
                }
                XmlElement body = (XmlElement)securedSOAP.GetElementsByTagName("s:Body")[0];


                if (!hasEncryptedContent(body))
                {
                    list.Add(body);
                    if (!bodySigned)
                    {
                        foreach (XmlNode node in body.ChildNodes)
                        {
                            if (!hasEncryptedContent(node) && (!node.Name.Equals("xenc:EncryptedData")))
                            {
                                list.Add(node);
                                if (!methodNameSigned)
                                {
                                    foreach (XmlNode nod in node.ChildNodes)
                                    {
                                        if (!hasEncryptedContent(nod) && (!nod.Name.Equals("xenc:EncryptedData")))
                                        {
                                            list.Add(nod);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                XmlNode[] retArray = new XmlNode[list.Count];

                for (int i = 0; i < list.Count; i++)
                {
                    retArray[i] = (XmlNode)list[i];
                }
                return retArray;
            }
           
        }

        private bool isSigned(XmlNode node)
        {
            bool signed = false;
            foreach(XmlAttribute att in node.Attributes )
            {
                if (att.Name.Equals("Id"))
                {
                    foreach (XmlNode refElem in securedSOAP.GetElementsByTagName("ds:Reference"))
                    {
                        foreach(XmlAttribute refAtt in refElem.Attributes)
                        {
                            if(refAtt.Name.Equals("URI"))
                            {
                                if (refAtt.Value.Equals("#"+att.Value))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            foreach (XmlNode xPath in securedSOAP.GetElementsByTagName("ds:XPath"))
            {
                string[] splitter = xPath.InnerText.Split(new char[]{'/'});
                if(splitter[splitter.Length-1].Equals(node.Name))
                {
                   return true;
                }
            }
            return signed;
        }

        public XmlNode[] getParameterToEdit()
        {
            ArrayList list = new ArrayList();
            if(bodyEncrypted ||bodySigned || methodNameEncrypted || methodNameSigned)
            {
                XmlNode[] emptySet = new XmlNode[0];
                return emptySet;
            }
            if (secHeaderEnc || secHeaderSigned)
            {
                XmlNode[] retArray = new XmlNode[0];
                return retArray;
            }

            if (!secHeaderEnc)
            {
                foreach (XmlNode param in securedSOAP.GetElementsByTagName("s:Body")[0].FirstChild.ChildNodes)
                {
                    if (!isSigned(param))
                    {
                        if (!hasEncryptedContent(param))
                        {
                            if (!param.Name.Equals("xenc:EncryptedData"))
                                list.Add(param);
                        }
                    }
                }
            }
            XmlNode[] nodeset = new XmlNode[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                nodeset[i] = (XmlNode)list[i];
            }
            return nodeset;
        }

        public XmlNode[] getParameter()
        {
            ArrayList list = new ArrayList();
            foreach (XmlNode node in securedSOAP.GetElementsByTagName("s:Body")[0].ChildNodes[0].ChildNodes)
            {
                XmlNode[] signedNodes = getSignedElements();
                bool isSigned = false;
                foreach (XmlNode signedElement in signedNodes)
                {
                    if (signedElement.Name.Equals(node.Name))
                    {
                        isSigned = true;
                    }

                }
                XmlNode[] encryptedNodes = getEncryptedElements();
                bool isEncrypted=false;
                foreach (XmlNode encryptedNode in encryptedNodes)
                {
                    if(encryptedNode.Equals(node))
                    {
                        isEncrypted = true;
                    }
                }
                if (!isSigned && !isEncrypted)
                {
                    list.Add(node);
                }
            }
            XmlNode[] nodeset = new XmlNode[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                nodeset[i] = (XmlNode)list[i];
            }
            return nodeset;
        }

        public string getSignatureAlg()
        {
            return sigset.sigAlg;
        }

        void settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SoapSettings s = (SoapSettings)sender;

            switch (e.PropertyName)
            {
                case "SignatureAlg":
                    sigset.sigAlg = s.SignatureAlg;
                    break;

                case "SigXPathRef":
                    sigset.Xpath = s.SigXPathRef;
                    break;

                case "SigShowSteps":
                    sigset.showsteps = s.SigShowSteps;
                    break;
                case "EncContentRadio":
                    if (s.EncContentRadio == 0)
                    {
                        encset.content = false;
                    }
                    if (s.EncContentRadio == 1)
                    {
                        encset.content = true;
                    }
                    break;

                case "EncShowSteps":
                    encset.showsteps = s.EncShowSteps;
                    break;
                case "gotkey":
                    this.gotKey = s.gotkey;
                    break;
                case "wspublicKey":
                   this.wsPublicKey =s.wspublicKey;
                    break;
                case "dsacryptoProv":
                    this.dsaCryptoProv.FromXmlString( s.dsacryptoProv);
                    break;
                case "rsacryptoProv":
                    this.rsaCryptoProv.FromXmlString( s.rsacryptoProv);
                    break;
                case "wsRSAcryptoProv":
                    this.wsRSACryptoProv.FromXmlString(s.wsRSAcryptoProv);
                    break;
                case "contentcounter":
                    this.contentCounter = s.contentcounter;
                    break;
                case "secheaderSigned":
                    this.secHeaderSigned = s.secheaderSigned;
                    break;
                case "secheaderEnc":
                    this.secHeaderEnc = s.secheaderEnc;
                    break;
                case "methodnameencrypted":
                    this.methodNameEncrypted = s.methodnameencrypted;
                    break;
                case "bodyencrypted":
                    this.bodyEncrypted = s.bodyencrypted;
                    break;
                case "methodnameSigned":
                    this.methodNameSigned = s.methodnameSigned;
                    break;
                case "bodysigned":
                    this.bodySigned= s.bodysigned;
                    break;
                case "idtable": 
                    this.idTable = s.idtable;
                    break;
                case "securedsoap":
                    if (s.securedsoap != null)
                    {
                        if (!loaded)
                        {
                            securedSOAP = (stringToXml(s.securedsoap));
                            showsecuredSoap();
                            loaded = true;
                        }
                        else
                        {
                            loaded = true;
                        }
                    }
                    break;
                case "soapelement":
                    if (s.soapelement != null)
                    {
                        this.soap = stringToXml(s.soapelement);
                    }
                   break;
                case "wsdlloaded":
                    this.wsdlLoaded = s.wsdlloaded ;
                    break;
                case "resetSoap":
                    if (this.soap != null)
                    {
                        securedSOAP = (XmlDocument)this.soap.Clone();
                        mySettings.securedsoap = xmlToString(securedSOAP);
                        showsecuredSoap();
                    }
                    break;
                case "AnimationSpeed":
                    presentation.setAnimationSpeed(s.AnimationSpeed);
                    break;
                case "playPause":
                    presentation.startstopanimation();
                    break;
                case "endAnimation":
                    presentation.endAnimation();
                    break;

            }
        }

        public void saveSoap()
        {
            mySettings.securedsoap = xmlToString(this.securedSOAP);
        }

        public void loadWSDL(string wsdlString)
        {
            if (!wsdlString.Equals(""))
            {
                StringReader sr = new StringReader(wsdlString);
                XmlTextReader tx = new XmlTextReader(sr);

                ServiceDescription t = ServiceDescription.Read(tx);
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
                XmlSchema fsdf = types.Schemas[0];
                if (fsdf == null)
                {
                    Console.WriteLine("Test");
                }
                StringWriter twriter = new StringWriter();
                fsdf.Write(twriter);
                DataSet set = new DataSet();
                StringReader sreader = new StringReader(twriter.ToString());
                XmlTextReader xmlreader = new XmlTextReader(sreader);
                set.ReadXmlSchema(xmlreader);
                this.setSignedElements(set);
                soap = new XmlDocument();
                node = soap.CreateXmlDeclaration("1.0", "ISO-8859-1", "yes");
                soap.AppendChild(node);
                envelope = soap.CreateElement("s", "Envelope", "http://www.w3.org/2003/05/soap-envelope");
                soap.AppendChild(envelope);
                body = soap.CreateElement("s", "Body", "http://www.w3.org/2003/05/soap-envelope");
                XmlNode eingabe = soap.CreateElement("tns", set.Tables[0].ToString(), set.Tables[0].Namespace);
                DataTable table = set.Tables[0];
                foreach (DataColumn tempColumn in table.Columns)
                {
                    XmlNode neu = soap.CreateElement("tns", tempColumn.ColumnName, set.Tables[0].Namespace);
                    eingabe.AppendChild(neu);
                }
                body.AppendChild(eingabe);
                envelope.AppendChild(body);
                StringWriter ti = new StringWriter();
                XmlTextWriter j = new XmlTextWriter(ti);
                j.Formatting = Formatting.Indented;
                soap.WriteContentTo(j);
                XmlNode rootElement = soap.SelectSingleNode("/*");
                presentation.origSoapItem = new System.Windows.Controls.TreeViewItem();
                presentation.origSoapItem.IsExpanded = true;
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
                presentation.origSoapItem.Header = panel1;
                loaded = false;
                securedSOAP = (XmlDocument)soap.Clone();
                mySettings.soapelement = xmlToString(soap);
                mySettings.securedsoap = xmlToString(securedSOAP);
                this.presentation.CopyXmlToTreeView(rootElement, ref presentation.origSoapItem);
                this.presentation.treeView.Items.Add(presentation.origSoapItem);
                presentation.treeView.Items.Refresh();
                showsecuredSoap();
                loaded = true;
                this.InputString = this.soap;
                wsdlLoaded = true;
                mySettings.wsdlloaded = true;
                OnPropertyChanged("OutputString");
            }
        }

        public bool getShowEncSteps()
        {
            return encset.showsteps;
        }

        public bool getEncContent()
        {
            return encset.content;
        }

        public string xmlToString(XmlDocument doc)
        {
            if (doc != null)
            {
                StringWriter sw = new StringWriter();
                doc.Normalize();
                XmlTextWriter tx = new XmlTextWriter(sw);
                tx.Formatting = Formatting.Indented;
                doc.WriteContentTo(tx);
                return sw.ToString();
            }
            else
            {
                return "";
            }
        }

        public XmlDocument stringToXml(string s)
        {
            XmlDocument doc = new XmlDocument();
            if (!s.Equals(""))
            {
                StringReader sr = new StringReader(s);
                XmlTextReader tx = new XmlTextReader(sr);
                doc.Load(tx);
            }
            return doc;
        }

        public void addSignedElement(string newElement)
        {
            bool isSigned = false;
            foreach (string s in signedElements)
            {
                if (s != null)
                {
                    if (s.Equals(newElement))
                    {
                        isSigned = true;
                    }
                }
            }
            if (!isSigned)
            {
                int count = -1;
                foreach (string s in signedElements)
                {
                    count++;
                    if(s==null)
                    {
                        break;
                    }
                
                }
                signedElements[count] = newElement;
            }
        }

        public bool isSigned(string Element)
        {
            bool issigned = false;
            foreach(string s in signedElements)
            {
                if (s != null)
                {
                    if (s.Equals(Element))
                    {
                        issigned = true;
                    }
                }
            }
            return issigned;
        }

        public void removeSignature(string Id)
        {
            XmlNodeList SignatureElements = securedSOAP.GetElementsByTagName("ds:Signature");
            ArrayList list = new ArrayList();
            XmlNode toDelete=null;
            foreach(XmlNode node in SignatureElements)
            {
                
                foreach(XmlNode child in node.FirstChild.ChildNodes)
                {
                if (child.Name.Equals("ds:Reference"))
                {
                    foreach(XmlAttribute att in child.Attributes)
                    {
                        if (att.Name.Equals("URI"))
                        {
                            if (att.Value.Equals("#" + Id))
                            {
                                toDelete = node;
                            }
                        }
                    }
                }
            }
            }
            if (toDelete != null)
            {
                foreach (XmlNode node in toDelete.ChildNodes)
                {
                    if(node.Name.Equals("ds:Reference"))
                    {
                        foreach (XmlAttribute att in node.Attributes)
                        {
                            if (att.Name.Equals("URI"))
                            {
                                if (!att.Value.Equals("#" + Id))
                                {
                                    string[] id = att.Value.Split(new char[]{'#'});
                                    XmlNode elem = getElementById(id[0]);
                                    list.Add(elem);
                                }
                            }
                        }
                    }
                }
            }
            XmlElement[] signArray = new XmlElement[list.Count];

            for (int i = 0; i < list.Count; i++)
            {
                signArray[i] = (XmlElement)list[i];
            }

            if (toDelete != null)
            {
                securedSOAP.GetElementsByTagName("wsse:Security")[0].RemoveChild(toDelete);
            }

            if (signArray.Length > 0)
            {
                signElements(signArray);
            }
            showsecuredSoap();
        }

        public void encElements(XmlElement[] elements)
        {
            if (gotKey)
            {
                bool content = encset.content;
                XmlNode secHeader = this.securedSOAP.GetElementsByTagName("wsse:Security")[0];
                if (secHeader == null)
                {
                    hadHeader = false;
                    XmlNode head = this.securedSOAP.CreateElement("s", "Header", "http://www.w3.org/2001/12/soap-envelope");

                    string wssenamespace = "http://docs.oasis-open.org/wss/2004/01/oasis -200401-wss-wssecurity-secext-1.0.xsd";

                    secHeader = this.securedSOAP.CreateElement("wsse", "Security", wssenamespace);

                    head.AppendChild(secHeader);
                    XmlNode env = this.securedSOAP.GetElementsByTagName("s:Envelope")[0];
                    XmlNode soapbody = this.securedSOAP.GetElementsByTagName("s:Body")[0];
                    env.InsertBefore(head, soapbody);
                }
                else
                {
                    hadHeader = true;
                }
                RijndaelManaged sessionKey = new RijndaelManaged();
                sessionKey.KeySize = 256;


                EncryptedXml encXML = new EncryptedXml();

                EncryptedKey ek = new EncryptedKey();

                byte[] encryptedKey = EncryptedXml.EncryptKey(sessionKey.Key, wsRSACryptoProv, false);
                ek.EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncRSA15Url);
                ek.CipherData = new CipherData(encryptedKey);
                //ek.KeyInfo = new KeyInfo();

                KeyInfoName name = new KeyInfoName();
                name.Value = "Web Service Public Key";
                ek.KeyInfo.AddClause(name);

                foreach (XmlElement elem in elements)
                {
                    if (elem != null)
                    {
                        //Check if Security Header or Body. Only content encryption is allowed by WS-Security
                        if (elem.Name.Equals("s:Body") || elem.Name.Equals("wsse:Security"))
                        {
                            if (content == false)
                            {
                                createErrorMessage("Only the content of the  "+elem.Name+" element can be encrypted");
                            }
                            content = true;
                        }
                        lastSessionKey = Convert.ToBase64String(sessionKey.Key);
                        byte[] encryptedElement = encXML.EncryptData(elem, sessionKey, content);
                        EncryptedData encElement = new EncryptedData();
                        DataReference ekRef = new DataReference();
                        if (!content)
                        {
                            encElement.Type = EncryptedXml.XmlEncElementUrl;
                            encElement.Id = idTable[elem.Name].ToString();
                            ekRef.Uri = "#" + idTable[elem.Name].ToString();
                        }
                        else
                        {
                            encElement.Type = EncryptedXml.XmlEncElementContentUrl;
                            addIdToElement(contentCounter+elem.Name);
                            encElement.Id = idTable[contentCounter+elem.Name].ToString();
                            ekRef.Uri = "#" + idTable[contentCounter+elem.Name].ToString();
                            contentCounter++;
                            mySettings.contentcounter = contentCounter;
                        }
                        
                        encElement.EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncAES256Url);
                        encElement.CipherData.CipherValue = encryptedElement;
                       
                        
                        ek.AddReference(ekRef);
                        string s = securedSOAP.GetElementsByTagName(elem.Name)[0].ParentNode.Name;

                        if (!content)
                        {
                            securedSOAP.GetElementsByTagName(s)[0].ReplaceChild(securedSOAP.ImportNode(encElement.GetXml(), true), securedSOAP.GetElementsByTagName(elem.Name)[0]);
                        }
                        else
                        {
                            securedSOAP.GetElementsByTagName(elem.Name)[0].RemoveAll();
                            securedSOAP.GetElementsByTagName(elem.Name)[0].AppendChild(securedSOAP.ImportNode(encElement.GetXml(), true));
                        }
                        if (elem.Name.Equals("s:Body"))
                        {
                            bodyEncrypted = true;
                            mySettings.bodyencrypted = true;
                        }
                        if (elem.Name.Equals(soap.GetElementsByTagName("s:Body")[0].ChildNodes[0].Name))
                        {
                            methodNameEncrypted = true;
                            mySettings.methodnameencrypted = methodNameEncrypted;
                        }
                        if (elem.Name.Equals("wsse:Security"))
                        {
                            secHeaderEnc = true;
                            mySettings.secheaderEnc = true;
                        }
                    }

                }
                secHeader.InsertBefore(securedSOAP.ImportNode(ek.GetXml(), true), secHeader.ChildNodes[0]);
                prefixesToEncryptedElement();
       
                mySettings.securedsoap = xmlToString(securedSOAP);
            }
            else
            {
                createErrorMessage("No key for encryption available");
            }
        }

        private void prefixesToEncryptedElement()
        {
            XmlNodeList encKeyElems = securedSOAP.GetElementsByTagName("EncryptedKey");
            foreach (XmlNode child in encKeyElems)
            {
                addPrefixesToNodeAndChildNode("xenc", child);
            }
            XmlNodeList encDataElemns = securedSOAP.GetElementsByTagName("EncryptedData");
            foreach(XmlNode child in encDataElemns)
            {
                addPrefixesToNodeAndChildNode("xenc", child);
            }
        }

        private void addPrefixesToNodeAndChildNode(string prefix ,XmlNode node)
        {
            if(node.Name.Equals("KeyInfo"))
        {
            node.Prefix = "ds";
            prefix = "ds";
        }
            else
        {
            node.Prefix = prefix;
        }
            foreach (XmlNode child in node.ChildNodes)
            {
                addPrefixesToNodeAndChildNode(prefix, child);
            }
        }

        public void decrypt()
        {
            XmlElement securityHeader = (XmlElement)securedSOAP.GetElementsByTagName("Security")[0];
            XmlElement encKeyXml = (XmlElement) securedSOAP.GetElementsByTagName("EncryptedKey")[0];
            XmlElement encData = (XmlElement)securedSOAP.GetElementsByTagName("EncryptedData")[0];
            XmlElement KeyInfo = securedSOAP.CreateElement("KeyInfo",SignedXml.XmlDsigNamespaceUrl);

            securityHeader.RemoveChild(encKeyXml);
            KeyInfo.AppendChild(encKeyXml);

            encData.InsertAfter(KeyInfo, encData.GetElementsByTagName("EncryptionMethod")[0]);
            this.showsecuredSoap();
            EncryptedXml encXml = new EncryptedXml(this.securedSOAP);
            encXml.AddKeyNameMapping("RSA-Key", rsaKey);
            encXml.DecryptDocument();
        }
        
        public void signElements(XmlElement[] elements)
        { 
                String sigAlgo = sigset.sigAlg;
                XmlNode secHeader = securedSOAP.GetElementsByTagName("Security")[0];
                if (secHeader == null)
                {
                    XmlNode head = securedSOAP.CreateElement("s","Header", "http://www.w3.org/2003/05/soap-envelope");

                    string wssenamespace = "http://docs.oasis-open.org/wss/2004/01/oasis -200401-wss-wssecurity-secext-1.0.xsd";

                    secHeader = securedSOAP.CreateElement("Security", wssenamespace);

                    head.AppendChild(secHeader);
                    XmlNode env = securedSOAP.GetElementsByTagName("Envelope")[0];
                    XmlNode soapbody = securedSOAP.GetElementsByTagName("Body")[0];
                    env.InsertBefore(head, soapbody);
                }
                SignedXml signedXML = new SignedXml(this.securedSOAP);
                foreach (XmlElement elem in elements)
                {
                    XmlAttribute idAttribute = securedSOAP.CreateAttribute("Id");
                    idAttribute.Value = idTable[elem.Name].ToString();
                    elem.Attributes.Append(idAttribute);
                    XmlAttributeCollection attributes = elem.Attributes;
                    XmlAttribute id = attributes["Id"];
                    Reference reference = new Reference("#" + id.Value);
                    //   Reference reference = new Reference("");
                    XmlElement xpathElement = securedSOAP.CreateElement("XPath");
                    string Xpath = "ancestor-or-self::Body";
                    XmlElement root = this.securedSOAP.DocumentElement;
                    XmlElement b = (XmlElement)securedSOAP.GetElementsByTagName("Body")[0];
                    XmlNamespaceManager manager = new XmlNamespaceManager(securedSOAP.NameTable);

                    manager.AddNamespace("s", b.NamespaceURI);
                    xpathElement.InnerText = Xpath;
                    XmlDsigXPathTransform xpathTrans = new XmlDsigXPathTransform();
                    XmlNodeList list = root.SelectNodes("/s:Envelope/s:Body", manager);
                    XmlNodeList list2 = root.SelectNodes("//. | //@* | //namespace::*");
                    xpathTrans.LoadInnerXml(xpathElement.SelectNodes("."));
                    XmlDsigExcC14NTransform trans = new XmlDsigExcC14NTransform();
                    reference.AddTransform(trans);
                    XmlElement boo = xpathTrans.GetXml();
                    Type xmlDocumentType = typeof(System.Xml.XmlDocument);

                    signedXML.AddReference(reference);
                    if (elem.Name.Equals("s:Body"))
                    {
                        bodySigned = true;
                        mySettings.bodysigned = true;
                    }
                    if (elem.Name.Equals(soap.GetElementsByTagName("s:Body")[0].ChildNodes[0].Name))
                    {
                        methodNameSigned = true;
                        mySettings.methodnameSigned = true;
                    }

                }
                if (sigAlgo.Equals("1"))
                {
                    CspParameters parameter = new CspParameters();

                    parameter.KeyContainerName = "Container";
                    RSACryptoServiceProvider provider = new RSACryptoServiceProvider(parameter);
                    signedXML.SigningKey = provider;
                    signedXML.ComputeSignature();

                    KeyInfo keyInfo = new KeyInfo();
                    keyInfo.AddClause(new RSAKeyValue(provider));
                    signedXML.KeyInfo = keyInfo;
                    Reference t = (Reference)signedXML.SignedInfo.References[0];
                    IEnumerator enumerator = t.TransformChain.GetEnumerator();
                    enumerator.MoveNext();
                    XmlElement root = (XmlElement)this.securedSOAP.GetElementsByTagName("Envelope")[0];
                    Transform tran = (Transform)enumerator.Current;
                    XmlNodeList list2 = root.SelectNodes("//. | //@* | //namespace::*");

                }
                if (sigAlgo.Equals("0"))
                {
                    DSA dsa = DSA.Create();
                    dsa.ToXmlString(false);
                    signedXML.SigningKey = dsa;
                    signedXML.ComputeSignature();
                }

                XmlElement signaturElement = signedXML.GetXml();
                secHeader.InsertBefore(securedSOAP.ImportNode(signaturElement, true),secHeader.ChildNodes[0]);
            
        } 

        public bool checkSecurityHeader()
        {
            bool securityheader = false;
            XmlNodeList list = securedSOAP.GetElementsByTagName("wsse:Security");
            if (!(list.Count == 0))
            {
                securityheader = true;
            }
            return securityheader;
        }

        public void createSecurityHeaderAndSoapHeader()
        {
            if (!checkSecurityHeader()) 
            {
                XmlElement env= (XmlElement) securedSOAP.GetElementsByTagName("s:Envelope")[0];
                XmlElement Header = securedSOAP.CreateElement("s", "Header", "http://www.w3.org/2001/12/soap-envelope");
                XmlElement secHead = securedSOAP.CreateElement("wsse", "Security", "http://docs.oasis-open.org/wss/2004/01/oasis -200401-wss-wssecurity-secext-1.0.xsd");
                env.InsertBefore(Header,env.FirstChild);
                Header.AppendChild(secHead);
                mySettings.securedsoap = xmlToString(securedSOAP);
            }
        }

        private string getXPathValue(XmlElement elem)
        {
            string xPathValue = "/s:Envelope";
            if (elem.Name.Equals("wsse:Security"))
            {
                xPathValue = xPathValue + "/wsse:Security";
                return xPathValue;
            }
            xPathValue = xPathValue + "/s:Body";
            if (elem.Name.Equals("s:Body"))
            {
                return xPathValue;
            }
            xPathValue = xPathValue + "/" + securedSOAP.GetElementsByTagName("s:Body")[0].FirstChild.Name;
            if (elem.Name.Equals(securedSOAP.GetElementsByTagName("s:Body")[0].FirstChild.Name))
            {
                return xPathValue;
            }
            xPathValue = xPathValue + "/"+elem.Name;
            return xPathValue;
        }

        public void createErrorMessage(string text)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(text, this, NotificationLevel.Error));
        }

        public void createInfoMessage(string text)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(text, this, NotificationLevel.Info));
        }

        public void signElementsManual(XmlElement[] elementsToSign)
        {
            
                string dsNs = "http://www.w3.org/2000/09/xmldsig#";
                XmlElement Signature = securedSOAP.CreateElement("ds", "Signature", dsNs);
                XmlElement SignedInfo = securedSOAP.CreateElement("ds", "SignedInfo", dsNs);
                XmlElement CanonicalizationMethod = securedSOAP.CreateElement("ds", "CanonicalizationMethod", dsNs);
                XmlAttribute canMeth = securedSOAP.CreateAttribute("Algorithm");
                canMeth.Value = SignedXml.XmlDsigExcC14NTransformUrl;
                CanonicalizationMethod.Attributes.Append(canMeth);
                XmlElement SignatureMethod = securedSOAP.CreateElement("ds", "SignatureMethod", dsNs);
                XmlAttribute sigMeth = securedSOAP.CreateAttribute("Algorithm");

                if (sigset.sigAlg.Equals("0"))
                {
                    sigMeth.Value = SignedXml.XmlDsigDSAUrl;
                }
                if (sigset.sigAlg.Equals("1"))
                {
                    sigMeth.Value = SignedXml.XmlDsigRSASHA1Url;
                }

                SignatureMethod.Attributes.Append(sigMeth);
                XmlNode securityHead = securedSOAP.GetElementsByTagName("wsse:Security")[0];
                securityHead.InsertBefore(Signature,securityHead.FirstChild);
                Signature.AppendChild(SignedInfo);
                SignedInfo.AppendChild(CanonicalizationMethod);
                SignedInfo.AppendChild(SignatureMethod);

                foreach (XmlElement tempElem in elementsToSign)
                {
                    addIdToElement(tempElem.Name);
                    XmlAttribute idAttribute = securedSOAP.CreateAttribute("Id");
                    idAttribute.Value = idTable[tempElem.Name].ToString();
                    tempElem.Attributes.Append(idAttribute);
                    XmlElement ReferenceElement = securedSOAP.CreateElement("ds", "Reference", dsNs);
                    XmlAttribute uri = securedSOAP.CreateAttribute("URI");
                    XmlElement Transforms = securedSOAP.CreateElement("ds", "Transforms", dsNs);
                    ReferenceElement.AppendChild(Transforms);

                    if (sigset.Xpath)
                    {
                        uri.Value = "";
                        XmlElement xPathTransform = securedSOAP.CreateElement("ds", "Transform", dsNs);
                        XmlAttribute xPathTransAtt = securedSOAP.CreateAttribute("Algorithm");
                        xPathTransAtt.Value = SignedXml.XmlDsigXPathTransformUrl;
                        xPathTransform.Attributes.Append(xPathTransAtt);
                        XmlElement xPathValue = securedSOAP.CreateElement("ds", "XPath", dsNs);
                        xPathValue.InnerXml = getXPathValue(tempElem);
                        xPathTransform.AppendChild(xPathValue);
                        Transforms.AppendChild(xPathTransform);
                    }
                    else
                    {
                        uri.Value = "#" + idTable[tempElem.Name].ToString();
                    }
                    ReferenceElement.Attributes.Append(uri);

                    XmlElement c14nTransform = securedSOAP.CreateElement("ds", "Transform", dsNs);
                    XmlAttribute c14Url = securedSOAP.CreateAttribute("Algorithm");
                    c14Url.Value = SignedXml.XmlDsigExcC14NTransformUrl;
                    c14nTransform.Attributes.Append(c14Url);
                    Transforms.AppendChild(c14nTransform);
                    XmlElement digestMethod = securedSOAP.CreateElement("ds", "DigestMethod", dsNs);
                    XmlAttribute digMethAtt = securedSOAP.CreateAttribute("Algorithm");
                    digMethAtt.Value = SignedXml.XmlDsigSHA1Url;
                    digestMethod.Attributes.Append(digMethAtt);
                    ReferenceElement.AppendChild(digestMethod);
                    XmlElement digestValue = securedSOAP.CreateElement("ds", "DigestValue", dsNs);
                    digestValue.InnerText = Convert.ToBase64String(getDigestValueForElement(tempElem));
                    ReferenceElement.AppendChild(digestValue);
                    SignedInfo.AppendChild(ReferenceElement);
                    if (tempElem.Name.Equals("s:Body"))
                    {
                        bodySigned = true;
                        mySettings.bodysigned = true;
                    }
                    if (tempElem.Name.Equals(soap.GetElementsByTagName("s:Body")[0].ChildNodes[0].Name))
                    {
                        methodNameSigned = true;
                        mySettings.methodnameSigned = true;
                    }
                    if (tempElem.Name.Equals("wsse:Security"))
                    {
                        secHeaderSigned = true;
                        mySettings.secheaderSigned = true;
                    }

                }
                XmlElement SignatureValue = securedSOAP.CreateElement("ds", "SignatureValue", dsNs);
                KeyInfo keyInfo = new KeyInfo();
                if (sigset.sigAlg.Equals("1"))
                {
                    SignatureValue.InnerXml = Convert.ToBase64String(rsaCryptoProv.SignHash(getDigestValueForElement(SignedInfo), CryptoConfig.MapNameToOID("SHA1")));
                    keyInfo.AddClause(new RSAKeyValue(rsaCryptoProv));
                    
                }
                if (sigset.sigAlg.Equals("0"))
                {
                    SignatureValue.InnerXml = Convert.ToBase64String(dsaCryptoProv.SignHash(getDigestValueForElement(SignedInfo), CryptoConfig.MapNameToOID("SHA1")));
                    keyInfo.AddClause(new DSAKeyValue(dsaCryptoProv));
                }
                Signature.AppendChild(SignatureValue);
                XmlElement xmlKeyInfo = keyInfo.GetXml();
                xmlKeyInfo.Prefix = "ds";
                foreach(XmlNode childNode in xmlKeyInfo.ChildNodes)
                {
                    childNode.Prefix = "ds"; 
                }
                Signature.AppendChild(securedSOAP.ImportNode(xmlKeyInfo, true));
                XmlElement secHead = (XmlElement)securedSOAP.GetElementsByTagName("wsse:Security")[0];
                mySettings.securedsoap = xmlToString(securedSOAP);
        }

        public string getIdToElement(string ElemName)
        {
            string retString = idTable[ElemName].ToString();
            return retString;
        }

        public bool getxPathTrans()
        {
            return sigset.Xpath;
        }

        public byte[] getDigestValueForElement(XmlElement elem)
        {
            Stream canonicalized = canonicalizeNodeWithExcC14n(elem);
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            byte[] byteValue = sha1.ComputeHash(canonicalized);
            return byteValue;
        }

        public Stream canonicalizeNodeWithExcC14n(XmlElement nodeToCanon)
        {
            XmlNode node = (XmlNode)nodeToCanon;
            XmlNodeReader reader = new XmlNodeReader(node);
            Stream stream = new MemoryStream();
            XmlWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
            writer.WriteNode(reader, false);
            writer.Flush();
            stream.Position = 0;
            XmlDsigExcC14NTransform trans = new XmlDsigExcC14NTransform();
            trans.LoadInput(stream);
            Stream stream2 = (Stream)trans.GetOutput();
            return stream2;
        }
    
        public void showsecuredSoap()
        {
            presentation.treeView.Items.Clear();
            presentation.namespacesTable.Clear();
            this.presentation.securedSoapItem = null;

            this.presentation.securedSoapItem = new System.Windows.Controls.TreeViewItem();

            presentation.securedSoapItem.IsExpanded = true;

            StackPanel panel1 = new StackPanel();
        

            panel1.Orientation = System.Windows.Controls.Orientation.Horizontal;

            TextBlock elem1 = new TextBlock();
            elem1.Text = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"; 
            panel1.Children.Insert(0, elem1);
            presentation.securedSoapItem.Header = panel1;
            XmlNode rootElement = securedSOAP.SelectSingleNode("/*");
            this.presentation.CopyXmlToTreeView(rootElement, ref presentation.securedSoapItem);
            this.presentation.treeView.Items.Add(presentation.securedSoapItem);
        }
       
        public XmlDocument soap2
        {get{return this.soap;}
           
       }

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public SoapSettings settings2()
        {
            return (SoapSettings) this.Settings;
        }
              
        #region IPlugin Member

        public void Dispose()
        {
           
        }

        public void Execute()
        {
            if (!send)
            {
                OnPropertyChanged("OutputString");
                send = true;
            }
        }

        public void Initialize()
        {
            
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public void Pause()
        {
            
        }

        public void PostExecution()
        {
            
        }

        public void PreExecution()
        {
            
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return this.presentation; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public ISettings Settings
        {
            get { return (SoapSettings) this.settings; }
        }

        public SoapSettings mySettings
        {
            get { return (SoapSettings)this.settings; }
        }

        public void Stop()
        {
            send = false;
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
      
        #endregion
    }
    public class ControlProxy : IControlWsdl
    {
        private Soap plugin;

        // Konstruktor
        public ControlProxy(Soap plugin)
        {
            this.plugin = plugin;
        }
        #region IControlWsdl Member

        public XmlDocument Wsdl
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        public void setWsdl(XmlDocument wsdlDocument)
        {
           plugin.Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                string s = plugin.xmlToString(wsdlDocument);
                plugin.loadWSDL(s);
                plugin.wsdlLoaded = true;

                plugin.OnPropertyChanged("wsdl");
               plugin.createInfoMessage("Received WSDL File");
                plugin.createInfoMessage("Created SOAP Message");
            }, null);
        }

        #endregion

        #region IControl Member

        public event IControlStatusChangedEventHandler OnStatusChanged;

        #endregion
    }
}
