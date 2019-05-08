using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using Godot;

public class GFXLibrary {
    public class GFXFile {
        //The name of the file given from the ATD file
        public string name;

        //The id of the file given from the ATD file
        public long typeID;

        public int isGFX;

        //The position offset of the file in the original GFX file
        public int libraryOffset;
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
    }


    public void GetFilesInLibrary()
    {
        File f = new File();
        f.Open(pathToGFXFile, (int)File.ModeFlags.Read);

        ReadHeader(f);

        ReadFileHeaders(f);

        if (files.Count > 0) {
            CreateTextureFromFile (files[0]);
        }
    }

    public Texture CreateTextureFromFile(GFXFile file){
        Image image = new Image();

        File f = new File();
        f.Open(pathToGFXFile, (int)File.ModeFlags.Read);

        f.Seek(file.libraryOffset); // Go to the position of the gfx file in the library file

        int a = f.Get32(); //skip unused data - 76 in glbasis.gli
        int fileSize = f.Get32(); //size of file in bytes

        int width = f.Get32(); //Get image width in pixel
        int height = f.Get32(); //Get image height in pixel

        byte[] colors = new byte[fileSize/2 * 4];

        f.Seek(file.libraryOffset + 76); //Skip unneeded values

        int c = 0;
        for(int i = 0; i < fileSize/2; i++){
            int color = f.Get16(); //Get a RGB565 color value


            //Convert it to a RGB888 value
            int r = ((color >> 11) & 0x1F);
            int g = ((color >> 5) & 0x3F);
            int b = (color & 0x1F);

            r = ((((color >> 11) & 0x1F) * 527) + 23) >> 6;
            g = ((((color >> 5) & 0x3F) * 259) + 33) >> 6;
            b = (((color & 0x1F) * 527) + 23) >> 6;

            

            colors[c] = (byte)r; //save
            colors[c+1] = (byte)g; //save
            colors[c+2] = (byte)b; //save
            
            colors[c+3] = 255;

            if(r == 0 && g == 0 && b == 0) //TODO Better transparency check needed! 
                colors[c+3] = 0;
             //save
            c += 4;
        }

        image.CreateFromData(width, height, false, Image.Format.Rgba8, colors);

        ImageTexture texture = new ImageTexture();
        texture.CreateFromImage(image);

        return texture;
    }

    private void ReadFileHeaders(File f)
    {
        for (int i = 0; i < filesInLibrary; i++)
        {
            f.Seek(GFXHeaderSize + i * FileHeaderSize); //Go to the current file header position

            GFXFile gfx = new GFXFile();

            gfx.typeID = f.Get32();
            gfx.isGFX = f.Get8();

            if (gfx.isGFX == 0) //Is the file declared as GFX file? There can be text stored
                continue;

            gfx.name = Encoding.UTF8.GetString(f.GetBuffer(8)); //Read the file name;
            gfx.libraryOffset = f.Get32();



            files.Add(gfx);
        }
    }

    private void ReadHeader(File f)
    {

        name = Encoding.UTF8.GetString(f.GetBuffer(5)); //Read the header name;

        f.Seek(f.GetPosition() + 5); //Skip unkown values

        archiveSize = f.Get32();
        f.Seek(f.GetPosition() + 20); //Skip unkown values

        filesInLibrary = f.Get32 () - 1; //There is always one less file than written
        if (filesInLibrary == -1) {
            filesInLibrary = 0;
            // in which cases is this possible ?
        }

        f.Seek(f.GetPosition() + 29);//Skip unkown values

        dataSize = f.Get32();
    }

    /*
#define GFXHEADERSIZE 76
#define DIMENSIONPOS 8
#define FOOTERSIZE 14
#define HEADERSIZE sizeof(dataHeader)
#define FILEHEADERSIZE 17

#pragma pack(push, 1)
struct dataHeader { //68 Bytes
	char header[5];
	BYTE unknown[5];
	__int32 archiveSize;
	BYTE unknown2[20];
	__int32 files;
	__int32 info;//54
	BYTE unknown3[21];
	__int32 dataSize;
};
#pragma pack(pop)

#pragma pack(push, 1)
struct fileHeader {
	__int32 typeID;
	bool isGFX;
	char filename[8];
	__int32 offset;
};
#pragma pack(pop)

    void GFXLib::readData() {
        FILE *file;
        fopen_s(&file, fileName.c_str(), "rb");

        long newHeaderPointer = 0;

        dataHeader header;
        memset(&header, 0, sizeof(dataHeader));

        fread_s(&header, sizeof(header), sizeof(header), 1, file);


        fileHeader headerFile;
        for (int i = 0; i < header.files - 1; i++) {
            fseek(file, HEADERSIZE + i * FILEHEADERSIZE, 0);


            memset(&headerFile, 0, FILEHEADERSIZE);

            int s = FILEHEADERSIZE;

            int oldHeader = ftell(file);
            int d = fread_s(&headerFile, FILEHEADERSIZE, FILEHEADERSIZE, 1, file);
            newHeaderPointer = ftell(file);

            int e = errno;
            int ea = ferror(file);
            int ef = feof(file);

            GFXFile gfxFile = GFXFile();
            gfxFile.name = headerFile.filename;
            gfxFile.fileOffset = headerFile.offset;

            files.insert(pair<string, GFXFile>(gfxFile.name, gfxFile));

            //readGfxData(file, headerFile.offset);
        }

    }
     */
}