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
using System.Collections;
using System.IO;
using System.ComponentModel;


namespace Twofish
{
  [Author("Gerhard Junker", null, "private project member", null)]
  [PluginInfo(false, "Twofish", "Twofish -- cipher",
    "http://www.schneier.com/twofish.html", "twofish/twofish1.png")]
  [EncryptionType(EncryptionType.SymmetricBlock)]
  public class Twofish : IEncryption
  {

    public Twofish()
    {
      settings = new TwofishSettings();

      ASCIIEncoding enc = new ASCIIEncoding();
      outputData = enc.GetBytes("NOT yet implemented.");
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
      GuiLogMessage("NOT yet complete implemented.", NotificationLevel.Warning);
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

    private void Crypt()
    {
      GuiLogMessage("NOT yet implemented.", NotificationLevel.Warning);


      NotifyUpdateOutput();
    }

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
		[PropertyInfo(Direction.Input, "Input Data Stream", "Input data stream to process", "", 
      false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
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
		[PropertyInfo(Direction.Input, "Input Data", "Input Data to process", "", 
      false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
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
    [PropertyInfo(Direction.Output, "Output Stream", "Output stream", "",
      true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null) ]
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
    [PropertyInfo(Direction.Output, "Output Data", "Output data", "",
      true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
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
  }
}
