using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cryptool.PluginBase;
using System.IO;
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Security.Cryptography;
// for [MethodImpl(MethodImplOptions.Synchronized)]
using System.Runtime.CompilerServices;
// for RegEx
using System.Text.RegularExpressions;
// for IControl
using Cryptool.PluginBase.Control;
// reference to the BFPController interface (own dll)
using Cryptool.BooleanFunctionParserController;

namespace BooleanFunctionParser
{
    [Author("Soeren Rinne", "soeren.rinne@cryptool.de", "Ruhr-Universitaet Bochum, Chair for System Security", "http://www.trust.rub.de/")]
    [PluginInfo(false, "Boolean Function Parser", "Boolean Function Parser (BFP). Computes the result of a boolean function f(i).", "BooleanFunctionParser/DetailedDescription/Description.xaml", "BooleanFunctionParser/Images/icon2.png")]
    public class BooleanFunctionParser : IThroughput
    {
        #region Private variables

        private BooleanFunctionParserSettings settings;
        private string inputFunction;
        private bool[] inputVariableOne;
        private bool[] inputVariableTwo;
        private bool[] inputVariableThree;
        private bool output;
        private bool lastInputWasFunction = false;
        private int inputs = 0;
        private string fillValue;
        private bool canSendPropertiesChangedEvent = true;

        #endregion

        #region Events
        public event DynamicPropertiesChanged OnDynamicPropertiesChanged;
        #endregion

        #region Public variables

        public int inputOneFlag = 0;
        public int inputTwoFlag = 0;
        public int inputThreeFlag = 0;

        #endregion

        #region Public interfaces

        /// <summary>
        /// Contructor
        /// </summary>
        public BooleanFunctionParser()
        {
            this.settings = new BooleanFunctionParserSettings();
            settings.PropertyChanged += settings_PropertyChanged;
            CanChangeDynamicProperty = true;
            // Thomas says:
            // No dynProp event in constructor - editor will read the property initial without the event.
            // event can cause problems when using save files and is processed after 
            // connections have been restored. 
            CreateInputOutput(false);
        }

        [PropertyInfo(Direction.InputData, "Boolean Function f(i)", "Boolean function f(i) to compute.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public String InputFunction
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return inputFunction; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                inputFunction = value;
                lastInputWasFunction = true;
                OnPropertyChanged("InputFunction");
            }
        }

        [PropertyInfo(Direction.InputData, "Function Variable One (i_1.j)", "Input a boolean value to be processed by the function", "", false, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public bool[] InputOne
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return this.inputVariableOne;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                this.inputVariableOne = value;
                lastInputWasFunction = false;
                OnPropertyChanged("InputOne");
                // clean inputOne
                inputOneFlag = 1;
            }
        }

        [PropertyInfo(Direction.InputData, "Function Variable Two (i_2.j)", "Input a boolean value to be processed by the function", "", false, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public bool[] InputTwo
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return this.inputVariableTwo;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                this.inputVariableTwo = value;
                lastInputWasFunction = false;
                OnPropertyChanged("InputTwo");
                // clean inputOne
                inputTwoFlag = 1;
            }
        }

        [PropertyInfo(Direction.InputData, "Function Variable Three (i_3.j)", "Input a boolean value to be processed by the function", "", false, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public bool[] InputThree
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return this.inputVariableThree;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                this.inputVariableThree = value;
                lastInputWasFunction = false;
                OnPropertyChanged("InputThree");
                // clean inputOne
                inputThreeFlag = 1;
            }
        }

        [PropertyInfo(Direction.OutputData, "Function output", "Output after procesing the given function.", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public bool Output
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return output;
            }
            set
            {   // is readonly
            }
        }

        #endregion

        #region IPlugin Members

        public void Dispose()
        {
            
        }

