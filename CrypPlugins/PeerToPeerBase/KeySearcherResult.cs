using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeySearcher;

namespace Cryptool.Plugins.PeerToPeer
{
    public class KeySearcherResult
    {
        private Encoding enc;

        public KeySearcherResult()
        {
            this.enc = UTF8Encoding.UTF8;
        }

        public KeySearcherResult(Encoding encType)
        {
            this.enc = encType;
        }

        /* serialization information: 3 fields per data set in the following order: 
         *  -------------------------------------------------------------------------------------------------------------------------------
         * | Dataset count | double-value (8 byte) | string len (1 byte) | string-data (n bytes) | byte len (1 byte) | byte-data (n bytes) |
         *  ------------------------------------------------------------------------------------------------------------------------------- */
        public byte[] SerializeResult(LinkedList<KeySearcher.KeySearcher.ValueKey> resultList)
        {
            ByteQueue queueResult = new ByteQueue();
            //amount of list entries
            queueResult.Push((byte)resultList.Count);

            foreach (KeySearcher.KeySearcher.ValueKey valKey in resultList)
            {
                queueResult.Push(BitConverter.GetBytes(valKey.value));
                queueResult.Push((byte)valKey.key.Length);
                queueResult.Push(enc.GetBytes(valKey.key));
                // maybe this could run into an exception, when decryption byte is longer than 256 bytes!
                if (valKey.decryption.Length > 255)
                {
                    queueResult.Push((byte)255);
                    byte[] shortDecrypt = new byte[255];
                    Buffer.BlockCopy(valKey.decryption, 0, shortDecrypt, 0, 255);
                    queueResult.Push(shortDecrypt);
                }
                else
                {
                    queueResult.Push((byte)valKey.decryption.Length);
                    queueResult.Push(valKey.decryption);
                }
            }
            return queueResult.ToArray();
        }

        private const int VALUE_SIZE = 8;
        public LinkedList<KeySearcher.KeySearcher.ValueKey> DeserializeResult(byte[] serializedResult)
        {
            ByteQueue resultQueue = new ByteQueue(serializedResult);
            LinkedList<KeySearcher.KeySearcher.ValueKey> lstRet = new LinkedList<KeySearcher.KeySearcher.ValueKey>();

            KeySearcher.KeySearcher.ValueKey valKey = new KeySearcher.KeySearcher.ValueKey();
            int stringLen = 0;
            int byteLen = 0;
            int iDatasetCount = Convert.ToInt32(resultQueue.PopOneByte());
            for (int i = 0; i < iDatasetCount; i++) //run through every data set (first byte = dataset count)
            {
                // 8 byte = double value
                valKey.value = BitConverter.ToDouble(resultQueue.PopBytes(VALUE_SIZE), 0);
                // variable string length
                stringLen = Convert.ToInt32(resultQueue.PopOneByte());
                valKey.key = enc.GetString(resultQueue.PopBytes(stringLen));
                // variable byte length
                byteLen = Convert.ToInt32(resultQueue.PopOneByte());
                valKey.decryption = resultQueue.PopBytes(byteLen);

                lstRet.AddLast(valKey);
            }
            return lstRet;
        }

        private class ByteQueue
        {
            private Queue<byte> myQueue;

            public int CurrentStackSize { get { return myQueue.Count; } }

            public ByteQueue()
            {
                myQueue = new Queue<byte>();
            }

            public ByteQueue(byte[] data)
            {
                myQueue = new Queue<byte>();
                for (int i = 0; i < data.Length; i++)
                {
                    myQueue.Enqueue(data[i]);
                }
            }

            public void Push(byte data)
            {
                myQueue.Enqueue(data);
            }

            public void Push(byte[] data)
            {
                for (int i = 0; i < data.Length; i++)
			    {
                    myQueue.Enqueue(data[i]);
			    }
            }

            public byte PopOneByte()
            {
                byte ret = myQueue.Dequeue();
                return ret;
            }

            public byte[] PopBytes(int num)
            {
                byte[] ret = new byte[num];
                for (int i = 0; i < num; i++)
                {
                    ret[i] = myQueue.Dequeue();
                }
                return ret;
            }

            public byte[] ToArray()
            {
                return myQueue.ToArray();
            }
        }
    }
}
