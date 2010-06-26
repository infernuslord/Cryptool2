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
            // Add simple data
            var bytesToUse = keySearcher.CostMaster.getBytesToUse();
            var rawIdentifier = "P2PJOB";
            rawIdentifier += settings.ChunkSize + settings.Key;
            rawIdentifier += keySearcher.ControlMaster.GetType();
            rawIdentifier += keySearcher.CostMaster.GetType();
            rawIdentifier += bytesToUse;
            rawIdentifier += keySearcher.CostMaster.getRelationOperator();

            // Add initialization vector when available
            if (keySearcher.InitVector != null)
            {
                rawIdentifier += Encoding.ASCII.GetString(keySearcher.InitVector);
            }

            // Add input data with the amount of used bytes
            var inputData = keySearcher.EncryptedData;
            if (inputData.Length > bytesToUse)
                Array.Copy(inputData, inputData, bytesToUse);

            rawIdentifier += Encoding.ASCII.GetString(inputData);

            // Add cost of input data to preserve cost master settings
            rawIdentifier += keySearcher.CostMaster.calculateCost(inputData);

            // Add decrypted input data to preserve encryption settings
            var keyLength = keySearcher.Pattern.giveInputPattern().Length / 3;
            var decryptedData = keySearcher.ControlMaster.Decrypt(inputData, new byte[keyLength], new byte[8]);
            rawIdentifier += Encoding.ASCII.GetString(decryptedData);

            var hashAlgorithm = new SHA1CryptoServiceProvider();
            var hash = hashAlgorithm.ComputeHash(Encoding.ASCII.GetBytes(rawIdentifier));
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}
