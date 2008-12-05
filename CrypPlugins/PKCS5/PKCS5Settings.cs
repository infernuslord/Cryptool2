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
    /// Settings for PKCS#5 v2
    /// </summary>
    public class PKCS5Settings : ISettings
    {
        private bool hasChanges = false;

        #region ISettings Member


			/// <summary>
			/// selected internal hash HMAC function
			/// </summary>
			  private PKCS5MaskGenerationMethod.ShaFunction selectedShaFunction 
					= PKCS5MaskGenerationMethod.ShaFunction.SHA256;

        [ContextMenu("SHA Function",
					"Select the hash function (MD5, SHA1 or one out of the SHA2 family)", 0, 
					DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, 
					new string[] {"MD5", "SHA1", "SHA256", "SHA384", "SHA512"})]
        [TaskPane("Select hash function", 
					"Select the hash function (MD2, SHA1 or one out of the SHA2 family)", "", 0, true, 
					DisplayLevel.Beginner, ControlType.ComboBox, 
					new string[] { "MD5", "SHA1", "SHA256", "SHA384", "SHA512" })]
        public int SHAFunction
        {
            get
            {
                return (int)this.selectedShaFunction;
            }
            set
            {
                this.selectedShaFunction = (PKCS5MaskGenerationMethod.ShaFunction)value;
								//CheckLength();
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
        [TaskPane("Number of iterations (counter)", 
					"The counter determines how often the hash function is applied." + 
					" A value bigger than 1000 is recommended.", "", 1, false, 	
					DisplayLevel.Beginner, ControlType.TextBox, ValidationType.RangeInteger, 1, 9999)]
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
        [TaskPane("Length of output key", 
					"The length of the output in bits must be a multiple of 8.", "", 2, false, 
					DisplayLevel.Beginner, ControlType.TextBox, ValidationType.RangeInteger, -64, 2048)]
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

						//CheckLength();

						hasChanges = true;
						OnPropertyChanged("Length");
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
