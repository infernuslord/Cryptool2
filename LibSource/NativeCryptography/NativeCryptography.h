#pragma once
#include "aes_core.h"
#include "DES/des.h"
#include <string.h>
#include "rc2.h"

using namespace System::Threading;
using namespace System;

namespace NativeCryptography {

	public ref class Crypto
	{
	private:
		enum class cryptMethod {methodAES, methodDES, method3DES};

		static array<unsigned char>^ zeroIV8 = gcnew array<unsigned char>(8);
		static array<unsigned char>^ zeroIV16 = gcnew array<unsigned char>(16);

		static void xorBlockAES(int *t1, int *t2);
		static void xorBlockDES(int *t1, int *t2);
		static void encrypt(unsigned char* in, unsigned char* out, const cryptMethod method, AES_KEY* aeskey, DES_key_schedule* deskey);
		static void decrypt(unsigned char* in, unsigned char* out, const cryptMethod method, AES_KEY* aeskey, DES_key_schedule* deskey);
		static void xorblock(unsigned char* t1, unsigned char* t2, const cryptMethod method);
		static array<unsigned char>^ decryptAESorDES(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int bits, const int length, const int mode, const int blockSize, const cryptMethod method);
		static array<unsigned char>^ encryptAESorDES(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int bits, const int length, const int mode, const int blockSize, const cryptMethod method);

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

		static array<unsigned char>^ decryptTripleDES(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length, const int mode)
		{
			const int blockSize = 8;			
			return decryptAESorDES(Input, Key, IV, 0, length, mode, blockSize, cryptMethod::method3DES);
		}

		static array<unsigned char>^ encryptAES(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int bits, const int length, const int mode)
		{
			const int blockSize = 16;
			return encryptAESorDES(Input, Key, IV, bits, length, mode, blockSize, cryptMethod::methodAES);
		}

		static array<unsigned char>^ encryptDES(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length, const int mode)
		{
			const int blockSize = 8;
			return encryptAESorDES(Input, Key, IV, 0, length, mode, blockSize, cryptMethod::methodDES);
		}		

		static array<unsigned char>^ decryptRC2(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int length, const int mode)
		{
			if (mode == 2)	//CFB
			{			
				throw gcnew System::Exception("Encrypting CFB not supported (yet?)");
			}

			array<unsigned char>^ output = gcnew array<unsigned char>(length);
			unsigned short xkey[64];
			
			cli::pin_ptr<unsigned char> p_key = &Key[0];
			cli::pin_ptr<unsigned char> p_iv = &IV[0];

			rc2_keyschedule( xkey, p_key, Key.Length, Key.Length * 8);					

			//put IV into saving-block
			unsigned char block[8] = {0,0,0,0,0,0,0,0};
			xorBlockDES((int*)block,(int*)p_iv);

			for(int i=0;i<length;i+=8)
			{	
				if (mode == 0) //ECB
				{
					cli::pin_ptr<unsigned char> p_input = &Input[i];
					cli::pin_ptr<unsigned char> p_output = &output[i];
					rc2_decrypt( xkey,p_output,p_input);
				}		
				if(mode == 1) //CBC
				{						
					cli::pin_ptr<unsigned char> p_input = &Input[i];
					cli::pin_ptr<unsigned char> p_output = &output[i];
					
					rc2_decrypt( xkey,p_output,p_input);
					xorBlockDES((int*)p_output,(int*)block);

					xorBlockDES((int*)block,(int*)block);
					xorBlockDES((int*)block,(int*)p_input);
				}
			}
			return output;
		}

		static array<unsigned char>^ md5(array<unsigned char>^ Input);
	};
}