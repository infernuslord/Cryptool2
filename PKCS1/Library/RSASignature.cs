using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Math;

namespace PKCS1.Library
{
    public class RSASignature : Signature
    {
        #region encrypted PKCS1 Signature

        private byte[] getCompleteHw()
        {
            byte[] hashIdent = Hex.Decode(Datablock.getInstance().HashFunctionIdent.DERIdent);
            byte[] hashDigest = Hashfunction.generateHashDigest(Datablock.getInstance().Message, Datablock.getInstance().HashFunctionIdent);          
            byte[] returnArray = new byte[hashIdent.Length + Hashfunction.getDigestSize()];
            Array.Copy(hashIdent, 0, returnArray, 0, hashIdent.Length);
            Array.Copy(hashDigest, 0, returnArray, returnArray.Length - hashDigest.Length, hashDigest.Length);

            return returnArray;
        }       

        public override void GenerateSignature()
        {
            if (RSAKeyManager.getInstance().isKeyGenerated())
            {
                // RSA Schlüssellänge setzen für Methode in Oberklasse
                this.m_KeyLength = RSAKeyManager.getInstance().RsaKeySize;

                IAsymmetricBlockCipher signerPkcs1Enc = new Pkcs1Encoding(new RsaEngine());
                signerPkcs1Enc.Init(true, RSAKeyManager.getInstance().getPrivKey());
                byte[] output = signerPkcs1Enc.ProcessBlock(this.getCompleteHw(), 0, this.getCompleteHw().Length);
 
                this.m_bSigGenerated = true;
                this.m_Signature = output;
                this.OnRaiseSigGenEvent(SignatureType.Pkcs1);                
            }
        }

        #endregion //encrypted PKCS1 Signature
    }
}
