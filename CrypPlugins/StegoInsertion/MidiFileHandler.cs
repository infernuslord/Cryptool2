using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace Cryptool.Plugins.StegoInsertion
{
    class MidiFileHandler
    {
        //Reader for the source MIDI file
        private BinaryReader srcReader = null;
        /*
        /// <summary>Destination stream</summary>
        protected Stream outputStream;
        */
        //Writer for the destination MIDI file
        private BinaryWriter dstWriter = null;

        //Count of 4-bit blocks to hide before a "Program Change" message
        private byte halfBytesPerMidiMessage;

        // Length of the message
        private byte countBytesToHide;

        //Has [halfBytesPerMidiMessage] already been written to/read from the file?
        private bool isHalfBytesPerMidiMessageFinished = false;

        //Has [countBytesToHide] already been written to/read from the file?
        private bool isCountBytesToHideFinished = false;

        /// <summary>Read a MIDI file and hide or extract a message</summary>
        /// <param name="srcFile">Clean MIDI file</param>
        /// <param name="secretMessage">The message to hide, or empty stream to retrieve the extracted message</param>
        /// <param name="key">A key pattern which specifies the count of ProgChg events to ignore before hiding the next half-byte</param>
        /// <param name="extract">true: Extract a message from [srcFileName]; false: Hide a message in [srcFileName]</param>
        public void HideOrExtract(Stream srcFile, Stream secretMessage, Stream key, Stream outputStream, byte localHalfBytesPerMidiMessage, bool extract)
        {
            this.halfBytesPerMidiMessage = localHalfBytesPerMidiMessage;
            srcReader = new BinaryReader(srcFile);
            dstWriter = new BinaryWriter(outputStream);

            //If the flag is true, the rest of the source file is copied without changes
            bool isMessageComplete = false;
            //stores the currently processed message
            MidiMessage midiMessage = new MidiMessage();

            try
            {
                MidiFileHeader header = new MidiFileHeader();

                //Read type
                header.HeaderType = CopyChars(4);
                header.DataLength = new byte[4];
                header.DataLength = CopyBytes(4);

                if ((new String(header.HeaderType) != "MThd")
                    || (header.DataLength[3] != 6))
                {
                    srcReader.Close();
                    dstWriter.Close();
                    throw new InvalidOperationException("No standard MIDI file.");
                }

                //These values are Int16, stored in reverse byte order
                header.FileType = CopyInt16();
                header.CountTracks = CopyInt16();
                header.Division = CopyInt16();

                //-------------------------------- Read Tracks

                //Get the first secret byte
                byte[] currentMessageByte = extract
                    ? new byte[2] { 0, 0 }
                    : SplitByte((byte)secretMessage.ReadByte());
                byte currentMessageByteIndex = 0;

                //Initialize counter for the bytes added to the track
                Int32 countBytesAdded = 0;

                //Get the first key byte (0 if no key used)
                int countIgnoreMessages = GetKeyValue(key);

                for (int track = 0; track < header.CountTracks; track++)
                {

                    if (srcReader.BaseStream.Position == srcReader.BaseStream.Length)
                    {
                        break; //no more tracks found
                    }

                    //Read track header

                    MidiTrackHeader th = new MidiTrackHeader();
                    th.HeaderType = CopyChars(4);
                    if (new String(th.HeaderType) != "MTrk")
                    {
                        //not a standard track - search the next track
                        while (srcReader.BaseStream.Position + 4 < srcReader.BaseStream.Length)
                        {
                            th.HeaderType = CopyChars(4);
                            if (new String(th.HeaderType) == "MTrk")
                            {
                                break;
                            }
                        }
                    }

                    int trackLengthPosition = (dstWriter == null) ? 0
                        : (int)dstWriter.BaseStream.Position;

                    //Read the length field and convert it to Int32
                    //srcReader.ReadInt32() returns a wrong value,
                    //because of the reverse byte order

                    byte[] trackLength = new byte[4];
                    trackLength = CopyBytes(4);

                    th.DataLength = trackLength[0] << 24;
                    th.DataLength += trackLength[1] << 16;
                    th.DataLength += trackLength[2] << 8;
                    th.DataLength += trackLength[3];

                    //start new track
                    bool isEndOfTrack = false;
                    //no bytes added yet
                    countBytesAdded = 0;
                    while (!isEndOfTrack)
                    {

                        //Read the messages

                        /* 1st field: Time - variable length
                         * 2nd field: Message type and channel - 1 byte
                         *    The lower four bits contain the channel (0-15),
                         *    the higher four bits contain the message type (8-F)
                         * 3rd and 4th field: Message parameters - 1 byte each */

                        ReadMidiMessageHeader(ref midiMessage);

                        if (midiMessage.MessageType == 0xFF)
                        { //non-MIDI event
                            if (dstWriter != null)
                            {
                                dstWriter.Write(midiMessage.Time);
                                dstWriter.Write(midiMessage.MessageType);
                            }
                            byte name = CopyByte();
                            int length = (int)CopyVariableLengthValue();
                            CopyBytes(length);

                            if ((name == 0x2F) && (length == 0))
                            { // End Of Track
                                isEndOfTrack = true;
                            }
                        }
                        else
                        {
                            //remove channel information by resetting the 4 lower bits
                            byte cleanMessageType = (byte)(((byte)(midiMessage.MessageType >> 4)) << 4);

                            if ((cleanMessageType != 0xC0) && (dstWriter != null))
                            {
                                //Not a "program change"/"note off" message - Copy it
                                dstWriter.Write(midiMessage.Time);
                                dstWriter.Write(midiMessage.MessageType);
                            }

                            switch (cleanMessageType)
                            {
                                case 0x80: //Note Off - Note and Velocity following
                                case 0x90: //Note On - Note and Velocity following
                                case 0xA0: //After Touch - Note and Pressure following
                                case 0xB0: //Control Change - Control and Value following
                                case 0xD0: //Channel Pressure - Value following
                                case 0xE0: //Pitch Wheel - 14-bit value following
                                    { 
                                        CopyBytes(2);
                                        break;
                                    }

                                case 0xC0:
                                    { //Program Change - Program following
                                        //Get program number
                                        midiMessage.MessageData = srcReader.ReadBytes(1);

                                        if (!isHalfBytesPerMidiMessageFinished)
                                        {
                                            if (extract)
                                            {
                                                //Read block size
                                                halfBytesPerMidiMessage = midiMessage.MessageData[0];
                                                countBytesAdded -= midiMessage.Time.Length + 2;

                                                //Get next message
                                                ReadMidiMessageHeader(ref midiMessage);
                                                //Get program number
                                                midiMessage.MessageData = srcReader.ReadBytes(1);
                                            }
                                            else
                                            {
                                                //Write block size
                                                MidiMessage msg = new MidiMessage(midiMessage, new byte[1] { halfBytesPerMidiMessage });
                                                WriteMidiMessage(msg);
                                                countBytesAdded += midiMessage.Time.Length + 2;
                                            }
                                            isHalfBytesPerMidiMessageFinished = true;
                                        }

                                        if (!isCountBytesToHideFinished)
                                        {
                                            if (extract)
                                            {
                                                //Read block size
                                                countBytesToHide = midiMessage.MessageData[0];
                                                countBytesAdded -= midiMessage.Time.Length + 2;

                                                //Get next message
                                                ReadMidiMessageHeader(ref midiMessage);
                                                //Get program number
                                                midiMessage.MessageData = srcReader.ReadBytes(1);
                                            }
                                            else
                                            {
                                                //Write block size
                                                MidiMessage msg = new MidiMessage(midiMessage, new byte[1] { (byte)secretMessage.Length });
                                                WriteMidiMessage(msg);
                                                countBytesAdded += midiMessage.Time.Length + 2;
                                            }
                                            isCountBytesToHideFinished = true;
                                        }

                                        ProcessMidiMessage(midiMessage, secretMessage, key, extract,
                                            ref isMessageComplete, ref countIgnoreMessages,
                                            ref currentMessageByte, ref currentMessageByteIndex,
                                            ref countBytesAdded);

                                        break;
                                    }
                                case 0xF0:
                                    { //SysEx - no length, read until end tag 0xF7 is found
                                        byte b = 0;
                                        while (b != 0xF7)
                                        {
                                            b = CopyByte();
                                        }
                                        break;
                                    }
                                default:
                                    break;
                            }

                        } //else - MIDI message

                    } //while() over messages

                    if (dstWriter != null)
                    {
                        //Change length field in track header
                        th.DataLength += countBytesAdded;
                        trackLength = IntToArray(th.DataLength);
                        dstWriter.Seek(trackLengthPosition, SeekOrigin.Begin);
                        dstWriter.Write(trackLength);
                        dstWriter.Seek(0, SeekOrigin.End);
                    }

                }//for() over tracks

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No standard MIDI file.", ex);
            }
            finally
            {
                srcReader.Close();
            }
        }

        /// <summary>Reads the header of a MIDI event.</summary>
        /// <param name="midiMessage">Returns a MIDI message with initialized time and type.</param>
        private void ReadMidiMessageHeader(ref MidiMessage midiMessage)
        {
            midiMessage = new MidiMessage();
            //Read time
            ReadVariableLengthValue(out midiMessage.Time);
            //Read type and channel
            midiMessage.MessageType = srcReader.ReadByte();
        }

        /// <summary>Splits an Int32 into four bytes.</summary>
        /// <param name="val">An Int32 value.</param>
        /// <returns>Four bytes: Highest byte first, lowest byte last.</returns>
        private static byte[] IntToArray(int val)
        {
            byte[] bytes = new byte[4];

            bytes[0] = (byte)((val & 0xFF000000) >> 24);
            bytes[1] = (byte)((val & 0x00FF0000) >> 16);
            bytes[2] = (byte)((val & 0x0000FF00) >> 8);
            bytes[3] = (byte)(val & 0x000000FF);

            return bytes;
        }

        /// <summary>Splits an Int64 into eight bytes.</summary>
        /// <param name="val">An Int64 value.</param>
        /// <returns>Eight bytes: Lowest byte first, highest byte last.</returns>
        public static byte[] IntToArray(Int64 val)
        {
            byte[] bytes = new byte[8];
            for (int n = 0; n < 8; n++)
            {
                bytes[n] = (byte)(val >> (n * 8));
            }
            return bytes;
        }

        /// <summary>Concatenates eight bytes to an Int64 value.</summary>
        /// <param name="bytes">Eight bytes.</param>
        /// <returns>The Int64.</returns>
        public Int64 ArrayToInt(byte[] bytes)
        {
            Int64 result = 0;
            for (int n = 0; n < bytes.Length; n++)
            {
                result += (bytes[n] << (n * 8));
            }
            return result;
        }

        /// <summary>Write a MIDI event into the destination stream.</summary>
        /// <param name="midiMessage">The MIDI event.</param>
        private void WriteMidiMessage(MidiMessage midiMessage)
        {
            dstWriter.Write(midiMessage.Time);
            dstWriter.Write(midiMessage.MessageType);
            dstWriter.Write(midiMessage.MessageData);
        }

        /// <summary>Call ProcessMidiMessageE or ProcessMidiMessageH.</summary>
        /// <param name="midiMessage">The current MIDI message.</param>
        /// <param name="secretMessage">The stream that is being hidden, or destination for extracted values.</param>
        /// <param name="key">Key stream.</param>
        /// <param name="extract">Current action: true=extract; hide=false.</param>
        /// <param name="isMessageComplete">This flag is set when the full message has been hidden/extracted.</param>
        /// <param name="countIgnoreMessages">
        /// Countdown for messages that are ignored according to the key.
        /// If the value is 0, the current message gets processed, otherwise the value gets decremented.
        /// </param>
        /// <param name="currentMessageByte">Destination for the extracted byte, or the byte to hide in the MIDI message, split into high and low part.</param>
        /// <param name="currentMessageByteIndex">Current index in [currentMessageByte].</param>
        /// <param name="countBytesAdded">
        /// Counter for the bytes already added to the MIDI file.
        /// This value must be updated everytime something is inserted into or deleted from the MIDI file, because
        /// the new track length will be calculated from it. MIDI files with incorrect length fields cannot be played.
        /// </param>
        private void ProcessMidiMessage(MidiMessage midiMessage, Stream secretMessage, Stream key, bool extract, ref bool isMessageComplete,
            ref int countIgnoreMessages, ref byte[] currentMessageByte, ref byte currentMessageByteIndex, ref int countBytesAdded)
        {

            if (extract)
            {
                ProcessMidiMessageE(midiMessage, secretMessage, key,
                    ref isMessageComplete, ref countIgnoreMessages,
                    ref currentMessageByte, ref currentMessageByteIndex,
                    ref countBytesAdded);
            }
            else
            {
                ProcessMidiMessageH(midiMessage, secretMessage, key,
                    ref isMessageComplete, ref countIgnoreMessages,
                    ref currentMessageByte, ref currentMessageByteIndex,
                    ref countBytesAdded);
            }
        }

        /// <summary>Extracts data from a MIDI message.</summary>
        /// <param name="midiMessage">The current MIDI message.</param>
        /// <param name="secretMessage">Destination for extracted values.</param>
        /// <param name="key">Key stream.</param>
        /// <param name="isMessageComplete">This flag is set when the full message has been extracted.</param>
        /// <param name="countIgnoreMessages">
        /// Countdown for messages that are ignored according to the key.
        /// If the value is 0, the current message gets processed, otherwise the value gets decremented.
        /// </param>
        /// <param name="currentMessageByte">Destination for the extracted byte, split into high and low part.</param>
        /// <param name="currentMessageByteIndex">Current index in [currentMessageByte].</param>
        /// <param name="countBytesAdded">
        /// Counter for the bytes already added to the MIDI file.
        /// This value must be updated everytime something is inserted into or deleted from the MIDI file, because
        /// the new track length will be calculated from it. MIDI files with incorrect length fields cannot be played.
        /// </param>
        private void ProcessMidiMessageE(MidiMessage midiMessage, Stream secretMessage, Stream key, ref bool isMessageComplete,
            ref int countIgnoreMessages, ref byte[] currentMessageByte, ref byte currentMessageByteIndex, ref int countBytesAdded)
        {
            if ((countIgnoreMessages == 0) && (!isMessageComplete))
            {
                for (int n = 0; n < halfBytesPerMidiMessage; n++)
                {
                    ExtractHalfByte(midiMessage, secretMessage, ref currentMessageByte, ref currentMessageByteIndex, ref countBytesAdded);

                    if ((countBytesToHide > 0) && (secretMessage.Length == countBytesToHide))
                    {
                        //All bytes extracted - ignore following "ProgramChange" messages
                        isMessageComplete = true;
                        break;
                    }

                    if ((n + 1) < halfBytesPerMidiMessage)
                    {
                        //There are more hidden packets following - read next header
                        ReadMidiMessageHeader(ref midiMessage);
                        midiMessage.MessageData = srcReader.ReadBytes(1);
                    }
                }

                //get next step width
                countIgnoreMessages = GetKeyValue(key);
                //copy non-message ProgramChange
                CopyMessage(midiMessage.MessageData.Length);

            }
            else
            {
                if (dstWriter != null)
                {
                    WriteMidiMessage(midiMessage);
                }
                countIgnoreMessages--;
            }
        }

        /// <summary>Hides blocks of half-bytes, copy the original Program Change event.</summary>
        /// <param name="midiMessage">The current MIDI message.</param>
        /// <param name="secretMessage">The stream that is being hidden.</param>
        /// <param name="key">Key stream.</param>
        /// <param name="isMessageComplete">This flag is set when the full message has been hidden/extracted.</param>
        /// <param name="countIgnoreMessages">
        /// Countdown for messages that are ignored according to the key.
        /// If the value is 0, the current message gets processed, otherwise the value gets decremented.
        /// </param>
        /// <param name="currentMessageByte">The current byte from [secretMessage], split into high and low part.</param>
        /// <param name="currentMessageByteIndex">Current index in [currentMessageByte].</param>
        /// <param name="countBytesAdded">
        /// Counter for the bytes already added to the MIDI file.
        /// This value must be updated everytime something is inserted into or deleted from the MIDI file, because
        /// the new track length will be calculated from it. MIDI files with incorrect length fields cannot be played.
        /// </param>
        private void ProcessMidiMessageH(MidiMessage midiMessage, Stream secretMessage, Stream key, ref bool isMessageComplete,
            ref int countIgnoreMessages, ref byte[] currentMessageByte, ref byte currentMessageByteIndex, ref int countBytesAdded)
        {

            if (!isMessageComplete)
            {
                if (countIgnoreMessages == 0)
                {
                    //Hide as many 4-bit-packets as specified
                    for (int n = 0; n < halfBytesPerMidiMessage; n++)
                    {
                        //Create a new message with the same content as the original, initialize data byte
                        MidiMessage msg = new MidiMessage(midiMessage, new byte[midiMessage.MessageData.Length]);
                        //Write the new message to the destination file
                        isMessageComplete = HideHalfByte(msg, secretMessage,
                            ref currentMessageByte, ref currentMessageByteIndex, ref countBytesAdded);

                        if (isMessageComplete) { break; }
                    }
                    //get next step width
                    countIgnoreMessages = GetKeyValue(key);
                }
                else
                {
                    countIgnoreMessages--; //Count down to 0
                }
            }

            //copy original message
            WriteMidiMessage(midiMessage);
        }

        /// <summary>Copies a MIDI event.</summary>
        /// <param name="countDataBytes">Number of parameter bytes.</param>
        private void CopyMessage(int countDataBytes)
        {
            //copy unchanged message
            CopyVariableLengthValue();
            CopyByte(); //read type and channel
            CopyBytes(countDataBytes);
        }

        /// <summary>Hides four bits in a MIDI message.</summary>
        /// <param name="midiMessage">The current MIDI message.</param>
        /// <param name="secretMessage">The stream that is being hidden.</param>
        /// <param name="currentMessageByte">The current byte from [secretMessage], split into high and low part.</param>
        /// <param name="currentMessageByteIndex">Current index in [currentMessageByte].</param>
        /// <param name="countBytesAdded">
        /// Counter for the bytes already added to the MIDI file.
        /// This value must be updated everytime something is inserted into or deleted from the MIDI file, because
        /// the new track length will be calculated from it. MIDI files with incorrect length fields cannot be played.
        /// </param>
        /// <returns>true = the secret message is finished; false = continue hiding.</returns>
        private bool HideHalfByte(MidiMessage midiMessage, Stream secretMessage, ref byte[] currentMessageByte, ref byte currentMessageByteIndex, ref int countBytesAdded)
        {
            bool returnValue = false;
            //Place the current byte of the secret message in the MIDI message's data byte
            midiMessage.MessageData[0] = currentMessageByte[currentMessageByteIndex];
            //Write it to destination file
            WriteMidiMessage(midiMessage);
            //Count the added bytes
            countBytesAdded += midiMessage.Time.Length + 1 + midiMessage.MessageData.Length;

            //Proceed to the next half-byte

            currentMessageByteIndex++;

            if (currentMessageByteIndex == 2)
            {
                int nextValue = secretMessage.ReadByte();
                if (nextValue < 0)
                {
                    returnValue = true;
                }
                else
                {
                    currentMessageByte = SplitByte((byte)nextValue);
                    currentMessageByteIndex = 0;
                }
            }

            return returnValue;
        }

        /// <summary>Extracts four bits from a MIDI message.</summary>
        /// <param name="midiMessage">The current MIDI message.</param>
        /// <param name="secretMessage">Destination for extracted values.</param>
        /// <param name="currentMessageByte">Destination for the extracted byte, split into high and low part.</param>
        /// <param name="currentMessageByteIndex">Current index in [currentMessageByte].</param>
        /// <param name="countBytesAdded">
        /// Counter for the bytes already added to the MIDI file.
        /// This value must be updated everytime something is inserted into or deleted from the MIDI file, because
        /// the new track length will be calculated from it. MIDI files with incorrect length fields cannot be played.
        /// </param>
        /// <returns>true = the secret message is finished; false = continue hiding.</returns>
        private void ExtractHalfByte(MidiMessage midiMessage, Stream secretMessage, ref byte[] currentMessageByte, ref byte currentMessageByteIndex, ref int countBytesAdded)
        {
            //Copy the hidden half-byte
            currentMessageByte[currentMessageByteIndex] = midiMessage.MessageData[0];

            //Count removed (negativly added) bytes: time, type, data
            countBytesAdded -= midiMessage.Time.Length + 1 + midiMessage.MessageData.Length;

            //Proceed to the next half-byte
            currentMessageByteIndex++;
            if (currentMessageByteIndex == 2)
            {
                //Write extracted byte
                byte completeMessageByte = (byte)((currentMessageByte[0] << 4) + currentMessageByte[1]);
                secretMessage.WriteByte(completeMessageByte);

                currentMessageByte[0] = 0;
                currentMessageByte[1] = 0;
                currentMessageByteIndex = 0;
            }
        }

        /// <summary>Splits a byte into high and low half-byte.</summary>
        /// <param name="b">The byte.</param>
        /// <returns>Two bytes. First value are the higher four bits, seconds value are the lower four bits.</returns>
        private byte[] SplitByte(byte b)
        {
            byte[] parts = new byte[2];
            parts[0] = (byte)(b >> 4); //shift higher half into lower half
            parts[1] = (byte)((byte)(b << 4) >> 4); //shift higher half outside, shift back
            return parts;
        }

        private char[] CopyChars(int count)
        {
            char[] chars = srcReader.ReadChars(count);
            if (dstWriter != null) { dstWriter.Write(chars); }
            return chars;
        }

        private byte[] CopyBytes(int count)
        {
            byte[] buffer = new byte[count];
            srcReader.Read(buffer, 0, count);
            if (dstWriter != null) { dstWriter.Write(buffer); }
            return buffer;
        }

        private byte CopyByte()
        {
            byte b = srcReader.ReadByte();
            if (dstWriter != null) { dstWriter.Write(b); }
            return b;
        }

        private Int16 CopyInt16()
        {
            byte b = CopyByte();
            return (Int16)((b << 8) + CopyByte());
        }

        private long CopyVariableLengthValue()
        {
            byte[] dummy;
            return CopyVariableLengthValue(out dummy);
        }

        private long CopyVariableLengthValue(out byte[] rawDataBuffer)
        {
            long returnValue;
            byte b;
            ArrayList allBytes = new ArrayList();

            //read the first byte
            returnValue = CopyByte();
            allBytes.Add((byte)returnValue);

            if ((returnValue & 0x80) > 0)
            { //bit 7 is set: there are more bytes to read
                returnValue &= 0x7F; //remove bit 7 - it is only a not-the-last-one flag
                do
                {
                    b = CopyByte(); //read next byte
                    allBytes.Add(b);
                    //remove flag and append byte
                    returnValue = (returnValue << 7) + (b & 0x7F);
                } while ((b & 0x80) > 0); //until bit-7 is not set
            }

            rawDataBuffer = (byte[])allBytes.ToArray(typeof(byte));
            return returnValue;
        }

        private long ReadVariableLengthValue(out byte[] rawDataBuffer)
        {
            long returnValue;
            byte b;
            ArrayList allBytes = new ArrayList();

            //read the first byte
            returnValue = srcReader.ReadByte();
            allBytes.Add((byte)returnValue);

            if ((returnValue & 0x80) > 0)
            { //bit 7 is set: there are more bytes to read
                returnValue &= 0x7F; //remove bit 7 - it is only a not-the-last-one flag
                do
                {
                    b = srcReader.ReadByte(); //read next byte
                    allBytes.Add(b);
                    //remove flag and append byte
                    returnValue = (returnValue << 7) + (b & 0x7F);
                } while ((b & 0x80) > 0); //until bit-7 is not set
            }

            rawDataBuffer = (byte[])allBytes.ToArray(typeof(byte));
            return returnValue;
        }

        /// <summary>
        /// Read the next byte of the key stream.
        /// Reset the stream if it is too short.
        /// </summary>
        /// <returns>The next key byte</returns>
        protected byte GetKeyValue(Stream keyStream)
        {
            int keyValue = 0;
            if (keyStream.Length > 0)
            {
                if ((keyValue = keyStream.ReadByte()) < 0)
                {
                    keyStream.Seek(0, SeekOrigin.Begin);
                    keyValue = keyStream.ReadByte();
                }
            }
            return (byte)keyValue;
        }

    }
}
