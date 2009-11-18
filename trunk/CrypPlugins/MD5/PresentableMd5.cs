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
        /// <summary>
        /// A list containing all states that have already been calculated
        /// </summary>
        public List<PresentableMd5State> StateHistory { get; set; }

        /// <summary>
        /// The current state of the algorithm
        /// </summary>
        public PresentableMd5State CurrentState { get; protected set; }

        /// <summary>
        /// Sequential number identifying the current state of the algorithm
        /// </summary>
        public int CurrentStateNumber { get; protected set; }

        /// <summary>
        /// The stream where data is read from
        /// </summary>
        protected Stream DataStream { get; set; }

        /// <summary>
        /// Returns whether this object has been initialized using the Initialize() method
        /// </summary>
        public bool IsInitialized { get; protected set; }

        /// <summary>
        /// Array of integer constants, each one is used in one of the compression function's 64 steps
        /// </summary>
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

        /// <summary>
        /// Array of 64 constants indicating how far the compression function's rotate operator shifts in each step
        /// </summary>
        protected static readonly ushort[] ShiftConstantTable = new ushort[64] 
			{	7, 12, 17, 22, 7, 12, 17, 22, 7, 12, 17, 22, 7, 12, 17, 22,
				5,  9, 14, 20, 5,  9, 14, 20, 5,  9, 14, 20, 5,  9, 14, 20,
                4, 11, 16, 23, 4, 11, 16, 23, 4, 11, 16, 23, 4, 11, 16, 23,
                6, 10, 15, 21, 6, 10, 15, 21, 6, 10, 15, 21, 6, 10, 15, 21      };

        /// <summary>
        /// Amount of bytes in one block of data
        /// </summary>
        protected const int DATA_BLOCK_SIZE = 64;

        /// <summary>
        /// The state before CurrentState, null if CurrentState is first state
        /// </summary>
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

        /// <summary>
        /// Delegate for status changed handlers
        /// </summary>
        public delegate void StatusChangedHandler();

        /// <summary>
        /// Raised whenever the algorithm changes its status
        /// </summary>
        public event StatusChangedHandler StatusChanged;

        /// <summary>
        /// Raised whenever a property changes, important for WPF binding
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Wrapper that raises a PropertyChanged event
        /// </summary>
        /// <param name="propertyName"></param>
        void OnPropChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises a StatusChanged event and PropertyChanged events for important properties
        /// </summary>
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

        /// <summary>
        /// Constructs state history list and adds the first "uninitialized" state
        /// </summary>
        public PresentableMd5()
        {
            StateHistory = new List<PresentableMd5State>();
            SetUninitializedState();
        }

        /// <summary>
        /// Assigns a data source and initializes the algorithm, putting it into "initialized" state
        /// </summary>
        /// <param name="dataStream">Data source</param>
        public void Initialize(Stream dataStream)
        {
            DataStream = dataStream;

            SetUninitializedState();
            PerformInitializationStep();

            IsInitialized = true;

            OnStatusChanged();
        }

        /// <summary>
        /// Clears the state history and adds the "uninitialized" state
        /// </summary>
        private void SetUninitializedState()
        {
            StateHistory.Clear();

            PresentableMd5State uninitializedState = new PresentableMd5State();
            uninitializedState.State = Md5State.UNINITIALIZED;
            StateHistory.Add(uninitializedState);
            CurrentState = uninitializedState;

            CurrentStateNumber = 0;
        }

        /// <summary>
        /// Adds a new state to the history and sets it as the current state
        /// </summary>
        protected void AddNewState()
        {
            if (CurrentStateNumber == -1)
                CurrentState = new PresentableMd5State();
            else
                CurrentState = new PresentableMd5State(StateHistory[CurrentStateNumber]);

            StateHistory.Add(CurrentState);
            CurrentStateNumber = StateHistory.Count - 1;
        }

        /// <summary>
        /// Determine if there are states beyond the current one available in the history
        /// </summary>
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

        /// <summary>
        /// Navigate one step back using the state history
        /// </summary>
        public void PreviousStep()
        {
            if (CurrentStateNumber == 0)
                return;

            CurrentStateNumber--;
            CurrentState = StateHistory[CurrentStateNumber];
            OnStatusChanged();
        }

        /// <summary>
        /// Determines whether the current state is the "finished" state
        /// </summary>
        public bool IsInFinishedState
        {
            get
            {
                return CurrentState.State == Md5State.FINISHED;
            }
        }

        /// <summary>
        /// Determines whether the current state is the first, "uninitialized", state
        /// </summary>
        public bool IsInFirstState
        {
            get
            {
                return CurrentStateNumber == 0;
            }
        }

        /// <summary>
        /// Goes one step forward by either restoring a previously calculated state from the history or calculating the next state
        /// </summary>
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

        /// <summary>
        /// Moves through the algorithm's steps until it is finished
        /// </summary>
        public void NextStepUntilFinished()
        {
            if (!IsInitialized)
                return;

            while (!IsInFinishedState)
                NextStep();
        }

        /// <summary>
        /// Moves one or more steps through the algorithm until the next "finished round" state
        /// </summary>
        public void NextStepUntilRoundEnd()
        {
            if (!IsInitialized)
                return;

            do
                NextStep();
            while (!IsInFinishedState && CurrentState.State != Md5State.FINISHED_ROUND);
        }


        /// <summary>
        /// Moves one or more steps through the algorithm until the next "finished compression" state
        /// </summary>
        public void NextStepUntilBlockEnd()
        {
            if (!IsInitialized)
                return;

            do
                NextStep();
            while (!IsInFinishedState && CurrentState.State != Md5State.FINISHED_COMPRESSION);
        }

        /// <summary>
        /// Performs the next step in the algorithm
        /// </summary>
        /// <param name="previousState">Previous state</param>
        /// <param name="newState">The new state which is to be determined</param>
        public void PerformStep(PresentableMd5State previousState, PresentableMd5State newState)
        {
            switch (previousState.State)
            {
                case Md5State.INITIALIZED:
                    // Start by reading data
                    newState.State = Md5State.READING_DATA;
                    break;

                case Md5State.READING_DATA:
                    // Fetch next data block and enter "data read" state
                    ReadData(newState);
                    newState.State = Md5State.READ_DATA;
                    break;

                case Md5State.READ_DATA:
                    // If an underfull buffer was read, enter "starting padding" state
                    // If a full buffer was read, enter "starting compression" state
                    if (previousState.DataLength < DATA_BLOCK_SIZE)
                        newState.State = Md5State.STARTING_PADDING;
                    else
                        newState.State = Md5State.STARTING_COMPRESSION;
                    break;

                case Md5State.STARTING_PADDING:
                    // First step of padding is adding the padding bytes
                    newState.State = Md5State.ADDING_PADDING_BYTES;
                    break;

                case Md5State.ADDING_PADDING_BYTES:
                    // Add necessary number of bytes and enter "added padding bytes" state
                    AddPaddingBytes(newState);
                    newState.State = Md5State.ADDED_PADDING_BYTES;
                    break;

                case Md5State.ADDED_PADDING_BYTES:
                    // Next step for padding is adding the data length
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
                    // Perform pre-compression initialization and continue by starting the first round
                    StartCompression(newState);
                    newState.State = Md5State.STARTING_ROUND;
                    break;

                case Md5State.STARTING_ROUND:
                    // Start the round and continue with the first round step
                    StartRound(newState);
                    newState.State = Md5State.STARTING_ROUND_STEP;
                    break;

                case Md5State.STARTING_ROUND_STEP:
                    // Perform the step and go into finished state
                    PerformRoundStep(newState);
                    newState.State = Md5State.FINISHED_ROUND_STEP;
                    break;

                case Md5State.FINISHED_ROUND_STEP:
                    // If last step, go into 'finished round' state, else continue with next step
                    if (previousState.IsLastStepInRound)
                        newState.State = Md5State.FINISHED_ROUND;
                    else
                    {
                        newState.RoundStepIndex++;
                        newState.State = Md5State.STARTING_ROUND_STEP;
                    }
                    break;

                case Md5State.FINISHED_ROUND:
                    // If last step, go into "finishing compression" state, else continue with next round
                    if (previousState.IsLastRound)
                        newState.State = Md5State.FINISHING_COMPRESSION;
                    else
                    {
                        newState.RoundIndex++;
                        newState.State = Md5State.STARTING_ROUND;
                    }
                    break;

                case Md5State.FINISHING_COMPRESSION:
                    // Perform finishing actions and go into "finished compression" state
                    FinishCompression(newState);
                    newState.State = Md5State.FINISHED_COMPRESSION;
                    break;

                case Md5State.FINISHED_COMPRESSION:
                    // If there's more data left in buffer, reenter compression function with offset
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

        /// <summary>
        /// Performs the steps necessary after the individual compression function steps have run
        /// </summary>
        /// <param name="newState">Algorithm state to modify</param>
        private void FinishCompression(PresentableMd5State newState)
        {
            // Add compression function results to accumulators
            newState.H1 += newState.A;
            newState.H2 += newState.B;
            newState.H3 += newState.C;
            newState.H4 += newState.D;

            // Increment the number of bytes hashed so far
            newState.BytesHashed += DATA_BLOCK_SIZE;
        }

        /// <summary>
        /// Reads from the data source
        /// </summary>
        /// <param name="newState">Algorithm state to modify</param>
        private void ReadData(PresentableMd5State newState)
        {
            // Fetch up to 64 bytes of data
            newState.Data = new byte[128];
            newState.DataLength = (uint)DataStream.Read(newState.Data, 0, 64);
            newState.DataOffset = 0;
        }

        /// <summary>
        /// Performs initialization before a round
        /// </summary>
        /// <param name="newState">Algorithm state to modify</param>
        private void StartRound(PresentableMd5State newState)
        {
            // Reset round step counter
            newState.RoundStepIndex = 0;
        }

        /// <summary>
        /// Performs initialization required before running compression function steps
        /// </summary>
        /// <param name="newState">Algorithm state to modify</param>
        private void StartCompression(PresentableMd5State newState)
        {
            // Read data into unsigned 32 bit integers
            newState.X = new uint[16];
            for (uint j = 0; j < 64; j += 4)
            {
                newState.X[j / 4] = (((uint)newState.Data[newState.DataOffset + (j + 3)]) << 24) |
                        (((uint)newState.Data[newState.DataOffset + (j + 2)]) << 16) |
                        (((uint)newState.Data[newState.DataOffset + (j + 1)]) << 8) |
                        (((uint)newState.Data[newState.DataOffset + (j)]));
            }

            // Reset round counter
            newState.RoundIndex = 0;

            // Initialize A, B, C, D with accumulated values
            newState.A = newState.H1;
            newState.B = newState.H2;
            newState.C = newState.H3;
            newState.D = newState.H4;
        }

        /// <summary>
        /// Adds the data length part of the padding
        /// </summary>
        /// <param name="newState">Algorithm state to modify</param>
        private void AddLength(PresentableMd5State newState)
        {
            // Determine offset behind last written byte
            uint lengthOffset = newState.DataLength + 8;

            // Write the length in bit as 8 byte little-endian integer
            for (int i = 8; i > 0; i--)
                newState.Data[lengthOffset - i] = (byte)(newState.LengthInBit >> ((8 - i) * 8) & 0xff);

            // Remember that padding is done now
            newState.IsPaddingDone = true;
        }

        /// <summary>
        /// Adds padding bytes to the data
        /// </summary>
        /// <param name="newState">Algorithm state to modify</param>
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

        /// <summary>
        /// Sets up and adds the "initialized" state
        /// </summary>
        public void PerformInitializationStep()
        {
            // Add a state
            AddNewState();

            // Initialize new state
            CurrentState.BytesHashed = 0;
            CurrentState.State = Md5State.INITIALIZED;

            // Set initial accumulator values
            CurrentState.H1 = 0x67452301;
            CurrentState.H2 = 0xEFCDAB89;
            CurrentState.H3 = 0x98BADCFE;
            CurrentState.H4 = 0X10325476;

        }

        /// <summary>
        /// Shift-Rotates an unsigned integer to the left
        /// </summary>
        /// <param name="uiNumber">Integer to rotate</param>
        /// <param name="shift">Number of bits to shift</param>
        /// <returns>Result of the shift</returns>
        public static uint RotateLeft(uint uiNumber, ushort shift)
        {
            return (uiNumber << shift) | (uiNumber >> (32 - shift));
        }

        /// <summary>
        /// Delegate for the inner round function applied in each step of the compression function
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        protected delegate uint RoundFunction(uint a, uint b, uint c, uint d);

        /// <summary>
        /// Constant array of the four inner round functions
        /// </summary>
        protected readonly RoundFunction[] ROUND_FUNCTION = { FuncF, FuncG, FuncH, FuncI };

        /// <summary>
        /// Performs one step of the compression function
        /// </summary>
        /// <param name="newState">Algorithm state to modify</param>
        private void PerformRoundStep(PresentableMd5State newState)
        {
            // Determine which round function to use
            RoundFunction roundFunction = ROUND_FUNCTION[newState.RoundIndex];

            // Determine which step in the compression function this is
            uint stepIndex = newState.RoundIndex * 16 + newState.RoundStepIndex;

            // Determine which part of the data to use in this step
            uint wordIndex;
            switch (newState.RoundIndex)
            {
                default:
                case 0:
                    wordIndex = stepIndex;
                    break;
                case 1:
                    wordIndex = 5 * stepIndex + 1;
                    break;
                case 2:
                    wordIndex = 3 * stepIndex + 5;
                    break;
                case 3:
                    wordIndex = 7 * stepIndex;
                    break;
            }
            wordIndex %= 16;

            // Execute the chosen round function
            ExecRoundFunction(newState, roundFunction, newState.X[wordIndex], stepIndex);
        }

        /// <summary>
        /// Executes the round function and modifies algorithm state to reflect results
        /// </summary>
        /// <param name="state">Algorithm state to modify</param>
        /// <param name="function">The inner round function to execute</param>
        /// <param name="W">The part of the data to use in the round function</param>
        /// <param name="i">Index of this step (range 0 - 63)</param>
        protected static void ExecRoundFunction(PresentableMd5State state, RoundFunction function, uint W, uint i)
        {
            // Apply central compression function
            state.A = state.B + RotateLeft((state.A + function(state.A, state.B, state.C, state.D) + W + AdditionConstantTable[i]), ShiftConstantTable[i]);

            // Right-rotate the 4 compression result accumulators
            uint oldD = state.D;
            state.D = state.C;
            state.C = state.B;
            state.B = state.A;
            state.A = oldD;
        }

        /// <summary>
        /// Inner round function F, applied in step 1-16 of the compression function
        /// </summary>
        /// <param name="a">Temporary step variable A</param>
        /// <param name="b">Temporary step variable B</param>
        /// <param name="c">Temporary step variable C</param>
        /// <param name="d">Temporary step variable D</param>
        /// <returns>Result of inner round function F</returns>
        protected static uint FuncF(uint a, uint b, uint c, uint d)
        {
            return d ^ (b & (c ^ d));
        }

        /// <summary>
        /// Inner round function G, applied in step 17-32 of the compression function
        /// </summary>
        /// <param name="a">Temporary step variable A</param>
        /// <param name="b">Temporary step variable B</param>
        /// <param name="c">Temporary step variable C</param>
        /// <param name="d">Temporary step variable D</param>
        /// <returns>Result of inner round function G</returns>
        protected static uint FuncG(uint a, uint b, uint c, uint d)
        {
            return c ^ (d & (b ^ c));
        }

        /// <summary>
        /// Inner round function H, applied in step 33-48 of the compression function
        /// </summary>
        /// <param name="a">Temporary step variable A</param>
        /// <param name="b">Temporary step variable B</param>
        /// <param name="c">Temporary step variable C</param>
        /// <param name="d">Temporary step variable D</param>
        /// <returns>Result of inner round function H</returns>
        protected static uint FuncH(uint a, uint b, uint c, uint d)
        {
            return b ^ c ^ d;
        }

        /// <summary>
        /// Inner round function I, applied in step 49-64 of the compression function
        /// </summary>
        /// <param name="a">Temporary step variable A</param>
        /// <param name="b">Temporary step variable B</param>
        /// <param name="c">Temporary step variable C</param>
        /// <param name="d">Temporary step variable D</param>
        /// <returns>Result of inner round function I</returns>
        protected static uint FuncI(uint a, uint b, uint c, uint d)
        {
            return c ^ (b | ~d);
        }

        /// <summary>
        /// The current state's accumulator variables as byte array
        /// </summary>
        /// <remarks>
        /// When the algorithm is finished, this is the computed MD5 digest value.
        /// </remarks>
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

        /// <summary>
        /// Writes an integer into an array in little-endian representation
        /// </summary>
        /// <param name="array">Array which should be written to</param>
        /// <param name="offset">Offset in the array</param>
        /// <param name="value">Integer to write to array</param>
        protected void writeUintToArray(byte[] array, int offset, uint value)
        {
            byte[] byteValue = BitConverter.GetBytes(value);
            Array.Copy(byteValue, 0, array, offset, 4);
        }
    }
}
