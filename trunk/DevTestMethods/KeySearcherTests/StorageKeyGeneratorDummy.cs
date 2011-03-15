using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeySearcher.P2P.Storage;

namespace KeySearcher.P2P.UnitTests
{
    class StorageKeyGeneratorDummy : StorageKeyGenerator
    {
        public StorageKeyGeneratorDummy(KeySearcher keySearcher, KeySearcherSettings settings) : base(keySearcher, settings)
        {
        }

        public override string Generate()
        {
            return "bla";
        }
    }
}
