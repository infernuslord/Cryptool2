//////////////////////////////////////////////////////////////////////////////////////////////////
// CrypTool V2
// © 2008 - Gerhard Junker
// Apache License see http://www.apache.org/licenses/
//
// $HeadURL$
//////////////////////////////////////////////////////////////////////////////////////////////////
// $Revision::                                                                                $://
// $Author::                                                                                  $://
// $Date::                                                                                    $://
//////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

using System.Security.Cryptography;
using System.Runtime.InteropServices;

#if DEBUG
using System.Diagnostics;
#endif

namespace System.Security.Cryptography
{

    [ComVisibleAttribute(true)]
    public class PKCS5MaskGenerationMethod : System.Security.Cryptography.MaskGenerationMethod
    {

        /// <summary>
        /// implemented hash functions
        /// </summary>
        public enum ShaFunction
        {
            MD5,
            SHA1,
            SHA256,
            SHA384,
            SHA512
        };

        private ShaFunction selectedShaFunction = ShaFunction.SHA256;

        /// <summary>
        /// Gets or sets the selected sha function.
        /// </summary>
        /// <value>The selected sha function.</value>
        public ShaFunction SelectedShaFunction
        {
            get
            {
                return selectedShaFunction;
            }
            set
            {
                selectedShaFunction = value;
            }
        }

        /// <summary>
        /// Gets the length of the hash.
        /// </summary>
        /// <param name="shaf">The shaf.</param>
        /// <returns></returns>
        public int GetHashLength()
        {
            switch (selectedShaFunction)
            {
                case ShaFunction.MD5:
                    return 16;

                case ShaFunction.SHA1:
                    return 20;

                case ShaFunction.SHA256:
                    return 32;

                case ShaFunction.SHA384:
                    return 48;

                case ShaFunction.SHA512:
                    return 64;

                default:
                    throw new ArgumentOutOfRangeException("SelectedShaFunction");
            }
        }

        /// <summary>
        /// Gets the sha hash.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="salt">The salt.</param>
        /// <returns></returns>
        private byte[] GetShaHash(byte[] key, byte[] salt)
        {
            HMAC ha;

            switch (selectedShaFunction)
            {
                case ShaFunction.MD5:
                    ha = (HMAC)new System.Security.Cryptography.HMACMD5();
                    break;

                case ShaFunction.SHA1:
                    ha = (HMAC)new System.Security.Cryptography.HMACSHA1();
                    break;

                case ShaFunction.SHA256:
                    ha = (HMAC)new System.Security.Cryptography.HMACSHA256();
                    break;

                case ShaFunction.SHA384:
                    ha = (HMAC)new System.Security.Cryptography.HMACSHA384();
                    break;

                case ShaFunction.SHA512:
                    ha = (HMAC)new System.Security.Cryptography.HMACSHA512();
                    break;

                default:
                    throw new ArgumentOutOfRangeException("SelectedShaFunction");
            }

            ha.Key = key;
            byte[] h = ha.ComputeHash(salt);
            ha.Clear();

            return h;
        }

        /// <summary>
        /// Gens the key block.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="count">The count.</param>
        /// <param name="blockIndex">Index of the block.</param>
        /// <returns></returns>
        private byte[] GenKeyBlock(byte[] password, byte[] salt, int count, int blockIndex)
        {
            int len = salt.Length;
            byte[] data = new byte[len + 4];

            for (int i = 0; i < len; i++)
                data[i] = salt[i];

            data[len] = data[len + 1] = data[len + 2] = 0;
            data[len + 3] = (byte)blockIndex;

            byte[] u1 = GetShaHash(password, data);

            byte[] result = new byte[u1.Length];
            for (int i = 0; i < u1.Length; i++)
                result[i] = u1[i];

            for (int c = 1; c < count; c++)
            {
                byte[] u2 = GetShaHash(password, u1);

                for (int i = 0; i < result.Length; i++)
                {
                    result[i] ^= u2[i];
                    u1[i] = u2[i];
                }
            }

            return result;
        }

