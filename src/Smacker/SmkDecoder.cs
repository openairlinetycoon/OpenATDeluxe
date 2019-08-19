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

public class SmackerDecoder {
	public struct Color {
		public byte r;
		public byte g;
		public byte b;

		public static Color FromArgb(int a, int r, int g, int b) {
			Color c = new Color();
			c.r = (byte)r;
			c.g = (byte)g;
			c.b = (byte)b;

			return c;
		}
		public static Color FromArgb(int r, int g, int b) {
			Color c = new Color();
			c.r = (byte)r;
			c.g = (byte)g;
			c.b = (byte)b;

			return c;
		}
	}
	/// <summary>
	/// Creates a new decoder for the specified file
	/// </summary>
	/// <param name="file">the file to create a decoder for</param>
	internal SmackerDecoder(SmackerFile file) {
		File = file;
		lastAudioData = new byte[7][];
		lastFrameData = new byte[File.Header.Width * File.Header.Height];
	}

	// palette map used in Smacker
	byte[] smackerMap = new byte[] {
			0x00, 0x04, 0x08, 0x0C, 0x10, 0x14, 0x18, 0x1C,
			0x20, 0x24, 0x28, 0x2C, 0x30, 0x34, 0x38, 0x3C,
			0x41, 0x45, 0x49, 0x4D, 0x51, 0x55, 0x59, 0x5D,
			0x61, 0x65, 0x69, 0x6D, 0x71, 0x75, 0x79, 0x7D,
			0x82, 0x86, 0x8A, 0x8E, 0x92, 0x96, 0x9A, 0x9E,
			0xA2, 0xA6, 0xAA, 0xAE, 0xB2, 0xB6, 0xBA, 0xBE,
			0xC3, 0xC7, 0xCB, 0xCF, 0xD3, 0xD7, 0xDB, 0xDF,
			0xE3, 0xE7, 0xEB, 0xEF, 0xF3, 0xF7, 0xFB, 0xFF
		};

	// Runlength map (used in block decoding)
	uint[] sizetable = new uint[] {
			   1,    2,    3,    4,    5,    6,    7,    8,
			   9,   10,   11,   12,   13,   14,   15,   16,
			  17,   18,   19,   20,   21,   22,   23,   24,
			  25,   26,   27,   28,   29,   30,   31,   32,
			  33,   34,   35,   36,   37,   38,   39,   40,
			  41,   42,   43,   44,   45,   46,   47,   48,
			  49,   50,   51,   52,   53,   54,   55,   56,
			  57,   58,   59,  128,  256,  512, 1024, 2048
		};

	// Palette containts 256 byte triples
	Color[] CurrentPalette = new Color[256];

	//File being decoded
	private SmackerFile file;

	internal SmackerFile File {
		get { return file; }
		set { file = value; }
	}

	//Current Frame being decoded
	private int currentFrame;

	public int CurrentFrame {
		get { return currentFrame; }
		set { currentFrame = value; }
	}

	public void UnpackPalette() {
		Stream s = File.Stream;

		uint startPos = (uint)s.Position;
		int len = 4 * s.ReadByte();

		byte[] chunk = new byte[len];
		s.Read(chunk, 0, len);
		var p = 0;

		Color[] oldPalette = new Color[256];
		Array.Copy(CurrentPalette, oldPalette, 256);

		var pal = 0;

		int sz = 0;
		byte b0;
		while (sz < 256) {
			b0 = chunk[p++];
			if ((b0 & 0x80) != 0) {               // if top bit is 1 (0x80 = 10000000)
				sz += (b0 & 0x7f) + 1;     // get lower 7 bits + 1 (0x7f = 01111111)
				pal += ((b0 & 0x7f) + 1);
			} else if ((b0 & 0x40) != 0) {        // if top 2 bits are 01 (0x40 = 01000000)
				byte c = (byte)((b0 & 0x3f) + 1);  // get lower 6 bits + 1 (0x3f = 00111111)
				uint ps = (uint)(chunk[p++]);
				sz += c;

				while (c-- != 0) {
					CurrentPalette[pal].r = oldPalette[ps].r;
					CurrentPalette[pal].g = oldPalette[ps].g;
					CurrentPalette[pal].b = oldPalette[ps].b;
					ps++;
				}
			} else {                       // top 2 bits are 00
				sz++;
				// get the lower 6 bits for each component (0x3f = 00111111)
				byte r = (byte)(b0 & 0x3f);
				byte g = (byte)(chunk[p++] & 0x3f);
				byte b = (byte)(chunk[p++] & 0x3f);

				// upscale to full 8-bit color values. The Multimedia Wiki suggests
				// a lookup table for this, but this should produce the same result.
				CurrentPalette[pal].r = (byte)(r * 4 + r / 16);
				CurrentPalette[pal].g = (byte)(g * 4 + g / 16);
				CurrentPalette[pal].b = (byte)(b * 4 + b / 16);
			}
		}

		s.Seek(startPos + len, SeekOrigin.Begin);

	}

