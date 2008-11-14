namespace Org.BouncyCastle.Asn1
{
    public class BerSet
        : DerSet
    {
        /**
         * create an empty sequence
         */
        public BerSet()
        {
        }

        /**
         * create a set containing one object
         */
        public BerSet(Asn1Encodable obj) : base(obj)
        {
        }

        /**
         * create a set containing a vector of objects.
         */
        public BerSet(Asn1EncodableVector v) : base(v, false)
        {
        }

        internal BerSet(Asn1EncodableVector v, bool needsSorting) : base(v, needsSorting)
        {
        }

        /*
         */
        internal override void Encode(
            DerOutputStream derOut)
        {
            if (derOut is Asn1OutputStream || derOut is BerOutputStream)
            {
                derOut.WriteByte(Asn1Tags.Set | Asn1Tags.Constructed);
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
