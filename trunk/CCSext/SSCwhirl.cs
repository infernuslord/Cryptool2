//////////////////////////////////////////////////////////////////////////////////////////////////
// CrypTool V2
// © 2008 - Gerhard Junker
// Apache License see http://www.apache.org/licenses/
//
// $HeadURL$
//////////////////////////////////////////////////////////////////////////////////////////////////
// $Revision::                                                                                $://
// $Author::                                                                                  $://
// $Date::                                                                                    $://
//////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

using System.Security.Cryptography;
using System.Runtime.InteropServices;

#if DEBUG
using System.Diagnostics;
#endif

namespace System.Security.Cryptography
{
	class Whirlpool: HashAlgorithm
	{
		protected override void HashCore(byte[] array, int ibStart, int cbSize)
		{
			throw new NotImplementedException();
		}

		protected override byte[] HashFinal()
		{
			throw new NotImplementedException();
		}

		public override void Initialize()
		{
			throw new NotImplementedException();
		}
	}
}
