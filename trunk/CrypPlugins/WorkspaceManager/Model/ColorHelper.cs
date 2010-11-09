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
using Cryptool.PluginBase.IO;
using System.IO;
using Cryptool.PluginBase.Control;

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

                if (type.GetInterface(typeof(IEncryption).Name) != null)
                {
                    EncryptionTypeAttribute eta = type.GetEncryptionTypeAttribute();
                    switch (eta.EncryptionType)
                    {
                        case EncryptionType.Asymmetric:
                            return (Color)ColorConverter.ConvertFromString("#6789a2");

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

        public static Color GetLineColor(Type type)
        {
            if (typeof(int).Equals(type) || typeof(int[]).Equals(type)) return Colors.Aqua;
            if (typeof(byte[]).Equals(type) || typeof(byte[]).Equals(type)) return Colors.ForestGreen;
            if (typeof(double).Equals(type) || typeof(double[]).Equals(type)) return Colors.Blue;
            if (typeof(bool).Equals(type) || typeof(bool[]).Equals(type)) return Colors.Maroon;

            if (typeof(CryptoolStream).Equals(type)) return Colors.Orange;
            if (typeof(Stream).Equals(type) || typeof(CStream).Equals(type)) return Colors.DarkOrange;
            if (typeof(string).Equals(type) || typeof(string[]).Equals(type)) return Colors.Gray;

            if (typeof(object).Equals(type)) return Colors.MediumPurple;
            if (typeof(BigInteger).Equals(type)) return Colors.Black;
            return Colors.Black;
        }
    }
}
