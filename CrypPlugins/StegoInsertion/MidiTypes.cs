/*
 Created: 29.07.2006
 Author: Corinna John

Copyright (C) 2006 SteganoDotNet Team

http://sourceforge.net/projects/steganodotnet

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
(current version) as published by the Free Software Foundation.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Cryptool.Plugins.StegoInsertion
{
	/// <summary>Header of a MIDI file (MThd).</summary>
	public struct MidiFileHeader
	{
		/// <summary>char[4] - must be "MThd" (beginning of file).</summary>
		public char[] HeaderType;
		///<summary>
		///Length of the header data - must be 6.
		///This value is an Int32 in Big Endian format (reverse byte order).
		///</summary>
		public byte[] DataLength;
		/// <summary>
		/// Format of the file
		/// 0 (one track)
		/// 1 (multiple simultaneous)
		/// 2 (multiple independent tracks)</summary>
		public Int16 FileType;
		/// <summary>Number of tracks.</summary>
		public Int16 CountTracks;
		/// <summary>Pulses Per Quarter Note.</summary>
		public Int16 Division;
	}

	/// <summary>Header of a MIDI track (MTrk).</summary>
	public struct MidiTrackHeader
	{
		/// <summary>char[4] - must be "MTrk" (beginning of track).</summary>
		public char[] HeaderType;
		///<summary>
		///Length in bytes of all messages in the track.
		///This value is stored in Big Endian format (reverse byte order).
		///</summary>
		public Int32 DataLength;
	}

	/// <summary>Time, Type and Data of an event.</summary>
	public struct MidiMessage
	{
		/// <summary>Delta time - variable-length field.</summary>
		public byte[] Time;
		/// <summary>//higher 4 bits type, lower 4 bits channel.</summary>
		public byte MessageType;
		/// <summary>
        /// One or two data bytes.
        /// SysEx (F0) messages can have more data bytes, but we don't need them.
        /// </summary>
		public byte[] MessageData;

		/// <summary>Creates a new message from a template message.</summary>
		/// <param name="template">Template for Time and Type.</param>
		/// <param name="messageData">Value for the data bytes.</param>
		public MidiMessage(MidiMessage template, byte[] messageData)
		{
			Time = template.Time;
			MessageType = template.MessageType;
			MessageData = messageData;
		}
	}
}
