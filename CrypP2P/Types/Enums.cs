using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.P2P.Types
{
    public enum P2PLinkManagerType
    {
        Snal = 0
    }

    public enum P2PBootstrapperType
    {
        LocalMachineBootstrapper = 0,
        IrcBootstrapper = 1,
        DnsBootstrapper = 2
    }

    public enum P2PArchitecture
    {
        FullMesh = 0,
        Chord = 1,
        Server = 2,
        WebDHT = 3
    }
}
