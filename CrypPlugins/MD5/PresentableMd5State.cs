using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows;

namespace Cryptool.MD5
{
    public class PresentableMd5State
    {
        public Md5State State { get; set; }

        public const int DATA_BLOCK_SIZE = 64;

        public uint A { get; set; }
        public uint B { get; set; }
        public uint C { get; set; }
        public uint D { get; set; }

        public uint RoundIndex;
        public uint Round
        {
            get
            {
                return RoundIndex + 1;
            }
        }

        public uint RoundStepIndex;
        public uint RoundStep
        {
            get
            {
                return RoundStepIndex + 1;
            }
        }

        public uint BytesHashed { get; set; }

        public bool IsPaddingDone { get; set; }

        public bool IsLastStepInRound
        {
            get
            {
                return RoundStep == 16;
            }
        }

        public bool IsLastRound
        {
            get
            {
                return Round == 4;
            }
        }

        public uint H1 { get; set; }
        public uint H2 { get; set; }
        public uint H3 { get; set; }
        public uint H4 { get; set; }

        public uint[] X { get; set; }
        public byte[] Data { get; set; }
        public uint DataLength { get; set; }
        public uint DataOffset { get; set; }
        public ulong LengthInBit { get; set; }

        public PresentableMd5State()
        {
        }

        public PresentableMd5State(PresentableMd5State other)
        {
            foreach (FieldInfo fi in GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
                fi.SetValue(this, fi.GetValue(other));


            foreach (PropertyInfo pi in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                if (pi.CanWrite && pi.CanRead)
                    pi.SetValue(this, pi.GetValue(other, null), null);
        }
    }
}
