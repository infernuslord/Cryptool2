using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PKCS1.OnlineHelp
{
    public enum OnlineHelpActions
    {
        None,
        KeyGen,
        SigGen,
        SigGenFakeBleichenbacher,
        SigGenFakeKuehn,
        SigVal,
        Gen_Datablock_Tab,
        Gen_PKCS1_Sig_Tab,
        Gen_Bleichenb_Sig_Tab,
        Gen_Kuehn_Sig_Tab,
        Gen_Kuehn_Iterations,
        KeyGen_PubExponent,
        KeyGen_ModulusSize,
        StartControl,
        Start
    }
}