	private void UpdatePalette() {
		//UnpackPalette();
		//return;
		// System.Console.WriteLine("Updating palette");
		Stream s = File.Stream;
		Color[] OldPallette = (Color[])CurrentPalette.Clone();
		int size = (int)Util.ReadByte(s);
		//For some dark reason we need to mask out the lower two bits
		size = size * 4 - 1;
		if (size == -1)
			return;

		int sz = 0;
		long pos = s.Position + size;
		int palIndex = 0;
		int j;
		while (sz < 256) {
			int t = (int)Util.ReadByte(s);
			if ((t & 0x80) == 0x80) {
				/* skip palette entries */
				sz += (t & 0x7F) + 1;
				for (int i = 0; i < (t & 0x7F) + 1; i++) {
					//sz++;
					if (palIndex > CurrentPalette.Length - 1)
						break;
					CurrentPalette[palIndex++] = Color.FromArgb(255, 0, 0, 0);
				}
				//palIndex += ((t & 0x7F) + 1);
			} else if ((t & 0x40) == 0x40) {
				/* copy with offset */
				int off = ((int)Util.ReadByte(s));
				j = (t & 0x3F) + 1;
				while ((j-- != 0) && sz < 256) {
					if (palIndex > CurrentPalette.Length - 1 || off > OldPallette.Length - 1)
						break;
					CurrentPalette[palIndex++] = OldPallette[off];
					sz++;
					off++;
				}
			} else {
				/* new entries */
				CurrentPalette[palIndex++] = Color.FromArgb(smackerMap[t], smackerMap[(int)Util.ReadByte(s) & 0x3F], smackerMap[(int)Util.ReadByte(s) & 0x3F]);
				sz++;
			}
		}
		s.Seek(pos, SeekOrigin.Begin);
	}

	public uint GetIndex(uint x, uint y) {
		return x + (uint)File.Header.Width * y;
	}

	private byte[] lastFrameData;
	private byte[][] lastAudioData;

