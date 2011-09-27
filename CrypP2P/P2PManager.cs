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
using System.Reflection;
using Cryptool.P2P.Interfaces;
using Cryptool.P2P.Types;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using System.IO;

namespace Cryptool.P2P
{
    public sealed class P2PManager
    {
        #region Variables

        public static IConnectionManager ConnectionManager { 
            get
            {
                if (!IsP2PSupported)
                {
                    throw new P2PNotSupportedException();
                }
                return (IConnectionManager) DLLP2PManagerType.GetProperty("ConnectionManager").GetValue(DLLP2PManagerType, null);
            }
        }
        public static IP2PBase P2PBase
        {
            get
            {
                if (!IsP2PSupported)
                {
                    throw new P2PNotSupportedException();
                }
                return (IP2PBase)DLLP2PManagerType.GetProperty("P2PBase").GetValue(DLLP2PManagerType, null);
            }
        }
        public static bool IsAutoconnectConsoleOptionSet
        {
            get
            {
                if (!IsP2PSupported)
                {
                    throw new P2PNotSupportedException();
                }
                return (bool)DLLP2PManagerType.GetProperty("IsAutoconnectConsoleOptionSet").GetValue(DLLP2PManagerType, null);
            }
            set
            {
                if (!IsP2PSupported)
                {
                    throw new P2PNotSupportedException();
                }
                DLLP2PManagerType.GetProperty("IsAutoconnectConsoleOptionSet").SetValue(null, value, null);
            }
        }
        public static UInt64 NetSize
        {
            get
            {
                if (!IsP2PSupported)
                {
                    throw new P2PNotSupportedException();
                }
                return (UInt64)DLLP2PManagerType.GetProperty("NetSize").GetValue(DLLP2PManagerType, null);
            }
        }

        public static string Password { 
            get
            {
                if (!IsP2PSupported)
                {
                    throw new P2PNotSupportedException();
                }
                return (string) DLLP2PBaseType.GetProperty("Password").GetValue(null, null);
            }
            set
            {
                if (!IsP2PSupported)
                {
                    throw new P2PNotSupportedException();
                }
                DLLP2PBaseType.GetProperty("Password").SetValue(null, value, null);
            }
        }

        public static bool IsP2PSupported { get; private set; }

        #endregion

        #region Events

        public static event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        #endregion Events


        #region Singleton

        //public static readonly P2PManager Instance = new P2PManager();
        private static Type DLLP2PManagerType;
        private static Type DLLP2PBaseType;

        static P2PManager()
        {
            IsP2PSupported = true;

            try
            {
                var assembly = Assembly.LoadFrom(Path.Combine(DirectoryHelper.BaseDirectory, "CrypP2PDLL.dll"));
                DLLP2PManagerType = assembly.GetType("Cryptool.P2PDLL.P2PManager");
                DLLP2PBaseType = assembly.GetType("Cryptool.P2PDLL.Internal.P2PBase");
                DLLP2PManagerType.GetEvent("OnGuiLogNotificationOccured").AddEventHandler(null, OnGuiLogNotificationOccured);
            }
            catch (Exception)
            {
                IsP2PSupported = false;
            }
        }

        #endregion

        
        #region CrypWin helper methods

        public static void HandleConnectOnStartup()
        {
            if (!IsP2PSupported)
            {
                throw new P2PNotSupportedException();
            }
            DLLP2PManagerType.GetMethod("HandleConnectOnStartup").Invoke(null, null);
        }

        public static void HandleDisconnectOnShutdown()
        {
            if (!IsP2PSupported)
            {
                throw new P2PNotSupportedException();
            }
            DLLP2PManagerType.GetMethod("HandleDisconnectOnShutdown").Invoke(null, null);
        }

        public static IRequestResult GetSuccessfullRequestResult()
        {
            if (!IsP2PSupported)
            {
                throw new P2PNotSupportedException();
            }
            return (IRequestResult) DLLP2PManagerType.GetMethod("GetSuccessfullRequestResult").Invoke(null, null);
        }

        #endregion

        #region DHT operations (blocking)

