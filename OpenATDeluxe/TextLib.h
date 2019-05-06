#pragma once
#include "stdafx.h"

#define COMMA ,
#define DEFINE_WRAPPER(thisType, type, method, arguments, argumentNames) \
type method (arguments);\
static type method (thisType *me, arguments) { return me-> method ( argumentNames ); }

class TextLib
{
private:
	int key1, key2;
	int verbose;
	uint32_t size;
public:
	typedef void (TextLib::*ModuleFunction)(void);

	TextLib();
	void saveDecryptedFile();
	void decrypt(uint8_t * data, uint32_t size);
	uint8_t * decompress(FILE * in);
	int handleCSV(const char * filename, const char * destFolder);
	static void handleCSV(TextLib *me, const char * filename, const char * destFolder) { me->handleCSV(filename, destFolder); }

	DEFINE_WRAPPER(TextLib, int, handleCSVA, const char * filename COMMA const char * destFolder, filename COMMA destFolder);


	static std::map< string, const void* > _module_functions;
};
template< typename T >
class ModuleFunctionAdder;

template<>
class ModuleFunctionAdder< TextLib >
{
public:
	ModuleFunctionAdder(string func_name, const void* func)
	{
		TextLib::_module_functions[func_name] = func;
	}
};
