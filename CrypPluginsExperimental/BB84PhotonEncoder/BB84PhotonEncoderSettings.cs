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
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.BB84PhotonEncoder
{
    public class BB84PhotonEncoderSettings : ISettings
    {
        #region Private Variables

        private int plusZeroEncoding;
        private int plusOneEncoding;
        private int xZeroEncoding;
        private int xOneEncoding;
        private double durationSetting;



        #endregion


        #region TaskPane Settings

        [TaskPane("Bit \"0\", Base \"+\": ", "Change Mode for Encoding on PLUS-Base", null, 1, false, ControlType.ComboBox, new string[] { "Vertical", "Horizontal" })]
        public int PlusZeroEncoding
        {
            get
            {
                return plusZeroEncoding;
            }
            set
            {
                if (plusZeroEncoding != value)
                {
                    if (value == 0)
                    {
                        this.plusZeroEncoding = 0;
                        this.PlusOneEncoding = 1;
                        OnPropertyChanged("PlusOneEncoding");
                    }
                    else
                    {
                        this.plusZeroEncoding = 1;
                        this.plusOneEncoding = 0;
                        OnPropertyChanged("PlusOneEncoding");
                    }                
                }
            }
        }
        [TaskPane("Bit \"1\", Base \"+\": ", "Change Mode for Encoding on PLUS-Base", null, 2, false, ControlType.ComboBox, new string[] { "Vertical", "Horizontal" })]
        public int PlusOneEncoding
        {
            get
            {
                return plusOneEncoding;
            }
            set
            {
                if (plusOneEncoding != value)
                {
                    if (value==0)
                    {
                        this.plusOneEncoding = 0;
                        this.plusZeroEncoding = 1;
                        OnPropertyChanged("PlusZeroEncoding");
                    }
                    else
                    {
                        this.plusOneEncoding = 1;
                        this.plusZeroEncoding = 0;
                        OnPropertyChanged("PlusZeroEncoding");
                    }
                }
            }
        }
        [TaskPane("Bit \"0\", Base \"x\": ", "Change Mode for Encoding on EX-Base", null, 3, false, ControlType.ComboBox, new string[] { "\\", "/" })]
        public int XZeroEncoding
        {
            get
            {
                return xZeroEncoding;
            }
            set
            {
                if (xZeroEncoding != value)
                {
                    if (value==0)
                    {
                        this.xZeroEncoding = 0;
                        this.xOneEncoding = 1;
                        OnPropertyChanged("XOneEncoding");
                    }
                    else
                    {
                        this.xZeroEncoding = 1;
                        this.xOneEncoding = 0;
                        OnPropertyChanged("XOneEncoding");
                    }                 
                }
            }
        }
        [TaskPane("Bit \"1\", Base \"x\": ", "Change Mode for Encoding on EX-Base", null, 4, false, ControlType.ComboBox, new string[] { "\\", "/" })]
        public int XOneEncoding
        {
            get
            {
                return xOneEncoding;
            }
            set
            {
                if (xOneEncoding != value)
                {
                    if (value==0)
                    {
                        this.xOneEncoding = 0;
                        this.xZeroEncoding = 1;
                        OnPropertyChanged("XZeroEncoding");
                    }
                    else
                    {
                        this.xOneEncoding = 1;
                        this.xZeroEncoding = 0;
                        OnPropertyChanged("XZeroEncoding");
                    }
                }
            }
        }

        [TaskPane("Animation Speed", "Sets the Speed of Presentation-Animation", null, 5, false, ControlType.Slider, 0.5, 2.0)]
        public double SpeedSetting
        {
            get
            {
                return durationSetting;
            }
            set
            {
                if (durationSetting != (value))
                {
                    durationSetting = (value);
                    OnPropertyChanged("DurationSetting");
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
