// This is the main DLL file.

#include "NativeCryptography.h"

namespace NativeCryptography {

	/* Fast way to xor an AES block */
	void Crypto::xorBlockAES(int *t1, int *t2)
	{
		t1[0] ^= t2[0];
		t1[1] ^= t2[1];
		t1[2] ^= t2[2];
		t1[3] ^= t2[3];
	}

	/* Fast way to xor an DES block */
	void Crypto::xorBlockDES(int *t1, int *t2)
	{
		t1[0] ^= t2[0];
		t1[1] ^= t2[1];
	}

	void Crypto::encrypt(unsigned char* in, unsigned char* out, const cryptMethod method, AES_KEY* aeskey, DES_key_schedule* deskey)
	{
		if (method == cryptMethod::methodAES)
			AES_encrypt(in, out, aeskey);
		else
			DES_ecb_encrypt((const_DES_cblock*)in, (const_DES_cblock*)out, deskey, DES_ENCRYPT);
	}

	void Crypto::decrypt(unsigned char* in, unsigned char* out, const cryptMethod method, AES_KEY* aeskey, DES_key_schedule* deskey)
	{
		if (method == cryptMethod::methodAES)
			AES_decrypt(in, out, aeskey);
		else
			DES_ecb_encrypt((const_DES_cblock*)in, (const_DES_cblock*)out, deskey, DES_DECRYPT);
	}


	void Crypto::xorblock(unsigned char* t1, unsigned char* t2, const cryptMethod method) {
		if (method == cryptMethod::methodAES)
			xorBlockAES((int*)t1, (int*)t2);
		else
			xorBlockDES((int*)t1, (int*)t2); 
	}

	array<unsigned char>^ Crypto::decryptAESorDES(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int bits, const int length, const int mode, const int blockSize, const cryptMethod method)
	{
		int numBlocks = length / blockSize;
		if (length % blockSize != 0)
			numBlocks++;

		if (IV == nullptr)
			IV = gcnew array<unsigned char>(blockSize);

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> iv = &IV[0];

		array<unsigned char>^ output = gcnew array<unsigned char>(length);
		pin_ptr<unsigned char> outp = &output[0];	

		AES_KEY aeskey;
		DES_key_schedule deskey;
		if (mode == 2)	//CFB
		{
			array<unsigned char>^ ShiftRegister = (array<unsigned char>^)IV->Clone();
			pin_ptr<unsigned char> shiftregister = &ShiftRegister[0];			
			unsigned char block[16];	//16 is enough for AES and DES

			if (method == cryptMethod::methodAES) 
				AES_set_encrypt_key(key, bits, &aeskey);
			else 
				DES_set_key_unchecked((const_DES_cblock*)key, &deskey);

			for (int i = 0; i < length; i++)
			{
				encrypt(shiftregister, block, method, &aeskey, &deskey);
				unsigned char leftmost = block[0];
				outp[i] = leftmost ^ input[i];
				
				//shift input[i] in register:
				for (int c = 0; c < blockSize - 1; c++)
					shiftregister[c] = shiftregister[c+1];
				shiftregister[blockSize-1] = input[i];
			}
		}
		else	//CBC or ECB
		{
			if (method == cryptMethod::methodAES)
				AES_set_decrypt_key(key, bits, &aeskey);
			else
				DES_set_key_unchecked((const_DES_cblock*)key, &deskey);

			decrypt(input, outp, method, &aeskey, &deskey);				
			if (mode == 1)		//CBC
				xorblock(outp, iv, method);	
			for (int c = 1; c < numBlocks; c++)
			{
				decrypt(input+c*blockSize, outp+c*blockSize, method, &aeskey, &deskey);
				if (mode == 1)		//CBC
					xorblock(outp+c*blockSize, input+(c-1)*blockSize, method);				
			}
		}

		return output;
	}

}