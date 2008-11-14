using System;
using System.Collections;
using System.IO;

using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Bcpg.OpenPgp.Examples
{
    /**
    * A simple utility class that encrypts/decrypts public key based
    * encryption files.
    * <p>
    * To encrypt a file: KeyBasedFileProcessor -e [-a|-ai] fileName publicKeyFile.<br/>
    * If -a is specified the output file will be "ascii-armored".
    * If -i is specified the output file will be have integrity checking added.</p>
    * <p>
    * To decrypt: KeyBasedFileProcessor -d fileName secretKeyFile passPhrase.</p>
    * <p>
    * Note: this example will silently overwrite files, nor does it pay any attention to
    * the specification of "_CONSOLE" in the filename. It also expects that a single pass phrase
    * will have been used.</p>
    */
    public sealed class KeyBasedFileProcessor
    {
        private KeyBasedFileProcessor()
        {
        }

        /**
        * A simple routine that opens a key ring file and loads the first available key suitable for
        * encryption.
        *
        * @param in
        * @return
        * @m_out
        * @
        */
        private static PgpPublicKey ReadPublicKey(
            Stream    inputStream)
        {
            inputStream = PgpUtilities.GetDecoderStream(inputStream);

            PgpPublicKeyRingBundle        pgpPub = new PgpPublicKeyRingBundle(inputStream);

            //
            // we just loop through the collection till we find a key suitable for encryption, in the real
            // world you would probably want to be a bit smarter about this.
            //

            //
            // iterate through the key rings.
            //

            foreach (PgpPublicKeyRing kRing in pgpPub.GetKeyRings())
            {
                foreach (PgpPublicKey k in kRing.GetPublicKeys())
                {
                    if (k.IsEncryptionKey)
                    {
                        return k;
                    }
                }
            }

            throw new ArgumentException("Can't find encryption key in key ring.");
        }

        /**
        * Load a secret key ring collection from keyIn and find the secret key corresponding to
        * keyId if it exists.
        *
        * @param keyIn input stream representing a key ring collection.
        * @param keyId keyId we want.
        * @param pass passphrase to decrypt secret key with.
        * @return
        */
        private static PgpPrivateKey FindSecretKey(
            Stream	keyIn,
            long	keyId,
            char[]	pass)
        {
            PgpSecretKeyRingBundle    pgpSec = new PgpSecretKeyRingBundle(
                PgpUtilities.GetDecoderStream(keyIn));

            PgpSecretKey pgpSecKey = pgpSec.GetSecretKey(keyId);

            if (pgpSecKey == null)
            {
                return null;
            }

            return pgpSecKey.ExtractPrivateKey(pass);
        }

        /**
        * decrypt the passed in message stream
        */
        private static void DecryptFile(
            Stream	inputStream,
            Stream	keyIn,
            char[]	passwd)

        {
            inputStream = PgpUtilities.GetDecoderStream(inputStream);

            try
            {
                PgpObjectFactory pgpF = new PgpObjectFactory(inputStream);
                PgpEncryptedDataList enc;

                PgpObject o = pgpF.NextPgpObject();
                //
                // the first object might be a PGP marker packet.
                //
                if (o is PgpEncryptedDataList)
                {
                    enc = (PgpEncryptedDataList)o;
                }
                else
                {
                    enc = (PgpEncryptedDataList)pgpF.NextPgpObject();
                }

                //
                // find the secret key
                //
                PgpPrivateKey               sKey = null;
                PgpPublicKeyEncryptedData   pbe = null;

                foreach (PgpPublicKeyEncryptedData pked in enc.GetEncryptedDataObjects())
                {
                    sKey = FindSecretKey(keyIn, pked.KeyId, passwd);

                    if (sKey != null)
                    {
                        pbe = pked;
                        break;
                    }
                }

				if (sKey == null)
                {
                    throw new ArgumentException("secret key for message not found.");
                }

                Stream clear = pbe.GetDataStream(sKey);

                PgpObjectFactory plainFact = new PgpObjectFactory(clear);

                PgpObject message = plainFact.NextPgpObject();

                if (message is PgpCompressedData)
                {
                    PgpCompressedData cData = (PgpCompressedData)message;
                    PgpObjectFactory pgpFact = new PgpObjectFactory(cData.GetDataStream());

                    message = pgpFact.NextPgpObject();
                }

                if (message is PgpLiteralData)
                {
                    PgpLiteralData ld = (PgpLiteralData)message;
                    Stream fOut = File.Create(ld.FileName);
					Stream unc = ld.GetInputStream();
					Streams.PipeAll(unc, fOut);
					fOut.Close();
                }
                else if (message is PgpOnePassSignatureList)
                {
                    throw new PgpException("encrypted message contains a signed message - not literal data.");
                }
                else
                {
                    throw new PgpException("message is not a simple encrypted file - type unknown.");
                }

                if (pbe.IsIntegrityProtected())
                {
                    if (!pbe.Verify())
                    {
                        Console.Error.WriteLine("message failed integrity check");
                    }
                    else
                    {
                        Console.Error.WriteLine("message integrity check passed");
                    }
                }
                else
                {
                    Console.Error.WriteLine("no message integrity check");
                }
            }
            catch (PgpException e)
            {
                Console.Error.WriteLine(e);

                Exception underlyingException = e.InnerException;
                if (underlyingException != null)
                {
                    Console.Error.WriteLine(underlyingException.Message);
                    Console.Error.WriteLine(underlyingException.StackTrace);
                }
            }
        }

        private static void EncryptFile(
            Stream			outputStream,
            string			fileName,
            PgpPublicKey	encKey,
            bool			armor,
            bool			withIntegrityCheck)
        {
            if (armor)
            {
                outputStream = new ArmoredOutputStream(outputStream);
            }

            try
            {
                MemoryStream bOut = new MemoryStream();

				PgpCompressedDataGenerator comData = new PgpCompressedDataGenerator(
                    CompressionAlgorithmTag.Zip);

				PgpUtilities.WriteFileToLiteralData(
					comData.Open(bOut),
					PgpLiteralData.Binary,
					new FileInfo(fileName));

				comData.Close();

				PgpEncryptedDataGenerator cPk = new PgpEncryptedDataGenerator(
					SymmetricKeyAlgorithmTag.Cast5, withIntegrityCheck, new SecureRandom());

				cPk.AddMethod(encKey);

				byte[] bytes = bOut.ToArray();

				Stream cOut = cPk.Open(outputStream, bytes.Length);

				cOut.Write(bytes, 0, bytes.Length);

				cOut.Close();

				if (armor)
				{
					outputStream.Close();
				}
            }
            catch (PgpException e)
            {
                Console.Error.WriteLine(e);

                Exception underlyingException = e.InnerException;
                if (underlyingException != null)
                {
                    Console.Error.WriteLine(underlyingException.Message);
                    Console.Error.WriteLine(underlyingException.StackTrace);
                }
            }
        }

        public static void Main(
            string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("usage: KeyBasedFileProcessor -e|-d [-a|ai] file [secretKeyFile passPhrase|pubKeyFile]");
                return;
            }

            if (args[0].Equals("-e"))
            {
				Stream keyIn, fos;
                if (args[1].Equals("-a") || args[1].Equals("-ai") || args[1].Equals("-ia"))
                {
                    keyIn = File.OpenRead(args[3]);
                    fos = File.Create(args[2] + ".asc");
                    EncryptFile(fos, args[2], ReadPublicKey(keyIn), true, (args[1].IndexOf('i') > 0));
                }
                else if (args[1].Equals("-i"))
                {
                    keyIn = File.OpenRead(args[3]);
                    fos = File.Create(args[2] + ".bpg");
                    EncryptFile(fos, args[2], ReadPublicKey(keyIn), false, true);
                }
                else
                {
                    keyIn = File.OpenRead(args[2]);
                    fos = File.Create(args[1] + ".bpg");
                    EncryptFile(fos, args[1], ReadPublicKey(keyIn), false, false);
                }
				keyIn.Close();
				fos.Close();
            }
            else if (args[0].Equals("-d"))
            {
                Stream fin = File.OpenRead(args[1]);
                Stream keyIn = File.OpenRead(args[2]);
                DecryptFile(fin, keyIn, args[3].ToCharArray());
				fin.Close();
				keyIn.Close();
            }
            else
            {
                Console.Error.WriteLine("usage: KeyBasedFileProcessor -d|-e [-a|ai] file [secretKeyFile passPhrase|pubKeyFile]");
            }
        }
    }
}
