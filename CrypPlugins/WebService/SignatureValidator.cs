using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;
using System.Windows.Threading;
using System.Threading;

namespace WebService
{
   public class SignatureValidator
   {
       private XmlDocument inputString, tempdocument;
       private ArrayList wsSecurityHeaderElements;
       private ArrayList referenceList,encryptedDataList, decryptedDataList, encryptedKeyElements;
       private XmlElement reference;
       private WSSecurityTracer tracer;
       public string canonicalizedSignedInfo;
       private WebService webService;
       private SignedXml signedXml;
       private ArrayList signatureReferenceList;
       private XmlElement transformedElement;
       private Canonicalizator canon;
       private XmlNode securityHeader;
       public bool valid;
       
       private struct SignatureReference
       {
          public int nr;
          public Signature signature;
          public ArrayList references;
       }
      
       public SignatureValidator(WebService webService)
       {
           valid = true;
           this.inputString = (XmlDocument)webService.InputString.Clone();
           this.canon = new Canonicalizator(this.inputString);
           this.tempdocument = (XmlDocument)this.inputString.Clone();
           this.wsSecurityHeaderElements = new ArrayList();
           encryptedDataList = new ArrayList();
           decryptedDataList = new ArrayList();
           encryptedKeyElements = new ArrayList();
           this.referenceList = new ArrayList();
           this.webService = webService;

          signedXml = new SignedXml(inputString);
          signatureReferenceList = new ArrayList();
         securityHeader= this.inputString.GetElementsByTagName("wsse:Security")[0];
         if (securityHeader != null)
         {
             foreach (XmlElement tempElement in securityHeader)
             {
                 if (tempElement.Name.Equals("xenc:EncryptedData"))
                 {
                     this.dercryptElement((XmlElement)wsSecurityHeaderElements[0]);
                     this.fillSecurityHeaderElementsList();
                 }
                 this.wsSecurityHeaderElements.Add(tempElement);
             }
         }

        
           tracer = new WSSecurityTracer();
           
        
          foreach (XmlElement tempElement in wsSecurityHeaderElements)
          {
              if (tempElement.Name.Equals("xenc:EncryptedKey"))
              {
                  try
                  {
                      this.dercryptElement(tempElement);
                  }
                  catch(Exception e)
                  {
                      this.webService.showError(e.Message);
                      valid = false;
                  }
              }
              if (tempElement.Name.Equals("ds:Signature"))
              {
                  
                      
                          this.validateSignature(tempElement);
                     
                  
                 
              }
              
          }
          this.webService.presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
          {
              this.webService.presentation.txtTrace.Text += this.tracer.signatureTrace;
              this.webService.presentation.txtTrace.Text += this.tracer.decryptionTrace;




          }, null);
   this.webService.modifiedInput = this.inputString;
       }

       private void fillSecurityHeaderElementsList()
       {
           this.wsSecurityHeaderElements.Clear();
          securityHeader= this.inputString.GetElementsByTagName("wsse:Security")[0];
          foreach (XmlElement tempElement in securityHeader)
          {
              this.wsSecurityHeaderElements.Add(tempElement);
          }
       }
       #region Decryption
       
