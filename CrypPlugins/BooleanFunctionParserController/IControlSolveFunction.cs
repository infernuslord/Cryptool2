using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// for IControl
using Cryptool.PluginBase.Control;

namespace Cryptool.BooleanFunctionParserController
{
    public interface IControlSolveFunction : IControl
    {
        int SolveFunction(string function, bool[] data, int switchInputs);
    }
}
