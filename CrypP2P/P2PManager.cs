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
using System.Text;
using System.ComponentModel;

using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.P2P
{
    public sealed class P2PManager
    {
        #region Singleton
        static readonly P2PManager INSTANCE = new P2PManager();
        static P2PManager() {}
        P2PManager() {}

        public static P2PManager Instance
        {
            get
            {
                return INSTANCE;
            }
        }
        #endregion

        # region Constants
        public const string P2PDisconnectImageURI = "images/peer2peer-disconnect.png";
        public const string P2PConnectImageURI = "images/peer2peer-connect.png";
        # endregion

        #region Private variables
        private bool isP2PNetworkConnected = false;
        #endregion

        public void initialize()
        {
            // TODO P2P init code
        }

        public void setConnectionState(bool newState)
        {
            // TODO implement logic for handling connects/disconnects
            isP2PNetworkConnected = newState;
        }

        public bool getP2PConnectionState()
        {
            return isP2PNetworkConnected;
        }
    }
}
