using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wizard
{
    [Serializable]
    public class StorageEntry
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public DateTime Created { get; set; }

        public StorageEntry(string key, string value)
        {
            Key = key;
            Value = value;
            Created = DateTime.Now;
        }
    }
}
