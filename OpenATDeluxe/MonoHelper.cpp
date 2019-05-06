#include "MonoHelper.h"
#include "stdafx.h"

using namespace std;
namespace MonoHelper {
	string file = "Net\\OpenATD.dll";
	MonoObject* exception = NULL;


	MonoDomain *domain;
	MonoImage *image;

	void debugException() {
#ifdef DEBUG
		// check for any thrown exception
		if (exception)
		{
			std::cout << mono_string_to_utf8(mono_object_to_string(exception, nullptr))
				<< std::endl;
		}
		exception = NULL;
#endif // DEBUG
	}

	void prepareDomain() {
		mono_set_dirs("C:\\Program Files\\Mono\\lib", "C:\\Program Files\\Mono\\etc");

#ifndef NO_DEBUGGER //The programm will crash when no debugger is listening on that port!
		const char* options[] = {
			"--debugger-agent=transport=dt_socket,address=127.0.0.1:10001"
		};
		mono_jit_parse_options(1, (char**)options);
		mono_debug_init(MONO_DEBUG_FORMAT_MONO);
#endif // NO_DEBUGGER
		mono_config_parse(nullptr);
		domain = mono_jit_init(file.c_str());
	}
	void prepareImage() {
		MonoAssembly *assembly = mono_domain_assembly_open(domain, file.c_str());

		image = mono_assembly_get_image(assembly);
	}
	void add_method(MonoObject* object, const char* funcName, const void* method) {
		MonoClass *k;
		string name;
		string nspace;

		k = mono_object_get_class(object);

		name = mono_class_get_name(k);
		nspace = mono_class_get_namespace(k);

		mono_add_internal_call((nspace + "." + name + "::" + funcName).c_str(), method);
	}
	void add_method(uint32_t id, const char* funcName, const void* method) {
		MonoClass *k;
		string name;
		string nspace;
		MonoObject *klass = get_object_from_handle(id);

		k = mono_object_get_class(klass);

		name = mono_class_get_name(k);
		nspace = mono_class_get_namespace(k);

		mono_add_internal_call((nspace + "." + name + "::" + funcName).c_str(), method);
	}

	void call_method(MonoObject *obj, const char* name) {
		MonoClass *klass;
		MonoDomain *domain;
		MonoMethod *method = NULL;

		klass = mono_object_get_class(obj);
		domain = mono_object_get_domain(obj);

		method = mono_class_get_method_from_name(klass, name, 0);


		mono_runtime_invoke(method, obj, NULL, &exception);
		debugException();

	}
	void call_method(uint32_t id, const char* name) {
		MonoClass *klass;
		MonoDomain *domain;
		MonoMethod *method = NULL;
		MonoObject *obj = get_object_from_handle(id);

		klass = mono_object_get_class(obj);
		domain = mono_object_get_domain(obj);

		method = mono_class_get_method_from_name(klass, name, 0);

		mono_runtime_invoke(method, obj, NULL, &exception);
		debugException();
	}

	void call_method(MonoObject *obj, const char *name, void *args[]) {
		MonoClass *klass;
		MonoDomain *domain;
		MonoMethod *method = NULL;

		klass = mono_object_get_class(obj);
		domain = mono_object_get_domain(obj);

		method = mono_class_get_method_from_name(klass, name, -1);

		mono_runtime_invoke(method, obj, args, &exception);
		debugException();
	}

	void call_method(uint32_t id, const char *name, void *args[]) {
		MonoClass *klass;
		MonoDomain *domain;
		MonoMethod *method = NULL;
		MonoObject *obj = get_object_from_handle(id);

		klass = mono_object_get_class(obj);
		domain = mono_object_get_domain(obj);

		method = mono_class_get_method_from_name(klass, name, -1);

		mono_runtime_invoke(method, obj, args, &exception);
		debugException();
	}

	MonoObject* create_object(MonoDomain *domain, MonoImage *image, const char* name, const char* nspace, bool startCtor) {
		MonoClass *klass;
		MonoObject *object;

		klass = mono_class_from_name(image, nspace, name);

		object = mono_object_new(domain, klass);

		mono_runtime_object_init(object);

		return object;
		//call_method(object);
	}
	MonoObject* create_object(const char* name, const char* nspace, bool startCtor) {
		MonoClass *klass;
		MonoObject *object;

		klass = mono_class_from_name(image, nspace, name);

		object = mono_object_new(domain, klass);

		if(startCtor)
			mono_runtime_object_init(object);

		return object;
		//call_method(object);
	}

	MonoObject* get_valueObject(MonoObject *klass, const char* name, int debugID) {
		MonoClass *k;
		MonoClassField *field;

		k = mono_object_get_class(klass);

		field = mono_class_get_field_from_name(k, name);//TODO: FIXME! A unkown bug causes this to die!

		return mono_field_get_value_object(domain, field, klass);
	}

	MonoObject* get_valueObject(uint32_t id, const char* name, int debugID) {
		MonoClass *k;
		MonoClassField *field;
		MonoObject *klass = get_object_from_handle(id);


		k = mono_object_get_class(klass);

		field = mono_class_get_field_from_name(k, name);//TODO: FIXME! A unkown bug causes this to die!

		return mono_field_get_value_object(domain, field, klass);
	}

	MonoObject *get_object_from_handle(uint32_t id) {
		return mono_gchandle_get_target(id);
	}

	uint32_t disable_gc(MonoObject *obj) {
		return mono_gchandle_new(obj, true);
	}

	void clean_up(uint32_t id) {
		mono_gchandle_free(id);
	}
}