	/// <summary>
	/// Reads the next frame.
	/// </summary>            
	public void ReadNextFrame() {
		uint mask = 1;

		if (CurrentFrame >= File.Header.NbFrames) {
			Reset();
			return;
		}
		//throw new EndOfStreamException("No more frames");

		long currentPos = File.Stream.Position;

		//If this frame has a palette record
		if ((File.FrameTypes[CurrentFrame] & mask) > 0) {
			//Update the palette
			UpdatePalette();
		}

		//Sound data
		// mask <<= 1;
		// for (int i = 0; i < 7; i++, mask <<= 1) {
		// 	if ((file.FrameTypes[CurrentFrame] & mask) > 0) {
		// 		long pos = File.Stream.Position;
		// 		uint length = Util.ReadDWord(File.Stream);

		// 		//We assume compression, if not, well too bad
		// 		uint unpackedLength = Util.ReadDWord(File.Stream);
		// 		BitStream m = new BitStream(File.Stream);
		// 		if (m.ReadBits(1) != 0) //Audio present
		// 		{
		// 			bool stereo = m.ReadBits(1) > 0;
		// 			bool is16Bit = m.ReadBits(1) > 0;

		// 			//Next are some trees
		// 			uint nbTrees = 1;
		// 			if (stereo)
		// 				nbTrees <<= 1;
		// 			if (is16Bit)
		// 				nbTrees <<= 1;
		// 			Huffmantree[] tree = new Huffmantree[nbTrees];
		// 			byte[] audioData = new byte[unpackedLength + 4];
		// 			uint audioDataIndex = 0;
		// 			for (int k = 0; k < nbTrees; k++) {
		// 				tree[k] = new Huffmantree();
		// 				tree[k].BuildTree(m);
		// 			}

		// 			int res;
		// 			if (is16Bit) {
		// 				Int16 rightBaseMSB = 0, rightBaseLSB = 0, leftBaseMSB = 0, leftBaseLSB = 0;
		// 				rightBaseMSB = (Int16)(m.ReadBits(8));
		// 				rightBaseLSB = (Int16)(m.ReadBits(8));
		// 				//Add sample (little endian)
		// 				audioData[audioDataIndex++] = (byte)rightBaseLSB; //Lower byte
		// 				audioData[audioDataIndex++] = (byte)rightBaseMSB; //Higher byte
		// 				if (stereo) {
		// 					leftBaseMSB = (Int16)(m.ReadBits(8));
		// 					leftBaseLSB = (Int16)(m.ReadBits(8));
		// 					//Add sample (little endian)
		// 					audioData[audioDataIndex++] = (byte)leftBaseLSB; //Lower byte
		// 					audioData[audioDataIndex++] = (byte)leftBaseMSB; //Higher byte
		// 				}

		// 				for (int l = 0; l < unpackedLength / 2; l++) {
		// 					if ((l & ((stereo) ? 1 : 0)) > 0) {
		// 						res = tree[2].Decode(m);
		// 						leftBaseLSB += (Int16)res;
		// 						res = tree[3].Decode(m);
		// 						leftBaseMSB += (Int16)res;
		// 						leftBaseMSB += (Int16)(leftBaseLSB >> 8);
		// 						leftBaseLSB &= 0xFF;

		// 						//Add sample (little endian)
		// 						audioData[audioDataIndex++] = (byte)leftBaseLSB; //Lower byte
		// 						audioData[audioDataIndex++] = (byte)leftBaseMSB; //Higher byte
		// 					} else {
		// 						res = tree[0].Decode(m);
		// 						rightBaseLSB += (Int16)res;
		// 						res = tree[1].Decode(m);
		// 						rightBaseMSB += (Int16)res;
		// 						rightBaseMSB += (Int16)(rightBaseLSB >> 8);
		// 						rightBaseLSB &= 0xFF;

		// 						//Add sample (little endian)
		// 						audioData[audioDataIndex++] = (byte)rightBaseLSB; //Lower byte
		// 						audioData[audioDataIndex++] = (byte)rightBaseMSB; //Higher byte
		// 					}
		// 				}
		// 			} else {
		// 				byte rightBase = (byte)m.ReadBits(8), leftBase = 0;

		// 				//Add sample 
		// 				audioData[audioDataIndex++] = rightBase;

		// 				if (stereo) {
		// 					leftBase = (byte)m.ReadBits(8);
		// 					//Add sample 
		// 					audioData[audioDataIndex++] = leftBase;
		// 				}

		// 				for (int l = 0; l < unpackedLength; l++) {
		// 					if ((l & ((stereo) ? 1 : 0)) > 0) {
		// 						leftBase += (byte)tree[1].Decode(m);
		// 						//Add sample 
		// 						audioData[audioDataIndex++] = leftBase;
		// 					} else {
		// 						rightBase += (byte)tree[0].Decode(m);
		// 						//Add sample 
		// 						audioData[audioDataIndex++] = rightBase;
		// 					}
		// 				}
		// 			}
		// 			lastAudioData[i] = audioData;
		// 		}

		// 		File.Stream.Seek(pos + (long)length, SeekOrigin.Begin);
		// 	}
		// }

		//Video data
		try {
			DecodeVideo();
		} catch (IOException exc) {
			Console.WriteLine("Exception caught while decoding frame:" + exc.ToString());
		}

		//Seek to the next frame
		File.Stream.Seek(currentPos + File.FrameSizes[CurrentFrame], SeekOrigin.Begin);
		CurrentFrame++;
	}

