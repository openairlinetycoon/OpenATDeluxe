#include "stdafx.h"
#include "TextLib.h"

#define DECLARE_MODULE_FUNCTION( function_name ) \
void function_name( JsonObject value );

#define DEFINE_MODULE_FUNCTION( function_name ) \
static void \
__LINE__##function_name( L ## #function_name , &TextLib::function_name );    \
void TextLib::function_name( JsonObject value )

TextLib::TextLib()
{
}

void TextLib::saveDecryptedFile()
{

}



//#define _POSIX_C_SOURCE 2 /* getopt() */
#define KEY_ONE  0xa5
#define KEY_TWO  0x00
#define MAGIC   "xtRLE"

void TextLib::decrypt(uint8_t *data, uint32_t size) {
	uint32_t i;
	uint8_t c, d;

	if (!size) return;

	c = data[0];
	for (i = 1; ; i++) {
		d = data[i];
		if (d == c) {
			for (; i < size; i++) {
				d = data[i];
				data[i - 1] = c ^ key2;
				if (d != c)
					break;
			}
		}
		else {
			data[i - 1] = c ^ key1;
		}
		if (i >= size - 1) {
			data[size - 1] = d ^ ((c == d) ? key2 : key1);
			break;
		}
		c = d;
	}
}

uint8_t * TextLib::decompress(FILE *in) {
	char magic[sizeof(MAGIC)];
	/* maybe 02 and 01 have something to do with single and multiple occurences? */
	uint32_t unknown;
	uint8_t *data = NULL, *data_ptr;
	unsigned num;
	int c;

	if (!fread(magic, sizeof(MAGIC), 1, in)) goto err_generic;
	if (!fread(&unknown, sizeof(unknown), 1, in)) goto err_generic;
	if (!fread(&size, sizeof(size), 1, in)) goto err_generic;

	if (memcmp(magic, MAGIC, sizeof(MAGIC))) {
		fprintf(stderr,
			"the 6 magic bytes of the supplied file don't match %s\n", MAGIC);
		goto err;
	}

	if (verbose)
		fprintf(stderr, "header read, decompressing %u bytes to memory\n", size);

	data = static_cast<uint8_t*>(malloc(size));
	if (!data) {
		perror("allocating memory for scrambled, decompressed content");
		goto err;
	}

	data_ptr = data;
	while ((c = fgetc(in)) != EOF) {
		if (!c) {
			fseek(in, 0x0e + num, SEEK_SET);
		}
		else {
			num = c & 0x7f;
			if (c & 0x80) {
				/* num bytes plain data follow */
				if (!fread(data_ptr, num, 1, in))
					goto err_generic;
			}
			else {
				if ((c = fgetc(in)) == EOF) {
					fprintf(stderr,
						"short read: expected argument for run of %u bytes\n", num);
					goto err;
				}
				memset(data_ptr, c, num);
			}
			data_ptr += num;
		}
	}

	return data;

err_generic:
	fprintf(stderr, "error reading input file: %s\n", strerror(ferror(in)));
err:
	free(data);
	return NULL;
}

int TextLib::handleCSV(const char *filename, const char *destFolder) {
	key1 = KEY_ONE;
	key2 = KEY_TWO;
	FILE *fin, *fout;
	uint8_t *data = NULL;
	char *endptr;
	int ret = 0;

	fin = (filename) ? fopen(filename, "rb") : stdin;
	if (!fin) {
		perror("opening input file");
		return EXIT_FAILURE;
	}
	fout = (destFolder) ? fopen(destFolder, "wb") : stdout;
	if (!fout) {
		perror("opening output file");
		fclose(fin);
		return EXIT_FAILURE;
	}

	if (verbose)
		fprintf(stderr, "decompressing input to memory\n");

	data = decompress(fin);
	fclose(fin);
	if (!data) {
		goto end;
	}

	decrypt(data, size);
	if (!fwrite(data, size, 1, fout)) {
		fprintf(stderr, "error writing output: %s\n", strerror(ferror(fout)));
		ret = EXIT_FAILURE;
	}

end:
	free(data);
	fclose(fout);
	return ret;
}
