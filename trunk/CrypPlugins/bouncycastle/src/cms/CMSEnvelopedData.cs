using System;
using System.Collections;
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Cms
{
    /**
    * containing class for an CMS Enveloped Data object
    */
    public class CmsEnvelopedData
    {
        internal RecipientInformationStore	recipientInfoStore;
        internal ContentInfo				contentInfo;

		private AlgorithmIdentifier	encAlg;
        private Asn1Set				unprotectedAttributes;
        private AlgorithmIdentifier	_encAlg;

		public CmsEnvelopedData(
            byte[] envelopedData)
            : this(CmsUtilities.ReadContentInfo(envelopedData))
        {
        }

        public CmsEnvelopedData(
            Stream envelopedData)
            : this(CmsUtilities.ReadContentInfo(envelopedData))
        {
        }

        public CmsEnvelopedData(
            ContentInfo contentInfo)
        {
            this.contentInfo = contentInfo;

			EnvelopedData envData = EnvelopedData.GetInstance(contentInfo.Content);

			//
            // read the encrypted content info
            //
            EncryptedContentInfo encInfo = envData.EncryptedContentInfo;

			this._encAlg = encInfo.ContentEncryptionAlgorithm;

			//
            // load the RecepientInfoStore
            //
            Asn1Set	s = envData.RecipientInfos;
			IList infos = new ArrayList();

			foreach (Asn1Encodable ae in s)
            {
                RecipientInfo info = RecipientInfo.GetInstance(ae);
				MemoryStream contentStream = new MemoryStream(encInfo.EncryptedContent.GetOctets(), false);

				object type = info.Info;

				if (type is KeyTransRecipientInfo)
				{
					infos.Add(new KeyTransRecipientInformation(
						(KeyTransRecipientInfo) type, _encAlg, contentStream));
				}
				else if (type is KekRecipientInfo)
				{
					infos.Add(new KekRecipientInformation(
						(KekRecipientInfo) type, _encAlg, contentStream));
				}
				else if (type is KeyAgreeRecipientInfo)
				{
					infos.Add(new KeyAgreeRecipientInformation(
						(KeyAgreeRecipientInfo) type, _encAlg, contentStream));
				}
				else if (type is PasswordRecipientInfo)
				{
					infos.Add(new PasswordRecipientInformation(
						(PasswordRecipientInfo) type, _encAlg, contentStream));
				}
            }

			this.encAlg = envData.EncryptedContentInfo.ContentEncryptionAlgorithm;
            this.recipientInfoStore = new RecipientInformationStore(infos);
            this.unprotectedAttributes = envData.UnprotectedAttrs;
        }

		public AlgorithmIdentifier EncryptionAlgorithmID
		{
			get { return encAlg; }
		}

		/**
        * return the object identifier for the content encryption algorithm.
        */
        public string EncryptionAlgOid
        {
			get { return encAlg.ObjectID.Id; }
        }

		/**
        * return a store of the intended recipients for this message
        */
        public RecipientInformationStore GetRecipientInfos()
        {
            return recipientInfoStore;
        }

		/**
        * return a table of the unprotected attributes indexed by
        * the OID of the attribute.
        */
        public Asn1.Cms.AttributeTable GetUnprotectedAttributes()
        {
            if (unprotectedAttributes == null)
                return null;

			return new Asn1.Cms.AttributeTable(unprotectedAttributes);
        }

		/**
        * return the ASN.1 encoded representation of this object.
        */
        public byte[] GetEncoded()
        {
			return contentInfo.GetEncoded();
        }
    }
}
