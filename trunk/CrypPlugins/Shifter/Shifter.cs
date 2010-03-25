/*
   Copyright 2008 Dr. Arno Wacker, University of Duisburg-Essen

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
using Cryptool.PluginBase.IO;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;
using System.Runtime.CompilerServices;

namespace Cryptool.Plugins.Shifter
{
    class Shifter : IThroughput
    {
        [Author("Raoul Falk", "falk@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
        [PluginInfo(false, "Shifter", "Shift operator with adjustable block size.", "", "Shifter/icons/left.png", "Shifter/icons/right.png")]

        #region private variables

        private object inputOne;
        private object inputTwo;
        private object output;
        private ShifterSettings settings = new ShifterSettings();

        #region public interfaces

        public Shifter()
        {
            this.settings = new ShifterSettings();
            this.settings.OnPluginStatusChanged += settings_OnPluginStatusChanged;
        }



        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

    public event PluginProgressChangedEventHandler OnPluginProgressChanged;

    public ISettings Settings
    {
        get { return this.settings; }
        set { this.settings = (ShifterSettings)value; }
    }

    public System.Windows.Controls.UserControl Presentation
    {
        get { return null; }
    }

    public System.Windows.Controls.UserControl QuickWatchPresentation
    {
        get { return null; }
    }

    [PropertyInfo(Direction.InputData, "Input one", "Input one.", "", true, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public object InputOne
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get { return inputOne; }
        [MethodImpl(MethodImplOptions.Synchronized)]
        set
        {
            if (value != inputOne)
            {
                inputOne = value;
                OnPropertyChanged("InputOne");
            }
        }
    }
    [PropertyInfo(Direction.InputData, "Input two", "Input two.", "", true, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public object InputTwo
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get { return inputTwo; }
        [MethodImpl(MethodImplOptions.Synchronized)]
        set
        {
            if (value != inputTwo)
            {
                inputTwo = value;
                OnPropertyChanged("InputTwo");
            }
        }
    }

    [PropertyInfo(Direction.OutputData, "Output", "Output.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public object Output
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            return output;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        set
        {
            this.output = value;
            OnPropertyChanged("Output");
        }
    }

    public void PreExecution()
    {
        throw new NotImplementedException();
    }

    public void Execute()
    {
        switch (settings.Operand)
        {
            case 0: //left
                {

                    break;
                }
            case 1: //right
                {
                    break;
                }
        }
    }

    public void PostExecution()
    {
        throw new NotImplementedException();
    }

    public void Pause()
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    public void Initialize()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    protected void OnPropertyChanged(string name)
    {
        EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
    }
    void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
    {
        if (OnPluginStatusChanged != null) OnPluginStatusChanged(this, args);
    }

        #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
    }
}
