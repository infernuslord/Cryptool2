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
                            return Colors.MediumSeaGreen;

                        case EncryptionType.Classic:
                            return Colors.LightBlue;

                        case EncryptionType.SymmetricBlock:
                            return Colors.LightYellow;

                        case EncryptionType.SymmetricStream:
                            return Colors.LightSteelBlue;

                        case EncryptionType.Hybrid:
                            return Colors.Khaki;
                    }
                }

                if (type.GetInterface(typeof(IGenerator).Name) != null)
                {
                    return Colors.LemonChiffon;
                }

                if (type.GetInterface(typeof(IHash).Name) != null)
                {
                    return Colors.Indigo;
                }

                if (type.GetInterface(typeof(IStatistic).Name) != null)
                {
                    return Colors.Violet;
                }

                if (type.GetInterface(typeof(IAnalysisMisc).Name) != null)
                {
                    return Colors.OrangeRed;
                }

                return Color.FromRgb(75,246,92);
            }
        }
    }
}
