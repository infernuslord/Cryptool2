using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// for IControl
using Cryptool.PluginBase.Control;

namespace Cryptool.TriviumController
{
    /* **********************************
     * Arguments:
     *      
     *                          
     *      Examples:
     * **********************************/
    public interface IControlTrivium : IControl
    {
        int GenerateTriviumKeystream(int[] IV, int[] key, int length, int rounds, bool byteSwapping);
    }
}
