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
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Twofish
{
  [Author("Gerhard Junker", null, "private project member", null)]
  [PluginInfo(false, "Twofish", "Twofish -- cipher",
    "Twofish/DetailedDescription/Description.xaml", "Twofish/Images/Twofish.png", "Twofish/Images/encrypt.png", "Twofish/Images/decrypt.png")]
  [EncryptionType(EncryptionType.SymmetricBlock)]
  public class Twofish : IEncryption
  {
      private byte[] iv = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };


    /// <summary>
    /// Initializes a new instance of the <see cref="Twofish"/> class.
    /// </summary>
    public Twofish()
    {
      settings = new TwofishSettings();
    }

    #region IPlugin Member


#pragma warning disable 67
    public event StatusChangedEventHandler  OnPluginStatusChanged;
    public event GuiLogNotificationEventHandler  OnGuiLogNotificationOccured;
    public event PluginProgressChangedEventHandler  OnPluginProgressChanged;
#pragma warning restore


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

    public System.Windows.Controls.UserControl QuickWatchPresentation
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
      Crypt();
    }

    public void PostExecution()
    {
    }

    public void Pause()
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
      foreach (CryptoolStream stream in listCryptoolStreamsOut)
      {
        stream.Close();
      }
      listCryptoolStreamsOut.Clear();
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
		[PropertyInfo(Direction.InputData, "Input Data Stream", "Input data stream to process", "", 
      false, false, QuickWatchFormat.Hex, null)]
		public CryptoolStream InputStream
		{
			get
			{
				CryptoolStream inputStream = new CryptoolStream();
				inputStream.OpenRead(this.GetPluginInfoAttribute().Caption, inputdata);
				return inputStream;
			}
			set
			{
        if (null == value)
        {
          inputdata = new byte[0];
          return;
        }

				long len = value.Length;
				inputdata = new byte[len];

				for (long i = 0; i < len; i++)
					inputdata[i] = (byte)value.ReadByte();

				NotifyUpdateInput();
			}
		}

		/// <summary>
		/// Gets the input data.
		/// </summary>
		/// <value>The input data.</value>
		[PropertyInfo(Direction.InputData, "Input Data", "Input Data to process", "", 
      false, false, QuickWatchFormat.Hex, null)]
		public byte[] InputData
		{
			get
			{
				return inputdata;
			}
			set
			{
        if (null == value)
        {
          inputdata = new byte[0];
          return;
        }
				long len = value.Length;
				inputdata = new byte[len];

				for (long i = 0; i < len; i++)
					inputdata[i] = value[i];

				NotifyUpdateInput();
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
    [PropertyInfo(Direction.InputData, "Key Stream", "Key - Input key data", 
      "", false, false, QuickWatchFormat.Hex, null)]
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
        if (null == value)
          return;

        long len = value.Length;
        key = new byte[len];

        for (long i = 0; i < len; i++)
          key[i] = (byte)value.ReadByte();

        NotifyUpdateKey();
        GuiLogMessage("KeyStream changed.", NotificationLevel.Debug);
      }
    }

    /// <summary>
    /// Gets or sets the key data.
    /// </summary>
    /// <value>The key data.</value>
    [PropertyInfo(Direction.InputData, "Key Data", "Key - Input key data", 
      "", false, false, QuickWatchFormat.Hex, null)]
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

    [PropertyInfo(Direction.InputData, "IV", "Initialization Vector", "",
    false, false, QuickWatchFormat.Hex, null)]
    public byte[] IV
    {
        get
        {
            return iv;
        }
        set
        {
            Array.Clear(iv, 0, iv.Length);

            if (null == value)
                return;

            for (int i = 0; i < value.Length && i < iv.Length; i++)
                iv[i] = value[i];

            NotifyUpdateInput();
            GuiLogMessage("InputData changed.", NotificationLevel.Debug);
        }
    }



    #region Output

    // Output
    private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();
    private byte[] outputData = { };

    /// <summary>
    /// Notifies the update output.
    /// </summary>
    private void NotifyUpdateOutput()
    {
      OnPropertyChanged("OutputStream");
      OnPropertyChanged("OutputData");
    }


    /// <summary>
    /// Gets or sets the output inputdata stream.
    /// </summary>
    /// <value>The output inputdata stream.</value>
    [PropertyInfo(Direction.OutputData, "Output Stream", "Output stream", "",
      true, false, QuickWatchFormat.Hex, null) ]
    public CryptoolStream OutputStream
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
        return outputDataStream;
      }
    }

    /// <summary>
    /// Gets the output inputdata.
    /// </summary>
    /// <value>The output inputdata.</value>
    [PropertyInfo(Direction.OutputData, "Output Data", "Output data", "",
      true, false, QuickWatchFormat.Hex, null)]
    public byte[] OutputData
    {
      get
      {
        return this.outputData;
      }
    }

    #endregion



    #region INotifyPropertyChanged Member

    public event PropertyChangedEventHandler  PropertyChanged;
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


    private void Crypt()
    {
      // fit key to correct length
      byte[] k2 = new byte[settings.KeyLength / 8];
      for (int i = 0; i < settings.KeyLength / 8; i++)
        if (i < key.Length)
          k2[i] = key[i];
        else
          k2[i] = 0;
    

      TwofishManaged tf = TwofishManaged.Create();

      tf.Mode = (settings.Mode == 0) ? CipherMode.CBC : CipherMode.ECB;
      
      int pos = 0;

      int len = inputdata.Length;
      if (settings.Action == 0) // input padding
      {
          if (len % 16 != 0)
              len += (16 - len % 16);
      }
      if (outputData.Length != len)  
        outputData = new byte[len];

      switch (settings.Action)
      {
        case 0: // encrypt
          {
            ICryptoTransform encrypt =  tf.CreateEncryptor(k2, iv);

            while (inputdata.Length - pos > encrypt.InputBlockSize)
            {
              pos += encrypt.TransformBlock(inputdata, pos, encrypt.InputBlockSize, outputData, pos);
            }
            byte[] final = encrypt.TransformFinalBlock(inputdata, pos, inputdata.Length - pos);
            Array.Copy(final, 0, outputData, pos, 16);
            encrypt.Dispose();
            break;
          }
        case 1: // decrypt
          {
            ICryptoTransform decrypt =  tf.CreateDecryptor(k2, iv);

            while (inputdata.Length - pos > decrypt.InputBlockSize)
            {
              pos += decrypt.TransformBlock(inputdata, pos, decrypt.InputBlockSize, outputData, pos);
            }
            if (inputdata.Length - pos >  0)
            {
              byte[] final = decrypt.TransformFinalBlock(inputdata, pos, inputdata.Length - pos);

              for (int i = pos; i < outputData.Length; i++)
                outputData[i] = final[i - pos];
            }

            decrypt.Dispose();
            break;
          }
      }

      NotifyUpdateOutput();
    }

  }
}
