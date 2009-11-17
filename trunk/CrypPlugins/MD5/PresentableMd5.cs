using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.ComponentModel;

namespace Cryptool.MD5
{
    public class PresentableMd5 : INotifyPropertyChanged
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

        public delegate void StatusChangedHandler();
        public event StatusChangedHandler StatusChanged;

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnStatusChanged()
        {
            OnPropChanged("CurrentState");
            OnPropChanged("LastState");
            OnPropChanged("CurrentStateNumber");
            OnPropChanged("IsInFinishedState");
            OnPropChanged("HashValueBytes");

            if (StatusChanged != null)
                StatusChanged();
        }

        public PresentableMd5()
        {
            StateHistory = new List<PresentableMd5State>();
            SetUninitializedState();
        }

        public void Initialize(Stream dataStream)
        {
            DataStream = dataStream;

            SetUninitializedState();
            PerformInitializationStep();

            IsInitialized = true;

            OnStatusChanged();
        }

        private void SetUninitializedState()
        {
            StateHistory.Clear();

            PresentableMd5State uninitializedState = new PresentableMd5State();
            uninitializedState.State = Md5State.UNINITIALIZED;
            StateHistory.Add(uninitializedState);
            CurrentState = uninitializedState;

            CurrentStateNumber = 0;
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
            OnStatusChanged();
        }

        public bool IsInFinishedState
        {
            get
            {
                return CurrentState.State == Md5State.FINISHED;
            }
        }

        public bool IsInFirstState
        {
            get
            {
                return CurrentStateNumber == 0;
            }
        }

        public void NextStep()
        {
            if (!IsInitialized)
                return;

            if (HistoryHasMoreStates)
            {
                CurrentStateNumber++;
                CurrentState = StateHistory[CurrentStateNumber];
                OnStatusChanged();
            }
            else
            {
                if (IsInFinishedState)
                    return;

                PresentableMd5State previousState = CurrentState;
                AddNewState();
                PerformStep(previousState, CurrentState);
                OnStatusChanged();
            }
        }

        public void NextStepUntilFinished()
        {
            if (!IsInitialized)
                return;

            while (!IsInFinishedState)
                NextStep();
        }

        public void NextStepUntilRoundEnd()
        {
            if (!IsInitialized)
                return;

            while (!IsInFinishedState && CurrentState.State != Md5State.FINISHED_ROUND)
                NextStep();
        }

        public void NextStepUntilBlockEnd()
        {
            if (!IsInitialized)
                return;

            while (!IsInFinishedState && CurrentState.State != Md5State.FINISHED_COMPRESSION)
                NextStep();
        }

