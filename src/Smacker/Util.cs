//
// SCSharp.Mpq.Smk
//
// Authors:
//	Chris Toshok (toshok@gmail.com)
//
// Copyright 2006-2010 Chris Toshok
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;

public class Util {
	// read in a LE word
	public static ushort ReadWord(Stream fs) {
		return ((ushort)(fs.ReadByte() | (fs.ReadByte() << 8)));
	}
	public static ushort ReadWord(byte[] buf, int position) {
		return ((ushort)((int)buf[position] | (int)buf[position + 1] << 8));
	}

	// read in a LE doubleword
	public static uint ReadDWord(Stream fs) {
		return (uint)(fs.ReadByte() | (fs.ReadByte() << 8) | (fs.ReadByte() << 16) | (fs.ReadByte() << 24));
	}
	public static uint ReadDWord(byte[] buf, int position) {
		return ((uint)((uint)buf[position] | (uint)buf[position + 1] << 8 | (uint)buf[position + 2] << 16 | (uint)buf[position + 3] << 24));
	}

	// read in a byte
	public static byte ReadByte(Stream fs) {
		return (byte)fs.ReadByte();
	}

	public static UInt32 MakeTag(params char[] chars) {
		if (chars.Length != 4)
			throw new ArgumentException("We need 4 chars");
		return (uint)(chars[0] | (chars[1] << 8) | (chars[2] << 16) | (chars[3] << 24));
	}
}