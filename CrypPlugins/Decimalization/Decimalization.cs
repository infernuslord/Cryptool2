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
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Numerics;

namespace Cryptool.Plugins.Decimalization
{
    [Author("Andreas Grüner", "agruener@informatik.hu-berlin.de", "Humboldt University Berlin", "http://www.hu-berlin.de")]
    [PluginInfo("Decimalization.Properties.Resources", "PluginCaption", "PluginToolTip", "Decimalization/DetailedDescription/doc.xml", "Decimalization/Decimalization.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class Decimalization : ICrypComponent
    {
        #region Data Class

        private class Result
        {
            private String sres;
            private int[] ires;

            public String Sres
            {
                get { return sres; }
                set { sres = value; }
            }

            public int[] Ires
            {
                get { return ires; }
                set { ires = value; }
            }
        }

        #endregion

        #region Private Variables

        private readonly DecimalizationSettings settings = new DecimalizationSettings();

        #endregion

        #region Data Properties
        
        [PropertyInfo(Direction.InputData, "InputCaption", "InputCaptionToolTip")]
        public byte[] BinaryNumber
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "Output1Caption", "Output1CaptionToolTip")]
        public int[] DecimalNumberInt
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "Output2Caption", "Output2CaptionToolTip")]
        public String DecimalNumberStr
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
            Result res = null;
           

            ProgressChanged(0, 1);

            switch (settings.Mode)
            {
                case 0:
                    res = ProcessVisaMethod();
                    break;
                case 1:
                    res = ProcessModuloMethod();
                    break;
                case 2:
                    res = ProcessMultMethod();
                    break;
                case 3:
                    res = ProcessIBMMethod();
                    break;
                default:
                    GuiLogMessage("Unknown Decimalization Mode", NotificationLevel.Error);
                    break;
            }

            DecimalNumberInt = res.Ires;
            DecimalNumberStr = res.Sres;
            
            OnPropertyChanged("DecimalNumberInt");
            OnPropertyChanged("DecimalNumberStr");

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
            settings.Initialize();
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

        private Result ProcessVisaMethod()
        {
            Result res = new Result();
            // Lists to save numbers
            List<uint> listhex = new List<uint>();
            List<uint> listhexgr9 = new List<uint>();
            List<int> listres = new List<int>();
            // Arrays to iterate
            uint[] hex;
            uint[] hexgr9;
            int[] resar = new int[settings.Quant];
            // Helper variables
            uint t = 0;
            int j = 0;

            // Create hexnumbers from binary data
            for (int i = 0; i < BinaryNumber.Length; i++)
            {
                t = BinaryNumber[i];
                t = t >> 4;
                listhex.Add(t);
                t = BinaryNumber[i];
                t = t << 28;
                t = t >> 28;
                listhex.Add(t);
            }

            // Copy list to array
            hex = new uint[listhex.Count];
            listhex.CopyTo(hex, 0);

            // Fill result list with hexnumbers smaller 10 
            for (int i = 0; i < settings.Quant; i++)
            {
                if ((j < (hex.Length)) && (hex[j] < 10))
                {
                    listres.Add((int)hex[j]);
                    j++;
                }
                else if ((j < (hex.Length)) && (hex[j] > 9))
                {
                    while ((j < (hex.Length)) && (hex[j] > 9))
                    {
                        listhexgr9.Add(hex[j]);
                        j++;
                    }
                }
                else
                {
                    break;
                }
            }

            // Fill rest of result with numbers greater 9 subtracted by 10
            if (listres.Count < settings.Quant)
            {
                hexgr9 = new uint[listhexgr9.Count];
                listhexgr9.CopyTo(hexgr9, 0);
                j = 0;
                while (listres.Count < settings.Quant)
                {
                    if (j > hexgr9.Length - 1)
                    {
                         GuiLogMessage("Too less random input data for requested quantity of decimals.", NotificationLevel.Warning);
                         break;
                    }
                    listres.Add((int)(hexgr9[j]-10));
                    j++;
                }
            }
            
            listres.CopyTo(resar);
            res.Ires = resar;
            res.Sres = intArrayToString(resar);

            return res;
        }

        private Result ProcessModuloMethod()
        {
            Result res = new Result();
            byte[] binar = new byte[BinaryNumber.Length + 1];
            BigInteger divisor = new BigInteger();
            BigInteger dividend = new BigInteger();
            BigInteger bigres = new BigInteger();

            try
            {
                divisor = new BigInteger(Math.Pow(10, settings.Quant));
            }
            catch (OverflowException e)
            {
                GuiLogMessage("Overflow Exception: Numbers to Big. Try again with smaller numbers", NotificationLevel.Error);
            }

            BinaryNumber.CopyTo(binar,0);
            binar[binar.Length-1] = 0x00;
            dividend = new BigInteger(binar);
            // Execute Modulo Operation
            bigres = BigInteger.Remainder(dividend, divisor);

            // BigInteger to Int[]
            res.Ires = bigIntegerToIntArray(bigres);
            res.Sres = bigres.ToString();

            return res;
        }

        private Result ProcessMultMethod()
        {
            Result res = new Result();
            BigInteger z, div = new BigInteger(), factor = new BigInteger(), bres;
            byte[]  zbyte = new byte[BinaryNumber.Length+1];

            // Calculate Result
            BinaryNumber.CopyTo(zbyte, 0);
            zbyte[zbyte.Length - 1] = 0x00;
            z = new BigInteger(zbyte);

            try
            {
                factor = new BigInteger(Math.Pow(10, settings.Quant));
                div = new BigInteger(Math.Pow(2, 8 * BinaryNumber.Length));
            }
            catch (OverflowException e)
            {
                GuiLogMessage("Overflow Exception: Numbers to Big. Try again with smaller numbers.", NotificationLevel.Error);
            }
            bres = BigInteger.Multiply(z, factor);

            try
            {
                bres = BigInteger.Divide(bres, div);
            }
            catch (DivideByZeroException e)
            {
                GuiLogMessage("Overflow Exception: Numbers to Big. Try again with smaller numbers.", NotificationLevel.Error);
            }

            // Write Result to Return Structure
            res.Ires = bigIntegerToIntArray(bres);
            res.Sres = bres.ToString();

            return res;
        }

        private Result ProcessIBMMethod()
        {
            Result res = new Result();
            uint t = 0;
            List<uint> listhex = new List<uint>();
            uint[] arhex;
            int[] arres = new int[settings.Quant];
            int[] assocTable = { 0, 0, 0, 0, 0, 0 };
            int j = 0;

            // Build Associtation Table for HexNumbers > 9
            assocTable[0] = settings.IbmA;
            assocTable[1] = settings.IbmB;
            assocTable[2] = settings.IbmC;
            assocTable[3] = settings.IbmD;
            assocTable[4] = settings.IbmE;
            assocTable[5] = settings.IbmF;

            // Create hexnumbers from binary data
            for (int i = 0; i < BinaryNumber.Length; i++)
            {
                t = BinaryNumber[i];
                t = t >> 4;
                listhex.Add(t);
                t = BinaryNumber[i];
                t = t << 28;
                t = t >> 28;
                listhex.Add(t);
            }

            arhex = new uint[listhex.Count];
            listhex.CopyTo(arhex, 0);

            // Fill Result Array
            while ((j < arres.Length) && (j < arhex.Length))
            {
                if (arhex[j] < 10)
                {
                    arres[j] = (int)arhex[j];
                }
                else
                {
                    arres[j] = assocTable[arhex[j] - 10];
                }
                j++;
            }

            if (j < arres.Length)
            {
                GuiLogMessage("Too less random input data for requested quantity of decimals.", NotificationLevel.Warning);
            }

            res.Ires = arres;
            res.Sres = intArrayToString(arres);

            return res;
        }

        private int[] bigIntegerToIntArray(BigInteger nr)
        {
            List<int> nrlist = new List<int>();
            int[] nrar;
            String s = nr.ToString();

            for (int i = 0; i < s.Length; i++)
            {
                nrlist.Add(Convert.ToInt32(s[i])-48);
            }
            nrar = new int[nrlist.Count];
            
            nrlist.CopyTo(nrar, 0);

            return nrar;
        }

        private String intArrayToString(int[] nrs)
        {
            String res = "";

            for (int i = 0; i < nrs.Length; i++)
            {
                res += nrs[i].ToString();
            }

            return res;
        }

        #endregion
    }
}
