#include "NTL_Wrapper.h"
#include "NTL/LLL.h"
#include <iostream>
using namespace System::Numerics; 
using namespace std;
using namespace System;

namespace NTL
{
	public ref class NTL_Wrapper
	{			

	public:
		NTL_Wrapper()
		{			
		}
		~NTL_Wrapper()
		{    
		}

		array<BigInteger,2>^ LLLReduce (array<BigInteger,2>^ matrix, long dim, double delta)
		{
			return LLLReduce (matrix, dim, dim, delta);
		}

		array<BigInteger,2>^ LLLReduce (array<BigInteger,2>^ matrix, long n, long m, double delta)
		{
			mat_ZZ B, U;					

			B.SetDims(n, m);

			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= m; j++) 
				{		
					B(i, j) = conv<ZZ>((char *) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(matrix[i - 1, j - 1].ToString()).ToPointer());
				}
			}

			LLL_FP(B, U, delta);

			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= m; j++) 
				{	
					matrix[i-1, j-1] = ConvertFromZZToBigInt(B(i, j));
				}
			}

			array<BigInteger,2>^ transMatrix  = gcnew array<BigInteger,2>(n, n);
			
			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= n; j++) 
				{	
					transMatrix[i-1, j-1] = ConvertFromZZToBigInt(U(i, j));
				}
			}
			return transMatrix;
		}

		BigInteger ConvertFromZZToBigInt(ZZ zz)
		{
			String^ result ("");	
			Char tmp;
			bool setMinus = false;

			if (zz < 0)
			{
				setMinus = true;
				zz *= -1;
			}

			while(zz > 0)
			{
				tmp = INTenc(to_int(zz % to_ZZ(10)));
				if(tmp!=0)
					result = System::String::Concat(tmp, result);

				zz /= 10;
			}

			if (setMinus)
				result = System::String::Concat("-", result);
			if (result == "")
				result = "0";

			return BigInteger::Parse(result);
		}


		char INTenc(int v)
		{
			char* tab = "0123456789";
			if (v>=0 && v<10)
				return tab[v];
			else
				return 0;
		}

		BigInteger Determinant (array<BigInteger,2>^ matrix, int dim)
		{
			mat_ZZ B;

			B.SetDims(dim, dim);

			for (int i = 1; i <= dim; i++)
			{
				for (int j = 1; j <= dim; j++) 
				{		
					B(i,j) = conv<ZZ>((char *) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(matrix[i - 1,j - 1].ToString()).ToPointer());
				}
			}

			ZZ det = determinant(B, 0);
			
			return ConvertFromZZToBigInt(det);	
		}

		BigInteger ModInverse (BigInteger a, BigInteger mod)
		{
			ZZ aZZ, modZZ;

			aZZ = conv<ZZ>((char *) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(a.ToString()).ToPointer());
			modZZ = conv<ZZ>((char *) System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(mod.ToString()).ToPointer());

			ZZ invModZZ = InvMod(aZZ, modZZ);

			return ConvertFromZZToBigInt(invModZZ);	
		}
	};
}