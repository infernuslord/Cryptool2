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
     *      string function:    the function to be solved. Can also be null to use BFP input or BFP quickwatch.
     *      bool[] data:        the data array for the function. Can also be null to use BFP input or FP quickwatch.
     *      int switchInputs:   in normal behaviour this should be 0. In special case like CubeAttack-mode
     *                          one can switch between quickwatch column 1 and column 2. Therefor the switch is 1 or 2.
     *                          
     *      Examples:
     *      SolveFunction("i_0.0 * i_0.1", bool[] myData = new bool[] {true, false}, 0);
     *      SolveFunction("i_1.0 * i_1.1", null, 0);
     *      SolveFunction("i_q.0 * i_q.1", bool[] myData = new bool[] {true, false}, 2);
     *      SolveFunction(null, null, 1);
     *      SolveFunction(null, null, 2);
     * **********************************/
    public interface IControlSolveFunction : IControl
    {
        int SolveFunction(string function, bool[] data, int switchInputs);
    }
}