        public void PerformStep(PresentableMd5State previousState, PresentableMd5State newState)
        {
            switch (previousState.State)
            {
                case Md5State.INITIALIZED:
                    // If initialization is complete, start by reading data
                    newState.State = Md5State.READING_DATA;
                    break;

                case Md5State.READING_DATA:
                    // Read data and enter "data read" state
                    ReadData(newState);
                    newState.State = Md5State.READ_DATA;
                    break;

                case Md5State.READ_DATA:
                    // If an underfull buffer was read, we're at the end of the digestible data, so enter "starting padding" state
                    // If a full buffer was read, enter "starting compression" state
                    if (previousState.DataLength < DATA_BLOCK_SIZE)
                        newState.State = Md5State.STARTING_PADDING;
                    else
                        newState.State = Md5State.STARTING_COMPRESSION;
                    break;

                case Md5State.STARTING_PADDING:
                    // First step of padding is adding the padding bytes, so enter that state
                    newState.State = Md5State.ADDING_PADDING_BYTES;
                    break;

                case Md5State.ADDING_PADDING_BYTES:
                    // Add necessary number of bytes and enter "added padding bytes" state
                    AddPaddingBytes(newState);
                    newState.State = Md5State.ADDED_PADDING_BYTES;
                    break;

                case Md5State.ADDED_PADDING_BYTES:
                    // The next step for padding is adding the data length, so enter that state
                    newState.State = Md5State.ADDING_LENGTH;
                    break;

                case Md5State.ADDING_LENGTH:
                    // Add the length of the data and enter "added length" state
                    AddLength(newState);
                    newState.State = Md5State.ADDED_LENGTH;
                    break;

                case Md5State.ADDED_LENGTH:
                    // Padding is done after adding data length, so enter "finished padding" state
                    newState.State = Md5State.FINISHED_PADDING;
                    break;

                case Md5State.FINISHED_PADDING:
                    // If padding is finished, call compression function for the last (two) time(s)
                    newState.State = Md5State.STARTING_COMPRESSION;
                    break;

                case Md5State.STARTING_COMPRESSION:
                    StartCompression(newState);
                    newState.State = Md5State.STARTING_ROUND;
                    break;

                case Md5State.STARTING_ROUND:
                    StartRound(newState);
                    newState.State = Md5State.STARTING_ROUND_STEP;
                    break;

                case Md5State.STARTING_ROUND_STEP:
                    PerformRoundStep(newState);
                    newState.State = Md5State.FINISHED_ROUND_STEP;
                    break;

                case Md5State.FINISHED_ROUND_STEP:
                    if (previousState.IsLastStepInRound)
                        newState.State = Md5State.FINISHED_ROUND;
                    else
                    {
                        newState.RoundStepIndex++;
                        newState.State = Md5State.STARTING_ROUND_STEP;
                    }
                    break;

                case Md5State.FINISHED_ROUND:
                    if (previousState.IsLastRound)
                        newState.State = Md5State.FINISHING_COMPRESSION;
                    else
                    {
                        newState.RoundIndex++;
                        newState.State = Md5State.STARTING_ROUND;
                    }
                    break;

                case Md5State.FINISHING_COMPRESSION:
                    FinishCompression(newState);
                    newState.State = Md5State.FINISHED_COMPRESSION;
                    break;

                case Md5State.FINISHED_COMPRESSION:
                    // If compression is finished, check if there's more data left in buffer. If so, reenter compression function with offset
                    if (previousState.DataLength - previousState.DataOffset > DATA_BLOCK_SIZE)
                    {
                        // Still some data left in buffer, rerun compression with offset
                        newState.DataOffset += DATA_BLOCK_SIZE;
                        newState.State = Md5State.STARTING_COMPRESSION;
                    }
                    else
                    {
                        // No data left in buffer

                        if (previousState.IsPaddingDone)
                        {
                            // If padding was already added, we're done
                            newState.State = Md5State.FINISHED;
                        }
                        else
                        {
                            // Read more data
                            newState.State = Md5State.READING_DATA;
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
            newState.RoundStepIndex = 0;
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

            newState.RoundIndex = 0;

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

        public void PerformInitializationStep()
        {
            AddNewState();

            CurrentState.BytesHashed = 0;
            CurrentState.H1 = 0x67452301;
            CurrentState.H2 = 0xEFCDAB89;
            CurrentState.H3 = 0x98BADCFE;
            CurrentState.H4 = 0X10325476;
            CurrentState.State = Md5State.INITIALIZED;
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

            RoundFunction roundFunction = ROUND_FUNCTION[newState.RoundIndex];

            uint i = newState.RoundIndex * 16 + newState.RoundStepIndex;

            uint wordIndex;
            switch (newState.RoundIndex)
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

            ExecRoundFunction(newState, roundFunction, newState.X[wordIndex], i);

            if (newState.IsLastRound && newState.IsLastStepInRound)

                Console.WriteLine("After  R {0} S {1,2}: A = {2,10} B = {3,10} C = {4,10} D = {5,10}", newState.Round, newState.RoundStep, newState.A, newState.B, newState.C, newState.D);
        }

        protected static void ExecRoundFunction(PresentableMd5State state, RoundFunction function, uint W, uint i)
        {
            state.A = state.B + RotateLeft((state.A + function(state.A, state.B, state.C, state.D) + W + AdditionConstantTable[i]), ShiftConstantTable[i]);

            uint oldD = state.D;
            state.D = state.C;
            state.C = state.B;
            state.B = state.A;
            state.A = oldD;
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
