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
// for QuickwatchPresentaton
using System.Windows.Threading;
using System.Threading;

namespace Cryptool.BooleanFunctionParser
{
    [Author("Soeren Rinne", "soeren.rinne@cryptool.de", "Ruhr-Universitaet Bochum, Chair for System Security", "http://www.trust.rub.de/")]
    [PluginInfo(false, "Boolean Function Parser", "Boolean Function Parser (BFP). Computes the result of a boolean function f(i).", "BooleanFunctionParser/DetailedDescription/Description.xaml", "BooleanFunctionParser/Images/icon2.png")]
    public class BooleanFunctionParser : IThroughput
    {
        #region Private variables

        private BooleanFunctionParserPresentation booleanFunctionParserPresentation;
        private BooleanFunctionParserSettings settings;
        private string inputFunction;
        //private bool[] inputVariableOne;
        //private bool[] inputVariableTwo;
        //private bool[] inputVariableThree;
        private bool output;
        private bool lastInputWasFunction = false;
        private int inputs = 1;
        //private string fillValue;
        private bool canSendPropertiesChangedEvent = true;

        #endregion

        #region Events
        public event DynamicPropertiesChanged OnDynamicPropertiesChanged;
        #endregion

        #region Public variables

        public int inputOneFlag = 0;
        public int inputTwoFlag = 0;
        public int inputThreeFlag = 0;
        public int[] additionalInputsFlag = null;

        #endregion

        #region Public interfaces

        /// <summary>
        /// Contructor
        /// </summary>
        public BooleanFunctionParser()
        {
            this.settings = new BooleanFunctionParserSettings();
            settings.PropertyChanged += settings_PropertyChanged;

            booleanFunctionParserPresentation = new BooleanFunctionParserPresentation();
            Presentation = booleanFunctionParserPresentation;
            booleanFunctionParserPresentation.textBoxInputFunction.TextChanged += textBoxInput_TextChanged;
            booleanFunctionParserPresentation.textBoxInputData.TextChanged += textBoxInput_TextChanged;

            CanChangeDynamicProperty = true;
            // Thomas says:
            // No dynProp event in constructor - editor will read the property initial without the event.
            // event can cause problems when using save files and is processed after 
            // connections have been restored. 
            CreateInputOutput(false);
        }

        void textBoxInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            settings.HasChanges = true;
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

        /*[PropertyInfo(Direction.InputData, "Function Variable One (i_1.j)", "Input a boolean value to be processed by the function", "", false, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
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
        }*/

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
            settings.Function = (string)booleanFunctionParserPresentation.textBoxInputFunction.Dispatcher.Invoke(
                DispatcherPriority.Normal, (DispatcherOperationCallback)delegate
                {
                    return booleanFunctionParserPresentation.textBoxInputFunction.Text;
                }, null);
            settings.Data = (string)booleanFunctionParserPresentation.textBoxInputData.Dispatcher.Invoke(
               DispatcherPriority.Normal, (DispatcherOperationCallback)delegate
               {
                   return booleanFunctionParserPresentation.textBoxInputData.Text;
               }, null);
            settings.FunctionCube = (string)booleanFunctionParserPresentation.textBoxInputFunction2.Dispatcher.Invoke(
                DispatcherPriority.Normal, (DispatcherOperationCallback)delegate
                {
                    return booleanFunctionParserPresentation.textBoxInputFunction2.Text;
                }, null);
            settings.DataCube = (string)booleanFunctionParserPresentation.textBoxInputData2.Dispatcher.Invoke(
               DispatcherPriority.Normal, (DispatcherOperationCallback)delegate
               {
                   return booleanFunctionParserPresentation.textBoxInputData2.Text;
               }, null);
        }

