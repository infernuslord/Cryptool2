using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;

namespace Gate
{
    enum Trigger
    {
        AlwaysOpen, AlwaysClosed, TrueValue, FalseValue, AnyEdge, PositiveEdge, NegativeEdge
    };

    class GateSettings : ISettings
    {
        private bool hasChanges = false;
        private Trigger trigger = 0;

        [TaskPane("Trigger", "Trigger to open gate", null, 1, true, DisplayLevel.Experienced, ControlType.RadioButton,
            new string[] { "no trigger (always open)", "no trigger (always closed)", "true value", "false value", "edge (value swap)", "positive edge (false->true)", "negative edge (true->false)" })]
        public Trigger Trigger
        {
            get
            {
                return trigger;
            }
            set
            {
                if (trigger != value)
                    hasChanges = true;

                trigger = value;
            }
        }

        #region ISettings Members

        public bool HasChanges
        {
            get
            {
                return hasChanges;
            }
            set
            {
                hasChanges = value;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
