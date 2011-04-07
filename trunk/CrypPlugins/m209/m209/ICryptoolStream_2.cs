using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cryptool.PluginBase.IO
{

    public interface ICryptoolStream
    {
        CStreamReader CreateReader();

        long Length
        {
            get;
        }
    }
}