	/// <summary>
	/// Returns the audiodata from the specified audiostream
	/// </summary>
	/// <param name="streamIndex">The index of the stream to return audio data for, should be between 0 and 7</param>
	/// <returns>PCM Audio data in a byte array</returns>
	public byte[] GetAudioData(int streamIndex) {
		return lastAudioData[streamIndex];
	}

	private void DecodeVideo() {
		uint x, y, mask, currentBlock = 0, runLength, colors, blockHeader, blockType = 0;
		uint posX, posY, index, pix, pix1, pix2, i, j;
		byte color, color1, color2;

		//Reset all huffman decoders
		File.MClr.ResetDecoder();
		File.MMap.ResetDecoder();
		File.Type.ResetDecoder();
		File.Full.ResetDecoder();

		//Allocate a new frame's data
		byte[] currentFrameData = new byte[File.Header.Width * File.Header.Height];
		BitStream m = new BitStream(File.Stream);

		uint nbBlocksX = File.Header.Width / 4;
		uint nbBlocksY = File.Header.Height / 4;
		uint nbBlocks = nbBlocksX * nbBlocksY;

		long runLengthNotComplete = 0;
		while (currentBlock < nbBlocks) {
			blockHeader = (uint)File.Type.Decode(m);
			runLength = sizetable[(blockHeader >> 2) & 0x3F];

			blockType = blockHeader & 3;
			//System.Console.Write("BLOCK " + currentBlock + " " + runLength + " ");

			switch (blockType) {
				case 2: //VOID BLOCK
						//System.Console.WriteLine("VOID - ");

					//Get block address
					for (i = 0; i < runLength && currentBlock < nbBlocks; i++) {
						//Get current block coordinates
						posX = 4 * (currentBlock % nbBlocksX);
						posY = 4 * (currentBlock / nbBlocksX);
						index = 0;
						for (x = 0; x < 4; x++) {
							for (y = 0; y < 4; y++) {
								index = GetIndex(posX + x, posY + y);
								currentFrameData[index] = lastFrameData[index];
							}
						}

						currentBlock++;
					}
					runLengthNotComplete = runLength - i;
					break;
				case 3: //SOLID BLOCK
						//System.Console.WriteLine("SOLID - ");
					color = (byte)(blockHeader >> 8);

					//Get block address
					for (i = 0; i < runLength && currentBlock < nbBlocks; i++) {
						//Get current block coordinates
						posX = 4 * (currentBlock % nbBlocksX);
						posY = 4 * (currentBlock / nbBlocksX);
						for (x = 0; x < 4; x++) {
							for (y = 0; y < 4; y++) {
								currentFrameData[GetIndex(posX + x, posY + y)] = color;
							}
						}

						currentBlock++;
					}
					runLengthNotComplete = runLength - i;
					break;
				case 0: //MONO BLOCK
						//System.Console.WriteLine("MONO - ");
					for (i = 0; i < runLength && currentBlock < nbBlocks; i++) {
						colors = (uint)File.MClr.Decode(m);
						color1 = (byte)(colors >> 8);
						color2 = (byte)(colors & 0xFF);

						mask = (uint)File.MMap.Decode(m);
						posX = (currentBlock % nbBlocksX) * 4;
						posY = (currentBlock / nbBlocksX) * 4;
						for (y = 0; y < 4; y++) {
							if ((mask & 1) > 0) {
								currentFrameData[GetIndex(posX, posY + y)] = color1;
							} else {
								currentFrameData[GetIndex(posX, posY + y)] = color2;
							}
							if ((mask & 2) > 0) {
								currentFrameData[GetIndex(posX + 1, posY + y)] = color1;
							} else {
								currentFrameData[GetIndex(posX + 1, posY + y)] = color2;
							}
							if ((mask & 4) > 0) {
								currentFrameData[GetIndex(posX + 2, posY + y)] = color1;
							} else {
								currentFrameData[GetIndex(posX + 2, posY + y)] = color2;
							}
							if ((mask & 8) > 0) {
								currentFrameData[GetIndex(posX + 3, posY + y)] = color1;
							} else {
								currentFrameData[GetIndex(posX + 3, posY + y)] = color2;
							}

							mask >>= 4;
						}
						currentBlock++;
					}
					runLengthNotComplete = runLength - i;
					//Console.WriteLine(runLengthNotComplete);
					break;
				case 1:
					//System.Console.Write("FULL - ");
					int mode = 0;
					if (File.IsV4) {
						int type = m.ReadBits(1);

						if (type == 0) {
							int abit = m.ReadBits(1);
							if (abit == 1)
								mode = 2;

						} else
							mode = 1;
					}

					switch (mode) {
						case 0://v2 Full block
							   //System.Console.WriteLine(" v2 0");

							for (i = 0; i < runLength && currentBlock < nbBlocks; i++) {
								posX = (currentBlock % nbBlocksX) * 4;
								posY = (currentBlock / nbBlocksX) * 4;
								for (y = 0; y < 4; y++) {
									colors = (uint)File.Full.Decode(m);
									color1 = (byte)(colors >> 8);
									color2 = (byte)(colors & 0xFF);

									currentFrameData[GetIndex(posX + 3, posY + y)] = color1;
									currentFrameData[GetIndex(posX + 2, posY + y)] = color2;


									colors = (uint)File.Full.Decode(m);
									color1 = (byte)(colors >> 8);
									color2 = (byte)(colors & 0xFF);
									currentFrameData[GetIndex(posX + 1, posY + y)] = color1;
									currentFrameData[GetIndex(posX + 0, posY + y)] = color2;

								}
								currentBlock++;
							}
							break;
						case 1:
							//System.Console.WriteLine(" v2 1  AHHHHHHHHHHHHHHHHHHHHHHHH");
							for (i = 0; i < runLength && currentBlock < nbBlocks; i++) {
								posX = (currentBlock % nbBlocksX) * 4;
								posY = (currentBlock / nbBlocksX) * 4;
								pix = (uint)File.Full.Decode(m);

								color1 = (byte)(pix >> 8);
								color2 = (byte)(pix & 0xFF);

								currentFrameData[GetIndex(posX + 0, posY + 0)] = color2;
								currentFrameData[GetIndex(posX + 1, posY + 0)] = color2;
								currentFrameData[GetIndex(posX + 2, posY + 0)] = color1;
								currentFrameData[GetIndex(posX + 3, posY + 0)] = color1;
								currentFrameData[GetIndex(posX + 0, posY + 1)] = color2;
								currentFrameData[GetIndex(posX + 1, posY + 1)] = color2;
								currentFrameData[GetIndex(posX + 2, posY + 1)] = color1;
								currentFrameData[GetIndex(posX + 3, posY + 1)] = color1;

								pix = (uint)File.Full.Decode(m);

								color1 = (byte)(pix >> 8);
								color2 = (byte)(pix & 0xFF);

								currentFrameData[GetIndex(posX + 0, posY + 2)] = color2;
								currentFrameData[GetIndex(posX + 1, posY + 2)] = color2;
								currentFrameData[GetIndex(posX + 2, posY + 2)] = color1;
								currentFrameData[GetIndex(posX + 3, posY + 2)] = color1;
								currentFrameData[GetIndex(posX + 0, posY + 3)] = color2;
								currentFrameData[GetIndex(posX + 1, posY + 3)] = color2;
								currentFrameData[GetIndex(posX + 2, posY + 3)] = color1;
								currentFrameData[GetIndex(posX + 3, posY + 3)] = color1;

								currentBlock++;
							}
							runLengthNotComplete = runLength - i;
							//Console.WriteLine(runLengthNotComplete);
							break;
						case 2:
							System.Console.WriteLine(" v2 2 AHHHHHHHHHHHHHHHHHHHHHHHH");
							for (j = 0; j < runLength && currentBlock < nbBlocks; j++) {
								posX = (currentBlock % nbBlocksX) << 2;
								posY = (currentBlock / nbBlocksX) << 2;
								for (i = 0; i < 2; i++) {

									pix1 = (uint)File.Full.Decode(m);
									pix2 = (uint)File.Full.Decode(m);


									color1 = (byte)(pix1 >> 8);
									color2 = (byte)(pix1 & 0xFF);

									currentFrameData[GetIndex(posX + 2, posY + (i << 1))] = color2;
									currentFrameData[GetIndex(posX + 3, posY + (i << 1))] = color1;
									currentFrameData[GetIndex(posX + 2, posY + (i << 1) + 1)] = color2;
									currentFrameData[GetIndex(posX + 3, posY + (i << 1) + 1)] = color1;


									color1 = (byte)(pix1 >> 8);
									color2 = (byte)(pix1 & 0xFF);


									currentFrameData[GetIndex(posX + 0, posY + (i << 1))] = color2;
									currentFrameData[GetIndex(posX + 1, posY + (i << 1))] = color1;
									currentFrameData[GetIndex(posX + 0, posY + (i << 1) + 1)] = color2;
									currentFrameData[GetIndex(posX + 1, posY + (i << 1) + 1)] = color1;


								}
								currentBlock++;
							}
							runLengthNotComplete = runLength - j;
							Console.WriteLine(runLengthNotComplete);
							break;
						default:
							System.Console.WriteLine(" DEFAULT");
							break;
					}

					break;
			}
		}

		// if (runLengthNotComplete > 0) {
		// 	Console.WriteLine("Warning: frame ended before runlength has reached zero");
		// }

		lastFrameData = currentFrameData;
	}

