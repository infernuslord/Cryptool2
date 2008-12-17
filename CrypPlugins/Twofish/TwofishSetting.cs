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
  class TwofishSettings : ISettings
  {

    private bool hasChanges = false;

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
    [ContextMenu("Action", "Do you want the input data to be encrypted or decrypted?", 2,
      DisplayLevel.Beginner, ContextMenuControlType.ComboBox,
      new int[] { 1, 2 }, "Encrypt", "Decrypt")]
    [TaskPane("Action",
      "Do you want the input data to be encrypted or decrypted?", "", 0, false,
       DisplayLevel.Beginner, ControlType.ComboBox,
       new string[] { "Encrypt", "Decrypt" })]
    public int Action
    {
      get
      {
        return this.action;
      }
      set
      {
        if (((int)value) != action)
          hasChanges = true;
        this.action = (int)value;
        OnPropertyChanged("Action");
      }
    }


    int mode = 0;
    [ContextMenu("Chaining mode",
          "Select the block cipher mode of operation.", 2, 
          DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null,
          new string[] { "Electronic Code Book (ECB)", "Cipher Block Chaining (CBC)" })]
    [TaskPane("Chaining mode", "Select the block cipher mode of operation.", "", 5, false,
          DisplayLevel.Experienced, ControlType.ComboBox,
          new string[] {"Electronic Code Book (ECB)", "Cipher Block Chaining (CBC)"} ) ]
    public int Mode
    {
      get
      {
        return this.mode;
      }

      set
      {
        if (((int)value) != mode)
          hasChanges = true;
        this.mode = (int)value;
        OnPropertyChanged("Mode");
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