        /// <summary>
        /// When overridden in a derived class, generates a mask with the specified length using the specified random salt.
        /// </summary>
        /// <param name="rgbSalt">The random salt to use to compute the mask.</param>
        /// <param name="cbReturn">The length of the generated mask in bytes.</param>
        /// <returns>
        /// A randomly generated mask whose length is equal to the <paramref name="cbReturn"/> parameter.
        /// </returns>
        [ComVisibleAttribute(true)]
        public override byte[] GenerateMask
            (
                byte[] rgbSalt,
                int cbReturn
            )
        {
            // member implemented for compatibility only ....
            // throw new System.NotImplementedException("GenerateMask needs more parameters");

            // Computes masks according to PKCS #1 for use by key exchange algorithms. 
            // Generates and returns a mask from the specified random salt of the specified length. 
            PKCS1MaskGenerationMethod pkcs1 = new PKCS1MaskGenerationMethod();
            return pkcs1.GenerateMask(rgbSalt, cbReturn);
        }

        /// <summary>
        /// When overridden in a derived class, generates a mask with the specified length using the specified password and random salt.
        /// Implementing PBKDF2 of PKCS #5 v2.1 Password-Basd Cryptography Standard
        /// </summary>
        /// <param name="password">The password to use to compute the mask.</param>
        /// <param name="rgbSalt">The random salt (seed) to use to compute the mask.</param>
        /// <param name="count">The iteration count, a positive integer</param>
        /// <param name="cbReturn">The length of the generated mask in bytes.</param>
        /// <returns>
        /// A randomly generated mask whose length is equal to the <paramref name="cbReturn"/> parameter.
        /// </returns>
        [ComVisibleAttribute(true)]
        public byte[] GenerateMask
            (
                byte[] password,
                byte[] rgbSalt,
                int count,
                int cbReturn
            )
        {
            if (cbReturn <= 0)
                cbReturn = 1;
            //throw new ArgumentOutOfRangeException("cbReturn", "cbReturn must be positive.");

            byte[] key = new byte[cbReturn];
            for (int i = 0; i < cbReturn; i++)
                key[i] = 0;

            // check parameters
            if (password.Length == 0)
                return key;
            //throw new ArgumentOutOfRangeException("Password", "password must be not empty; a minimum of 8 bytes is recommended.");

            if (rgbSalt.Length == 0)
                return key;
            //throw new ArgumentOutOfRangeException("Salt", "Salt must be not empty; a minimum of 8 bytes is recommended.");

            if (count <= 0)
                return key;
            //throw new ArgumentOutOfRangeException("Count", "Count must be positive; a minimum of 1000 is recommended.");

            int hLen = GetHashLength();

            // Let blockCount be the number of hLen-bytes blocks in the derived key, rounding up,
            // let fillBytes be the number of bytes in the last block.
            int blockCount = cbReturn / hLen + 1;
            int fillBytes = cbReturn - (blockCount - 1) * hLen;
            //if (fillBytes == 0)
            //  blockCount--;

#if DEBUG
            {
                string msg = string.Format("Generate {0} blocks with {1} bytes.", blockCount, hLen);
                Debug.WriteLine(msg);
                msg = string.Format("The last block has {0} bytes.", fillBytes);
                Debug.WriteLine(msg);
                msg = string.Format("requested bytes {0} - generating {1} bytes.", cbReturn, (blockCount - 1) * hLen + fillBytes);
                Debug.WriteLine(msg);
            }
#endif

            if (blockCount > 255)
            {
                string msg = string.Format("cbReturn must be lesser than {0} by implementation limits.", hLen * 255);
                throw new ArgumentOutOfRangeException("cbReturn", "msg");
            }

            int outPos = 0;

            for (int blockIndex = 0; blockIndex < blockCount - 1; blockIndex++)
            {
                byte[] block = GenKeyBlock(password, rgbSalt, count, blockIndex);
                for (int i = 0; i < hLen; i++)
                    key[outPos++] = block[i];
            }

            // last block
            if (fillBytes > 0)
            {
                byte[] block = GenKeyBlock(password, rgbSalt, count, blockCount);
                for (int i = 0; i < fillBytes; i++)
                    key[outPos++] = block[i];
            }

            return key;
        } // PBKDF2
    } // class
}

