using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace OnlineDocumentationGenerator.Generators.LaTeXGenerator
{
    class Helper
    {
        public static string EscapeLaTeX(string value)
        {
            var res = value.Replace("\\", "\\textbackslash").Replace("&", "\\&").Replace("%", "\\%")
                .Replace("#", "\\#").Replace("_", "\\_").Replace("{", "\\{")
                .Replace("}", "\\}").Replace("~", "\\textasciitilde").Replace("^", "\\textasciicircum")
                .Replace("`", "\\glq ").Replace("´", "\\grq ")
                .Replace("\"", "\\\"").Replace("“", "\"'").Replace("„", "\"`");
            return res;
            //foreach (var ch in res)
            //{
            //    if (CultureInfo.CurrentCulture.TextInfo.)
            //}
        }
    }
}
