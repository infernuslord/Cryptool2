#pragma once
#include "aes_core.h"
#include "DES/des.h"

using namespace System::Threading;
using namespace System;

namespace NativeCryptography {

	public ref class Crypto
	{
	private:
		enum class cryptMethod {methodAES, methodDES};

		static array<unsigned char>^ zeroIV8 = gcnew array<unsigned char>(8);
		static array<unsigned char>^ zeroIV16 = gcnew array<unsigned char>(16);

		static void xorBlockAES(int *t1, int *t2);
		static void xorBlockDES(int *t1, int *t2);
		static void encrypt(unsigned char* in, unsigned char* out, const cryptMethod method, AES_KEY* aeskey, DES_key_schedule* deskey);
		static void decrypt(unsigned char* in, unsigned char* out, const cryptMethod method, AES_KEY* aeskey, DES_key_schedule* deskey);
		static void xorblock(unsigned char* t1, unsigned char* t2, const cryptMethod method);
		static array<unsigned char>^ decryptAESorDES(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int bits, const int length, const int mode, const int blockSize, const cryptMethod method);

		static Mutex^ prepareMutex = gcnew Mutex();
		

	public:
		static double calculateEntropy(array<unsigned char>^ text, int bytesToUse);

		static array<unsigned char>^ decryptAES(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int bits, const int length, const int mode)
		{
			const int blockSize = 16;
			return decryptAESorDES(Input, Key, IV, bits, length, mode, blockSize, cryptMethod::methodAES);
		}

		static array<unsigned char>^ decryptDES(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length, const int mode)
		{
			const int blockSize = 8;
			return decryptAESorDES(Input, Key, IV, 0, length, mode, blockSize, cryptMethod::methodDES);
		}
	};
}