        /// <summary>
        /// Stores the given data in the DHT. This method will block until a response has been received.
        /// 
        /// The underlying DHT is versionend. Store attempts will fail, if the latest version has not been retrieved before.
        /// </summary>
        /// <param name="key">key to write</param>
        /// <param name="data">data to write</param>
        /// <exception cref="NotConnectedException">Will be thrown if the P2P system is not connected</exception>
        /// <returns>true if the store attempt was successful, false otherwise</returns>
        public static IRequestResult Store(string key, byte[] data)
        {
            if (!IsP2PSupported)
            {
                throw new P2PNotSupportedException();
            }
            return (IRequestResult) DLLP2PManagerType.GetMethod("Store", new Type[] {typeof(string), typeof(byte[])}).Invoke(null, new object[] {key, data});
        }

        /// <summary>
        /// Stores the given data in the DHT. This method will block until a response has been received.
        /// 
        /// The underlying DHT is versionend. Store attempts will fail, if the latest version has not been retrieved before.
        /// </summary>
        /// <param name="key">key to write</param>
        /// <param name="data">data to write</param>
        /// <exception cref="NotConnectedException">Will be thrown if the P2P system is not connected</exception>
        /// <returns>true if the store attempt was successful, false otherwise</returns>
        public static IRequestResult Store(string key, string data)
        {
            if (!IsP2PSupported)
            {
                throw new P2PNotSupportedException();
            }
            return (IRequestResult)DLLP2PManagerType.GetMethod("Store", new Type[] { typeof(string), typeof(string) }).Invoke(null, new object[] { key, data });
        }

        /// <summary>
        /// Retrieves the latest version of a given in key from the DHT. This method will block until a response has been received.
        /// </summary>
        /// <param name="key">key to retrieve</param>
        /// <exception cref="NotConnectedException">Will be thrown if the P2P system is not connected</exception>
        /// <returns>byte array containing the data</returns>
        public static IRequestResult Retrieve(string key)
        {
            if (!IsP2PSupported)
            {
                throw new P2PNotSupportedException();
            }
            return (IRequestResult)DLLP2PManagerType.GetMethod("Retrieve", new Type[] { typeof(string) }).Invoke(null, new object[] { key });
        }

        /// <summary>
        /// Removes a key and its data from the DHT. This method will block until a response has been received.
        /// 
        /// The underlying DHT is versionend. Remove attempts will fail, if the latest version has not been retrieved before.
        /// </summary>
        /// <param name="key">key to remove</param>
        /// <exception cref="NotConnectedException">Will be thrown if the P2P system is not connected</exception>
        /// <returns>bool determining wether the attempt was successful</returns>
        public static IRequestResult Remove(string key)
        {
            if (!IsP2PSupported)
            {
                throw new P2PNotSupportedException();
            }
            return (IRequestResult)DLLP2PManagerType.GetMethod("Remove", new Type[] { typeof(string) }).Invoke(null, new object[] { key });
        }

        #endregion

        #region Connection methods

        /// <summary>
        /// Connect to the peer-to-peer network.
        /// <see cref="ConnectionManager">OnP2PConnectionStateChangeOccurred will be fired when the connection state changes.</see>
        /// </summary>
        public static void Connect()
        {
            if (!IsP2PSupported)
            {
                throw new P2PNotSupportedException();
            }
            DLLP2PManagerType.GetMethod("Connect").Invoke(null, null);
        }

        /// <summary>
        /// Disconnect from the peer-to-peer network.
        /// <see cref="ConnectionManager">OnP2PConnectionStateChangeOccurred will be fired when the connection state changes.</see>
        /// </summary>
        public static void Disconnect()
        {
            if (!IsP2PSupported)
            {
                throw new P2PNotSupportedException();
            }
            DLLP2PManagerType.GetMethod("Disconnect").Invoke(null, null);
        }

        /// <summary>
        /// Boolean which indicates, if the peer-to-peer network is currently connected and not in a connect/disconnect attempt.
        /// </summary>
        public static bool IsConnected
        {
            get
            {
                if (!IsP2PSupported)
                {
                    throw new P2PNotSupportedException();
                }
                return (bool)DLLP2PManagerType.GetProperty("IsConnected").GetValue(DLLP2PManagerType, null);
            }
        }

        /// <summary>
        /// Boolean which indicates, if the peer-to-peer network is currently in a connect/disconnect attempt.
        /// </summary>
        public static bool IsConnecting
        {
            get
            {
                if (!IsP2PSupported)
                {
                    throw new P2PNotSupportedException();
                }
                return (bool)DLLP2PManagerType.GetProperty("IsConnecting").GetValue(DLLP2PManagerType, null);
            }
        }

        #endregion
    }
}