        public void Execute()
        {
            try
            {
                // do calculation only, if all input flags are clean (= 1) or last event was from the function and all inputs are dirty (= 0)
                int sumOfFlags = 0;
                //string allFlags = null;
                foreach (int flag in additionalInputsFlag) {
                    sumOfFlags += flag;
                    //allFlags += flag.ToString();
                }
                //GuiLogMessage("sumOfFlags: " + sumOfFlags + ", addIFl: " + allFlags, NotificationLevel.Info);

                //if (sumOfFlags == additionalInputsFlag.Length || (lastInputWasFunction == true && sumOfFlags == 0))
                if (sumOfFlags == additionalInputsFlag.Length)
                {
                    // set all flags to zero
                    for (int flagIteration = 0; flagIteration < additionalInputsFlag.Length; flagIteration++)
                    {
                        additionalInputsFlag[flagIteration] = 0;
                    }
                    // revert also state of inputFunction flag
                    lastInputWasFunction = false;

                    int intOutput = ParseBooleanFunction(inputFunction, null, 0);
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
            if (booleanFunctionParserPresentation.textBoxInputFunction != null)
                booleanFunctionParserPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    booleanFunctionParserPresentation.textBoxInputFunction.Text = settings.Function;
                }, null);
            if (booleanFunctionParserPresentation.textBoxInputData != null)
                booleanFunctionParserPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    booleanFunctionParserPresentation.textBoxInputData.Text = settings.Data;
                }, null);
            if (booleanFunctionParserPresentation.textBoxInputFunction2 != null)
                booleanFunctionParserPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    booleanFunctionParserPresentation.textBoxInputFunction2.Text = settings.FunctionCube;
                }, null);
            if (booleanFunctionParserPresentation.textBoxInputData2 != null)
                booleanFunctionParserPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    booleanFunctionParserPresentation.textBoxInputData2.Text = settings.DataCube;
                }, null);
            booleanFunctionParserPresentation.SwitchCubeView(settings.UseBFPforCube);
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
            else if (e.PropertyName == "UseBFPforCube")
            {
                booleanFunctionParserPresentation.SwitchCubeView(settings.UseBFPforCube);
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
                inputs = 0;
                additionalInputsFlag = new int[settings.CountOfInputs];
                for (int i = 0; i < settings.CountOfInputs; i++)
                {
                    AddInput("Input " + i, "Boolean[] Input " + i);
                    additionalInputsFlag[i] = 0;
                }
                // Event should be fired even if no additional inputs are set, because when 
                // setting back to zero editor needs the event to remove the input.
                if (announcePropertyChange) DynamicPropertiesChanged();
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
        public int ParseBooleanFunction(string function, bool[] inputVariables, int switchInputs)
        {
            // if function is empty, use input funtion (could happen in case of a master/slave call) or quickwatch function
            // get quickwatch function
            string quickwatchFunction = (string)this.booleanFunctionParserPresentation.textBoxInputFunction.Dispatcher.Invoke(DispatcherPriority.Normal, (DispatcherOperationCallback)delegate
            {
                return booleanFunctionParserPresentation.textBoxInputFunction.Text;
            }, booleanFunctionParserPresentation);
            string quickwatchFunctionCube = (string)this.booleanFunctionParserPresentation.textBoxInputFunction2.Dispatcher.Invoke(DispatcherPriority.Normal, (DispatcherOperationCallback)delegate
            {
                return booleanFunctionParserPresentation.textBoxInputFunction2.Text;
            }, booleanFunctionParserPresentation);

            if (function == null || function == string.Empty)
            {
                if (inputFunction != null && inputFunction != string.Empty)
                    function = inputFunction;
                else if (quickwatchFunction != null && quickwatchFunction != string.Empty && switchInputs == 1)
                    function = quickwatchFunction;
                else if (quickwatchFunctionCube != null && quickwatchFunctionCube != string.Empty && switchInputs == 2)
                    function = quickwatchFunctionCube;
                else if (quickwatchFunction != null && quickwatchFunction != string.Empty)
                    function = quickwatchFunction;
                else
                    return -1;
            }
            // replace variables with data
            string strExpression = ReplaceVariables(function, inputVariables, switchInputs);
            // test if function is valid
            string strExpressionTested = TestFunction(strExpression);
            if (strExpressionTested == "foo")
            {
                GuiLogMessage(strExpression + " is not a binary expression (e.g. 1 + 0 * 1). Aborting now.", NotificationLevel.Error);
                return -1;
            }
            else
            {
                //GuiLogMessage("Your expression with variables replaced: " + strExpression, NotificationLevel.Info);
                output = EvaluateString(strExpressionTested);
            }
            // Just testing
            //bool[] test = (bool[])getCurrentValue("Input 2");
            //GuiLogMessage("InputTest: " + test[0].ToString(), NotificationLevel.Info);

            return Convert.ToInt32(output);
        }

        #region private functions

        private string makeStarsInText(Match m)
        {
            // Get the matched string.
            string x = m.ToString();
            // insert an * before the i
            x = x.Insert(x.Length - 1, "*");
            // a new star is born
            return x;
        }

        private string ReplaceVariables(string strExpressionWithVariables, bool[] externData, int switchData)
        {
            // remove spaces
            string strExpression = strExpressionWithVariables.Replace(" ", "");
            // add * if there aren't any (and should be)
            // example: x^2+x^2x^3 ==> x^2+x^2*x^3
            Regex makeStars = new Regex("([0-9])i");
            strExpression = makeStars.Replace(strExpression, new MatchEvaluator(makeStarsInText));

            // replace variables with value and get numeric values from boolean inputs (if there are any)
            /*if (inputOneFlag == 1 && inputVariableOne != null)
            {
                char[] strInputVariableOne = new char[inputVariableOne.Length];
                for (int i = inputVariableOne.Length - 1; i >= 0; i--)
                {
                    // get numeric values from bool inputs
                    strInputVariableOne[i] = inputVariableOne[i] ? '1' : '0';
                    // replace variables with value
                    string replacement = "i_1." + i;
                    strExpression = strExpression.Replace(replacement, strInputVariableOne[i].ToString());
                }
            }
            if (inputTwoFlag == 1 && inputVariableTwo != null)
            {
                char[] strInputVariableTwo = new char[inputVariableTwo.Length];
                for (int i = inputVariableTwo.Length - 1; i >= 0; i--)
                {
                    // get numeric values from bool inputs
                    strInputVariableTwo[i] = inputVariableTwo[i] ? '1' : '0';
                    string replacement = "i_2." + i;
                    strExpression = strExpression.Replace(replacement, strInputVariableTwo[i].ToString());
                }
            }
            if (inputThreeFlag == 1 && inputVariableThree != null)
            {
                char[] strInputVariableThree = new char[inputVariableThree.Length];
                for (int i = inputVariableThree.Length - 1; i >= 0; i--)
                {
                    // get numeric values from bool inputs
                    strInputVariableThree[i] = inputVariableThree[i] ? '1' : '0';
                    string replacement = "i_3." + i;
                    strExpression = strExpression.Replace(replacement, strInputVariableThree[i].ToString());
                }
            }*/
            // replace additional inputs data (if there are any)
            for (int i = 0; i < inputs; i++)
            {
                if (getCurrentValue("Input " + i) != null)
                {
                    bool[] additionalTempValueBool = (bool[])methodGetValue("Input " + i);
                    char[] strInputVariableAditionalTemp = new char[additionalTempValueBool.Length];
                    for (int j = additionalTempValueBool.Length - 1; j >= 0; j--)
                    {
                        // get numeric values from bool inputs
                        strInputVariableAditionalTemp[j] = additionalTempValueBool[j] ? '1' : '0';
                        string replacement = "i_" + (i) + "." + j;
                        strExpression = strExpression.Replace(replacement, strInputVariableAditionalTemp[j].ToString());
                    }
                }
            }
            // replace extern data (i_0.*) (if there is any)
            if (externData != null && externData.Length != 0)
            {
                char[] strInputVariableExtern = new char[externData.Length];
                for (int i = strInputVariableExtern.Length - 1; i >= 0; i--)
                {
                    // get numeric values from bool inputs
                    strInputVariableExtern[i] = externData[i] ? '1' : '0';
                    string replacement = "i_0." + i;
                    strExpression = strExpression.Replace(replacement, strInputVariableExtern[i].ToString());
                }
            }
            // replace quickwatch data (i_q.*) (if there is any)
            if (switchData == 1 || switchData == 0)
            {
                string quickwatchData = (string)this.booleanFunctionParserPresentation.textBoxInputData.Dispatcher.Invoke(DispatcherPriority.Normal, (DispatcherOperationCallback)delegate
                {
                    return booleanFunctionParserPresentation.textBoxInputData.Text;
                }, booleanFunctionParserPresentation);
                if (quickwatchData == null || quickwatchData != string.Empty)
                {
                    char[] strInputVariableQuickwatch = new char[quickwatchData.Length];
                    strInputVariableQuickwatch = quickwatchData.ToCharArray();
                    for (int i = strInputVariableQuickwatch.Length - 1; i >= 0 ; i--)
                    {
                        string replacement = "i_q." + i;
                        strExpression = strExpression.Replace(replacement, strInputVariableQuickwatch[i].ToString());
                    }
                }
            } if (switchData == 2)
            {
                string quickwatchDataCube = (string)this.booleanFunctionParserPresentation.textBoxInputData2.Dispatcher.Invoke(DispatcherPriority.Normal, (DispatcherOperationCallback)delegate
                {
                    return booleanFunctionParserPresentation.textBoxInputData2.Text;
                }, booleanFunctionParserPresentation);
                if (quickwatchDataCube == null || quickwatchDataCube != string.Empty)
                {
                    char[] strInputVariableQuickwatch = new char[quickwatchDataCube.Length];
                    strInputVariableQuickwatch = quickwatchDataCube.ToCharArray();
                    for (int i = strInputVariableQuickwatch.Length - 1; i >= 0; i--)
                    {
                        string replacement = "i_q." + i;
                        strExpression = strExpression.Replace(replacement, strInputVariableQuickwatch[i].ToString());
                    }
                }
            }
            

            // replace AND, NAND, OR, NOR, XOR, NXOR with symbols
            // NAND => -
            strExpression = strExpression.Replace("NAND", "-");
            // AND => *
            strExpression = strExpression.Replace("AND", "*");

            // NOR => _
            strExpression = strExpression.Replace("NOR", "_");

            // NXOR => °
            strExpression = strExpression.Replace("NXOR", "°");
            // XOR => *
            strExpression = strExpression.Replace("XOR", "+");

            // OR => |
            strExpression = strExpression.Replace("OR", "|");

            // replace ^ and & with symbols
            // ^ => XOR => +
            strExpression = strExpression.Replace("^", "+");

            // & => AND => *
            strExpression = strExpression.Replace("&", "*");

            return strExpression;
        }

        // validates expression in function
        private string TestFunction(string strExpression)
        {
            // remove spaces from given expression
            string strExpressionNormalized = strExpression.Replace(" ", "");
            char tab = '\u0009';
            strExpressionNormalized = strExpressionNormalized.Replace(tab.ToString(), "");

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

            //GuiLogMessage("Position ( & ): " + positionLeftParenthesis + ", " + positionRightParenthesis, NotificationLevel.Debug);

            if (positionLeftParenthesis != -1 && positionRightParenthesis != -1)
            {
                temp = function.Substring(positionLeftParenthesis + 1, positionRightParenthesis - positionLeftParenthesis - 1);
                //GuiLogMessage("New function: " + temp, NotificationLevel.Debug);
                bool parenthesisResult = EvaluateString(temp);
                function = function.Remove(positionLeftParenthesis, positionRightParenthesis - positionLeftParenthesis + 1);
                function = function.Insert(positionLeftParenthesis, Convert.ToInt32(parenthesisResult).ToString());
            }

            //GuiLogMessage("Function after '(':  " + function, NotificationLevel.Debug);

            // test for exclamation mark aka 'NOT'
            int positionExclamationMark = function.IndexOf("!");

            while (positionExclamationMark != -1)
            {
                //GuiLogMessage("Position of '!': " + positionExclamationMark, NotificationLevel.Debug);

                // remove exclamation mark
                function = function.Remove(positionExclamationMark, 1);

                // invert the binary digit following the excl. mark
                string toInvert = function.Substring(positionExclamationMark, 1);
                //GuiLogMessage("toInvert: " + toInvert, NotificationLevel.Debug);

                if (toInvert == "1") toInvert = "0";
                else toInvert = "1";
                // remove old value
                function = function.Remove(positionExclamationMark, 1);
                // insert new value
                function = function.Insert(positionExclamationMark, toInvert);

                // any other NOTs in there?
                positionExclamationMark = function.IndexOf("!");
            }

            //GuiLogMessage("Function after '!':  " + function, NotificationLevel.Debug);

            // test for AND aka '*'
            int positionAND = function.IndexOf("*");

            while (positionAND != -1)
            {
                //GuiLogMessage("Position of '*': " + positionAND, NotificationLevel.Debug);

                // get both operands
                string operator1 = function.Substring(positionAND - 1, 1);
                string operator2 = function.Substring(positionAND + 1, 1);
                //GuiLogMessage("op1 and op2: " + operator1 + ", " + operator2, NotificationLevel.Info);

                string sum = null;
                try
                {
                    sum = (Int32.Parse(operator1) & Int32.Parse(operator2)).ToString();
                }
                catch (Exception ex)
                {
                    GuiLogMessage("sum fehlgeschlagen:", NotificationLevel.Info);
                    GuiLogMessage("op1 and op2: " + operator1 + ", " + operator2, NotificationLevel.Info);
                }
                
                //GuiLogMessage("sum: " + sum, NotificationLevel.Debug);

                // remove old values
                function = function.Remove(positionAND - 1, 3);

                // insert new value
                function = function.Insert(positionAND - 1, sum);
                //GuiLogMessage("function: " + function, NotificationLevel.Debug);

                // any other ANDs in there?
                positionAND = function.IndexOf("*");
            }

            // test for XOR aka '+'
            int positionXOR = function.IndexOf("+");

            while (positionXOR != -1)
            {
                //GuiLogMessage("Position of '+': " + positionXOR, NotificationLevel.Debug);

                // get both operands
                string operator1 = function.Substring(positionXOR - 1, 1);
                string operator2 = function.Substring(positionXOR + 1, 1);
                //GuiLogMessage("op1 and op2: " + operator1 + ", " + operator2, NotificationLevel.Debug);

                string product = (Int32.Parse(operator1) ^ Int32.Parse(operator2)).ToString();
                
                //GuiLogMessage("product: " + product, NotificationLevel.Debug);

                // remove old values
                function = function.Remove(positionXOR - 1, 3);

                // insert new value
                function = function.Insert(positionXOR - 1, product);
                //GuiLogMessage("function: " + function, NotificationLevel.Debug);

                // any other XORs in there?
                positionXOR = function.IndexOf("+");
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
            // set flag of input <-- is now set in Execute()
            //int numberInPropertyKey = Int32.Parse(propertyKey.Substring(propertyKey.Length - 1));
            //additionalInputsFlag[numberInPropertyKey] = 0;

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
                    // set flag of input
                    int numberInPropertyKey = Int32.Parse(propertyKey.Substring(6));
                    additionalInputsFlag[numberInPropertyKey] = 1;
                }

                OnPropertyChanged(propertyKey);
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return Presentation; }
        }

        public UserControl Presentation { get; private set; }

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

        private IControlSolveFunction bfpSlave;
        [PropertyInfo(Direction.ControlSlave, "BFP Slave", "Direct access to BFP.", "", DisplayLevel.Beginner)]
        public IControlSolveFunction BFPSlave
        {
            get
            {
                if (bfpSlave == null)
                    bfpSlave = new BFPControl(this);
                return bfpSlave;
            }
        }

        #endregion
    }

    #region BFPControl : IControlSolveFunction

    public class BFPControl : IControlSolveFunction
    {
        public event IControlStatusChangedEventHandler OnStatusChanged;
        private BooleanFunctionParser plugin;

        public BFPControl(BooleanFunctionParser Plugin)
        {
            this.plugin = Plugin;
        }

        #region IControlEncryption Members

        // here comes the slave side implementation of SolveFunction
        public int SolveFunction(string function, bool[] data, int switchInputs)
        {
            int resultInt;

            // the result is computed by calling the ParseBooleanFunction (step into it with F11)
            // returns -1 on error (e.g. not a valid function)
            resultInt = plugin.ParseBooleanFunction(function, data, switchInputs);

            return resultInt;
        }

        #endregion
    }

    #endregion
}
