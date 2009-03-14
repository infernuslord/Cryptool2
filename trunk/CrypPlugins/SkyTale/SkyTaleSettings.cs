using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;

namespace Cryptool.SkyTale
{
	class SkyTaleSettings : ISettings
	{
	    private string charsPerTurn;
	    private string charsPerColumn;

        public bool HasChanges
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;


    }
}
