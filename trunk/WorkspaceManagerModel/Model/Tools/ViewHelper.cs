/*                              
   Copyright 2011 Nils Kopal

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

namespace WorkspaceManagerModel.Model.Tools
{
    /// <summary>
    /// This class contains helper methods for the view. Conversion from model data to view and so on
    /// </summary>
    public static class ViewHelper
    {
        /// <summary>
        /// This number is the maximum number of values of an array which should
        /// be used for the creation of the PresentationString
        /// </summary>
        private const int MaxUsedArrayValues = 100;

        /// <summary>
        /// Converts a given model data value into a valid view string which
        /// can be shown to the user. Returns "null" if there was no valid data
        /// or a given array was empty. Maximum length of returned string is 50.
        /// If string gets longer than maxcount only string.Substring(0,maxcount) + "..." is
        /// returned 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="maxCharacters"></param>
        /// <returns></returns>
        public static string GetDataPresentationString(object data, int maxCharacters = 200)
        {
            var str = ConvertDataPresentation(data);
            if (str.Length > maxCharacters)
            {
                return str.Substring(0, maxCharacters) + "...";
            }
            return str;
        }

        private static string ConvertDataPresentation(object data)
        {
            if (data == null)
            {
                return "null";
            }

            if (data is byte[])
            {
                return BitConverter.ToString((byte[])data);
            }
            
            if (data is Array)
            {
                var array = (Array)data;
                
                switch (array.Length)
                {
                    case 0:
                        return "null";
                    case 1:
                        return array.GetValue(0).ToString();
                    default:
                        var str = "" + array.GetValue(0) + ",";
                        var counter = 0;
                        for (int i = 1; i < array.Length - 1; i++)
                        {
                            str += (array.GetValue(i) + ",");
                            counter++;
                            if(counter==MaxUsedArrayValues)
                            {
                                return str;
                            }
                        }
                        str += array.GetValue(array.Length - 1);
                        return str;
                }
            }

            return data.ToString();    
        }
    }
}
