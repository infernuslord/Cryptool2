using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Security.Cryptography.Xml;
using System.Collections;
using System.IO;

namespace WebService
{
   public class Canonicalizator
   {
       private Transform transform;
       private XmlDocument inputString;
       public Canonicalizator(XmlDocument inputString)
       {
       
           this.inputString = inputString;
       }

       public Stream canonicalizeNode(XmlElement nodeToCanon)
       {

           XmlNode node = (XmlNode)nodeToCanon;
        
      
           XmlNodeReader reader = new XmlNodeReader(node);
           Stream stream = new MemoryStream();

           XmlWriter writer = new XmlTextWriter(stream, Encoding.UTF8);

           writer.WriteNode(reader, false);
           writer.Flush();
          
           stream.Position = 0;

           stream.Position = 0;
           //Transform anwenden
           XmlDsigExcC14NTransform trans = new XmlDsigExcC14NTransform();
           trans.LoadInput(stream);
        
           Stream stream2 = (Stream)trans.GetOutput();
           //StreamReader sreader = new StreamReader(stream2);
           //string canonString = sreader.ReadToEnd();
           //stream2.Position = 0;
           return stream2;
        
       }
      
 
    }
}
