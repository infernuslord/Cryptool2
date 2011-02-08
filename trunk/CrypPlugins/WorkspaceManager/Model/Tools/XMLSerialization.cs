/*                              
   Copyright 2010 Nils Kopal, Viktor M.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Collections;
using System.IO.Compression;
using Cryptool.PluginBase;
using WorkspaceManager;
using System.Windows;
using System.Globalization;

namespace XMLSerialization
{
    /// <summary>
    /// Provides static methods for XML serialization and deserialization
    /// </summary>
    public class XMLSerialization
    {
        /// <summary>
        /// Serializes the given object and all of its members to the given file using UTF-8 encoding
        /// Works only on objects which are marked as "Serializable"
        /// If compress==true then GZip is used for compressing
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="filename"></param>
        /// /// <param name="compress"></param>
        public static void Serialize(object obj, string filename,bool compress = false)
        {
            XMLSerialization.Serialize(obj, filename, Encoding.UTF8,compress);
        }

        /// <summary>
        /// Serializes the given object and all of its members to the given file using
        /// the given encoding
        /// Works only on objects which are marked as "Serializable"
        /// If compress==true then GZip is used for compressing
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="filename"></param>
        /// <param name="compress"></param>
        public static void Serialize(object obj, string filename,Encoding encoding,bool compress = false)
        {

            FileStream sourceFile = File.Create(filename);
            if (compress)
            {
                GZipStream compStream = new GZipStream(sourceFile, CompressionMode.Compress);
                StreamWriter writer = new StreamWriter(compStream);
                try
                {

                    XMLSerialization.Serialize(obj, writer,compress);
                }
                finally
                {
                    if (writer != null)
                    {
                        writer.Close();
                    }
                    if (compStream != null)
                    {
                        compStream.Dispose();
                    }
                    if (sourceFile != null)
                    {
                        sourceFile.Close();
                    }
                }
            }
            else
            {
                StreamWriter writer = new StreamWriter(sourceFile);
                try
                {
                    
                    XMLSerialization.Serialize(obj, writer);
                }
                finally
                {
                    if (writer != null)
                    {
                        writer.Close();
                    }                    
                    if (sourceFile != null)
                    {
                        sourceFile.Close();
                    }
                }
            }
        }
        /// <summary>
        /// Serializes the given object and all of its members to the given writer as xml
        /// Works only on objects which are marked as "Serializable"
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="writer"></param>
        public static void Serialize(object obj, StreamWriter writer,bool compress=false)
        {
            HashSet<object> alreadySerializedObjects = new HashSet<object>();

            writer.WriteLine("<?xml version=\"1.0\" encoding=\"" + writer.Encoding.HeaderName + "\"?>");
            writer.WriteLine("<!--");
            writer.WriteLine("     XML serialized C# Objects");
            writer.WriteLine("     File created: " + System.DateTime.Now);
            writer.WriteLine("     File compressed: " + compress);
            writer.WriteLine("     XMLSerialization created by Nils Kopal");
            writer.WriteLine("     mailto: Nils.Kopal(AT)stud.uni-due.de");
            writer.WriteLine("-->");
            writer.WriteLine("<objects>");
            SerializeIt(obj, writer, alreadySerializedObjects);
            writer.WriteLine("</objects>");
            writer.Flush();
        }

        /// <summary>
        /// Serializes the given object and all of its members to the given writer as xml
        /// Works only on object which are marked as "Serializable"
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="writer"></param>
        private static void SerializeIt(object obj, StreamWriter writer,HashSet<object> alreadySerializedObjects)
        {
            //we only work on complex objects which are serializable and we did not see before
            if (obj == null || 
                isPrimitive(obj) || 
                !obj.GetType().IsSerializable || 
                alreadySerializedObjects.Contains(obj))
            {
                return;
            }

            MemberInfo[] memberInfos = obj.GetType().FindMembers(
                MemberTypes.All, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, new MemberFilter(DelegateToSearchCriteria), "ReferenceEquals");
            
            writer.WriteLine("<object>");
            writer.WriteLine("<type>" + obj.GetType().FullName + "</type>");
            writer.WriteLine("<id>" + obj.GetHashCode() + "</id>");
          
            writer.WriteLine("<members>");

            foreach (MemberInfo memberInfo in memberInfos)
            {
                if (memberInfo.MemberType == MemberTypes.Field && !obj.GetType().GetField(memberInfo.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).IsNotSerialized)
                {
                    string type = obj.GetType().GetField(memberInfo.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).FieldType.FullName;
                    object value = obj.GetType().GetField(memberInfo.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(obj);

                    if (value is System.Collections.IList && value.GetType().IsGenericType)
                    {
                        string gentype = value.GetType().GetGenericArguments()[0].FullName;
                        type = "System.Collections.Generic.List;" + gentype;
                    }

                    writer.WriteLine("<member>");
                    writer.WriteLine("<name>" + ReplaceXMLSymbols(memberInfo.Name) + "</name>");
                    writer.WriteLine("<type>" + ReplaceXMLSymbols(type) + "</type>");

                    if (value is System.Byte[])
                    {
                        byte[] bytes = (byte[])value;
                        writer.WriteLine("<value><![CDATA[" + ReplaceXMLSymbols(Convert.ToBase64String(bytes)) + "]]></value>");
                    }                   
                    else if (value is System.Collections.IList)
                    {
                        writer.WriteLine("<list>");
                        foreach (object o in (System.Collections.IList)value)
                        {
                            if (o.GetType().IsSerializable)
                            {
                                writer.WriteLine("<entry>");
                                writer.WriteLine("<type>" + o.GetType().FullName + "</type>");
                                if (isPrimitive(o))
                                {
                                    if (o is Enum)
                                    {
                                        writer.WriteLine("<value>" + o.GetHashCode() + "</value>");
                                    }
                                    else if(o is Point)
                                    {
                                        Point p = (Point) o;
                                        writer.WriteLine("<value><![CDATA[" + p.X + ";" + p.Y + "]]></value>");

                                    }
                                    else
                                    {
                                        writer.WriteLine("<value><![CDATA[" + o + "]]></value>");
                                    }
                                }
                                else
                                {
                                    writer.WriteLine("<reference>" + o.GetHashCode() + "</reference>");
                                }
                                writer.WriteLine("</entry>");
                            }
                        }
                        writer.WriteLine("</list>");
                    }
                    else if (value == null)
                    {
                        writer.WriteLine("<value></value>");
                    }
                    else if (isPrimitive(value))
                    {
                        if (value is Enum)
                        {
                            writer.WriteLine("<value>" + value.GetHashCode() + "</value>");
                        }
                        else if(value is Point)
                        {
                            Point p = (Point)value;
                            writer.WriteLine("<value><![CDATA[" + p.X + ";" + p.Y + "]]></value>");   
                        }
                        else
                        {
                            writer.WriteLine("<value><![CDATA[" + value.ToString() + "]]></value>");
                        }
                    }
                    else
                    {
                        writer.WriteLine("<reference>" + value.GetHashCode() + "</reference>");
                    }
                    writer.WriteLine("</member>");
                }
            }
            writer.WriteLine("</members>");            
            writer.WriteLine("</object>");
            writer.Flush();
            
            //Save obj so that we will not work on it again
            alreadySerializedObjects.Add(obj);

            foreach (MemberInfo memberInfo in memberInfos)
            {
                if (memberInfo.MemberType == MemberTypes.Field)
                {
                    string type = obj.GetType().GetField(memberInfo.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).FieldType.FullName;
                    object value = obj.GetType().GetField(memberInfo.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(obj);
                    
                    if (value is System.Collections.IList && !(value is System.Byte[]))
                    {
                        foreach (object o in (System.Collections.IList)value)
                        {
                            SerializeIt(o, writer, alreadySerializedObjects);
                        }
                    }
                    else
                    {
                        SerializeIt(value, writer, alreadySerializedObjects);
                    }
                    
                }              
            }
        }

        /// <summary>
        /// Check if the given object ist Primitve
        /// Primitive means isPrimitive returns true
        /// or Fullname does not start with "System"
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private static Boolean isPrimitive(object o)        
        {
            if (o == null)
            {
                return false;
            }
            if (o is Enum)
            {
                return true;
            }

            return (o.GetType().IsPrimitive || o.GetType().FullName.Substring(0, 6).Equals("System"));
        }

        /// <summary>
        /// Returns true if MemberType is Field or Property
        /// </summary>
        /// <param name="objMemberInfo"></param>
        /// <param name="objSearch"></param>
        /// <returns></returns>
        private static bool DelegateToSearchCriteria(MemberInfo objMemberInfo, Object objSearch)
        {
            if (objMemberInfo.MemberType == MemberTypes.Field)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Replaces 
        /// <		with		&lt;
        /// >		with		&gt;
        /// &		with		&amp;
        /// "		with		&quot;
        /// '		with		&apos;
        /// If input string is null it returns "null" string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string ReplaceXMLSymbols(String str)
        {
            if (str == null)
            {
                return "null";
            }

            return str.
                Replace("<", "&lt;").
                Replace(">", "&gt").
                Replace("&", "&amp;").
                Replace("\"", "&quot;").
                Replace("'", "&apos;");
        }

        /// <summary>
        /// Inverse to ReplaceXMLSymbols
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string RevertXMLSymbols(String str)
        {
            if (str == null)
            {
                return "null";
            }

            return str.
                Replace("&lt;","<").
                Replace("&gt", ">").
                Replace("&amp;","&").
                Replace("&quot;","\"").
                Replace("&apos;","'");
        }

        /// <summary>
        /// Deserializes the given XML and returns the root as obj
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="compress"></param>
        /// <param name="workspaceManager"></param>
        /// <returns></returns>
        public static object Deserialize(String filename, bool compress=false,WorkspaceManager.WorkspaceManager workspaceManager = null)
        {
            FileStream sourceFile = File.OpenRead(filename);
            XmlDocument doc = new XmlDocument(); ;
            GZipStream compStream = null;

            if (compress)
            {
                compStream = new GZipStream(sourceFile, CompressionMode.Decompress);
                doc.Load(compStream);
            }
            else
            {
                doc.Load(sourceFile);
            }

            try
            {
                return XMLSerialization.Deserialize(doc,workspaceManager);
            }
            finally
            {
                if (compStream != null)
                {
                    compStream.Close();
                }
            }
        }

        /// <summary>
        /// Deserializes the given XMLDocument and returns the root as obj
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="workspaceManager"></param>
        /// <returns></returns>
        public static object Deserialize(XmlDocument doc, WorkspaceManager.WorkspaceManager workspaceManager = null)
        {
            Dictionary<string, object> createdObjects = new Dictionary<string, object>();
            LinkedList<object[]> links = new LinkedList<object[]>();

            XmlElement objects = doc.DocumentElement;

            foreach (XmlNode objct in objects.ChildNodes)
            {
                XmlNode type = objct.ChildNodes[0];
                XmlNode id = objct.ChildNodes[1];
                XmlNode members = objct.ChildNodes[2];

                object newObject = System.Activator.CreateInstance(Type.GetType(type.InnerText));
                createdObjects.Add(id.InnerText, newObject);

                foreach (XmlNode member in members.ChildNodes)
                {
                    XmlNode membername = member.ChildNodes[0];
                    XmlNode membertype = member.ChildNodes[1];

                    object newmember;

                    try
                    {
                        if (member.ChildNodes[2].Name.Equals("value"))
                        {

                            XmlNode value = member.ChildNodes[2];
                            if (RevertXMLSymbols(membertype.InnerText).Equals("System.String"))
                            {

                                newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public |
                                                             BindingFlags.Instance).SetValue(newObject, value.InnerText);
                            }
                            else if (RevertXMLSymbols(membertype.InnerText).Contains("System.Int"))
                            {
                                Int32 result = 0;
                                System.Int32.TryParse(RevertXMLSymbols(value.InnerText), out result);
                                newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public |
                                                             BindingFlags.Instance).SetValue(newObject, result);
                            }
                            else if (RevertXMLSymbols(membertype.InnerText).Equals("System.Double"))
                            {                                
                                Double result = 0;
                                System.Double.TryParse(RevertXMLSymbols(value.InnerText.Replace(',', '.')),
                                                                        NumberStyles.Number,
                                                                        CultureInfo.CreateSpecificCulture("en-GB"),
                                                                        out result);
                                newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public |
                                                             BindingFlags.Instance).SetValue(newObject, result);
                            }
                            else if (RevertXMLSymbols(membertype.InnerText).Equals("System.Char"))
                            {
                                Char result = ' ';
                                System.Char.TryParse(RevertXMLSymbols(value.InnerText), out result);
                                newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public |
                                                             BindingFlags.Instance).SetValue(newObject, result);
                            }
                            else if (RevertXMLSymbols(membertype.InnerText).Equals("System.Boolean"))
                            {
                                Boolean result = false;
                                System.Boolean.TryParse(RevertXMLSymbols(value.InnerText), out result);
                                newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public |
                                                             BindingFlags.Instance).SetValue(newObject, result);
                            }
                            else if (RevertXMLSymbols(membertype.InnerText).Equals("System.Windows.Point"))
                            {
                                string[] values = value.InnerText.Split(new char[] {';'});

                                if(values.Length != 2)
                                {
                                    throw new Exception("Can not create a Point with " + values.Length + " Coordinates!");
                                }

                                double x = 0;
                                double y = 0;
                                double.TryParse(values[0], out x);
                                double.TryParse(values[1], out y);

                                System.Windows.Point result = new System.Windows.Point(x, y);
                                newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public |
                                                             BindingFlags.Instance).SetValue(newObject, result);
                            }
                            else if (RevertXMLSymbols(membertype.InnerText).Equals("System.Byte[]"))
                            {
                                byte[] bytearray = Convert.FromBase64String(value.InnerText);

                                newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public |
                                                             BindingFlags.Instance).SetValue(newObject, bytearray);
                            }
                            else
                            {
                                newmember =
                                    System.Activator.CreateInstance(Type.GetType(RevertXMLSymbols(membertype.InnerText)));

                                if (newmember is Enum)
                                {
                                    Int32 result = 0;
                                    System.Int32.TryParse(RevertXMLSymbols(value.InnerText), out result);
                                    object newEnumValue =
                                        Enum.ToObject(Type.GetType(RevertXMLSymbols(membertype.InnerText)), result);

                                    newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                                 BindingFlags.NonPublic |
                                                                 BindingFlags.Public |
                                                                 BindingFlags.Instance).SetValue(newObject, newEnumValue);
                                }
                                else
                                {
                                    newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                                 BindingFlags.NonPublic |
                                                                 BindingFlags.Public |
                                                                 BindingFlags.Instance).SetValue(newObject, newmember);
                                }

                            }
                        }
                        else if (member.ChildNodes[2].Name.Equals("reference"))
                        {
                            XmlNode reference = member.ChildNodes[2];
                            links.AddLast(new object[]
                                              {
                                                  newObject,
                                                  RevertXMLSymbols(membername.InnerText),
                                                  RevertXMLSymbols(reference.InnerText),
                                                  false
                                              });
                        }
                        else if (member.ChildNodes[2].Name.Equals("list"))
                        {
                            String[] types = RevertXMLSymbols(membertype.InnerText).Split(';');

                            if (types.Length == 1)
                            {
                                newmember = System.Activator.CreateInstance(Type.GetType(types[0]));
                                newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public |
                                                             BindingFlags.Instance).SetValue(newObject, newmember);
                            }
                            else if (types.Length == 2)
                            {
                                //we have 2 types, that means that we have a generic list with generic type types[1]
                                Type t = typeof (System.Collections.Generic.List<>);
                                Type[] typeArgs = {Type.GetType(types[1])};
                                Type constructed = t.MakeGenericType(typeArgs);
                                newmember = Activator.CreateInstance(constructed);
                                newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.Public |
                                                             BindingFlags.Instance).SetValue(newObject, newmember);
                            }
                            else
                            {
                                throw new Exception("Expected 1 or 2 types for list; But found:" + types.Length);
                            }

                            foreach (XmlNode entry in member.ChildNodes[2].ChildNodes)
                            {
                                if (entry.ChildNodes[1].Name.Equals("reference"))
                                {
                                    XmlNode reference = entry.ChildNodes[1];
                                    links.AddLast(new object[]
                                                      {
                                                          newObject,
                                                          RevertXMLSymbols(membername.InnerText),
                                                          RevertXMLSymbols(reference.InnerText),
                                                          true
                                                      });
                                }
                                else
                                {
                                    XmlNode typ = entry.ChildNodes[1];
                                    XmlNode value = entry.ChildNodes[1];
                                    if (RevertXMLSymbols(typ.InnerText).Equals("System.String"))
                                    {

                                        ((IList) newmember).Add(RevertXMLSymbols(value.InnerText));
                                    }
                                    else if (RevertXMLSymbols(typ.InnerText).Equals("System.Int16"))
                                    {
                                        Int16 result = 0;
                                        System.Int16.TryParse(RevertXMLSymbols(value.InnerText), out result);
                                        ((IList) newmember).Add(result);
                                    }
                                    else if (RevertXMLSymbols(typ.InnerText).Equals("System.Int32"))
                                    {
                                        Int32 result = 0;
                                        System.Int32.TryParse(RevertXMLSymbols(value.InnerText), out result);
                                        ((IList) newmember).Add(result);
                                    }
                                    else if (RevertXMLSymbols(typ.InnerText).Equals("System.Int64"))
                                    {
                                        Int64 result = 0;
                                        System.Int64.TryParse(RevertXMLSymbols(value.InnerText), out result);
                                        ((IList) newmember).Add(result);
                                    }
                                    else if (RevertXMLSymbols(typ.InnerText).Equals("System.Double"))
                                    {
                                        Double result = 0;
                                        System.Double.TryParse(RevertXMLSymbols(value.InnerText.Replace(',', '.')),
                                                                                NumberStyles.Number,
                                                                                CultureInfo.CreateSpecificCulture("en-GB"),
                                                                                out result);
                                        newObject.GetType().GetField(RevertXMLSymbols(membername.InnerText),
                                                                     BindingFlags.NonPublic |
                                                                     BindingFlags.Public |
                                                                     BindingFlags.Instance).SetValue(newObject, result);
                                        ((IList) newmember).Add(result);
                                    }
                                    else if (RevertXMLSymbols(typ.InnerText).Equals("System.Char"))
                                    {
                                        Char result = ' ';
                                        System.Char.TryParse(RevertXMLSymbols(value.InnerText), out result);
                                        ((IList) newmember).Add(result);
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        if(workspaceManager != null)
                        {
                            workspaceManager.GuiLogMessage("Could not deserialize model element \"" + membername.InnerText + "\" of type \"" + membertype.InnerText + "\" because of:" + ex.Message,NotificationLevel.Warning);
                        }else
                        {
                            Console.WriteLine("Could not deserialize model element \"" + membername.InnerText + "\" of type \"" + membertype.InnerText + "\" because of:" + ex.Message);
                        }
                    }
                }
            }

            foreach (object[] triple in links)
            {

                object obj = triple[0];
                string membername = (string)triple[1];
                string reference = (string)triple[2];
                bool isList = (bool)triple[3];
                object obj2 = null;
                createdObjects.TryGetValue(reference, out obj2);

                try
                {
                    if (isList)
                    {
                        ((IList) obj.GetType().GetField(membername).GetValue(obj)).Add(obj2);
                    }
                    else
                    {
                        if (obj != null && obj2 != null)
                        {
                            FieldInfo fieldInfo = obj.GetType().GetField(membername,
                                                                         BindingFlags.NonPublic |
                                                                         BindingFlags.Public |
                                                                         BindingFlags.Instance);

                            fieldInfo.SetValue(obj, obj2);
                        }
                    }
                }
                catch(Exception ex)
                {
                    if (workspaceManager != null)
                    {
                        workspaceManager.GuiLogMessage("Could not restore reference beteen model element \"" + membername + "\" and its reference with id \"" + reference + "\" because of:" + ex.Message, NotificationLevel.Warning);
                    }
                    else
                    {
                        Console.WriteLine("Could not restore reference beteen model element \"" + membername + "\" and its reference with id \"" + reference + "\" because of:" + ex.Message);
                    }
                }
            }

            return createdObjects.Values.First();
        }
    }
}
