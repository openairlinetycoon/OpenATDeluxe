#include "stdafx.h"
#include "ATD_SDL.h"


std::list<Drawable>* ATD_SDL::drawables;
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
	drawables->push_back(*new Drawable(file));

}
MonoObject *ATD_SDL::AddDrawableM(MonoString *file) {
	char *s = mono_string_to_utf8(file);

	Drawable *d = new Drawable(new std::string(s));

	drawables->push_back(*d);
	mono_free(s);

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



bool ATD_SDL::OnInit() {
	if (SDL_Init(SDL_INIT_EVERYTHING) < 0) {
		return false;
	}

	MonoHelper::prepareDomain();
	MonoHelper::prepareImage();

	MonoObject *sdlWrapper;
	sdlWrapper = MonoHelper::create_object("SDLWrapper", "OpenATD.SDL");
	MonoHelper::add_method(sdlWrapper, "AddDrawable", AddDrawableM);
	MonoHelper::add_method(sdlWrapper, "GetFPS", GetFPS);
	MonoHelper::add_method(sdlWrapper, "GetMouseX", GetMouseX);
	MonoHelper::add_method(sdlWrapper, "GetMouseY", GetMouseY);
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

	while (isRunning) {
		while (SDL_PollEvent(&Event)) {
			OnEvent(&Event);
		}

		//OnLoop();
		OnRender();

		countedFrames++;

		avgFPS = countedFrames / (fpsTimer.getTicks() / 1000.f);
		if (avgFPS > 2000000) {
			avgFPS = 0;
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