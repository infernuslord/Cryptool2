using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Controls;
using Cryptool.PluginBase;


namespace Cryptool.AESTeaching
{
    [PluginInfo("A453D58A-ECE2-45cf-A54A-93050F71C2E7", 
        "AES (from Scratch)",
        "The Advanced Encryption Standard (AES), also known as Rijndael, is a block cipher adopted as an encryption standard by the U.S. government. It has been analyzed extensively and is now used widely worldwide as was the case with its predecessor, the Data Encryption Standard (DES). AES was announced by National Institute of Standards and Technology (NIST) as U.S. FIPS PUB 197 (FIPS 197) on November 26, 2001 after a 5-year standardization process (see Advanced Encryption Standard process for more details). It became effective as a standard May 26, 2002. As of 2006, AES is one of the most popular algorithms used in symmetric key cryptography. It is available by choice in many different encryption packages.",
        "Cryptool.AESTeaching.icon.png"        
        )]

    public partial class AESTeaching : IEncryptionAlgorithm
    {
        #region Private variables
        private AESSettings settings;
        private Stream inputData;
        private Stream outputData;
        private UserControlAES AESUserControl;
        #endregion

        #region AESTeaching Public interface
        
        public AESTeaching()
        {
            this.settings = new AESSettings();
            this.AESUserControl = new UserControlAES();
        }


        [Input("Data Input", "Data to be encrypted with AES")]
        public Stream InputData
        {
            get { return this.inputData; }
            set { this.inputData = value; }
        }

        [Output("Data Output", "AES encrypted data")]
        public Stream OutputData
        {
            get { return this.outputData; }
            set { this.outputData = value; }
        }

        #region IEncryptionAlgorithm Members

        public IEncryptionAlgorithmSettings Settings
        {
            get
            {
                return this.settings;
            }
            set
            {
                this.settings = (AESSettings)value;
            }
        }

        public void Encrypt()
        {
            throw new NotImplementedException();
        }

        public void Decrypt()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IPlugin Members

        public event StatusBarTextChangedHandler OnStatusBarTextChanged;

        public event StatusBarProgressbarValueChangedHandler OnStatusBarProgressbarValueChanged;

        public System.Windows.Controls.UserControl PresentationControl
        {
            get { return (UserControl)AESUserControl; }
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
        
        #endregion

        #region AESTeaching Private Methods





        #endregion



    }

}
