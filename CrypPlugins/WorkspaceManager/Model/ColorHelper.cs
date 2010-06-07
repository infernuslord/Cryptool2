using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Windows.Media;

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
        public static Color getColor(Type type)
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
                return Colors.SkyBlue;
            }
            else if (type.FullName == "Cryptool.PluginBase.IO.CryptoolStream")
            {
                return Colors.Orange;
            }
            else if (type.FullName == "System.Byte" || type.FullName == "System.Byte[]")
            {
                return Colors.LightGreen;
            }
            else if (type.FullName == "System.Boolean" || type.FullName == "System.Boolean[]")
            {
                return Colors.Red;
            }
            else if (type.FullName == "System.Numerics.BigInteger")
            {
                return Colors.Purple;
            }
            else
            {
                return Colors.Black;
            }
        }
    }
}
