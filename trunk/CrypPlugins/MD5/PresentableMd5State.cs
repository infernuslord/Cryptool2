using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Cryptool.MD5
{
    public class PresentableMd5State
    {
        internal enum Md5State
        {
            INITIALIZED,
            READING_DATA, READ_DATA,
            STARTING_PADDING, ADDING_PADDING_BYTES, ADDED_PADDING_BYTES, ADDING_LENGTH, ADDED_LENGTH, FINISHED_PADDING,
            STARTING_COMPRESSION, STARTING_ROUND, STARTING_ROUND_STEP, FINISHED_ROUND_STEP, FINISHED_ROUND, FINISHING_COMPRESSION, FINISHED_COMPRESSION,
            FINISHED
        }

        internal const int DATA_BLOCK_SIZE = 64;

        internal uint A;
        internal uint B;
        internal uint C;
        internal uint D;

        internal uint _RoundIndex;
        internal uint Round
        {
            get
            {
                return _RoundIndex + 1;
            }
        }

        internal uint _RoundStepIndex;
        internal uint RoundStep
        {
            get
            {
                return _RoundStepIndex + 1;
            }
        }

        internal uint BytesHashed;

        internal bool IsPaddingDone;

        internal bool IsLastStepInRound
        {
            get
            {
                return RoundStep == 16;
            }
        }

        internal bool IsLastRound
        {
            get
            {
                return Round == 4;
            }
        }

        internal uint H1;
        internal uint H2;
        internal uint H3;
        internal uint H4;

        internal uint[] X;
        internal byte[] Data;
        internal uint DataLength;
        internal uint DataOffset;
        internal ulong LengthInBit;

        internal Md5State State;

        internal PresentableMd5State()
        {
        }

        internal PresentableMd5State(PresentableMd5State other)
        {
            foreach (FieldInfo fi in GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
                fi.SetValue(this, fi.GetValue(other));
            H1 = other.H1;
        }
    }
}
