using System;
using System.Collections;
using System.IO;
using System.Text;

using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Bcpg.OpenPgp.Examples
{
    /**
    * Simple routine to encrypt and decrypt using a passphrase.
    * This service routine provides the basic PGP services between
    * byte arrays.
    *
    * Note: this code plays no attention to -Console in the file name
    * the specification of "_CONSOLE" in the filename.
    * It also expects that a single pass phrase will have been used.
    *
    */
    public sealed class ByteArrayHandler
    {
        private ByteArrayHandler()
        {
        }

        /**
        * decrypt the passed in message stream
        *
        * @param encrypted  The message to be decrypted.
        * @param passPhrase Pass phrase (key)
        *
        * @return Clear text as a byte array.  I18N considerations are
        *         not handled by this routine
        * @exception IOException
        * @exception PgpException
        */
        public static byte[] Decrypt(
            byte[] encrypted,
            char[] passPhrase)
        {
            Stream inputStream = new MemoryStream(encrypted);

            inputStream = PgpUtilities.GetDecoderStream(inputStream);

            PgpObjectFactory pgpF = new PgpObjectFactory(inputStream);
            PgpEncryptedDataList enc = null;
            PgpObject o = pgpF.NextPgpObject();

			//
            // the first object might be a PGP marker packet.
            //
            if (o is PgpEncryptedDataList)
            {
                enc = (PgpEncryptedDataList) o;
            }
            else
            {
                enc = (PgpEncryptedDataList) pgpF.NextPgpObject();
            }

            PgpPbeEncryptedData pbe = (PgpPbeEncryptedData)enc[0];

            Stream clear = pbe.GetDataStream(passPhrase);

            PgpObjectFactory pgpFact = new PgpObjectFactory(clear);

            PgpCompressedData cData = (PgpCompressedData) pgpFact.NextPgpObject();

            pgpFact = new PgpObjectFactory(cData.GetDataStream());

            PgpLiteralData ld = (PgpLiteralData) pgpFact.NextPgpObject();

            Stream unc = ld.GetInputStream();

			return Streams.ReadAll(unc);
        }

        /**
        * Simple PGP encryptor between byte[].
        *
        * @param clearData  The test to be encrypted
        * @param passPhrase The pass phrase (key).  This method assumes that the
        *                   key is a simple pass phrase, and does not yet support
        *                   RSA or more sophisiticated keying.
        * @param fileName   File name. This is used in the Literal Data Packet (tag 11)
        *                   which is really inly important if the data is to be
        *                   related to a file to be recovered later.  Because this
        *                   routine does not know the source of the information, the
        *                   caller can set something here for file name use that
        *                   will be carried.  If this routine is being used to
        *                   encrypt SOAP MIME bodies, for example, use the file name from the
        *                   MIME type, if applicable. Or anything else appropriate.
        *
        * @param armor
        *
        * @return encrypted data.
        * @exception IOException
        * @exception PgpException
        */
        public static byte[] Encrypt(
            byte[]     clearData,
            char[]         passPhrase,
            string         fileName,
            SymmetricKeyAlgorithmTag algorithm,
            bool     armor)
        {
            if (fileName == null)
            {
                fileName = PgpLiteralData.Console;
            }

            MemoryStream bOut = new MemoryStream();

            PgpCompressedDataGenerator comData = new PgpCompressedDataGenerator(
				CompressionAlgorithmTag.Zip);
            Stream cos = comData.Open(bOut); // open it with the final destination
            PgpLiteralDataGenerator lData = new PgpLiteralDataGenerator();

            // we want to Generate compressed data. This might be a user option later,
            // in which case we would pass in bOut.
            Stream pOut = lData.Open(
				cos,					// the compressed output stream
                PgpLiteralData.Binary,
                fileName,				// "filename" to store
                clearData.Length,		// length of clear data
                DateTime.UtcNow			// current time
            );

			pOut.Write(clearData, 0, clearData.Length);

			lData.Close();
            comData.Close();

            PgpEncryptedDataGenerator   cPk = new PgpEncryptedDataGenerator(algorithm, new SecureRandom());

            cPk.AddMethod(passPhrase);

            byte[] bytes = bOut.ToArray();

			MemoryStream encOut = new MemoryStream();
			Stream os = encOut;
			if (armor)
			{
				os = new ArmoredOutputStream(os);
			}

			Stream cOut = cPk.Open(os, bytes.Length);
            cOut.Write(bytes, 0, bytes.Length);  // obtain the actual bytes from the compressed stream
            cOut.Close();

			if (armor)
			{
				os.Close();
			}

			encOut.Close();

			return encOut.ToArray();
        }

        public static void Main(
			string[] args)
        {
            string passPhrase = "Dick Beck";
            char[] passArray = passPhrase.ToCharArray();

            byte[] original = Encoding.ASCII.GetBytes("Hello world");
            Console.WriteLine("Starting PGP test");
            byte[] encrypted = Encrypt(original, passArray, "iway", SymmetricKeyAlgorithmTag.Cast5, true);

            Console.WriteLine("\nencrypted data = '"+Encoding.ASCII.GetString(encrypted)+"'");
            byte[] decrypted= Decrypt(encrypted,passArray);

            Console.WriteLine("\ndecrypted data = '"+Encoding.ASCII.GetString(decrypted)+"'");

            encrypted = Encrypt(original, passArray, "iway", SymmetricKeyAlgorithmTag.Aes256, false);

            Console.WriteLine("\nencrypted data = '"+Encoding.ASCII.GetString(Org.BouncyCastle.Utilities.Encoders.Hex.Encode(encrypted))+"'");
            decrypted= Decrypt(encrypted, passArray);

            Console.WriteLine("\ndecrypted data = '"+Encoding.ASCII.GetString(decrypted)+"'");
        }
    }
}
