using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Analysis;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.IO;
// Reference to the BFPController interface
using Cryptool.BooleanFunctionParserController;
// Reference to the TriviumController interface (own dll)
using Cryptool.TriviumController;

namespace Cryptool.CubeAttack
{
    [Author("David Oruba", 
        "dav083@web.de", 
        "Uni-Bochum", 
        "http://www.ruhr-uni-bochum.de/")]
    [PluginInfo(true, 
        "Cube Attack", 
        "Cube Attack", 
        "CubeAttack/DetailedDescription/Description.xaml",
        "CubeAttack/Images/ca_color.png")]
    public class CubeAttack : IAnalysisMisc
    {
        #region Private variables

        private CubeAttackSettings settings;
        private string outputSuperpoly;
        private string outputKeyBits;
        private enum CubeAttackMode { preprocessing, online, setPublicBits };
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();
        private bool stop = false;

        #endregion


        #region Public variables

        public Matrix superpolyMatrix = null;
        public List<List<int>> listCubeIndexes = null;
        public int[] pubVarGlob = null;
        public int indexOutputBit;
        public int[] outputBitIndex;

        #endregion


        #region Properties (Inputs/Outputs)
        
        [PropertyInfo(Direction.OutputData, 
            "Output of superpolys", 
            "Output the located linearly independent superpolys, cube indexes and its corresponding output bits.", 
            "", 
            false, 
            false, 
            DisplayLevel.Beginner, 
            QuickWatchFormat.Text, 
            null)]
        public CryptoolStream OutputSuperpoly
        {
            get
            {
                if (outputSuperpoly != null)
                {
                    CryptoolStream cs = new CryptoolStream();
                    listCryptoolStreamsOut.Add(cs);
                    cs.OpenRead(Encoding.Default.GetBytes(outputSuperpoly.ToCharArray()));
                    return cs;
                }
                else
                {
                    return null;
                }
            }
            set { }
        }

        [PropertyInfo(Direction.OutputData, 
            "Key bits", 
            "This output provides the result of the secret key bits", 
            "", 
            false, 
            false, 
            DisplayLevel.Beginner, 
            QuickWatchFormat.Text, 
            null)]
        public CryptoolStream OutputKeyBits
        {
            get
            {
                if (outputKeyBits != null)
                {
                    CryptoolStream cs = new CryptoolStream();
                    listCryptoolStreamsOut.Add(cs);
                    cs.OpenRead(Encoding.Default.GetBytes(outputKeyBits.ToCharArray()));
                    return cs;
                }
                else
                {
                    return null;
                }
            }
            set { }
        }

        #endregion


        #region Public interface

        /// <summary>
        /// Contructor
        /// </summary>
        public CubeAttack()
        {
            this.settings = new CubeAttackSettings();
            ((CubeAttackSettings)(this.settings)).LogMessage += CubeAttack_LogMessage;
        }

