using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Drawing;


public class SmackerFile {
	private SmackerHeader header;

	public SmackerHeader Header {
		get { return header; }
		set { header = value; }
	}

	private UInt32[] frameSizes;

	public UInt32[] FrameSizes {
		get { return frameSizes; }
		set { frameSizes = value; }
	}

	private byte[] frameTypes;

	public byte[] FrameTypes {
		get { return frameTypes; }
		set { frameTypes = value; }
	}

	private bool isV4;

	public bool IsV4 {
		get { return isV4; }
		set { isV4 = value; }
	}

	BigHuffmanTree mMap, mClr, full, type;

	public BigHuffmanTree Type {
		get { return type; }
		set { type = value; }
	}

	public BigHuffmanTree Full {
		get { return full; }
		set { full = value; }
	}

	public BigHuffmanTree MClr {
		get { return mClr; }
		set { mClr = value; }
	}

	public BigHuffmanTree MMap {
		get { return mMap; }
		set { mMap = value; }
	}

	private Stream stream;

	public Stream Stream {
		get { return stream; }
		set { stream = value; }
	}

	SmackerFile() {
	}

	private static SmackerHeader ReadHeader(Stream s) {
		SmackerHeader smk = new SmackerHeader();
		int i;

		/* read and check header */
		smk.Signature = Util.ReadDWord(s);
		if (smk.Signature != Util.MakeTag('S', 'M', 'K', '2') && smk.Signature != Util.MakeTag('S', 'M', 'K', '4'))
			throw new InvalidDataException("Not an SMK stream");

		smk.Width = Util.ReadDWord(s);
		smk.Height = Util.ReadDWord(s);
		smk.NbFrames = Util.ReadDWord(s);
		smk.Pts_Inc = (int)Util.ReadDWord(s);
		smk.Fps = CalcFps(smk);
		smk.Flags = Util.ReadDWord(s);
		for (i = 0; i < 7; i++)
			smk.AudioSize[i] = Util.ReadDWord(s);
		smk.TreesSize = Util.ReadDWord(s);
		smk.NMap_Size = Util.ReadDWord(s);
		smk.MClr_Size = Util.ReadDWord(s);
		smk.Full_Size = Util.ReadDWord(s);
		smk.Type_Size = Util.ReadDWord(s);
		for (i = 0; i < 7; i++)
			smk.AudioRate[i] = Util.ReadDWord(s); ;
		smk.Dummy = Util.ReadDWord(s);

		/* setup data */
		if (smk.NbFrames > 0xFFFFFF)
			throw new InvalidDataException("Too many frames: " + smk.NbFrames);

		return smk;
	}

	private static double CalcFps(SmackerHeader smk) {
		if ((int)smk.Pts_Inc > 0)
			return 1000.0 / (int)smk.Pts_Inc;
		else if ((int)smk.Pts_Inc < 0)
			return 100000.0 / (-(int)smk.Pts_Inc);
		else
			return 10.0;
	}

	public static SmackerFile OpenFromStream(Stream s) {
		int i;
		SmackerFile file = new SmackerFile();

		file.Header = ReadHeader(s);

		uint nbFrames = file.Header.NbFrames;
		//The ring frame is not counted!
		if (file.Header.HasRingFrame()) nbFrames++;

		file.FrameSizes = new UInt32[nbFrames];
		file.FrameTypes = new byte[nbFrames];

		file.IsV4 = (file.Header.Signature != Util.MakeTag('S', 'M', 'K', '2'));

		/* read frame info */

		for (i = 0; i < nbFrames; i++) {
			file.FrameSizes[i] = Util.ReadDWord(s);
		}
		for (i = 0; i < nbFrames; i++) {
			file.FrameTypes[i] = Util.ReadByte(s);
		}
		//The rest of the header is a bitstream
		BitStream m = new BitStream(s);

		//Read huffman trees

		//MMap 
		// System.Console.WriteLine("Mono map tree");
		file.MMap = new BigHuffmanTree();
		file.MMap.BuildTree(m);
		//MClr (color map)
		//  System.Console.WriteLine("Mono Color tree");
		file.MClr = new BigHuffmanTree();
		file.MClr.BuildTree(m);
		//Full (full block stuff)
		// System.Console.WriteLine("Full tree");
		file.Full = new BigHuffmanTree();
		file.Full.BuildTree(m);
		//Type (full block stuff)
		// System.Console.WriteLine("Type descriptor tree");
		file.Type = new BigHuffmanTree();
		file.Type.BuildTree(m);

		//We are ready to decode frames

		file.Stream = s;
		return file;
	}

	/// <summary>
	/// Returns a decoder for this Smackerfile
	/// </summary>
	public SmackerDecoder Decoder {
		get {
			return new SmackerDecoder(this);
		}
	}
}

public class SmackerHeader {
	/* Smacker file header */
	public UInt32 Signature;
	public UInt32 Width, Height;
	public UInt32 NbFrames;
	public int Pts_Inc;
	public double Fps;
	public UInt32 Flags;
	public UInt32[] AudioSize = new UInt32[7];
	public UInt32 TreesSize;
	public UInt32 NMap_Size, MClr_Size, Full_Size, Type_Size;
	public UInt32[] AudioRate = new UInt32[7];
	public UInt32 Dummy;

	/// <summary>
	/// Returns the sample rate for the specified audio track
	/// </summary>
	/// <param name="i">the audio track to return the sample rate for</param>
	/// <returns>The smaple rate for track i</returns>
	public int GetSampleRate(int i) {
		return (int)(AudioRate[i] & 0xFFFFFF); //Mask out the upper byte
	}

	public bool IsStereoTrack(int i) {
		return ((AudioRate[i] >> 24) & 16) > 0;
	}

	public bool Is16BitTrack(int i) {
		return ((AudioRate[i] >> 24) & 32) > 0;
	}

	public bool IsCompressedTrack(int i) {
		return ((AudioRate[i] >> 24) & 128) > 0;
	}

	public bool HasRingFrame() {
		return (Flags & 1) > 0;
	}

	public bool IsYInterlaced() {
		return (Flags & 2) > 0;
	}

	public bool IsYDoubled() {
		return (Flags & 4) > 0;
	}
}
