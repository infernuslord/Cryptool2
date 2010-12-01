using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase.Miscellaneous;

namespace KeySearcher
{
    class KeySearcherOpenCLSubbatchOptimizer
    {
        private IKeyTranslator keyTranslator = null;
        private List<int> batchSizeFactors = null;
        private List<int> amountOfSubbatchesFactors = null;
        private int amountOfSubbatches;
        private DateTime begin;
        private TimeSpan lastDuration;
        private bool optimisticDecrease;
        private bool lastStepIncrease;
        private const long TOLERANCE = 500;
        private readonly int maxNumberOfThreads;

        public KeySearcherOpenCLSubbatchOptimizer(int maxNumberOfThreads)
        {
            this.maxNumberOfThreads = maxNumberOfThreads;
        }

        public int GetAmountOfSubbatches(IKeyTranslator keyTranslator)
        {
            if (this.keyTranslator != keyTranslator)
            {
                this.keyTranslator = keyTranslator;
                
                //Find factors of OpenCL batch size:
                List<Msieve.Factor> factors = Msieve.TrivialFactorization(keyTranslator.GetOpenCLBatchSize());
                amountOfSubbatchesFactors = new List<int>();
                foreach (var fac in factors)
                {
                    for (int i = 0; i < fac.count; i++)
                        amountOfSubbatchesFactors.Add((int)fac.factor);
                }
                amountOfSubbatches = keyTranslator.GetOpenCLBatchSize();

                batchSizeFactors = new List<int>();
                DecreaseAmountOfSubbatches();

                lastDuration = TimeSpan.MaxValue;
                optimisticDecrease = false;
                lastStepIncrease = false;
            }

            return amountOfSubbatches;
        }

        private void DecreaseAmountOfSubbatches()
        {
            if (amountOfSubbatchesFactors.Count < 1)
                return;

            if (keyTranslator.GetOpenCLBatchSize() / amountOfSubbatches >= maxNumberOfThreads)   //not more than MAXNUMBEROFTHREADS threads concurrently
                return;

            do
            {
                int maxElement = amountOfSubbatchesFactors.Max();
                batchSizeFactors.Add(maxElement);
                amountOfSubbatchesFactors.Remove(maxElement);

                amountOfSubbatches = amountOfSubbatchesFactors.Aggregate(1, (current, i) => current * i);
            } while (keyTranslator.GetOpenCLBatchSize()/amountOfSubbatches < (256*256));        //each batch should have at least size 256*256
        }

        private void IncreaseAmountOfSubbatches()
        {
            if (batchSizeFactors.Count < 1)
                return;

            if (keyTranslator.GetOpenCLBatchSize() / amountOfSubbatches <= (256 * 256)) //each batch should have at least size 256*256
                return;

            do
            {
                int minElement = batchSizeFactors.Min();
                amountOfSubbatchesFactors.Add(minElement);
                batchSizeFactors.Remove(minElement);

                amountOfSubbatches = amountOfSubbatchesFactors.Aggregate(1, (current, i) => current * i);
            } while (keyTranslator.GetOpenCLBatchSize() / amountOfSubbatches > maxNumberOfThreads);        //not more than MAXNUMBEROFTHREADS threads concurrently
        }

        public void BeginMeasurement()
        {
            begin = DateTime.Now;
        }

        public void EndMeasurement()
        {
            var thisduration = DateTime.Now - begin;

            if (Math.Abs((thisduration - lastDuration).TotalMilliseconds) > TOLERANCE)
            {
                if (lastDuration > thisduration)
                {
                    DecreaseAmountOfSubbatches();
                    lastStepIncrease = false;
                }
                else
                {
                    if (!lastStepIncrease)
                    {
                        IncreaseAmountOfSubbatches();
                        lastStepIncrease = true;
                    }
                    else
                        lastStepIncrease = false;
                }
            }
            else
            {
                lastStepIncrease = false;
                if (optimisticDecrease)
                {
                    DecreaseAmountOfSubbatches();
                    optimisticDecrease = false;
                }
                else
                    optimisticDecrease = true;
            }

            lastDuration = thisduration;
        }
    }
}
