using Godot;
using System;
using System.Text;
using BinaryWriter = System.IO.BinaryWriter;
using MemoryStream = System.IO.MemoryStream;

public class BaseFileDecoder : ATFile {
	protected string fileData;

	public const string xtRLEMagic = "xtRLE";
	protected const int KEY_ONE_XTRLE = 0xa5;
	protected const int KEY_TWO_XTRLE = 0x00;
	protected static Encoding BaseEncoding => Encoding.GetEncoding(1252);

	public enum DecodingMethod {
		None,
		xtRLE,
	}

	public BaseFileDecoder(string _filePath) : base(_filePath) {
		File f = new File();
		Error e = f.Open(filePath, File.ModeFlags.Read);

		if (e != Error.Ok) {
			throw new ArgumentException("Error opening file: " + filePath + " - Error " + e.ToString());
		}

		byte[] data = ReadFile(f);
		fileData = BaseEncoding.GetString(data);
	}

	/// <summary>
	/// Encrypts the data of the file according to the header of the file. 
	/// </summary>
	/// <param name="fileIn">An already opened file</param>
	/// <returns>Returns the decrypted file ready to be filled in a document.</returns>
	public byte[] ReadFile(File fileIn) {
		DecodingMethod method = IsXTRLE(fileIn);

		byte[] data = null;

		switch (method) {
			case (DecodingMethod.None):
				data = fileIn.GetBuffer((int)fileIn.GetLen());
				break;
			case (DecodingMethod.xtRLE):
				data = DecodeXTRLE(fileIn);
				break;
		}

		return data;
	}

	/// <summary>
	/// Checks the header of the file for xtRLE
	/// </summary>
	/// <param name="fileIn">An already opened file</param>
	/// <returns>If file is an xtRLE file: DecodingMethod.xtRLE; if not DecodingMethod.None</returns>
	private static DecodingMethod IsXTRLE(File fileIn) {
		string magic = Encoding.UTF8.GetString(fileIn.GetBuffer(xtRLEMagic.Length));
		DecodingMethod method = (xtRLEMagic == magic) ? DecodingMethod.xtRLE : DecodingMethod.None;
		fileIn.Seek(0); //Go back to the start of the file
		return method;
	}

	//
	/// <summary>
	/// Decodes an xtRLE file
	/// Checks the header of the file for xtRLE
	/// </summary>
	/// <param name="fileIn">An already opened file</param>
	/// <returns>Returns the decrypted xtRLE file in a byte[]</returns>
	private byte[] DecodeXTRLE(File fileIn) {
		byte[] data;

		fileIn.Seek(10); //Skip header
		int finalSize = (int)fileIn.Get32(); //Get file size

		data = null;

		using (MemoryStream ms = new MemoryStream())
		using (BinaryWriter bw = new BinaryWriter(ms)) { //Easy way to write the file into a byte[]
			uint num;//How far we should skip / how many times we should repeat the byte

			while (fileIn.GetError() != Error.FileEof) {
				byte command = (byte)fileIn.Get8(); //Get the next command
				num = (uint)(command & 0b0111_1111);//0b0111_1111;

				if ((command & 0b1000_0000) == 0b1000_0000) { //Copy data command
					byte[] bytes = fileIn.GetBuffer(checked((int)num)); //Get the data
					for (int i = 0; i < bytes.Length; i++) {
						bytes[i] ^= KEY_ONE_XTRLE; //Decrypt it with KEY_ONE
					}

					bw.Write(bytes); //Save it to the stream
				} else if ((command & 0b1000_0000) == 0b0000_0000) { //Repeat the following bytes "num" times
					byte repeatingData = (byte)fileIn.Get8(); //What Byte to repeat

					repeatingData ^= KEY_TWO_XTRLE; //Decrypt it with KEY_TWO

					for (int i = 0; i < num; i++) //Repeat and save
						bw.Write(repeatingData);
				}
			}

			data = ms.ToArray(); //Save to array
		}

		return data;
	}
}
