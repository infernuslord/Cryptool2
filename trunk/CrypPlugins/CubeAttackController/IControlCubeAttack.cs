using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// for IControl
using Cryptool.PluginBase.Control;

namespace Cryptool.CubeAttackController
{
    public interface IControlCubeAttack : IControl
    {
        int GenerateBlackboxOutputBit(int[] v, int[] x, int outputBit);
    }
}