       public string dercryptElement(XmlElement encryptedKeyElement)
       {
          // XmlDocument doc = (XmlDocument)this.inputString.Clone();
           EncryptedKey encKey = new EncryptedKey();
          
           encKey.LoadXml(encryptedKeyElement);
           ReferenceList referenceList = encKey.ReferenceList;
           EncryptedReference encryptedReference=referenceList.Item(0);
           string uri=encryptedReference.Uri;
           KeyInfo keyInfo = encKey.KeyInfo;
       
           this.referenceList.Clear();
           ArrayList referenceElementList = this.findEle(uri, this.inputString.ChildNodes[1]);
           XmlElement keyInfoElement = this.inputString.CreateElement("KeyInfo", SignedXml.XmlDsigNamespaceUrl);
           keyInfoElement.AppendChild(encryptedKeyElement);
           XmlElement encryptedDataElement = (XmlElement)referenceElementList[0];
         //  encryptedDataElement.InsertAfter(keyInfoElement, encryptedDataElement.GetElementsByTagName("EncryptionMethod")[0]);
          
           RSACryptoServiceProvider provider = this.webService.provider;
           XmlDocument doc= new XmlDocument();
           XmlElement root= doc.CreateElement("root");
           root.AppendChild(doc.ImportNode((XmlNode)encryptedKeyElement, true));
           root.AppendChild(doc.ImportNode(encryptedDataElement,true));
      
           doc.AppendChild(root);
           EncryptedXml encxml2 = new EncryptedXml(doc);
           EncryptedKey encKey2= new EncryptedKey();
           encKey2.LoadXml((XmlElement)doc.GetElementsByTagName("xenc:EncryptedKey")[0]);
           EncryptedData encData2 = new EncryptedData();
           EncryptedData encDataElement2= new EncryptedData();
           XmlElement data2=(XmlElement) doc.GetElementsByTagName("xenc:EncryptedData")[0];
           encDataElement2.LoadXml((XmlElement)doc.GetElementsByTagName("xenc:EncryptedData")[0]);
           encxml2.AddKeyNameMapping("Web Service Public Key", provider);

           SymmetricAlgorithm algo2 = SymmetricAlgorithm.Create();
           algo2.Key = encxml2.DecryptEncryptedKey(encKey2);
           byte[] t2 = encxml2.DecryptData(encDataElement2, algo2);
           encxml2.ReplaceData(data2, t2);
           doc.GetElementsByTagName("root")[0].RemoveChild(doc.GetElementsByTagName("xenc:EncryptedKey")[0]);
         
           tracer.appendDecryptedData(uri,doc.FirstChild.InnerXml);
           
           EncryptedXml encXml = new EncryptedXml(this.inputString);
           encXml.AddKeyNameMapping("Web Service Public Key", provider);
         //  encXml.DecryptDocument();
           EncryptedData data = new EncryptedData();
           data.LoadXml((XmlElement)encryptedDataElement);
           SymmetricAlgorithm algo = SymmetricAlgorithm.Create();
           algo.Key = encXml.DecryptEncryptedKey(encKey);
         byte[] t=  encXml.DecryptData(data, algo);
         encXml.ReplaceData(encryptedDataElement, t);
         this.encryptedDataList.Add(encryptedDataElement);
         this.decryptedDataList.Add(doc.GetElementsByTagName("root")[0]);
         this.encryptedKeyElements.Add(encryptedKeyElement);
         string decryptedXmlString;
         return decryptedXmlString=Convert.ToBase64String(t);
         //  this.webService.InputString = this.inputString;


       }
       public XmlElement decryptSingleElement(int encryptedKeyNumber)
       {
           XmlElement decryptedXmlElement;
           EncryptedKey encKey = new EncryptedKey(); 
            encKey.LoadXml((XmlElement)encryptedKeyElements[encryptedKeyNumber]);
         
           ReferenceList referenceList = encKey.ReferenceList;
           EncryptedReference encryptedReference = referenceList.Item(0);
           string uri = encryptedReference.Uri;
           KeyInfo keyInfo = encKey.KeyInfo;

           this.referenceList.Clear();
           ArrayList referenceElementList= new ArrayList();
            referenceElementList   = this.findEle(uri, this.tempdocument.ChildNodes[1]);
           XmlElement keyInfoElement = this.tempdocument.CreateElement("KeyInfo", SignedXml.XmlDsigNamespaceUrl);
           keyInfoElement.AppendChild(tempdocument.ImportNode((XmlNode) encKey.GetXml(),true));
           XmlElement encryptedDataElement = (XmlElement)referenceElementList[0];
           //  encryptedDataElement.InsertAfter(keyInfoElement, encryptedDataElement.GetElementsByTagName("EncryptionMethod")[0]);

           RSACryptoServiceProvider provider = this.webService.provider;
        

        

        
           EncryptedXml encXml = new EncryptedXml(this.tempdocument);
           encXml.AddKeyNameMapping("Web Service Public Key", provider);
           //  encXml.DecryptDocument();
           EncryptedData data = new EncryptedData();
           data.LoadXml((XmlElement)encryptedDataElement);
           SymmetricAlgorithm algo = SymmetricAlgorithm.Create();
           algo.Key = encXml.DecryptEncryptedKey(encKey);
           byte[] t = encXml.DecryptData(data, algo);
           encXml.ReplaceData(encryptedDataElement, t);

           this.tempdocument.GetElementsByTagName("wsse:Security")[0].RemoveChild(tempdocument.GetElementsByTagName("xenc:EncryptedKey")[0]);
           string decryptedXmlString;
           XmlElement root = (XmlElement)this.decryptedDataList[encryptedKeyNumber];
           //if(root.FirstChild.NodeType.Equals(XmlNodeType.Text))
           //{
           //    decryptedXmlElement = tempdocument.CreateElement("TextElement");
           //    XmlNode textNode = root.FirstChild.Clone();
           //    decryptedXmlElement.AppendChild(textNode);
           //}

           
             //  (XmlElement)root.FirstChild;
          
           return (XmlElement)root;
           //  this.webService.InputString = this.inputString;
       }

