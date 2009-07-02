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

        #endregion

        #region Public variables

        public int inputOneFlag = 0;
        public int inputTwoFlag = 0;
        public int inputThreeFlag = 0;

        #endregion

        #region Public interface

        /// <summary>
        /// Contructor
        /// </summary>
        public BooleanFunctionParser()
        {
            this.settings = new BooleanFunctionParserSettings();
            //((BooleanFunctionParserSettings)(this.settings)).LogMessage += Xor_LogMessage;
        }

        [PropertyInfo(Direction.Input, "Boolean Function f(i)", "Boolean function f(i) to compute.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
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

        [PropertyInfo(Direction.Input, "Function Variable One (i_1.j)", "Input a boolean value to be processed by the function", "", false, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public bool[] InputOne
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                /*if (settings.InvertInputOne)
                {
                    return (!inputOne);
                }
                else*/ return this.inputVariableOne;
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

        [PropertyInfo(Direction.Input, "Function Variable Two (i_2.j)", "Input a boolean value to be processed by the function", "", false, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public bool[] InputTwo
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                /*if (settings.InvertInputOne)
                {
                    return (!inputOne);
                }
                else*/
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

        [PropertyInfo(Direction.Input, "Function Variable Three (i_3.j)", "Input a boolean value to be processed by the function", "", true, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public bool[] InputThree
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                /*if (settings.InvertInputOne)
                {
                    return (!inputOne);
                }
                else*/
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

        [PropertyInfo(Direction.Output, "Function output", "Output after procesing the given function.", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
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
                    // get function from input and replace variables
                    string strExpression = ReplaceVariables(inputFunction);
                    // test if function is valid
                    string strExpressionTested = TestFunction(strExpression);
                    if (strExpressionTested == "foo")
                    {
                        GuiLogMessage(strExpression + " is not a binary expression (e.g. 1 + 0 * 1). Aborting now.", NotificationLevel.Error);
                        return;
                    }
                    else
                    {
                        GuiLogMessage("Your expression with variables replaced: " + strExpression, NotificationLevel.Info);
                        output = EvaluateTree(output, FillTree(strExpressionTested));
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

        public void Pause()
        {
            
        }

        public void PostExecution()
        {
            
        }

        public void PreExecution()
        {
            
        }

        private string ReplaceVariables(string strExpressionWithVariables)
        {
            string strExpression = strExpressionWithVariables;

            // replace variables with value and get numeric values from bool inputs
            if (inputOneFlag == 1)
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
            if (inputTwoFlag == 1)
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
            if (inputThreeFlag == 1)
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

        private string TestFunction(string strExpression)
        {
            // remove spaces from given expression
            strExpression = strExpression.Replace(" ", "");

            // test expression
            Regex objBoolExpression = new Regex("([0-1]([\\*]|[\\+]|[\\|]|[\\-]|[_]|[°]|[v]|[\\^])+[0-1]{1})");
            if (!objBoolExpression.IsMatch(strExpression))
            {
                return "foo";
            }
            else
            {
                return strExpression;
            }
        }

        private ATreeNode FillTree(string strExpression)
        {
            // fill tree
            //char[] charPolynomial = { '0', '*', '1', '+', '0', '*', '1', '+', '1' }; // sample for debug
            char[] charPolynomial = strExpression.ToCharArray();
            int i;
            ATreeNode[] treeArray = new ATreeNode[charPolynomial.Length];

            for (i = 0; i < charPolynomial.Length; i++)
            {
                if (Char.IsDigit(charPolynomial[i]))
                // should be a digit
                {
                    if (i == 0)
                    {
                        // first digit
                        treeArray[i] = new ATreeNode("Constant");
                        if (charPolynomial[i] == '0') treeArray[i].ConstantData = false;
                        else treeArray[i].ConstantData = true;
                    }
                    else
                    {
                        // any other digit
                        treeArray[i] = new ATreeNode("Constant");
                        if (charPolynomial[i] == '0') treeArray[i].ConstantData = false;
                        else treeArray[i].ConstantData = true;
                        treeArray[i - 1].RightChild = treeArray[i];
                    }
                }
                else
                // should be an operand
                {

                    /*if (charPolynomial[i] == '!')
                    {
                        // ! operand
                        // TO DO
                    }
                    else*/
                    {
                        // * or + operands
                        if (i == 1)
                        {
                            treeArray[i] = new ATreeNode(charPolynomial[i].ToString());
                            treeArray[i].LeftChild = treeArray[i - 1];
                        }
                        else
                        {
                            treeArray[i] = new ATreeNode(charPolynomial[i].ToString());
                            treeArray[i].LeftChild = treeArray[i - 2];
                        }
                    }
                }
                // debug output
                //Console.Out.Write(treeArray[i].NodeType + " ");
            }

            return treeArray[treeArray.Length - 2];
        }

        private bool EvaluateTree(bool X, ATreeNode NodePointer)
        {
            //float RightTemp, LeftTemp, UnaryTemp;

            switch (NodePointer.NodeType)
            {
                case "+":
                    X = EvaluateTree(X, NodePointer.LeftChild) && EvaluateTree(X, NodePointer.RightChild);
                    break;

                case "-":
                    X = !(EvaluateTree(X, NodePointer.LeftChild) && EvaluateTree(X, NodePointer.RightChild));
                    break;

                case "*":
                    X = EvaluateTree(X, NodePointer.LeftChild) ^ EvaluateTree(X, NodePointer.RightChild);
                    break;

                case "°":
                    X = !(EvaluateTree(X, NodePointer.LeftChild) ^ EvaluateTree(X, NodePointer.RightChild));
                    break;

                case "|":
                    X = EvaluateTree(X, NodePointer.LeftChild) | EvaluateTree(X, NodePointer.RightChild);
                    break;

                case "_":
                    X = !(EvaluateTree(X, NodePointer.LeftChild) | EvaluateTree(X, NodePointer.RightChild));
                    break;

                case "XVariable":
                    // X = X;
                    break;

                case "Constant":
                    X = NodePointer.ConstantData;
                    break;
            }

            return X;
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
    }

    public class ATreeNode
    {
        #region private variables

        private ATreeNode _leftChild;
        private ATreeNode _rightChild;
        private ATreeNode _unaryChild;
        private string _nodeType;
        private bool _constantData;

        /*private enum _nodeType
        {
            Add,Subtract,Multiply,Divide,Power,AbsValue,Log,Negation,Sine,Square,SquareRoot,XVariable,Constant
        }*/

        #endregion

        #region public interfaces

        public ATreeNode LeftChild
        {
            get { return _leftChild; }
            set { _leftChild = value; }
        }

        public ATreeNode RightChild
        {
            get { return _rightChild; }
            set { _rightChild = value; }
        }

        public ATreeNode UnaryChild
        {
            get { return _unaryChild; }
            set { _unaryChild = value; }
        }

        public string NodeType
        {
            get { return _nodeType; }
        }

        public bool ConstantData
        {
            get { return _constantData; }
            set { _constantData = value; }
        }

        public bool HasChildren
        {
            get { return (_leftChild != null || _rightChild != null); }
        }

        #endregion

        // Constructor
        public ATreeNode(string nodeType)
        {
            _nodeType = nodeType;
            _leftChild = _rightChild = _unaryChild = null;
        }
    }
}
