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
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Cryptool.PluginBase;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;

namespace Whirlpool
{
	[Author("Gerhard Junker", null, "private project member", null)]
  [PluginInfo(false, "Whirlpool", "Whirlpool hash function", "", "Whirlpool/Whirlpool.png")]
	public class WPHash : IHash
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

        WhirlpoolSettings whirlpoolSetting = new WhirlpoolSettings();

		/// <summary>
		/// Initializes a new instance of the <see cref="WPHash"/> class.
		/// </summary>
		public WPHash()
		{
		}

		/// <summary>
		/// Gets or sets the settings.
		/// </summary>
		/// <value>The settings.</value>
		public ISettings Settings
		{
			get
            {
                return whirlpoolSetting;
            }
		}


		#region Input data

		// Input input
        private static byte[] empty = {};
		private byte[] input = empty;
		private dataCanal inputCanal = dataCanal.none;

		/// <summary>
		/// Notifies the update input.
		/// </summary>
		private void NotifyUpdateInput()
		{
			OnPropertyChanged("InputStream");
			OnPropertyChanged("InputData");
		}

		/// <summary>
		/// Gets or sets the input data.
		/// </summary>
		/// <value>The input input.</value>
		[PropertyInfo(Direction.Input, "Input Stream", "Input stream to be hashed", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
		public CryptoolStream InputStream
		{
			get
			{
				CryptoolStream inputDataStream = new CryptoolStream();
				inputDataStream.OpenRead(this.GetPluginInfoAttribute().Caption, input);
				return inputDataStream;
			}
			set
			{
				if (inputCanal != dataCanal.none && inputCanal != dataCanal.streamCanal)
					GuiLogMessage("Duplicate input key not allowed!", NotificationLevel.Error);
				inputCanal = dataCanal.streamCanal;

                if (null == value)
                    input = empty;
                else
                {
                    long len = value.Length;
                    input = new byte[len];

                    for (long i = 0; i < len; i++)
                        input[i] = (byte)value.ReadByte();
                }
				NotifyUpdateInput();
				GuiLogMessage("InputStream changed.", NotificationLevel.Debug);
			}
		}

		/// <summary>
		/// Gets the input data.
		/// </summary>
		/// <value>The input data.</value>
		[PropertyInfo(Direction.Input, "Input Data", "Input stream to be hashed", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
		public byte[] InputData
		{
			get
			{
				return input;
			}
			set
			{
				if (inputCanal != dataCanal.none && inputCanal != dataCanal.byteCanal)
					GuiLogMessage("Duplicate input data not allowed!", NotificationLevel.Error);
				inputCanal = dataCanal.byteCanal;

                if (null == value)
                    input = empty;
                else
                {
				    long len = value.Length;
				    input = new byte[len];

				    for (long i = 0; i < len; i++)
					    input[i] = value[i];
                }
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
		/// Gets or sets the output data stream.
		/// </summary>
		/// <value>The output data stream.</value>
		[PropertyInfo(Direction.Output, "Hashed Stream", "Output stream of the hashed value", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
		public CryptoolStream HashOutputStream
		{
			get
			{
				CryptoolStream outputDataStream = null;
				if (outputData != null)
				{
					outputDataStream = new CryptoolStream();
					listCryptoolStreamsOut.Add(outputDataStream);
					outputDataStream.OpenRead(this.GetPluginInfoAttribute().Caption, outputData);
				}
				GuiLogMessage("Got HashOutputStream.", NotificationLevel.Debug);
				return outputDataStream;
			}
			//set
			//{
			//} //readonly
		}

		/// <summary>
		/// Gets the output data.
		/// </summary>
		/// <value>The output data.</value>
		[PropertyInfo(Direction.Output, "Hashed Data", "Output data of the hashed value", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
		public byte[] HashOutputData
		{
			get
			{
				GuiLogMessage("Got HashOutputData.", NotificationLevel.Debug);
				return this.outputData;
			}
			//set
			//{
			//    if (outputData != value)
			//    {
			//        this.outputData = value;
			//    }
			//    NotifyUpdateOutput();
			//}
		}

		/// <summary>
		/// Hashes this instance.
		/// </summary>
		public void Hash()
		{
            WhirlpoolHash wh = new WhirlpoolHash();

            wh.Add(input);
            wh.Finish();

            outputData = wh.Hash;
            wh = null;


			NotifyUpdateOutput();
		}
		#endregion

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
		/// Gets the quick watch presentation - will be displayed inside of the plugin presentation-element. You
		/// can return the existing Presentation if it makes sense to display it inside a small area. But be aware that
		/// if Presentation is displayed in QuickWatchPresentation you can't open Presentation it in a tab before you
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

		/// <summary>
		/// Will be called from editor after restoring settings and before adding to workspace.
		/// </summary>
		public void Initialize()
		{
			GuiLogMessage("Initialize.", NotificationLevel.Debug);
		}

		/// <summary>
		/// Will be called from editor before right before chain-run starts
		/// </summary>
		public void PreExecution()
		{
			GuiLogMessage("PreExecution.", NotificationLevel.Debug);
		}

		/// <summary>
		/// Will be called from editor while chain-run is active and after last necessary input
		/// for plugin has been set.
		/// </summary>
		public void Execute()
		{
			GuiLogMessage("Execute.", NotificationLevel.Debug);
			Hash();
		}

		/// <summary>
		/// Will be called from editor after last plugin in chain has finished its work.
		/// </summary>
		public void PostExecution()
		{
			GuiLogMessage("PostExecution.", NotificationLevel.Debug);
		}

		/// <summary>
		/// Not defined yet.
		/// </summary>
		public void Pause()
		{
			GuiLogMessage("Pause.", NotificationLevel.Debug);
		}

		/// <summary>
		/// Will be called from editor while chain-run is active. Plugin hast to stop work immediately.
		/// </summary>
		public void Stop()
		{
			GuiLogMessage("Stop.", NotificationLevel.Debug);
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
			GuiLogMessage("Dispose.", NotificationLevel.Debug);
		}

		#endregion

		#region INotifyPropertyChanged Member

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Called when [property changed].
		/// </summary>
		/// <param name="name">The name.</param>
		protected void OnPropertyChanged(string name)
		{
			EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
		}

		/// <summary>
		/// GUIs the log message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="logLevel">The log level.</param>
		private void GuiLogMessage(string message, NotificationLevel logLevel)
		{
			EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
		}

		#endregion
	}
}
