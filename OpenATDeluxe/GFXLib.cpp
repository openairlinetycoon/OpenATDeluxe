#include "stdafx.h"
#include "GFXLib.h"

GFXLib::GFXLib(string fileName){
	this->fileName = fileName;

	readData();
}

MonoObject *GFXLib::Create(MonoString* str)
{
	MonoObject *e = MonoHelper::create_object("GFXLib", "OpenATD.SDL");


	//MonoHelper::add_method(e, "GetAllImageNames", GFXLib::GetAllImageNames);

	void *number;// = mono_object_unbox( MonoHelper::get_valueObject(e, "pointer", 0));
	number = new GFXLib(mono_string_to_utf8(str));
	mono_field_set_value(e, mono_class_get_field_from_name(mono_object_get_class(e), "pointer"), number);

	mono_field_set_value(e, mono_class_get_field_from_name(mono_object_get_class(e), "fileName"), mono_string_new(MonoHelper::domain, ((GFXFile*)number)->name.c_str()));
	//string name = mono_string_to_utf8(mono_object_to_string(e, NULL));

	//string file = mono_string_to_utf8((MonoString*)mono_object_unbox(MonoHelper::get_value(gfxLib, "fileName")));
	
	return e;
}

MonoArray* GFXLib::GetAllImageNames(MonoObject* obj)
{	//TBF
	GFXLib* lib = NULL;//(IntPtr*)mono_object_unbox(obj);
	map<string, GFXFile>::iterator it;

	cout << lib->fileName;

	MonoArray *array = mono_array_new(MonoHelper::domain, mono_get_string_class(), lib->files.size());

	int i = 0;
	for (it = lib->files.begin(); it != lib->files.end(); it++)
	{
		std::cout << it->first
			<< std::endl;
		
		mono_array_setref(array, i, mono_string_new(MonoHelper::domain, it->first.c_str()));

		i++;
	}
	return array;
}

BYTE* ConvertRGBToBMPBuffer(BYTE* Buffer, int width, int height, long* newsize);
BYTE *appendBMPHeader(BYTE *pixels, int width, int height, long bufferSize, long *finalSize);
void saveGfxToFile(string filename, BYTE *pixels, int width, int height, long pixelSize);

SDL_Texture * GFXLib::GetTexture(string gfxName)
{
	int width, height;
	long size = 0;
	long finalSize = 0;

	BYTE *data = NULL, *nData = NULL;
	data = readGfxData(NULL, files[gfxName.c_str()].fileOffset, data, &width, &height);
	//nData = appendBMPHeader(
	//	ConvertRGBToBMPBuffer(data, width, height, &size), width, height, size, &finalSize);
	
	//saveGfxToFile("test.bmp", data, width, height, size);
	int pitch = width * 2;

	SDL_Texture *texture;

	SDL_Surface *surf = SDL_CreateRGBSurfaceWithFormatFrom(data, width, height, 8, pitch, SDL_PIXELFORMAT_RGB565);
	//SDL_Surface *surf = SDL_LoadBMP_RW(SDL_RWFromMem(nData, finalSize), 1);
	if (surf == NULL) {
		std::cout << "SDL_CreateRGBSurfaceWithFormatFrom Error: " << SDL_GetError() << std::endl;
		return NULL;
	}

	texture = SDL_CreateTextureFromSurface(ATD_SDL::renderer, surf);
	SDL_FreeSurface(surf);
	if (texture == NULL) {
		std::cout << "SDL_CreateTextureFromSurface Error: " << SDL_GetError() << std::endl;
		return NULL;
	}

	return texture;
}

GFXLib::~GFXLib(){

}

#pragma region File Reading

//#define CONVERT_TO_RGB

typedef __int16 pixel;

struct pixelFileBuffer {
#ifdef CONVERT_TO_RGB
	pixel data;
#else
	BYTE a;
	BYTE b;
#endif
};

#define RED 11 //The bitmask to read only the red
#define GREEN 5 //The bitmask to read only the green

#define BLUE 0

#define SizeFive 31
#define SizeSix 63

#define GFXHEADERSIZE 76
#define DIMENSIONPOS 8
#define FOOTERSIZE 14
#define HEADERSIZE sizeof(dataHeader)
#define FILEHEADERSIZE 17

