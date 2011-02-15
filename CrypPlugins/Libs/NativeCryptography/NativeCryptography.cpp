// This is the main DLL file.

#include "NativeCryptography.h"
#include <stdlib.h>
#include <string.h>

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

		bool noIV = false;

		if (IV == nullptr)
		{
			noIV = true;
			if (blockSize == 8)
				IV = zeroIV8;
			else if (blockSize == 16)
				IV = zeroIV16;
			else
				return nullptr;
		}

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> iv = &IV[0];

		array<unsigned char>^ output = gcnew array<unsigned char>(length);
		pin_ptr<unsigned char> outp = &output[0];

		AES_KEY aeskey;
		DES_key_schedule deskey;
		if (mode == 2)	//CFB
		{			
			unsigned char block[16];	//16 is enough for AES and DES
			unsigned char shiftregister[16];
			//works only for little endian architectures:
			if (blockSize == 8)
			{
				*((unsigned int*)shiftregister) = *((unsigned int*)&iv[1]);
				*((unsigned int*)&shiftregister[4]) = (*((unsigned int*)&iv[4]) >> 8) | ((unsigned int)(input[0]) << 24);
			}
			else if (blockSize == 16)
			{
				*((unsigned int*)shiftregister) = *((unsigned int*)&iv[1]);
				*((unsigned int*)&shiftregister[4]) = (*((unsigned int*)&iv[4]) >> 8) | ((unsigned int)iv[8] << 24);
				*((unsigned int*)&shiftregister[8]) = (*((unsigned int*)&iv[8]) >> 8) | ((unsigned int)iv[12] << 24);
				*((unsigned int*)&shiftregister[12]) = (*((unsigned int*)&iv[12]) >> 8) | ((unsigned int)input[0] << 24);
			}
			else
				return nullptr;
			
			if (method == cryptMethod::methodAES)
				AES_set_encrypt_key(key, bits, &aeskey);
			else
				DES_set_key_unchecked((const_DES_cblock*)key, &deskey);

			encrypt(iv, block, method, &aeskey, &deskey);
			unsigned char leftmost = block[0];
			outp[0] = leftmost ^ input[0];

			for (int i = 1; i < length; i++)
			{
				encrypt(shiftregister, block, method, &aeskey, &deskey);
				leftmost = block[0];
				outp[i] = leftmost ^ input[i];
				
				//shift input[i] in register:
				if (blockSize == 8)
				{
					*((unsigned int*)shiftregister) = *((unsigned int*)&shiftregister[1]);
					*((unsigned int*)&shiftregister[4]) = (*((unsigned int*)&shiftregister[4]) >> 8) | ((unsigned int)input[i] << 24);
				}
				else if (blockSize == 16)
				{
					*((unsigned int*)shiftregister) = *((unsigned int*)&shiftregister[1]);
					*((unsigned int*)&shiftregister[4]) = (*((unsigned int*)&shiftregister[4]) >> 8) | ((unsigned int)shiftregister[8] << 24);
					*((unsigned int*)&shiftregister[8]) = (*((unsigned int*)&shiftregister[8]) >> 8) | ((unsigned int)shiftregister[12] << 24);
					*((unsigned int*)&shiftregister[12]) = (*((unsigned int*)&shiftregister[12]) >> 8) | ((unsigned int)input[i] << 24);
				}
			}
		}
		else	//CBC or ECB
		{
			if (method == cryptMethod::methodAES)
				AES_set_decrypt_key(key, bits, &aeskey);
			else
				DES_set_key_unchecked((const_DES_cblock*)key, &deskey);

			decrypt(input, outp, method, &aeskey, &deskey);				
			if (mode == 1 && !noIV)		//CBC
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

	array<unsigned char>^ Crypto::encryptAESorDES(array<unsigned char>^ Input, array<unsigned char>^ Key, array<unsigned char>^ IV, const int bits, const int length, const int mode, const int blockSize, const cryptMethod method)
	{
		int numBlocks = length / blockSize;		
		if (length % blockSize != 0)
			throw gcnew System::Exception("Input must be multiple of " + blockSize);

		bool noIV = false;

		if (IV == nullptr)
		{
			noIV = true;
			if (blockSize == 8)
				IV = zeroIV8;
			else if (blockSize == 16)
				IV = zeroIV16;
			else
				return nullptr;
		}

		pin_ptr<unsigned char> input = &Input[0];
		pin_ptr<unsigned char> key = &Key[0];
		pin_ptr<unsigned char> iv = &IV[0];

		array<unsigned char>^ output = gcnew array<unsigned char>(length);
		pin_ptr<unsigned char> outp = &output[0];

		array<unsigned char>^ block = nullptr;
		pin_ptr<unsigned char> blockp = nullptr;
		if (mode == 1)
		{
			block = gcnew array<unsigned char>(blockSize);
			blockp = &block[0];
		}		

		AES_KEY aeskey;
		DES_key_schedule deskey;
		if (mode == 2)	//CFB
		{			
			throw gcnew System::Exception("Encrypting CFB not supported (yet?)");
		}
		else	//CBC or ECB
		{
			if (method == cryptMethod::methodAES)
				AES_set_encrypt_key(key, bits, &aeskey);
			else
				DES_set_key_unchecked((const_DES_cblock*)key, &deskey);
						
			if (mode == 1 && !noIV)		//CBC
			{				
				for (int d = 0; d < blockSize; d++)
					block[d] = input[d];
				xorblock(blockp, iv, method);
				encrypt(blockp, outp, method, &aeskey, &deskey);
			}
			else
				encrypt(input, outp, method, &aeskey, &deskey);

			for (int c = 1; c < numBlocks; c++)
			{
				if (mode == 1)		//CBC
				{
					for (int d = 0; d < blockSize; d++)
						block[d] = input[c*blockSize+d];
					xorblock(blockp, outp+(c-1)*blockSize, method);
					encrypt(blockp, outp+c*blockSize, method, &aeskey, &deskey);
				}
				else
					encrypt(input+c*blockSize, outp+c*blockSize, method, &aeskey, &deskey);
			}
		}

		return output;
	}

	float *xlogx = 0;

	void prepareEntropy(int size)
    {
		if (xlogx != 0)
			free(xlogx);
        xlogx = (float*)malloc((size + 1)*sizeof(float));
        //precomputations for fast entropy calculation	
        xlogx[0] = 0.0;
        for (int i = 1; i <= size; i++)
			xlogx[i] = -1.0 * i * Math::Log(i / (float)size) / Math::Log(2.0);
    }


	double Crypto::calculateEntropy(array<unsigned char>^ text, int bytesToUse)
	{
        if (bytesToUse > text->Length)
            bytesToUse = text->Length;
		static int lastUsedSize = -1;

        if (lastUsedSize != bytesToUse)
        {
            try
            {
                prepareMutex->WaitOne();
                if (lastUsedSize != bytesToUse)
                {
                    prepareEntropy(bytesToUse);
                    lastUsedSize = bytesToUse;
                }
            }
            finally
            {
                prepareMutex->ReleaseMutex();
            }
        }

		pin_ptr<unsigned char> t = &text[0];

        int n[256];
		memset(n,0,sizeof(n));
        //count all ASCII symbols
        for (int counter = 0; counter < bytesToUse; counter++)
        {
            n[t[counter]]++;
        }

        float entropy = 0;
        //calculate probabilities and sum entropy
        for (short i = 0; i < 256; i++)			
            entropy += xlogx[n[i]];

        return entropy / (double)bytesToUse;
	}

}