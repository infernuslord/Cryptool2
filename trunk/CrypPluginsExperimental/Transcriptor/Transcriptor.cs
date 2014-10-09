/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Transcriptor;
using System.Windows.Threading;
using System.Threading;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.IO;
using System.IO;
using System.Windows.Media.Imaging;

namespace Cryptool.Plugins.Transcriptor
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Olga Groh", "o_groh@student.uni-kassel.de", "Uni Kassel", "www.uni-kassel.de")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("Transcriptor", "Transcriptor", "Transcriptor/userdoc.xml", new[] {"Transcriptor/icon.png"})]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    [ComponentVisualAppearance(ComponentVisualAppearance.VisualAppearanceEnum.Opened)]
    public class Transcriptor : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly TranscriptorSettings settings;
        private TranscriptorPresentation transcriptorPresentation;

        #endregion

        public Transcriptor()
        {
            transcriptorPresentation = new TranscriptorPresentation(this);
            settings = new TranscriptorSettings();

            Presentation = transcriptorPresentation;
        }

        #region Data Properties

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "Image File", "Image ToolTip")]
        public ICryptoolStream Image
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "Working Alphabet", "Alphabet ToolTip")]
        public string Alphabet
        {
            get;
            set;
        }

        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "TextOutput name", "TextOutput Tooltip")]
        public string Text
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation { get; private set; }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            switch (settings.Color)
            {
                case 0: transcriptorPresentation.RectangleColor = "Black"; break;
                case 1: transcriptorPresentation.RectangleColor = "White"; break;
                case 2: transcriptorPresentation.RectangleColor = "Red"; break;
            }

            transcriptorPresentation.StrokeThicknes = settings.Stroke;

            if (settings.Mode == 0)
            {
                transcriptorPresentation.MatchTemplateOn = false;
            }
            else
            {
                transcriptorPresentation.MatchTemplateOn = true;
                transcriptorPresentation.Threshold = settings.Threshold / 100f;
                transcriptorPresentation.ComparisonMethod = settings.Method;
            }
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            
            transcriptorPresentation.Dispatcher.Invoke(DispatcherPriority.Background, (SendOrPostCallback)delegate
            {
                var decoder = BitmapDecoder.Create(new MemoryStream(Image.CreateReader().ReadFully()),
                BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);

                if (decoder.Frames.Count > 0)
                {
                    transcriptorPresentation.picture.Source = decoder.Frames[0];
                }

            }, null);
            
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
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

        internal void GenerateText(string outputText)
        {
            Text = outputText;
            OnPropertyChanged("Text");
            //ProgressChanged(1, 1);
        }
    }
}
