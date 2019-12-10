using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using Godot;

public class GFXLibrary {

	public static string pathToAirlineTycoonD = "P:/Projekte/Major Games/BASE/ATD";//@"..\BASE\ATD";

	[Export]
	public string _pathToAirlineTycoonD {
		set { pathToAirlineTycoonD = value; }
		get { return pathToAirlineTycoonD; }
	}

	public class GFXFile {
		public GFXFile(GFXLibrary parentLib) {
			parent = parentLib;
		}

		GFXLibrary parent;

		//The name of the file given from the ATD file
		public string name;

		//The id of the file given from the ATD file
		public long typeID;

		public int isGFX;

		//The position offset of the file in the original GFX file
		public int libraryOffset;

		Texture texture;

		public Texture GetTexture() {

			if (texture == null) {
				texture = parent.CreateTextureFromFile(this);
			}

			return texture;
		}
	}
	public string name; //Name of the GFXFile from ATD
	public long archiveSize; //How big is the GFXLibrary size
	public long filesInLibrary; //How many files are in this GFXLibrary
	public long dataSize; // I dunno

	public List<GFXFile> files = new List<GFXFile>(); //A list with all GFX files inside the given GFX Library file
	string pathToGFXFile;

	public const int GFXHeaderSize = 67; //Bytes - Size of the GFXLibrary file Header
	public const int FileHeaderSize = 17; //Bytes - Size of an individual File Header


	public GFXLibrary(string _pathToGFXFile) //Load a gl***.gli file from the GLI folder in ATD - specify the full path to the file (relative or absolute)!
	{
		pathToGFXFile = _pathToGFXFile;
		name = System.IO.Path.GetFileNameWithoutExtension(pathToGFXFile);
	}



	public void GetFilesInLibrary() {
		File f = new File();
		try {
			f.Open(pathToGFXFile, (int)File.ModeFlags.Read);
			//GD.Print("Reading: " + pathToGFXFile);
			ReadHeader(f);
			ReadFileHeaders(f);
		} catch (Exception e) {
			GD.Print(e.Message);
			GD.Print(e.StackTrace);
		} finally {
			f.Close();
		}
	}

	File handle;

	public void Open() {
		handle = new File();
		Error e = handle.Open(pathToGFXFile, (int)File.ModeFlags.Read);
	}

	public void Close() {
		handle.Close();
		handle.Dispose();
	}

	public Texture CreateTextureFromFile(GFXFile file) {
		Image image = new Image();

		bool disposeOfHandle = false;

		if (handle?.IsOpen() != true) {
			Open();
			disposeOfHandle = true;
		}

		handle.Seek(file.libraryOffset); // Go to the position of the gfx file in the library file

		int a = handle.Get32(); //skip unused data - 76 in glbasis.gli
		int fileSize = handle.Get32(); //size of file in bytes

		int width = handle.Get32(); //Get image width in pixel
		int height = handle.Get32(); //Get image height in pixel

		byte[] colors = new byte[fileSize / 2 * 2];

		handle.Seek(file.libraryOffset + 76); //Skip unneeded values

		byte[] readColors = handle.GetBuffer(fileSize);

		int c = 0;
		for (int i = 0; i + 1 < fileSize; i += 2) {
			int color = readColors[i] + (readColors[i + 1] << 8);//handle.Get16(); //Get a RGB565 color value

			int r, g, b;

			r = ((((color >> 11) & 0x1F) * 527) + 23) >> 6;
			g = ((((color >> 5) & 0x3F) * 259) + 33) >> 6;
			b = (((color & 0x1F) * 527) + 23) >> 6;

			int alpha = 255;

			int alphaCutoff = 1;

			if (r + b + g < alphaCutoff) //TODO Better transparency check needed! 
				alpha = 0;

			//Convert color to a RGB4444 value
			colors[c] = (byte)((r & 0xF) << 4 | (g & 0xF)); //save
			colors[c + 1] = (byte)((b & 0xF) << 4 | (alpha & 0xF)); //save

			c += 2;
		}
		if (colors.Length == 0) {
			if (disposeOfHandle)
				handle.Close();
			//GD.PrintErr("Empty Texture! GFX: " + file.name);
			return null;
		}
		if (colors.Length != width * height * 4) {
			if (disposeOfHandle)
				handle.Close();
			//GD.PrintErr("Wrong Texture Size! GFX: " + file.name);
			return null;
		}

		image.CreateFromData(width, height, false, Image.Format.Rgba4444, colors);

		ImageTexture texture = new ImageTexture();
		texture.Create(width, height, Image.Format.Rgba4444, 0);
		texture.SetData(image);
		texture.Flags = (int)Texture.FlagsEnum.Filter;

		image.Dispose();
		if (disposeOfHandle)
			handle.Close();

		return texture;
	}

	private void ReadFileHeaders(File f) {
		for (int i = 0; i < filesInLibrary; i++) {
			f.Seek(GFXHeaderSize + i * FileHeaderSize); //Go to the current file header position

			GFXFile gfx = new GFXFile(this);

			gfx.typeID = f.Get32();
			gfx.isGFX = f.Get8();

			if (gfx.isGFX == 0) //Is the file declared as GFX file? There can be text stored
				continue;

			gfx.name = Encoding.UTF8.GetString(f.GetBuffer(8)); //Read the file name;
			gfx.libraryOffset = f.Get32();



			files.Add(gfx);
		}
	}

	private void ReadHeader(File f) {
		string magic = Encoding.UTF8.GetString(f.GetBuffer(5)); //Read the header name;

		if (magic != "GLIB2")
			throw new NotSupportedException("Can't handle non GLIB2 files! " + name);

		f.Seek(f.GetPosition() + 5); //Skip unkown values

		archiveSize = f.Get32();
		f.Seek(f.GetPosition() + 20); //Skip unkown values

		filesInLibrary = f.Get32() - 1; //There is always one less file than written
		if (filesInLibrary == -1) {
			filesInLibrary = 0;
			// in which cases is this possible ?
		}

		f.Seek(f.GetPosition() + 29);//Skip unkown values

		dataSize = f.Get32();
	}
}