	/// <summary>
	/// Encapsulates the video data from the last decoded frame in a System.Drawing.Bitmap
	/// </summary>
	/// <returns>A System.Drawing.Bitmap</returns>
	// public Bitmap GetVideoDataBitmap()
	// {
	//     Bitmap bmp = new Bitmap((int)File.Header.Width, (int)File.Header.Height, PixelFormat.Format8bppIndexed);

	//     ColorPalette pal = GetColorPalette(256);
	//     Array.Copy(CurrentPalette, pal.Entries, 256);
	//     bmp.Palette = pal;

	//     BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
	//     System.Runtime.InteropServices.Marshal.Copy(lastFrameData, 0, data.Scan0, lastFrameData.Length);
	//     bmp.UnlockBits(data);
	//     return bmp;
	// }

	/// <summary>
	/// Property containing the raw video data 
	/// For each pixel there is a byte specifying an index in the palette
	/// </summary>
	public byte[] VideoData {
		get {
			return lastFrameData;
		}
	}

	/// <summary>
	/// The pallette to use to render the frame
	/// </summary>
	public Color[] Palette {
		get {
			return CurrentPalette;
		}
	}

	/// <summary>
	/// Returns the video data as RGB data
	/// </summary>
	public byte[] RGBData {
		get {
			byte[] result = new byte[lastFrameData.Length * 3];
			int j = 0;
			for (int i = 0; i < lastFrameData.Length; i++) {
				//if (CurrentPalette[lastFrameData[i]] == null)
				//	continue;
				j = i * 3;
				result[j] = CurrentPalette[lastFrameData[i]].r;
				result[j + 1] = CurrentPalette[lastFrameData[i]].g;
				result[j + 2] = CurrentPalette[lastFrameData[i]].b;

			}
			return result;
		}
	}
	public byte[] TESTData {
		get {
			byte[] result = new byte[lastFrameData.Length * 3];
			int j = 0;
			for (int i = 0; i < lastFrameData.Length; i++) {
				//Console.WriteLine(lastFrameData[i]);
				//if (CurrentPalette[lastFrameData[i]] == null)
				//	continue;
				//Console.WriteLine("NOT FOUND!");
				j = i * 3;
				result[j] = lastFrameData[i];
				result[j + 1] = lastFrameData[i];
				result[j + 2] = lastFrameData[i];

			}
			return result;
		}
	}
	/// <summary>
	/// Returns the video data as RGBA data
	/// </summary>
	public byte[] RGBAData {
		get {
			byte[] result = new byte[lastFrameData.Length * 4];
			int j = 0;
			for (int i = 0; i < lastFrameData.Length; i++) {
				//if (CurrentPalette[lastFrameData[i]] == null)
				//	continue;
				j = i * 4;
				result[j] = CurrentPalette[lastFrameData[i]].r;
				result[j + 1] = CurrentPalette[lastFrameData[i]].g;
				result[j + 2] = CurrentPalette[lastFrameData[i]].b;
				result[j + 3] = (byte)(CurrentPalette[lastFrameData[i]].g == 255 && CurrentPalette[lastFrameData[i]].b == 0 ? 0 : 255);

			}
			return result;
		}
	}

