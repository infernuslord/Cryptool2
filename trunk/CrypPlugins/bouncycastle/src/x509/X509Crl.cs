using System;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Utilities.Date;
using Org.BouncyCastle.X509.Extension;

namespace Org.BouncyCastle.X509
{
	/**
	 * The following extensions are listed in RFC 2459 as relevant to CRLs
	 *
	 * Authority Key Identifier
	 * Issuer Alternative Name
	 * CRL Number
	 * Delta CRL Indicator (critical)
	 * Issuing Distribution Point (critical)
	 */
	public class X509Crl
		: X509ExtensionBase
		// TODO Add interface Crl?
	{
		private readonly CertificateList c;
		private readonly string sigAlgName;
		private readonly byte[] sigAlgParams;
		private readonly bool isIndirect;

		public X509Crl(
			CertificateList c)
		{
			this.c = c;

			try
			{
				this.sigAlgName = X509SignatureUtilities.GetSignatureName(c.SignatureAlgorithm);

				if (c.SignatureAlgorithm.Parameters != null)
				{
					this.sigAlgParams = ((Asn1Encodable)c.SignatureAlgorithm.Parameters).GetDerEncoded();
				}
				else
				{
					this.sigAlgParams = null;
				}

				this.isIndirect = IsIndirectCrl;
			}
			catch (Exception e)
			{
				throw new CrlException("CRL contents invalid: " + e);
			}
		}

		protected override X509Extensions GetX509Extensions()
		{
			return Version == 2
				?	c.TbsCertList.Extensions
				:	null;
		}

		public virtual byte[] GetEncoded()
		{
			try
			{
				return c.GetDerEncoded();
			}
			catch (Exception e)
			{
				throw new CrlException(e.ToString());
			}
		}

		public virtual void Verify(
			AsymmetricKeyParameter publicKey)
		{
			if (!c.SignatureAlgorithm.Equals(c.TbsCertList.Signature))
			{
				throw new CrlException("Signature algorithm on CertificateList does not match TbsCertList.");
			}

			ISigner sig = SignerUtilities.GetSigner(SigAlgName);
			sig.Init(false, publicKey);

			byte[] encoded = this.GetTbsCertList();
			sig.BlockUpdate(encoded, 0, encoded.Length);

			if (!sig.VerifySignature(this.GetSignature()))
			{
				throw new SignatureException("CRL does not verify with supplied public key.");
			}
		}

		public virtual int Version
		{
			get { return c.Version; }
		}

		public virtual X509Name IssuerDN
		{
			get { return c.Issuer; }
		}

		public virtual DateTime ThisUpdate
		{
			get { return c.ThisUpdate.ToDateTime(); }
		}

		public virtual DateTimeObject NextUpdate
		{
			get
			{
				return c.NextUpdate == null
					?	null
					:	new DateTimeObject(c.NextUpdate.ToDateTime());
			}
		}

		public virtual X509CrlEntry GetRevokedCertificate(
			BigInteger serialNumber)
		{
			CrlEntry[] certs = c.GetRevokedCertificates();

			if (certs != null)
			{
				X509Name previousCertificateIssuer = IssuerDN;
				for (int i = 0; i < certs.Length; i++)
				{
					X509CrlEntry crlEntry = new X509CrlEntry(
						certs[i], isIndirect, previousCertificateIssuer);
					previousCertificateIssuer = crlEntry.GetCertificateIssuer();
					if (crlEntry.SerialNumber.Equals(serialNumber))
					{
						return crlEntry;
					}
				}
			}

			return null;
		}

		public virtual ISet GetRevokedCertificates()
		{
			CrlEntry[] certs = c.GetRevokedCertificates();

			if (certs != null)
			{
				ISet s = new HashSet();
				X509Name previousCertificateIssuer = IssuerDN;
				for (int i = 0; i < certs.Length; i++)
				{
					X509CrlEntry crlEntry = new X509CrlEntry(
						certs[i], isIndirect, previousCertificateIssuer);
					s.Add(crlEntry);
					previousCertificateIssuer = crlEntry.GetCertificateIssuer();
				}

				return s;
			}

			return null;
		}

		public virtual byte[] GetTbsCertList()
		{
			try
			{
				return c.TbsCertList.GetDerEncoded();
			}
			catch (Exception e)
			{
				throw new CrlException(e.ToString());
			}
		}

		public virtual byte[] GetSignature()
		{
			return c.Signature.GetBytes();
		}

		public virtual string SigAlgName
		{
			get { return sigAlgName; }
		}

		public virtual string SigAlgOid
		{
			get { return c.SignatureAlgorithm.ObjectID.Id; }
		}

		public virtual byte[] GetSigAlgParams()
		{
			return Arrays.Clone(sigAlgParams);
		}

		public override bool Equals(
			object obj)
		{
			if (obj == this)
				return true;

			X509Crl other = obj as X509Crl;

			if (other == null)
				return false;

			return c.Equals(other.c);

			// NB: May prefer this implementation of Equals if more than one certificate implementation in play
			//return Arrays.AreEqual(this.GetEncoded(), other.GetEncoded());
		}

		public override int GetHashCode()
		{
			return c.GetHashCode();
		}

		/**
		 * Returns a string representation of this CRL.
		 *
		 * @return a string representation of this CRL.
		 */
		public override string ToString()
		{
			return "X.509 CRL";
		}

		/**
		 * Checks whether the given certificate is on this CRL.
		 *
		 * @param cert the certificate to check for.
		 * @return true if the given certificate is on this CRL,
		 * false otherwise.
		 */
//		public bool IsRevoked(
//			Certificate cert)
//		{
//			if (!cert.getType().Equals("X.509"))
//			{
//				throw new RuntimeException("X.509 CRL used with non X.509 Cert");
//			}
		public virtual bool IsRevoked(
			X509Certificate cert)
		{
			CrlEntry[] certs = c.GetRevokedCertificates();

			if (certs != null)
			{
//				BigInteger serial = ((X509Certificate)cert).SerialNumber;
				BigInteger serial = cert.SerialNumber;

				for (int i = 0; i < certs.Length; i++)
				{
					if (certs[i].UserCertificate.Value.Equals(serial))
					{
						return true;
					}
				}
			}

			return false;
		}

		protected virtual bool IsIndirectCrl
		{
			get
			{
				Asn1OctetString idp = GetExtensionValue(X509Extensions.IssuingDistributionPoint);
				bool isIndirect = false;

				try
				{
					if (idp != null)
					{
						isIndirect = IssuingDistributionPoint.GetInstance(
							X509ExtensionUtilities.FromExtensionValue(idp)).IsIndirectCrl;
					}
				}
				catch (Exception e)
				{
					// TODO
//					throw new ExtCrlException("Exception reading IssuingDistributionPoint", e);
					throw new CrlException("Exception reading IssuingDistributionPoint" + e);
				}

				return isIndirect;
			}
		}
	}
}
