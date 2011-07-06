using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;

namespace PKCS1
{
    class PKCS1Settings : ISettings
    {
        private bool hasChanges = false;

        bool ISettings.HasChanges
        {
            get
            {
                return this.hasChanges;
            }
            set
            {
                this.hasChanges = value;
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
