using PeersAtPlay.P2POverlay;

namespace Cryptool.Plugins.PeerToPeer.Internal
{
    public class PeerId
    {
        private const uint OFFSET_BASIS = 2166136261;
        private const uint PRIME = 16777619; // 2^24 + 2^8 + 0x93
        private readonly byte[] byteId;
        private readonly int hashCode;
        private readonly string stringId;

        public PeerId(OverlayAddress oAddress)
        {
            if (oAddress != null)
            {
                stringId = oAddress.ToString();
                byteId = oAddress.ToByteArray();

                // FNV-1 hashing
                uint fnvHash = OFFSET_BASIS;
                foreach (byte b in byteId)
                {
                    fnvHash = (fnvHash*PRIME) ^ b;
                }
                hashCode = (int) fnvHash;
            }
        }

        /// <summary>
        /// Returns true when the byteId content is identical
        /// </summary>
        /// <param name="right"></param>
        /// <returns></returns>
        public override bool Equals(object right)
        {
            /* standard checks for reference types */

            if (right == null)
                return false;

            if (ReferenceEquals(this, right))
                return true;

            if (GetType() != right.GetType())
                return false;

            // actual content comparison
            return this == (PeerId) right;
        }

        public static bool operator ==(PeerId left, PeerId right)
        {
            // because it's possible that one parameter is null, catch this exception
            /* Begin add - Christian Arnold, 2009.12.16, must cast the parameters, otherwise --> recursion */
            if (left == (object) right)
                return true;

            if ((object) left == null || (object) right == null)
                return false;
            /* End add */

            if (left.hashCode != right.hashCode)
                return false;

            if (left.byteId.Length != right.byteId.Length)
                return false;

            for (int i = 0; i < left.byteId.Length; i++)
            {
                // uneven pattern content
                if (left.byteId[i] != right.byteId[i])
                    return false;
            }

            return true;
        }

        public static bool operator !=(PeerId left, PeerId right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public override string ToString()
        {
            return stringId;
        }

        public byte[] ToByteArray()
        {
            return byteId;
        }
    }
}