       #endregion

       #region Signatur Check

       public bool validateSignature(XmlElement signatureElement)
       {bool valid= true;
           if (valid)
           {
           }
           
           Signature signature = new Signature();
           signature.LoadXml(signatureElement);
           XmlNodeList signatureList = this.inputString.GetElementsByTagName("ds:Signature");
           if (signatureList.Count != 0)
           {
               
           
           signedXml.LoadXml((XmlElement)signatureElement);
          // XmlNodeList signatureList = this.inputString.GetElementsByTagName("Signature");
         bool validReference=  validateReferences(signedXml);
         if (validReference)
         {
             canonicalizeSignedInfo(signature.SignedInfo.GetXml());
             signedXml.LoadXml((XmlElement)signatureElement);
         }
         else
         {
            this.valid = false;
         }
              
                  
               
              

             
               
           
           }
           return valid;
         
           
       }
       public void canonicalizeSignedInfo(XmlElement SignedInfo)
       {   
           Canonicalizator canon = new Canonicalizator(inputString);
           //StreamReader sreader = new StreamReader(stream2);
           // string test3 = sreader.ReadToEnd();
           //return test3;
       Stream stream=    canon.canonicalizeNode(SignedInfo);
       StreamReader sreader = new StreamReader(stream);
       string canonString = sreader.ReadToEnd();
       this.canonicalizedSignedInfo = canonString;
       this.validateSignature(this.signedXml.Signature, signedXml.SignatureValue);
          
       }
      