        public void Execute()
        {
            try
            {
                // do calculation only, if last event wasn't from the function input
                if (lastInputWasFunction == false)
                {
                    int intOutput = ParseBooleanFunction(inputFunction, null);
                    if (intOutput == -1) return;
                    else
                    {
                        output = Convert.ToBoolean(intOutput);
                        OnPropertyChanged("Output");
                    }
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
            finally
            {
                ProgressChanged(1, 1);
            }
        }

        public void Initialize()
        {
            
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        // catches PropertyChanged event from settings
        void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // if the count of inputs has been change, renew all inputs
            if (e.PropertyName == "CountOfInputs")
            {
                CreateInputOutput(true);
            }
        }

        public void Pause()
        {
            
        }

        public void PostExecution()
        {
            
        }

        public void PreExecution()
        {
            
        }

        public void CreateInputOutput(bool announcePropertyChange)
        {
            try
            {
                DicDynamicProperties.Clear();
                for (int i = 0; i < settings.CountOfInputs; i++)
                {
                    AddInput("Input " + i, "Test Input " + i);
                    if (announcePropertyChange) DynamicPropertiesChanged();
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
        }

        /* *******************************************************************************
         * Main function to be used in the M/S mode and in general
         * inputs:
         * string function - the boolean function to be computed with variables
         * bool[] inputVariables - a boolean array to replace the variables
         * 
         * ouput:
         * int - the one bit long result of the given function; returns -1 on any failure
         * *******************************************************************************
        */
        public int ParseBooleanFunction(string function, bool[] inputVariables)
        {
            // get function from input and replace variables with data
            string strExpression = ReplaceVariables(function, inputVariables);
            // test if function is valid
            string strExpressionTested = TestFunction(strExpression);
            if (strExpressionTested == "foo")
            {
                GuiLogMessage(strExpression + " is not a binary expression (e.g. 1 + 0 * 1). Aborting now.", NotificationLevel.Error);
                return -1;
            }
            else
            {
                GuiLogMessage("Your expression with variables replaced: " + strExpression, NotificationLevel.Info);
                output = EvaluateString(strExpressionTested);
            }
            // Just testing
            //bool[] test = (bool[])getCurrentValue("Input 2");
            //GuiLogMessage("InputTest: " + test[0].ToString(), NotificationLevel.Info);

            return Convert.ToInt32(output);
        }

        #region private functions

        private string ReplaceVariables(string strExpressionWithVariables, bool[] externData)
        {
            string strExpression = strExpressionWithVariables;

            // replace variables with value and get numeric values from boolean inputs (if there are any)
            if (inputOneFlag == 1 && inputVariableOne.Length != 0)
            {
                char[] strInputVariableOne = new char[inputVariableOne.Length];
                for (int i = 0; i < inputVariableOne.Length; i++)
                {
                    // get numeric values from bool inputs
                    strInputVariableOne[i] = inputVariableOne[i] ? '1' : '0';
                    // replace variables with value
                    string replacement = "i_1." + i;
                    strExpression = strExpression.Replace(replacement, strInputVariableOne[i].ToString());
                }
            }
            if (inputTwoFlag == 1 && inputVariableTwo.Length != 0)
            {
                char[] strInputVariableTwo = new char[inputVariableTwo.Length];
                for (int i = 0; i < inputVariableTwo.Length; i++)
                {
                    // get numeric values from bool inputs
                    strInputVariableTwo[i] = inputVariableTwo[i] ? '1' : '0';
                    string replacement = "i_2." + i;
                    strExpression = strExpression.Replace(replacement, strInputVariableTwo[i].ToString());
                }
            }
            if (inputThreeFlag == 1 && inputVariableThree.Length != 0)
            {
                char[] strInputVariableThree = new char[inputVariableThree.Length];
                for (int i = 0; i < inputVariableThree.Length; i++)
                {
                    // get numeric values from bool inputs
                    strInputVariableThree[i] = inputVariableThree[i] ? '1' : '0';
                    string replacement = "i_3." + i;
                    strExpression = strExpression.Replace(replacement, strInputVariableThree[i].ToString());
                }
            }
            // replace extern data (i_0.*) (if there is any)
            if (externData != null && externData.Length != 0)
            {
                char[] strInputVariableExtern = new char[externData.Length];
                for (int i = 0; i < strInputVariableExtern.Length; i++)
                {
                    // get numeric values from bool inputs
                    strInputVariableExtern[i] = externData[i] ? '1' : '0';
                    string replacement = "i_0." + i;
                    strExpression = strExpression.Replace(replacement, strInputVariableExtern[i].ToString());
                }
            }
            

            // replace AND, NAND, OR, NOR, XOR, NXOR with symbols
            // NAND => -
            strExpression = strExpression.Replace("NAND", "-");
            // AND => +
            strExpression = strExpression.Replace("AND", "+");

            // NOR => _
            strExpression = strExpression.Replace("NOR", "_");

            // NXOR => °
            strExpression = strExpression.Replace("NXOR", "°");
            // XOR => *
            strExpression = strExpression.Replace("XOR", "*");

            // OR => |
            strExpression = strExpression.Replace("OR", "|");

            // replace ^ and v with symbols
            // ^ => AND => +
            strExpression = strExpression.Replace("^", "+");

            // v => OR => |
            strExpression = strExpression.Replace("v", "|");

            return strExpression;
        }

        // validates expression in function
        private string TestFunction(string strExpression)
        {
            // remove spaces from given expression
            string strExpressionNormalized = strExpression.Replace(" ", "");

            // test if count of '(' equals count of ')'
            Regex countLeftPRegEx = new Regex(@"\(");
            Regex countRightPRegEx = new Regex(@"\)");
            if (countLeftPRegEx.Matches(strExpressionNormalized).Count != countRightPRegEx.Matches(strExpressionNormalized).Count)
            {
                GuiLogMessage("The count of ( is not equal to the count of )", NotificationLevel.Error);
                return "foo";
            }

            // test expression
            Regex objBoolExpression = new Regex(@"([\(]?[\!]?)([0-1]([\\*]|[\\+]|[\\|]|[\\-]|[_]|[°]|[v]|[\\^]|[\\!])+[0-1]{1})");
            if (!objBoolExpression.IsMatch(strExpressionNormalized))
            {
                GuiLogMessage("That's not a legal function", NotificationLevel.Error);
                return "foo";
            }
            else
            {
                return strExpressionNormalized;
            }
        }

        // solves string with variables replaced by values
        private bool EvaluateString(string function)
        {
            string temp;

            // test for parenthesis
            int positionLeftParenthesis = function.IndexOf("(");
            int positionRightParenthesis = function.LastIndexOf(")");

            GuiLogMessage("Position ( & ): " + positionLeftParenthesis + ", " + positionRightParenthesis, NotificationLevel.Debug);

            if (positionLeftParenthesis != -1 && positionRightParenthesis != -1)
            {
                temp = function.Substring(positionLeftParenthesis + 1, positionRightParenthesis - positionLeftParenthesis - 1);
                GuiLogMessage("New function: " + temp, NotificationLevel.Debug);
                bool parenthesisResult = EvaluateString(temp);
                function = function.Remove(positionLeftParenthesis, positionRightParenthesis - positionLeftParenthesis + 1);
                function = function.Insert(positionLeftParenthesis, Convert.ToInt32(parenthesisResult).ToString());
            }

            GuiLogMessage("Function after '(':  " + function, NotificationLevel.Debug);

            // test for exclamation mark aka 'NOT'
            int positionExclamationMark = function.IndexOf("!");

            while (positionExclamationMark != -1)
            {
                GuiLogMessage("Position of '!': " + positionExclamationMark, NotificationLevel.Debug);

                // remove exclamation mark
                function = function.Remove(positionExclamationMark, 1);

                // invert the binary digit following the excl. mark
                string toInvert = function.Substring(positionExclamationMark, 1);
                GuiLogMessage("toInvert: " + toInvert, NotificationLevel.Debug);

                if (toInvert == "1") toInvert = "0";
                else toInvert = "1";
                // remove old value
                function = function.Remove(positionExclamationMark, 1);
                // insert new value
                function = function.Insert(positionExclamationMark, toInvert);

                // any other NOTs in there?
                positionExclamationMark = function.IndexOf("!");
            }

            GuiLogMessage("Function after '!':  " + function, NotificationLevel.Debug);

            // test for XOR aka '*'
            int positionXOR = function.IndexOf("*");

            while (positionXOR != -1)
            {
                GuiLogMessage("Position of '*': " + positionXOR, NotificationLevel.Debug);

                // remove XOR
                function = function.Remove(positionXOR, 1);


                // get both operands
                string operator1 = function.Substring(positionXOR - 1, 1);
                string operator2 = function.Substring(positionXOR, 1);
                GuiLogMessage("op1 and op2: " + operator1 + ", " + operator2, NotificationLevel.Debug);

                string product = (Int32.Parse(operator1) ^ Int32.Parse(operator2)).ToString();
                GuiLogMessage("product: " + product, NotificationLevel.Debug);
                // remove old values
                function = function.Remove(positionXOR, 1);
                function = function.Remove(positionXOR - 1, 1);
                // insert new value
                function = function.Insert(positionXOR - 1, product);
                GuiLogMessage("function: " + function, NotificationLevel.Debug);

                // any other XORs in there?
                positionXOR = function.IndexOf("*");
            }

            // test for AND aka '+'
            int positionAND = function.IndexOf("+");

            while (positionAND != -1)
            {
                GuiLogMessage("Position of '+': " + positionAND, NotificationLevel.Debug);

                // remove XOR
                function = function.Remove(positionAND, 1);


                // get both operands
                string operator1 = function.Substring(positionAND - 1, 1);
                string operator2 = function.Substring(positionAND, 1);
                GuiLogMessage("op1 and op2: " + operator1 + ", " + operator2, NotificationLevel.Debug);

                string sum = (Int32.Parse(operator1) & Int32.Parse(operator2)).ToString();
                GuiLogMessage("sum: " + sum, NotificationLevel.Debug);
                // remove old values
                function = function.Remove(positionAND, 1);
                function = function.Remove(positionAND - 1, 1);
                // insert new value
                function = function.Insert(positionAND - 1, sum);
                GuiLogMessage("function: " + function, NotificationLevel.Debug);

                // any other ANDs in there?
                positionAND = function.IndexOf("+");
            }

            bool result = Convert.ToBoolean(Int32.Parse(function));

            return result;
        }

        private object getCurrentValue(string name)
        {
            if (DicDynamicProperties.ContainsKey(name))
            {
                return DicDynamicProperties[name].Value;
            }
            return null;
        }
        
        private void AddInput(string name, string toolTip)
        {
            inputs++;
            if (name == null || name == string.Empty) name = "Input " + inputs;
            DicDynamicProperties.Add(name,
              new DynamicProperty(name, typeof(bool[]),
                new PropertyInfoAttribute(Direction.InputData, name, toolTip, "", false, true, DisplayLevel.Beginner, QuickWatchFormat.None, null))
            );
        }

        #endregion

        [MethodImpl(MethodImplOptions.Synchronized)]
        public object methodGetValue(string propertyKey)
        {
            return getCurrentValue(propertyKey); // QuickWatchDataCall to Input values
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void methodSetValue(string propertyKey, object value)
        {
            try
            {
                if (DicDynamicProperties.ContainsKey(propertyKey))
                {
                    DicDynamicProperties[propertyKey].Value = value;
                }

                OnPropertyChanged(propertyKey);
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public ISettings Settings
        {
            get
            {
                return (ISettings)this.settings;
            }
            set
            {
                this.settings = (BooleanFunctionParserSettings)value;
            }
        }

        public void Stop()
        {
            
        }

        public Dictionary<string, DynamicProperty> dicDynamicProperties = new Dictionary<string, DynamicProperty>();

        [DynamicPropertyInfo("methodGetValue", "methodSetValue", "CanChangeDynamicProperty", "OnDynamicPropertiesChanged", "CanSendPropertiesChangedEvent")]
        public Dictionary<string, DynamicProperty> DicDynamicProperties
        {
            get { return dicDynamicProperties; }
            set { dicDynamicProperties = value; }
        }

        public bool CanChangeDynamicProperty
        {
            get { return settings.CanChangeProperty; }
            set { settings.CanChangeProperty = value; }
        }

        public bool CanSendPropertiesChangedEvent
        {
            get { return canSendPropertiesChangedEvent; }
            set { canSendPropertiesChangedEvent = value; }
        }

        private void DynamicPropertiesChanged()
        {
            if (OnDynamicPropertiesChanged != null) OnDynamicPropertiesChanged(this);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region IControl

        private IControlSolveFunction testSlave;
        [PropertyInfo(Direction.ControlSlave, "BFP Slave", "Direct access to BFP.", "", DisplayLevel.Beginner)]
        public IControlSolveFunction TestSlave
        {
            get
            {
                if (testSlave == null)
                    testSlave = new BFPControl(this);
                return testSlave;
            }
        }

        #endregion
    }

    #region BFPControl : IControlSolveFunction

    public class BFPControl : IControlSolveFunction
    {
        private BooleanFunctionParser plugin;

        public BFPControl(BooleanFunctionParser Plugin)
        {
            this.plugin = Plugin;
        }

        #region IControlEncryption Members

        // here comes the slave side implementation of SolveFunction
        public int SolveFunction(string function, bool[] data)
        {
            int resultInt;

            // the result is computed by calling the ParseBooleanFunction (step into it with F11)
            // returns -1 on error (e.g. not a valid function)
            resultInt = plugin.ParseBooleanFunction(function, data);

            return resultInt;
        }

        #endregion
    }

    #endregion
}
