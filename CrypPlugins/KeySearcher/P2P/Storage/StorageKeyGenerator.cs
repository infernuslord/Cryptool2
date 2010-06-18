using System;
using System.Security.Cryptography;
using System.Text;

namespace KeySearcher.P2P.Storage
{
    class StorageKeyGenerator
    {
        private readonly KeySearcher keySearcher;
        private readonly KeySearcherSettings settings;

        public StorageKeyGenerator(KeySearcher keySearcher, KeySearcherSettings settings)
        {
            this.keySearcher = keySearcher;
            this.settings = settings;
            
        }

        public String Generate()
        {
            var rawIdentifier = "P2PJOB";
            rawIdentifier += settings.ChunkSize + settings.Key;
            rawIdentifier += keySearcher.ControlMaster.GetType();
            rawIdentifier += keySearcher.CostMaster.GetType();
            rawIdentifier += keySearcher.CostMaster.getBytesToUse();
            rawIdentifier += keySearcher.CostMaster.getRelationOperator();
            rawIdentifier += Encoding.ASCII.GetString(keySearcher.EncryptedData);

            var hashAlgorithm = new SHA1CryptoServiceProvider();
            var hash = hashAlgorithm.ComputeHash(Encoding.ASCII.GetBytes(rawIdentifier));
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}
