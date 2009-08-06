//////////////////////////////////////////////////////////////////////////////////////////////////
// CrypTool V2
// © 2008 - Gerhard Junker
// Apache License see http://www.apache.org/licenses/
//
// $HeadURL$
//////////////////////////////////////////////////////////////////////////////////////////////////
// $Revision::                                                                                $://
// $Author::                                                                                  $://
// $Date::                                                                                    $://
//////////////////////////////////////////////////////////////////////////////////////////////////


// read more about Tiger
//
// http://en.wikipedia.org/wiki/Tiger_(cryptography)
// http://de.wikipedia.org/wiki/Tiger_(Hashfunktion)
// http://www.cs.technion.ac.il/~biham/Reports/Tiger/
//
// based first on an VisualBasic implementation of Markus Hahn - Thanks.
// from http://www.hotpixel.net/software.html
// and changed to fit more the published algorithm

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


namespace Tiger
{

  [Author("Gerhard Junker", null, "private project member", null)]
  [PluginInfo(false, "Tiger", "Tiger2 V1.0.2 Hash",
    "Tiger/DetailedDescription/Description.xaml", "Tiger/Tiger1.png")]
  public class Tiger : IHash
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
    /// Initializes A new instance of the <see cref="Tiger"/> class.
    /// </summary>
    public Tiger()
    {
      this.settings = new TigerSettings();
    }

    #region Settings
    TigerSettings settings;

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
        settings = (TigerSettings)value;
        OnPropertyChanged("Settings");
        GuiLogMessage("Settings changed.", NotificationLevel.Debug);
      }
    }

    /// <summary>
    /// Gets or sets A value indicating whether this instance has changes.
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

    #region Input inputdata / password

    // Input inputdata
    private byte[] inputdata = { };
    //private dataCanal inputCanal = dataCanal.none;

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
    [PropertyInfo(Direction.InputData, "Input Data Stream", "Input data stream to be hashed", "",
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
        //if (inputCanal != dataCanal.none && inputCanal != dataCanal.streamCanal)
        //  GuiLogMessage("Duplicate input data not allowed!", NotificationLevel.Error);
        //inputCanal = dataCanal.streamCanal;

        long len = value.Length;
        inputdata = new byte[len];

        for (long i = 0; i < len; i++)
          inputdata[i] = (byte)value.ReadByte();

        NotifyUpdateInput();
        GuiLogMessage("InputStream changed.", NotificationLevel.Debug);
      }
    }

    /// <summary>
    /// Gets the input data.
    /// </summary>
    /// <value>The input data.</value>
    [PropertyInfo(Direction.InputData, "Input Data", "Input Data to be hashed", "",
      false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
    public byte[] InputData
    {
      get
      {
        return inputdata;
      }
      set
      {
        //if (inputCanal != dataCanal.none && inputCanal != dataCanal.byteCanal)
        //  GuiLogMessage("Duplicate Data data not allowed!", NotificationLevel.Error);
        //inputCanal = dataCanal.byteCanal;
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
        GuiLogMessage("InputData changed.", NotificationLevel.Debug);
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
      OnPropertyChanged("HashOutputStream");
      OnPropertyChanged("HashOutputData");
    }


    /// <summary>
    /// Gets or sets the output inputdata stream.
    /// </summary>
    /// <value>The output inputdata stream.</value>
    [PropertyInfo(Direction.OutputData, "Hashed Stream", "Output stream of the hashed value", "",
      true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
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
    /// Gets the output inputdata.
    /// </summary>
    /// <value>The output inputdata.</value>
    [PropertyInfo(Direction.OutputData, "Hashed Data", "Output data of the hashed value", "",
      true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
    public byte[] HashOutputData
    {
      get
      {
        GuiLogMessage("Got HashOutputData.", NotificationLevel.Debug);
        return this.outputData;
      }
    }

    #endregion

    void Hash()
    {
      HMACTIGER2 tg = new HMACTIGER2();
      outputData = tg.ComputeHash(inputdata);
      NotifyUpdateOutput();
    }

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
    /// Gets the quick watch presentation 
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
    /// Will be called from editor before right before chain-run starts
    /// </summary>
    public void PreExecution()
    {
    }

    /// <summary>
    /// Will be called from editor while chain-run is active and after last necessary input
    /// for plugin has been set.
    /// </summary>
    public void Execute()
    {
      Hash();
    }

    /// <summary>
    /// Will be called from editor after last plugin in chain has finished its work.
    /// </summary>
    public void PostExecution()
    {
    }


    public void Pause()
    {
    }

    /// <summary>
    /// Will be called from editor while chain-run is active. Plugin hast to stop work immediately.
    /// </summary>
    public void Stop()
    {
    }

    /// <summary>
    /// Will be called from editor after restoring settings and before adding to workspace.
    /// </summary>
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
          Hash();
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
