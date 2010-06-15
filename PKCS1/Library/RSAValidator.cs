﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Security;

namespace PKCS1.Library
{
    class RSAValidator
    {
        HashFunctionIdent funcIdent = null;

        #region verify Signatures

        private bool verifySig(ISigner verifier, string message, byte[] signature)
        {            
            verifier.Init(false, RSAKeyManager.getInstance().getPubKey());            
            byte[] messageInBytes = Encoding.ASCII.GetBytes(message);
            verifier.BlockUpdate(messageInBytes, 0, messageInBytes.Length); // update bekommt klartextnachricht als param           
            return verifier.VerifySignature(signature); // input ist verschlüsselte Nachricht
        }

        public bool verifyRsaSignature(string message, byte[] signature)
        {
            IAsymmetricBlockCipher eng = new Pkcs1Encoding(new RsaEngine());
            //IAsymmetricBlockCipher eng = new RsaEngine();
            eng.Init(false, RSAKeyManager.getInstance().getPubKey());

            try
            {
                //byte[] messageInBytes = Encoding.ASCII.GetBytes(message);
                byte[] data = eng.ProcessBlock(signature, 0, signature.Length);
                funcIdent = this.extractHashFunction(Encoding.ASCII.GetString(Hex.Encode(data)));

                if (null != funcIdent)
                {
                    string signerIdent = funcIdent.diplayName + "withRSA";
                    ISigner verifier = SignerUtilities.GetSigner(signerIdent);
                    return this.verifySig(verifier, message, signature);
                }
            }
            catch(Exception e)
            {
                return false;
            }

            return false;
        }

        public string getHashFunctionName()
        {
            if (this.funcIdent != null)
            {
                return funcIdent.diplayName;
            }
            return String.Empty;
        }

        private HashFunctionIdent extractHashFunction(string input)
        {
            string inputString = input.ToLower();

            if (inputString.StartsWith(HashFunctionHandler.SHA1.DERIdent.ToLower()))
            {
                return HashFunctionHandler.SHA1;
            }
            else if (inputString.StartsWith(HashFunctionHandler.SHA256.DERIdent.ToLower()))
            {
                return HashFunctionHandler.SHA256;
            }
            else if (inputString.StartsWith(HashFunctionHandler.SHA384.DERIdent.ToLower()))
            {
                return HashFunctionHandler.SHA384;
            }
            else if (inputString.StartsWith(HashFunctionHandler.SHA512.DERIdent.ToLower()))
            {
                return HashFunctionHandler.SHA512;
            }
            else if (inputString.StartsWith(HashFunctionHandler.MD2.DERIdent.ToLower()))
            {
                return HashFunctionHandler.MD2;
            }
            else if (inputString.StartsWith(HashFunctionHandler.MD5.DERIdent.ToLower()))
            {
                return HashFunctionHandler.MD5;
            }

            return null;
        }

        public bool verifyRsaSignatureWithFlaw(string message, byte[] signature)
        {
            BigInteger signatureBigInt = new BigInteger(1,signature);
            byte[] messageInBytes = Encoding.ASCII.GetBytes(message);           

            RsaKeyParameters pubkeyParam = (RsaKeyParameters)RSAKeyManager.getInstance().getPubKey();
            BigInteger exponent = pubkeyParam.Exponent;
            BigInteger modulus = pubkeyParam.Modulus;

            byte[] sigDecrypted = (signatureBigInt.ModPow(exponent, modulus)).ToByteArray();
            byte[] block = this.DerEncode(sigDecrypted); // hiernach steht DERIdent und hash am anfang des arrays, danach garbage

            funcIdent = this.extractHashFunction(Encoding.ASCII.GetString(Hex.Encode(block)));

            // SHA1, Digest length: 160 Bit
            if(funcIdent == HashFunctionHandler.SHA1)
            {
                return this.verifySigWithoutPad(block, messageInBytes, HashFunctionHandler.SHA1, 20);
            }

            // SHA-256, Digest length: 256 Bit
            if(funcIdent == HashFunctionHandler.SHA256)
            {
                return this.verifySigWithoutPad(block, messageInBytes, HashFunctionHandler.SHA256, 32);
            }

            // SHA-384, Digest length: 384 Bit
            if(funcIdent == HashFunctionHandler.SHA384)
            {
                return this.verifySigWithoutPad(block, messageInBytes, HashFunctionHandler.SHA384, 48);
            }

            // SHA-512, Digest length: 512 Bit
            if(funcIdent == HashFunctionHandler.SHA512)
            {
                return this.verifySigWithoutPad(block, messageInBytes, HashFunctionHandler.SHA512, 64);
            }

            // MD2, Digest length: 120 Bit
            if(funcIdent == HashFunctionHandler.MD2)
            {
                return this.verifySigWithoutPad(block, messageInBytes, HashFunctionHandler.MD2, 15);
            }

            // MD5, Digest length: 120 Bit
            if(funcIdent == HashFunctionHandler.MD5)
            {
                return this.verifySigWithoutPad(block, messageInBytes, HashFunctionHandler.MD5, 15);
            }

            return false;
        }

        private bool verifySigWithoutPad(byte[] sigWithoutPad, byte[] message, HashFunctionIdent hashFuncIdent, int digestLength)
        {
            //TODO Längen überprüfen!
            string blockString = Encoding.ASCII.GetString(Hex.Encode(sigWithoutPad)).ToLower();

            byte[] hashDigestFromSig = new byte[digestLength];
            int endOfIdent = hashFuncIdent.DERIdent.Length / 2;
            Array.Copy(sigWithoutPad, endOfIdent, hashDigestFromSig, 0, digestLength);

            IDigest hashFunctionDigest = DigestUtilities.GetDigest(hashFuncIdent.diplayName);
            byte[] hashDigestMessage = Hashfunction.generateHashDigest(message, hashFuncIdent);

            return this.compareByteArrays(hashDigestFromSig, hashDigestMessage);
        }

        private bool compareByteArrays(byte[] input1, byte[] input2)
        {
            bool bEqual = false;

            string test1 = Encoding.ASCII.GetString(Hex.Encode(input1));
            string test2 = Encoding.ASCII.GetString(Hex.Encode(input2));

            if (input1.Length == input2.Length)
            {
                int i = 0;
                while ((i < input1.Length) && (input1[i] == input2[i]))
                {
                    i += 1;
                }
                if (i == input1.Length)
                {
                    bEqual = true;
                }
            }
            return bEqual;
        }

        private byte[] DerEncode(byte[] block)
        {
            // hier 0001 checken, dann FF Bytes skippen, dann HW zurück geben, Länge auslesen?            
            byte type = block[0];

            if (type != 1 && type != 2)
            {
                // TODO Exception schmeissen
                //throw new InvalidCipherTextException("unknown block type");
            }

            int start;
            for (start = 1; start != block.Length; start++)
            {
                byte pad = block[start];

                if (pad == 0)
                {
                    break;
                }

                if (type == 1 && pad != (byte)0xff)
                {
                    throw new InvalidCipherTextException("block padding incorrect");
                }
            }
            start++;           // data should start at the next byte

            /*
            if (start > block.Length || start < HeaderLength)
            {
                throw new InvalidCipherTextException("no data in block");
            }*/

            byte[] result = new byte[block.Length - start];
            Array.Copy(block, start, result, 0, result.Length);

            return result; // anschliessend muss punkt 2.3 kommen
        }

        #endregion // verify Signatures


    }
}