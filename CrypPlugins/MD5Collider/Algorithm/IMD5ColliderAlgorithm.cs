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
        byte[] RandomSeed { get; set; }

        TimeSpan ElapsedTime { get; }

        void FindCollision();

        void Stop();

        string Status { get; set; }

        int MatchProgressMax { get; }
        int MatchProgress { get; }
    }
}