	/// <summary>
	/// Returns the video data as BGRA data
	/// </summary>
	public byte[] BGRAData {
		get {
			byte[] result = new byte[lastFrameData.Length * 4];
			int j = 0;
			for (int i = 0; i < lastFrameData.Length; i++) {
				j = i * 4;
				result[j] = CurrentPalette[lastFrameData[i]].b;
				result[j + 1] = CurrentPalette[lastFrameData[i]].g;
				result[j + 2] = CurrentPalette[lastFrameData[i]].r;
				if (result[j] != 0 || result[j + 1] != 0 || result[j + 2] != 0)
					result[j + 3] = 0xFF;
			}
			return result;
		}
	}

	/// <summary>
	/// Hack as described on MSDN to get a clean palette
	/// </summary>
	/// <param name="nColors">the number of colors the palette should contain (between 2-256)</param>
	/// <returns>An empty, clean ColorPalette structure, ready to be written to</returns>
	// protected ColorPalette GetColorPalette(uint nColors)
	// {
	//     // Assume monochrome image.
	//     PixelFormat bitscolordepth = PixelFormat.Format1bppIndexed;
	//     ColorPalette palette;    // The Palette we are stealing
	//     Bitmap bitmap;     // The source of the stolen palette

	//     // Determine number of colors.
	//     if (nColors > 2)
	//         bitscolordepth = PixelFormat.Format4bppIndexed;
	//     if (nColors > 16)
	//         bitscolordepth = PixelFormat.Format8bppIndexed;

