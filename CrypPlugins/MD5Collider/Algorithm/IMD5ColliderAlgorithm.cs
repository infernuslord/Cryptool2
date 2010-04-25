using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Cryptool.Plugins.MD5Collider.Algorithm
{
    public interface IMD5ColliderAlgorithm : INotifyPropertyChanged
    {
        byte[] FirstCollidingData { get; }
        byte[] SecondCollidingData { get; }
        byte[] RandomSeed { set; }
        byte[] IHV { set; }
        long CombinationsTried { get; }

        TimeSpan ElapsedTime { get; }

        void FindCollision();

        void Stop();

        int MatchProgressMax { get; }
        int MatchProgress { get; }
    }
}
