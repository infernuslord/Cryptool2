using System;

namespace Cryptool.PluginBase.Attributes
{
    public enum Ct2BuildType
    {
        Developer = 0,
        Nightly = 1,
        Beta = 2,
        Stable = 3
    }

    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyCt2BuildTypeAttribute : Attribute
    {
        public Ct2BuildType BuildType
        {
            get; set;
        }

        public AssemblyCt2BuildTypeAttribute(Ct2BuildType type)
        {
            this.BuildType = type;
        }

        public AssemblyCt2BuildTypeAttribute(int type)
        {
            this.BuildType = (Ct2BuildType) type;
        }
    }
}
