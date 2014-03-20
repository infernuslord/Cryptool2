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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase.Miscellaneous;
//using SigabaBruteforce.Cryptool.PluginBase.Control;

namespace SigabaBruteforce
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Julian Weyers", "weyers@cryptool.org", "CrypTool 2 Team", "http://cryptool2.vs.uni-due.de")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("SigabaBruteforce.Properties.Resources", "PluginCaption", "PluginToolTip",
        "Enigma/DetailedDescription/doc.xml",
        "Sigaba/Images/Icon.png", "Enigma/Images/encrypt.png", "Enigma/Images/decrypt.png")]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class SigabaBruteforce : ICrypComponent
    {
        private IControlSigabaEncryption controlMaster;

        private IControlCost costMaster;
        private Boolean stop;

        #region Constructor 

        public SigabaBruteforce()
        {
            var sigpa = new SigabaBruteforceQuickWatchPresentation();
            Presentation = sigpa;
            sigpa.doppelClick += doppelClick;
        }

        #endregion

        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        public readonly SigabaBruteforceSettings _settings = new SigabaBruteforceSettings();

        #endregion

        #region Data Properties

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "Input name", "Input tooltip description")]
        public string Input { get; set; }

        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "Output name", "Output tooltip description")]
        public string Output { get; set; }

        #endregion

        [PropertyInfo(Direction.ControlMaster, "ControlMasterCaption", "ControlMasterTooltip", false)]
        public IControlSigabaEncryption ControlMaster
        {
            get { return controlMaster; }
            set
            {
                // value.OnStatusChanged += onStatusChanged;
                controlMaster = value;
                OnPropertyChanged("ControlMaster");
            }
        }

        [PropertyInfo(Direction.ControlMaster, "CostMasterCaption", "CostMasterTooltip", false)]
        public IControlCost CostMaster
        {
            get { return costMaster; }
            set { costMaster = value; }
        }

        #region ICrypComponent Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return _settings; }
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
            stop = false;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            bruteforceAlphabetMaze();

            ProgressChanged(1, 1);
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
            stop = true;
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

        #region Bruteforce 

        private LinkedList<ValueKey> list1;
        private Queue valuequeue;

        private int[][] rotorSettings()
        {
            var value = new[]
                            {
                                new[]
                                    {
                                        _settings.Cipher1AnalysisUseRotor1 ? 1 : -1,
                                        _settings.Cipher1AnalysisUseRotor2 ? 2 : -1,
                                        _settings.Cipher1AnalysisUseRotor3 ? 3 : -1,
                                        _settings.Cipher1AnalysisUseRotor4 ? 4 : -1,
                                        _settings.Cipher1AnalysisUseRotor5 ? 5 : -1,
                                        _settings.Cipher1AnalysisUseRotor6 ? 6 : -1,
                                        _settings.Cipher1AnalysisUseRotor7 ? 7 : -1,
                                        _settings.Cipher1AnalysisUseRotor8 ? 8 : -1,
                                        _settings.Cipher1AnalysisUseRotor9 ? 9 : -1,
                                        _settings.Cipher1AnalysisUseRotor0 ? 10 : -1,
                                    },
                                new[]
                                    {
                                        _settings.Cipher2AnalysisUseRotor1 ? 1 : -1,
                                        _settings.Cipher2AnalysisUseRotor2 ? 2 : -1,
                                        _settings.Cipher2AnalysisUseRotor3 ? 3 : -1,
                                        _settings.Cipher2AnalysisUseRotor4 ? 4 : -1,
                                        _settings.Cipher2AnalysisUseRotor5 ? 5 : -1,
                                        _settings.Cipher2AnalysisUseRotor6 ? 6 : -1,
                                        _settings.Cipher2AnalysisUseRotor7 ? 7 : -1,
                                        _settings.Cipher2AnalysisUseRotor8 ? 8 : -1,
                                        _settings.Cipher2AnalysisUseRotor9 ? 9 : -1,
                                        _settings.Cipher2AnalysisUseRotor0 ? 10 : -1
                                    },
                                new[]
                                    {
                                        _settings.Cipher3AnalysisUseRotor1 ? 1 : -1,
                                        _settings.Cipher3AnalysisUseRotor2 ? 2 : -1,
                                        _settings.Cipher3AnalysisUseRotor3 ? 3 : -1,
                                        _settings.Cipher3AnalysisUseRotor4 ? 4 : -1,
                                        _settings.Cipher3AnalysisUseRotor5 ? 5 : -1,
                                        _settings.Cipher3AnalysisUseRotor6 ? 6 : -1,
                                        _settings.Cipher3AnalysisUseRotor7 ? 7 : -1,
                                        _settings.Cipher3AnalysisUseRotor8 ? 8 : -1,
                                        _settings.Cipher3AnalysisUseRotor9 ? 9 : -1,
                                        _settings.Cipher3AnalysisUseRotor0 ? 10 : -1
                                    },
                                new[]
                                    {
                                        _settings.Cipher4AnalysisUseRotor1 ? 1 : -1,
                                        _settings.Cipher4AnalysisUseRotor2 ? 2 : -1,
                                        _settings.Cipher4AnalysisUseRotor3 ? 3 : -1,
                                        _settings.Cipher4AnalysisUseRotor4 ? 4 : -1,
                                        _settings.Cipher4AnalysisUseRotor5 ? 5 : -1,
                                        _settings.Cipher4AnalysisUseRotor6 ? 6 : -1,
                                        _settings.Cipher4AnalysisUseRotor7 ? 7 : -1,
                                        _settings.Cipher4AnalysisUseRotor8 ? 8 : -1,
                                        _settings.Cipher4AnalysisUseRotor9 ? 9 : -1,
                                        _settings.Cipher4AnalysisUseRotor0 ? 10 : -1
                                    },
                                new[]
                                    {
                                        _settings.Cipher5AnalysisUseRotor1 ? 1 : -1,
                                        _settings.Cipher5AnalysisUseRotor2 ? 2 : -1,
                                        _settings.Cipher5AnalysisUseRotor3 ? 3 : -1,
                                        _settings.Cipher5AnalysisUseRotor4 ? 4 : -1,
                                        _settings.Cipher5AnalysisUseRotor5 ? 5 : -1,
                                        _settings.Cipher5AnalysisUseRotor6 ? 6 : -1,
                                        _settings.Cipher5AnalysisUseRotor7 ? 7 : -1,
                                        _settings.Cipher5AnalysisUseRotor8 ? 8 : -1,
                                        _settings.Cipher5AnalysisUseRotor9 ? 9 : -1,
                                        _settings.Cipher5AnalysisUseRotor0 ? 10 : -1
                                    },
                                new[]
                                    {
                                        _settings.Control1AnalysisUseRotor1 ? 1 : -1,
                                        _settings.Control1AnalysisUseRotor2 ? 2 : -1,
                                        _settings.Control1AnalysisUseRotor3 ? 3 : -1,
                                        _settings.Control1AnalysisUseRotor4 ? 4 : -1,
                                        _settings.Control1AnalysisUseRotor5 ? 5 : -1,
                                        _settings.Control1AnalysisUseRotor6 ? 6 : -1,
                                        _settings.Control1AnalysisUseRotor7 ? 7 : -1,
                                        _settings.Control1AnalysisUseRotor8 ? 8 : -1,
                                        _settings.Control1AnalysisUseRotor9 ? 9 : -1,
                                        _settings.Control1AnalysisUseRotor0 ? 10 : -1
                                    },
                                new[]
                                    {
                                        _settings.Control2AnalysisUseRotor1 ? 1 : -1,
                                        _settings.Control2AnalysisUseRotor2 ? 2 : -1,
                                        _settings.Control2AnalysisUseRotor3 ? 3 : -1,
                                        _settings.Control2AnalysisUseRotor4 ? 4 : -1,
                                        _settings.Control2AnalysisUseRotor5 ? 5 : -1,
                                        _settings.Control2AnalysisUseRotor6 ? 6 : -1,
                                        _settings.Control2AnalysisUseRotor7 ? 7 : -1,
                                        _settings.Control2AnalysisUseRotor8 ? 8 : -1,
                                        _settings.Control2AnalysisUseRotor9 ? 9 : -1,
                                        _settings.Control2AnalysisUseRotor0 ? 10 : -1
                                    },
                                new[]
                                    {
                                        _settings.Control3AnalysisUseRotor1 ? 1 : -1,
                                        _settings.Control3AnalysisUseRotor2 ? 2 : -1,
                                        _settings.Control3AnalysisUseRotor3 ? 3 : -1,
                                        _settings.Control3AnalysisUseRotor4 ? 4 : -1,
                                        _settings.Control3AnalysisUseRotor5 ? 5 : -1,
                                        _settings.Control3AnalysisUseRotor6 ? 6 : -1,
                                        _settings.Control3AnalysisUseRotor7 ? 7 : -1,
                                        _settings.Control3AnalysisUseRotor8 ? 8 : -1,
                                        _settings.Control3AnalysisUseRotor9 ? 9 : -1,
                                        _settings.Control3AnalysisUseRotor9 ? 10 : -1
                                    },
                                new[]
                                    {
                                        _settings.Control4AnalysisUseRotor1 ? 1 : -1,
                                        _settings.Control4AnalysisUseRotor2 ? 2 : -1,
                                        _settings.Control4AnalysisUseRotor3 ? 3 : -1,
                                        _settings.Control4AnalysisUseRotor4 ? 4 : -1,
                                        _settings.Control4AnalysisUseRotor5 ? 5 : -1,
                                        _settings.Control4AnalysisUseRotor6 ? 6 : -1,
                                        _settings.Control4AnalysisUseRotor7 ? 7 : -1,
                                        _settings.Control4AnalysisUseRotor8 ? 8 : -1,
                                        _settings.Control4AnalysisUseRotor9 ? 9 : -1,
                                        _settings.Control4AnalysisUseRotor0 ? 10 : -1
                                    },
                                new[]
                                    {
                                        _settings.Control5AnalysisUseRotor1 ? 1 : -1,
                                        _settings.Control5AnalysisUseRotor2 ? 2 : -1,
                                        _settings.Control5AnalysisUseRotor3 ? 3 : -1,
                                        _settings.Control5AnalysisUseRotor4 ? 4 : -1,
                                        _settings.Control5AnalysisUseRotor5 ? 5 : -1,
                                        _settings.Control5AnalysisUseRotor6 ? 6 : -1,
                                        _settings.Control5AnalysisUseRotor7 ? 7 : -1,
                                        _settings.Control5AnalysisUseRotor8 ? 8 : -1,
                                        _settings.Control5AnalysisUseRotor9 ? 9 : -1,
                                        _settings.Control5AnalysisUseRotor0 ? 10 : -1
                                    }
                            };


            return value;
        }

        private int[][] indexRotorSettings()
        {
            var value = new[]
                            {
                                new[]
                                    {
                                        _settings.Index1AnalysisUseRotor1 ? 1 : -1,
                                        _settings.Index1AnalysisUseRotor2 ? 2 : -1,
                                        _settings.Index1AnalysisUseRotor3 ? 3 : -1,
                                        _settings.Index1AnalysisUseRotor4 ? 4 : -1,
                                        _settings.Index1AnalysisUseRotor5 ? 5 : -1,
                                    },
                                new[]
                                    {
                                        _settings.Index2AnalysisUseRotor1 ? 1 : -1,
                                        _settings.Index2AnalysisUseRotor2 ? 2 : -1,
                                        _settings.Index2AnalysisUseRotor3 ? 3 : -1,
                                        _settings.Index2AnalysisUseRotor4 ? 4 : -1,
                                        _settings.Index2AnalysisUseRotor5 ? 5 : -1,
                                    },
                                new[]
                                    {
                                        _settings.Index3AnalysisUseRotor1 ? 1 : -1,
                                        _settings.Index3AnalysisUseRotor2 ? 2 : -1,
                                        _settings.Index3AnalysisUseRotor3 ? 3 : -1,
                                        _settings.Index3AnalysisUseRotor4 ? 4 : -1,
                                        _settings.Index3AnalysisUseRotor5 ? 5 : -1
                                    },
                                new[]
                                    {
                                        _settings.Index4AnalysisUseRotor1 ? 1 : -1,
                                        _settings.Index4AnalysisUseRotor2 ? 2 : -1,
                                        _settings.Index4AnalysisUseRotor3 ? 3 : -1,
                                        _settings.Index4AnalysisUseRotor4 ? 4 : -1,
                                        _settings.Index4AnalysisUseRotor5 ? 5 : -1
                                    },
                                new[]
                                    {
                                        _settings.Index5AnalysisUseRotor1 ? 1 : -1,
                                        _settings.Index5AnalysisUseRotor2 ? 2 : -1,
                                        _settings.Index5AnalysisUseRotor3 ? 3 : -1,
                                        _settings.Index5AnalysisUseRotor4 ? 4 : -1,
                                        _settings.Index5AnalysisUseRotor5 ? 5 : -1
                                    }
                            };


            return value;
        }
        /*
        private int[] getWhiteList()
        {
            int[] getSettings = new[]
                            {
                                _settings.CipherRotor1Rev, _settings.CipherRotor2Rev, _settings.CipherRotor3Rev,
                                _settings.CipherRotor4Rev, _settings.CipherRotor5Rev, _settings.ControlRotor1Rev,
                                _settings.ControlRotor2Rev, _settings.ControlRotor3Rev, _settings.ControlRotor4Rev,
                                _settings.ControlRotor5Rev
                            };

            List<int> value = new List<int>();
            
            for (int r = 0; r < 1024; r++)
            {
            start:

                if (r == 1024)
                    break;
                string bin = GetIntBinaryString(r);
                //reversekey = bin.Replace('1', 'R').Replace('0', ' ');

                bool b = true;
                for (int i = 0; i < bin.Length; i++)
                {
                    if (getSettings[i] == 1 && bin[i] == '1')
                    {
                        r++;
                        goto start;
                    }
                    if (getSettings[i] == 2 && bin[i] == '0')
                    {
                        r++;
                        goto start;
                    }

                   
                }
                value.Add(r);
            }

            int[] ret = new int[value.Count];

            for (int i=0;i <value.Count;i++)
            {
                ret[i] = value[i];
            }

            return ret;
        }
        */
        private static string GetIntBinaryString(int n)
        {
            var b = new char[10];
            int pos = 9;
            int i = 0;

            while (i < 10)
            {
                if ((n & (1 << i)) != 0)
                {
                    b[pos] = '1';
                }
                else
                {
                    b[pos] = '0';
                }
                pos--;
                i++;
            }
            return new string(b);
        }

        private void bruteforceAlphabetMaze()
        {
            DateTime lastUpdate = DateTime.Now;
            DateTime starttime = DateTime.Now;
            BigInteger isummax = _settings.getKeyspaceAsLong();
            BigInteger icount = 0;
            List<double> bestlist = new List<double>();
            
            double best = Double.MinValue;
            bestlist.Add(Double.MinValue);

            int[][] indexarr = indexRotorSettings();
            int[][] controlarr = rotorSettings();

            int[] whitelist = _settings.getWhiteList();

            int[] arr2 = _settings.setStartingArr(indexarr);

            string input = controlMaster.preFormatInput(Input);

            string lastkey = "1111111111";
            string reversekey;
            string bin = "";

            

            if (costMaster.GetRelationOperator() == RelationOperator.LessThen)
            {
                best = Double.MaxValue;
                bestlist.Add(Double.MaxValue);
            }

            isummax *= whitelist.Length;
            list1 = getDummyLinkedList(best);
            valuequeue = Queue.Synchronized(new Queue());

            do
            {
                for (byte i = 0; i < arr2.Length; i++)
                {
                    controlMaster.setIndexRotors(i, (byte) arr2[i]);
                }
                
                for (int i1 = _settings.IndexRotor1From;
                     i1 < _settings.IndexRotor1To + 1;
                     i1++)
                {
                    ControlMaster.setPositionsIndex((byte) arr2[0], 0, (byte) i1);

                    for (int i2 = _settings.IndexRotor2From;
                         i2 < _settings.IndexRotor2To + 1;
                         i2++)
                    {
                        ControlMaster.setPositionsIndex((byte) arr2[1], 1, (byte) i2);

                        for (int i3 = _settings.IndexRotor3From;
                             i3 < _settings.IndexRotor3To + 1;
                             i3++)
                        {
                            ControlMaster.setPositionsIndex((byte) arr2[2], 2, (byte) i3);
                            for (int i4 = _settings.IndexRotor4From;
                                 i4 < _settings.IndexRotor4To + 1;
                                 i4++)
                            {
                                ControlMaster.setPositionsIndex((byte) arr2[3], 3, (byte) i4);
                                for (
                                    int i5 =
                                        _settings.IndexRotor5From;
                                    i5 < _settings.IndexRotor5To + 1;
                                    i5++)
                                {
                                    ControlMaster.setPositionsIndex((byte) arr2[4], 4, (byte) i5);
                                    
                                    int[] arr = _settings.setStartingArr(controlarr);
                                    
                                    do
                                    {
                                        for (byte i = 0; i < arr.Length; i++)
                                        {
                                            if (i < 5)
                                            {
                                                controlMaster.setCipherRotors(i, (byte) arr[i]);
                                            }
                                            else
                                            {
                                                controlMaster.setControlRotors(i, (byte) arr[i]);
                                            }
                                        }

                                        for (int co5 = _settings.ControlRotor5From;
                                             co5 < _settings.ControlRotor5To + 1;
                                             co5++)
                                        {
                                            ControlMaster.setPositionsControl((byte)arr[9], 9, (byte)co5);
                                            
                                            
                                            for (int r = 0; r < whitelist.Length; r++)
                                            {
                                                bin = GetIntBinaryString(whitelist[r]);
                                                reversekey = bin.Replace('1', 'R').Replace('0', ' ');
                                                
                                                for (int i = 0; i < bin.Length; i++)
                                                {
                                                //        if (lastkey[i] != bin[i])
                                                        ControlMaster.setBool((byte) arr[i], (byte) i, bin[i]%2 == 1);
                                                }
                                              //  lastkey = bin;

                                                ControlMaster.setIndexMaze();
                                            
                                                for (int ci1 = _settings.CipherRotor1From;
                                                     ci1 < _settings.CipherRotor1To + 1;
                                                     ci1++)
                                                {
                                                    for (int ci2 = _settings.CipherRotor2From;
                                                         ci2 < _settings.CipherRotor2To + 1;
                                                         ci2++)
                                                    {
                                                        for (int ci3 = _settings.CipherRotor3From;
                                                             ci3 < _settings.CipherRotor3To + 1;
                                                             ci3++)
                                                        {
                                                            for (int ci4 = _settings.CipherRotor4From;
                                                                 ci4 < _settings.CipherRotor4To + 1;
                                                                 ci4++)
                                                            {
                                                                for (int ci5 = _settings.CipherRotor5From;
                                                                     ci5 < _settings.CipherRotor5To + 1;
                                                                     ci5++)
                                                                {
                                                                    for (int co1 = _settings.ControlRotor1From;
                                                                         co1 < _settings.ControlRotor1To + 1;
                                                                         co1++)
                                                                    {
                                                                        ControlMaster.setPositionsControl(
                                                                            (byte) arr[5], 5, (byte) co1);
                                                                        for (int co2 = _settings.ControlRotor2From;
                                                                             co2 < _settings.ControlRotor2To + 1;
                                                                             co2++)
                                                                        {
                                                                            for (int co3 = _settings.ControlRotor3From;
                                                                                 co3 < _settings.ControlRotor3To + 1;
                                                                                 co3++)
                                                                            {
                                                                                for (
                                                                                    int co4 =
                                                                                        _settings.ControlRotor4From;
                                                                                    co4 < _settings.ControlRotor4To + 1;
                                                                                    co4++)
                                                                                {
                                                                                    
                                                                                    byte[] plain =
                                                                                        controlMaster.DecryptFast(
                                                                                            Encoding.ASCII.GetBytes(
                                                                                                input), arr,
                                                                                            new[]
                                                                                                {
                                                                                                    (byte) ci1,
                                                                                                    (byte) ci2,
                                                                                                    (byte) ci3,
                                                                                                    (byte) ci4,
                                                                                                    (byte) ci5,
                                                                                                    (byte) co1,
                                                                                                    (byte) co2,
                                                                                                    (byte) co3,
                                                                                                    (byte) co4,
                                                                                                    (byte) co5
                                                                                                });
                                                                                    double val =
                                                                                        costMaster.CalculateCost(plain);

                                                                                    if (
                                                                                        costMaster.
                                                                                            GetRelationOperator() ==
                                                                                        RelationOperator.LessThen)
                                                                                    {
                                                                                        if (val <= bestlist.Last() )
                                                                                        {
                                                                                            bestlist.Add(val);
                                                                                            bestlist.Sort();
                                                                                            
                                                                                            if (bestlist.Count > 10)
                                                                                                bestlist.RemoveAt(10);



                                                                                            var valkey =
                                                                                                new ValueKey();
                                                                                            String keyStr = "";

                                                                                            var builderCipherKey =
                                                                                                new StringBuilder();

                                                                                            builderCipherKey.Append(
                                                                                                (char) (ci1 + 65));
                                                                                            builderCipherKey.Append(
                                                                                                (char) (ci2 + 65));
                                                                                            builderCipherKey.Append(
                                                                                                (char) (ci3 + 65));
                                                                                            builderCipherKey.Append(
                                                                                                (char) (ci4 + 65));
                                                                                            builderCipherKey.Append(
                                                                                                (char) (ci5 + 65));
                                                                                            string cipherKey =
                                                                                                builderCipherKey.
                                                                                                    ToString();
                                                                                            var builderControlKey =
                                                                                                new StringBuilder();
                                                                                            builderControlKey.Append(
                                                                                                (char) (co1 + 65));
                                                                                            builderControlKey.Append(
                                                                                                (char) (co2 + 65));
                                                                                            builderControlKey.Append(
                                                                                                (char) (co3 + 65));
                                                                                            builderControlKey.Append(
                                                                                                (char) (co4 + 65));
                                                                                            builderControlKey.Append(
                                                                                                (char) (co5 + 65));
                                                                                            string controlKey =
                                                                                                builderControlKey.
                                                                                                    ToString();
                                                                                            var builderIndexKey =
                                                                                                new StringBuilder();
                                                                                            builderIndexKey.Append(i1);
                                                                                            builderIndexKey.Append(i2);
                                                                                            builderIndexKey.Append(i3);
                                                                                            builderIndexKey.Append(i4);
                                                                                            builderIndexKey.Append(i5);
                                                                                            string indexKey =
                                                                                                builderIndexKey.ToString
                                                                                                    ();
                                                                                            string CipherRotors =
                                                                                                arr[0] + "" +
                                                                                                reversekey[0] + "" +
                                                                                                arr[1] + "" +
                                                                                                reversekey[1] + "" +
                                                                                                arr[2] + "" +
                                                                                                reversekey[2] + "" +
                                                                                                arr[3] + "" +
                                                                                                reversekey[3] + "" +
                                                                                                arr[4] + "" +
                                                                                                reversekey[4];
                                                                                            string ControlRotors =
                                                                                                arr[5] + "" +
                                                                                                reversekey[5] + "" +
                                                                                                arr[6] + "" +
                                                                                                reversekey[6] + "" +
                                                                                                arr[7] + "" +
                                                                                                reversekey[7] + "" +
                                                                                                arr[8] + "" +
                                                                                                reversekey[8] + "" +
                                                                                                arr[9] + "" +
                                                                                                reversekey[9];
                                                                                            string IndexRotors =
                                                                                                arr2[0] + "" + arr2[1] +
                                                                                                "" + arr2[2] + "" +
                                                                                                arr2[3] + "" + arr2[4];
                                                                                            valkey.decryption = plain;
                                                                                            valkey.cipherKey =
                                                                                                cipherKey;
                                                                                            valkey.indexKey =
                                                                                                indexKey;
                                                                                            valkey.controlKey =
                                                                                                controlKey;
                                                                                            valkey.cipherRotors =
                                                                                                CipherRotors;
                                                                                            valkey.controlRotors =
                                                                                                ControlRotors;
                                                                                            valkey.indexRotors =
                                                                                                IndexRotors;
                                                                                            valkey.value = val;
                                                                                            valuequeue.Enqueue(
                                                                                                valkey);
                                                                                        }
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        if (val >= bestlist.Last() )
                                                                                        {
                                                                                            bestlist.Add(val);
                                                                                            bestlist.Sort();
                                                                                            bestlist.Reverse();

                                                                                            if (bestlist.Count > 10)
                                                                                                bestlist.RemoveAt(10);


                                                                                            var valkey =
                                                                                                new ValueKey();
                                                                                            String keyStr = "";

                                                                                            var builderCipherKey =
                                                                                                new StringBuilder();

                                                                                            builderCipherKey.Append(
                                                                                                (char) (ci1 + 65));
                                                                                            builderCipherKey.Append(
                                                                                                (char) (ci2 + 65));
                                                                                            builderCipherKey.Append(
                                                                                                (char) (ci3 + 65));
                                                                                            builderCipherKey.Append(
                                                                                                (char) (ci4 + 65));
                                                                                            builderCipherKey.Append(
                                                                                                (char) (ci5 + 65));
                                                                                            string cipherKey =
                                                                                                builderCipherKey.
                                                                                                    ToString();
                                                                                            var builderControlKey =
                                                                                                new StringBuilder();
                                                                                            builderControlKey.Append(
                                                                                                (char) (co1 + 65));
                                                                                            builderControlKey.Append(
                                                                                                (char) (co2 + 65));
                                                                                            builderControlKey.Append(
                                                                                                (char) (co3 + 65));
                                                                                            builderControlKey.Append(
                                                                                                (char) (co4 + 65));
                                                                                            builderControlKey.Append(
                                                                                                (char) (co5 + 65));
                                                                                            string controlKey =
                                                                                                builderControlKey.
                                                                                                    ToString();
                                                                                            var builderIndexKey =
                                                                                                new StringBuilder();
                                                                                            builderIndexKey.Append(i1);
                                                                                            builderIndexKey.Append(i2);
                                                                                            builderIndexKey.Append(i3);
                                                                                            builderIndexKey.Append(i4);
                                                                                            builderIndexKey.Append(i5);
                                                                                            string indexKey =
                                                                                                builderIndexKey.
                                                                                                    ToString();
                                                                                            string CipherRotors =
                                                                                                arr[0] + "" +
                                                                                                reversekey[0] + "" +
                                                                                                arr[1] + "" +
                                                                                                reversekey[1] + "" +
                                                                                                arr[2] + "" +
                                                                                                reversekey[2] + "" +
                                                                                                arr[3] + "" +
                                                                                                reversekey[3] + "" +
                                                                                                arr[4] + "" +
                                                                                                reversekey[4];
                                                                                            string ControlRotors =
                                                                                                arr[5] + "" +
                                                                                                reversekey[5] + "" +
                                                                                                arr[6] + "" +
                                                                                                reversekey[6] + "" +
                                                                                                arr[7] + "" +
                                                                                                reversekey[7] + "" +
                                                                                                arr[8] + "" +
                                                                                                reversekey[8] + "" +
                                                                                                arr[9] + "" +
                                                                                                reversekey[9];
                                                                                            string IndexRotors =
                                                                                                arr2[0] + "" + arr2[1] +
                                                                                                "" + arr2[2] + "" +
                                                                                                arr2[3] + "" + arr2[4];
                                                                                            valkey.decryption = plain;
                                                                                            valkey.cipherKey =
                                                                                                cipherKey;
                                                                                            valkey.indexKey =
                                                                                                indexKey;
                                                                                            valkey.controlKey =
                                                                                                controlKey;
                                                                                            valkey.cipherRotors =
                                                                                                CipherRotors;
                                                                                            valkey.controlRotors =
                                                                                                ControlRotors;
                                                                                            valkey.indexRotors =
                                                                                                IndexRotors;
                                                                                            valkey.value = val;
                                                                                            valuequeue.Enqueue(
                                                                                                valkey);
                                                                                        }
                                                                                    }

                                                                                    if (
                                                                                        lastUpdate.AddMilliseconds(
                                                                                            500) <
                                                                                        DateTime.Now)
                                                                                    {
                                                                                        UpdatePresentationList(
                                                                                            isummax,
                                                                                            icount,
                                                                                            starttime);
                                                                                        lastUpdate = DateTime.Now;
                                                                                    }

                                                                                    icount++;

                                                                                    if (stop)
                                                                                    {
                                                                                        return;
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    } 
                                    while (_settings.NextPermutation(arr, controlarr));
                                }
                            }
                        }
                    }
                }
            } while (_settings.NextPermutation(arr2, indexarr));
            UpdatePresentationList(isummax, icount, starttime);
            
        }

        

        private void doppelClick(object sender, EventArgs e)
        {
            var lvi = sender as ListViewItem;
            var rse = lvi.Content as ResultEntry;
            Output = rse.Text;
            OnPropertyChanged("Output");
        }

        private LinkedList<ValueKey> getDummyLinkedList(double best)
        {
            var valueKey = new ValueKey();
            valueKey.value = best;
            valueKey.cipherKey = "dummykey";
            valueKey.controlKey = "dummykey";
            valueKey.indexKey = "dummykey";
            valueKey.cipherRotors = "dummykey";
            valueKey.controlRotors = "dummykey";

            valueKey.decryption = new byte[0];
            var list = new LinkedList<ValueKey>();
            LinkedListNode<ValueKey> node = list.AddFirst(valueKey);
            for (int i = 0; i < 9; i++)
            {
                node = list.AddAfter(node, valueKey);
            }
            return list;
        }

        private DateTime UpdatePresentationList(BigInteger size, BigInteger sum, DateTime starttime)
        {
            DateTime lastUpdate;
            updateToplist(list1);
            showProgress(starttime, size, sum);

            double d = (((double) sum/(double) size)*100);

            ProgressChanged(d, 100);
            lastUpdate = DateTime.Now;
            return lastUpdate;
        }

        private void showProgress(DateTime startTime, BigInteger size, BigInteger sum)
        {
            LinkedListNode<ValueKey> linkedListNode;
            if (Presentation.IsVisible)
            {
                DateTime currentTime = DateTime.Now;

                TimeSpan elapsedtime = DateTime.Now.Subtract(startTime);
                ;
                var elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes,
                                               elapsedtime.Seconds, 0);


                TimeSpan span = currentTime.Subtract(startTime);
                int seconds = span.Seconds;
                int minutes = span.Minutes;
                int hours = span.Hours;
                int days = span.Days;

                long allseconds = seconds + 60*minutes + 60*60*hours + 24*60*60*days;
                if (allseconds == 0) allseconds = 1;

                if (allseconds == 0)
                    allseconds = 1;

                double keysPerSec = Math.Round((double) sum/allseconds, 2);

                BigInteger keystodo = (size - sum);


                if (keysPerSec == 0)
                    keysPerSec = 1;

                double secstodo = ((double)keystodo / keysPerSec);

                //dummy Time 
                var endTime = new DateTime(1970, 1, 1);
                try
                {
                    endTime = DateTime.Now.AddSeconds(secstodo);
                }
                catch
                {
                }


                (Presentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                                                                                                          {
                                                                                                              ((
                                                                                                               SigabaBruteforceQuickWatchPresentation
                                                                                                               )
                                                                                                               Presentation)
                                                                                                                  .
                                                                                                                  startTime
                                                                                                                  .
                                                                                                                  Content
                                                                                                                  = "" +
                                                                                                                    startTime;
                                                                                                              ((
                                                                                                               SigabaBruteforceQuickWatchPresentation
                                                                                                               )
                                                                                                               Presentation)
                                                                                                                  .
                                                                                                                  keysPerSecond
                                                                                                                  .
                                                                                                                  Content
                                                                                                                  = "" +
                                                                                                                    keysPerSec;


                                                                                                              if (
                                                                                                                  endTime !=
                                                                                                                  (new DateTime
                                                                                                                      (1970,
                                                                                                                       1,
                                                                                                                       1)))
                                                                                                              {
                                                                                                                  ((
                                                                                                                   SigabaBruteforceQuickWatchPresentation
                                                                                                                   )
                                                                                                                   Presentation).timeLeft.Content = "" + endTime.Subtract(DateTime.Now).ToString(@"dd\.hh\:mm\:ss");
                                                                                                                  ((
                                                                                                                   SigabaBruteforceQuickWatchPresentation
                                                                                                                   )
                                                                                                                   Presentation)
                                                                                                                      .
                                                                                                                      elapsedTime
                                                                                                                      .
                                                                                                                      Content
                                                                                                                      =
                                                                                                                      "" +
                                                                                                                      elapsedspan;
                                                                                                                  ((
                                                                                                                   SigabaBruteforceQuickWatchPresentation
                                                                                                                   )
                                                                                                                   Presentation)
                                                                                                                      .
                                                                                                                      endTime
                                                                                                                      .
                                                                                                                      Content
                                                                                                                      =
                                                                                                                      "" +
                                                                                                                      endTime;
                                                                                                              }
                                                                                                              else
                                                                                                              {
                                                                                                                  ((
                                                                                                                   SigabaBruteforceQuickWatchPresentation
                                                                                                                   )
                                                                                                                   Presentation)
                                                                                                                      .
                                                                                                                      timeLeft
                                                                                                                      .
                                                                                                                      Content
                                                                                                                      =
                                                                                                                      "incalculable";

                                                                                                                  ((
                                                                                                                   SigabaBruteforceQuickWatchPresentation
                                                                                                                   )
                                                                                                                   Presentation)
                                                                                                                      .
                                                                                                                      endTime
                                                                                                                      .
                                                                                                                      Content
                                                                                                                      =
                                                                                                                      "in a galaxy far, far away...";
                                                                                                              }
                                                                                                              if (
                                                                                                                  list1 !=
                                                                                                                  null)
                                                                                                              {
                                                                                                                  linkedListNode
                                                                                                                      =
                                                                                                                      list1
                                                                                                                          .
                                                                                                                          First;
                                                                                                                  ((
                                                                                                                   SigabaBruteforceQuickWatchPresentation
                                                                                                                   )
                                                                                                                   Presentation)
                                                                                                                      .
                                                                                                                      entries
                                                                                                                      .
                                                                                                                      Clear
                                                                                                                      ();
                                                                                                                  int i
                                                                                                                      =
                                                                                                                      0;
                                                                                                                  while
                                                                                                                      (
                                                                                                                      linkedListNode !=
                                                                                                                      null)
                                                                                                                  {
                                                                                                                      i
                                                                                                                          ++;
                                                                                                                      var
                                                                                                                          entry
                                                                                                                              =
                                                                                                                              new ResultEntry
                                                                                                                                  ();
                                                                                                                      entry.Ranking = i.ToString(CultureInfo.InvariantCulture);


                                                                                                                      String
                                                                                                                          dec
                                                                                                                              =
                                                                                                                              Encoding
                                                                                                                                  .
                                                                                                                                  ASCII
                                                                                                                                  .
                                                                                                                                  GetString
                                                                                                                                  (linkedListNode
                                                                                                                                       .
                                                                                                                                       Value
                                                                                                                                       .
                                                                                                                                       decryption);
                                                                                                                      if
                                                                                                                          (
                                                                                                                          dec
                                                                                                                              .
                                                                                                                              Length >
                                                                                                                          2500)
                                                                                                                          // Short strings need not to be cut off
                                                                                                                      {
                                                                                                                          dec
                                                                                                                              =
                                                                                                                              dec
                                                                                                                                  .
                                                                                                                                  Substring
                                                                                                                                  (0,
                                                                                                                                   2500);
                                                                                                                      }
                                                                                                                      entry
                                                                                                                          .
                                                                                                                          Text
                                                                                                                          =
                                                                                                                          dec;
                                                                                                                      entry
                                                                                                                          .
                                                                                                                          CipherKey
                                                                                                                          =
                                                                                                                          linkedListNode
                                                                                                                              .
                                                                                                                              Value
                                                                                                                              .
                                                                                                                              cipherKey;
                                                                                                                      entry
                                                                                                                          .
                                                                                                                          IndexKey
                                                                                                                          =
                                                                                                                          linkedListNode
                                                                                                                              .
                                                                                                                              Value
                                                                                                                              .
                                                                                                                              indexKey;
                                                                                                                      entry
                                                                                                                          .
                                                                                                                          ControlKey
                                                                                                                          =
                                                                                                                          linkedListNode
                                                                                                                              .
                                                                                                                              Value
                                                                                                                              .
                                                                                                                              controlKey;
                                                                                                                      entry
                                                                                                                          .
                                                                                                                          CipherRotors
                                                                                                                          =
                                                                                                                          linkedListNode
                                                                                                                              .
                                                                                                                              Value
                                                                                                                              .
                                                                                                                              cipherRotors;
                                                                                                                      entry
                                                                                                                          .
                                                                                                                          ControlRotors
                                                                                                                          =
                                                                                                                          linkedListNode
                                                                                                                              .
                                                                                                                              Value
                                                                                                                              .
                                                                                                                              controlRotors;
                                                                                                                      entry
                                                                                                                          .
                                                                                                                          IndexRotors
                                                                                                                          =
                                                                                                                          linkedListNode
                                                                                                                              .
                                                                                                                              Value
                                                                                                                              .
                                                                                                                              indexRotors;
                                                                                                                      entry
                                                                                                                          .
                                                                                                                          Value
                                                                                                                          =
                                                                                                                          Math
                                                                                                                              .
                                                                                                                              Round
                                                                                                                              (linkedListNode
                                                                                                                                   .
                                                                                                                                   Value
                                                                                                                                   .
                                                                                                                                   value,
                                                                                                                               2) +
                                                                                                                          "";


                                                                                                                      ((
                                                                                                                       SigabaBruteforceQuickWatchPresentation
                                                                                                                       )
                                                                                                                       Presentation)
                                                                                                                          .
                                                                                                                          entries
                                                                                                                          .
                                                                                                                          Add
                                                                                                                          (entry);

                                                                                                                      linkedListNode
                                                                                                                          =
                                                                                                                          linkedListNode
                                                                                                                              .
                                                                                                                              Next;
                                                                                                                  }
                                                                                                              }
                                                                                                          }
                                                      , null);
            }
        }

        private void updateToplist(LinkedList<ValueKey> costList)
        {
            LinkedListNode<ValueKey> node;

            var enc = new ASCIIEncoding();

            while (valuequeue.Count != 0)
            {
                var vk = (ValueKey) valuequeue.Dequeue();
                if (costMaster.GetRelationOperator() == RelationOperator.LargerThen)
                {
                    if (vk.value > costList.Last().value)
                    {
                        node = costList.First;
                        int i = 0;
                        while (node != null)
                        {
                            if (vk.value > node.Value.value)
                            {
                                costList.AddBefore(node, vk);
                                costList.RemoveLast();
                                if (i == 0)
                                {
                                    Output = enc.GetString(vk.decryption);
                                }
                                // value_threshold = costList.Last.Value.value;
                                break;
                            }
                            node = node.Next;
                            i++;
                        } //end while
                    } //end if
                }
                else
                {
                    if (vk.value < costList.Last().value)
                    {
                        node = costList.First;
                        int i = 0;
                        while (node != null)
                        {
                            if (vk.value < node.Value.value)
                            {
                                costList.AddBefore(node, vk);
                                costList.RemoveLast();
                                if (i == 0)
                                {
                                    Output = enc.GetString(vk.decryption);
                                }

                                // value_threshold = costList.Last.Value.value;
                                break;
                            }
                            node = node.Next;
                            i++;
                        } //end while
                    } //end if
                }
            }
            OnPropertyChanged("Output");
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


    /*namespace Cryptool.PluginBase.Control
    {
        public interface IControlSigabaEncryption : IControl, IDisposable
        {
            string Decrypt(string ciphertext);
            void setInternalConfig();
            void changeSettings(string setting, object value);

            byte[] DecryptFast(byte[] ciphertext, int[] a, byte[] positions);

            void setCipherRotors(int i, byte a);

            void setControlRotors(byte i, byte b);

            void setIndexRotors(byte i, byte c);

            void setIndexMaze();

            void setIndexMaze(int[] indexmaze);

            void setBool(byte ix, byte i, bool rev);

            void setPositionsControl(byte ix, byte i, byte position);

            void setPositionsIndex(byte ix, byte i, byte position);

            string preFormatInput(string text);
            string postFormatOutput(string text);
        }
    }*/
}