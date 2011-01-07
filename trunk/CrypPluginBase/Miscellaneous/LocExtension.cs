/*
   Copyright 2010 Sven Rech

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
using System.Resources;
using System.Text;
using System.Windows.Markup;
using System.Xaml;
using Cryptool.PluginBase.Attributes;

// Register the extention in the Microsoft's default namespaces
[assembly: System.Windows.Markup.XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Cryptool.PluginBase.Miscellaneous")]
[assembly: System.Windows.Markup.XmlnsDefinition("http://schemas.microsoft.com/winfx/2007/xaml/presentation", "Cryptool.PluginBase.Miscellaneous")]
[assembly: System.Windows.Markup.XmlnsDefinition("http://schemas.microsoft.com/winfx/2008/xaml/presentation", "Cryptool.PluginBase.Miscellaneous")]

namespace Cryptool.PluginBase.Miscellaneous
{
    [MarkupExtensionReturnType(typeof(object))]
    [ContentProperty("Key")]
    public class LocExtension : MarkupExtension
    {
        public string Key { get; set; }

        public LocExtension(String key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            try
            {
                IRootObjectProvider service = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
                var locAttribute = (LocalizationAttribute)Attribute.GetCustomAttribute(service.RootObject.GetType(), typeof(LocalizationAttribute));
                ResourceManager resman = new ResourceManager(locAttribute.ResourceFile, service.RootObject.GetType().Assembly);
                
                if (resman.GetString(Key) != null)
                    return resman.GetString(Key);
                else
                    return Key;
            }
            catch (Exception ex)
            {
                return Key;
            }
        }

    }
}
