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
  /// <summary>
  /// 
  /// </summary>
  [ComVisibleAttribute(true)]
  partial class TWOFISH : SymmetricAlgorithm
  {
    private const int	BLOCK_SIZE   = 128;
    private const int	ROUNDS       =  16;
    private const int	MAX_KEY_BITS = 256;
    private const int	MIN_KEY_BITS = 128;

    /// <summary>
    /// Initializes a new instance of the <see cref="TWOFISH"/> class.
    /// </summary>
    /// <exception cref="T:System.Security.Cryptography.CryptographicException">
    /// The implementation of the class derived from the symmetric algorithm is not valid.
    /// </exception>
    public TWOFISH()
    {
      BlockSize = BLOCK_SIZE;   // valid: 128 = 16 bytes 
      KeySize   = MIN_KEY_BITS; // valid: 128, 192, 256

      Padding   = PaddingMode.Zeros;
      ModeValue = CipherMode.ECB;

      Key = new byte[KeySize   / 8]; // zeroed by default
      IV  = new byte[BlockSize / 8]; // zeroed by default
    }

    /// <summary>
    /// Creates a symmetric decryptor object with the specified 
    /// <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key"/> property and 
    /// initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV"/>).
    /// </summary>
    /// <param name="rgbKey">The secret key to use for the symmetric algorithm.</param>
    /// <param name="rgbIV">The initialization vector to use for the symmetric algorithm.</param>
    /// <returns>A symmetric decryptor object.</returns>
    public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
    {
      Key = rgbKey;

      if (Mode == CipherMode.CBC)
        IV = rgbIV;

      return new TwofishEncryption(KeySize, ref KeyValue, ref IVValue, ModeValue, 
        TWOFISH.EncryptionDirection.Decrypting);
    }

    /// <summary>
    /// Creates a symmetric encryptor object with the specified 
    /// <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key"/> property and 
    /// initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV"/>).
    /// </summary>
    /// <param name="rgbKey">The secret key to use for the symmetric algorithm.</param>
    /// <param name="rgbIV">The initialization vector to use for the symmetric algorithm.</param>
    /// <returns>A symmetric encryptor object.</returns>
    public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
    {
      Key = rgbKey; // this appears to make a new copy

      if (Mode == CipherMode.CBC)
        IV = rgbIV;

      return new TwofishEncryption(KeySize, ref KeyValue, ref IVValue, ModeValue, 
        TWOFISH.EncryptionDirection.Encrypting);
    }

    /// <summary>
    /// Generates a random initialization vector 
    /// (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV"/>) to use for the algorithm.
    /// </summary>
    public override void GenerateIV()
    {
      Array.Clear(IV, 0, IV.Length);
    }

    /// <summary>
    /// Generates a random key 
    /// (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key"/>) to use for the algorithm.
    /// </summary>
    public override void GenerateKey()
    {
      Array.Clear(Key, 0, Key.Length);
    }


    /// <summary>
    /// Gets or sets the mode for operation of the symmetric algorithm.
    /// </summary>
    /// <value></value>
    /// <returns>
    /// The mode for operation of the symmetric algorithm. 
    /// The default is <see cref="F:System.Security.Cryptography.CipherMode.CBC"/>.
    /// </returns>
    /// <exception cref="T:System.Security.Cryptography.CryptographicException">
    /// The cipher mode is not one of the <see cref="T:System.Security.Cryptography.CipherMode"/> values.
    /// </exception>
    public override CipherMode Mode
    {
      set
      {
        switch (value)
        {
          case CipherMode.CBC:
          case CipherMode.ECB:
            break;

          default:
            throw new CryptographicException("CipherMode is not supported.");
        }
        ModeValue = value;
      }
    }
  }
}
