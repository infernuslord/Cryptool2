using Cryptool.PluginBase.Control;

namespace KeySearcher.Helper
{
    class KeyQualityHelper
    {
        private readonly IControlCost costFunction;

        public KeyQualityHelper(IControlCost costFunction)
        {
            this.costFunction = costFunction;
        }

        public bool IsBetter(double value, double threshold)
        {
            if (costFunction.getRelationOperator() == RelationOperator.LargerThen)
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
