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
using Cryptool.PluginBase.Attributes;
using System.Windows.Threading;
using System.Threading;
using Keccak.Properties;
using System.Windows;


namespace Cryptool.Plugins.Keccak
{
    [Author("Max Brandi", "max.brandi@rub.de", null, null)]
    [PluginInfo("Keccak.Properties.Resources", "PluginCaption", "PluginDescription", "Keccak/Documentation/doc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.HashFunctions)]
    [ComponentVisualAppearance(ComponentVisualAppearance.VisualAppearanceEnum.Opened)]
    public class Keccak : ICrypComponent
    {
        #region Private Variables

        private KeccakPres pres = new KeccakPres();
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
            /* do not execute if checks in PreExecution() failed */
            if (!execute)
                return;

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

            /* show presentation intro */
            #region presentation intro
                       

            if (pres.IsVisible)
            {
                /* clean up last round */
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.buttonNext.Content = "Next";
                    pres.absorbGrid.Visibility = Visibility.Hidden;
                    pres.imgBlankPage.Visibility = Visibility.Hidden;
                    pres.labelOutput.Visibility = Visibility.Hidden;
                    pres.textBlockStateBeforeAbsorb.Text = "";
                    pres.textBlockBlockToAbsorb.Text = "";
                    pres.textBlockStateAfterAbsorb.Text = "";
                    pres.labelCurrentPhase.Content = "";
                    pres.labelCurrentStep.Content = "";
                }, null);

                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.imgIntroFirstPage.Visibility = Visibility.Visible;
                }, null);

                AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                buttonNextClickedEvent.WaitOne();

                if (!pres.stopButtonClicked)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.imgIntroFirstPage.Visibility = Visibility.Hidden;
                        pres.imgIntroIntroduction.Visibility = Visibility.Visible;
                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                if (!pres.stopButtonClicked)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelCurrentPhase.Content = "Introduction";
                        pres.labelCurrentStep.Content = "Initialization";

                        pres.textBlockIntroduction.Text =
                            string.Format("The state of the sponge construction is initialized. Every b bits of the state are initialized to 0. " +
                            "The state is partitioned into two parts: Capacity - c bits and Bit Rate - r bits. " +
                            "With the current settings the values are: b = {0}, c = {1}, r = {2}.", (rate + capacity), capacity, rate);

                        pres.imgIntroIntroduction.Visibility = Visibility.Hidden;
                        pres.imgIntroSpongeInit.Visibility = Visibility.Visible;
                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                if (!pres.stopButtonClicked)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelCurrentStep.Content = "Absorbing Phase";

                        pres.textBlockIntroduction.Text =
                            string.Format("Every block p of the padded input is absorbed (exclusive-or'ed) by the sponge state followed by " +
                            "an execution of the Keccak-f permutation. The input blocks only update the {0} bits of the r-bit part of the state.", rate);

                        pres.imgIntroSpongeInit.Visibility = Visibility.Hidden;
                        pres.imgIntroSpongeAbsorb.Visibility = Visibility.Visible;
                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                if (!pres.stopButtonClicked)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelCurrentStep.Content = "Squeezing Phase";
                        pres.textBlockIntroduction.Text = "The hash value is extracted from the r-bit part (z) of the state. If the size of the requested output " +
                            "is larger than r, the state is permuted by Keccak-f iteratively until enough output is extracted.";

                        pres.imgIntroSpongeAbsorb.Visibility = Visibility.Hidden;
                        pres.imgIntroSpongeSqueeze.Visibility = Visibility.Visible;
                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                if (!pres.stopButtonClicked)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.labelCurrentStep.Content = "The Keccak-f Permutation";
                        pres.textBlockIntroduction.Text = "The Keccak-f permutation performs 12 + 2 * l rounds. Each round consists of five step mappings " +
                            "which permute the state in different ways. For the selected state size l equals 6 which makes a total of 24 rounds.";

                        pres.imgIntroSpongeSqueeze.Visibility = Visibility.Hidden;
                        pres.imgIntroSpongeKeccakf2.Visibility = Visibility.Visible;
                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                if (!pres.stopButtonClicked)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.imgIntroSpongeKeccakf2.Visibility = Visibility.Hidden;
                        pres.imgIntroStateMapping.Visibility = Visibility.Visible;
                        pres.textBlockIntroduction.Text = string.Format("For a better understanding of the step mappings, the {0} state bits are presented " +
                            "as a three-dimensional cube. The row and column size is fixed to 5 bits. The lane size " +
                            "depends on the state size and is {1} bits.", (capacity + rate), (capacity + rate) / 25);
                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                if (!pres.stopButtonClicked)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.imgIntroStateMapping.Visibility = Visibility.Hidden;
                        pres.imgIntroExecution.Visibility = Visibility.Visible;
                        pres.textBlockIntroduction.Text = "";
                        pres.labelCurrentPhase.Content = "";
                        pres.labelCurrentStep.Content = "";
                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.imgIntroExecution.Visibility = Visibility.Hidden;
                }, null);

                if (pres.stopButtonClicked)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.imgIntroFirstPage.Visibility = Visibility.Hidden;
                        pres.imgIntroIntroduction.Visibility = Visibility.Hidden;
                        pres.imgIntroSpongeInit.Visibility = Visibility.Hidden;
                        pres.imgIntroSpongeAbsorb.Visibility = Visibility.Hidden;
                        pres.imgIntroSpongeSqueeze.Visibility = Visibility.Hidden;
                        pres.imgIntroSpongeKeccakf2.Visibility = Visibility.Hidden;
                        pres.imgIntroStateMapping.Visibility = Visibility.Hidden;
                        pres.imgIntroExecution.Visibility = Visibility.Hidden;
                        pres.textBlockIntroduction.Text = "";
                    }, null);
                }
            }

            
            #endregion

            /* hash input */
            output = KeccakHashFunction.Hash(input, outputLength, rate, capacity, ref pres);

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
            get { return pres; }
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
                    GuiLogMessage(Resources.OutputTooLongWarning, NotificationLevel.Warning);
                }

                return;
            }
            else
            {
                if (!stateSizeOk)
                {
                    GuiLogMessage(Resources.StateSizeMatchError, NotificationLevel.Error);
                }
                if (!outputLengthOk)
                {
                    GuiLogMessage(Resources.OutputMatchError, NotificationLevel.Error);
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
            pres.autostep = false;
            pres.skipStep = false;
            pres.stopButtonClicked = false;
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            pres.buttonNextClickedEvent.Set();
            pres.stopButtonClicked = true;
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            settings.UpdateTaskPaneVisibility();
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