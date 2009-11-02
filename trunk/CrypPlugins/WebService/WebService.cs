using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;
using System.Xml;
using System.Web.Services.Description;
using System.Web.Services.Configuration;
using System.CodeDom.Compiler;


using Microsoft.CSharp;

using System.Security.Cryptography.Xml;

using System.Security.Cryptography;
using System.Data;
using System.IO;
using System.Xml.Schema;
using System.Collections;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Media;

using System.Security.Cryptography.X509Certificates;

using System.Windows.Controls;



//using Microsoft.Web.Services3.Messaging;


namespace WebService
{
    [Author("Tim Podeszwa", "tim.podeszwa@student.uni-siegen.de", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo(false, "WebService", "Represents a Web Service", "", "WebService/webservice.png")]
    public class WebService:IThroughput
    {
        private ISettings settings = new WebServiceSettings();
        public WebServicePresentation presentation;
        private WebServiceQuickWatchPresentation quickWatch;
        public XmlDocument inputString, outputString, modifiedInput;
        public Object service;
        public string[] wsdlMethode;
        public ServiceDescription serviceDescription;
        public XmlDocument wsdlDocument, soapResponse;
        public XmlNode node, envelope, body;
        public string[] stringArray = new string[5];
        public DataSet set;
        public string eingabeparameter = "";
        public string eingabeparameterString = "";
        public string[] rückgabeparameter = new string[5];
        public string methodName="";
        public string webMethod = "";
        private SignedXml signedXml;
        private SignatureValidator validator;
        private XmlSchemaCollection collection;
        private string wsdl,publicRSAkey;

        public RSACryptoServiceProvider provider;
        public WebService()
        {


       
         
         
            
            wsdlDocument = new XmlDocument();
            modifiedInput = new XmlDocument();
          //  XmlSchema soapSchema = new XmlSchema();
          //  XmlSchema signatureSchema = new XmlSchema();
          //  string soapschema = Resource1.SoapSchema;
          //  string signatureschema = Resource1.SignatureSchema;
          //  string wsseSchema = Resource1.WSSESchema;
          //  collection = new XmlSchemaCollection();
          //  StringReader sreader = new StringReader(soapschema);
          //  XmlTextReader reader = new XmlTextReader(sreader);
          //  collection.Add("http://www.w3.org/2003/05/soap-envelope", reader);
          // sreader = new StringReader(signatureschema);
          //reader = new XmlTextReader(sreader);
          //  collection.Add("http://www.w3.org/2000/09/xmldsig#", reader);
          //  sreader = new StringReader(wsseSchema);
          //  reader = new XmlTextReader(sreader);
          //  collection.Add("http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd", reader);


   
            
           // XmlSchema schema = (XmlSchema)obj;
            wsdlMethode = new string[1];
            wsdlMethode[0]="\n" + @"   
            public ServiceDescription getWsdl(){
            ServiceDescription s1;
            ServiceDescriptionReflector serviceDescriptionReflector = new ServiceDescriptionReflector();
            serviceDescriptionReflector.Reflect(typeof(Service), null);
            System.IO.StringWriter stringWriter = new System.IO.StringWriter();
            serviceDescriptionReflector.ServiceDescriptions[0].Write(stringWriter);
            s1 = serviceDescriptionReflector.ServiceDescriptions[0];
            XmlSchema schema = s1.Types.Schemas[0];
            string theWsdl = stringWriter.ToString();
            return s1;
            }}";

            
          
            stringArray[0] = @" using System;
            using System.Web;
            using System.Web.Services;
            using System.Web.Services.Protocols;
            using System.Web.Services.Description;
            using System.Xml;
            using System.Xml.Schema;
            using System.IO;";

            stringArray[1] = @"
            
            public class Service : System.Web.Services.WebService
            {
              public Service()
             {
     
              }";
            stringArray[2] = @"[WebMethod]";
            this.PropertyChanged += new PropertyChangedEventHandler(WebService_PropertyChanged);
            this.presentation = new WebServicePresentation(this);
      //      this.quickWatch = new WebServiceQuickWatchPresentation();
            settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);
            this.WebServiceSettings.Test = 1;
            this.WebServiceSettings.Integer = 1;
            this.WebServiceSettings.MethodName = "methodName";
            OnGuiLogNotificationOccured += new GuiLogNotificationEventHandler(WebService_OnGuiLogNotificationOccured);
            CspParameters parameters = new CspParameters();
            parameters.KeyContainerName = "Container";
            provider = new RSACryptoServiceProvider(parameters);
           
        }

        void WebService_OnGuiLogNotificationOccured(IPlugin sender, GuiLogEventArgs args)
        {
            int n =5;
        }

        void WebService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "InputString")
            {
                this.checkSoap();
                if (this.inputString != null)
                {
                    presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.presentation.resetSoapInputItem();

                        this.presentation.namespacesTable.Clear();
                        this.presentation.CopyXmlToTreeView(this.inputString.ChildNodes[1], ref this.presentation.soapInput,false);
                        
                       
                        this.presentation.treeView4.Items.Add(this.presentation.soapInput);
                        this.presentation.findItem(presentation.soapInput, "Envelope",1).IsExpanded = true;
                        this.presentation.findItem(presentation.soapInput,"Header",1).IsExpanded=true;
                        this.presentation.findItem(presentation.soapInput, "Security",1).IsExpanded = true;
                        this.presentation.findItem(presentation.soapInput, "Signature",1).IsExpanded = true;
                        this.presentation.findItem(presentation.soapInput, "Body",1).IsExpanded = true;
                        this.presentation.treeView4.Items.Refresh();
                       
                    }, null);
                    
                }
                }
        }
        public void createKey()
        {
            CspParameters parameters= new CspParameters();
            parameters.KeyContainerName = "Container";
            provider = new RSACryptoServiceProvider(parameters);
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("Schlüsselpaar erfolgreich erstellt", this, NotificationLevel.Info));

          

        }
        public string exportPublicKey()
        {
            if(provider!=null)
            {
                EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("Öffentlicher Schlüssel exportiert", this, NotificationLevel.Info));
             return   provider.ToXmlString(false);
            }
            return "";
        }
        void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            WebServiceSettings s = (WebServiceSettings)sender;
            if(e.PropertyName.Equals("createKey"))
            {
                this.createKey();
           

            }
            if (e.PropertyName.Equals("publishKey"))
            {
                this.publicKey = this.exportPublicKey();
            }
            if (e.PropertyName.Equals("exportWSDL"))
            {
                this.Wsdl=wsdlDocument;
            }
            if (e.PropertyName.Equals("MethodenStub"))
            {
                
                
                this.WebServiceSettings.Integer = 1;
                this.WebServiceSettings.String = 0;
                this.WebServiceSettings.Test = 4;
                this.WebServiceSettings.Double = 2;
                this.WebServiceSettings.MethodName = "berechneVerzinsung";
             
               
                if (presentation.textBlock1.Inlines != null)
                {
                    presentation.textBlock1.Inlines.Clear();
                }
             
                this.presentation.textBlock1.Inlines.Add(new Bold(new Run(stringArray[2].ToString()+"\n")));
                this.presentation.visualMethodName(WebServiceSettings.MethodName) ;
                this.presentation.visualReturnParam("double");
          
                
                this.presentation.richTextBox1.Document.Blocks.Clear();
                this.presentation.richTextBox1.AppendText("double endKapital;\n"+"int laufzeit=intparam1;\n"+"double zinssatz=doubleparam1;\n"+"double startKapital=doubleparam2;\n"+"endKapital=Math.Round(startKapital*(Math.Pow(1+zinssatz/100,laufzeit)));\n"+"return endKapital;") ;
            
            }

            else
            {
                if (e.PropertyName.Equals("Test"))
                {
                    rückgabeparameter[0] = "void";
                    rückgabeparameter[1] = "int";
                    rückgabeparameter[2] = "string";
                    rückgabeparameter[3] = "float";
                       rückgabeparameter[4] = "double";
                    this.presentation.visualReturnParam(rückgabeparameter[s.Test]);
                }
                  
             
                   
              
                if (e.PropertyName.Equals("Integer"))
                {
                    if (s.Integer == 1)
                    {
                       
                        this.presentation.visualParam("int",1);
                    }
                    if (s.Integer == 2)
                    {
                         this.presentation.visualParam("int",2);
                    }
                    if (s.Integer == 0)
                    {
                        this.presentation.visualParam("int", 0);
                    }

                }
                if (e.PropertyName.Equals("String"))
                {
                    if (s.String == 1)
                    {
                        this.presentation.visualParam("string", 1);
                    }
                    if (s.String == 2)
                    {

                        this.presentation.visualParam("string", 2);
                        }
                    
                    if (s.String == 0)
                    {
                        this.presentation.visualParam("string", 0);
                    }
                }
                if (e.PropertyName.Equals("Double"))
                {
                    if (s.Double == 1)
                    {
                        this.presentation.visualParam("double", 1);
                    }
                    if (s.Double == 2)
                    {

                        this.presentation.visualParam("double", 2);
                    }

                    if (s.Double == 0)
                    {
                        this.presentation.visualParam("double", 0);
                    }
                }


                if (e.PropertyName.Equals("MethodName"))
                {
                    this.methodName = s.MethodName;
                    this.presentation.visualMethodName(s.MethodName);
                }
              
                {
                    string komma = "";
                    if(!eingabeparameter.Equals("")){
                        komma=",";
                    }
                    if(eingabeparameterString.Equals("")){
                        komma="";
                    }
                    stringArray[3] = @"public" + " " + rückgabeparameter[s.Test] + " " + methodName + "(" + "" + eingabeparameter +komma+ eingabeparameterString+")\n{";

                } StringBuilder code = new StringBuilder();

                code.Append(stringArray[0]);
                code.Append(stringArray[1]);
                code.Append(stringArray[2]);
                code.Append(stringArray[3]);
             
     
              
            }   
        }
        public int getSignatureNumber()
        {
            return this.inputString.GetElementsByTagName("ds:Signature").Count;

        }
       
        public bool checkSignature()
        {
            XmlDocument neu = new XmlDocument();
        //   this.inputString.Save(@"C:\Users\Tim\Desktop\test.xml");
           XmlNodeList signatureElements = this.inputString.GetElementsByTagName("Signature");
           XmlElement signaturElement = (XmlElement)signatureElements.Item(0);
           SignedXml signedXml = new SignedXml(this.inputString);
           signedXml.LoadXml(signaturElement);
           bool test = signedXml.CheckSignature();
            return test;
        }
        public void readDescription()
        {
            set = new DataSet();
            XmlSchema paramsSchema = this.serviceDescription.Types.Schemas[0];
         //   this.collection.Add(paramsSchema);
            StringWriter schemaStringWriter = new StringWriter();
            paramsSchema.Write(schemaStringWriter);
            StringReader sreader = new StringReader(schemaStringWriter.ToString());
            XmlTextReader xmlreader = new XmlTextReader(sreader);
            set.ReadXmlSchema(xmlreader);

        }
       // private bool checkSchema()
       // {
       //     XmlSchema soapSchema = collection["http://www.w3.org/2003/05/soap-envelope"];
       //     XmlSchema paramsSchema = collection["http://tempuri.org/"];
       //     XmlSchema signatureSchema = collection["http://www.w3.org/2000/09/xmldsig#"];
       //     XmlSchema wsseSchema = collection["http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"];
       //     string xmlString= this.inputString.InnerXml;
       //     StringReader stringReader= new StringReader(xmlString);
       //     XmlTextReader xmlreader = new XmlTextReader(stringReader);
       //     XmlValidatingReader validatingReader = new XmlValidatingReader(xmlreader);
       //     validatingReader.Schemas.Add(collection);
       //     validatingReader.ValidationEventHandler += new ValidationEventHandler(validatingReader_ValidationEventHandler);
       //     validatingReader.ValidationType = System.Xml.ValidationType.Schema;
       //     XmlDocument inputStringClone = (XmlDocument)this.inputString.Clone();
       //     inputStringClone.Schemas.Add(soapSchema);
       //     inputStringClone.Schemas.Add(signatureSchema);
       //     inputStringClone.Schemas.Add(wsseSchema);
       //     inputStringClone.Schemas.Add(paramsSchema);
       //     try
       //     {
       //         inputStringClone.Load(validatingReader);

       //     inputStringClone.Validate(validatingReader_ValidationEventHandler);
       //     }
       //     catch (Exception exception)
       //     {
       //         validatingReader.Close();
       //         return false;
       //     }
       //     validatingReader.Close();
       //     return true;
       // }

       //void validatingReader_ValidationEventHandler(object sender, ValidationEventArgs e)
       // {
       //     switch (e.Severity)
       //     {
       //         case XmlSeverityType.Error:
       //             Console.WriteLine("Error: {0}", e.Message);
       //             break;
       //         case XmlSeverityType.Warning:
       //             Console.WriteLine("Warning {0}", e.Message);
       //             break;
       //     }
       // }
       private bool compiled()
       {
           if (serviceDescription == null)
           {
               EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("Es liegt keine Service Beschreibung vor. Wurde der Web Service nicht kompiliert?", this, NotificationLevel.Error));
               return false;
           }
           return true;
       }
        public SignatureValidator getValidator()
        {
            return this.validator;
        }
        private void checkSoap()
        {
            bool signatureValid = true;
           // this.checkSchema();
            this.compiled();
            if (this.inputString.GetElementsByTagName("ds:Signature") != null)
            {

                validator = new SignatureValidator(this);
            }
            signatureValid = validator.valid;
          
            object test2 = new object();
            object[] array = null;
            string response;
            if (serviceDescription == null)
            {
                EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("Es liegt keine Service Beschreibung vor. Wurde der Web Service nicht kompiliert?", this, NotificationLevel.Error));
            }
            else
            {
                if (!signatureValid)
                {
                    EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("Signature Validation failed", this, NotificationLevel.Error));
                    goto Abbruch;
                }

                Types types = this.serviceDescription.Types;
                PortTypeCollection portTypes = this.serviceDescription.PortTypes;
                MessageCollection messages = this.serviceDescription.Messages;
                PortType porttype = portTypes[0];
                Operation operation = porttype.Operations[0];
                OperationOutput output = operation.Messages[0].Operation.Messages.Output;
                OperationInput input = operation.Messages[0].Operation.Messages.Input;
                Message messageOutput = messages[output.Message.Name];
                Message messageInput = messages[input.Message.Name];
                MessagePart messageOutputPart = messageOutput.Parts[0];
                MessagePart messageInputPart = messageInput.Parts[0];
                XmlSchema xmlSchema = types.Schemas[0];


                XmlSchemaElement outputSchema = (XmlSchemaElement)xmlSchema.Elements[messageOutputPart.Element];
                XmlSchemaElement inputSchema = (XmlSchemaElement)xmlSchema.Elements[messageInputPart.Element];

                XmlSchemaComplexType complexTypeOutput = (XmlSchemaComplexType)outputSchema.SchemaType;
                XmlSchemaSequence sequenzTypeOutput = (XmlSchemaSequence)complexTypeOutput.Particle;

                XmlSchemaComplexType complexTypeInput = (XmlSchemaComplexType)inputSchema.SchemaType;
                XmlSchemaSequence sequenzTypeInput = (XmlSchemaSequence)complexTypeInput.Particle;

                Hashtable paramTypesTable = new Hashtable();
                StringWriter twriter = new StringWriter();
                //  TextWriter writer= new TextWriter(twriter);
                xmlSchema.Write(twriter);

               
                set = new DataSet();
                StringReader sreader = new StringReader(twriter.ToString());
                XmlTextReader xmlreader = new XmlTextReader(sreader);
                set.ReadXmlSchema(xmlreader);
                if (sequenzTypeInput != null)
                {
                    foreach (XmlSchemaElement inputParam in sequenzTypeInput.Items)
                    {
                        XmlQualifiedName schemaName = inputParam.SchemaTypeName;
                        paramTypesTable.Add(inputParam.QualifiedName.Name, schemaName.Name);
                    }

                    

               
                    
                    XmlDocument t = new XmlDocument();

               
                    XmlNamespaceManager manager = new XmlNamespaceManager(this.modifiedInput.NameTable);
                    XmlElement b = (XmlElement)this.inputString.GetElementsByTagName("s:Body")[0];
                    manager.AddNamespace("s", b.NamespaceURI);
                    manager.AddNamespace("tns", "http://tempuri.org/");
                    XmlNode node = this.modifiedInput.SelectSingleNode("s:Envelope/s:Body/" + "tns:" + set.Tables[0].TableName, manager);
                    XmlElement ele = (XmlElement)node; 
                  //XmlElement ele2 = (XmlElement)this.modifiedInput.GetElementsByTagName(set.Tables[0].TableName, fsdf.TargetNamespace)[0];
                    //   object test = service.GetType().InvokeMember(operation.Name, System.Reflection.BindingFlags.InvokeMethod, null, service,); 
                    int n = new Int32();
                    try
                    {
                       
                   
                        n = ele.ChildNodes.Count;

                    }

                    catch
                    {
                        EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("Es wurden nicht alle Parameter übergeben!", this, NotificationLevel.Error));
                    }
                    if (n != 0)
                    {
                        array = new Object[n];

                        for (int i = 0; i < n; i++)
                        {
                            string param = ele.ChildNodes[i].InnerText;
                            Object paramType = paramTypesTable[ele.ChildNodes[i].LocalName];
                            if (paramType.ToString().Equals("int"))
                            {
                                if (!ele.ChildNodes[i].InnerText.Equals(""))
                                {
                                    try
                                    {
                                        array[i] = Convert.ToInt32((Object)ele.ChildNodes[i].InnerText);
                                    }
                                    catch(Exception e) 
                                    {
                                        EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(e.Message, this, NotificationLevel.Error));
                                        goto Abbruch;
                                    }
                                }
                                else { EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs("Es wurden nicht alle Parameter übergeben!", this, NotificationLevel.Error));
                                goto Abbruch;
                                
                                }
                            }
                            if (paramType.ToString().Equals("string"))
                            {
                                try
                                {
                                    array[i] = Convert.ToString((Object)ele.ChildNodes[i].InnerText);
                                }
                                catch (Exception e)
                                {
                                    EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(e.Message, this, NotificationLevel.Error));
                                    goto Abbruch;
                                }
                            }
                            if (paramType.ToString().Equals("double"))
                            {
                                try
                                {
                                    array[i] = Convert.ToDouble((Object)ele.ChildNodes[i].InnerText);

                                }
                                catch (Exception e)
                                {
                                    EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(e.Message, this, NotificationLevel.Error));
                                    goto Abbruch;
                                }
                            }

                        }

                        //object test2 = service.GetType().getmInvokeMember(operation.Name, System.Reflection.BindingFlags.InvokeMethod, null, null, array);
                          for(int i =0;i<array.Length;i++)
                        {
                            if (array[i] == null)
                            {goto Abbruch;
                             
                            }
                        }
                        try
                        {   
                            Type typ = service.GetType().GetMethod(operation.Name).ReturnType;
                            string returnType = typ.ToString();
                            if (!returnType.Equals("System.Void"))
                            {
                                test2 = service.GetType().GetMethod(operation.Name).Invoke(service, array).ToString();
                            }
                            else { service.GetType().GetMethod(operation.Name).Invoke(service, array).ToString(); }
                        }
                        catch(Exception e)
                        {
                            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(e.Message, this, NotificationLevel.Error));
                            goto Abbruch;
                        }
                        }
                     
                    
                    else
                    {
                        if (sequenzTypeOutput != null)
                        {
                            try
                            {
                                test2 = service.GetType().GetMethod(operation.Name).Invoke(service, null).ToString();
                            }
                            catch(Exception e)
                            {
                                EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(e.Message, this, NotificationLevel.Error));
                                goto Abbruch;
                            }
                        }
                        else { service.GetType().GetMethod(operation.Name).Invoke(service, array); }
                    }
                    response = test2.ToString();
                    this.createResponse(response);

            
                   
                        
                    
                }
            }
        Abbruch: ;
        }
        public void createResponse(string response)
        {
            soapResponse = new XmlDocument();
            node = soapResponse.CreateXmlDeclaration("1.0", "ISO-8859-1", "yes");

            soapResponse.AppendChild(node);
            envelope = soapResponse.CreateElement("Envelope", "http://www.w3.org/2001/12/soap-envelope");


            soapResponse.AppendChild(envelope);

            body = soapResponse.CreateElement("Body", "http://www.w3.org/2001/12/soap-envelope");
            XmlNode eingabe = soapResponse.CreateElement(set.Tables[1].ToString(), set.Tables[1].Namespace);
            DataTable table = set.Tables[1];
            foreach (DataColumn tempColumn in table.Columns)
            {
                XmlNode neu = soapResponse.CreateElement(tempColumn.ColumnName, set.Tables[1].Namespace);
                neu.InnerText = response;
                eingabe.AppendChild(neu);
            }
            body.AppendChild(eingabe);
            envelope.AppendChild(body);
            this.OutputString = soapResponse;
        }
        [PropertyInfo(Direction.InputData, "SOAP input", "Input a SOAP message to be processed by the Web Service", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text,"XmlConverter")]
        public XmlDocument InputString
        {
            get { return this.inputString; }
            set
            {
               
                    this.inputString = value;
                    OnPropertyChanged("InputString");
                
            }

        }
        
        [PropertyInfo(Direction.ControlMaster,"Public-Key output", "Encryption Key",null,DisplayLevel.Beginner)]
      public string publicKey
    {
        get
        {

            return this.publicRSAkey;
        
        }
        set 
        { this.publicRSAkey = value;
        OnPropertyChanged("publicKey");
        }
           
    }

        [PropertyInfo(Direction.ControlMaster, "WSDL output", "Web Service Description", null, DisplayLevel.Beginner)]
        public XmlDocument Wsdl
        {
            get { return this.wsdlDocument; }
            set
            {
                this.wsdlDocument = value;
                OnPropertyChanged("Wsdl");
            }
        }
        [PropertyInfo(Direction.InputData, "SOAP output", "Response from Web Service", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, "XmlOutputConverter")]
        public XmlDocument OutputString
        {
            get { return this.outputString; }
            set
            {
                this.outputString = value;
                OnPropertyChanged("OutputString");
            }


        }
        public Object XmlOutputConverter(Object Data)
        {
            string test = Data.ToString();

            XmlDocument doc = (XmlDocument)this.outputString;
            StringWriter stringWriter = new StringWriter();
            Object obj = new Object();
            try
            {
                XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
                xmlWriter.Formatting = Formatting.Indented;
                doc.WriteContentTo(xmlWriter);
                obj = (Object)stringWriter.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
              
            }


            return obj;
        }
        public Object XmlConverter(Object Data)
        {
          
           
            XmlDocument doc = (XmlDocument)this.inputString;
            StringWriter stringWriter = new StringWriter();
            Object obj = new Object();
            try
            {
                XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
                xmlWriter.Formatting = Formatting.Indented;
                doc.WriteContentTo(xmlWriter);
                obj = (Object)stringWriter.ToString();
            }
            catch (Exception e)
            {Console.WriteLine(e.ToString());
       
            }
 
            return obj ;
        }

        public ServiceDescription description
        {
            get { return this.serviceDescription; }
            set { this.serviceDescription = value; }
        }
        public WebServiceSettings  WebServiceSettings
        {
            get { return (WebServiceSettings) this.settings; }
            set { this.settings = value; }
        }
        public ArrayList getSignatureReferences(int i)
        {
           return this.validator.getSignatureReferences(i);
        }
        public ArrayList getSignedXmlSignatureReferences()
        {
            return this.validator.getSignedXmlSignatureReferences();
        }
        public void compile(string code)
        {
            CSharpCodeProvider codeProvider = new CSharpCodeProvider(new Dictionary<String, String> { { "CompilerVersion", "v3.5" } });
            string header = "";
            presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    header=this.presentation.copyContentToString(this.presentation.textBlock1);
                   
                }, null);
            codeProvider.CreateGenerator();
            CompilerParameters cp = new CompilerParameters();
            cp.ReferencedAssemblies.Add("System.dll"); //includes
            cp.ReferencedAssemblies.Add("System.Web.dll");
            cp.ReferencedAssemblies.Add("System.Web.Services.dll");
            cp.ReferencedAssemblies.Add("System.Xml.dll");
            CompilerResults cr = null;
            cp.GenerateExecutable = false;

            try
            {
                Run methodDeclaration = null;
                string wsdl = wsdlMethode[0];


                cr = codeProvider.CompileAssemblyFromSource(cp, stringArray[0].ToString() + stringArray[1].ToString() + code + wsdl);

                System.Reflection.Assembly a = cr.CompiledAssembly;
                service = a.CreateInstance("Service");




                object obj = service.GetType().InvokeMember("getWsdl", System.Reflection.BindingFlags.InvokeMethod, null, service, null);
                ServiceDescription description = (ServiceDescription)obj;
                System.IO.StringWriter stringWriter = new System.IO.StringWriter();

                XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
                xmlWriter.Formatting = Formatting.Indented;
                description.Write(xmlWriter);

                string theWsdl = stringWriter.ToString();
                presentation.showWsdl(theWsdl);
                this.description = description;
                StringReader stringReader = new StringReader(theWsdl);
                XmlTextReader xmlReader = new XmlTextReader(stringReader);
                wsdlDocument.LoadXml(theWsdl);
                System.Windows.Controls.TreeViewItem xmlDecl = null;
                presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (presentation.treeView1.HasItems)
                    {
                        xmlDecl = (System.Windows.Controls.TreeViewItem)presentation.treeView1.Items[0];
                        if (xmlDecl.HasItems)
                        {
                            xmlDecl.Items.Clear();
                        }
                    }

                }, null);
                presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    presentation.CopyXmlToTreeView(wsdlDocument.ChildNodes[1], ref presentation.item,false);
                    TreeView parent = (TreeView)presentation.item.Parent;

                    if (parent != null)
                    {
                        int pos = parent.Items.IndexOf(presentation.item);
                        parent.Items.RemoveAt(pos);
                    }
                     
                    presentation.treeView1.Items.Add(presentation.item);
                    presentation.item.IsExpanded = true;
                    for (int i = 0; i < presentation.item.Items.Count; i++)
                    {
                        TreeViewItem item =(TreeViewItem) presentation.item.Items[i];
                        item.IsExpanded = true;
                    }
                  
                    presentation.textBox3.Text = "Erstellen erfolgreich";
                    this.readDescription();
                  //  presentation.fillElementTextBox();

                }, null);
               


            }

           
           
           
          
            catch (Exception exception)
            {  
               CompilerErrorCollection errors = cr.Errors;
                int errorCounter= errors.Count;
               if (errors != null)
               {
                   for (int i = 0; i < errorCounter; i++)
                   {
                       this.presentation.textBox3.Text+="Fehlermeldung: "+errors[i].ErrorText+"\n";
                   }
               }
            }
        }



        #region IPlugin Member

        public void Dispose()
        {
           
        }

        public void Execute()
        {
            
            
        }

        public void Initialize()
        {
           // if (presentation.textBox1.Text != null)
                presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {presentation.richTextBox1.AppendText(this.WebServiceSettings.UserCode);
                   
                }, null);
                if (this.WebServiceSettings.Compiled == true)
                {
                    this.presentation.compile();
                }
            
        }

        public void showError(string message)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, NotificationLevel.Error));
        }

        public void showWarning(string message)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, NotificationLevel.Warning));
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
            get { return this.quickWatch; }
        }

        public ISettings Settings
        {
            get { return this.settings; }
        }

        public void Stop()
        {
           
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        
        }
        #endregion
    }
}
