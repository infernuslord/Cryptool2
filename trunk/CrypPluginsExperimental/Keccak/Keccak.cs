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

#define _DEBUG_

using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System;


namespace Cryptool.Plugins.Keccak
{
    [Author("Max Brandi", "max.brandi@rub.de", null, null)]
    [PluginInfo("Keccak.Properties.Resources", "PluginCaption", "PluginDescription", "Keccak/Documentation/doc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.HashFunctions)]
    public class Keccak : ICrypComponent
    {
        #region Private Variables

        private Encoding encoding = Encoding.UTF8;
        private readonly KeccakSettings settings = new KeccakSettings();
        private bool execute = true;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputDataStreamTooltip", true)]
        public ICryptoolStream InputStream
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputDataStreamTooltip", true)]
        public ICryptoolStream OutputStream
        {
            get;
            set;
        }

        #if _DEBUG_
        [PropertyInfo(Direction.OutputData, "DebugStreamCaption", "DebugDataStreamTooltip", true)]
        public ICryptoolStream DebugStream
        {
            get;
            set;
        }
        #endif


        #endregion

        public void Execute()
        {
            if (!execute)
            {
                return;
            }

            ProgressChanged(0, 1);

            byte[] input, output;
            int outputLength, rate, capacity;

            /* setup output stream writer */
            CStreamWriter OutputStreamwriter = new CStreamWriter();
            OutputStream = OutputStreamwriter;
            OnPropertyChanged("OutputStream");

            #if _DEBUG_
            /* setup debug stream writer */
            TextWriter consoleOut = Console.Out;    // save the standard output
            CStreamWriter debugStream = new CStreamWriter();
            StreamWriter debugStreamWriter = new StreamWriter(debugStream);
            debugStreamWriter.AutoFlush = true;     // flush stream every time WriteLine is called
            Console.SetOut(debugStreamWriter);
            DebugStream = debugStream;
            OnPropertyChanged("DebugStream");
            #endif

            #region get input

            /* read input */
            using (CStreamReader reader = InputStream.CreateReader())
            {
                int bytesRead;
                byte[] buffer = new byte[128];      // buffer of length 128 byte

                MemoryStream stream = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(stream);
                
                while ((bytesRead = reader.Read(buffer)) > 0)
                {
                    bw.Write(buffer, 0, bytesRead);
                }

                bw.Close();
                input = stream.ToArray();
                OnPropertyChanged("OutputStream");
            }

            #endregion

            outputLength = settings.OutputLength;
            rate = settings.Rate;
            capacity = settings.Capacity;

            /* hash input */
            output = KeccakHashFunction.Hash(input, outputLength, rate, capacity);

            /* write output */
            OutputStreamwriter.Write(output);
            OutputStreamwriter.Close();

            #if _DEBUG_
            /* close debug stream and reset standard output */
            debugStreamWriter.Close();
            Console.SetOut(consoleOut);
            #endif

            ProgressChanged(1, 1);
        }

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
        public UserControl Presentation
        {
            get { return null; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            bool stateSizeOk = (settings.GetStateSize() == settings.Rate + settings.Capacity);
            bool outputLengthOk = settings.OutputLength % 8 == 0;
            bool outputLengthTruncated = settings.OutputLengthTruncated();

            if (stateSizeOk && outputLengthOk)
            {
                if (outputLengthTruncated)
                {
                    GuiLogMessage("Output length too long, will be truncated to 174760 bits", NotificationLevel.Warning);
                }

                return;
            }
            else
            {
                if (!stateSizeOk)
                {
                    GuiLogMessage("The sum of the specified bit rate and capacity must match the selected state size!", NotificationLevel.Error);
                }
                if (!outputLengthOk)
                {
                    GuiLogMessage("The output length must be divisible by 8!", NotificationLevel.Error);
                }
                execute = false;
            }
        }

        // public void Execute()
        // {
        // }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            execute = true;
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
            settings.UpdateTaskPaneVisibility();

            /* select Keccak-256 as default */
            //settings.KECCAKFunction = 2;
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
    }
}
