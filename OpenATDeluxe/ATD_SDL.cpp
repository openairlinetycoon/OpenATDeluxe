#include "stdafx.h"
#include "ATD_SDL.h"


list<Drawable>* ATD_SDL::drawables;
SDL_Renderer *ATD_SDL::renderer;
ATD_SDL *ATD_SDL::instance;

ATD_SDL::ATD_SDL(){
	instance = this;
	//mono_add_internal_call("", AddDrawableM);
}
ATD_SDL::~ATD_SDL(){
	OnCleanup();
}

void ATD_SDL::AddDrawable(std::string *file) {
	drawables->push_back(*new Drawable(file, NULL));

}
MonoObject *ATD_SDL::AddDrawableM(MonoString *file) {
	char *s = mono_string_to_utf8(file);

	Drawable *d = new Drawable(new std::string(s), NULL);

	drawables->push_back(*d);
	mono_free(s);

	return d->object;
}

MonoObject *ATD_SDL::AddDrawableLib(MonoString *file, MonoString *name) {
	char *fileName = mono_string_to_utf8(file);
	char *gfxName = mono_string_to_utf8(name);

	GFXLib l = GFXLib(fileName);

	Drawable *d = new Drawable(new std::string(gfxName), &l);

	drawables->push_back(*d);

	mono_free(fileName);
	mono_free(gfxName);

	return d->object;
}

float ATD_SDL::GetFPS() {
	return instance->avgFPS;
}

int ATD_SDL::GetMouseX() {
	int x;
	SDL_GetMouseState(&x,NULL);
	return x;
}

int ATD_SDL::GetMouseY() {
	int y;
	SDL_GetMouseState(NULL, &y);
	return y;
}
void hello();

void ATD_SDL::PrepareMonoMethods()
{
	MonoObject *intermediate;
	intermediate = MonoHelper::create_object("SDLWrapper", "OpenATD.SDL");
	MonoHelper::add_method(intermediate, "AddDrawable", AddDrawableM);
	MonoHelper::add_method(intermediate, "AddDrawableLib", AddDrawableLib);
	MonoHelper::add_method(intermediate, "GetFPS", GetFPS);
	MonoHelper::add_method(intermediate, "GetMouseX", GetMouseX);
	MonoHelper::add_method(intermediate, "GetMouseY", GetMouseY);


	intermediate = MonoHelper::create_object("GFXLib", "OpenATD.SDL", false);
	MonoHelper::add_method(intermediate, "Create", GFXLib::Create);
	MonoHelper::add_method(intermediate,"_GetAllImageNames", GFXLib::GetAllImageNames);
}



bool ATD_SDL::OnInit() {
	if (SDL_Init(SDL_INIT_EVERYTHING) < 0) {
		return false;
	}

	MonoHelper::prepareDomain();
	MonoHelper::prepareImage();

	PrepareMonoMethods();

	drawables = new std::list<Drawable>();


	window = SDL_CreateWindow("Game Window", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, 640, 480, SDL_WINDOW_OPENGL);

	if (window == NULL) {
		return false;
	}

	renderer = SDL_CreateRenderer(window, -1, SDL_RENDERER_ACCELERATED);
	if (renderer == NULL) {
		SDL_DestroyWindow(window);
		std::cout << "SDL_CreateRenderer Error: " << SDL_GetError() << std::endl;
		SDL_Quit();
		return false;
	}
	/*
	int error = 0;
	error = Sound_Init();
	if (error != 0) {
		std::cout << "Sound_Init Error: " << Sound_GetError() << std::endl;
		SDL_Quit();
		return false;
	}
	*/
	callbackManager = MonoHelper::disable_gc(MonoHelper::create_object("CallbackManager", "OpenATD"));

	isRunning = true;

	return true;
}

int ATD_SDL::OnExecute() {
	if (OnInit() == false) {
		return -1;
	}

	LTimer fpsTimer;
	fpsTimer.start();
	countedFrames = 0;

	SDL_Event Event;

	#define FPS_INTERVAL 1.0 //seconds.

	Uint32 fps_lasttime = SDL_GetTicks(); //the last recorded time.
	Uint32 fps_current; //the current FPS.
	Uint32 fps_frames = 0; //frames passed since the last recorded fps.

	while (isRunning) {
		while (SDL_PollEvent(&Event)) {
			OnEvent(&Event);
		}

		//OnLoop();
		OnRender();

		fps_frames++;
		if (fps_lasttime < SDL_GetTicks() - FPS_INTERVAL * 1000)
		{
			fps_lasttime = SDL_GetTicks();
			avgFPS = fps_frames;
			fps_frames = 0;
		}
	}

	OnCleanup();

	return 0;
}


void ATD_SDL::OnCleanup() {
	//mono_environment_exitcode_get();


	for (std::list<Drawable>::iterator it = drawables->begin(); it != drawables->end(); ++it){
		it->cleanup();
	}

	delete drawables;

	//mono_jit_cleanup(MonoHelper::domain);

	SDL_DestroyRenderer(renderer);
	SDL_DestroyWindow(window);
	SDL_Quit();
}

void ATD_SDL::OnEvent(SDL_Event *event) {
	if (event->type == SDL_QUIT) {
		isRunning = false;
	}

	MonoHelper::call_method(callbackManager, "OnEvent", NULL);


	if (event->type == SDL_KEYDOWN) {
		void *args[1];
		MonoString *s = mono_string_new(mono_domain_get(),SDL_GetKeyName(event->key.keysym.sym));
		args[0] = s;
		MonoHelper::call_method(callbackManager, "InputCallback", args);
	}

}

void ATD_SDL::OnRender() {

	SDL_RenderClear(renderer);
	for (std::list<Drawable>::iterator it = drawables->begin(); it != drawables->end(); ++it)
	{
		it->draw();
	}
	SDL_RenderPresent(renderer);
}