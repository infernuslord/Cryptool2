using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Control;

namespace KeySearcher.Helper
{
    class KeyQualityHelper
    {
        private readonly IControlCost _costFunction;

        public KeyQualityHelper(IControlCost costFunction)
        {
            _costFunction = costFunction;
        }

        public bool IsBetter(double value, double threshold)
        {
            if (_costFunction.getRelationOperator() == RelationOperator.LargerThen)
            {
                if (value > threshold)
                {
                    return true;
                }
            }
            else
            {
                if (value < threshold)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
