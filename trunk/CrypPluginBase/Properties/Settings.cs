using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace Cryptool.PluginBase.Properties
{
    public sealed partial class Settings
    {
        [UserScopedSetting()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        public ArrayList Wizard_Storage
        {
            get
            {
                return ((ArrayList)(this["Wizard_Storage"]));
            }
            set
            {
                this["Wizard_Storage"] = value;
            }
        }
    }
}
