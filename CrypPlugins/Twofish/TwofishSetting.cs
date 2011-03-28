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

using System;
using System.Collections.Generic;
using System.Text;

using Cryptool.PluginBase;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;

using System.Security.Cryptography;
using System.ComponentModel;
using System.Runtime.InteropServices;

using System.Windows.Controls;

namespace Twofish
{
  public class TwofishSettings : ISettings
  {

    private bool hasChanges = false;

    static int[] keyTab = { 128, 192, 256};

    #region ISettings Member

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
        return hasChanges;
      }
      set
      {
        hasChanges = value;
      }
    }

    #endregion

    int action = 1;
    [ContextMenu("ActionCaption", "ActionTooltip", 2, ContextMenuControlType.ComboBox, 
        new int[] { 1, 2 }, "Encrypt", "Decrypt")]
    [TaskPane("ActionCaption", "ActionTooltip", "", 0, false, ControlType.ComboBox,
        new string[] { "Encrypt", "Decrypt" })]
    public int Action
    {
      get
      {
        return action;
      }
      set
      {
        if (((int)value) != action)
          hasChanges = true;
        action = (int)value;
        OnPropertyChanged("Action");
      }
    }


    int mode = 0;
    [ContextMenu("ModeCaption", "ModeTooltip", 2, ContextMenuControlType.ComboBox, null,
        new string[] { "Electronic Code Book (ECB)", "Cipher Block Chaining (CBC)" })]
    [TaskPane("ModeCaption", "ModeTooltip", "", 5, false, ControlType.ComboBox,
        new string[] {"Electronic Code Book (ECB)", "Cipher Block Chaining (CBC)"} ) ]
    public int Mode
    {
      get
      {
        return mode;
      }

      set
      {
        if (((int)value) != mode)
          hasChanges = true;
        this.mode = (int)value;
        OnPropertyChanged("Mode");
      }
    }


    int keySize = 0;
    [ContextMenu("KeySizeCaption", "KeySizeTooltip", 3, 
        ContextMenuControlType.ComboBox, null, "128 Bits", "192 Bits", "256 Bits")]
    [TaskPane("KeySizeCaption", "KeySizeTooltip", "", 3, false, 
        ControlType.ComboBox, new String[] { "128 Bits", "192 Bits", "256 Bits" })]
    public int KeySize
    {
      get
      {
        return keySize;
      }
      set
      {
        if (((int)value) != keySize)
          hasChanges = true;
        keySize = (int)value;
        OnPropertyChanged("KeySize");
      }
    }


    /// <summary>
    /// Gets the length of the key.
    /// </summary>
    /// <value>The length of the key.</value>
    public int KeyLength
    {
      get
      {
        return keyTab[keySize];
      }
    }

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
        PropertyChanged(this, new PropertyChangedEventArgs(name));
      }
      hasChanges = true;
    }

    #endregion
  }
}
