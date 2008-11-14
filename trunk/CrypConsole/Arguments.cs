using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace Cryptool.Console
{
    class Arguments
    {
        private StringDictionary parameters;

        public Arguments(string[] args)
        {
            parameters = new StringDictionary();
            Regex splitter = new Regex(@"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Regex remover = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            string parameter = null;
            string[] parts;

            foreach (string strPart in args)
            {
                parts = splitter.Split(strPart, 3);

                switch (parts.Length)
                {
                    case 1:
                        if (parameter != null)
                        {
                            if (!parameters.ContainsKey(parameter))
                            {
                                parts[0] = remover.Replace(parts[0], "$1");
                                parameters.Add(parameter, parts[0]);
                            }
                            parameter = null;
                        }
                        break;
                    case 2:
                        if (parameter != null)
                        {
                            if (!parameters.ContainsKey(parameter))
                                parameters.Add(parameter, "true");
                        }
                        parameter = parts[1];
                        break;
                    case 3:
                        if (parameter != null)
                        {
                            if (!parameters.ContainsKey(parameter))
                                parameters.Add(parameter, "true");
                        }
                        parameter = parts[1];
                        if (!parameters.ContainsKey(parameter))
                        {
                            parts[2] = remover.Replace(parts[2], "$1");
                            parameters.Add(parameter, parts[2]);
                        }
                        parameter = null;
                        break;
                }
            }
            if (parameter != null)
            {
                if (!parameters.ContainsKey(parameter))
                    parameters.Add(parameter, "true");
            }
        }
        public string this[string param]
        {
            get
            {
                return (parameters[param]);
            }
        }
    }
}