	//     // Make a new Bitmap object to get its Palette.
	//     bitmap = new Bitmap(1, 1, bitscolordepth);

	//     palette = bitmap.Palette;   // Grab the palette

	//     bitmap.Dispose();           // cleanup the source Bitmap

	//     return palette;             // Send the palette back
	// }

	bool firstTime = true; //Indicates whether the animation is decoded for the first time

	/// <summary>
	/// Resets the decoder to the first frame, if there is a ring frame the first frame is skipped as it should.
	/// </summary>
	public void Reset() {
		uint nbFrames = file.Header.NbFrames;

		if (file.Header.HasRingFrame())
			nbFrames++;

		//Seek to the beginning of the frame data section
		//Header = 104 bytes, 5 bytes per frame (one dword + one byte) + trees
		int pos = (int)(104 + 5 * nbFrames + file.Header.TreesSize);
		file.Stream.Seek(pos, SeekOrigin.Begin);
		CurrentFrame = 0;

		//The ring frame replace the first frame on the second+ run.
		if (!firstTime && file.Header.HasRingFrame()) {
			//Seek ahead 1 frame
			file.Stream.Seek(file.FrameSizes[0], SeekOrigin.Current);
			CurrentFrame = 1;
		}

		firstTime = false;
	}

	/// <summary>
	/// Set the decoder to decode the specified frame next
	/// </summary>
	/// <param name="i">the index of the next frame the decoder should decode </param>
	public void SeekTo(int i) {
		Reset(); //Seek to frame 0
		if (i >= File.Header.NbFrames) throw new IndexOutOfRangeException("Not a valid frame number!");
		uint total = 0;
		for (int j = 0; j < i; j++) {
			total += File.FrameSizes[j];
		}
		CurrentFrame = i;
		file.Stream.Seek(total, SeekOrigin.Current);
	}
}