       public ArrayList getSignedXmlSignatureReferences()
       {
           return signedXml.SignedInfo.References;
       }
       public bool validateReferences(SignedXml signedXml)
       {
           byte[] digest;
          ArrayList references = signedXml.SignedInfo.References;
          int i = 1;
           foreach(Reference reference in references)
           {

              
               string uri= reference.Uri;
         
               string hashAlgorithm = reference.DigestMethod;
               if (!uri.Equals(""))
               {
                   this.referenceList.Clear();
                   SignatureReference sigReference = new SignatureReference();
                   sigReference.nr = i;
                   i++;
                   sigReference.references = new ArrayList();
                   ArrayList newList = new ArrayList();
                  newList=this.findEle(uri, this.inputString.ChildNodes[0].NextSibling);
                  XmlElement referenceEle= (XmlElement)newList[0];
                  XmlElement clone = (XmlElement)referenceEle.Clone();
                  newList = (ArrayList)this.referenceList.Clone();
                  sigReference.references.Add(clone);
                   this.signatureReferenceList.Add(sigReference);

            
               }
               if (uri.Equals(""))
               {
                   XmlNode node=null;
                   SignatureReference sigReference = new SignatureReference();
                   sigReference.nr = i;
                   i++;
                   ArrayList list= new ArrayList();
                   XmlDocument doc = new XmlDocument();
                   Transform trans = reference.TransformChain[0];
                   XmlDsigXPathTransform xpathTransform = (XmlDsigXPathTransform)trans;
                   XmlElement xpathElement = xpathTransform.GetXml();
                   string xpath = xpathElement.InnerText;
                   XmlNamespaceManager manager = new XmlNamespaceManager(this.inputString.NameTable);
                   XmlElement b = (XmlElement)this.inputString.GetElementsByTagName("s:Body")[0];
                   manager.AddNamespace("s", b.NamespaceURI);
                   manager.AddNamespace("tns", "http://tempuri.org/");
                   node = this.inputString.SelectSingleNode(xpath, manager);
                   list.Add((XmlElement)node.Clone());
                   sigReference.references = list;
                   this.signatureReferenceList.Add(sigReference);
               }
            XmlElement referenceTransformed= this.applyTransform(reference);
             digest=digestElement(referenceTransformed,hashAlgorithm,"");
           string digestValue= Convert.ToBase64String(digest);
          
           this.tracer.appendReferenceValidation(uri, digestValue);
         string convertedDigest=Convert.ToBase64String(reference.DigestValue);
         if (convertedDigest.Equals(digestValue))
         {
             return true;
         }
         else { return false; }
          
               

           }
           return false;
       }
       public XmlElement applyTransform(Reference reference)
       {
           XmlNode node=null;
           TransformChain transformChain = reference.TransformChain;
           int transCounter = transformChain.Count;
           IEnumerator enumerator=transformChain.GetEnumerator();
           Stream transformstream = new MemoryStream();
           if (reference.Uri.Equals(""))
           {

               this.inputString.Save(transformstream);
               transformstream.Position = 0;

           }
           else
           {

               XmlNodeReader reader = new XmlNodeReader((XmlNode)this.reference);

               XmlWriter writer = new XmlTextWriter(transformstream, Encoding.UTF8);
               writer.WriteNode(reader, false);
               writer.Flush();
               transformstream.Position = 0;
           }
           for (int i = 0; i < transCounter; i++)
           {
               XmlUrlResolver test = new XmlUrlResolver();

               SignedXml t = signedXml;
               enumerator.MoveNext();
              
               Transform trans = (Transform) enumerator.Current;
               string typ = trans.ToString();
               XmlElement input;
               
               
             
               switch (typ)
               {
                   case "System.Security.Cryptography.Xml.XmlDsigExcC14NTransform":

                       if (!reference.Uri.Equals(""))
                       {
                           for (int j = 0; j < referenceList.Count; j++)
                           {
                               XmlElement temp = (XmlElement)referenceList[j];
                               string uri = "#" + temp.Attributes["Id"].Value;
                               if (uri.Equals(reference.Uri))
                               {
                                   node = temp;
                               }
                           }
                           
                       }
                    
                       break;
                   case "System.Security.Cryptography.Xml.XmlDsigXPathTransform":
                       XmlDocument doc= new XmlDocument();
                       XmlDsigXPathTransform xpathTransform = (XmlDsigXPathTransform)trans;
                       XmlElement xpathElement=xpathTransform.GetXml();
                       string xpath = xpathElement.InnerText;
                       XmlNamespaceManager manager = new XmlNamespaceManager(this.inputString.NameTable);
                       XmlElement b = (XmlElement)this.inputString.GetElementsByTagName("s:Body")[0];
                       manager.AddNamespace("s", b.NamespaceURI);
                       manager.AddNamespace("tns", "http://tempuri.org/");
                       node = this.inputString.SelectSingleNode(xpath,manager);
                        break;
                    
                      
                      
               }
               
           }
           return (XmlElement) node;

       }


