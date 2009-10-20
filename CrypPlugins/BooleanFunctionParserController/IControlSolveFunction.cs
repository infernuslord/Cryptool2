using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// for IControl
using Cryptool.PluginBase.Control;

namespace Cryptool.BooleanFunctionParserController
{
    /* **********************************
     * Arguments:
     *      bool[] dataOne:        the data array for the function. Can also be null to use BFP input or FP quickwatch.
     *      bool[] dataTwo:        the data array for the function. Can also be null to use BFP input or FP quickwatch.
     *                          
     *      Examples:
     *      SolveFunction(null, null);
     * **********************************/
    public interface IControlSolveFunction : IControl
    {
        int SolveFunction(bool[] dataOne, bool[] dataTwo);
    }
}
