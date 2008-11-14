namespace Org.BouncyCastle.Asn1
{
    public class BerSequence
        : DerSequence
    {
        /**
         * create an empty sequence
         */
        public BerSequence()
        {
        }

		/**
         * create a sequence containing one object
         */
        public BerSequence(Asn1Encodable obj) : base(obj)
        {
        }

		public BerSequence(params Asn1Encodable[] v) : base(v)
		{
		}

		/**
         * create a sequence containing a vector of objects.
         */
        public BerSequence(Asn1EncodableVector v) : base(v)
        {
        }

		/*
         */
        internal override void Encode(
            DerOutputStream derOut)
        {
            if (derOut is Asn1OutputStream || derOut is BerOutputStream)
            {
                derOut.WriteByte(Asn1Tags.Sequence | Asn1Tags.Constructed);
                derOut.WriteByte(0x80);

				foreach (object o in this)
				{
                    derOut.WriteObject(o);
                }

				derOut.WriteByte(0x00);
                derOut.WriteByte(0x00);
            }
            else
            {
                base.Encode(derOut);
            }
        }
    }
}