       public bool validateSignature(Signature signature, byte[] bytes)
       {
           bool valid=false;
           KeyInfo keyInfo= signature.KeyInfo;
           CspParameters parameter = new CspParameters();
           RSACryptoServiceProvider rsa;
           DSACryptoServiceProvider dsa;
          XmlElement KeyInfoXml = keyInfo.GetXml();
        Type type=   keyInfo.GetType();
        if (KeyInfoXml.FirstChild.FirstChild.Name.Equals("RSAKeyValue"))
        {
            rsa = new RSACryptoServiceProvider(parameter);
            rsa.FromXmlString(keyInfo.GetXml().InnerXml);
            RSAParameters param = rsa.ExportParameters(false);
            byte[] digestSignedInfo = this.digestElement(signature.SignedInfo.GetXml(), "", "");
            XmlElement signed = signature.SignedInfo.GetXml();
            string oid = CryptoConfig.MapNameToOID("SHA1");
          
            valid = rsa.VerifyHash(digestSignedInfo, oid, this.signedXml.SignatureValue);
          //  this.valid = valid;
        }
        else
        {
            dsa = new DSACryptoServiceProvider(parameter);
            dsa.FromXmlString(KeyInfoXml.InnerXml);
            byte[] digestSignedInfo = this.digestElement(signature.SignedInfo.GetXml(), "", "");
            string oid = CryptoConfig.MapNameToOID("SHA1");
            valid = dsa.VerifyHash(digestSignedInfo, oid, this.signedXml.SignatureValue);
           // this.valid = valid;
        }
         
         
          
           return valid;
       }
       public byte[] digestElement(XmlElement element, string hashAlgorithm, string canonicalizationAlgorithm)
       {
           Canonicalizator canonicalizator = new Canonicalizator(inputString);
           Stream canonicalStream=canonicalizator.canonicalizeNode(element);
           canonicalStream.Position = 0;
           StreamReader sreader = new StreamReader(canonicalStream);
           string canonString = sreader.ReadToEnd();
           SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
           canonicalStream.Position = 0;
           byte[] hash = sha1.ComputeHash(canonicalStream);
           string t = Convert.ToBase64String(hash);
           return hash;
          

           
       }
       public ArrayList findEle(string uri, XmlNode elem)
       
       { XmlElement foundEle = null;
       
           String uri2 = uri;
           string[] t = new string[2];
           char separators = '#';
           char s = uri2[1];
           t = uri2.Split(separators);
          
           string wert = t[1];

           for (int i = 0; i < elem.ChildNodes.Count; i++)
           {
               XmlNode childNote = (XmlNode)elem.ChildNodes[i];

               if (childNote.HasChildNodes)
               {
                   findEle(uri2, childNote);
               }
               if (childNote.Attributes != null)
               {
                   if (childNote.Attributes["Id"] != null)
                   {
                       if (childNote.Attributes["Id"].Value.Equals(wert))
                       {
                           foundEle = (XmlElement)childNote;
                           this.referenceList.Add(foundEle);
                           this.reference = foundEle;
                           break;
                       }
                       if (foundEle != null)
                       {
                           break;
                       }
                   }
               }
           }


           return this.referenceList;


       }

     

