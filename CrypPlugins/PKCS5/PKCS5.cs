//////////////////////////////////////////////////////////////////////////////////////////////////
// CrypTool V2
// © 2008 - Gerhard Junker
// Apache License see http://www.apache.org/licenses/
//
// $HeadURL: $
//////////////////////////////////////////////////////////////////////////////////////////////////
// $Revision:: 270                                                                            $://
// $Author:: gju                                                                              $://
// $Date:: 2008-09-08 11:59:41 +0200 (Mo, 08 Sep 2008)                                        $://
//////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Cryptool.PluginBase;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;

namespace PKCS5
{
    [Author("Gerhard Junker", null, "private project member", "http://nothing.info")]
    [PluginInfo(false, "PKCS#5", "PKCS#5 V2.1 Hash", "", "PKCS5/PKCS5.png")]
    public class PKCS5 : IHash
    {

        /// <summary>
        /// can only handle one input canal
        /// </summary>
        private enum dataCanal
        {
            /// <summary>
            /// nothing assigned
            /// </summary>
            none,
            /// <summary>
            /// using stream interface
            /// </summary>
            streamCanal,
            /// <summary>
            /// using byte array interface
            /// </summary>
            byteCanal
        };


        /// <summary>
        /// Initializes a new instance of the <see cref="PKCS5"/> class.
        /// </summary>
        public PKCS5()
        {
            this.settings = new PKCS5Settings();
        }

        #region Settings

        private PKCS5Settings settings;

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
                settings = (PKCS5Settings)value;
                OnPropertyChanged("Settings");
                NotifyUpdateKey();
                GuiLogMessage("Settings changed.", NotificationLevel.Debug);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has changes.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has changes; otherwise, <c>false</c>.
        /// </value>
        public bool HasChanges
        {
            get
            {
                return settings.HasChanges;
            }

            set
            {
                settings.HasChanges = value;
                GuiLogMessage("HasChanges changed.", NotificationLevel.Debug);
            }
        }

        #endregion

        #region Input key / password

        // Input key
        private byte[] key = { 0 };
        private dataCanal keyCanal = dataCanal.none;

        /// <summary>
        /// Notifies the update input.
        /// </summary>
        private void NotifyUpdateKey()
        {
            OnPropertyChanged("KeyStream");
            OnPropertyChanged("KeyData");
        }

