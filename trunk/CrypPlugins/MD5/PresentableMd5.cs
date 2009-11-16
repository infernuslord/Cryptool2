using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Cryptool.MD5
{
    public class PresentableMd5
    {
        public List<PresentableMd5State> StateHistory { get; set; }

        public PresentableMd5State CurrentState { get; set; }

        public int CurrentStateNumber { get; protected set; }

        protected Stream DataStream { get; set; }

        public bool IsInitialized { get; protected set; }

        protected static readonly uint[] AdditionConstantTable = new uint[64] 
			{	0xd76aa478,0xe8c7b756,0x242070db,0xc1bdceee,
				0xf57c0faf,0x4787c62a,0xa8304613,0xfd469501,
                0x698098d8,0x8b44f7af,0xffff5bb1,0x895cd7be,
                0x6b901122,0xfd987193,0xa679438e,0x49b40821,
				0xf61e2562,0xc040b340,0x265e5a51,0xe9b6c7aa,
                0xd62f105d,0x2441453,0xd8a1e681,0xe7d3fbc8,
                0x21e1cde6,0xc33707d6,0xf4d50d87,0x455a14ed,
				0xa9e3e905,0xfcefa3f8,0x676f02d9,0x8d2a4c8a,
                0xfffa3942,0x8771f681,0x6d9d6122,0xfde5380c,
                0xa4beea44,0x4bdecfa9,0xf6bb4b60,0xbebfbc70,
                0x289b7ec6,0xeaa127fa,0xd4ef3085,0x4881d05,
				0xd9d4d039,0xe6db99e5,0x1fa27cf8,0xc4ac5665,
                0xf4292244,0x432aff97,0xab9423a7,0xfc93a039,
                0x655b59c3,0x8f0ccc92,0xffeff47d,0x85845dd1,
                0x6fa87e4f,0xfe2ce6e0,0xa3014314,0x4e0811a1,
				0xf7537e82,0xbd3af235,0x2ad7d2bb,0xeb86d391     };

        protected static readonly ushort[] ShiftConstantTable = new ushort[64] 
			{	7, 12, 17, 22, 7, 12, 17, 22, 7, 12, 17, 22, 7, 12, 17, 22,
				5,  9, 14, 20, 5,  9, 14, 20, 5,  9, 14, 20, 5,  9, 14, 20,
                4, 11, 16, 23, 4, 11, 16, 23, 4, 11, 16, 23, 4, 11, 16, 23,
                6, 10, 15, 21, 6, 10, 15, 21, 6, 10, 15, 21, 6, 10, 15, 21      };

        protected const int DATA_BLOCK_SIZE = 64;

        public PresentableMd5State LastState
        {
            get
            {
                if (CurrentStateNumber == 0)
                    return null;
                else
                    return StateHistory[CurrentStateNumber - 1];
            }
        }

        public PresentableMd5(Stream dataStream)
        {
            Initialize(dataStream);
        }

        public PresentableMd5()
        {
        }

        public void Initialize(Stream dataStream)
        {
            StateHistory = new List<PresentableMd5State>();

            DataStream = dataStream;

            InitFirstState();

            IsInitialized = true;
        }

        protected void AddNewState()
        {
            if (CurrentStateNumber == -1)
                CurrentState = new PresentableMd5State();
            else
                CurrentState = new PresentableMd5State(StateHistory[CurrentStateNumber]);

            StateHistory.Add(CurrentState);
            CurrentStateNumber = StateHistory.Count - 1;
        }

        public bool HistoryHasMoreStates
        {
            get
            {
                if (!IsInitialized)
                    return false;
                else
                    return StateHistory.Count - 1 > CurrentStateNumber;
            }
        }

        public void PreviousStep()
        {
            if (CurrentStateNumber == 0)
                return;

            CurrentStateNumber--;
            CurrentState = StateHistory[CurrentStateNumber];
        }

        public bool IsInFinishedState
        {
            get
            {
                return CurrentState.State == PresentableMd5State.Md5State.FINISHED;
            }
        }

        public void NextStep()
        {
            if (HistoryHasMoreStates)
            {
                CurrentStateNumber++;
                CurrentState = StateHistory[CurrentStateNumber];
            }
            else
            {
                if (IsInFinishedState)
                    return;

                PresentableMd5State previousState = CurrentState;
                AddNewState();
                PerformStep(previousState, CurrentState);
            }
        }

        public void NextStepUntilFinished()
        {
            while (!IsInFinishedState)
                NextStep();
        }

        public void NextStepUntilRoundEnd()
        {
            while (!IsInFinishedState && CurrentState.State != PresentableMd5State.Md5State.FINISHED_ROUND)
                NextStep();
        }

        public void NextStepUntilBlockEnd()
        {
            while (!IsInFinishedState && CurrentState.State != PresentableMd5State.Md5State.FINISHED_COMPRESSION)
                NextStep();
        }

        public void PerformStep(PresentableMd5State previousState, PresentableMd5State newState)
        {
            switch (previousState.State)
            {
                case PresentableMd5State.Md5State.INITIALIZED:
                    // If initialization is complete, start by reading data
                    newState.State = PresentableMd5State.Md5State.READING_DATA;
                    break;

                case PresentableMd5State.Md5State.READING_DATA:
                    // Read data and enter "data read" state
                    ReadData(newState);
                    newState.State = PresentableMd5State.Md5State.READ_DATA;
                    break;

                case PresentableMd5State.Md5State.READ_DATA:
                    // If an underfull buffer was read, we're at the end of the digestible data, so enter "starting padding" state
                    // If a full buffer was read, enter "starting compression" state
                    if (previousState.DataLength < DATA_BLOCK_SIZE)
                        newState.State = PresentableMd5State.Md5State.STARTING_PADDING;
                    else
                        newState.State = PresentableMd5State.Md5State.STARTING_COMPRESSION;
                    break;

                case PresentableMd5State.Md5State.STARTING_PADDING:
                    // First step of padding is adding the padding bytes, so enter that state
                    newState.State = PresentableMd5State.Md5State.ADDING_PADDING_BYTES;
                    break;

                case PresentableMd5State.Md5State.ADDING_PADDING_BYTES:
                    // Add necessary number of bytes and enter "added padding bytes" state
                    AddPaddingBytes(newState);
                    newState.State = PresentableMd5State.Md5State.ADDED_PADDING_BYTES;
                    break;

                case PresentableMd5State.Md5State.ADDED_PADDING_BYTES:
                    // The next step for padding is adding the data length, so enter that state
                    newState.State = PresentableMd5State.Md5State.ADDING_LENGTH;
                    break;

                case PresentableMd5State.Md5State.ADDING_LENGTH:
                    // Add the length of the data and enter "added length" state
                    AddLength(newState);
                    newState.State = PresentableMd5State.Md5State.ADDED_LENGTH;
                    break;

                case PresentableMd5State.Md5State.ADDED_LENGTH:
                    // Padding is done after adding data length, so enter "finished padding" state
                    newState.State = PresentableMd5State.Md5State.FINISHED_PADDING;
                    break;

                case PresentableMd5State.Md5State.FINISHED_PADDING:
                    // If padding is finished, call compression function for the last (two) time(s)
                    newState.State = PresentableMd5State.Md5State.STARTING_COMPRESSION;
                    break;

                case PresentableMd5State.Md5State.STARTING_COMPRESSION:
                    StartCompression(newState);
                    newState.State = PresentableMd5State.Md5State.STARTING_ROUND;
                    break;

                case PresentableMd5State.Md5State.STARTING_ROUND:
                    StartRound(newState);
                    newState.State = PresentableMd5State.Md5State.STARTING_ROUND_STEP;
                    break;

                case PresentableMd5State.Md5State.STARTING_ROUND_STEP:
                    PerformRoundStep(newState);
                    newState.State = PresentableMd5State.Md5State.FINISHED_ROUND_STEP;
                    break;

                case PresentableMd5State.Md5State.FINISHED_ROUND_STEP:
                    if (previousState.IsLastStepInRound)
                        newState.State = PresentableMd5State.Md5State.FINISHED_ROUND;
                    else
                    {
                        newState._RoundStepIndex++;
                        newState.State = PresentableMd5State.Md5State.STARTING_ROUND_STEP;
                    }
                    break;

                case PresentableMd5State.Md5State.FINISHED_ROUND:
                    if (previousState.IsLastRound)
                        newState.State = PresentableMd5State.Md5State.FINISHING_COMPRESSION;
                    else
                    {
                        newState._RoundIndex++;
                        newState.State = PresentableMd5State.Md5State.STARTING_ROUND;
                    }
                    break;

                case PresentableMd5State.Md5State.FINISHING_COMPRESSION:
                    FinishCompression(newState);
                    newState.State = PresentableMd5State.Md5State.FINISHED_COMPRESSION;
                    break;

                case PresentableMd5State.Md5State.FINISHED_COMPRESSION:
                    // If compression is finished, check if there's more data left in buffer. If so, reenter compression function with offset
                    if (previousState.DataLength - previousState.DataOffset > DATA_BLOCK_SIZE)
                    {
                        // Still some data left in buffer, rerun compression with offset
                        newState.DataOffset += DATA_BLOCK_SIZE;
                        newState.State = PresentableMd5State.Md5State.STARTING_COMPRESSION;
                    }
                    else
                    {
                        // No data left in buffer

                        if (previousState.IsPaddingDone)
                        {
                            // If padding was already added, we're done
                            newState.State = PresentableMd5State.Md5State.FINISHED;
                        }
                        else
                        {
                            // Read more data
                            newState.State = PresentableMd5State.Md5State.READING_DATA;
                        }
                    }
                    break;
            }
        }

        private void FinishCompression(PresentableMd5State newState)
        {
            newState.H1 += newState.A;
            newState.H2 += newState.B;
            newState.H3 += newState.C;
            newState.H4 += newState.D;

            newState.BytesHashed += DATA_BLOCK_SIZE;
        }

        private void ReadData(PresentableMd5State newState)
        {
            newState.Data = new byte[128];
            newState.DataLength = (uint)DataStream.Read(newState.Data, 0, 64);
            newState.DataOffset = 0;
        }

        private void StartRound(PresentableMd5State newState)
        {
            newState._RoundStepIndex = 0;
        }

        private void StartCompression(PresentableMd5State newState)
        {
            newState.X = new uint[16];

            for (uint j = 0; j < 64; j += 4)
            {
                newState.X[j / 4] = (((uint)newState.Data[newState.DataOffset + (j + 3)]) << 24) |
                        (((uint)newState.Data[newState.DataOffset + (j + 2)]) << 16) |
                        (((uint)newState.Data[newState.DataOffset + (j + 1)]) << 8) |
                        (((uint)newState.Data[newState.DataOffset + (j)]));
            }

            newState._RoundIndex = 0;

            newState.A = newState.H1;
            newState.B = newState.H2;
            newState.C = newState.H3;
            newState.D = newState.H4;
        }

        private void AddLength(PresentableMd5State newState)
        {
            uint lengthOffset = newState.DataLength + 8;

            for (int i = 8; i > 0; i--)
                newState.Data[lengthOffset - i] = (byte)(newState.LengthInBit >> ((8 - i) * 8) & 0xff);

            newState.IsPaddingDone = true;
        }

        private void AddPaddingBytes(PresentableMd5State newState)
        {
            // Save length of data in bit
            newState.LengthInBit = (newState.BytesHashed + newState.DataLength) * 8;

            // Add '1' bit to end of data
            newState.Data[newState.DataLength] = 0x80;
            newState.DataLength++;

            // Add zero bytes until 8 bytes short of next 64-byte block
            while (newState.DataLength % 64 != 56)
            {
                newState.Data[newState.DataLength++] = 0;
            }
        }

        public void InitFirstState()
        {
            Console.WriteLine("InitFirstState()");

            StateHistory.Clear();
            CurrentStateNumber = -1;
            AddNewState();

            CurrentState.BytesHashed = 0;
            CurrentState.H1 = 0x67452301;
            CurrentState.H2 = 0xEFCDAB89;
            CurrentState.H3 = 0x98BADCFE;
            CurrentState.H4 = 0X10325476;
            CurrentState.State = PresentableMd5State.Md5State.INITIALIZED;
        }

        public static uint RotateLeft(uint uiNumber, ushort shift)
        {
            return (uiNumber << shift) | (uiNumber >> (32 - shift));
        }

        protected delegate uint RoundFunction(uint a, uint b, uint c, uint d);
        protected readonly RoundFunction[] ROUND_FUNCTION = { FuncF, FuncG, FuncH, FuncI };

        private void PerformRoundStep(PresentableMd5State newState)
        {
            Console.WriteLine("Before R {0} S {1,2}: A = {2,10} B = {3,10} C = {4,10} D = {5,10}", newState.Round, newState.RoundStep, newState.A, newState.B, newState.C, newState.D);

            RoundFunction roundFunction = ROUND_FUNCTION[newState._RoundIndex];

            uint i = newState._RoundIndex * 16 + newState._RoundStepIndex;

            uint wordIndex;
            switch (newState._RoundIndex)
            {
                default:
                case 0:
                    wordIndex = i;
                    break;
                case 1:
                    wordIndex = 5 * i + 1;
                    break;
                case 2:
                    wordIndex = 3 * i + 5;
                    break;
                case 3:
                    wordIndex = 7 * i;
                    break;
            }
            wordIndex %= 16;

            switch (newState._RoundStepIndex % 4)
            {
                case 0:
                    ExecRoundFunction(ref newState.A, newState.B, newState.C, newState.D, roundFunction, newState.X[wordIndex], i);
                    break;
                case 1:
                    ExecRoundFunction(ref newState.D, newState.A, newState.B, newState.C, roundFunction, newState.X[wordIndex], i);
                    break;
                case 2:
                    ExecRoundFunction(ref newState.C, newState.D, newState.A, newState.B, roundFunction, newState.X[wordIndex], i);
                    break;
                case 3:
                    ExecRoundFunction(ref newState.B, newState.C, newState.D, newState.A, roundFunction, newState.X[wordIndex], i);
                    break;
            }

            if (newState.IsLastRound && newState.IsLastStepInRound)

                Console.WriteLine("After  R {0} S {1,2}: A = {2,10} B = {3,10} C = {4,10} D = {5,10}", newState.Round, newState.RoundStep, newState.A, newState.B, newState.C, newState.D);
        }

        protected static void ExecRoundFunction(ref uint a, uint b, uint c, uint d, RoundFunction function, uint W, uint i)
        {
            a = b + RotateLeft((a + function(a, b, c, d) + W + AdditionConstantTable[i]), ShiftConstantTable[i]);
        }

        protected static uint FuncF(uint a, uint b, uint c, uint d)
        {
            return d ^ (b & (c ^ d));
        }

        protected static uint FuncG(uint a, uint b, uint c, uint d)
        {
            return c ^ (d & (b ^ c));
        }

        protected static uint FuncH(uint a, uint b, uint c, uint d)
        {
            return b ^ c ^ d;
        }

        protected static uint FuncI(uint a, uint b, uint c, uint d)
        {
            return c ^ (b | ~d);
        }

        public byte[] HashValueBytes
        {
            get
            {
                byte[] result = new byte[16];
                writeUintToArray(result, 0, CurrentState.H1);
                writeUintToArray(result, 4, CurrentState.H2);
                writeUintToArray(result, 8, CurrentState.H3);
                writeUintToArray(result, 12, CurrentState.H4);
                return result;
            }
        }

        protected void writeUintToArray(byte[] array, int offset, uint value)
        {
            byte[] byteValue = BitConverter.GetBytes(value);
            Array.Copy(byteValue, 0, array, offset, 4);
        }
    }
}
