#pragma once

#include "stdafx.h"

class Drawable
{
private:
	SDL_Texture *texture;
	std::string *file;


	int id;//the gc handle
public:
	int x;
	int y;

	MonoObject *object;

	Drawable(std::string *file);

	void updatePos();

	void prepareDraw(std::string file);
	void draw();
	void cleanup();
};

