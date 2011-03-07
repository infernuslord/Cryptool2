/*                              
   Copyright 2010 Nils Kopal

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
    public static class ColorHelper
    {
        public static Color AsymmetricColor { get; set; }
        public static Color ClassicColor { get; set; }
        public static Color SymmetricBlockColor { get; set; }
        public static Color SymmetricStreamColor { get; set; }
        public static Color HybridColor { get; set; }
        public static Color GeneratorColor { get; set; }
        public static Color HashColor { get; set; }
        public static Color StatisticColor { get; set; }
        public static Color AnalysisMiscColor { get; set; }

        public static Color IntegerColor { get; set; }
        public static Color ByteColor { get; set; }
        public static Color DoubleColor { get; set; }
        public static Color BoolColor { get; set; }
        public static Color StreamColor { get; set; }
        public static Color StringColor { get; set; }
        public static Color ObjectColor { get; set; }
        public static Color BigIntegerColor { get; set; }
        public static Color DefaultColor { get; set; }

        static ColorHelper(){
            SetDefaultColors();   
        }

        /// <summary>
        /// Set colors to default values
        /// </summary>
        public static void SetDefaultColors(){
            AsymmetricColor = (Color)ColorConverter.ConvertFromString("#6789a2");
            ClassicColor = (Color)ColorConverter.ConvertFromString("#b8c881");
            SymmetricBlockColor = (Color)ColorConverter.ConvertFromString("#d49090");
            SymmetricStreamColor = (Color)ColorConverter.ConvertFromString("#94bc8a");
            HybridColor = (Color)ColorConverter.ConvertFromString("#d49090");
            GeneratorColor = (Color)ColorConverter.ConvertFromString("#8abc94");
            HashColor = (Color)ColorConverter.ConvertFromString("#8abbbc");
            StatisticColor = (Color)ColorConverter.ConvertFromString("#8c8abc");
            AnalysisMiscColor = (Color)ColorConverter.ConvertFromString("#bc8aac");
            IntegerColor = Colors.Aqua;
            ByteColor = Colors.ForestGreen;
            DoubleColor = Colors.Blue;
            BoolColor = Colors.Maroon;
            StreamColor = Colors.DarkOrange;
            StringColor = Colors.Gray;
            ObjectColor = Colors.MediumPurple;
            BigIntegerColor = Colors.Black;
            DefaultColor = (Color)ColorConverter.ConvertFromString("#a3d090");
        }

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
            try
            {
                if (type == null)
                {
                    return DefaultColor;
                }
                if (type.GetInterface(typeof(IEncryption).Name) != null)
                {
                    EncryptionTypeAttribute eta = type.GetEncryptionTypeAttribute();
                    switch (eta.EncryptionType)
                    {
                        case EncryptionType.Asymmetric:
                            return AsymmetricColor;

                        case EncryptionType.Classic:
                            return ClassicColor;

                        case EncryptionType.SymmetricBlock:
                            return SymmetricBlockColor;

                        case EncryptionType.SymmetricStream:
                            return SymmetricStreamColor;

                        case EncryptionType.Hybrid:
                            return HybridColor;
                    }
                }

                if (type.GetInterface(typeof(IGenerator).Name) != null)
                {
                    return GeneratorColor;
                }

                if (type.GetInterface(typeof(IHash).Name) != null)
                {
                    return HashColor;
                }

                if (type.GetInterface(typeof(IStatistic).Name) != null)
                {
                    return StatisticColor;
                }

                if (type.GetInterface(typeof(IAnalysisMisc).Name) != null)
                {
                    return AnalysisMiscColor;
                }

                return DefaultColor;
            }
            catch (Exception)
            {
                return Colors.Black; ;
            }
            
        }

        /// <summary>
        /// Get a color for a ConnectorModel (drawed as line)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Color GetLineColor(Type type)
        {
            try
            {
                if (typeof(int).Equals(type) || typeof(int[]).Equals(type)) return IntegerColor;
                if (typeof(byte[]).Equals(type) || typeof(byte[]).Equals(type)) return ByteColor;
                if (typeof(double).Equals(type) || typeof(double[]).Equals(type)) return DoubleColor;
                if (typeof(bool).Equals(type) || typeof(bool[]).Equals(type)) return BoolColor;

                if (typeof(Stream).Equals(type)) return StreamColor;
                if (typeof(string).Equals(type) || typeof(string[]).Equals(type)) return StringColor;

                if (typeof(object).Equals(type)) return ObjectColor;
                if (typeof(BigInteger).Equals(type)) return BigIntegerColor;
                return DefaultColor;
            }
            catch (Exception)
            {
                return Colors.Black;
            }
        }
    }
}