        /// <summary>
        /// Gets or sets the input data.
        /// </summary>
        /// <value>The input key.</value>
        [PropertyInfo(Direction.Input, "Key Stream", "Key stream to be hashed", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public CryptoolStream KeyStream
        {
            get
            {
                CryptoolStream keyDataStream = new CryptoolStream();
                keyDataStream.OpenRead(this.GetPluginInfoAttribute().Caption, key);
                return keyDataStream;
            }
            set
            {
                if (keyCanal != dataCanal.none && keyCanal != dataCanal.streamCanal)
                    GuiLogMessage("Duplicate input key not allowed!", NotificationLevel.Error);
                keyCanal = dataCanal.streamCanal;

                long len = value.Length;
                key = new byte[len];

                for (long i = 0; i < len; i++)
                    key[i] = (byte)value.ReadByte();

                NotifyUpdateKey();
                GuiLogMessage("KeyStream changed.", NotificationLevel.Debug);
            }
        }

        /// <summary>
        /// Gets the input data.
        /// </summary>
        /// <value>The input data.</value>
        [PropertyInfo(Direction.Input, "Key Data", "Key stream to be hashed", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public byte[] KeyData
        {
            get
            {
                return key;
            }
            set
            {
                if (keyCanal != dataCanal.none && keyCanal != dataCanal.byteCanal)
                    GuiLogMessage("Duplicate key data not allowed!", NotificationLevel.Error);
                keyCanal = dataCanal.byteCanal;

                long len = value.Length;
                key = new byte[len];

                for (long i = 0; i < len; i++)
                    key[i] = value[i];

                NotifyUpdateKey();
                GuiLogMessage("KeyData changed.", NotificationLevel.Debug);
            }
        }
        #endregion

        #region Salt data / Seed data

        // Salt Data
        private byte[] salt = { 0 };
        private dataCanal saltCanal = dataCanal.none;

        /// <summary>
        /// Notifies the update salt.
        /// </summary>
        private void NotifyUpdateSalt()
        {
            OnPropertyChanged("SaltStream");
            OnPropertyChanged("SaltData");
        }

        /// <summary>
        /// Gets or sets the salt data.
        /// </summary>
        /// <value>The salt data.</value>
        [PropertyInfo(Direction.Input, "Salt Stream", "Salt - Input salt data to change the PKCS hash", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public CryptoolStream SaltStream
        {
            get
            {
                CryptoolStream saltDataStream = new CryptoolStream();
                saltDataStream.OpenRead(this.GetPluginInfoAttribute().Caption, salt);
                return saltDataStream;
            }
            set
            {
                if (saltCanal != dataCanal.none && saltCanal != dataCanal.streamCanal)
                    GuiLogMessage("Duplicate salt input not allowed!", NotificationLevel.Error);
                saltCanal = dataCanal.streamCanal;

                long len = value.Length;
                salt = new byte[len];

                for (long i = 0; i < len; i++)
                    salt[i] = (byte)value.ReadByte();

                NotifyUpdateSalt();
                GuiLogMessage("SaltStream changed.", NotificationLevel.Debug);
            }
        }

        /// <summary>
        /// Gets or sets the salt data.
        /// </summary>
        /// <value>The salt data.</value>
        [PropertyInfo(Direction.Input, "Salt Data", "Salt - Input salt data to to be change the PKCS hash", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public byte[] SaltData
        {
            get
            {
                return salt;
            }

            set
            {
                if (saltCanal != dataCanal.none && saltCanal != dataCanal.byteCanal)
                    GuiLogMessage("Duplicate salt input not allowed!", NotificationLevel.Error);
                saltCanal = dataCanal.byteCanal;

                long len = value.Length;
                salt = new byte[len];

                for (long i = 0; i < len; i++)
                    salt[i] = value[i];

                NotifyUpdateSalt();
                GuiLogMessage("SaltData changed.", NotificationLevel.Debug);
            }
        }
        #endregion

        #region Output

        // Output
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();
        private byte[] outputData = { 0 };

        /// <summary>
        /// Notifies the update output.
        /// </summary>
        private void NotifyUpdateOutput()
        {
            OnPropertyChanged("HashOutputStream");
            OnPropertyChanged("HashOutputData");
        }


        /// <summary>
        /// Gets or sets the output data stream.
        /// </summary>
        /// <value>The output data stream.</value>
        [PropertyInfo(Direction.Output, "Hashed Stream", "Output stream of the hashed value", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public CryptoolStream HashOutputStream
        {
            get
            {
                CryptoolStream outputDataStream = null;
                if (outputData != null)
                {
                    outputDataStream = new CryptoolStream();
                    outputDataStream.OpenRead(this.GetPluginInfoAttribute().Caption, outputData);
                    listCryptoolStreamsOut.Add(outputDataStream);
                }
                GuiLogMessage("Got HashOutputStream.", NotificationLevel.Debug);
                return outputDataStream;
            }
            //set
            //{
            //} //readonly
        }

        /// <summary>
        /// Gets the output data.
        /// </summary>
        /// <value>The output data.</value>
        [PropertyInfo(Direction.Output, "Hashed Data", "Output data of the hashed value", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
        public byte[] HashOutputData
        {
            get
            {
                GuiLogMessage("Got HashOutputData.", NotificationLevel.Debug);
                return this.outputData;
            }
            set
            {
                if (outputData != value)
                {
                    this.outputData = value;
                }
                NotifyUpdateOutput();
            }
        }

        /// <summary>
        /// Hashes this instance.
        /// </summary>
        public void Hash()
        {
            System.Security.Cryptography.PKCS5MaskGenerationMethod pkcs5Hash = new System.Security.Cryptography.PKCS5MaskGenerationMethod();

            pkcs5Hash.SelectedShaFunction = (PKCS5MaskGenerationMethod.ShaFunction)settings.SHAFunction;

            outputData = pkcs5Hash.GenerateMask(this.key, this.salt, settings.Count, pkcs5Hash.GetHashLength());

            NotifyUpdateOutput();
        }
        #endregion

        #region IPlugin Member

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
#pragma warning restore

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

        /// <summary>
        /// Gets the quick watch presentation - will be displayed inside of the plugin presentation-element. You
        /// can return the existing Presentation if it makes sense to display it inside a small area. But be aware that
        /// if Presentation is displayed in QuickWatchPresentation you can't open Presentation it in a tab before you
        /// you close QuickWatchPresentation;
        /// Return null if your plugin has no QuickWatchPresentation.
        /// </summary>
        /// <value>The quick watch presentation.</value>
        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Will be called from editor after restoring settings and before adding to workspace.
        /// </summary>
        public void Initialize()
        {
            GuiLogMessage("Initialize.", NotificationLevel.Debug);
        }

        /// <summary>
        /// Will be called from editor before right before chain-run starts
        /// </summary>
        public void PreExecution()
        {
            GuiLogMessage("PreExecution.", NotificationLevel.Debug);
        }

        /// <summary>
        /// Will be called from editor while chain-run is active and after last necessary input
        /// for plugin has been set.
        /// </summary>
        public void Execute()
        {
            GuiLogMessage("Execute.", NotificationLevel.Debug);
            Hash();
        }

        /// <summary>
        /// Will be called from editor after last plugin in chain has finished its work.
        /// </summary>
        public void PostExecution()
        {
            GuiLogMessage("PostExecution.", NotificationLevel.Debug);
        }

        /// <summary>
        /// Not defined yet.
        /// </summary>
        public void Pause()
        {
            GuiLogMessage("Pause.", NotificationLevel.Debug);
        }

        /// <summary>
        /// Will be called from editor while chain-run is active. Plugin hast to stop work immediately.
        /// </summary>
        public void Stop()
        {
            GuiLogMessage("Stop.", NotificationLevel.Debug);
        }

        /// <summary>
        /// Will be called from editor when element is deleted from worksapce.
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            foreach (CryptoolStream stream in listCryptoolStreamsOut)
            {
                stream.Close();
            }
            listCryptoolStreamsOut.Clear();
            GuiLogMessage("Dispose.", NotificationLevel.Debug);
        }

        #endregion

        #region INotifyPropertyChanged Member

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="name">The name.</param>
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                if (name == "Settings")
                    Hash();
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
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        #endregion
    }
}
