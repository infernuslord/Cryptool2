/* HOWTO: Set year, author name and organization.
   Copyright 2011 CrypTool 2 Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.IO;

namespace Cryptool.Plugins.StegoPermutation
{
    [Author("Corinna John", "coco@steganografie.eu", "", "http://www.steganografie.eu")]
    [PluginInfo(false, "Permutation Steganography", "Encodes a message in the permutation of a list", null, "CrypWin/images/default.png")]
    [ComponentCategory(ComponentCategory.Steganography)]
    public class StegoPermutation : ICrypComponent
    {
        #region Private Variables

        private readonly StegoPermutationSettings settings = new StegoPermutationSettings();

        #endregion

        #region Data Properties

        /// <summary>
        /// Message to be encoded.
        /// </summary>
        [PropertyInfo(Direction.InputData, "Message", "Input message", null)]
        public string InputMessage
        {
            get;
            set;
        }

        /// <summary>
        /// List of words to be sorted.
        /// </summary>
        [PropertyInfo(Direction.InputData, "Carrier list", "Input list", null)]
        public string InputList
        {
            get;
            set;
        }

        /// <summary>
        /// Sorted output list.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "Carrier List", "Result", null)]
        public string OutputList
        {
            get;
            set;
        }

        /// <summary>
        /// Decoded message.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "Message", "Result message", null)]
        public string OutputMessage
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// HOWTO: You can provide a custom (tabbed) presentation to visualize your algorithm.
        /// Return null if you don't provide one.
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

        /// <summary>
        /// HOWTO: You can provide custom (quickwatch) presentation to visualize your algorithm.
        /// Return null if you don't provide one.
        /// </summary>
        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        /// <summary>
        /// HOWTO: Enter the algorithm you'd like to implement in this method.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            Collection<string> inputCarrierList = new Collection<string>(InputList.Split(','));
            Sorter<string> sorter = new Sorter<string>(inputCarrierList);

            if (settings.Action == 0)
            {
                if (sorter.Capacity < Encoding.Default.GetByteCount(InputMessage))
                {
                    GuiLogMessage("List too short for message. Only the beginning will be encoded, the tail will get lost.", NotificationLevel.Debug);
                    InputMessage = InputMessage.Substring(0, sorter.Capacity);
                }

                using (MemoryStream messageStream = new MemoryStream(Encoding.Default.GetBytes(InputMessage)))
                {
                    Collection<string> result = sorter.Encode(messageStream, settings.Alphabet);
                    OutputList = string.Join<string>(",", result);
                    OnPropertyChanged("OutputList");
                }
            }
            else
            {
                using (MemoryStream messageStream = new MemoryStream())
                {
                    sorter.Decode(messageStream, settings.Alphabet);
                    messageStream.Position = 0;
                    using (StreamReader reader = new StreamReader(messageStream))
                    {
                        OutputMessage = reader.ReadToEnd();
                        OnPropertyChanged("OutputMessage");
                    }
                }
            }
            ProgressChanged(1, 1);
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

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
