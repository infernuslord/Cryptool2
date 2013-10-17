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
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

using Cryptool.PluginBase;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase.Miscellaneous;
using SigabaBruteforce.Cryptool.PluginBase.Control;


namespace SigabaBruteforce
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Julian Weyers", "weyers@cryptool.org", "CrypTool 2 Team", "http://cryptool2.vs.uni-due.de")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("Sigaba Bruteforcer", "Subtract one number from another", "SigabaBruteforce/userdoc.xml",
        new[] {"CrypWin/images/default.png"})]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class SigabaBruteforce : ICrypComponent
    {
      
        private Boolean stop;


          #region Constructor 
        public SigabaBruteforce()
        {

            SigabaBruteforceQuickWatchPresentation sigpa = new SigabaBruteforceQuickWatchPresentation();
            Presentation = sigpa;
            sigpa.doppelClick += new EventHandler(this.doppelClick);
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
        public string Output
        {
            get; 
            set;
        }

        #endregion

        #region IPlugin Members

        private IControlSigabaEncryption controlMaster;

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

        private IControlCost costMaster;
        [PropertyInfo(Direction.ControlMaster, "CostMasterCaption", "CostMasterTooltip", false)]
        public IControlCost CostMaster
        {
            get { return costMaster; }
            set
            {
                costMaster = value;
            }
        }


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
        public UserControl Presentation
        {
            get;
            private set;
        }

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
            // HOWTO: Use this to show the progress of a plugin algorithm execution in the editor.
            ProgressChanged(0, 1);

            // HOWTO: After you have changed an output property, make sure you announce the name of the changed property to the CT2 core.
            //SomeOutput = SomeInput - settings.SomeParameter;

            //controlMaster.changeSettings();

            

            //SomeOutput = controlMaster.Decrypt(SomeInput);

            controlMaster.changeSettings("CipherKey", "AAAAA");
            controlMaster.changeSettings("ControlKey", "AAAAA");
            controlMaster.changeSettings("IndexKey", "00000");

            controlMaster.setInternalConfig();

            //bruteforceAlphabetMaze();

            bruteforceSteppingMaze();

            //SomeOutput = controlMaster.postFormatOutput(controlMaster.Decrypt(controlMaster.preFormatInput(SomeInput)));
            /*
            SomeOutput =
                controlMaster.postFormatOutput(
                    controlMaster.Decrypt(controlMaster.preFormatInput(SomeInput)));
            OnPropertyChanged("SomeOutput");
            */

            // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
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
        
        private Queue valuequeue;
        LinkedList<ValueKey> list1;

        private void bruteforceAlphabetMaze()
        {
            DateTime lastUpdate = DateTime.Now;
            DateTime starttime = DateTime.Now;

            double best = Double.MinValue;
            
            if (costMaster.GetRelationOperator() == RelationOperator.LessThen)
            {
                best = Double.MaxValue;
            }

            list1 = getDummyLinkedList(best);
            valuequeue = Queue.Synchronized(new Queue());

            
            long isummax = (_settings.CipherRotor1To + 1 - _settings.CipherRotor1From) * (_settings.CipherRotor2To + 1 - _settings.CipherRotor2From) * (_settings.CipherRotor3To + 1 - _settings.CipherRotor3From) * (_settings.CipherRotor4To + 1 - _settings.CipherRotor4From) * (_settings.CipherRotor5To + 1 - _settings.CipherRotor5From);

            long icount = 0;

            for (int i1 = _settings.CipherRotor1From; i1 < _settings.CipherRotor1To + 1; i1++)
            {
                for (int i2 = _settings.CipherRotor2From; i2 < _settings.CipherRotor2To + 1; i2++)
                {
                    for (int i3 = _settings.CipherRotor3From; i3 < _settings.CipherRotor3To + 1; i3++)
                    {
                        for (int i4 = _settings.CipherRotor4From; i4 < _settings.CipherRotor4To + 1; i4++)
                        {
                            for (int i5 = _settings.CipherRotor5From; i5 < _settings.CipherRotor5To + 1; i5++)
                            {

                                controlMaster.changeSettings("ControlKey", "AAAAA");

                                controlMaster.changeSettings("CipherKey", ((char)(i1 + 65)) + "" + ((char)(i2 + 65)) + "" + ((char)(i3 + 65)) + "" + ((char)(i4 + 65)) + "" + ((char)(i5 + 65)));

                                controlMaster.setInternalConfig();

                                string key = ((char)(i1 + 65)) + "" + ((char)(i2 + 65)) + "" + ((char)(i3 + 65)) + "" + ((char)(i4 + 65)) + "" + ((char)(i5 + 65));
                               

                                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                                byte[] s =
                                    enc.GetBytes(controlMaster.postFormatOutput(
                                        controlMaster.Decrypt(controlMaster.preFormatInput(Input))));

                                

                                double val = costMaster.CalculateCost(s);

                                if (costMaster.GetRelationOperator() == RelationOperator.LessThen)
                                {
                                    if (val <= best)
                                    {
                                        ValueKey valkey = new ValueKey();
                                        String keyStr = "";
                                        
                                        valkey.decryption = s;
                                        valkey.key = key;
                                        valkey.value = val;
                                        valuequeue.Enqueue(valkey);
                                    }
                                }
                                else
                                {
                                    if (val >= best)
                                    {
                                        ValueKey valkey = new ValueKey();
                                        String keyStr = "";
                                        
                                        valkey.decryption = s;
                                        valkey.key = key;
                                        valkey.value = val;
                                        valuequeue.Enqueue(valkey);
                                    }
                                }

                                if(lastUpdate.AddMilliseconds(1000) <  DateTime.Now)
                                {
                                    UpdatePresentationList(isummax, icount, starttime);
                                    lastUpdate = DateTime.Now;
                                }

                                icount++;
                                
                                if(stop)
                                {
                                    return;
                                    
                                }

                            }
                        }
                    }
                }
            }

            UpdatePresentationList(isummax, icount, starttime);
        }

        private void bruteforceSteppingMaze()
        {
            DateTime lastUpdate = DateTime.Now;
            DateTime starttime = DateTime.Now;

            double best = Double.MinValue;

            if (costMaster.GetRelationOperator() == RelationOperator.LessThen)
            {
                best = Double.MaxValue;
            }

            list1 = getDummyLinkedList(best);
            valuequeue = Queue.Synchronized(new Queue());


            long isummax = (_settings.ControlRotor1To + 1 - _settings.ControlRotor1From) * (_settings.ControlRotor2To + 1 - _settings.ControlRotor2From) * (_settings.ControlRotor3To + 1 - _settings.ControlRotor3From) * (_settings.ControlRotor4To + 1 - _settings.ControlRotor4From) * (_settings.ControlRotor5To + 1 - _settings.ControlRotor5From);

            long icount = 0;

            for (int i1 = _settings.ControlRotor1From; i1 < _settings.ControlRotor1To + 1; i1++)
            {
                for (int i2 = _settings.ControlRotor2From; i2 < _settings.ControlRotor2To + 1; i2++)
                {
                    for (int i3 = _settings.ControlRotor3From; i3 < _settings.ControlRotor3To + 1; i3++)
                    {
                        for (int i4 = _settings.ControlRotor4From; i4 < _settings.ControlRotor4To + 1; i4++)
                        {
                            for (int i5 = _settings.ControlRotor5From; i5 < _settings.ControlRotor5To + 1; i5++)
                            {

                                controlMaster.changeSettings("CipherKey", "AAAAA");

                                controlMaster.changeSettings("ControlKey", ((char)(i1 + 65)) + "" + ((char)(i2 + 65)) + "" + ((char)(i3 + 65)) + "" + ((char)(i4 + 65)) + "" + ((char)(i5 + 65)));

                                controlMaster.setInternalConfig();

                                string key = ((char)(i1 + 65)) + "" + ((char)(i2 + 65)) + "" + ((char)(i3 + 65)) + "" + ((char)(i4 + 65)) + "" + ((char)(i5 + 65));


                                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                                byte[] s =
                                    enc.GetBytes(controlMaster.postFormatOutput(
                                        controlMaster.Decrypt(controlMaster.preFormatInput(Input))));



                                double val = costMaster.CalculateCost(s);

                                if (costMaster.GetRelationOperator() == RelationOperator.LessThen)
                                {
                                    if (val <= best)
                                    {
                                        ValueKey valkey = new ValueKey();
                                        String keyStr = "";

                                        valkey.decryption = s;
                                        valkey.key = key;
                                        valkey.value = val;
                                        valuequeue.Enqueue(valkey);
                                    }
                                }
                                else
                                {
                                    if (val >= best)
                                    {
                                        ValueKey valkey = new ValueKey();
                                        String keyStr = "";

                                        valkey.decryption = s;
                                        valkey.key = key;
                                        valkey.value = val;
                                        valuequeue.Enqueue(valkey);
                                    }
                                }

                                if (lastUpdate.AddMilliseconds(1000) < DateTime.Now)
                                {
                                    UpdatePresentationList(isummax, icount, starttime);
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

            UpdatePresentationList(isummax, icount, starttime);
        }

        private void doppelClick(object sender, EventArgs e)
        {
            ListViewItem lvi = sender as ListViewItem;
            ResultEntry rse = lvi.Content as ResultEntry;
            Output = rse.Text;
            OnPropertyChanged("Output");
        }

        private void keyFactory(string plain)
        {
            DateTime starttime = DateTime.Now;
            DateTime lastUpdate = DateTime.Now;
            
            
            double best = list1.Last.Value.value;
            int[] key = { };

            

            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();


            byte[] dec = enc.GetBytes(plain);


            if (dec != null)
            {
                double val = costMaster.CalculateCost(dec);
                
                if (costMaster.GetRelationOperator() == RelationOperator.LessThen)
                {
                    if (val <= best)
                    {
                        ValueKey valkey = new ValueKey();
                        String keyStr = "";
                        foreach (int xyz in key)
                        {
                            keyStr += xyz + ", ";
                        }
                        valkey.decryption = dec;
                        valkey.key = keyStr;
                        valkey.value = val;
                        valuequeue.Enqueue(valkey);
                    }
                }
                else
                {
                    if (val >= best)
                    {
                        ValueKey valkey = new ValueKey();
                        String keyStr = "";
                        foreach (int xyz in key)
                        {
                            keyStr += xyz;
                        }
                        valkey.decryption = dec;
                        valkey.key = keyStr;
                        valkey.value = val;
                        valuequeue.Enqueue(valkey);
                    }
                }
            }


            if (DateTime.Now >= lastUpdate.AddMilliseconds(1000))
            {
                lastUpdate = UpdatePresentationList(0, 1, starttime);
            }
        }

        private LinkedList<ValueKey> getDummyLinkedList(double best)
        {
            ValueKey valueKey = new ValueKey();
            valueKey.value = best;
            valueKey.key = "dummykey";
            valueKey.decryption = new byte[0];
            LinkedList<ValueKey> list = new LinkedList<ValueKey>();
            LinkedListNode<ValueKey> node = list.AddFirst(valueKey);
            for (int i = 0; i < 9; i++)
            {
                node = list.AddAfter(node, valueKey);
            }
            return list;
        }

        private DateTime UpdatePresentationList(long size, long sum, DateTime starttime)
        {
            DateTime lastUpdate;
            updateToplist(list1);
            showProgress(starttime, size, sum);
            ProgressChanged(sum, size);
            lastUpdate = DateTime.Now;
            return lastUpdate;
        }

        private void showProgress(DateTime startTime, long size, long sum)
        {
            LinkedListNode<ValueKey> linkedListNode;
            if (Presentation.IsVisible )
            {
                DateTime currentTime = DateTime.Now;

                TimeSpan elapsedtime = DateTime.Now.Subtract(startTime); ;
                TimeSpan elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);



                TimeSpan span = currentTime.Subtract(startTime);
                int seconds = span.Seconds;
                int minutes = span.Minutes;
                int hours = span.Hours;
                int days = span.Days;

                long allseconds = seconds + 60 * minutes + 60 * 60 * hours + 24 * 60 * 60 * days;
                if (allseconds == 0) allseconds = 1;

                if (allseconds == 0)
                    allseconds = 1;

                long keysPerSec = sum / allseconds;

                long keystodo = (size - sum);


                if (keysPerSec == 0)
                    keysPerSec = 1;

                long secstodo = keystodo / keysPerSec;

                //dummy Time 
                DateTime endTime = new DateTime(1970, 1, 1);
                try
                {
                    endTime = DateTime.Now.AddSeconds(secstodo);
                }
                catch
                {

                }


                ((SigabaBruteforceQuickWatchPresentation)Presentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {

                    ((SigabaBruteforceQuickWatchPresentation)Presentation).startTime.Content = "" + startTime;
                    ((SigabaBruteforceQuickWatchPresentation)Presentation).keysPerSecond.Content = "" + keysPerSec;


                    if (endTime != (new DateTime(1970, 1, 1)))
                    {
                        ((SigabaBruteforceQuickWatchPresentation)Presentation).timeLeft.Content = "" + endTime.Subtract(DateTime.Now);
                        ((SigabaBruteforceQuickWatchPresentation)Presentation).elapsedTime.Content = "" + elapsedspan;
                        ((SigabaBruteforceQuickWatchPresentation)Presentation).endTime.Content = "" + endTime;
                    }
                    else
                    {
                        ((SigabaBruteforceQuickWatchPresentation)Presentation).timeLeft.Content = "incalculable";

                        ((SigabaBruteforceQuickWatchPresentation)Presentation).endTime.Content = "in a galaxy far, far away...";
                    }
                    if (list1 != null)
                    {
                        linkedListNode = list1.First;
                        ((SigabaBruteforceQuickWatchPresentation)Presentation).entries.Clear();
                        int i = 0;
                        while (linkedListNode != null)
                        {
                            i++;
                            ResultEntry entry = new ResultEntry();
                            entry.Ranking = i.ToString();


                            String dec = System.Text.Encoding.ASCII.GetString(linkedListNode.Value.decryption);
                            if (dec.Length > 2500) // Short strings need not to be cut off
                            {
                                dec = dec.Substring(0, 2500);
                            }
                            entry.Text = dec;
                            entry.Key = linkedListNode.Value.key;
                            entry.Value = Math.Round(linkedListNode.Value.value, 2) + "";


                            ((SigabaBruteforceQuickWatchPresentation)Presentation).entries.Add(entry);

                            linkedListNode = linkedListNode.Next;
                        }

                    }
                }
                , null);

            }
        }

        private void updateToplist(LinkedList<ValueKey> costList)
        {
            LinkedListNode<ValueKey> node;

            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            while (valuequeue.Count != 0)
            {
                ValueKey vk = (ValueKey)valuequeue.Dequeue();
                if (this.costMaster.GetRelationOperator() == RelationOperator.LargerThen)
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
                                   Output =  enc.GetString(vk.decryption);
                                }
                                // value_threshold = costList.Last.Value.value;
                                break;
                            }
                            node = node.Next;
                            i++;
                        }//end while
                    }//end if
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
                        }//end while
                    }//end if
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
        public byte[] keyArray;
        public double value;
        public String key;
        public byte[] decryption;
    };
    public class ResultEntry
    {
        public string Ranking { get; set; }
        public string Value { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }

    }


namespace Cryptool.PluginBase.Control
{
    public interface IControlSigabaEncryption : IControl, IDisposable
    {

        string Decrypt(string ciphertext);
        void setInternalConfig();
        void changeSettings(string setting, object value);
        
        string preFormatInput(string text);
        string postFormatOutput(string text);
    }
}
}
