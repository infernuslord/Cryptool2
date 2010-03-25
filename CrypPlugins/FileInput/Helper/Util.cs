/*
   Copyright 2008 Thomas Schmid, University of Siegen

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
using System.Globalization;

namespace FileInput.Helper
{
  static class Util
  {
    public static string GetDisplayBytes(long size)
    {
      const long multi = 1024;
      long kb = multi;
      long mb = kb * multi;
      long gb = mb * multi;
      long tb = gb * multi;

      const string BYTES = "Bytes";
      const string KB = "KB";
      const string MB = "MB";
      const string GB = "GB";
      const string TB = "TB";

      string result;
      if (size < kb)
        result = string.Format("{0} {1}", size, BYTES);
      else if (size < mb)
        result = string.Format("{0} {1} ({2} Bytes)",
            ConvertToOneDigit(size, kb), KB, ConvertBytesDisplay(size));
      else if (size < gb)
        result = string.Format("{0} {1} ({2} Bytes)",
            ConvertToOneDigit(size, mb), MB, ConvertBytesDisplay(size));
      else if (size < tb)
        result = string.Format("{0} {1} ({2} Bytes)",
            ConvertToOneDigit(size, gb), GB, ConvertBytesDisplay(size));
      else
        result = string.Format("{0} {1} ({2} Bytes)",
            ConvertToOneDigit(size, tb), TB, ConvertBytesDisplay(size));

      return result;
    }

    static string ConvertBytesDisplay(long size)
    {
      return size.ToString("###,###,###,###,###", CultureInfo.CurrentCulture);
    }

    static string ConvertToOneDigit(long size, long quan)
    {
      double result = (double)size / (double)quan;
      string sResult = result.ToString("0.#", CultureInfo.CurrentCulture);
      return sResult;
    }
  }


}
