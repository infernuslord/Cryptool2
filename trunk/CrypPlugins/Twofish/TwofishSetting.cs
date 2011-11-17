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
using Cryptool.PluginBase.IO;

using System.Security.Cryptography;
using System.ComponentModel;
using System.Runtime.InteropServices;

using System.Windows.Controls;

namespace Twofish
{
  public class TwofishSettings : ISettings
  {

    static int[] keyTab = { 128, 192, 256 };

    #region ISettings Member

      #endregion

    int action = 1;
    [ContextMenu("ActionCaption", "ActionTooltip", 2, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "ActionList1", "ActionList2")]
    [TaskPane("ActionCaption", "ActionTooltip", "", 0, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
    public int Action
    {
      get
      {
        return action;
      }
      set
      {
        if (((int)value) != action)
        {
            action = (int)value;
            OnPropertyChanged("Action");            
        }
      }
    }


    int mode = 0;
    [ContextMenu("ModeCaption", "ModeTooltip", 2, ContextMenuControlType.ComboBox, null, new string[] { "ModeList1", "ModeList2" })]
    [TaskPane("ModeCaption", "ModeTooltip", "", 5, false, ControlType.ComboBox, new string[] { "ModeList1", "ModeList2" })]
    public int Mode
    {
      get
      {
        return mode;
      }

      set
      {
        if (((int)value) != mode)
        {
            this.mode = (int)value;
            OnPropertyChanged("Mode");   
        }
      }
    }


    int keySize = 0;
    [ContextMenu("KeySizeCaption", "KeySizeTooltip", 3, ContextMenuControlType.ComboBox, null, "KeySizeList1", "KeySizeList2", "KeySizeList3")]
    [TaskPane("KeySizeCaption", "KeySizeTooltip", "", 3, false, ControlType.ComboBox, new String[] { "KeySizeList1", "KeySizeList2", "KeySizeList3" })]
    public int KeySize
    {
      get
      {
        return keySize;
      }
      set
      {
        if (((int)value) != keySize)
        {
            keySize = (int)value;
            OnPropertyChanged("KeySize");   
        }
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
    }

    #endregion
  }
}
