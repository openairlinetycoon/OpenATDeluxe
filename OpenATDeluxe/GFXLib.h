#pragma once

#include "stdafx.h"
using namespace std;

struct GFXFile {
	string name;
	int fileOffset;
};

class GFXLib {
private:
	string fileName;

	map<string, GFXFile> files;

	BYTE *readGfxData(FILE * file, long fileOffset, BYTE * pixels, int *width, int *height);
	void readData();
public:
	GFXLib(string fileName);

	SDL_Texture *GetTexture(string gfxName);

	~GFXLib();
};

