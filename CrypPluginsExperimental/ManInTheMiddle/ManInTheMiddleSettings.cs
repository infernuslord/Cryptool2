﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using System.ComponentModel;

namespace ManInTheMiddle
{

  
    class ManInTheMiddleSettings:ISettings
    {
        private bool send;

        [TaskPane("Insert own SOAP-Body", "If checked the plugin will send the output message on play", null, 0, false, ControlType.CheckBox)]
        public bool insertBody
        {
            get { return send; }
            set
            {
                send = value;
                OnPropertyChanged("insertBody");
                HasChanges = true;
            }
        }

        #region variables

        private string soap;
        public string Soap
        {
            get
            {

                return soap;
            }
            set
            {
                soap = value;
                OnPropertyChanged("Soap");
            }
        }

        #endregion


        #region ISettings Member

        private bool changes;
        public bool HasChanges
        {
            get
            {
              return changes;
            }
            set
            {
                changes=value;
            }
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}