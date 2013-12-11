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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase.Miscellaneous;

namespace SigabaPhaseII
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Anonymous", "coredevs@cryptool.org", "CrypTool 2 Team", "http://cryptool2.vs.uni-due.de")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("SigabaPhaseII", "Subtract one number from another", "SigabaPhaseII/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class SigabaPhaseII : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly SigabaPhaseIISettings settings = new SigabaPhaseIISettings();
        private readonly SigabaPhaseIICore _core ;
        private IControlCost costMaster;
        private SigabaPhaseIIPresentation presentation; 
        private SigabaPhaseIIDecipher _decipher;

        #endregion

        #region Constructor
        public SigabaPhaseII()
        {
            _core = new SigabaPhaseIICore(this);
            presentation = new SigabaPhaseIIPresentation();
            Presentation = presentation;
        }


        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "Cipher text", "Input tooltip description")]
        public string CiphertText
        {
            get;
            set;
        }

       /* [PropertyInfo(Direction.InputData, "Plain text", "Input tooltip description")]
        public string PlainText
        {
            get;
            set;
        }*/

        [PropertyInfo(Direction.InputData, "Survivor", "Input tooltip description")]
        public object[] Survivor
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "Plain text", "Output tooltip description")]
        public string BestPlainText
        {
            get;
            set;
        }

        [PropertyInfo(Direction.ControlMaster, "CostMasterCaption", "CostMasterTooltip", false)]
        public IControlCost CostMaster
        {
            get { return costMaster; }
            set { costMaster = value; }
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
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            
            ProgressChanged(0, 1);
            if(Survivor!=null)
                if (Survivor[0] != null && Survivor[1] != null && Survivor[2] != null)
            {
                int[][] repeatList = Survivor[2] as int[][];
                int[] testarr = Survivor[1] as int[];
                string[] keys = Survivor[0] as string[];

                Console.Write("Hello:");
                foreach (int[] intse in repeatList)
                {
                    foreach (int i in intse)
                    {
                        Console.Write(i + ",");
                    }
                    Console.Write("}, new int[] {");
                }

                foreach (int i in testarr)
                {
                    Console.WriteLine(i);
                }


                _core.setCodeWheels(testarr, keys);

                _core.stepOneCompact(repeatList);

                /*
                List<Candidate> winnerList = _core.stepOne(repeatList);

                List<Candidate> winnerList2 = _core.WinnerConfirm(repeatList, winnerList);

                List<Candidate> winnerList3 = new List<Candidate>();
                if (winnerList2.Count != 0)
                {
                    winnerList3 = winnerList2;
                }
                else
                {
                    winnerList3 = winnerList;
                }

                Console.WriteLine(winnerList3.Count);

                 _core.SteppingMazeCompletion(repeatList, winnerList3);
                    */

                OnPropertyChanged("SomeOutput");


                ProgressChanged(1, 1);
            }
            else
            {
                GuiLogMessage("Check connections!",NotificationLevel.Error);
            }
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

        #region CoreCommunication

        public void AddPresentationEntry(Candidate entry )
        {
            byte[] testarr = Survivor[1] as byte[];
            
            for(int i = 0;i<5;i++)
            {
                _decipher.setCipherRotors(i, testarr[i]);
            }

            for(byte i = 0;i<3;i++)
            {
                _decipher.setControlRotors(i, (byte)entry.RotorType[i] );
            }

            for(byte i = 2;i<10;i++)
            {
                if(i<5)
                    _decipher.setBool(i,i,entry.Reverse[i]);
                else
                    _decipher.setBool(i,i,true);
            }

            /*_decipher.setPseudoRotor((byte)entry.Pseudo);

            _decipher.Encrypt();

            byte[] plain = new byte[];

            double val = costMaster.CalculateCost(plain);*/
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
    public struct ValueKey
    {
        public String cipherKey;
        public String cipherRotors;
        public String controlKey;
        public String controlRotors;
        public byte[] decryption;
        public String indexKey;
        public String indexRotors;
        public byte[] keyArray;
        public double value;
    };

    public class ResultEntry
    {
        public string Ranking { get; set; }
        public string Value { get; set; }
        public string CipherKey { get; set; }
        public string ControlKey { get; set; }
        public string IndexKey { get; set; }
        public string CipherRotors { get; set; }
        public string ControlRotors { get; set; }
        public string IndexRotors { get; set; }
        public string Text { get; set; }
    }
}
