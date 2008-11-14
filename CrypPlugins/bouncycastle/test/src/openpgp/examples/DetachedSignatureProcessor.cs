using System;
using System.Collections;
using System.IO;


using Org.BouncyCastle.Bcpg.OpenPgp;

namespace Org.BouncyCastle.Bcpg.OpenPgp.Examples
{
    /**
    * A simple utility class that creates seperate signatures for files and verifies them.
    * <p>
    * To sign a file: DetachedSignatureProcessor -s [-a] fileName secretKey passPhrase.<br/>
    * If -a is specified the output file will be "ascii-armored".</p>
    * <p>
    * To decrypt: DetachedSignatureProcessor -v  fileName signatureFile publicKeyFile.</p>
    * <p>
    * Note: this example will silently overwrite files.
    * It also expects that a single pass phrase
    * will have been used.</p>
    */
    public sealed class DetachedSignatureProcessor
    {
        private DetachedSignatureProcessor()
        {
        }

        /**
         * A simple routine that opens a key ring file and loads the first available key suitable for
         * signature generation.
         *
         * @param in
         * @return
         * @m_out
         * @
         */
        private static PgpSecretKey ReadSecretKey(
            Stream    inputStream)
        {
            inputStream = PgpUtilities.GetDecoderStream(inputStream);

            PgpSecretKeyRingBundle        pgpSec = new PgpSecretKeyRingBundle(inputStream);
            //
            // we just loop through the collection till we find a key suitable for encryption, in the real
            // world you would probably want to be a bit smarter about this.
            //

            //
            // iterate through the key rings.
            //
            foreach (PgpSecretKeyRing kRing in pgpSec.GetKeyRings())
            {
                foreach (PgpSecretKey k in kRing.GetSecretKeys())
                {
                    if (k.IsSigningKey)
                    {
                        return k;
                    }
                }
            }
            throw new ArgumentException("Can't find encryption key in key ring.");
        }

        /**
        * verify the signature in in against the file fileName.
        */
        private static void VerifySignature(
            string	fileName,
            Stream	inputStream,
            Stream	keyIn)
        {
            inputStream = PgpUtilities.GetDecoderStream(inputStream);

            PgpObjectFactory pgpFact = new PgpObjectFactory(inputStream);
            PgpSignatureList p3 = null;
            PgpObject o = pgpFact.NextPgpObject();
            if (o is PgpCompressedData)
            {
                PgpCompressedData c1 = (PgpCompressedData)o;
                pgpFact = new PgpObjectFactory(c1.GetDataStream());

                p3 = (PgpSignatureList)pgpFact.NextPgpObject();
            }
            else
            {
                p3 = (PgpSignatureList)o;
            }

            PgpPublicKeyRingBundle pgpPubRingCollection = new PgpPublicKeyRingBundle(
				PgpUtilities.GetDecoderStream(keyIn));
            Stream dIn = File.OpenRead(fileName);
            int ch;
            PgpSignature sig = p3[0];
            PgpPublicKey key = pgpPubRingCollection.GetPublicKey(sig.KeyId);
            sig.InitVerify(key);
            while ((ch = dIn.ReadByte()) >= 0)
            {
                sig.Update((byte)ch);
            }
			dIn.Close();
            if (sig.Verify())
            {
                Console.WriteLine("signature verified.");
            }
            else
            {
                Console.WriteLine("signature verification failed.");
            }
        }

        private static void CreateSignature(
            string	fileName,
            Stream	keyIn,
            Stream	outputStream,
            char[]	pass,
            bool	armor)
        {
            if (armor)
            {
                outputStream = new ArmoredOutputStream(outputStream);
            }

            PgpSecretKey pgpSec = ReadSecretKey(keyIn);
            PgpPrivateKey pgpPrivKey = pgpSec.ExtractPrivateKey(pass);
            PgpSignatureGenerator sGen = new PgpSignatureGenerator(
				pgpSec.PublicKey.Algorithm, HashAlgorithmTag.Sha1);

			sGen.InitSign(PgpSignature.BinaryDocument, pgpPrivKey);

			BcpgOutputStream bOut = new BcpgOutputStream(outputStream);

			Stream fIn = File.OpenRead(fileName);
            int ch;
            while ((ch = fIn.ReadByte()) >= 0)
            {
                sGen.Update((byte)ch);
            }
			fIn.Close();

			sGen.Generate().Encode(bOut);

			if (armor)
			{
				outputStream.Close();
			}
        }

		public static void Main(
            string[] args)
        {
            if (args[0].Equals("-s"))
            {
				Stream keyIn, fos;
                if (args[1].Equals("-a"))
                {
                    keyIn = File.OpenRead(args[3]);
                    fos = File.Create(args[2] + ".asc");

					CreateSignature(args[2], keyIn, fos, args[4].ToCharArray(), true);
                }
                else
                {
                    keyIn = File.OpenRead(args[2]);
                    fos = File.Create(args[1] + ".bpg");

                    CreateSignature(args[1], keyIn, fos, args[3].ToCharArray(), false);
                }
				keyIn.Close();
				fos.Close();
            }
            else if (args[0].Equals("-v"))
            {
                Stream fin = File.OpenRead(args[2]);
                Stream keyIn = File.OpenRead(args[3]);

                VerifySignature(args[1], fin, keyIn);

				fin.Close();
				keyIn.Close();
			}
            else
            {
                Console.Error.WriteLine("usage: DetachedSignatureProcessor [-s [-a] file keyfile passPhrase]|[-v file sigFile keyFile]");
            }
        }
    }
}
