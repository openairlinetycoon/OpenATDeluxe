#pragma once
#include <mono/jit/jit.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/loader.h>
#include <mono/metadata/environment.h>
#include <mono/utils/mono-publib.h>
#include <mono/metadata/mono-config.h>
#include <mono/metadata/mono-debug.h>



namespace MonoHelper {
	extern MonoDomain *domain;
	extern MonoImage *image;

	void prepareDomain();
	void prepareImage();
	void add_method(MonoObject* klass, const char* funcName, const void* method);
	void add_method(uint32_t id, const char* funcName, const void* method);
	void call_method(MonoObject *obj, const char* name);
	void call_method(uint32_t id, const char* name);
	void call_method(MonoObject *obj, const char* name, void *args[]);
	void call_method(uint32_t id, const char* name, void *args[]);
	MonoObject* create_object(MonoDomain *domain, MonoImage *image, const char* name, const char* nspace, bool startCtor = true);
	MonoObject* create_object(const char* name, const char* nspace, bool startCtor = true);
	void *get_value(MonoObject *klass, const char* name, int debugID);
	MonoObject* get_valueObject(MonoObject *klass, const char* name, int debugID = 0);
	MonoObject* get_valueObject(uint32_t id, const char* name, int debugID = 0);
	MonoObject *get_object_from_handle(uint32_t id);
	uint32_t disable_gc(MonoObject *obj);
	void clean_up(uint32_t id);
}

