using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Cryptool.CrypWin.Properties
{
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
    {
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        public global::System.Collections.ArrayList DisabledPlugins
        {
            get
            {
                return ((global::System.Collections.ArrayList)(this["DisabledPlugins"]));
            }
            set
            {
                this["DisabledPlugins"] = value;
            }
        }
    }
}