#pragma pack(push, 1)
struct dataHeader { //68 Bytes
	char header[5];
	BYTE unkown[5];
	__int32 archiveSize;
	BYTE unkown2[20];
	__int32 files;
	__int32 info;//54
	BYTE unkown3[21];
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


BYTE* ConvertRGBToBMPBuffer(BYTE* Buffer, int width, int height, long* newsize) {

	// first make sure the parameters are valid
	if ((NULL == Buffer) || (width == 0) || (height == 0))
		return NULL;

	// now we have to find with how many bytes
	// we have to pad for the next DWORD boundary	

	int padding = 0;
	int scanlinebytes = width * 3;
	while ((scanlinebytes + padding) % 4 != 0)     // DWORD = 4 bytes
		padding++;
	// get the padded scanline width
	int psw = scanlinebytes + padding;

	// we can already store the size of the new padded buffer
	*newsize = height * psw;

	// and create new buffer
	BYTE* newbuf = new BYTE[*newsize];

	// fill the buffer with zero bytes then we dont have to add
	// extra padding zero bytes later on
	memset(newbuf, 0, *newsize);

	// now we loop trough all bytes of the original buffer, 
	// swap the R and B bytes and the scanlines
	long bufpos = 0;
	long newpos = 0;
	for (int y = 0; y < height; y++)
		for (int x = 0; x < 3 * width; x += 3)
		{
			bufpos = y * 3 * width + x;     // position in original buffer !!We hope that the file is of the correct size, with the given Values
			newpos = (height - y - 1) * psw + x;           // position in padded buffer

			newbuf[newpos] = Buffer[bufpos + 2];       // swap r and b
			newbuf[newpos + 1] = Buffer[bufpos + 1]; // g stays
			newbuf[newpos + 2] = Buffer[bufpos];     // swap b and r
		}

	return newbuf;
}

BYTE *appendBMPHeader(BYTE *pixels, int width, int height, long bufferSize,long *finalSize) {
	BITMAPFILEHEADER bmfh;
	BITMAPINFOHEADER info;
	memset(&bmfh, 0, sizeof(BITMAPFILEHEADER));
	memset(&info, 0, sizeof(BITMAPINFOHEADER));
	bmfh.bfType = 0x4d42;       // 0x4d42 = 'BM'
	bmfh.bfReserved1 = 0;
	bmfh.bfReserved2 = 0;

	bmfh.bfSize = sizeof(BITMAPFILEHEADER) +
		sizeof(BITMAPINFOHEADER) + bufferSize;
	bmfh.bfOffBits = 0x36;

	info.biSize = sizeof(BITMAPINFOHEADER);
	info.biWidth = width;
	info.biHeight = height;
	info.biPlanes = 1;
	info.biBitCount = 24;
	info.biCompression = BI_RGB;
	info.biSizeImage = 0;
	info.biXPelsPerMeter = 0x0ec4;
	info.biYPelsPerMeter = 0x0ec4;
	info.biClrUsed = 0;
	info.biClrImportant = 0;

	BYTE *result = new BYTE[bmfh.bfSize];
	memset(result, 0, bmfh.bfSize);

	memcpy_s(result, bmfh.bfSize, &bmfh, sizeof(BITMAPFILEHEADER));

	memcpy_s(result + sizeof(BITMAPFILEHEADER), bmfh.bfSize, &info, sizeof(BITMAPINFOHEADER));

	memcpy_s(result + sizeof(BITMAPINFOHEADER) + sizeof(BITMAPFILEHEADER), bmfh.bfSize, &pixels, bufferSize);

	long _finalSize = bmfh.bfSize;
	*finalSize = _finalSize;

	return result;
}

void saveGfxToFile(string filename, BYTE *pixels, int width, int height, long pixelSize) {
	BITMAPFILEHEADER bmfh;
	BITMAPINFOHEADER info;
	memset(&bmfh, 0, sizeof(BITMAPFILEHEADER));
	memset(&info, 0, sizeof(BITMAPINFOHEADER));
	bmfh.bfType = 0x4d42;       // 0x4d42 = 'BM'
	bmfh.bfReserved1 = 0;
	bmfh.bfReserved2 = 0;

	bmfh.bfSize = sizeof(BITMAPFILEHEADER) +
		sizeof(BITMAPINFOHEADER) + pixelSize;
	bmfh.bfOffBits = 0x36;

	info.biSize = sizeof(BITMAPINFOHEADER);
	info.biWidth = width;
	info.biHeight = height;
	info.biPlanes = 1;
	info.biBitCount = 24;
	info.biCompression = BI_RGB;
	info.biSizeImage = 0;
	info.biXPelsPerMeter = 0x0ec4;
	info.biYPelsPerMeter = 0x0ec4;
	info.biClrUsed = 0;
	info.biClrImportant = 0;

	FILE *file;
	fopen_s(&file, filename.c_str(), "w");

	fwrite(&bmfh, sizeof(bmfh), 1, file);
	fwrite(&info, sizeof(info), 1, file);

	long bufferSize;
	BYTE* buffer = ConvertRGBToBMPBuffer(pixels, width, height, &bufferSize);

	cout << fwrite(buffer, bufferSize, 1, file);

	fclose(file);

	delete buffer;
}

int get_color(pixel n, int size, int colorWanted) {
	int c = n >> colorWanted;
	return c & size;
}


BYTE* GFXLib::readGfxData(FILE *file, long fileOffset, BYTE* pixels, int *width, int *height) {
	if (file == NULL)
		fopen_s(&file, fileName.c_str(), "rb");

	fseek(file, fileOffset, 0);

	int oldHeader = ftell(file);
	fseek(file, DIMENSIONPOS, 1);
	oldHeader = ftell(file);
	//int width;
	fread_s(width, sizeof(__int32), sizeof(__int32), 1, file);
	//int height;
	fread_s(height, sizeof(__int32), sizeof(__int32), 1, file);
	oldHeader = ftell(file);

	fseek(file, fileOffset + GFXHEADERSIZE, 0);
	oldHeader = ftell(file);

	long fileSize = *width * *height * 2;

#ifdef CONVERT_TO_RGB
	long bufferSize = fileSize / 2 * 3;
#else
	long bufferSize = fileSize;
#endif

	pixels = new BYTE[bufferSize];
	memset(pixels, 0, bufferSize);


	long iB = 0;
	for (long i = 0; i < fileSize / 2; i++) {
		pixelFileBuffer buff;
		fread_s(&buff, sizeof(buff), sizeof(buff), 1, file);

#ifdef CONVERT_TO_RGB
		int r = get_color(buff.data, SizeFive, RED);
		int g = get_color(buff.data, SizeSix, GREEN);
		int b = get_color(buff.data, SizeFive, BLUE);

		pixels[iB] = r * 255 / 31;
		pixels[iB + 1] = g * 255 / 63;
		pixels[iB + 2] = b * 255 / 31;

		//cout << buff.data << "\n";
		//cout << r << " is " << r * 255 / 31 << "\n";
		//cout << g << " is " << g * 255 / 63 << "\n";
		//cout << b << " is " << b * 255 / 31 << "\n";

		iB += 3;
#else
		pixels[iB] = buff.a;
		pixels[iB + 1] = buff.b;
		iB += 2;
#endif // CONVERT_TO_RGB

	}
	fseek(file, fileOffset + fileSize + GFXHEADERSIZE, 0);

	int d = ftell(file);

	char readFilename[20];
	fread_s(&readFilename, sizeof(readFilename), sizeof(readFilename), 1, file);

	char buffer[20] = { '\0' };
	bool hitData = false;
	int aReal = 0;
	for (int a = 0; a < sizeof(readFilename) - 1; a++) {
		if (readFilename[a] != '\0' || hitData) {
			if (hitData) {
				buffer[aReal] = readFilename[a];
				aReal++;
			}

			if (readFilename[a] == '\\') {
				hitData = true;
				aReal = 0;
			}

		}
	}
	if (aReal < 20)
		buffer[aReal] = '\0';

	cout << buffer << "\n";


	//saveGfxToFile(buffer + string(".bmp"), pixels, *width, *height, bufferSize);

	return pixels;
}


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
#pragma endregion