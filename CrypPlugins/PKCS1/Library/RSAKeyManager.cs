using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace PKCS1.Library
{
    class RSAKeyManager
    {
        #region Singleton

        private static RSAKeyManager m_Instance = null;
        public static RSAKeyManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new RSAKeyManager();
                }
                return m_Instance;
            }
        }

        private RSAKeyManager()
        {
        }

        #endregion

        public event ParamChanged RaiseKeyGeneratedEvent;
              
        private AsymmetricCipherKeyPair keyPair = null;
        private bool m_bRsaKeyGenerated = false;

        private int m_RsaKeySize = 2048; // default
        public int RsaKeySize
        {
            set 
            {
                this.m_RsaKeySize = (int)value;
                OnRaiseKeyGenerated(ParameterChangeType.ModulusSize);
            }
            get { return this.m_RsaKeySize; }
        }

        private BigInteger m_PubExponent = BigInteger.ValueOf(3); // default
        public int PubExponent
        {
            set 
            { 
                this.m_PubExponent = BigInteger.ValueOf(value);
                OnRaiseKeyGenerated(ParameterChangeType.PublicExponent);
            }
            get { return this.m_PubExponent.IntValue; }
        }

        // Rsa Schlüssel generieren       
        public void genRsaKeyPair(int certainty)
        {
            BigInteger pubExp = BigInteger.ValueOf(this.PubExponent);
            int strength = this.RsaKeySize;
            RsaKeyPairGenerator fact = new RsaKeyPairGenerator();

            RsaKeyGenerationParameters factParams = new RsaKeyGenerationParameters(pubExp, new SecureRandom(), strength, certainty);
            fact.Init(factParams);

            this.keyPair = fact.GenerateKeyPair();
            this.m_bRsaKeyGenerated = true;
            OnRaiseKeyGenerated(ParameterChangeType.RsaKey);
        }

        public bool isKeyGenerated()
        {
            return this.m_bRsaKeyGenerated;
        }

        private void OnRaiseKeyGenerated(ParameterChangeType type)
        {
            if (RaiseKeyGeneratedEvent != null)
            {
                RaiseKeyGeneratedEvent(type);
            }
        }


        public AsymmetricKeyParameter getPrivKey()
        {
            return this.keyPair.Private;          
        }

        public BigInteger getPrivKeyToBigInt()
        {
            RsaKeyParameters privKeyParam = (RsaKeyParameters)this.getPrivKey();
            return privKeyParam.Exponent;
        }

        public AsymmetricKeyParameter getPubKey()
        {
             return this.keyPair.Public;
        }

        public BigInteger getPubKeyToBigInt()
        {
            RsaKeyParameters pubKeyParam = (RsaKeyParameters)this.getPubKey();
            return pubKeyParam.Exponent;
        }

        public BigInteger getModulusToBigInt()
        {            
            RsaKeyParameters pubkeyParam = (RsaKeyParameters)RSAKeyManager.Instance.getPubKey();           
            return pubkeyParam.Modulus;
        }
    }
}
