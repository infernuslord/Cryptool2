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

namespace PKCS5
{
  /// <summary>
  /// Settings for PKCS#5 v2
  /// </summary>
  public class PKCS5Settings : ISettings
  {
    private bool hasChanges = false;

    #region ISettings Member

    public enum EncodingTypes
    {
      Default = 0,
      Unicode = 1,
      UTF7 = 2,
      UTF8 = 3,
      UTF32 = 4,
      ASCII = 5,
      BigEndianUnicode = 6
    };
    private EncodingTypes encoding = EncodingTypes.Default;
    /// <summary>
    /// Retrieves the current used encoding, or sets it.
    /// </summary>
    public EncodingTypes Encoding
    {
      get
      {
        return this.encoding;
      }
      set
      {
        if (this.Encoding != value)
          hasChanges = true;
        this.encoding = value;
        OnPropertyChanged("EncodingSetting");
      }
    }

    /// <summary>
    /// selected internal hash HMAC function
    /// </summary>
    private PKCS5MaskGenerationMethod.ShaFunction selectedShaFunction 
					= PKCS5MaskGenerationMethod.ShaFunction.SHA256;

    [ContextMenu( "SHAFunctionCaption", "SHAFunctionTooltip", 0,
      ContextMenuControlType.ComboBox, null,
      new string[] { "SHAFunctionList1", "SHAFunctionList2", "SHAFunctionList3", "SHAFunctionList4", "SHAFunctionList5", "SHAFunctionList6", "SHAFunctionList7" })]
    [TaskPane( "SHAFunctionTPCaption", "SHAFunctionTPTooltip", "", 0, true,
      ControlType.ComboBox,
      new string[] { "SHAFunctionList1", "SHAFunctionList2", "SHAFunctionList3", "SHAFunctionList4", "SHAFunctionList5", "SHAFunctionList6", "SHAFunctionList7" })]
    public int SHAFunction
    {
      get
      {
        return (int)this.selectedShaFunction;
      }
      set
      {
        this.selectedShaFunction = (PKCS5MaskGenerationMethod.ShaFunction)value;
        // set to max hash length
        length = PKCS5MaskGenerationMethod.GetHashLength(selectedShaFunction) * 8;
        hasChanges = true;
        OnPropertyChanged("SHAFunction");
        OnPropertyChanged("Length");
      }
    }

    /// <summary>
    /// count of hash loops
    /// </summary>
    private int count = 1000;
    [TaskPane( "CountCaption", "CountTooltip", "", 1, false,
      ControlType.TextBox, ValidationType.RangeInteger, 1, 9999)]
    public int Count
    {
      get
      {
        return count;
      }
      set
      {
        count = value;
        if (count == 0)
          count = 1000;
        hasChanges = true;
        OnPropertyChanged("Count");
      }
    }

    /// <summary>
    /// length of calculated hash in bits
    /// </summary>
    private int length = 256;
    [TaskPane( "LengthCaption", "LengthTooltip", "", 2, false,
      ControlType.TextBox, ValidationType.RangeInteger, -64, 2048)]
    public int Length
    {
      get
      {
        return length;
      }
      set
      {
        length = value;
        if (length < 0) // change from bytes to bits [hack]
          length *= -8;

        while ((length & 0x07) != 0) // go to the next multiple of 8
          length++;

        hasChanges = true;
        OnPropertyChanged("Length");
      }
    }



    /// <summary>
    /// Encoding property used in the Settings pane. 
    /// </summary>
    [ContextMenu( "EncodingSettingCaption", "EncodingSettingTooltip", 1, ContextMenuControlType.ComboBox, null, 
      new string[] { "EncodingSettingList1", "EncodingSettingList2", "EncodingSettingList3", "EncodingSettingList4", "EncodingSettingList5", "EncodingSettingList6", "EncodingSettingList7" })]
    [TaskPane( "EncodingSettingCaption", "EncodingSettingTooltip", 
      null, 1, false, ControlType.RadioButton, 
      new string[] { "EncodingSettingList1", "EncodingSettingList2", "EncodingSettingList3", "EncodingSettingList4", "EncodingSettingList5", "EncodingSettingList6", "EncodingSettingList7" })]
    public int EncodingSetting
    {
      get
      {
        return (int)this.encoding;
      }
      set
      {
        if (this.encoding != (EncodingTypes)value)
          HasChanges = true;
        this.encoding = (EncodingTypes)value;
        OnPropertyChanged("EncodingSetting");
      }
    }


    /// <summary>
    /// Checks the length.
    /// </summary>
    private void CheckLength()
    {
      // get max length of this hash
      int newlen = PKCS5MaskGenerationMethod.GetHashLength(selectedShaFunction) * 8;
      if (newlen < length)
      {
        length = newlen; // reduce it to max length
        hasChanges = true;
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
        return hasChanges;
      }
      set
      {
        hasChanges = value;
      }
    }

    #endregion

    #region INotifyPropertyChanged Member

    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

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
