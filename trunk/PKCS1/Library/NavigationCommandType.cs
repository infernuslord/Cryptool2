using System;
using System.Collections.Generic;
using System.Text;

namespace PKCS1.Library
{
    public enum NavigationCommandType
    {
        None,
        Start,
        RsaKeyGen,
        SigGen,
        SigGenFakeBleichenb,
        SigGenFakeShort,
        SigVal
    }
}