        /// <summary>
        /// Get or set all settings for this algorithm
        /// </summary>
        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (CubeAttackSettings)value; }
        }      

        /// <summary>
        /// Complete CubeAttack 
        /// </summary>
        public void Preprocessing()
        {
            ProcessCubeAttack(CubeAttackMode.preprocessing);
        }

        public void Online()
        {
            ProcessCubeAttack(CubeAttackMode.online);
        }

        /// <summary>
        /// Manual input of public bits
        /// </summary>
        public void SetPublicBits()
        {
            ProcessCubeAttack(CubeAttackMode.setPublicBits);
        }

        #endregion


        #region IPlugin members

        public void Initialize()
        {
        }

        public void Dispose()
        {
            stop = false;
            foreach (CryptoolStream stream in listCryptoolStreamsOut)
            {
                stream.Close();
            }
            listCryptoolStreamsOut.Clear();
        }

        public bool HasChanges
        {
            get { return settings.HasChanges; }
            set { settings.HasChanges = value; }
        }

        /// <summary>
        /// Fire, if progress bar has to be updated
        /// </summary>
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        /// <summary>
        /// Fire, if new message has to be shown in the status bar
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public UserControl Presentation
        {
            get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void Stop()
        {
            this.stop = true;
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void PreExecution()
        {
            Dispose();
        }

        #pragma warning disable 67
		public event StatusChangedEventHandler OnPluginStatusChanged;
        #pragma warning restore

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public void Execute()
        {
            if (settings.MaxCube > settings.PublicVar)
                CubeAttack_LogMessage("Error: Max Cube Size cannot be greater than Public Bit Size.", NotificationLevel.Error);
            else
            {
                try
                {
                    switch (settings.Action)
                    {
                        case 0:
                            Preprocessing();
                            break;
                        case 1:
                            Online();
                            break;
                        case 2:
                            SetPublicBits();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    CubeAttack_LogMessage("Error: " + ex, NotificationLevel.Error);
                }
                finally
                {
                    ProgressChanged(1.0, 1.0);
                }
            }
        }

        /// <summary>
        /// Returns the output bit of the master polynomial p
        /// </summary>
        /// <param name="v">Public bits.</param>
        /// <param name="x">Secret bits.</param>
        /// <returns>Returns the output bit of the master polynomial, either 0 or 1.</returns>
        public int Blackbox(int[] v, int[] x)
        {
            /*// Begin Trivium
            // Initialisierung
	        int[] a = new int[93]; 
	        int[] b = new int[84];
            int[] c = new int[111];
            int t1, t2, t3;
            int i,j; 
	        for (i = 0; i < 80; i++)
            {
                a[i] = x[i];
                b[i] = v[i];
		        c[i] = 0;
	        }
	        while (i < 84){
		        a[i] = 0;
		        b[i] = 0;
		        c[i] = 0;
		        i++;
	        }
	        while (i < 93){
		        a[i] = 0;
		        c[i] = 0;
		        i++;
	        }
	        while (i < 108){
		        c[i] = 0;
		        i++;
	        }
	        while (i < 111){
		        c[i] = 1;
		        i++;
	        }

	        for (i = 0; i < 672; i++)
            {
		        t1 = a[65] ^ a[92];
		        t2 = b[68] ^ b[83];
		        t3 = c[65] ^ c[110];
		        t1 = t1 ^ (a[90] & a[91]) ^ b[77];
		        t2 = t2 ^ (b[81] & b[82]) ^ c[86];
		        t3 = t3 ^ (c[108] & c[109]) ^ a[68];
		        for (j = 92; j > 0; j--)
			        a[j] = a[j-1];
		        for (j = 83; j > 0; j--)
			        b[j] = b[j-1];
		        for (j = 110; j > 0; j--)
			        c[j] = c[j-1];
		        a[0] = t3;
		        b[0] = t1;
		        c[0] = t2;
	        }

            // Keystream 
            List<int> keyOutput = new List<int>(); 
	        for (i = 0; i < 4; i++)
            {
		        t1 = a[65] ^ a[92];
		        t2 = b[68] ^ b[83];
		        t3 = c[65] ^ c[110];
                keyOutput.Add(t1 ^ t2 ^ t3);
		        t1 = t1 ^ (a[90] & a[91]) ^ b[77];
		        t2 = t2 ^ (b[81] & b[82]) ^ c[86];
		        t3 = t3 ^ (c[108] & c[109]) ^ a[68];
		        for (j = 92; j > 0; j--)
			        a[j] = a[j-1];
		        for (j = 83; j > 0; j--)
			        b[j] = b[j-1];
		        for (j = 110; j > 0; j--)
			        c[j] = c[j-1];
		        a[0] = t3;
		        b[0] = t1;
		        c[0] = t2;
	        } 
            return keyOutput[3]; 
            // End Trivium */

            int result = 0;
            
            try
            {
                switch (settings.BlackBox)
                {
                    // Parser as black box
                    case 0:
                        bool[] vBool = new bool[v.Length];
                        bool[] xBool = new bool[x.Length];
                        for (int i = 0; i < v.Length; i++)
                            vBool[i] = Convert.ToBoolean(v[i]);
                        for (int i = 0; i < x.Length; i++)
                            xBool[i] = Convert.ToBoolean(x[i]);
                        bool[] vx = new bool[v.Length + x.Length];
                        System.Buffer.BlockCopy(vBool, 0, vx, 0, vBool.Length);
                        System.Buffer.BlockCopy(xBool, 0, vx, vBool.Length, xBool.Length);
                        result = ParserOutput.SolveFunction(null, vx, 1);
                        break;
                    // Trivium as black box
                    case 1:
                        if (settings.PublicVar != 80 || settings.SecretVar != 80)
                        {
                            CubeAttack_LogMessage("Public bit size and Secret bit size must be 80", NotificationLevel.Error);
                            stop = true;
                            break;
                        }
                        else
                        {
                            result = TriviumOutput.GenerateTriviumKeystream(v, x, indexOutputBit, settings.TriviumRounds, false);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                stop = true;
                CubeAttack_LogMessage("Error: " + ex, NotificationLevel.Error);
            }
            return result; 
        }

        /// <summary>
        /// The function derives the algebraic structure of the superpoly from the maxterm.
        /// The structure is derived by computing the free term and the coefficients in the superpoly.
        /// </summary>
        /// <param name="cube">The summation cube I.</param>
        /// <returns>Returns the superpoly of I in p.</returns>
        public List<int> ComputeSuperpoly(int[] pubVarElement, List<int> maxterm)
        {
            int constant = 0;
            int coeff = 0;
            List<int> superpoly = new List<int>();
            int[] secVarElement = new int[settings.SecretVar];

            CubeAttack_LogMessage("Start deriving the algebraic structure of the superpoly", NotificationLevel.Info);

            // Compute the free term
            for (ulong i = 0; i < Math.Pow(2, maxterm.Count); i++)
            {
                if (stop)
                    return superpoly;

                for (int j = 0; j < maxterm.Count; j++)
                    pubVarElement[maxterm[j]] = (i & ((ulong)1 << j)) > 0 ? 1 : 0;
                constant ^= Blackbox((int[])pubVarElement.Clone(), (int[])secVarElement.Clone());
            }
            superpoly.Add(constant);
            CubeAttack_LogMessage("Constant term = " + (constant).ToString(), NotificationLevel.Info);

            // Compute coefficients
            for (int k = 0; k < settings.SecretVar; k++)
            {
                for (ulong i = 0; i < Math.Pow(2, maxterm.Count); i++)
                {
                    if (stop)
                        return superpoly;

                    secVarElement[k] = 1;
                    for (int j = 0; j < maxterm.Count; j++)
                        pubVarElement[maxterm[j]] = (i & ((ulong)1 << j)) > 0 ? 1 : 0;
                    coeff ^= Blackbox((int[])pubVarElement.Clone(), (int[])secVarElement.Clone());
                }
                superpoly.Add(constant ^ coeff);
                CubeAttack_LogMessage("Coefficient of x" + k + " = " + (constant ^ coeff), NotificationLevel.Info);
                coeff = 0;
                secVarElement[k] = 0;
            }
            return superpoly;
        }

        /// <summary>
        /// The function outputs a the superpolys, cube indexes and output bits.
        /// </summary>
        /// <param name="cubeIndexes">The cube indexes of the maxterm.</param>
        /// <param name="superpoly">The superpoly for the given cube indexes.</param>
        public void OutputSuperpolys(List<int> cubeIndexes, List<int> superpoly)
        {
            StringBuilder output = new StringBuilder(string.Empty);
            bool superpolyIsEmpty = true;
            bool flag = false;
            output.Append("Superpoly: ");
            if (superpoly[0] == 1)
            {
                output.Append("1");
                superpolyIsEmpty = false;
                flag = true;
            }
            for (int i = 1; i < superpoly.Count; i++)
                if (superpoly[i] == 1)
                {
                    if (flag)
                        output.Append("+x" + Convert.ToString(i - 1));
                    else
                        output.Append("x" + Convert.ToString(i - 1));
                    superpolyIsEmpty = false;
                    flag = true;
                }
            if (superpolyIsEmpty)
                output.Append("0");
            output.Append("   Cube Indexes: {");
            if (cubeIndexes.Count > 0)
            {
                cubeIndexes.Sort();
                for (int i = 0; i < cubeIndexes.Count - 1; i++)
                    output.Append(cubeIndexes[i] + ",");
                output.Append(cubeIndexes[cubeIndexes.Count - 1] + "}");
            }
            else
                output.Append(" }");

            // Output Bit Index if Trivium is Black Box
            if (settings.BlackBox == 1)
                output.Append("   Trivium Output Bit Index: " + (indexOutputBit + settings.TriviumRounds - 1) + "\n");
            else
                output.Append("\n");

            outputSuperpoly += output.ToString();
            OnPropertyChanged("OutputSuperpoly");
        }

        /// <summary>
        /// The function outputs the key bits.
        /// </summary>
        /// <param name="res">Result vector</param>
        public void OutputKey(Vector res)
        {
            StringBuilder output = new StringBuilder(string.Empty);
            for (int i=0; i<res.Length; i++)
                output.AppendLine("x" + i + " = " + res[i]);
            outputKeyBits = output.ToString();
        }

        /// <summary>
        /// Test if superpoly is already in matrix.
        /// </summary>
        /// <param name="superpoly">The superpoly of I in p.</param>
        /// <param name="matrix">An n x n matrix whose rows contain their corresponding superpolys.</param>
        /// <returns>A boolean value indicating if the superpoly is in the matrix or not.</returns>
        public bool InMatrix(List<int> superpoly, Matrix matrix)
        {
            bool isEqual = true;
            for (int i = 0; i < matrix.Rows; i++)
            {
                isEqual = true;
                for (int j = 0; j < superpoly.Count; j++)
                    if (matrix[i, j] != superpoly[j])
                        isEqual = false;
                if (isEqual)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Test if a maxterm is already known.
        /// </summary>
        /// <param name="cubeList">A list of cube indexes.</param>
        /// <param name="maxterm">The located maxterm.</param>
        /// <returns>A boolean value indicating if the maxterm is already in the list of cubes indexes or not.</returns>
        public bool MaxtermKnown(List<List<int>> cubeList, List<int> maxterm)
        {
            bool isEqual = true;
            for (int i = 0; i < cubeList.Count; i++)
            {
                isEqual = true;
                if (cubeList[i].Count == maxterm.Count)
                {
                    for (int j = 0; j < maxterm.Count; j++)
                        if (!cubeList[i].Contains(maxterm[j]))
                            isEqual = false;
                    if (isEqual)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Test if a superpoly is linear (BLR linearity test).
        /// </summary>
        /// <param name="maxterm">The located maxterm.</param>
        /// <returns>A boolean value indicating if the superpoly is probably linear or not.</returns>
        public bool IsSuperpolyLinear(int[] pubVarElement, List<int> maxterm)
        {
            Random rnd = new Random();
            int psLeft = 0;
            int psRight = 0;
            int[] vectorX = new int[settings.SecretVar];
            int[] vectorY = new int[settings.SecretVar];
            int[] vecXY = new int[settings.SecretVar];

            for (int k = 0; k < settings.LinTest; k++)
            {
                CubeAttack_LogMessage("Linearity test " + (k + 1) + " of " + settings.LinTest, NotificationLevel.Info);
                psLeft = 0;
                psRight = 0;

                // Choose vectors x and y at random
                for (int i = 0; i < settings.SecretVar; i++)
                {
                    vectorX[i] = rnd.Next(0, 2);
                    vectorY[i] = rnd.Next(0, 2);
                }

                pubVarElement = new int[settings.PublicVar];
                for (int i = 0; i < settings.SecretVar; i++)
                    vecXY[i] = (vectorX[i] ^ vectorY[i]);

                for (ulong i = 0; i < Math.Pow(2, maxterm.Count); i++)
                {
                    if (stop)
                        return false;

                    for (int j = 0; j < maxterm.Count; j++)
                        pubVarElement[maxterm[j]] = (i & ((ulong)1 << j)) > 0 ? 1 : 0;
                    psLeft ^= Blackbox((int[])pubVarElement.Clone(), new int[settings.SecretVar]) 
                            ^ Blackbox((int[])pubVarElement.Clone(), (int[])vectorX.Clone()) 
                            ^ Blackbox((int[])pubVarElement.Clone(), (int[])vectorY.Clone());
                    psRight ^= Blackbox((int[])pubVarElement.Clone(), (int[])vecXY.Clone());
                }
                if (psLeft != psRight)
                {
                    CubeAttack_LogMessage("Linearity test " + (k + 1) + " failed", NotificationLevel.Info);
                    return false;
                }

                if (stop)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Test if superpoly is a constant value.
        /// </summary>
        /// <param name="maxterm">The located maxterm.</param>
        /// <returns>A boolean value indicating if the superpoly is constant or not.</returns>
        public bool IsSuperpolyConstant(int[] pubVarElement, List<int> maxterm)
        {
            Random rnd = new Random();
            int[] vectorX = new int[settings.SecretVar];
            int flag = 0;
            int output = 0;
            int[] secVarElement = new int[settings.SecretVar];

            string outputCube = string.Empty;
            foreach (int element in maxterm)
                outputCube += "v" + element + " ";
            if(settings.ConstTest > 0)
                CubeAttack_LogMessage("Test if superpoly of subset " + outputCube + " is constant", NotificationLevel.Info);
            for (int i = 0; i < settings.ConstTest; i++)
            {
                for (int j = 0; j < settings.SecretVar; j++)
                    vectorX[j] = rnd.Next(0, 2);
                for (ulong j = 0; j < Math.Pow(2, maxterm.Count); j++)
                {
                    if (stop)
                        return false;

                    for (int k = 0; k < maxterm.Count; k++)
                        pubVarElement[maxterm[k]] = (j & ((ulong)1 << k)) > 0 ? 1 : 0;
                    output ^= Blackbox(pubVarElement, vectorX);
                }
                if (i == 0)
                    flag = output;
                if (flag != output)
                {
                    CubeAttack_LogMessage("Superpoly of subset " + outputCube + " is not constant", NotificationLevel.Info);
                    return false;
                }
                output = 0;

                if (stop)
                    return false;
            }
            if (settings.ConstTest > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Generates a random permutation of a finite set—in plain terms, for randomly shuffling the set.
        /// </summary>
        /// <param name="ilist">A List of values.</param>
        public static void Shuffle(List<int> ilist)
        {
            Random rand = new Random();
            int iIndex;
            int tTmp;
            for (int i = 1; i < ilist.Count; ++i)
            {
                iIndex = rand.Next(i + 1);
                tTmp = ilist[i];
                ilist[i] = ilist[iIndex];
                ilist[iIndex] = tTmp;
            }
        }

        /// <summary>
        /// Test if an n x m matrix contains n linearly independent vectors.
        /// </summary>
        /// <param name="A">n x m matrix.</param>
        /// <returns>A boolean value indicating if the matrix is regular or not.</returns>
        public bool IsLinearIndependent(Matrix A)
        {
            double maxval;
            int maxind;
            double temp;
            int Rang = 0;
            double[,] a = new double[A.Cols, A.Rows];

            for (int i = 0; i < A.Cols; i++)
                for (int j = 0; j < A.Rows; j++)
                    a[i, j] = A[j, i];

            for (int j = 0; j < A.Rows; j++)
            {
                // Find maximum
                maxval = a[j, j];
                maxind = j;
                for (int k = j; k < A.Cols; k++)
                {
                    if (a[k, j] > maxval)
                    {
                        maxval = a[k, j];
                        maxind = k;
                    }
                    if (-a[k, j] > maxval)
                    {
                        maxval = -a[k, j];
                        maxind = k;
                    }
                }

                if (maxval != 0)
                {
                    Rang++;
                    // Swap_Rows(j, maxind)
                    for (int k = j; k < A.Rows; k++)
                    {
                        temp = a[j, k];
                        a[j, k] = a[maxind, k];
                        a[maxind, k] = temp;
                    }

                    // Gauss elimination 
                    for (int i = j + 1; i < A.Cols; i++)
                        for (int k = j + 1; k < A.Rows; k++)
                            a[i, k] = a[i, k] - (a[i, j] / a[j, j] * a[j, k]);
                }
            }
            if (A.Rows == Rang)
                return true;
            else
                return false;
        }  

        /// <summary>
        /// Preprocessing Phase of the cube attack. 
        /// Implementation of the algorithm Random-Walk to find maxterms.
        /// </summary>
        public void PreprocessingPhase()
        {
            CubeAttack_LogMessage("Start preprocessing, try to find " + settings.SecretVar + " linearly independent superpolys", NotificationLevel.Info);

            indexOutputBit = settings.TriviumOutputBit;
            pubVarGlob = null;
            outputSuperpoly = string.Empty;
            superpolyMatrix = new Matrix(settings.SecretVar, settings.SecretVar + 1);
            listCubeIndexes = new List<List<int>>();
            outputBitIndex = new int[settings.SecretVar];

            int countSuperpoly = 0;
            Random rnd = new Random();
            int numberOfVariables = 0;
            List<int> chooseIndexI = new List<int>();
            Matrix matrixCheckLinearitySuperpolys = new Matrix(0, settings.SecretVar);
            List<int> superpoly = new List<int>();
            List<int> maxterm = new List<int>();
            List<List<int>> cubeList = new List<List<int>>();

            // Save all public variables indexes in a list 
            for (int i = 0; i < settings.PublicVar; i++)
                chooseIndexI.Add(i);

            // Find n maxterms and save their in the matrix
            while (countSuperpoly < settings.SecretVar)
            {
                if (stop)
                    return;
                else
                {
                    maxterm = new List<int>();
                    superpoly.Clear();

                    // Generate random size k between 1 and the number of public variables
                    numberOfVariables = rnd.Next(1, settings.MaxCube + 1);

                    // Permutation of the public variables
                    Shuffle(chooseIndexI);

                    // Construct cube of size k. Add k public variables to the cube
                    for (int i = 0; i < numberOfVariables; i++)
                        maxterm.Add(chooseIndexI[i]);

                    string outputCube = string.Empty;
                    foreach (int element in maxterm)
                        outputCube += "v" + element + " ";
                    CubeAttack_LogMessage("Start search for maxterm with subterm: " + outputCube, NotificationLevel.Info);
                    if (settings.TriviumOutputBit != indexOutputBit)
                    {
                        // User has changed Output Bit index, store new value
                        indexOutputBit = settings.TriviumOutputBit;

                        // Reset list of cube indexes, since a single maxterms can be associated with multiple superpolys from different outputs
                        cubeList = new List<List<int>>();
                    }
                    while (superpoly.Count == 0)
                    {
                        if (maxterm.Count == 0)
                        {
                            if (numberOfVariables < chooseIndexI.Count)
                            {
                                CubeAttack_LogMessage("Subset is empty, add variable v" + chooseIndexI[numberOfVariables], NotificationLevel.Info);
                                maxterm.Add(chooseIndexI[numberOfVariables]);
                                numberOfVariables++;
                            }
                            else
                                break;
                        }
                        if (MaxtermKnown(cubeList, maxterm))
                        {
                            // Maxterm is already known, break and restart with new subset
                            outputCube = string.Empty;
                            foreach (int element in maxterm)
                                outputCube += "v" + element + " ";
                            CubeAttack_LogMessage("Maxterm " + outputCube + " is already known, restart with new subset", NotificationLevel.Info);
                            break;
                        }

                        if (IsSuperpolyConstant(new int[settings.PublicVar], maxterm))
                        {
                            if (stop)
                                return;
                            else
                            {
                                CubeAttack_LogMessage("Superpoly is likely constant, drop variable v" + maxterm[0], NotificationLevel.Info);
                                maxterm.RemoveAt(0);
                            }
                        }
                        else if (!IsSuperpolyLinear(new int[settings.PublicVar], maxterm))
                        {
                            if (stop)
                                return;
                            else
                            {
                                CubeAttack_LogMessage("Superpoly is not linear", NotificationLevel.Info);
                                if (numberOfVariables < chooseIndexI.Count)
                                {
                                    if (maxterm.Count < settings.MaxCube)
                                    {
                                        CubeAttack_LogMessage("Add variable v" + chooseIndexI[numberOfVariables], NotificationLevel.Info);
                                        maxterm.Add(chooseIndexI[numberOfVariables]);
                                        numberOfVariables++;
                                    }
                                    else
                                        break;
                                }
                                else
                                    break;
                            }
                        }
                        else
                        {
                            if (stop)
                                return;
                            else
                            {
                                //CubeAttack_LogMessage("Superpoly is likely linear", NotificationLevel.Info);
                                cubeList.Add(maxterm);
                                outputCube = string.Empty;
                                foreach (int element in maxterm)
                                    outputCube += "v" + element + " ";
                                CubeAttack_LogMessage(outputCube + " is new maxterm", NotificationLevel.Info);
                                outputCube = string.Empty;
                                superpoly = ComputeSuperpoly(new int[settings.PublicVar], maxterm);
                                bool flag = false;
                                outputCube += "Superpoly: ";
                                if (superpoly[0] == 1)
                                {
                                    outputCube += "1";
                                    flag = true;
                                }
                                for (int i = 1; i < superpoly.Count; i++)
                                    if (superpoly[i] == 1)
                                    {
                                        if (flag)
                                            outputCube += "+x" + Convert.ToString(i - 1);
                                        else
                                            outputCube += "x" + Convert.ToString(i - 1);
                                        flag = true;
                                    }
                                outputCube += "   Cube Indexes: {";
                                if (maxterm.Count > 0)
                                {
                                    maxterm.Sort();
                                    for (int i = 0; i < maxterm.Count - 1; i++)
                                        outputCube += maxterm[i] + ",";
                                    outputCube += maxterm[maxterm.Count - 1] + "}";
                                }
                                else
                                    outputCube += " }";

                                // Output Bit Index if Trivium is Black Box
                                if (settings.BlackBox == 1)
                                    outputCube += "   Trivium Output Bit Index: " + (indexOutputBit + settings.TriviumRounds - 1);

                                CubeAttack_LogMessage(outputCube, NotificationLevel.Info);
                                break;
                            }
                        }
                    }//End while (superpoly.Count == 0)

                    if (!InMatrix(superpoly, superpolyMatrix))
                    {
                        List<int> superpolyWithoutConstant = new List<int>();
                        for (int i = 1; i < superpoly.Count; i++)
                            superpolyWithoutConstant.Add(superpoly[i]);

                        matrixCheckLinearitySuperpolys = matrixCheckLinearitySuperpolys.AddRow(superpolyWithoutConstant);
                        if (IsLinearIndependent(matrixCheckLinearitySuperpolys))
                        {
                            for (int j = 0; j < superpoly.Count; j++)
                                superpolyMatrix[countSuperpoly, j] = superpoly[j];

                            listCubeIndexes.Add(maxterm);
                            OutputSuperpolys(maxterm, superpoly);
                            outputBitIndex[countSuperpoly] = indexOutputBit;
                            countSuperpoly++;
                            CubeAttack_LogMessage("Found " + countSuperpoly + " of " + settings.SecretVar + " linearly independent superpolys", NotificationLevel.Info);
                            ProgressChanged((double)countSuperpoly / (double)settings.SecretVar, 1.0);
                        }
                        else
                            matrixCheckLinearitySuperpolys = matrixCheckLinearitySuperpolys.DeleteLastRow();
                    }

                    if (countSuperpoly == settings.SecretVar)
                        CubeAttack_LogMessage(settings.SecretVar + " linearly independent superpolys have been found, preprocessing phase completed", NotificationLevel.Info);
                }
            }//End while (countSuperpoly < settings.SecretVar)
        }//End PreprocessingPhase

        /// <summary>
        /// Online Phase of the cube attack.
        /// </summary>
        /// <param name="superpolyMatrix">An n x n matrix which contains the superpolys.</param>
        /// <param name="listCubeIndexes">A list of lists of cube indexes.</param>
        public void OnlinePhase(Matrix superpolyMatrix, List<List<int>> listCubeIndexes)
        {
            if (superpolyMatrix == null || listCubeIndexes == null)
                CubeAttack_LogMessage("Preprocessing phase has to be executed first", NotificationLevel.Error);
            else
            {
                CubeAttack_LogMessage("Start online phase", NotificationLevel.Info);

                outputSuperpoly = string.Empty;
                int[] pubVarElement = new int[settings.PublicVar];

                if (pubVarGlob != null)
                {
                    for (int i = 0; i < settings.PublicVar; i++)
                        pubVarElement[i] = pubVarGlob[i];
                }

                Vector b = new Vector(settings.SecretVar);
                StringBuilder output = new StringBuilder(string.Empty);
                bool flag = false;
                string logOutput = string.Empty;

                for (int i = 0; i < listCubeIndexes.Count; i++)
                {
                    flag = false;
                    logOutput = string.Empty;
                    output.Append("Maxterm Equation: ");
                    if (superpolyMatrix[i, 0] == 1)
                    {
                        output.Append("1");
                        logOutput += "1";
                        flag = true;
                    }
                    for (int j = 1; j < superpolyMatrix.Cols; j++)
                    {
                        if (superpolyMatrix[i, j] == 1)
                        {
                            if (flag)
                            {
                                output.Append("+x" + Convert.ToString(j - 1));
                                logOutput += "+x" + Convert.ToString(j - 1);
                            }
                            else
                            {
                                output.Append("x" + Convert.ToString(j - 1));
                                logOutput += "x" + Convert.ToString(j - 1);
                            }
                            flag = true;
                        }
                    }
                    CubeAttack_LogMessage("Compute value of maxterm equation " + logOutput, NotificationLevel.Info);

                    for (ulong k = 0; k < Math.Pow(2, listCubeIndexes[i].Count); k++)
                    {
                        if (stop)
                            return;
                        for (int l = 0; l < listCubeIndexes[i].Count; l++)
                            pubVarElement[listCubeIndexes[i][l]] = (k & ((ulong)1 << l)) > 0 ? 1 : 0;
                        try
                        {
                            switch(settings.BlackBox)
                            {
                                case 0:
                                    // Online phase BooleanFunctionParser
                                    bool[] vBool = new bool[pubVarElement.Length];
                                    for (int l = 0; l < pubVarElement.Length; l++)
                                        vBool[l] = Convert.ToBoolean(pubVarElement[l]);
                                    b[i] ^= ParserOutput.SolveFunction(null, vBool, 2);
                                    break;
                                case 1:
                                    // Online phase Trivium
                                    b[i] ^= TriviumOutput.GenerateTriviumKeystream(pubVarElement, null, outputBitIndex[i], settings.TriviumRounds, false);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            CubeAttack_LogMessage("Error: " + ex, NotificationLevel.Error);
                        }
                    }
                    for (int j = 0; j < settings.PublicVar; j++)
                        pubVarElement[j] = 0;

                    outputSuperpoly += output.Append(" = " + b[i] + "\n").ToString();
                    OnPropertyChanged("OutputSuperpoly");
                    ProgressChanged((double)i / (double)listCubeIndexes.Count, 1.0);
                    outputSuperpoly = string.Empty;
                }

                if (listCubeIndexes.Count == settings.SecretVar)
                {
                    CubeAttack_LogMessage("Solve system of equations", NotificationLevel.Info);
                    for (int i = 0; i < settings.SecretVar; i++)
                        b[i] ^= superpolyMatrix[i, 0];
                    // Delete first column and invert
                    OutputKey(superpolyMatrix.DeleteFirstColumn().Inverse() * b);
                    OnPropertyChanged("OutputKeyBits");
                    CubeAttack_LogMessage("Key bits successfully discovered, online phase completed", NotificationLevel.Info);
                }
                else
                    CubeAttack_LogMessage("Not enough linearly independent superpolys have been found to discover all secret bits !", NotificationLevel.Info);
            }
        }

        /// <summary>
        /// User-Mode to set public bit values manually.
        /// </summary>
        public void SetPublicBitsPhase()
        {
            outputSuperpoly = string.Empty;
            outputKeyBits = string.Empty;
            superpolyMatrix = new Matrix(settings.SecretVar, settings.SecretVar + 1);
            listCubeIndexes = new List<List<int>>();
            pubVarGlob = new int[settings.PublicVar];
            List<int> maxterm = new List<int>();
            bool fault = false;

            if (settings.TriviumOutputBit != indexOutputBit)
                indexOutputBit = settings.TriviumOutputBit;

            if (settings.SetPublicBits.Length != settings.PublicVar)
                CubeAttack_LogMessage("Input public bits must have size " + settings.PublicVar + " (Currently: " + settings.SetPublicBits.Length + " )", NotificationLevel.Error);
            else
            {
                for (int i = 0; i < settings.SetPublicBits.Length; i++)
                {
                    switch (settings.SetPublicBits[i])
                    {
                        case '0':
                            pubVarGlob[i] = 0;
                            break;
                        case '1':
                            pubVarGlob[i] = 1;
                            break;
                        case '*':
                            maxterm.Add(i);
                            break;
                        default:
                            fault = true;
                            break;
                    }
                }
                if (fault)
                    CubeAttack_LogMessage("The input public bits does not consists only of characters : \'0\',\'1\',\'*\' !", NotificationLevel.Error);
                else
                {
                    if (maxterm.Count > 0)
                    {
                        if (!IsSuperpolyConstant(pubVarGlob, maxterm))
                            if (IsSuperpolyLinear(pubVarGlob, maxterm))
                            {
                                List<int> superpoly = ComputeSuperpoly(pubVarGlob, maxterm);
                                if (!stop)
                                {
                                    for (int i = 0; i < superpoly.Count; i++)
                                        superpolyMatrix[0, i] = superpoly[i];
                                    listCubeIndexes.Add(maxterm);
                                    OutputSuperpolys(maxterm, superpoly);
                                }
                            }
                            else
                            {
                                if(!stop)
                                    CubeAttack_LogMessage("The corresponding superpoly is not a linear polynomial !", NotificationLevel.Info);
                            }
                        else
                            CubeAttack_LogMessage("The corresponding superpoly is constant !", NotificationLevel.Info);
                    }
                    else
                    {
                        StringBuilder output = new StringBuilder(string.Empty);
                        output.Append("Black box output bit: " + Blackbox(pubVarGlob, new int[settings.SecretVar]));
                        outputSuperpoly += output.ToString();
                        OnPropertyChanged("OutputSuperpoly");
                        CubeAttack_LogMessage("Black box output bit: " + Blackbox(pubVarGlob, new int[settings.SecretVar]), NotificationLevel.Info);
                    }
                }
            }
        }

        public void Pause()
        {
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Does the actual CubeAttack processing
        /// </summary>
        private void ProcessCubeAttack(CubeAttackMode mode)
        {
            switch (mode)
            {
                case CubeAttackMode.preprocessing:
                    PreprocessingPhase();
                    break;
                case CubeAttackMode.online:
                    OnlinePhase(superpolyMatrix, listCubeIndexes);
                    break;
                case CubeAttackMode.setPublicBits:
                    SetPublicBitsPhase();
                    break;
            }
        }

        /// <summary>
        /// Handles log messages
        /// </summary>
        private void CubeAttack_LogMessage(string msg, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(msg, this, logLevel));
            }
        }

        #region IControlEncryption Members

        private IControlSolveFunction parserOutput;
        [PropertyInfo(Direction.ControlMaster, "Master for BFP", "Master for BFP (SolveFunction)", "", DisplayLevel.Beginner)]
        public IControlSolveFunction ParserOutput
        {
            get { return parserOutput; }
            set
            {
                if (value != null)
                    parserOutput = value;
            }
        }

        private IControlTrivium triviumOutput;
        [PropertyInfo(Direction.ControlMaster, "Master for Trivium", "Master for Trivium", "", DisplayLevel.Beginner)]
        public IControlTrivium TriviumOutput
        {
            get { return triviumOutput; }
            set
            {
                if (value != null)
                    triviumOutput = value;
            }
        }

        #endregion

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
}
