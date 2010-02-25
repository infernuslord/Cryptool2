#pragma once
#include "aes_core.h"
#include "DES/des.h"
#include <stdlib.h>

using namespace System;

namespace NativeCryptography {

	public ref class Crypto
	{
	private:
		static void arrayToCArray(array<unsigned char>^ a, unsigned char *ca, int length)
		{
			int counter;
			int l = (a->Length < length) ? a->Length : length;
			for (counter = 0; counter < l; counter++)
				ca[counter] = a[counter];
			for (; counter < length; counter++)
				ca[counter] = 0;
		}

		static void carrayToArray(array<unsigned char>^ a, unsigned char *ca, int length)
		{
			int counter;
			for (counter = 0; counter < length; counter++)
				a[counter] = ca[counter];
		}

		static void xorBlockAES(int *t1, int *t2)
		{
			t1[0] ^= t2[0];
			t1[1] ^= t2[1];
			t1[2] ^= t2[2];
			t1[3] ^= t2[3];
		}

		static void xorBlockDES(int *t1, int *t2)
		{
			t1[0] ^= t2[0];
			t1[1] ^= t2[1];
		}

	public:
		static array<unsigned char>^ decryptAES(array<unsigned char>^ input, array<unsigned char>^ key, const int bits, const int length, const int mode)
		{
			const int blockSize = 16;
			int numBlocks = length / blockSize;
			if (length % blockSize != 0)
				numBlocks++;

			unsigned char* inp = (unsigned char*)malloc(numBlocks*blockSize);
			unsigned char* outp = (unsigned char*)malloc(numBlocks*blockSize);
			unsigned char ckey[32];

			arrayToCArray(input, inp, numBlocks*blockSize);			
			arrayToCArray(key, ckey, bits/8);

			AES_KEY aeskey;
			AES_set_decrypt_key(ckey, bits, &aeskey);
			AES_decrypt(inp, outp, &aeskey);
			for (int c = 1; c < numBlocks; c++)
			{
				AES_decrypt((inp+c*blockSize), outp+c*blockSize, &aeskey);
				if (mode == 1)		//CBC
					xorBlockAES((int*)(outp+c*blockSize), (int*)(inp+(c-1)*blockSize));
			}

			array<unsigned char>^ output = gcnew array<unsigned char>(length);
			carrayToArray(output, outp, length);
			free(inp);
			free(outp);			
			return output;
		}

		static array<unsigned char>^ decryptDES(array<unsigned char>^ input, array<unsigned char>^ key, const int length, const int mode)
		{
			const int blockSize = 8;
			int numBlocks = length / blockSize;
			if (length % blockSize != 0)
				numBlocks++;

			unsigned char* inp = (unsigned char*)malloc(numBlocks*blockSize);
			unsigned char* outp = (unsigned char*)malloc(numBlocks*blockSize);
			unsigned char ckey[8];

			arrayToCArray(input, inp, numBlocks*blockSize);			
			arrayToCArray(key, ckey, 8);			

			DES_key_schedule deskey;
			DES_set_key_unchecked(&ckey, &deskey);
			DES_ecb_encrypt((const_DES_cblock*)inp, (const_DES_cblock*)outp, &deskey, DES_DECRYPT);
			for (int c = 1; c < numBlocks; c++)
			{
				DES_ecb_encrypt((const_DES_cblock*)(inp+c*blockSize), (const_DES_cblock*)(outp+c*blockSize), &deskey, DES_DECRYPT);
				if (mode == 1)		//CBC
					xorBlockDES((int*)(outp+c*blockSize), (int*)(inp+(c-1)*blockSize));
			}

			array<unsigned char>^ output = gcnew array<unsigned char>(length);
			carrayToArray(output, outp, length);
			free(inp);
			free(outp);			
			return output;
		}
	};
}