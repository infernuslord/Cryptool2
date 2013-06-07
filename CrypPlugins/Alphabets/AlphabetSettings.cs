/*
   Copyright 2008 Sebastian Przybylski, University of Siegen

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
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;

namespace Cryptool.Alphabets
{
    public class AlphabetSettings : ISettings
    {
        private string data;
        public string Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
                OnPropertyChanged("Data");
            }
        }

        private string outputOrderData;
        public string OutputOrderData
        {
            get
            {
                return outputOrderData;
            }
            set
            {
                outputOrderData = value;
                OnPropertyChanged("OutputOrderData");
            }
        }

        public static string SerializeOutputOrder(List<OutputOrder> items)
        {
            MemoryStream stream = new MemoryStream();
            string ret = string.Empty;
            try
            {

                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, items);
                ret = Convert.ToBase64String(stream.GetBuffer());
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
            }
            finally
            {
                if (stream != null)
                    stream.Close();

            }

            return ret;
        }

        public static List<OutputOrder> DeserializeOutputOrder(string serItems)
        {
            List<OutputOrder> ret = null;
            MemoryStream stream = null;
            try
            {
                byte[] buffer = Convert.FromBase64String(serItems);
                stream = new MemoryStream(buffer);
                BinaryFormatter formatter = new BinaryFormatter();
                ret = (List<OutputOrder>)formatter.Deserialize(stream);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            return ret;
        }


        public static string SerializeAlphabetItemData(List<AlphabetItemData> items)
        {
            MemoryStream stream = new MemoryStream();
            string ret = string.Empty;
            try
            {
               
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, items);
                ret = Convert.ToBase64String(stream.GetBuffer());
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
               
            }

            return ret;
        }

        public static List<AlphabetItemData> DeserializeAlphabetItemData(string serItems)
        {
            List<AlphabetItemData> ret = null;
            MemoryStream stream = null;
            try
            {
                byte[] buffer = Convert.FromBase64String(serItems);
                stream  = new MemoryStream(buffer);
                BinaryFormatter formatter = new BinaryFormatter();
                ret = (List<AlphabetItemData>)formatter.Deserialize(stream);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
            }
            finally
            {
                if(stream != null)
                    stream.Close();
            }

            return ret;
        }
        
        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}
