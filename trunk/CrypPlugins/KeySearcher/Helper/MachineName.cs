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
        
        private static string realMachineName = Cryptool.PluginBase.Miscellaneous.UniqueIdentifier.GetHostName();
        private static long id = Cryptool.PluginBase.Miscellaneous.UniqueIdentifier.GetID();

        public static string MachineNameToUse
        {
            get;
            private set;
        }

        static MachineName()
        {
            MachineNameToUse = GenerateMachineNameToUse();
            Settings.Default.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Default_PropertyChanged);
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
            if (!Settings.Default.Anonymize)
            {
                return realMachineName;
            }
            else
            {
                return String.Format("{0}_{1:X}", realMachineName.Substring(0, Settings.Default.MachNameChars), id);
            }
        }
    }
}
