/* 
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
using System.Windows.Threading;
using System.Threading;

namespace Cryptool.Plugins.StegoPermutation
{
    [Author("Corinna John, Armin Krauß", "coco@steganografie.eu", "", "http://www.steganografie.eu")]
    [PluginInfo("StegoPermutation.Properties.Resources", "PluginCaption", "PluginTooltip", "StegoPermutation/DetailedDescription/doc.xml", "StegoPermutation/Images/StegoPermutation.png")]
    [ComponentCategory(ComponentCategory.Steganography)]
    public class StegoPermutation : ICrypComponent
    {
        #region Private Variables

        private readonly StegoPermutationSettings settings = new StegoPermutationSettings();
        private Collection<string> inputList;
        private Sorter<string> sorter;
        private StegoPermutationPresentation presentation = new StegoPermutationPresentation();
        
        #endregion

        #region Data Properties

        /// <summary>
        /// Message to be encoded.
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputMessageCaption", "InputMessageTooltip")]
        public byte[] InputMessage
        {
            get;
            set;
        }

        /// <summary>
        /// List of words to be sorted.
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputListCaption", "InputListTooltip")]
        public string[] InputList
        {
            get
            {
                return inputList.ToArray();
            }
            set
            {
                string[] valueParts = value;
                this.inputList = new Collection<string>();
                HashSet<string> isPresent = new HashSet<string>();  // use HashSet for faster lookup
                HashSet<string> duplicates = new HashSet<string>();
                foreach (string s in valueParts)
                {
                    if( !isPresent.Contains(s) )
                    {
                        this.inputList.Add(s);
                        isPresent.Add(s);
                    }
                    else
                    {
                        // duplicate item found
                        duplicates.Add(s);
                    }
                }

                // print the found duplicates, but combine them in one message if there are too many

                if (duplicates.Count <= 10)
                {
                    foreach (var s in duplicates)
                        GuiLogMessage("Duplicate item removed from the list: " + s, NotificationLevel.Warning);
                }
                else
                {
                    Array dup = duplicates.ToArray();
                    Array.Sort(dup);
                    String s = String.Join(",", dup.Cast<String>());
                    if (s.Length > 500) s = s.Substring(0, 500) + "...";
                    GuiLogMessage("Duplicate items removed from the list: " + s, NotificationLevel.Warning);
                }

                this.sorter = new Sorter<string>(this.inputList);
                GuiLogMessage(String.Format("The list has {0} elements, so the maximum length of the message text is {1} bytes.", this.inputList.Count, this.sorter.Capacity), NotificationLevel.Info);
            }
        }

        /// <summary>
        /// Sorted output list.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputListCaption", "OutputListTooltip")]
        public string[] OutputList
        {
            get;
            set;
        }

        /// <summary>
        /// Decoded message.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputMessageCaption", "OutputMessageTooltip")]
        public byte[] OutputMessage
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

        public UserControl Presentation
        {
            get { return presentation; }
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
            SendOrPostCallback updatePresentationInputListDelegate = (SendOrPostCallback)delegate
            {
                presentation.UpdateInputList(this.inputList);
            };

            if (presentation.IsVisible)
            {
                presentation.Dispatcher.Invoke(DispatcherPriority.Normal, updatePresentationInputListDelegate, null);
            }

            ProgressChanged(0, 1);

            if (sorter == null || InputList == null || InputList.Length == 0)
            {
                GuiLogMessage("Input list required.", NotificationLevel.Error);
                return;
            }

            if (settings.Action == 0)
            {   // encode
                
                if (InputMessage == null)
                {
                    GuiLogMessage("Please provide the message to encode.", NotificationLevel.Error);
                    return;
                } 

                if (sorter.Capacity < InputMessage.Length)
                {
                    GuiLogMessage(String.Format("The list is too short for this message of {0} bytes. Only the first {1} bytes will be encoded, the tail will get lost.", InputMessage.Length, sorter.Capacity), NotificationLevel.Warning);
                    byte[] tmp = new byte[sorter.Capacity];
                    Array.Copy(InputMessage, tmp, tmp.Length);
                    InputMessage = tmp;
                }

                using (MemoryStream messageStream = new MemoryStream(InputMessage))
                {
                    Collection<string> result = sorter.Encode(messageStream, settings.Alphabet, presentation, this);
                    OutputList = result.ToArray();
                    OnPropertyChanged("OutputList");
                }
            }
            else
            {   // decode

                using (MemoryStream messageStream = new MemoryStream())
                {
                    sorter.Decode(messageStream, settings.Alphabet, presentation, this);
                    messageStream.Position = 0;
                    Array buf = messageStream.ToArray();
                    OutputMessage = (byte[])buf;
                    OnPropertyChanged("OutputMessage");
                }
            }

            ProgressChanged(1, 1);
        }

        public void PostExecution()
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

        public void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
