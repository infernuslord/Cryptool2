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
using System.Linq;
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
        GuiLogMessage("Settings changed.", NotificationLevel.Debug);
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

    /// <summary>
    /// Gets the quick watch presentation 
    /// will be displayed inside of the plugin presentation-element. 
    /// You can return the existing Presentation if it makes sense to display 
    /// it inside a small area. But be aware that 
    /// if Presentation is displayed in QuickWatchPresentation 
    /// you can't open Presentation it in a tab before you
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

    public void PreExecution()
    {
    }

    public void Execute()
    {
      throw new NotImplementedException();
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

    public void Dispose()
    {
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
          //Hash();
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
