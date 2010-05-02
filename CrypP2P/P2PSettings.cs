/*
   Copyright 2010 Paul Lelgemann, University of Duisburg-Essen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.P2P.Helper;
using Cryptool.P2P.Internal;

namespace Cryptool.P2P
{
    public class P2PSettings
    {
        public P2PBase Base { get; set; }
        public string PeerName { get; set; }
        public string WorldName { get; set; }
        public bool Log2Monitor { get; set; }
        public P2PLinkManagerType LinkManagerType { get; set; }
        public P2PBootstrapperType BSType { get; set; }
        public P2POverlayType OverlayType { get; set; }
        public P2PDHTType DhtType { get; set; }

        public P2PSettings()
        {
            PeerName = PAPCertificate.CERTIFIED_PEER_NAME;
            WorldName = "CrypTool";
            Log2Monitor = true;
            LinkManagerType = P2PLinkManagerType.Snal;
            BSType = P2PBootstrapperType.IrcBootstrapper;
            OverlayType = P2POverlayType.FullMeshOverlay;
            DhtType = P2PDHTType.FullMeshDHT;
        }
    }
}
