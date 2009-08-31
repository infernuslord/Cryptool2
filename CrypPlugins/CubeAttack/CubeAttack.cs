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
// reference to the BFPController interface
using Cryptool.BooleanFunctionParserController;

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
        //private int inputBlackBox;
        private string outputMaxterm;
        private string outputKeyBits;
        //private bool[] outputPublicBit = new bool[3];
        //private bool[] outputSecretBit = new bool[3];
        private enum CubeAttackMode { findMaxterms, setPublicBits };
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();
        
        #endregion

        #region Public Variables
        public bool lastEventwasInputBB = false;
        public bool inputBBFlag = true;
        //public bool inputBBFlag = false;
        #endregion

        #region Properties (Inputs/Outputs)

        /*[PropertyInfo(Direction.Input, 
            "Black box input", 
            "The output bit from the black box", 
            "",
            false, 
            false, 
            DisplayLevel.Beginner, 
            QuickWatchFormat.Text, 
            null)]
        public int InputBlackBox
        {
            get {
                return this.inputBlackBox; }
            set
            {
                
                    this.inputBlackBox = value;
                    OnPropertyChanged("InputBlackBox");
            }
        }*/

        /*[PropertyInfo(Direction.Output, 
            "Public bits", 
            "Array of public bits", 
            "", 
            false, 
            false, 
            DisplayLevel.Beginner, 
            QuickWatchFormat.Text, 
            null)]
        public bool[] OutputPublicBit
        {
            get { return this.outputPublicBit; }
            set
            {
                outputPublicBit = value;
                OnPropertyChanged("OutputPublicBit");
            }
        }

        [PropertyInfo(Direction.Output, 
            "Secret bits", 
            "Array of secret bits", 
            "", 
            false, 
            false, 
            DisplayLevel.Beginner, 
            QuickWatchFormat.Text, 
            null)]
        public bool[] OutputSecretBit
        {
            get { return this.outputSecretBit; }
            set
            {
                outputSecretBit = value;
                OnPropertyChanged("OutputSecretBit");
            }
        }
        */
        [PropertyInfo(Direction.OutputData, 
            "Maxterm output", 
            "Outputs the located maxterms.", 
            "", 
            false, 
            false, 
            DisplayLevel.Beginner, 
            QuickWatchFormat.Text, 
            null)]
        public CryptoolStream OutputMaxterm
        {
            get
            {
                if (outputMaxterm != null)
                {
                    CryptoolStream cs = new CryptoolStream();
                    listCryptoolStreamsOut.Add(cs);
                    cs.OpenRead(this.GetPluginInfoAttribute().Caption, Encoding.Default.GetBytes(outputMaxterm.ToCharArray()));
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
            "This output provides the result of the secret key", 
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
                    cs.OpenRead(this.GetPluginInfoAttribute().Caption, Encoding.Default.GetBytes(outputKeyBits.ToCharArray()));
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
        public void FindMaxterms()
        {
            ProcessCubeAttack(CubeAttackMode.findMaxterms);
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
            if (lastEventwasInputBB)
                lastEventwasInputBB = false;
            else
            {
                switch (settings.Action)
                {
                    case 0:
                        FindMaxterms();
                        break;
                    case 1:
                        SetPublicBits();
                        break;
                }
                ProgressChanged(1.0, 1.0);
            }
        }

        // Returns the output bit of the master polynomial P
        public int Blackbox(int[] v, int[] x)
        {
            // ****** Beispiel Master Polynome ******
            //return v[0] & x[0] ^ v[0] & x[1] ^ v[1] & x[1] ^ v[0] ^ x[1] ^ v[0] & v[1] ^ v[1] & x[0] & x[1] ^ 1;
            //return v[0] & x[0] ^ v[0] & x[1] ^ v[1] & x[1] ^ v[0] ^ x[1] ^ v[0] & v[1] ^ v[1] & x[0] & x[1] ^ 1;

            //return v[0] & v[1] & x[0] ^ v[0] & v[1] & x[1] ^ v[2] & x[0] & x[2] ^ v[1] & x[2] ^ v[0] & x[0] ^ v[0] & v[1] ^
              //   x[0] & x[2] ^ v[1] ^ x[2] ^ 1;

            //return v[0] & v[1] & x[0] ^ v[0] & v[1] & x[2] ^ v[2] & x[0] & x[1] ^ v[0] & x[0] ^ v[0] & x[1] ^ v[1] & x[2] ^
               //  v[2] & x[1] ^ x[0] & x[2] ^ v[0] ^ v[1] ^ x[0] ^ x[1] ^ 1;

            return v[0] & v[1] & x[2] ^ v[2] & v[4] & x[1] ^ v[0] & v[3] & x[3] ^ v[0] & v[3] & x[2] ^
                v[1] & v[2] & x[1] ^ v[3] & v[4] & x[4] ^ v[2] & v[4] & x[3] ^ v[2] & v[4] & x[4] ^
                v[2] & v[4] & x[0] ^ v[2] & v[4] & x[2] ^ v[0] & v[1] & v[2] ^ v[0] & v[1] ^ v[3] & x[1] ^
                v[3] & v[4] ^ x[1] & x[2] ^ x[1] & x[3] ^ v[4] ^ x[2] ^ x[1] ^ x[0] ^ v[1] & x[4]; 
            // ****** Ende Beispiel Master Polynome ****** */


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
	        for (i = 0; i < 13; i++)
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
            return keyOutput[12]; 
            // End Trivium */
        }

        /// <summary>
        /// Searches for the free term and the coefficients in the superpoly
        /// </summary>
        /// <param name="secVarElement">Secret values.</param>
        /// <param name="pubVarElement">Public values.</param>
        /// <param name="cube">Cube I.</param>
        /// <returns>Returns the superpoly of I in p.</returns>
        public List<int> ComputePSI(List<int> cube)
        {
            int constant = 0;
            int coeff = 0;
            List<int> superpoly = new List<int>();
            int[] pubVarElement = new int[settings.PublicVar];
            int[] secVarElement = new int[settings.SecretVar];

            CubeAttack_LogMessage("Computing superpoly...", NotificationLevel.Info);

            // Compute the free term
            for (int i = 0; i < Math.Pow(2, cube.Count); i++)
            {
                for (int j = 0; j < cube.Count; j++)
                    pubVarElement[cube[j]] = (i & (1 << j)) > 0 ? 1 : 0;
                constant ^= Blackbox((int[])pubVarElement.Clone(), (int[])secVarElement.Clone());
            }
            superpoly.Add(constant);
            CubeAttack_LogMessage("c = " + (constant).ToString(), NotificationLevel.Info);

            // Compute aj of I
            for (int k = 0; k < settings.SecretVar; k++)
            {
                for (int i = 0; i < Math.Pow(2, cube.Count); i++)
                {
                    secVarElement[k] = 1;
                    for (int j = 0; j < cube.Count; j++)
                        pubVarElement[cube[j]] = (i & (1 << j)) > 0 ? 1 : 0;
                    coeff ^= Blackbox((int[])pubVarElement.Clone(), (int[])secVarElement.Clone());
                }
                superpoly.Add(constant ^ coeff);
                CubeAttack_LogMessage("a" + k + " = " + (constant ^ coeff).ToString(), NotificationLevel.Info);
                coeff = 0;
                secVarElement[k] = 0;
            }
            return superpoly;
        }

        /// <summary>
        /// Outputs the maxterms.
        /// </summary>
        /// <param name="cube">The summation Cube I.</param>
        /// <param name="superpoly">The superpoly of I in p.</param>
        public void OutputMaxterms(List<int> cube, List<int> superpoly)
        {
            StringBuilder output = new StringBuilder(string.Empty);
            bool superpolyIsEmpty = true;
            bool flag = false;
            output.Append("Maxterm Equation: ");
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
            if (cube.Count > 0)
            {
                for (int i = 0; i < cube.Count - 1; i++)
                    output.Append(cube[i] + ",");
                output.AppendLine(cube[cube.Count - 1] + "}");
            }
            else
                output.Append(" }\n");
            outputMaxterm += output.ToString();
            OnPropertyChanged("OutputMaxterm");
        }

        /// <summary>
        /// Outputs the key bits.
        /// </summary>
        /// <param name="res">Vector with the secret bits.</param>
        public void OutputKB(Vector res)
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
        public bool IsInMatrix(List<int> superpoly, Matrix matrix)
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

        public bool IsCubeKnown(List<int> cube, Matrix mCube)
        {
            bool isEqual = true;
            for (int i = 0; i < mCube.Rows; i++)
            {
                isEqual = true;
                for (int j = 0; j < cube.Count; j++)
                    if (mCube[i, j] != cube[j])
                        isEqual = false;
                if (isEqual)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Test if superpoly is linear (BLR linearity test).
        /// </summary>
        /// <param name="cube">The summation Cube I.</param>
        /// <returns>A boolean value indicating if the superpoly is probably linear or not.</returns>
        public bool IsLinear(List<int> cube)
        {
            Random rnd = new Random();
            int psLeft = 0;
            int psRight = 0;
            int[] pubVarElement = new int[settings.PublicVar];
            int[] vectorX = new int[settings.SecretVar];
            int[] vectorY = new int[settings.SecretVar];
            int[] vecXY = new int[settings.SecretVar];

            for (int k = 0; k < settings.LinTest; k++)
            {
                //CubeAttack_LogMessage("Linearly Test " + k.ToString() + ": ", NotificationLevel.Info);
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

                for (int i = 0; i < Math.Pow(2, cube.Count); i++)
                {
                    for (int j = 0; j < cube.Count; j++)
                        pubVarElement[cube[j]] = (i & (1 << j)) > 0 ? 1 : 0;
                    psLeft ^= Blackbox((int[])pubVarElement.Clone(), new int[settings.SecretVar]) 
                            ^ Blackbox((int[])pubVarElement.Clone(), (int[])vectorX.Clone()) 
                            ^ Blackbox((int[])pubVarElement.Clone(), (int[])vectorY.Clone());
                    psRight ^= Blackbox((int[])pubVarElement.Clone(), (int[])vecXY.Clone());
                }
                if (psLeft != psRight)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Test if superpoly is constant.
        /// </summary>
        /// <param name="cube">The summation Cube I.</param>
        /// <returns>A boolean value indicating if the superpoly is constant or not.</returns>
        public bool IsSuperpolyConstant(List<int> cube)
        {
            Random rnd = new Random();
            int[] vectorX = new int[settings.SecretVar];
            int flag = 0;
            int output = 0;
            int[] secVarElement = new int[settings.SecretVar];
            int[] pubVarElement = new int[settings.PublicVar];

            for (int i = 0; i < settings.SecretVar; i++)
            {
                for (int j = 0; j < settings.SecretVar; j++)
                    vectorX[j] = rnd.Next(0, 2);
                for (int j = 0; j < Math.Pow(2, cube.Count); j++)
                {
                    for (int k = 0; k < cube.Count; k++)
                        pubVarElement[cube[k]] = (j & (1 << k)) > 0 ? 1 : 0;
                    output ^= Blackbox(pubVarElement, vectorX);
                }
                if (i == 0)
                    flag = output;
                if (flag != output)
                    return false;
                output = 0;
            }
            return true;

            /*for (int i = 0; i < Math.Pow(2, settings.SecretVar); i++)
            {
                for (int j = 0; j < settings.SecretVar; j++)
                    secVarElement[j] = (i & (1 << j)) > 0 ? 1 : 0;
                for (int j = 0; j < Math.Pow(2, cube.Count); j++)
                {
                    for (int k = 0; k < cube.Count; k++)
                        pubVarElement[cube[k]] = (j & (1 << k)) > 0 ? 1 : 0;
                    output ^= Blackbox(pubVarElement, secVarElement);
                }
                if (i == 0)
                    flag = output;
                if (flag != output)
                    return false;
                output = 0;
            }
            return true; */
        }

        /// <summary>
        /// Generates a random permutation of a finite set—in plain terms, for randomly shuffling the set.
        /// </summary>
        /// <param name="cube">A List of values.</param>
        public static void Shuffle<T>(List<int> ilist)
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

        public bool IsLinearIndependent(Matrix A)
        {
            double[,] a = new double[A.Cols, A.Rows];

            for (int i = 0; i < A.Cols; i++)
                for (int j = 0; j < A.Rows; j++)
                    a[i, j] = A[j, i];

            double maxval;
            int maxind;
            double temp;
            int Rang = 0;
            int zeilen = A.Cols;
            int spalten = A.Rows;

            for (int j = 0; j < spalten; j++)
            {
                // Find maximum
                maxval = a[j, j];
                maxind = j;
                for (int k = j; k < zeilen; k++)
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
                    for (int k = j; k < spalten; k++)
                    {
                        temp = a[j, k];
                        a[j, k] = a[maxind, k];
                        a[maxind, k] = temp;
                    }

                    // Gauss elimination 
                    for (int i = j + 1; i < zeilen; i++)
                        for (int k = j + 1; k < spalten; k++)
                            a[i, k] = a[i, k] - (a[i, j] / a[j, j] * a[j, k]);
                }
            }
            if (A.Rows == Rang)
                return true;
            else
                return false;
        }  

        /// <summary>
        /// Preprocessing Phase of the cube attack. The main challenge is to find maxterms.
        /// </summary>
        public void FindMaxtermsPhase()
        {
            outputMaxterm = string.Empty;
            int countMaxterms = 0;
            Random rnd = new Random();
            int numberOfVariables = 0;
            List<int> chooseIndexI = new List<int>();
            Matrix mSuperpoly = new Matrix(settings.SecretVar, settings.SecretVar + 1);
            Matrix mCube = new Matrix(settings.SecretVar, settings.SecretVar);
            Matrix mCheckLinearity = new Matrix(0, settings.SecretVar);
            List<int> superpoly = new List<int>();
            List<int> cube = new List<int>();
            Vector freeTerms = new Vector(settings.SecretVar);
            List<List<int>> cubeIndex = new List<List<int>>();
            int countRuns = 0;

            // Save all public variables indexes in a list 
            for (int i = 0; i < settings.PublicVar; i++)
                chooseIndexI.Add(i);

            // Find n maxterms and save their in the matrix
            while (countMaxterms < settings.SecretVar)
            {
                // Maybe all maxterms have been found ? 
                if (countRuns > 300)
                    break;
                countRuns++;
                
                cube = new List<int>();
                superpoly.Clear();

                // Generate random size k between 1 and the number of public variables
                numberOfVariables = rnd.Next(1, settings.MaxCube + 1);
                Shuffle<int>(chooseIndexI);
                
                for (int i = 0; i < numberOfVariables; i++)
                    cube.Add(chooseIndexI[i]);

                int count = 0;
                while (superpoly.Count == 0)
                {
                    if (count > 200)
                        break;
                    count++;
                    if (IsSuperpolyConstant(cube) && (cube.Count != 0))
                        cube.RemoveAt(0);
                    else if (!IsLinear(cube))
                    {
                        if (numberOfVariables < chooseIndexI.Count)
                        {
                            cube.Add(chooseIndexI[numberOfVariables]);
                            numberOfVariables++;
                        }
                        else
                            break;
                    }
                    else
                    {
                        if(cube.Count > 0) // brauche Funktion IsCubeKnown(cube, mCube), mCube muss n x n Matrix sein
                            //if (!IsCubeKnown(cube, mCube))
                            {
                                //for (int i = 0; i < cube.Count; i++)
                                    //mCube[countMaxterms, i] = cube[i]; 
                                superpoly = ComputePSI(cube); 
                            }
                    }
                }
                
                if (!IsInMatrix(superpoly, mSuperpoly))
                {
                    List<int> superpolyWithoutConstant = new List<int>();
                    for (int i = 1; i < superpoly.Count; i++)
                        superpolyWithoutConstant.Add(superpoly[i]);

                    mCheckLinearity = mCheckLinearity.AddRow(superpolyWithoutConstant);
                    if (IsLinearIndependent(mCheckLinearity))
                    {
                        for (int j = 0; j < superpoly.Count; j++)
                            mSuperpoly[countMaxterms, j] = superpoly[j];

                        cubeIndex.Add(cube);
                        countMaxterms++;
                        OutputMaxterms(cube, superpoly);
                        ProgressChanged((double)countMaxterms / (double)settings.SecretVar, 1.0);
                    }
                    else
                        mCheckLinearity = mCheckLinearity.DeleteLastRow();      
                }
            }

            if (countMaxterms == settings.SecretVar)
            {
                // Save the free terms of the superpolys in a vector
                for (int i = 0; i < settings.SecretVar; i++)
                    freeTerms[i] = mSuperpoly[i, 0];
                mSuperpoly = mSuperpoly.DeleteFirstColumn();

                try
                {
                    mSuperpoly = mSuperpoly.Inverse();
                    if (settings.OnlinePhase)
                        OnlinePhase(mSuperpoly, freeTerms, cubeIndex);
                }
                catch (Exception exception)
                {
                    GuiLogMessage(exception.Message, NotificationLevel.Error);
                }
            }
            else
                CubeAttack_LogMessage(countMaxterms.ToString() + " maxterms have been found !", NotificationLevel.Info);
        }

        /// <summary>
        /// Online Phase of the cube attack.
        /// </summary>
        /// <param name="mSuperpoly">An n x n matrix which contains the superpolys (without the free terms).</param>
        /// <param name="b">Vector which contains the corresponding free terms of the superpolys.</param>
        /// <param name="cubeIndex">A list of lists of cube indices.</param>
        public void OnlinePhase(Matrix mSuperpoly, Vector b, List<List<int>> cubeIndex)
        {
            int[] secretBits = new int[settings.SecretVar];
            secretBits[0] = 1; secretBits[1] = 1; //secretBits[2] = 0; secretBits[3] = false; secretBits[4] = true;
            int[] secVarElement = new int[settings.SecretVar];
            int[] pubVarElement = new int[settings.PublicVar];
            
            for (int i = 0; i < settings.SecretVar; i++)
                secVarElement[i] = secretBits[i];
            for (int i = 0; i < settings.SecretVar; i++)
            {
                for (int j = 0; j < settings.PublicVar; j++)
                    pubVarElement[j] = 0;
                for (int k = 0; k < Math.Pow(2, cubeIndex[i].Count); k++)
                {
                    for (int l = 0; l < cubeIndex[i].Count; l++)
                        pubVarElement[cubeIndex[i][l]] = (k & (1 << l)) > 0 ? 1 : 0;
                    b[i] ^= Blackbox(pubVarElement, secVarElement);
                }
            }
            OutputKB(mSuperpoly * b);
            OnPropertyChanged("OutputKeyBits");
        }

        /// <summary>
        /// In this mode the user is allowed to set the summation cube manually.
        /// </summary>
        public void SetPublicBitsPhase()
        {
            outputMaxterm = string.Empty;
            outputKeyBits = string.Empty;
            int[] secVari = new int[settings.SecretVar];
            int[] pubVari = new int[settings.PublicVar];
            List<int> cube = new List<int>();
            bool fault = false;

            if (settings.SetPublicBits.Length != settings.PublicVar)
                CubeAttack_LogMessage("Input public bits must have size " + settings.PublicVar + " !", NotificationLevel.Error);
            else
            {
                for (int i = 0; i < settings.SetPublicBits.Length; i++)
                {
                    switch (settings.SetPublicBits[i])
                    {
                        case '0':
                            pubVari[i] = 0;
                            break;
                        case '1':
                            pubVari[i] = 1;
                            break;
                        case '*':
                            cube.Add(i);
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
                    if (cube.Count > 0)
                    {
                        if (IsLinear(cube))
                        {
                            List<int> superpoly = ComputePSI(cube);
                            OutputMaxterms(cube, superpoly);
                        }
                        else
                            CubeAttack_LogMessage("The corresponding superpoly is not a linear polynomial !", NotificationLevel.Info);
                    }
                    else
                    {
                        CubeAttack_LogMessage("Summation cube must contain at least one variable !", NotificationLevel.Error);
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
                case CubeAttackMode.findMaxterms:
                    FindMaxtermsPhase();
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

        private IControlSolveFunction testProperty;
        [PropertyInfo(Direction.ControlMaster, "Test IControlSolveFunction Master", "Tooltip1", "", DisplayLevel.Beginner)]
        public IControlSolveFunction TestProperty
        {
            get { return testProperty; }
            set
            {
                if (value != null)
                {
                    testProperty = value;
                    try
                    {
                        // here comes the function to be solved, now just a dummy
                        string function = "";
                        // the result of the function
                        int result = -1;
                        // the data to fill the functin variables
                        bool[] data = new bool[] { true, true };
                        // the result is computed by calling the SolveFunction function with parameters
                        // the SolveFunction is also implemented by the slave, the BooleanFunctionParser
                        result = testProperty.SolveFunction(function, data);
                        GuiLogMessage("Rückgabewert: " + result, NotificationLevel.Info);
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage("Problem: " + ex, NotificationLevel.Error);
                    }
                }
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
