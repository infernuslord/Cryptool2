using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Analysis;
using Cryptool.PluginBase;
using System.Windows.Controls;
using System.ComponentModel;
using Cryptool.PluginBase.Control;

namespace KeySearcher
{
  [Author("Thomas Schmid", "thomas.schmid@cryptool.org", "Uni Siegen", "http://www.uni-siegen.de")]
  [PluginInfo(false, "KeySearcher", "Demo plugin shows way to use ControlMaster", null, "KeySearcher/Images/icon.png")]
  public class KeySearcher : IAnalysisMisc
  {

    #region IPlugin Members

    public event StatusChangedEventHandler OnPluginStatusChanged;

    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

    public event PluginProgressChangedEventHandler OnPluginProgressChanged;

    private KeySearcherSettings settings = new KeySearcherSettings();
    public ISettings Settings
    {
      get { return settings; }
    }

    public UserControl Presentation
    {
      get { return null; }
    }

    public UserControl QuickWatchPresentation
    {
      get { return null; }
    }

    public void PreExecution()
    {
    }

    public void Execute()
    {
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

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged(string property)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(property));
    }

    #endregion

    #region IControlEncryption Members

    private IControlEncryption testProperty;
    [PropertyInfo(Direction.ControlMaster, "Test IControlEncryption Master", "Tooltip1", "", DisplayLevel.Beginner)]
    public IControlEncryption TestProperty
    {
      get { return testProperty; }
      set
      {
        if (value != null)
        {
          testProperty = value;
          int keySize = 8;
          int ivSize = 8;
          int end = 255;
          string secret = "Hallo Welt!";

          byte[] testKey = new byte[keySize];
          byte[] searchKey = new byte[keySize];
          byte[] iv = new byte[ivSize];
          Random ran = new Random();
          for (int i = 0; i < keySize; i++)
          {
            testKey[i] = (byte)ran.Next(end);
            searchKey[i] = testKey[i];
            iv[i] = (byte)ran.Next(end);
          }

          byte[] testData = Encoding.Default.GetBytes(secret);
          byte[] dataEncrypted = testProperty.Encrypt(testKey, testData, iv);


          for (int i = 0; i < end; i++)
          {
            searchKey[0] = (byte)i;
            byte[] dataDecrypted = testProperty.Decrypt(searchKey, dataEncrypted, iv);
            string test = Encoding.Default.GetString(dataDecrypted);
            if (test.Contains(secret))
            {
              GuiLogMessage("Found key: " + i.ToString(), NotificationLevel.Info);
              break;
            }
          }
          // key found
        }
      }
    }

    private void GuiLogMessage(string message, NotificationLevel loglevel)
    {
      if (OnGuiLogNotificationOccured != null)
        OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, loglevel));
    }

    #endregion
  }
}
