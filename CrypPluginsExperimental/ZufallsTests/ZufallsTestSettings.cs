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
using System.Numerics;
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.ZufallsTests
{
    // HOWTO: rename class (click name, press F2)
    public class ZufallsTestSettings : ISettings
    {

        #region Private Variables

        public void Initialize()
        {

        }

        private BigInteger dataInput = 0;

        #endregion

        #region Properties

        public enum TestNumber : int { Birthday = 0, OPERM5 = 1, BinaryRank32 = 2, BinaryRank6x8 = 3, Bitstream = 4, DNATest = 5, CountOnesStream = 6, CountOnesByte = 7, ParkingLot = 8, MinimumDistance = 9, Sphere3D = 10, Squeeze = 11, Sums = 12, Runs = 13, Craps = 14, MarsagliaTsang = 15, STSMonobit = 100, STSRuns = 101, STSSerial = 102, RGBBitDistribution = 200, RGBMinimumDistance = 201, RGBPermutations = 202, RGBLaggedSum = 203, RGBKolmogorovSmirnov = 204, ByteDistribution = 205, DABDCT = 206, DABFillTree = 207, DABFillTree2 = 208, DABMonobit2 = 209, DiehardBirthdaysTest = 210, ALLTests = 999 };

        private TestNumber selectedTest = TestNumber.Birthday;

        [PropertySaveOrder(1)]
        [TaskPane("Selected test", "Run all the tests or just one specific", null, 1, false, ControlType.ComboBox, new string[] { "Diehard Birthdays Test ", "Diehard OPERM5 Test ", "Diehard 32x32 Binary Rank Test ", "Diehard 6x8 Binary Rank Test", "Diehard Bitstream Test", "Diehard DNA Test", "Diehard Count the 1s (stream) Test", "Diehard Count the 1s Test (byte)", "Diehard Parking Lot Test", "Diehard Minimum Distance (2d Circle) Test", "Diehard 3d Sphere (Minimum Distance) Test", "Diehard Squeeze Test", "Diehard Sums Test", "Diehard Runs Test", "Diehard Craps Test", "Marsaglia and Tsang GCD Test", "STS Monobit Test", "STS Runs Test", "STS Serial Test (Generalized) ", "RGB Bit Distribution Test ", "RGB Generalized Minimum Distance Test", "RGB Permutations Test", "RGB Lagged Sum Test", "RGB Kolmogorov-Smirnov Test Test ", "Byte Distribution", "DAB DCT ", "DAB Fill Tree Test", "DAB Fill Tree 2 Test", "DAB Monobit 2 Test", "Diehard Birthdays Test", "Execute all tests" })]
        public TestNumber SelectedTest
        {
            get
            {
                return this.selectedTest;
            }
            set
            {
                if (value != selectedTest)
                {
                    this.selectedTest = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        //private int fasterF = 1;

        /*
        [PropertySaveOrder(2)]
        [TaskPane("FasterFactor", "To enshorten the amount of data and time needed for test execution this fasterFactor can be used at cost of accurancy!", null, 1, false, ControlType.ComboBox, new string[] { "1 (Test will be executed on the specified amount of data according to the dieharder specification) ", "2 (The test will be executed on the half of data)", "3", "4", "5"})]
        public int FasterFactor
        {
            get
            {
                return this.fasterF;
            }
            set
            {
                if (value != fasterF)
                {
                    this.fasterF = value;
                    OnPropertyChanged("Action");
                }
            }
        }
         */

        private int nTuple = 1;

        [PropertySaveOrder(3)]
        [TaskPane("NTuple", "Some tests need a user input which determines the ntuples or dimensions", null, 1, false, ControlType.NumericUpDown, Cryptool.PluginBase.ValidationType.RangeInteger, 1, 5)]
        public int NTuple
        {
            get
            {
                return this.nTuple;
            }
            set
            {
                if (value != nTuple)
                {
                    this.nTuple = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
