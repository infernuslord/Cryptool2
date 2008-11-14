using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Cryptool.PluginBase;

namespace Cryptool.RSA
{
    public class RSASettings : IEncryptionAlgorithmSettings
    {
        private Stream inputData;
        private RSAParameters rsaKeyInfo;

        [ControlType(ControlType.TextBox, DisplayLevel.Beginner, true, "", "", new string[] { })]
        public Stream InputData
        {
            get { return this.inputData; }
            set { this.inputData = value; }
        }

        [ControlType(ControlType.TextBox, DisplayLevel.Beginner, true, "", "", new string[] { })]
        public RSAParameters RsaKeyInfo
        {
            get { return this.rsaKeyInfo; }
            set { this.rsaKeyInfo = value; }
        }
    }
}
