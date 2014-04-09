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
using System.Diagnostics;
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
using ExamplePluginCT2;

namespace StampChallenge2
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Anonymous", "coredevs@cryptool.org", "CrypTool 2 Team", "http://cryptool2.vs.uni-due.de")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("Stamp Challenge(old)", "Subtract one number from another", "ExamplePluginCT2/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class ExamplePluginCT2 : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly ExamplePluginCT2Settings settings = new ExamplePluginCT2Settings();
        private SigabaCoreFast _core;
        private StampChallenge2Presentation _presentation = new StampChallenge2Presentation();
        private IControlCost costMaster;
        private LinkedList<ValueKey> list1;
        private Queue valuequeue;
        List<double> bestlist = new List<double>();
        DateTime lastUpdate = DateTime.Now;
        DateTime starttime = DateTime.Now;

        public ExamplePluginCT2()
        {
            Presentation = _presentation;
            _core = new SigabaCoreFast(this);
        }

        #endregion

        #region Data Properties

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "Cipher", "Input tooltip description")]
        public String Cipher
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "Crib", "Input tooltip description")]
        public String Crib
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


        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "Output name", "Output tooltip description")]
        public string Output
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
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {

            

            double best = Double.MinValue;
            bestlist.Add(Double.MinValue);

            if (costMaster.GetRelationOperator() == RelationOperator.LessThen)
            {
                best = Double.MaxValue;
                bestlist.Add(Double.MaxValue);
            }
            starttime = DateTime.Now;
            list1 = getDummyLinkedList(best);
            valuequeue = Queue.Synchronized(new Queue());

            // HOWTO: Use this to show the progress of a plugin algorithm execution in the editor.
            ProgressChanged(0, 1);

            // HOWTO: After you have changed an output property, make sure you announce the name of the changed property to the CT2 core.
            //SomeOutput = SomeInput - settings.SomeParameter;
            /*Thread t1 = new Thread(bruteforceAlphabetMaze);
            Thread t2 = new Thread(bruteforceAlphabetMaze);

            t1.Start(0);
            t2.Start(1);
            
           /*while(true)
            {
                if (scape != "")
                {
                    Console.WriteLine(scape);

                    scape = "";
                }
            
            Thread.Sleep(30000);
            }*/

            bruteforceAlphabetMaze();

            OnPropertyChanged("SomeOutput");

            // HOWTO: You can pass error, warning, info or debug messages to the CT2 main window.
            if (settings.SomeParameter < 0)
                GuiLogMessage("SomeParameter is negative", NotificationLevel.Debug);

            // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
            ProgressChanged(1, 1);
        }
        public static string GetIntBinaryString(int n)
        {
            var b = new char[5];
            int pos = 4;
            int i = 0;

            while (i < 5)
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
            
            

            int[] foo = new int[10] {  1, 2, 3, 4 ,5,6,7,8,9,10};
            IEnumerable<IEnumerable<int>> combis = Blupp.Combinations(foo, 5);
            byte[] positions  = new byte[5]{0,1,2,3,4};

            int counter = 0;

            
            
            _core.InitializeRotors();

            for (int enumi = 0; enumi < combis.Count();enumi++ )
            {
                int[] arr = combis.ElementAt(enumi).ToArray();

                int[] f = foo.Except(arr).ToArray();
                do
                {
                    for (int y = 0; y < 5; y++)
                    {
                        _core.setCipherRotors(y, (byte)arr[y]);

                    }
                    for (int ix = 0; ix < 32; ix++)
                    {
                        String s = GetIntBinaryString(ix);
                        _core.setBool((byte)arr[0], 0, s[0] == '1');
                        _core.setBool((byte)arr[1], 1, s[1] == '1');
                        _core.setBool((byte)arr[2], 2, s[2] == '1');
                        _core.setBool((byte)arr[3], 3, s[3] == '1');
                        _core.setBool((byte)arr[4], 4, s[4] == '1');

                        _core.CipherRotors[0].Reverse = s[0] == '1';
                        _core.CipherRotors[1].Reverse = s[1] == '1';
                        _core.CipherRotors[2].Reverse = s[2] == '1';
                        _core.CipherRotors[3].Reverse = s[3] == '1';
                        _core.CipherRotors[4].Reverse = s[4] == '1';

                        /*
                        _core.CipherRotors[0].Reverse = true;
                        _core.setBool((byte) arr[0], 0, true);
                        _core.setBool((byte) arr[1], 1, true);
                        _core.setBool((byte) arr[2], 2, true);
                        _core.setBool((byte) arr[3], 3, true);
                        _core.setBool((byte) arr[4], 4, true);

                        /*_core.CipherRotors[0].Reverse = true;
                        _core.CipherRotors[1].Reverse = true;
                        _core.CipherRotors[2].Reverse = true;
                        _core.CipherRotors[3].Reverse = true;
                        _core.CipherRotors[4].Reverse = true;
                        */

                        List<int[][]> retlst = new List<int[][]>();
                        System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

                        retlst = _core.PhaseI3(enc.GetBytes(Cipher), enc.GetBytes(Crib), arr, positions);

                        _core.setCodeWheels(arr, new[] { "" });

                        foreach (int[][] intse in retlst)
                        {
                            int x = 0;
                            foreach (int i in arr)
                            {
                                Console.Write(i + "  " + _core.CipherRotors[4 - x].Reverse + "  ");
                                x++;
                            }
                            Console.WriteLine("");
                            Stopwatch sw = new Stopwatch();
                            sw.Start();
                            if (_core.stepOneCompact(intse, enc.GetBytes(Cipher), arr))
                            {
                                sw.Stop();
                            }

                            Console.WriteLine("Elapsed={0}", sw.Elapsed);
                        }



                    }

                } while (NextPermutation(arr));

                // break;
            }

            Console.WriteLine(counter);

        }

        public void addEntry(byte[] plain)
        {
            double val = costMaster.CalculateCost(plain);
            if (costMaster.GetRelationOperator() == RelationOperator.LessThen)
            {

                if (val <= bestlist.Last())
                {
                    bestlist.Add(val);
                    bestlist.Sort();
                    bestlist.Reverse();

                    if (bestlist.Count > 10)
                        bestlist.RemoveAt(10);
                    var valkey = new ValueKey();
                    valkey.decryption = plain;
                    valkey.value = val;
                    valuequeue.Enqueue(
                        valkey);
                }
            }
            else
                {
                    if (val >= bestlist.Last())
                    {
                        bestlist.Add(val);
                        bestlist.Sort();
                        bestlist.Reverse();

                        if (bestlist.Count > 10)
                            bestlist.RemoveAt(10);
                        var valkey = new ValueKey();
                        valkey.decryption = plain;
                        valkey.value = val;
                        valuequeue.Enqueue(valkey);
                    }
                }
            if (lastUpdate.AddMilliseconds(500) <DateTime.Now)
            {
                UpdatePresentationList(0,0,starttime);
                lastUpdate = DateTime.Now;
            }

                
            
        }

        private DateTime UpdatePresentationList(BigInteger size, BigInteger sum, DateTime starttime)
        {
            DateTime lastUpdate;
            updateToplist(list1);
            showProgress(starttime, size, sum);

            double d = (((double)sum / (double)size) * 100);

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

                long allseconds = seconds + 60 * minutes + 60 * 60 * hours + 24 * 60 * 60 * days;
                if (allseconds == 0) allseconds = 1;

                if (allseconds == 0)
                    allseconds = 1;

                double keysPerSec = Math.Round((double)sum / allseconds, 2);

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


                (Presentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((
                     StampChallenge2Presentation
                     )
                     Presentation)
                        .
                        startTime
                        .
                        Content
                        = "" +
                          startTime;
                    ((
                     StampChallenge2Presentation
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
                         StampChallenge2Presentation
                         )
                         Presentation).timeLeft.Content = "" + endTime.Subtract(DateTime.Now).ToString(@"dd\.hh\:mm\:ss");
                        ((
                         StampChallenge2Presentation
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
                         StampChallenge2Presentation
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
                         StampChallenge2Presentation
                         )
                         Presentation)
                            .
                            timeLeft
                            .
                            Content
                            =
                            "incalculable";

                        ((
                         StampChallenge2Presentation
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
                         StampChallenge2Presentation
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
                             StampChallenge2Presentation
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
                var vk = (ValueKey)valuequeue.Dequeue();
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

        private static bool NextPermutation(int[] numList)
        {
            /*
             Knuths
             1. Find the largest index j such that a[j] < a[j + 1]. If no such index exists, the permutation is the last permutation.
             2. Find the largest index l such that a[j] < a[l]. Since j + 1 is such an index, l is well defined and satisfies j < l.
             3. Swap a[j] with a[l].
             4. Reverse the sequence from a[j + 1] up to and including the final element a[n].

             */
            var largestIndex = -1;
            for (var i = numList.Length - 2; i >= 0; i--)
            {
                if (numList[i] < numList[i + 1])
                {
                    largestIndex = i;
                    break;
                }
            }

            if (largestIndex < 0) return false;

            var largestIndex2 = -1;
            for (var i = numList.Length - 1; i >= 0; i--)
            {
                if (numList[largestIndex] < numList[i])
                {
                    largestIndex2 = i;
                    break;
                }
            }

            var tmp = numList[largestIndex];
            numList[largestIndex] = numList[largestIndex2];
            numList[largestIndex2] = tmp;

            for (int i = largestIndex + 1, j = numList.Length - 1; i < j; i++, j--)
            {
                tmp = numList[i];
                numList[i] = numList[j];
                numList[j] = tmp;
            }

            return true;
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

        public void GuiLogMessage(string message, NotificationLevel logLevel)
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
