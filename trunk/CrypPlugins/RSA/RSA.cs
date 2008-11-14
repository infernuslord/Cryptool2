using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using Cryptool.PluginBase;

namespace Cryptool.RSA
{
    [PluginInfo("E31C83F3-A686-40ad-AA8D-A0CF6AE1B9CC","RSA","RSA cipher")]
    public class RSA : IEncryptionAlgorithm
    {
        private RSASettings settings;

        public RSA()
        {
            this.settings = new RSASettings();
        }

        public void Add(IEncryptionAlgorithmVisualization visualization)
        {

        }

        public IKey GenerateKey()
        {
            return null;
        }

        public EncryptionAlgorithmType AlgorithmType
        {
            get { return EncryptionAlgorithmType.Asymmetric; }
        }


        public Stream Encrypt(IEncryptionAlgorithmSettings settings)//byte[] inputData, RSAParameters RSAKeyInfo)
        {
            RSACryptoServiceProvider rsaCipher = new RSACryptoServiceProvider();

            rsaCipher.ImportParameters(((RSASettings)settings).RsaKeyInfo);

          //  return rsaCipher.Encrypt(((RSASettings)settings).InputData, false);

            return null;
        }

        public Stream Decrypt(IEncryptionAlgorithmSettings settings)//byte[] inputData, RSAParameters RSAKeyInfo)
        {
            RSACryptoServiceProvider rsaCipher = new RSACryptoServiceProvider();

            rsaCipher.ImportParameters(((RSASettings)settings).RsaKeyInfo);

          //  return rsaCipher.Decrypt(((RSASettings)settings).InputData, false);

            return null;
        }

        public IEncryptionAlgorithmSettings GetSettingsObject()
        {
            return this.settings;
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