       #endregion
       #region GUIINTERFACE
         public string canonicalizeSignedInfo(int signatureNumber)
       {
           XmlElement signedInfo=(XmlElement)this.inputString.GetElementsByTagName("ds:SignedInfo")[signatureNumber];
            canon = new Canonicalizator(inputString);
           //StreamReader sreader = new StreamReader(stream2);
           // string test3 = sreader.ReadToEnd();
           //return test3;
           Stream stream = canon.canonicalizeNode(signedInfo);
           StreamReader sreader = new StreamReader(stream);
           string canonString = sreader.ReadToEnd();
           return canonString;
       }
       public string digestElement(int signatureNumber, int referenceNumber)
         {
             SignedXml signedXml = new SignedXml();
           signedXml.LoadXml((XmlElement)this.inputString.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#")[signatureNumber]);
           Signature signature = signedXml.Signature; 
           Reference reference= (Reference)signature.SignedInfo.References[referenceNumber];
           string uri = reference.Uri;
           ArrayList references=this.getSignatureReferences(signatureNumber);
        byte[] digestedElement=this.digestElement((XmlElement)references[referenceNumber], "", "");
        string convertedDigest = Convert.ToBase64String(digestedElement);
          return Convert.ToBase64String(digestedElement);
        
       }

       public string makeTransforms(int signatureNumber, int referenceNumber, int transformChainNumber)
       {
           SignedXml signedXml = new SignedXml();
           signedXml.LoadXml((XmlElement)this.tempdocument.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#")[signatureNumber]);
           Signature signature = signedXml.Signature;
           Reference reference = (Reference)signature.SignedInfo.References[referenceNumber];
           Transform trans = reference.TransformChain[transformChainNumber];
           string result = "";
           if (trans.ToString().Equals("System.Security.Cryptography.Xml.XmlDsigXPathTransform"))
           {
               XmlNode node = null;
               XmlDocument doc = new XmlDocument();
               XmlDsigXPathTransform xpathTransform = (XmlDsigXPathTransform)trans;
               XmlElement xpathElement = xpathTransform.GetXml();
               string xpath = xpathElement.InnerText;
               XmlNamespaceManager manager = new XmlNamespaceManager(this.inputString.NameTable);
               XmlElement b = (XmlElement)this.inputString.GetElementsByTagName("s:Body")[0];
               manager.AddNamespace("s", b.NamespaceURI);
               manager.AddNamespace("tns", "http://tempuri.org/");
               node = this.tempdocument.SelectSingleNode(xpath, manager);
               StringWriter sw = new StringWriter();
               XmlTextWriter xw = new XmlTextWriter(sw);
               xw.Formatting = Formatting.Indented;
               XmlElement element = (XmlElement)node;
               element.Normalize();
              
               element.WriteTo(xw);
               this.transformedElement = element;
             result= sw.ToString();
              
           }
           if (trans.ToString().Equals("System.Security.Cryptography.Xml.XmlDsigExcC14NTransform"))
           {
               Stream stream;
              if (transformedElement != null)
               {
                stream = this.canon.canonicalizeNode(transformedElement);
                   StreamReader sreader = new StreamReader(stream);
                   string canonString = sreader.ReadToEnd();
                   result = canonString;
                
               }
               else
               {  
                   ArrayList references = this.getSignatureReferences(signatureNumber);
                   XmlElement referenceElement = (XmlElement)references[referenceNumber];
                   stream = this.canon.canonicalizeNode(referenceElement);
                   StreamReader sreader = new StreamReader(stream);
                   string canonString = sreader.ReadToEnd();
                   result = canonString;
               }
           }
        
         
           return result;
       }
    
       public int getReferenceNumber(int signatureNumber)
       {
           return getSignatureReferences(signatureNumber).Count;
       }
       public ArrayList getSignatureReferences(int i)
       {
           SignatureReference signatureReference = (SignatureReference)this.signatureReferenceList[i];
           return signatureReference.references;
       }
       public string getSignatureReferenceName(int i)
       {
           ArrayList referencedElementList = this.getSignatureReferences(i);
           XmlElement referencedElement = (XmlElement)referencedElementList[0];
           string[] splitter = referencedElement.Name.Split(new Char[] { ':' });
           return splitter[1].ToString();
  
       }
       public bool compareDigestValues(int signatureNumber, int referenceNumber, string digestValue)
       {
           SignedXml signedXml = new SignedXml();
           signedXml.LoadXml((XmlElement)this.inputString.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#")[signatureNumber]);
           Signature signature = signedXml.Signature;
           Reference reference = (Reference)signature.SignedInfo.References[referenceNumber];
          return Convert.ToBase64String(reference.DigestValue).Equals(digestValue);
       }

       public int getTransformsCounter(int signatureNumber,int referenceNumber)
       {
           SignedXml signedXml = new SignedXml();
           signedXml.LoadXml((XmlElement)this.inputString.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#")[signatureNumber]);
           Signature signature = signedXml.Signature;
           Reference reference = (Reference)signature.SignedInfo.References[referenceNumber];
           return reference.TransformChain.Count;
       }
       #endregion

       public int getEncryptedKeyNumber()
       {
           return this.encryptedKeyElements.Count;
       }

       public int getTotalSecurityElementsNumber()
       {
           return this.wsSecurityHeaderElements.Count;
       }
       public string getWSSecurityHeaderElement(int i)
       {string returnString="";
           try
           {
               XmlElement tempElement = (XmlElement)this.wsSecurityHeaderElements[i];
               returnString = tempElement.Name;
           }
           catch
           {
               returnString = "null";
           }
           return returnString;
       }

   }   
    
}
