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
using System.Linq;
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
    /// 
    /// </summary>
    public class PKCS5Settings : ISettings
    {


        private bool hasChanges = false;


        private PKCS5MaskGenerationMethod.ShaFunction selectedShaFunction = PKCS5MaskGenerationMethod.ShaFunction.SHA256;
       // static string[] menu = Enum.GetNames(typeof(PKCS5MaskGenerationMethod.ShaFunction)).Clone();
        
        private int count = 1000;

        #region ISettings Member

        [ContextMenu("SHA Function", "Select the hash function (MD5, SHA1 or one out of the SHA2 family)", 0, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new string[] {"MD5", "SHA1", "SHA256", "SHA384", "SHA512"})]
        [TaskPane("SHA Function", "Select the hash function (MD2, SHA1 or one out of the SHA2 family)", "", 0, true, DisplayLevel.Beginner, ControlType.ComboBox, new string[] {"MD5", "SHA1", "SHA256", "SHA384", "SHA512"})]
        public int SHAFunction
        {
            get
            {
                return (int)this.selectedShaFunction;
            }
            set
            {
                this.selectedShaFunction = (PKCS5MaskGenerationMethod.ShaFunction)value;
                hasChanges = true;
                OnPropertyChanged("Settings");
            }
        }

        [TaskPane("Count", "Count - iteration count for Hash function, a value greather 1000 is recommended.", "", 1, false, DisplayLevel.Beginner, ControlType.TextBox, ValidationType.RangeInteger, 1, 9999)]
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
                OnPropertyChanged("Settings");
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
