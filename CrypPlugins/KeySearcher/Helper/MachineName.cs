using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows; 
using KeySearcher.Properties;

namespace KeySearcher.Helper
{
    public static class MachineName
    {
        public delegate void OnMachineNameToUseChangedHandler(string newMachineNameToUse);
        public static event OnMachineNameToUseChangedHandler OnMachineNameToUseChanged;
        
        private static string realMachineName = "";
        private static long id = 0;

        public static string MachineNameToUse
        {
            get;
            private set;
        }

        static MachineName()
        {
            MachineNameToUse = GenerateMachineNameToUse();
            Cryptool.PluginBase.Properties.Settings.Default.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Default_PropertyChanged);
        }

        private static void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == "Anonymize") || (e.PropertyName == "MachNameChars"))
            {
                if (OnMachineNameToUseChanged != null)
                {
                    MachineNameToUse = GenerateMachineNameToUse();
                    OnMachineNameToUseChanged(MachineNameToUse);
                }
            }
        }

        private static string GenerateMachineNameToUse()
        {            
            if (!Cryptool.PluginBase.Properties.Settings.Default.KeySearcher_Anonymize)
            {
                return realMachineName;
            }
            else
            {
                return String.Format("{0}_{1:X}", realMachineName.Substring(0, Cryptool.PluginBase.Properties.Settings.Default.KeySearcher_MachNameChars), id);
            }
        }
    }
}
