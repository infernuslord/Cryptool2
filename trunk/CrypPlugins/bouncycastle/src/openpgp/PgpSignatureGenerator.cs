using System;
using System.IO;

using Org.BouncyCastle.Bcpg.Sig;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Bcpg.OpenPgp
{
	/// <remarks>Generator for PGP signatures.</remarks>
	// TODO Should be able to implement ISigner?
    public class PgpSignatureGenerator
    {
		private static readonly SignatureSubpacket[] EmptySignatureSubpackets = new SignatureSubpacket[0];

		private PublicKeyAlgorithmTag	keyAlgorithm;
        private HashAlgorithmTag		hashAlgorithm;
        private PgpPrivateKey			privKey;
        private ISigner					sig;
        private IDigest					dig;
        private int						signatureType;
        private byte					lastb;

		private SignatureSubpacket[] unhashed = EmptySignatureSubpackets;
        private SignatureSubpacket[] hashed = EmptySignatureSubpackets;

		/// <summary>Create a generator for the passed in keyAlgorithm and hashAlgorithm codes.</summary>
        public PgpSignatureGenerator(
            PublicKeyAlgorithmTag	keyAlgorithm,
            HashAlgorithmTag		hashAlgorithm)
        {
            this.keyAlgorithm = keyAlgorithm;
            this.hashAlgorithm = hashAlgorithm;

			dig = DigestUtilities.GetDigest(PgpUtilities.GetDigestName(hashAlgorithm));
            sig = SignerUtilities.GetSigner(PgpUtilities.GetSignatureName(keyAlgorithm, hashAlgorithm));
        }

		/// <summary>Initialise the generator for signing.</summary>
        public void InitSign(
            int				sigType,
            PgpPrivateKey	key)
        {
            this.privKey = key;
            this.signatureType = sigType;

			try
            {
                sig.Init(true, key.Key);
            }
            catch (InvalidKeyException e)
            {
                throw new PgpException("invalid key.", e);
            }

			dig.Reset();
            lastb = 0;
        }

		public void Update(
            byte b)
        {
            if (signatureType == PgpSignature.CanonicalTextDocument)
            {
				doCanonicalUpdateByte(b);
            }
            else
            {
				doUpdateByte(b);
            }
        }

		private void doCanonicalUpdateByte(
			byte b)
		{
			if (b == '\r')
			{
				doUpdateCRLF();
			}
			else if (b == '\n')
			{
				if (lastb != '\r')
				{
					doUpdateCRLF();
				}
			}
			else
			{
				doUpdateByte(b);
			}

			lastb = b;
		}

		private void doUpdateCRLF()
		{
			doUpdateByte((byte)'\r');
			doUpdateByte((byte)'\n');
		}

		private void doUpdateByte(
			byte b)
		{
			sig.Update(b);
			dig.Update(b);
		}

		public void Update(
            byte[] b)
        {
            if (signatureType == PgpSignature.CanonicalTextDocument)
            {
                for (int i = 0; i != b.Length; i++)
                {
                    doCanonicalUpdateByte(b[i]);
                }
            }
            else
            {
                sig.BlockUpdate(b, 0, b.Length);
                dig.BlockUpdate(b, 0, b.Length);
            }
        }

		public void Update(
            byte[]	b,
            int		off,
            int		len)
        {
            if (signatureType == PgpSignature.CanonicalTextDocument)
            {
                int finish = off + len;

				for (int i = off; i != finish; i++)
                {
                    doCanonicalUpdateByte(b[i]);
                }
            }
            else
            {
                sig.BlockUpdate(b, off, len);
                dig.BlockUpdate(b, off, len);
            }
        }

		public void SetHashedSubpackets(
            PgpSignatureSubpacketVector hashedPackets)
        {
			hashed = hashedPackets == null
				?	EmptySignatureSubpackets
				:	hashedPackets.ToSubpacketArray();
        }

		public void SetUnhashedSubpackets(
            PgpSignatureSubpacketVector unhashedPackets)
        {
			unhashed = unhashedPackets == null
				?	EmptySignatureSubpackets
				:	unhashedPackets.ToSubpacketArray();
        }

		/// <summary>Return the one pass header associated with the current signature.</summary>
        public PgpOnePassSignature GenerateOnePassVersion(
            bool isNested)
        {
            return new PgpOnePassSignature(
				new OnePassSignaturePacket(
					signatureType, hashAlgorithm, keyAlgorithm, privKey.KeyId, isNested));
        }

		/// <summary>Return a signature object containing the current signature state.</summary>
        public PgpSignature Generate()
        {
            MPInteger[] sigValues;
            int version = 4;
            MemoryStream sOut = new MemoryStream();

//            int index = 0;
//
//			if (!creationTimeFound)
//            {
//                hashed[index++] = new SignatureCreationTime(false, DateTime.UtcNow);
//            }
//
//			if (!issuerKeyIDFound)
//            {
//                hashed[index++] = new IssuerKeyId(false, privKey.KeyId);
//            }
			SignatureSubpacket[] hPkts, unhPkts;

			if (!packetPresent(hashed, SignatureSubpacketTag.CreationTime))
			{
				hPkts = insertSubpacket(hashed, new SignatureCreationTime(false, DateTime.UtcNow));
			}
			else
			{
				hPkts = hashed;
			}

			if (!packetPresent(hashed, SignatureSubpacketTag.IssuerKeyId)
				&& !packetPresent(unhashed, SignatureSubpacketTag.IssuerKeyId))
			{
				unhPkts = insertSubpacket(unhashed, new IssuerKeyId(false, privKey.KeyId));
			}
			else
			{
				unhPkts = unhashed;
			}

			try
            {
                sOut.WriteByte((byte)version);
                sOut.WriteByte((byte)signatureType);
                sOut.WriteByte((byte)keyAlgorithm);
                sOut.WriteByte((byte)hashAlgorithm);

				MemoryStream hOut = new MemoryStream();

				for (int i = 0; i != hPkts.Length; i++)
				{
					hPkts[i].Encode(hOut);
                }

				byte[] data = hOut.ToArray();

				sOut.WriteByte((byte)(data.Length >> 8));
                sOut.WriteByte((byte)data.Length);
                sOut.Write(data, 0, data.Length);
            }
            catch (IOException e)
            {
                throw new PgpException("exception encoding hashed data.", e);
            }

			byte[] hData = sOut.ToArray();

			sig.BlockUpdate(hData, 0, hData.Length);
            dig.BlockUpdate(hData, 0, hData.Length);

			sOut = new MemoryStream();
            sOut.WriteByte((byte)version);
            sOut.WriteByte(0xff);
            sOut.WriteByte((byte)(hData.Length >> 24));
            sOut.WriteByte((byte)(hData.Length >> 16));
            sOut.WriteByte((byte)(hData.Length >> 8));
            sOut.WriteByte((byte)(hData.Length));

			hData = sOut.ToArray();

			sig.BlockUpdate(hData, 0, hData.Length);
            dig.BlockUpdate(hData, 0, hData.Length);

			byte[] sigBytes = sig.GenerateSignature();
			byte[] digest = DigestUtilities.DoFinal(dig);
			byte[] fingerPrint = new byte[] { digest[0], digest[1] };

			if (keyAlgorithm == PublicKeyAlgorithmTag.RsaSign
                || keyAlgorithm == PublicKeyAlgorithmTag.RsaGeneral)    // an RSA signature
            {
				sigValues = new MPInteger[]{ new MPInteger(new BigInteger(1, sigBytes)) };
            }
            else
            {
				sigValues = PgpUtilities.DsaSigToMpi(sigBytes);
            }

			return new PgpSignature(
				new SignaturePacket(signatureType, privKey.KeyId, keyAlgorithm,
					hashAlgorithm, hPkts, unhPkts, fingerPrint, sigValues));
        }

		/// <summary>Generate a certification for the passed in ID and key.</summary>
		/// <param name="id">The ID we are certifying against the public key.</param>
		/// <param name="pubKey">The key we are certifying against the ID.</param>
		/// <returns>The certification.</returns>
        public PgpSignature GenerateCertification(
            string			id,
            PgpPublicKey	pubKey)
        {
            byte[] keyBytes;
            try
            {
                keyBytes = pubKey.publicPk.GetEncodedContents();
            }
            catch (IOException e)
            {
                throw new PgpException("exception preparing key.", e);
            }

			// TODO Factor out this block with other similar ones below
			Update(0x99);
            Update((byte)(keyBytes.Length >> 8));
            Update((byte)(keyBytes.Length));
            Update(keyBytes);

			//
            // hash in the id
            //
            byte[] idBytes = new byte[id.Length];

			for (int i = 0; i != idBytes.Length; i++)
            {
                idBytes[i] = (byte)id[i];
            }

			Update(0xb4);
            Update((byte)(idBytes.Length >> 24));
            Update((byte)(idBytes.Length >> 16));
            Update((byte)(idBytes.Length >> 8));
            Update((byte)(idBytes.Length));
            Update(idBytes);

			return Generate();
        }

		/// <summary>Generate a certification for the passed in key against the passed in master key.</summary>
		/// <param name="masterKey">The key we are certifying against.</param>
		/// <param name="pubKey">The key we are certifying.</param>
		/// <returns>The certification.</returns>
        public PgpSignature GenerateCertification(
            PgpPublicKey	masterKey,
            PgpPublicKey	pubKey)
        {
            byte[] keyBytes;
            try
            {
                keyBytes = masterKey.publicPk.GetEncodedContents();
            }
            catch (IOException e)
            {
                throw new PgpException("exception preparing key.", e);
            }

			Update(0x99);
            Update((byte)(keyBytes.Length >> 8));
            Update((byte)(keyBytes.Length));
            Update(keyBytes);

			try
            {
                keyBytes = pubKey.publicPk.GetEncodedContents();
            }
            catch (IOException e)
            {
                throw new PgpException("exception preparing key.", e);
            }

			Update(0x99);
            Update((byte)(keyBytes.Length >> 8));
            Update((byte)(keyBytes.Length));
            Update(keyBytes);

			return Generate();
        }

		/// <summary>Generate a certification, such as a revocation, for the passed in key.</summary>
		/// <param name="pubKey">The key we are certifying.</param>
		/// <returns>The certification.</returns>
        public PgpSignature GenerateCertification(
            PgpPublicKey pubKey)
        {
            byte[] keyBytes;
            try
            {
                keyBytes = pubKey.publicPk.GetEncodedContents();
            }
            catch (IOException e)
            {
                throw new PgpException("exception preparing key.", e);
            }

			Update(0x99);
            Update((byte)(keyBytes.Length >> 8));
            Update((byte)(keyBytes.Length));
            Update(keyBytes);

			return Generate();
        }

		private bool packetPresent(
			SignatureSubpacket[]	packets,
			SignatureSubpacketTag	type)
		{
			for (int i = 0; i != packets.Length; i++)
			{
				if (packets[i].SubpacketType == type)
				{
					return true;
				}
			}

			return false;
		}

		private SignatureSubpacket[] insertSubpacket(
			SignatureSubpacket[]	packets,
			SignatureSubpacket		subpacket)
		{
			SignatureSubpacket[] tmp = new SignatureSubpacket[packets.Length + 1];
			tmp[0] = subpacket;
			packets.CopyTo(tmp, 1);
			return tmp;
		}
	}
}
