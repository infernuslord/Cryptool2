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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Web.UI.WebControls;
using System.Windows.Controls;
using System.Windows.Threading;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.Control;


namespace SigabaKnownPlaintext
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Anonymous", "coredevs@cryptool.org", "CrypTool 2 Team", "http://cryptool2.vs.uni-due.de")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("SIGABA-Widerspruchsbeweis", "", "SigabaPhaseI/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class SigabaKnownPlaintext : ICrypComponent
    {
        #region Private Variables

        private IControlSigabaEncryption controlMaster;
        private IControlCost costMaster;
        private SigabaKnownPlaintextPresentaion _sigpa;
        private readonly SigabaKnownPlaintextSettings settings = new SigabaKnownPlaintextSettings();
        private SigabaCoreFastKnownPlaintext _core ;
        private SigabaCoreFastPhaseI _coreP1 = new SigabaCoreFastPhaseI();

        private LinkedList<ValueKey> list1;
        private Queue valuequeue;

        #endregion

        public SigabaKnownPlaintext()
        {
            _sigpa = new SigabaKnownPlaintextPresentaion();
            _core = new SigabaCoreFastKnownPlaintext(this);
            this.Presentation= _sigpa;
            //         _coreP2 = new SigabaCoreFastPhaseII(this);
        }

        #region Data Properties

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

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "Cipher", "Input tooltip description")]
        public string Cipher
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "Crib", "Input tooltip description")]
        public string Crib
        {
            get;
            set;
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

        BlockingCollection<KeyValuePair<Survivor, int[][]>> _blocc = new BlockingCollection<KeyValuePair<Survivor, int[][]>>(100);
        Task[] _tasks = new Task[Environment.ProcessorCount];

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            // HOWTO: Use this to show the progress of a plugin algorithm execution in the editor.
            ProgressChanged(0, 1);
            List<double> bestlist = new List<double>();
            double best = Double.MinValue;
            bestlist.Add(Double.MinValue);

            if (costMaster.GetRelationOperator() == RelationOperator.LessThen)
            {
                best = Double.MaxValue;
                bestlist.Add(Double.MaxValue);
            }
            list1 = getDummyLinkedList(best);
            valuequeue = Queue.Synchronized(new Queue());
            // HOWTO: After you have changed an output property, make sure you announce the name of the changed property to the CT2 core.
            //SomeOutput = SomeInput - settings.SomeParameter;

            
            survivorProducer();
           
            Console.WriteLine(DateTime.Now);

            OnPropertyChanged("SomeOutput");

            
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

        private void survivorConsumer()
        {
            KeyValuePair<Survivor, int[][]> survivor;
            
            while (_blocc.TryTake(out survivor, Timeout.Infinite))
            {
                foreach (byte c in survivor.Key.key)
                {
                    Console.Write(c);
                }

                foreach (char c in survivor.Key.rev)
                {
                    Console.Write(c);
                }

                foreach (int c in survivor.Key.type)
                {
                    Console.Write(c);
                }
                foreach ( int[] c in survivor.Value)
                {
                    foreach (int i in c)
                    {
                        Console.Write(i);
                    }
                    Console.Write(" ; ");

                }
                Console.WriteLine();

                //_coreP2.setCodeWheels(survivor.Key.type);
                Stopwatch sw= new Stopwatch();
                sw.Start();
                //List<Candidate> winnerList = _coreP2.stepOneCompact(survivor.Value);
                sw.Stop();
                Console.WriteLine(sw.Elapsed + "+++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                

            }
        }

        ResultEntry actRes ;

        private void survivorProducer()
        {
            List<long> ticklist = new List<long>();
            var foo = new int[10] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            IEnumerable<IEnumerable<int>> combis = foo.Combinations(5);
            
            
            int counter = 0;

            //int inter = end - start;
            _core.InitializeRotors();

            for (int enumi = 0; enumi <252; enumi++)
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

                        

                        
                        var enc = new ASCIIEncoding();

                        byte[] loopvars = new byte[] { 0, 0, 0, 0, 0};

                        
                        
                        int[][] test2 = new int[5][];

                        for (int i = 0; i < 5; i++)
                        {
                            test2[i] = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
                        }

                        do
                        {
                            var retlst = new List<int[][]>();

                            _core.setCodeWheels(arr);
                            retlst = _core.PhaseI3(enc.GetBytes(Cipher), enc.GetBytes(Crib), arr, loopvars);
                            if (retlst.Count != 0)
                            {
                                List<int[]>[] treetlst = new List<int[]>[Crib.Length];
                                List<List<int>>[] pathLst = new List<List<int>>[Crib.Length];

                                for (int i = 0; i < retlst.Count; i++)
                                {
                                    for (int j = 0; j < retlst[i].Length; j++)
                                    {
                                        if (treetlst[j] == null)
                                        {

                                            treetlst[j] = new List<int[]>();
                                            treetlst[j].Add(retlst[i][j]);
                                            pathLst[j] = new List<List<int>>() {new List<int>()};

                                            //pathLst[j].Last().Add(Array.IndexOf(treetlst[j + 1].ToArray(), retlst[i][j + 1]));

                                        }

                                        Boolean b = true;
                                        for (int iy = 0; iy < treetlst[j].Count(); iy++)
                                        {
                                            if (ArraysEqual(treetlst[j][iy], retlst[i][j]))
                                            {
                                                b = false;
                                                /*    if(j>0)
                                                    if(!pathLst[j].Last().Contains(Array.IndexOf(treetlst[j+1].ToArray(),retlst[i][j+1])))
                                                    {
                                                        pathLst[j].Last().Add(Array.IndexOf(treetlst[j+1].ToArray(),retlst[i][j+1]));
                                                    }*/
                                            }
                                        }


                                        if (b)
                                        {
                                            pathLst[j].Add(new List<int>());
                                            treetlst[j].Add(retlst[i][j]);

                                        }

                                    }
                                }

                                for (int j = 0; j < treetlst.Count() - 1; j++)
                                {
                                    for (int iy = 0; iy < treetlst[j].Count(); iy++)
                                    {
                                        for (int i = 0; i < retlst.Count; i++)
                                        {
                                            if (ArraysEqual(treetlst[j][iy], retlst[i][j]))
                                            {
                                                if (
                                                    !pathLst[j][iy].Contains(
                                                        treetlst[j + 1].FindLastIndex(
                                                            item =>
                                                            ArraysEqual(item.ToArray(), retlst[i][j + 1].ToArray()))))
                                                {
                                                    /*int xgh=Array.IndexOf(treetlst[j + 1].ToArray(), retlst[i][j + 1]);
                                                    
                                                    int[][] llist = treetlst[j + 1].ToArray();
                                                    int[] gh = retlst[i][j + 1];
                                                    int xgh2 = Array.IndexOf(llist, gh);
                                                    Console.WriteLine(gh+""+xgh+"" +llist+""+xgh2);*/
                                                    pathLst[j][iy].Add(
                                                        treetlst[j + 1].FindLastIndex(
                                                            item =>
                                                            ArraysEqual(item.ToArray(), retlst[i][j + 1].ToArray())));
                                                }
                                            }
                                        }

                                    }
                                }
                                /*
                                List<Node> nodeList = new List<Node>();

                                for (int i = 0; i < retlst.Count; i++)
                                {
                                    for (int j = 0; j < retlst[i].Length; j++)
                                    {
                                        if (treetlst[j] == null)
                                        {
                                            treetlst[j] = new List<int[]>();
                                            treetlst[j].Add(retlst[i][j]);
                                        }

                                        Boolean b = true;
                                        for (int iy = 0; iy < treetlst[j].Count(); iy++)
                                        {
                                            if (ArraysEqual(treetlst[j][iy], retlst[i][j]))
                                            {
                                                b = false;
                                            }
                                        }


                                        if (b)
                                        {
                                            Node n = new Node();
                                            n.Pfad = retlst[i][j];
                                        
                                            treetlst[j].Add(retlst[i][j]);


                                        }

                                    }
                                }*/

                                Stopwatch sw = new Stopwatch();
                                sw.Start();

                                counter++;
                                actRes = new ResultEntry();
                                actRes.CipherKey = loopvars[0] + "" + loopvars[1] + "" + loopvars[2] + "" + loopvars[3] +
                                                   "" + loopvars[4];
                                actRes.CipherRotors = "" + arr[0] + "" + arr[1] + "" + arr[2] + "" + arr[3] + "" +
                                                      arr[4] + "" + s[0] + "" + s[1] + "" + s[2] + "" + s[3] + "" + s[4];

                                controlMaster.setCipherRotors(0, (byte) (arr[0]));
                                controlMaster.setCipherRotors(1, (byte) (arr[1]));
                                controlMaster.setCipherRotors(2, (byte) (arr[2]));
                                controlMaster.setCipherRotors(3, (byte) (arr[3]));
                                controlMaster.setCipherRotors(4, (byte) (arr[4]));

                                if (!treetlst.Contains(null))
                                    if (_core.stepOneCompact(enc.GetBytes(Cipher), enc.GetBytes(Crib), arr, loopvars,
                                                             treetlst, pathLst))
                                    {

                                        sw.Stop();
                                        ticklist.Add(sw.Elapsed.Ticks);
                                        TimeSpan ts = new TimeSpan((long) ticklist.Average());
                                        Console.WriteLine("time" + ts.Minutes + ":  " + ts.Seconds);
                                        Console.WriteLine("How many?" + ticklist.Count + " :  " + counter);
                                    }
                            }
                        } while (increment2(loopvars, test2));

                    }
                } while (NextPermutation(arr));
                                           
            }

                Console.WriteLine(counter);
                                     
        }

        


        static bool increment2(byte[] inc, int[][] con)
        {
            Boolean flag = false;
            for (int i = inc.Length-1; i > 0; i--)
            {
                if (inc[i] < con[i].Length - 1)
                {
                    flag = true;
                    inc[i]++;
                    break;
                }
                else
                {
                    inc[i] = 0;
                }
            }

            return flag;
        }

        static bool ArraysEqual<T>(T[] a1, T[] a2)
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < a1.Length; i++)
            {
                if (!comparer.Equals(a1[i], a2[i])) return false;
            }
            return true;
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

        internal void AddEntryConfirmed(Candidate winner, int p, int p_2)
        {
           // throw new NotImplementedException();
        }

        internal void AddEntryCandidate(Candidate winner)
        {
            //throw new NotImplementedException();
        }

        internal void AddEntryComplete(Candidate winner, int[] steppingmaze, int pos, int pos2, int p, int p_2, bool p_3, bool p_4,List<int[]> indexkey)
        {
            Console.WriteLine(actRes.CipherKey);
            
            int[] b = new int[]{(byte)(actRes.CipherRotors[0]-48),(byte)(actRes.CipherRotors[1]-48),(byte)(actRes.CipherRotors[2]-48),(byte)(actRes.CipherRotors[3]-48),(byte)(actRes.CipherRotors[4]-48),(winner.RotorTypeReal[0]),(winner.RotorTypeReal[1]),(winner.RotorTypeReal[2]),p,p_2};
            byte[] b2 = new byte[]{(byte)(actRes.CipherKey[0]-48),(byte)(actRes.CipherKey[1]-48),(byte)(actRes.CipherKey[2]-48),(byte)(actRes.CipherKey[3]-48),(byte)(actRes.CipherKey[4]-48),(byte)winner.Positions[0],(byte)winner.Positions[1],(byte)winner.Positions[2],(byte)pos,(byte)pos2};

            controlMaster.setPositionsControl( (byte)(winner.RotorTypeReal[0]), 5,(byte)winner.Positions[0]);
            controlMaster.setPositionsControl( (byte)(winner.RotorTypeReal[1]), 6,(byte)winner.Positions[1]);
            controlMaster.setPositionsControl( (byte)(winner.RotorTypeReal[2]), 7,(byte)winner.Positions[2]);
            controlMaster.setPositionsControl( (byte)(p), 8,(byte)pos);
            controlMaster.setPositionsControl( (byte)(p_2), 9,(byte)pos2);

            Task t1 = Task.Factory.StartNew(() => controlMaster.setControlRotors(5, (byte)(winner.RotorTypeReal[0])));
            Task t2 = Task.Factory.StartNew(() => controlMaster.setControlRotors(6, (byte)(winner.RotorTypeReal[1])));
            Task t3 = Task.Factory.StartNew(() => controlMaster.setControlRotors(7, (byte)(winner.RotorTypeReal[2])));
            Task t4 = Task.Factory.StartNew(() => controlMaster.setControlRotors(8, (byte)(p)));
            Task t5 = Task.Factory.StartNew(() => controlMaster.setControlRotors(9, (byte)(p_2)));

            
            Task t6 = Task.Factory.StartNew(() => controlMaster.setIndexRotors(0, (byte)(indexkey[1][0]+1)));
            Task t7 = Task.Factory.StartNew(() => controlMaster.setIndexRotors(1, (byte)(indexkey[1][1]+1)));
            Task t8 = Task.Factory.StartNew(() => controlMaster.setIndexRotors(2, (byte)(indexkey[1][2]+1)));
            Task t9 = Task.Factory.StartNew(() => controlMaster.setIndexRotors(3, (byte)(indexkey[1][3]+1)));
            Task t10 = Task.Factory.StartNew(() => controlMaster.setIndexRotors(4, (byte)(indexkey[1][4]+1)));

            Task t11 = Task.Factory.StartNew(() => controlMaster.setPositionsIndex((byte)(indexkey[1][0]+1), 0,(byte)(indexkey[0][0])));
            Task t12 = Task.Factory.StartNew(() => controlMaster.setPositionsIndex((byte)(indexkey[1][1]+1), 1,(byte)(indexkey[0][1])));
            Task t13 = Task.Factory.StartNew(() => controlMaster.setPositionsIndex((byte)(indexkey[1][2]+1), 2,(byte)(indexkey[0][2])));
            Task t14 = Task.Factory.StartNew(() => controlMaster.setPositionsIndex((byte)(indexkey[1][3]+1), 3,(byte)(indexkey[0][3])));
            Task t15 = Task.Factory.StartNew(() => controlMaster.setPositionsIndex((byte)(indexkey[1][4]+1), 4,(byte)(indexkey[0][4])));

            

            Task.WaitAll(new []{t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12,t13,t14,t15});

            Task t =  Task.Factory.StartNew(() => ControlMaster.setIndexMaze());
            t.Wait(100);


            byte[] plain = controlMaster.DecryptFast(Encoding.ASCII.GetBytes(Cipher),b,b2);
                                                                           
            double val =
                costMaster.CalculateCost(plain);

          /*  if (
                costMaster.
                    GetRelationOperator() ==
                RelationOperator.LessThen)
            {*/
                /*if (val <= bestlist.Last())
                {
                    bestlist.Add(val);
                    bestlist.Sort();

                    if (bestlist.Count > 10)
                        bestlist.RemoveAt(10);
                */


                    var valkey =
                        new ValueKey();
                    String keyStr = "";

                    var builderCipherKey =
                        new StringBuilder();

                    
                    
                    var builderControlKey =
                        new StringBuilder();
                    builderControlKey.Append(
                        (char)(winner.Positions[0] + 65));
                    builderControlKey.Append(
                        (char)(winner.Positions[1] + 65));
                    builderControlKey.Append(
                        (char)(winner.Positions[2] + 65));
                    builderControlKey.Append(
                        (char)(pos + 65));
                    builderControlKey.Append(
                        (char)(pos2 + 65));
                    string controlKey =
                        builderControlKey.
                            ToString();
                    var builderIndexKey =
                        new StringBuilder();
                    builderIndexKey.Append(indexkey[0][0]);
                    builderIndexKey.Append(indexkey[0][1]);
                    builderIndexKey.Append(indexkey[0][2]);
                    builderIndexKey.Append(indexkey[0][3]);
                    builderIndexKey.Append(indexkey[0][4]);
                    string indexKey =builderIndexKey.ToString();
                   
                    string ControlRotors =
                        winner.RotorTypeReal[0] + "" +
                        (winner.Reverse[0] ?  "R": " ") + "" +
                        winner.RotorTypeReal[1] + "" +
                        (winner.Reverse[1] ? "R":" ")+ "" +
                        winner.RotorTypeReal[2] + "" +
                        (winner.Reverse[2] ?"R":" ")+ "" +
                        p + "" +
                        (p_3? "R":" " )+ "" +
                        p_2 + "" +
                        (p_4? "R": " ");
                    string IndexRotors =
                        (int)(indexkey[1][0]+1)  + " " + (int)(indexkey[1][1]+1) + " " + (int)(indexkey[1][2] +1)+ " " + (int)(indexkey[1][3] +1)+ " " + (int)(indexkey[1][4] +1);
                    valkey.decryption = plain;
                    
                    valkey.indexKey =
                        indexKey;
                    valkey.controlKey =
                        controlKey;
                    valkey.cipherRotors = actRes.CipherRotors[0] + "" + (actRes.CipherRotors[5] == '0' ? " " : "R") + "" + actRes.CipherRotors[1] + "" + (actRes.CipherRotors[6] == '0' ? " " : "R") + " " + actRes.CipherRotors[2] + "" + (actRes.CipherRotors[7] == '0' ? " " : "R") + " " + actRes.CipherRotors[3] + "" + (actRes.CipherRotors[8] == '0' ? " " : "R") + " " + actRes.CipherRotors[4] + "" + (actRes.CipherRotors[9] == '0' ? " " : "R");
                    valkey.cipherKey= (char)(actRes.CipherKey[0] +17)+ "" + (char)(actRes.CipherKey[1]+17) + "" + (char)(actRes.CipherKey[2] +17)+ "" + (char)(actRes.CipherKey[3]+17) + "" + (char)(actRes.CipherKey[4]+17);
                    valkey.controlRotors =
                        ControlRotors;
                    valkey.indexRotors =
                        IndexRotors;
                    valkey.value = val;
                    valuequeue.Enqueue(
                        valkey);
                //}
            //}

                    UpdatePresentationList(0,0,DateTime.Now);

        }

        #region Prestnation stuff

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

        private DateTime UpdatePresentationList(int size, int sum, DateTime starttime)
        {
            DateTime lastUpdate;
            updateToplist(list1);
            showProgress(starttime, size, sum);

            double d = (((double)sum / (double)size) * 100);

            ProgressChanged(d, 100);
            lastUpdate = DateTime.Now;
            return lastUpdate;
        }

        private void showProgress(DateTime startTime, int size, int sum)
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

                int keystodo = (size - sum);


                if (keysPerSec == 0)
                    keysPerSec = 1;

                double secstodo = (keystodo/keysPerSec);

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
                                                                                                               SigabaKnownPlaintextPresentaion
                                                                                                               )
                                                                                                               Presentation)
                                                                                                                  .
                                                                                                                  startTime
                                                                                                                  .
                                                                                                                  Content
                                                                                                                  = "" +
                                                                                                                    startTime;
                                                                                                              ((
                                                                                                               SigabaKnownPlaintextPresentaion
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
                                                                                                                   SigabaKnownPlaintextPresentaion
                                                                                                                   )
                                                                                                                   Presentation)
                                                                                                                      .
                                                                                                                      timeLeft
                                                                                                                      .
                                                                                                                      Content
                                                                                                                      =
                                                                                                                      "" +
                                                                                                                      endTime
                                                                                                                          .
                                                                                                                          Subtract
                                                                                                                          (DateTime
                                                                                                                               .
                                                                                                                               Now)
                                                                                                                          .
                                                                                                                          ToString
                                                                                                                          (@"dd\.hh\:mm\:ss");
                                                                                                                  ((
                                                                                                                   SigabaKnownPlaintextPresentaion
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
                                                                                                                   SigabaKnownPlaintextPresentaion
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
                                                                                                                   SigabaKnownPlaintextPresentaion
                                                                                                                   )
                                                                                                                   Presentation)
                                                                                                                      .
                                                                                                                      timeLeft
                                                                                                                      .
                                                                                                                      Content
                                                                                                                      =
                                                                                                                      "incalculable";

                                                                                                                  ((
                                                                                                                   SigabaKnownPlaintextPresentaion
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
                                                                                                                   SigabaKnownPlaintextPresentaion
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
                                                                                                                      entry
                                                                                                                          .
                                                                                                                          Ranking
                                                                                                                          =
                                                                                                                          i
                                                                                                                              .
                                                                                                                              ToString
                                                                                                                              (CultureInfo
                                                                                                                                   .
                                                                                                                                   InvariantCulture);


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
                                                                                                                       SigabaKnownPlaintextPresentaion
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

        #endregion 

    }



    #region Presentation member
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
    #endregion

    public struct Node
    {
        public int[] Pfad;
        public int[] Children;
        
    }

    public struct Survivor
    {
        public byte[] key;
        public int[] type;
        public string rev;
    }

}
