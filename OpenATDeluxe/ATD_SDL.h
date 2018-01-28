#pragma once

#include "stdafx.h"
#include <list>
#include <string>

class ATD_SDL
{
private:
	SDL_Window * window;
	uint32_t callbackManager;
	bool isRunning;
	int countedFrames;
	float avgFPS;

	static std::list<Drawable> *drawables;
	static map<string, GFXFile> files;
public:
	static ATD_SDL *instance;
	static SDL_Renderer *renderer;
	
	ATD_SDL();
	~ATD_SDL();

	void AddDrawable(std::string *file);
	static MonoObject *AddDrawableM(MonoString *file);
	static MonoObject *AddDrawableLib(MonoString * file, MonoString *name);
	static float GetFPS();

	static int GetMouseX();
	static int GetMouseY();

	int OnExecute();

	bool OnInit();

	void OnEvent(SDL_Event* Event);

	void OnLoop();

	void OnRender();

	void OnCleanup();
};

