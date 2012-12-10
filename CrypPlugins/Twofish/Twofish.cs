//////////////////////////////////////////////////////////////////////////////////////////////////
// CrypTool V2
// © 2008 - Gerhard Junker
// Apache License see http://www.apache.org/licenses/
//
// $HeadURL: https://www.cryptool.org/svn/CrypTool2/trunk/SSCext/TwofishBase.cs $
//////////////////////////////////////////////////////////////////////////////////////////////////
// $Revision:: 157                                                                            $://
// $Author:: junker                                                                           $://
// $Date:: 2008-12-17 08:07:48 +0100 (Mi, 17 Dez 2008)                                        $://
//////////////////////////////////////////////////////////////////////////////////////////////////

// more about at http://www.schneier.com/twofish.html

using System;
using System.Collections.Generic;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Twofish
{
    [Author("Gerhard Junker", null, "private project member", null)]
    [PluginInfo("Twofish.Properties.Resources", "PluginCaption", "PluginTooltip", "Twofish/DetailedDescription/doc.xml",
        "Twofish/Images/Twofish.png", "Twofish/Images/encrypt.png", "Twofish/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class Twofish : ICrypComponent
    {
        private byte[] iv = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private CryptoStream p_crypto_stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="Twofish"/> class.
        /// </summary>
        public Twofish()
        {
            settings = new TwofishSettings();
        }

        #region IPlugin Member


#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
#pragma warning restore

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        TwofishSettings settings;
        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        /// <value>The settings.</value>
        public ISettings Settings
        {
            get
            {
                return settings;
            }
            set
            {
                settings = (TwofishSettings)value;
                OnPropertyChanged("Settings");
            }
        }

        /// <summary>
        /// Provide all presentation stuff in this user control, it will be opened in an tab.
        /// Return null if your plugin has no presentation.
        /// </summary>
        /// <value>The presentation.</value>
        public System.Windows.Controls.UserControl Presentation
        {
            get
            {
                return null;
            }
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
            Progress(0.0, 1.0);

            Crypt();

            Progress(1.0, 1.0);
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        /// <summary>
        /// Will be called from editor when element is deleted from worksapce.
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (p_crypto_stream != null)
                {
                    p_crypto_stream.Flush();
                    p_crypto_stream.Clear();
                    p_crypto_stream = null;
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        #endregion


        #region Input inputdata

        // Input inputdata
        private byte[] inputdata = { };

        /// <summary>
        /// Notifies the update input.
        /// </summary>
        private void NotifyUpdateInput()
        {
            OnPropertyChanged("InputStream");
            OnPropertyChanged("InputData");
        }

        /// <summary>
        /// Gets or sets the input inputdata.
        /// </summary>
        /// <value>The input inputdata.</value>
        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", true)]
        public ICryptoolStream InputStream
        {
            get
            {
                if (inputdata == null)
                {
                    return null;
                }
                else
                {
                    return new CStreamWriter(inputdata);
                }
            }
            set
            {
                if (value != null)
                {
                    using (CStreamReader reader = value.CreateReader())
                    {
                        inputdata = reader.ReadFully();
                    }

                    NotifyUpdateInput();
                }
            }
        }

        #endregion

        #region Key data

        // Salt Data
        private byte[] key = { };

        /// <summary>
        /// Notifies the update key.
        /// </summary>
        private void NotifyUpdateKey()
        {
            OnPropertyChanged("KeyStream");
            OnPropertyChanged("KeyData");
        }
        
        /// <summary>
        /// Gets or sets the key data.
        /// </summary>
        /// <value>The key data.</value>
        [PropertyInfo(Direction.InputData, "KeyDataCaption", "KeyDataTooltip", true)]
        public byte[] KeyData
        {
            get
            {
                return key;
            }

            set
            {
                long len = value.Length;
                key = new byte[len];

                for (long i = 0; i < len; i++)
                    key[i] = value[i];

                NotifyUpdateKey();
                GuiLogMessage("KeyData changed.", NotificationLevel.Debug);
            }
        }

        #endregion

        [PropertyInfo(Direction.InputData, "IVCaption", "IVTooltip", false)]
        public byte[] IV
        {
            get
            {
                return iv;
            }
            set
            {
                Array.Clear(iv, 0, iv.Length);

                if (value == null) return;

                Array.Copy(value, iv, Math.Min(iv.Length, value.Length));

                NotifyUpdateInput();
                GuiLogMessage("IV changed.", NotificationLevel.Debug);
            }
        }

        #region Output

        // Output
        private CStreamWriter outputStreamWriter;
        
        /// <summary>
        /// Gets or sets the output inputdata stream.
        /// </summary>
        /// <value>The output inputdata stream.</value>
        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
        public ICryptoolStream OutputStream
        {
            get
            {
                return outputStreamWriter;
            }
        }
        
        #endregion

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="name">The name.</param>
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                if (name == "Settings")
                {
                    Crypt();
                }
                else
                    PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// GUIs the log message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="logLevel">The log level.</param>
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this,
              new GuiLogEventArgs(message, this, logLevel));
        }

        #endregion

        private void ConfigureAlg(SymmetricAlgorithm alg)
        {   
            // fit key to correct length
            if( key.Length < settings.KeyLength / 8 )
                GuiLogMessage("The supplied key is too short, padding with zero bits.", NotificationLevel.Warning);
            else if (key.Length > settings.KeyLength / 8)
                GuiLogMessage("The supplied key is too long, ignoring extra bits.", NotificationLevel.Warning);

            byte[] k2 = new byte[settings.KeyLength / 8];
            for (int i = 0; i < settings.KeyLength / 8; i++)
                k2[i] = (i < key.Length) ? key[i] : (byte)0;
            alg.Key = k2;

            //switch (settings.Mode)
            //{
            //    case 1: alg.Mode = CipherMode.CBC; break;
            //    default: alg.Mode = CipherMode.ECB; break;
            //}

            alg.Mode = CipherMode.ECB;
            alg.Padding = PaddingMode.None;
        }

        private void Crypt()
        {
            try
            {
                ICryptoTransform p_encryptor;
                SymmetricAlgorithm p_alg = TwofishManaged.Create();

                ConfigureAlg(p_alg);

                outputStreamWriter = new CStreamWriter();
                ICryptoolStream inputdata = InputStream;

                if (settings.Action == 0)
                    inputdata = BlockCipherHelper.AppendPadding(InputStream, settings.padmap[settings.Padding], p_alg.BlockSize / 8);

                CStreamReader reader = inputdata.CreateReader();

                byte[] tmpInput = BlockCipherHelper.StreamToByteArray(inputdata);
                byte[] outputData = new byte[tmpInput.Length];
                byte[] IV = new byte[p_alg.IV.Length];
                Array.Copy(p_alg.IV, IV, p_alg.IV.Length);
                int bs = p_alg.BlockSize >> 3;

                if (settings.Mode == 0) // ECB
                {
                    p_encryptor = (settings.Action==0) ? p_alg.CreateEncryptor(p_alg.Key, p_alg.IV) : p_alg.CreateDecryptor(p_alg.Key, p_alg.IV);
                    for (int pos = 0; pos < tmpInput.Length; pos += bs)
                    {
                        p_encryptor.TransformBlock(tmpInput, pos, bs, outputData, pos);
                    }
                } 
                else if (settings.Mode == 1) // CBC
                {
                    if (settings.Action == 0)
                    {
                        p_encryptor = p_alg.CreateEncryptor(p_alg.Key, p_alg.IV);
                        for (int pos = 0; pos < tmpInput.Length; pos += bs)
                        {
                            for (int i = 0; i < bs; i++) tmpInput[pos + i] ^= IV[i];
                            p_encryptor.TransformBlock(tmpInput, pos, bs, outputData, pos);
                            for (int i = 0; i < bs; i++) IV[i] = outputData[pos + i];
                        }
                    }
                    else
                    {
                        p_encryptor = p_alg.CreateDecryptor(p_alg.Key, p_alg.IV);
                        for (int pos = 0; pos < tmpInput.Length; pos += bs)
                        {
                            p_encryptor.TransformBlock(tmpInput, pos, bs, outputData, pos);
                            for (int i = 0; i < bs; i++)
                            {
                                outputData[pos + i] ^= IV[i];
                                IV[i] = tmpInput[pos + i];
                            }
                        }
                    }
                }
                else if (settings.Mode == 2) // CFB
                {
                    p_encryptor = p_alg.CreateEncryptor(p_alg.Key, p_alg.IV);
                    if (settings.Action == 0)
                    {
                        for (int pos = 0; pos < tmpInput.Length; pos += bs)
                        {
                            p_encryptor.TransformBlock(IV, 0, p_encryptor.InputBlockSize, outputData, pos);
                            for (int i = 0; i < bs; i++)
                            {
                                outputData[pos + i] ^= tmpInput[pos + i];
                                IV[i] = outputData[pos + i];
                            }
                        }
                    }
                    else
                    {
                        for (int pos = 0; pos < tmpInput.Length; pos += bs)
                        {
                            p_encryptor.TransformBlock(IV, 0, p_encryptor.InputBlockSize, outputData, pos);
                            for (int i = 0; i < bs; i++)
                            {
                                IV[i] = tmpInput[pos + i];
                                outputData[pos + i] ^= tmpInput[pos + i];
                            }
                        }
                    }
                } 
                else if (settings.Mode == 3) // OFB
                {
                    p_encryptor = p_alg.CreateEncryptor(p_alg.Key, p_alg.IV);
                    for (int pos = 0; pos < tmpInput.Length; pos += bs)
                    {
                        p_encryptor.TransformBlock(IV, 0, p_encryptor.InputBlockSize, outputData, pos);
                        for (int i = 0; i < bs; i++)
                        {
                            IV[i] = outputData[pos + i];
                            outputData[pos + i] ^= tmpInput[pos + i];
                        }
                    }
                }

                outputStreamWriter.Write(outputData);

                //if( p_encryptor!=null ) p_encryptor.Dispose();
                outputStreamWriter.Close();

                if (settings.Action == 1)
                    outputStreamWriter = BlockCipherHelper.StripPadding(outputStreamWriter, settings.padmap[settings.Padding], p_alg.BlockSize / 8) as CStreamWriter;

                OnPropertyChanged("OutputStream");
            }
            catch (CryptographicException cryptographicException)
            {
                // TODO: For an unknown reason p_crypto_stream can not be closed after exception.
                // Trying so makes p_crypto_stream throw the same exception again. So in Dispose 
                // the error messages will be doubled. 
                // As a workaround we set p_crypto_stream to null here.
                p_crypto_stream = null;
                GuiLogMessage(cryptographicException.Message, NotificationLevel.Error);
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }
    }
}
