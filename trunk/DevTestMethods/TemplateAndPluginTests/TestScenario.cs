using System;
using System.Reflection;

namespace Tests.TemplateAndPluginTests
{
    internal abstract class TestScenario
    {
        private readonly PropertyInfo[] _inputProperties;
        private readonly object[] _inputObjects;
        private readonly PropertyInfo[] _outputProperties;
        private readonly object[] _outputObjects;

        protected TestScenario(PropertyInfo[] inputProperties, object[] inputObjects, PropertyInfo[] outputProperties, object[] outputObjects)
        {
            _inputProperties = inputProperties;
            _inputObjects = inputObjects;
            _outputProperties = outputProperties;
            _outputObjects = outputObjects;
        }

        public bool Test(object[] inputValues, object[] expectedOutputValues)
        {
            Initialize();

            if (inputValues.Length != _inputProperties.Length)
            {
                throw new ArgumentException("input vector doesn't match scenario.");
            }
            if (expectedOutputValues.Length != _outputProperties.Length)
            {
                throw new ArgumentException("output vector doesn't match scenario.");
            }

            for (int i = 0; i < _inputProperties.Length; i++)
            {
                _inputProperties[i].SetValue(_inputObjects[i], inputValues[i], null);
            }
            
            Execute();

            for (int i = 0; i < _outputProperties.Length; i++)
            {
                if (!_outputProperties[i].GetValue(_outputObjects[i], null).Equals(expectedOutputValues[i]))
                {
                    return false;
                }
            }
            return true;
        }

        protected abstract void Execute();
        protected abstract void Initialize();
    }
}