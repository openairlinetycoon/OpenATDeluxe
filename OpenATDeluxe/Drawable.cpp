#include "stdafx.h"
#include "Drawable.h"


Drawable::Drawable(std::string *file) {
	this->file = file;

	prepareDraw(*file);
	
	object = MonoHelper::create_object("Drawable", "OpenATD.SDL");

	id = MonoHelper::disable_gc(object);

	void *args[3];

	MonoString *str = mono_string_new(mono_domain_get(), (*file).c_str());

	args[0] = str;
	args[1] = &x;
	args[2] = &y;


	MonoHelper::call_method(object, "Setup", args);
}

void Drawable::updatePos() {
	MonoObject *v = MonoHelper::get_value(id, "x", id);
	x = *(int*)mono_object_unbox(v);

	v = MonoHelper::get_value(id, "y", id);
	y = *(int*)mono_object_unbox(v);
}

void Drawable::draw() {
	//Draw the texture

	updatePos();

	int w, h;
	SDL_QueryTexture(texture, NULL, NULL, &w, &h);

	SDL_Rect r;
	r.x = x;
	r.y = y;
	r.w = w;
	r.h = h;

	SDL_RenderCopy(ATD_SDL::renderer, texture, NULL, &r);

}
#include <direct.h>
#define GetCurrentDir _getcwd

void Drawable::prepareDraw(std::string file) {
	char cCurrentPath[FILENAME_MAX];

	if (!GetCurrentDir(cCurrentPath, sizeof(cCurrentPath)))
	{
		return;
	}

	cCurrentPath[sizeof(cCurrentPath) - 1] = '\0'; /* not really required */


	SDL_Surface *bmp = SDL_LoadBMP(file.c_str());
	if (bmp == NULL) {
		std::cout << "SDL_LoadBMP Error: " << SDL_GetError() << std::endl;
		return;
	}

	texture = SDL_CreateTextureFromSurface(ATD_SDL::renderer, bmp);
	SDL_FreeSurface(bmp);
	if (texture == NULL) {
		std::cout << "SDL_CreateTextureFromSurface Error: " << SDL_GetError() << std::endl;
		return;
	}
}

void Drawable::cleanup() {
	SDL_DestroyTexture(texture);
	MonoHelper::clean_up(id);
}