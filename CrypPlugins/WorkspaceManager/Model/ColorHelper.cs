using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Windows.Media;
using Cryptool.PluginBase;
using System.Reflection;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.Analysis;
using Cryptool.PluginBase.Generator;

namespace WorkspaceManager.Model
{
    public class ColorHelper
    {
        /// <summary>
        /// Returns a Color for a given Type
        /// 
        /// example:
        ///     System.String -> Colors.LightGray;
        ///     
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Color GetColor(Type type)
        {           
            if (type.FullName == "System.String")
            {                
                return Colors.WhiteSmoke;
            }
            else if (   type.FullName == "System.Int16" || 
                        type.FullName == "System.Int32" ||
                        type.FullName == "System.Int64" || 
                        type.FullName == "System.Int16[]" || 
                        type.FullName == "System.Int32[]" ||
                        type.FullName == "System.Int64[]")
            {
                return Colors.LightGoldenrodYellow;
            }
            else if (type.FullName == "Cryptool.PluginBase.IO.CryptoolStream")
            {
                return Colors.LightGreen;
            }
            else if (type.FullName == "System.Byte" || type.FullName == "System.Byte[]")
            {
                return Colors.LightSkyBlue;
            }
            else if (type.FullName == "System.Boolean" || type.FullName == "System.Boolean[]")
            {
                return Colors.Tomato;
            }
            else if (type.FullName == "System.Numerics.BigInteger")
            {
                return Colors.SteelBlue;
            }
            else
            {
                if (type.GetInterface(typeof(IEncryption).Name) != null)
                {
                    EncryptionTypeAttribute eta = type.GetEncryptionTypeAttribute();
                    switch (eta.EncryptionType)
                    {
                        case EncryptionType.Asymmetric:
                            return (Color)ColorConverter.ConvertFromString("#7089a2");

                        case EncryptionType.Classic:
                            return (Color)ColorConverter.ConvertFromString("#b8c881");

                        case EncryptionType.SymmetricBlock:
                            return (Color)ColorConverter.ConvertFromString("#d49090");

                        case EncryptionType.SymmetricStream:
                            return (Color)ColorConverter.ConvertFromString("#94bc8a");

                        case EncryptionType.Hybrid:
                            return (Color)ColorConverter.ConvertFromString("#d49090");
                    }
                }

                if (type.GetInterface(typeof(IGenerator).Name) != null)
                {
                    return (Color)ColorConverter.ConvertFromString("#8abc94");
                }

                if (type.GetInterface(typeof(IHash).Name) != null)
                {
                    return (Color)ColorConverter.ConvertFromString("#8abbbc");
                }

                if (type.GetInterface(typeof(IStatistic).Name) != null)
                {
                    return (Color)ColorConverter.ConvertFromString("#8c8abc");
                }

                if (type.GetInterface(typeof(IAnalysisMisc).Name) != null)
                {
                    return (Color)ColorConverter.ConvertFromString("#bc8aac");
                }

                return (Color)ColorConverter.ConvertFromString("#a3d090");
            }
        }
    }
}
