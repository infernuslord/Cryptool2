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

namespace Tiger
{
	class TigerSettings : ISettings
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
