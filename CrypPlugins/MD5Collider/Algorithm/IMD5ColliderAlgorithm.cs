using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.MD5Collider.Algorithm
{
    interface IMD5ColliderAlgorithm
    {
        byte[] FirstCollidingData { get; }
        byte[] SecondCollidingData { get; }
        byte[] RandomSeed { get; set; }

        void FindCollision();

        void Stop();
    }
}
