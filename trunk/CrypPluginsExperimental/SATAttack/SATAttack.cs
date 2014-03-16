using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.IO;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cryptool.Plugins.SATAttack
{
    [Author("Max Brandi", "max.brandi@rub.de", null, null)]
    [PluginInfo("SATAttack.Properties.Resources", "PluginCaption", "PluginDescription", "SATAttack/Documentation/doc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class SATAttack : ICrypComponent
    {
        #region Private Variables

        private readonly SATAttackSettings settings = new SATAttackSettings();
        private CStreamWriter outputStream;
        private CStreamWriter cbmcOutputStream;
        private CStreamWriter satSolverOutputStream;
        private StringBuilder cbmcOutputString;
        private StringBuilder satSolverOutputString;
        private Encoding encoding = Encoding.UTF8;
        private string pluginDataPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            + "\\Data\\SATAttack\\";
        string codefileName = "codefile.c";

        private Process cbmcProcess;
        private Process satSolverProcess;

        StringBuilder outputStringBuilder = new StringBuilder();
        
        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputDataStreamTooltip", false)]
        public ICryptoolStream InputStream
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "CbmcOutputStream", "CbmcOutputStreamTooltip", false)]
        public ICryptoolStream CbmcOutputStream
        {
            get
            {
                return cbmcOutputStream;
            }
            set
            {
                // empty
            }
        }

        [PropertyInfo(Direction.OutputData, "SatSolverOutputStream", "SatSolverOutputStreamTooltip", false)]
        public ICryptoolStream SatSolverOutputStream
        {
            get
            {
                return satSolverOutputStream;
            }
            set
            {
                // empty
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputDataStreamTooltip", true)]
        public ICryptoolStream OutputStream
        {
            get
            {
                return outputStream;
            }
            set
            {
                // empty
            }
        }

        #endregion

        public void Execute()
        {
            ProgressChanged(0, 100);

            #region prepare
            
            /* reset output string */
            outputStringBuilder = null;
            outputStringBuilder = new StringBuilder();

            /* reset cbmc output stream */
            cbmcOutputString = null;
            cbmcOutputString = new StringBuilder();

            /* reset sat solver output stream */
            satSolverOutputString = null;
            satSolverOutputString = new StringBuilder();

            #endregion

            #region cbmc

            /* get the file which contains the C code */
            string codefilePath = GetCodefilePath();

            string cbmcMainFunctionName = settings.MainFunctionName;

            /* measure time */
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Reset();

            stopwatch.Start();

            if (CallCbmcProcess(codefilePath, cbmcMainFunctionName) != 0)
            {
                /* write info to output stream */
                outputStringBuilder.Append("failed!" + Environment.NewLine);
                outputStream = new CStreamWriter();
                outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
                outputStream.Close();
                OnPropertyChanged("OutputStream");

                GuiLogMessage("Cbmc process returned with an error.", NotificationLevel.Error);
                return;
            }

            stopwatch.Stop();
            TimeSpan cbmcTime = stopwatch.Elapsed;

            GuiLogMessage(String.Format("Cbmc process returned after {0} seconds.", cbmcTime.ToString("s'.'fff")), NotificationLevel.Info);

            /* write info to output stream */
            outputStringBuilder.AppendFormat(Environment.NewLine + "Cbmc process returned successfully after {0} seconds." + Environment.NewLine, cbmcTime.ToString("s'.'fff"));
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            string inputMappingFilePath = "output.inputMapping.txt";
            string outputMappingFilePath = "output.outputMapping.txt";
            string outputCnfFilePath = "output.cnf.txt";

            #endregion cbmc

            ProgressChanged(33, 100);

            #region cnf encoding and options

            #region hash encoding

            /* append encoding of the output hash to the cnf */
            if (encodeHashInCnf(outputMappingFilePath, outputCnfFilePath) != 0)
            {
                /* write info to output stream */
                outputStringBuilder.Append("failed!" + Environment.NewLine);
                outputStream = new CStreamWriter();
                outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
                outputStream.Close();
                OnPropertyChanged("OutputStream");

                GuiLogMessage("Hash encoding returned with an error.", NotificationLevel.Error);
                return;
            }

            #endregion

            #region second preimage encoding

            /* append encoding of second preimage */
            if (settings.AttackMode == 1)
            {
                if (encodeSecondPreimageInCnf(inputMappingFilePath, outputCnfFilePath) != 0)
                {
                    /* write info to output stream */
                    outputStringBuilder.Append("failed!");
                    outputStream = new CStreamWriter();
                    outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
                    outputStream.Close();
                    OnPropertyChanged("OutputStream");

                    GuiLogMessage("Encoding of the second preimage bits returned with an error.", NotificationLevel.Error);
                    return;
                }
            }

            #endregion

            #region guessed bits encoding

            /* append encoding of guessed bits to the cnf, if parallelized attack is selected */
            if (settings.GuessBits)
            {
                if (encodeGuessedBitsInCnf(inputMappingFilePath, outputCnfFilePath) != 0)
                {
                    /* write info to output stream */
                    outputStringBuilder.Append("failed!" + Environment.NewLine);
                    outputStream = new CStreamWriter();
                    outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
                    outputStream.Close();
                    OnPropertyChanged("OutputStream");

                    GuiLogMessage("Encoding of the guessed bits returned with an error.", NotificationLevel.Error);
                    return;
                }
            }

            #endregion

            #region copy cnf

            if (settings.CnfFileName != "")
            {
                /* write info to output stream */
                outputStringBuilder.AppendFormat("Copying CNF to {0}... ", settings.CnfFileName);
                outputStream = new CStreamWriter();
                outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
                outputStream.Close();
                OnPropertyChanged("OutputStream");

                File.Copy(outputCnfFilePath, settings.CnfFileName);

                /* write info to output stream */
                outputStringBuilder.Append("successful!" + Environment.NewLine);
                outputStream = new CStreamWriter();
                outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
                outputStream.Close();
                OnPropertyChanged("OutputStream");
            }

            #endregion

            #region only cnf output

            if (settings.OnlyCnfOutput)
            {
                /* write info to output stream */
                outputStringBuilder.Append("Skipping Sat solver process..." + Environment.NewLine);
                outputStream = new CStreamWriter();
                outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
                outputStream.Close();
                OnPropertyChanged("OutputStream");

                ProgressChanged(1, 1);
                return;
            }

            #endregion

            #endregion

            ProgressChanged(66, 100);

            #region sat solver

            /* call SAT solver and pass cnf file to it */
            string satSolverOutputFilename = "solver.output.txt";
            string satSolverOutputFilePath = pluginDataPath + satSolverOutputFilename;

            stopwatch.Reset();
            TimeSpan satSolverTime;

            if (callSatSolver(outputCnfFilePath, satSolverOutputFilePath) != 10) // 10: sat, 20: unsat
            {
                stopwatch.Stop();
                satSolverTime = stopwatch.Elapsed;

                GuiLogMessage(String.Format("Sat solver process returned after {0} seconds.", satSolverTime.ToString("s'.'fff")), NotificationLevel.Info);

                GuiLogMessage("Sat solver did not return \"satisfiable\".", NotificationLevel.Info);

                /* write info to output stream */
                outputStringBuilder.AppendFormat("Sat solver process returned after {0} seconds... but failed to find a solution.", satSolverTime.ToString("s'.'fff") + Environment.NewLine);
                outputStream = new CStreamWriter();
                outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
                outputStream.Close();
                OnPropertyChanged("OutputStream");

                return;                
            }

            stopwatch.Stop();
            satSolverTime = stopwatch.Elapsed;

            GuiLogMessage(String.Format("Sat solver process returned after {0} seconds.", satSolverTime.ToString("s'.'fff")), NotificationLevel.Info);

            /* write info to output stream */
            outputStringBuilder.AppendFormat("Sat solver process returned successfully after {0} seconds." + Environment.NewLine, satSolverTime.ToString("s'.'fff"));
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            /* interpret SAT solver output */
            string outputString = processSatSolverOutput(satSolverOutputFilePath, inputMappingFilePath);

            if (outputString == null)
            {
                GuiLogMessage("Processing SAT solver output returned an error.", NotificationLevel.Error);
                return;  
            }

            outputStringBuilder.Append(outputString);
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");
            
            #endregion

            ProgressChanged(100, 100);
        }

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
            try
            {
                cbmcProcess.Kill();
                GuiLogMessage("Successfully killed Cbmc process.", NotificationLevel.Debug);
            }
            catch (Exception e)
            { 
                GuiLogMessage(String.Format("Killing Cbmc process threw an exception:" + Environment.NewLine + "{0}", e), NotificationLevel.Debug); 
            }

            try
            {
                satSolverProcess.Kill();
                GuiLogMessage("Successfully killed Sat solver process.", NotificationLevel.Debug);
            }
            catch (Exception e)
            {
                GuiLogMessage(String.Format("Killing Sat solver process threw an exception:" + Environment.NewLine + "{0}", e), NotificationLevel.Debug);
            }
        }

        public void Initialize()
        {
            settings.UpdateTaskPaneVisibility();
        }

        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion

        #region Custom Methods

        /// <summary>
        /// This event is fired when the CBMC process writes data to standard output.
        /// </summary>
        void CbmcProcess_OutputDataReceived(object sendingProcess, DataReceivedEventArgs data)
        {
            bool dataReceived = data.Data != null;

            if (dataReceived)
            {
                cbmcOutputString.AppendLine(data.Data.ToString());

                cbmcOutputStream = new CStreamWriter();
                cbmcOutputStream.Write(encoding.GetBytes(cbmcOutputString.ToString()));
                cbmcOutputStream.Close();
                OnPropertyChanged("CbmcOutputStream");
            }
        }

        /// <summary>
        /// This event is fired when the CBMC process writes data to standard error.
        /// </summary>
        void CbmcProcess_ErrorDataReceived(object sendingProcess, DataReceivedEventArgs data)
        {
            bool dataReceived = data.Data != null;

            if (dataReceived)
            {
                cbmcOutputString.AppendLine(data.Data.ToString());

                cbmcOutputStream = new CStreamWriter();
                cbmcOutputStream.Write(encoding.GetBytes(cbmcOutputString.ToString()));
                cbmcOutputStream.Close();
                OnPropertyChanged("CbmcOutputStream");
            }
        }

        /// <summary>
        /// This event is fired when the Sat solver process writes data to standard output.
        /// </summary>
        void SatSolverProcess_OutputDataReceived(object sendingProcess, DataReceivedEventArgs data)
        {
            bool dataReceived = data.Data != null;

            if (dataReceived)
            {
                satSolverOutputString.AppendLine(data.Data.ToString());

                satSolverOutputStream = new CStreamWriter();
                satSolverOutputStream.Write(encoding.GetBytes(satSolverOutputString.ToString()));
                satSolverOutputStream.Close();
                OnPropertyChanged("SatSolverOutputStream");
            }
        }

        /// <summary>
        /// This event is fired when the Sat solver process writes data to standard error.
        /// </summary>
        void SatSolverProcess_ErrorDataReceived(object sendingProcess, DataReceivedEventArgs data)
        {
            bool dataReceived = data.Data != null;

            if (dataReceived)
            {
                satSolverOutputString.AppendLine(data.Data.ToString());

                satSolverOutputStream = new CStreamWriter();
                satSolverOutputStream.Write(encoding.GetBytes(satSolverOutputString.ToString()));
                satSolverOutputStream.Close();
                OnPropertyChanged("SatSolverOutputStream");
            }
        }

        /// <summary>
        /// Writes a stream of type ICryptoolStream to a file. The parameter filepath should end with "\\".
        /// </summary>
        void ReadInputStreamToCodefile(ICryptoolStream inputStream, string path, string filename)
        {
            using (CStreamReader reader = InputStream.CreateReader())
            {
                int bytesRead;
                byte[] buffer = new byte[1024];

                /* create directory if not existent */
                bool dirExists = Directory.Exists(path);

                if (!dirExists)
                    Directory.CreateDirectory(path);

                FileStream fs = File.Open(path + filename, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);

                while ((bytesRead = reader.Read(buffer)) > 0)
                {
                    bw.Write(buffer, 0, bytesRead);
                }

                fs.Close();
                bw.Close();
            }
        }

        /// <summary>
        /// Get path and filename of the C code input file.
        /// </summary>
        string GetCodefilePath()
        {
            string inputFilename = "";

            /* either read from InputStream or get file location from settings.InputFile */
            int inputSelection = settings.InputSelection;

            if (inputSelection == 0) // via InputStream
            {
                ReadInputStreamToCodefile(InputStream, pluginDataPath, codefileName);

                inputFilename = pluginDataPath + codefileName;
            }
            else if (inputSelection == 1) // via settings.InputFile
            {
                inputFilename = settings.InputFile;
            }

            return inputFilename;
        }

        /// <summary>
        /// Calls the CBMC process. When successful, a file output.cnf.txt is created which contains the cnf representation
        /// of the code in codefile.c.
        /// </summary>
        /// <param name="codefilePath">Full path to the codefile, including the name of the codefile.</param>
        /// <returns>
        /// 0 if successful or 1 the input codefile could not be found
        /// </returns>
        int CallCbmcProcess(string codefilePath, string mainFunctionName)
        {
            /* write info to output stream */
            outputStringBuilder.Append("Calling Cbmc process... ");
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            bool codefileExists = File.Exists(codefilePath);

            if (!codefileExists)
            {
                GuiLogMessage(String.Format("Codefile not found at {0}.", codefilePath), NotificationLevel.Error);
                return 1;
            }           
            
            string cbmcExeFilename = "cbmc.exe";

            /* build args */
            string cbmcProcessArgs = "\"" + codefilePath + "\"";
            if (mainFunctionName != "" && mainFunctionName != null)
                cbmcProcessArgs += " --function " + mainFunctionName;

            //GuiLogMessage(String.Format("cbmc.exe path is {0}",
            //    (pluginDataPath + cbmcExecutableFilename)), NotificationLevel.Info);

            cbmcProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = pluginDataPath + cbmcExeFilename,
                    CreateNoWindow = true,
                    Arguments = cbmcProcessArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            };

            cbmcProcess.OutputDataReceived += new DataReceivedEventHandler(CbmcProcess_OutputDataReceived);
            cbmcProcess.ErrorDataReceived += new DataReceivedEventHandler(CbmcProcess_ErrorDataReceived);

            cbmcProcess.Start();
            cbmcProcess.BeginOutputReadLine();
            cbmcProcess.BeginErrorReadLine();
            cbmcProcess.WaitForExit();

            GuiLogMessage(String.Format(String.Format("Cbmc process returned with exitcode {0}",
                cbmcProcess.ExitCode)), NotificationLevel.Debug);
                
            //TODO: review cbmc process exitcodes
            return cbmcProcess.ExitCode;            
        }

        /// <summary>
        /// Converts a hex string into a bit array. The first bit in the array is the least significant bit and the last bit in the array is the most significant bit.
        /// </summary>
        BitArray HexStringToBitArray(string hex)
        {
            BitArray bitArray;

            if (!IsHexString(hex)) // check if all characters are hex
            {
                GuiLogMessage("When using the prefix \"0x\" for the hash value, ensure the it is an even amount of regular hex numbers.", NotificationLevel.Error);
                return null;
            }
            else if (hex.Length % 2 != 0) // hash value must be an even amount of hex values
            {
                GuiLogMessage("Ensure the hash value is an even amount of regular hexadecimal numbers.", NotificationLevel.Error);
                return null;
            }            

            byte[] byteArray = HexStringToByteArray(hex);
            bitArray = new BitArray(byteArray);            

            return bitArray;
        }

        /// <summary>
        /// Converts a bit string into a bit array. The first bit in the array is the least significant bit and the last bit in the array is the most significant bit.
        /// </summary>
        BitArray BitStringToBitArray(string bitstring)
        {
            BitArray bitArray;

            if (!IsBitString(bitstring)) // check if all characters are hex
            {
                GuiLogMessage("When using the prefix \"0b\", ensure the following string only contains bit values (0 and 1).", NotificationLevel.Error);
                return null;
            }

            int numberOfBits = bitstring.Length;

            /* reverse bitstring to obtain the correct ordering (original ordering is msb to lsb, we need lsb to msb) */
            char[] bits = bitstring.ToCharArray();
            Array.Reverse(bits);
            string reversedBitstring = new string(bits);
            
            Boolean[] hashBools = new Boolean[numberOfBits];

            for (int i = 0; i < numberOfBits; i++)
            {
                if (reversedBitstring.ElementAt(i).Equals('0'))
                    hashBools[i] = false;
                else if (reversedBitstring.ElementAt(i).Equals('1'))
                    hashBools[i] = true;
            }

            bitArray = new BitArray(hashBools);

            return bitArray;
        }

        /// <summary>
        /// Converts a BitArray into a hex string. The first bit in the array should be the least significant bit and the last bit in the array should be the most significant bit.
        /// </summary>
        string BitArrayToHexString(BitArray ba)
        {
            byte[] data = new byte[ba.Length / 8];
            ba.CopyTo(data, 0);

            // reverse to obtain the correct bit ordering
            Array.Reverse(data);
            
            string hexString = BitConverter.ToString(data);

            /* replace all dashes in hexString (0xab-cd-ef -> 0xabcdef) */
            string output = hexString.Replace("-", "");

            return output;
        }
        
        /// <summary>
        /// Converts a hex string into a byte array.
        /// </summary>
        byte[] HexStringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                // begin with the last two nibbles and iterate from right to left to respect byte ordering
                bytes[(numberChars / 2) - 1 - i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        string BinaryStringToHexString(string binary)
        {
            string true_binary = binary.Replace('x', '0');      // if binary contains the wildcard character 'x' replace them with '0' by default

            StringBuilder result = new StringBuilder(binary.Length / 8 + 1);

            for (int i = 0; i < binary.Length; i += 8)
            {
                string eightBits = binary.Substring(i, 8);
                result.AppendFormat("{0:X2}", Convert.ToByte(eightBits, 2));
            }

            return result.ToString();
        }

        /// <summary>
        /// Check if a string only contains hex characters.
        /// </summary>
        bool IsHexString(string test)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(test, @"\A\b[0-9a-fA-F]+\b\Z");
        }

        /// <summary>
        /// Check if a string only contains bit characters (0 or 1).
        /// </summary>
        bool IsBitString(string test)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(test, @"\A\b[0-1]+\b\Z");
        }

        /// <summary>
        /// Convert byte to bits
        /// </summary>
        IEnumerable<bool> GetBits(byte b)
        {
            for (int i = 0; i < 8; i++)
            {
                yield return (b & 0x80) != 0;
                b *= 2;
            }
        }

        /// <summary>
        /// Returns an int array which contains the labels of literals (from lsb to msb).
        /// </summary>
        int[][] getMapping(string mappingFilePath)
        {
            bool mappingFileExists = File.Exists(mappingFilePath);

            if (!mappingFileExists)
            {
                GuiLogMessage(String.Format("Mapping file not found at {0}.", mappingFilePath), NotificationLevel.Error);
                return null;
            }
            
            /* read output mapping file  */
            string mappingFileContent = readFileToString(mappingFilePath);
            string[] mappingFileLines = mappingFileContent.Split(Environment.NewLine.ToCharArray());
                        
            string[] infoLine = mappingFileLines[0].Split(' ');        // format: <type> <number of variables> <size of variables>

            /* get type of mapping (INPUT or OUTPUT) */
            string variableType = infoLine[0];

            int numberOfVariables = int.Parse(infoLine[1]);     //TODO: change in TryParse with error handling
            int sizeOfVariables = int.Parse(infoLine[2]);       //TODO: change in TryParse with error handling

            /* build mapping array */
            int[][] mapping = new int[numberOfVariables][];

            string variableNumberAsString;
            int variableNumber;
            string bitNumberAsString;
            int bitNumber;
            int positionOfUnderscore;
            int positionOfColon;
            int literalValue;
            string[] tmp;
            string variableString;
            string literalString;

            foreach (string line in mappingFileLines)
            {
                tmp = null;
                tmp = line.Split(' ');

                if (tmp.Length == 2) // skip first (info) line
                {
                    variableString = tmp[0];
                    literalString = tmp[1];

                    positionOfUnderscore = variableString.IndexOf("_");
                    positionOfColon = variableString.IndexOf(":");
                    variableNumberAsString = variableString.Substring(positionOfUnderscore + 1, positionOfColon - (positionOfUnderscore + 1));
                    bitNumberAsString = variableString.Substring(positionOfColon + 1);

                    if (int.TryParse(variableNumberAsString, out variableNumber))
                    {
                        if (mapping[variableNumber] == null)
                            mapping[variableNumber] = new int[sizeOfVariables];

                        if (int.TryParse(bitNumberAsString, out bitNumber))
                        {
                            if (int.TryParse(literalString, out literalValue))
                            {
                                if (variableNumber < mapping.Length)
                                {
                                    mapping[variableNumber][bitNumber] = literalValue;
                                }
                                else
                                {
                                    GuiLogMessage(String.Format("Make sure the variable definitions start with {0}_0 respectively, the indices are incremented steadily and each variable is used in the code.", variableType), NotificationLevel.Error);
                                    return null;
                                }
                            }
                            else
                            {
                                GuiLogMessage(String.Format("Failed to parse the literal for {0} variable {1}: {2}.", variableType, variableString, literalString), NotificationLevel.Error);
                                return null;
                            }
                        }
                        else
                        {
                            GuiLogMessage(String.Format("Failed to parse the bit number for {0} variable {1}: {2}.", variableType, variableString, bitNumberAsString), NotificationLevel.Error);
                            return null;
                        }                        
                    }
                    else
                    {
                        GuiLogMessage(String.Format("Failed to parse the variable index for {0} variable {1}: {2}.",variableType, variableString, variableNumberAsString), NotificationLevel.Error);
                        return null;
                    }
                }
            }

            /* print warning for unassigned variables */
            for (int i = 0; i < mapping.Length; i++)
            {
                for (int j = 0; j < mapping[i].Length; j++)
                {
                    if (mapping[i][j] == 0)
                        GuiLogMessage(String.Format("Variable {0}_{1}:{2} does not appear in the {0} mapping, is it used in the code? It will be assigned 'false' by default.", variableType, i, j), NotificationLevel.Warning);
                } 
            }

            return mapping;
        }

        /// <summary>
        /// Reads the mapping file generated by cbmc and reads the hash value provided by the user in the plugin parameters.
        /// Opens the cnf file generated by cbmc and appends the encoding of the hash value provided by the user.
        /// </summary>
        int encodeHashInCnf(string outputMappingFilePath, string outputCnfFilePath)
        {
            /* write info to output stream */
            outputStringBuilder.Append("Encoding hash value in CNF... ");
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            /* get the output literals */
            int[][] outputMapping;
            if ((outputMapping = getMapping(outputMappingFilePath)) == null)
            {
                GuiLogMessage("Error retreiving outuput mapping.", NotificationLevel.Error);
                return 1;
            }

            /* parse input hash value which can be either a binary or hexadecimal value */
            string inputHashValueString = settings.InputHashValue;
            string inputHashValueType = inputHashValueString.Substring(0, 2);
            string inputHashValue = inputHashValueString.Substring(2);
            
            BitArray hashBits;

            if (inputHashValueType == "0x")
	        {
		        /* get the hash value as bit array (ranging from lsb to msb) */
                if ((hashBits = HexStringToBitArray(inputHashValue)) == null)
                {
                    GuiLogMessage("Error retreiving hash bits.", NotificationLevel.Error);
                    return 1;
                }                 
	        }
            else if (inputHashValueType == "0b")
            {
                /* get the hash value as bit array (ranging from lsb to msb) */
                if ((hashBits = BitStringToBitArray(inputHashValue)) == null)
                {
                    GuiLogMessage("Error retreiving hash bits.", NotificationLevel.Error);
                    return 1;
                }
            }
            else
            {
                GuiLogMessage("Use the prefix \"0x\" for a hexadecimal hash value or the prefix \"0b\" for a binary hash value.", NotificationLevel.Error);
                return 1;
            }

            /* ensure hash value and output bits have the same size */
            int outputMappingLength = 0;
            for (int i = 0; i < outputMapping.Length; i++)
            {
                outputMappingLength += outputMapping[i].Length;
            } 

            if (hashBits.Length != outputMappingLength)
            {
                GuiLogMessage("Ensure the specified hash value has the correct length." + Environment.NewLine
                    + String.Format("Hash value: {0} bits", hashBits.Length) + Environment.NewLine
                    + String.Format("Output: {0} bits", outputMappingLength)
                    , NotificationLevel.Error);
                return 1;
            }

            /* build the clauses that encode the hash value */
            StringBuilder hashEncoding = new StringBuilder();

            string sign;
            int offset = 0;

            for (int i = 0; i < outputMapping.Length; i++)
            {
                for (int j = 0; j < outputMapping[i].Length; j++)
                {
                    if (hashBits[offset + j])    // 1
                        sign = "";
                    else                    // 0
                        sign = "-";

                    hashEncoding.Append(String.Format("{0}{1} 0" + Environment.NewLine, sign, outputMapping[i][j])); // clause line
                }

                offset += outputMapping[i].Length;  // maintain the correct offset in the one dimensional array hashBits
            }

            /* append hash encoding to the cnf */
            bool cnfFileExists = File.Exists(outputCnfFilePath);

            if (!cnfFileExists)
            {
                GuiLogMessage(String.Format("Cnf file not found at {0}.", outputCnfFilePath), NotificationLevel.Error);
                return 1;
            }

            using (FileStream fs = File.Open(outputCnfFilePath, FileMode.Append))
            {
                fs.Write(encoding.GetBytes(hashEncoding.ToString()), 0, hashEncoding.Length);            
            }

            /* write info to output stream */
            outputStringBuilder.Append("successful!" + Environment.NewLine);
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            return 0;
        }

        /// <summary>
        /// Calls the SAT solver Cryptominisat and passes the CNF file at `cnfFilePath`. The output of the SAT solver (literal assignment) is written to `satSolverOutputFilepath`.
        /// </summary>
        /// <param name="cnfFilePath">Path to the CNF file</param>
        /// <param name="satSolverOutputFilePath">Path where the SAT solver output file is written to (including filename)</param>
        /// <returns></returns>
        int callSatSolver(string cnfFilePath, string satSolverOutputFilePath)
        {
            /* write info to output stream */
            outputStringBuilder.Append("Calling Sat solver process... " + Environment.NewLine);
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            bool outputCnfFileExists = File.Exists(cnfFilePath);

            if (!outputCnfFileExists)
            {
                GuiLogMessage(String.Format("Cnf file not found @ {0}.", cnfFilePath), NotificationLevel.Error);
                return 1;
            }

            string satSolverFilename = "cryptominisat32.exe";

            /* build args */
            string satSolverProcessArgs = "\"" + cnfFilePath + "\" \"" + satSolverOutputFilePath + "\"";

            satSolverProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = pluginDataPath + satSolverFilename,
                    CreateNoWindow = true,
                    Arguments = satSolverProcessArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            };

            GuiLogMessage(String.Format(String.Format("Calling SAT solver at {0} with parameters {1}",
                pluginDataPath+satSolverFilename, satSolverProcessArgs)), NotificationLevel.Debug);

            satSolverProcess.OutputDataReceived += new DataReceivedEventHandler(SatSolverProcess_OutputDataReceived);
            satSolverProcess.ErrorDataReceived += new DataReceivedEventHandler(SatSolverProcess_ErrorDataReceived);

            satSolverProcess.Start();
            satSolverProcess.BeginOutputReadLine();
            satSolverProcess.BeginErrorReadLine();
            satSolverProcess.WaitForExit();

            GuiLogMessage(String.Format(String.Format("SAT solver process returned with exitcode {0}",
                satSolverProcess.ExitCode)), NotificationLevel.Debug);

            //TODO: review cmsat exitcodes
            return satSolverProcess.ExitCode;
        }

        /// <summary>
        /// Interprets the SAT solver's output literal assignment to define the bits of the input variables.
        /// </summary>
        /// <param name="satSolverOutputFilePath">Path to the file which contains the literal assignments obtained by the SAT solver</param>
        /// <param name="inputMappingFilePath">Path to the input mapping file obtained by CBMC which maps the input variables to CNF literals</param>
        /// <returns>A string which describes the assignment of the input variables in binary and hexadecimal form</returns>
        string processSatSolverOutput(string satSolverOutputFilePath, string inputMappingFilePath)
        {
            bool satSolverOutputFileExists = File.Exists(satSolverOutputFilePath);

            if (!satSolverOutputFileExists)
            { 
                GuiLogMessage(String.Format("Sat solver output file not found at {0}.", satSolverOutputFilePath), NotificationLevel.Error);
                return null;
            }
        
            /* read sat solver output file */
            string satSolverOutputFileContent = readFileToString(satSolverOutputFilePath);

            string[] lines = satSolverOutputFileContent.Split(Environment.NewLine.ToCharArray());

            int[][] messageBits;

            if (lines[0] == "SAT")
            {
                int[][] inputMapping = getMapping(inputMappingFilePath);
                if (inputMapping == null)
                {
                    GuiLogMessage("Error retreiving input mapping.", NotificationLevel.Error);
                    return null;                    
                }

                int numberOfInputBits = 0;
                for (int i = 0; i < inputMapping.Length; i++)
                {
                    numberOfInputBits += inputMapping[i].Length;
                }

                string[] inputLiterals = lines[1].Split(' ');

                /* initialize messageBits array with the value -1 */
                messageBits = new int[inputMapping.Length][];

                for (int i = 0; i < messageBits.Length; i++)
                { 
                    messageBits[i] = new int[inputMapping[i].Length];

                    for (int j = 0; j < messageBits[i].Length; j++)
                    {
                        messageBits[i][j] = -1;
                    }
                }
                
                int signedLiteralValue;
                int literalValue;
                int sign;

                foreach (string literal in inputLiterals)
                {
                    if (int.TryParse(literal, System.Globalization.NumberStyles.AllowLeadingSign, null, out signedLiteralValue))
                    {
                        if (signedLiteralValue == 0)     // skip last zero which occurs at the end of the sat solver variable assignments
                            continue;

                        if (signedLiteralValue < 0)      // literal looks like "-x"
                        {
                            sign = 0;
                            literalValue = Math.Abs(signedLiteralValue);  // get absolute value without sign
                        }
                        else                // literal looks like "x"
                        {
                            sign = 1;
                            literalValue = signedLiteralValue;
                        }

                        int variableNumber = -1;
                        int bitPosition = -1;

                        /* get the variable number and bit position to which the current literal refers (e.g. variable number = 1 and bit position = 7 for "INPUT_1:7") */
                        for (int i = 0; i < inputMapping.Length; i++)
                        {
                            bitPosition = Array.IndexOf(inputMapping[i], literalValue);

                            if (bitPosition != -1)
                            {
                                variableNumber = i;

                                /* assign the sign to the related input bit if it occurs in the input mapping array (literals 1 and 2 will not appear there, since they are given out by cbmc to encode the constant zero and one gates) */
                                messageBits[variableNumber][bitPosition] = sign;

                                break;
                            }
                        }                        
                    }
                    else
                    {
                        GuiLogMessage(String.Format("Failed to parse the literal {0}.", literal), NotificationLevel.Error);
                        return null;
                    }
                }
            }
            else
            {
                GuiLogMessage(String.Format("First line in SAT solver output file {0} is not \"SAT\".", satSolverOutputFilePath), NotificationLevel.Error);
                return null; 
            }

            StringBuilder messageBitsString = new StringBuilder();

            //int separatorCounter = 0;

            /* by using insert instead of append, the string shows the lsb on the right and msb on the left */
            for (int i = 0; i < messageBits.Length; i++)
            {
                for (int j = 0; j < messageBits[i].Length; j++)
                {
                    if (messageBits[i][j] == 1)
                        messageBitsString.Insert(0, "1");
                    else if (messageBits[i][j] == 0)
                        messageBitsString.Insert(0, "0");
                    else if (messageBits[i][j] == -1)
                        messageBitsString.Insert(0, "x");   // this means that the related bit can be either 0 or 1, i.e. its value does not matter since it does not affect the output hash value
                }
            }

            string messageBitsBinaryString = messageBitsString.ToString();
            string messageBitsHexString = "";
            string inputHexCaption = "";

            if (messageBitsBinaryString.Length % 8 == 0)
            {
                messageBitsHexString = BinaryStringToHexString(messageBitsBinaryString);
                inputHexCaption = "(Hexadecimal): 0x";
            }

            string outputString = 
                Environment.NewLine + "-----------------------------------------" + Environment.NewLine
                + "Input Message found!" + Environment.NewLine + Environment.NewLine
                + "(Binary): 0b" + messageBitsBinaryString + Environment.NewLine
                + inputHexCaption
                + messageBitsHexString;

            return outputString;
        }

        /// <summary>
        /// Reads a file and returns its content in a string object.
        /// </summary>
        /// <param name="filepath">Path to the file to be read</param>
        /// <returns>String which contains the content of the file</returns>
        string readFileToString(string filepath)
        {
            using (FileStream fs = File.Open(filepath, FileMode.Open))
            {
                StringBuilder sb = new StringBuilder();

                if (fs.Length > 65536) // stop if the mapping file is too big (arbitrary big number) (paranoid)
                {
                    GuiLogMessage(String.Format("File {0} is very big: {1} bytes", filepath, fs.Length), NotificationLevel.Warning);
                }

                byte[] buffer = new byte[fs.Length];

                while (fs.Read(buffer, 0, buffer.Length) > 0)
                {
                    sb.Append(encoding.GetString(buffer));
                }

                return sb.ToString();
            }        
        }

        int encodeGuessedBitsInCnf(string inputMappingFilePath, string outputCnfFilePath)
        {
            /* write info to output stream */
            outputStringBuilder.Append("Encoding guessed bits in CNF... ");
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            string guessedBitsString = settings.GuessedBits;

            if (guessedBitsString == "")
            {
                GuiLogMessage("The guess bits option was selected but no bits where guessed.", NotificationLevel.Warning);
            }

            /* check if guessed bits string only contains 0, 1 or x (the x denotes "don't guess this bit", e.g. "00xx11" means set bit 0 and 1 to false and bit 4 and 5 to true and bits 2 and 3 are not guessed) */
            foreach (char c in guessedBitsString)
            {
                if (!c.Equals('0') && !c.Equals('1') && !c.Equals('x'))
                {
                    GuiLogMessage(String.Format("Use only the characters 0, 1 and x to guess bits, the character {0} is not allowed.", c), NotificationLevel.Error);
                    return 1;
                }
            }

            char[] guessedBits = guessedBitsString.ToCharArray();

            /* get input mapping */
            int[][] inputMapping = getMapping(inputMappingFilePath);
            if (inputMapping == null)
            {
                GuiLogMessage("Error retreiving input mapping.", NotificationLevel.Error);
                return 1;
            }

            /* ensure guessed bits are less or equal to the amoung of input bits */
            int inputMappingLength = 0;
            for (int i = 0; i < inputMapping.Length; i++)
            {
                inputMappingLength += inputMapping[i].Length;
            }

            if (!(guessedBits.Length <= inputMappingLength))
            {
                GuiLogMessage("Ensure the amount of guessed bits is less or equal to the amoung of input bits." + Environment.NewLine
                    + String.Format("Guessed bits: {0} bits", guessedBits.Length) + Environment.NewLine
                    + String.Format("Input: {0} bits", inputMappingLength)
                    , NotificationLevel.Error);
                return 1;
            }

            /* build the clauses that encode guessed bits */
            StringBuilder guessedBitsEncoding = new StringBuilder();

            string sign;
            int offset = 0;

            for (int i = 0; (i < inputMapping.Length) && (offset < guessedBits.Length); i++)
            {
                for (int j = 0; (j < inputMapping[i].Length) && (offset + j < guessedBits.Length); j++)
                {
                    if (guessedBits[offset + j].Equals('0'))
                        sign = "-";
                    else if (guessedBits[offset + j].Equals('1'))
                        sign = "";
                    else if (guessedBits[offset + j].Equals('x'))
                        continue;
                    else
                    {
                        GuiLogMessage("Something went wrong in the function encodeGuessedBitsInCnf (this code should never be reached).", NotificationLevel.Error);
                        break;
                    }

                    guessedBitsEncoding.Append(String.Format("{0}{1} 0" + Environment.NewLine, sign, inputMapping[i][j])); // clause line
                }

                offset += inputMapping[i].Length; // maintain the correct offset in the one dimensional array guessedBits
            }

            /* append guessed bits encoding to the cnf */
            bool cnfFileExists = File.Exists(outputCnfFilePath);

            if (!cnfFileExists)
            {
                GuiLogMessage(String.Format("Cnf file not found at {0}.", outputCnfFilePath), NotificationLevel.Error);
                return 1;
            }

            using (FileStream fs = File.Open(outputCnfFilePath, FileMode.Append))
            {
                fs.Write(encoding.GetBytes(guessedBitsEncoding.ToString()), 0, guessedBitsEncoding.Length);
            }

            /* write info to output stream */
            outputStringBuilder.Append("successful!" + Environment.NewLine);
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            return 0;
        }
        
        int encodeSecondPreimageInCnf(string inputMappingFilePath, string outputCnfFilePath)
        {
            /* write info to output stream */
            outputStringBuilder.Append("Encoding second preimage in CNF... ");
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            /* get input mapping */
            int[][] inputMapping = getMapping(inputMappingFilePath);
            if (inputMapping == null)
            {
                GuiLogMessage("Error retreiving input mapping.", NotificationLevel.Error);
                return 1;
            }

            /* parse second preimage value which can be either a binary or hexadecimal value */
            string secondPreimageString = settings.SecondPreimage;
            string secondPreimageType = secondPreimageString.Substring(0, 2);
            string secondPreimageValue = secondPreimageString.Substring(2);

            BitArray secondPreimageBits;

            if (secondPreimageType == "0x")
            {
                /* get the second preimage value as bit array (ranging from lsb to msb) */
                if ((secondPreimageBits = HexStringToBitArray(secondPreimageValue)) == null)
                {
                    GuiLogMessage("Error retreiving second preimage bits.", NotificationLevel.Error);
                    return 1;
                }
            }
            else if (secondPreimageType == "0b")
            {
                /* get the second preimage value as bit array (ranging from lsb to msb) */
                if ((secondPreimageBits = BitStringToBitArray(secondPreimageValue)) == null)
                {
                    GuiLogMessage("Error retreiving second preimage bits.", NotificationLevel.Error);
                    return 1;
                }
            }
            else
            {
                GuiLogMessage("Use the prefix \"0x\" for a hexadecimal hash value or the prefix \"0b\" for a binary second preimage.", NotificationLevel.Error);
                return 1;
            }

            /* ensure second preimage and output bits have the same size */
            int inputMappingLength = 0;
            for (int i = 0; i < inputMapping.Length; i++)
            {
                inputMappingLength += inputMapping[i].Length;
            }

            if (secondPreimageBits.Length != inputMappingLength)
            {
                GuiLogMessage("Ensure the specified second preimage has the correct length." + Environment.NewLine
                    + String.Format("Second preimage: {0} bits", secondPreimageBits.Length) + Environment.NewLine
                    + String.Format("Input: {0} bits", inputMappingLength)
                    , NotificationLevel.Error);
                return 1;
            }

            /* build the clauses that encode the second preimage */
            StringBuilder secondPreimageEncoding = new StringBuilder();

            string sign;
            int offset = 0;

            for (int i = 0; i < inputMapping.Length; i++)
            {
                for (int j = 0; j < inputMapping[i].Length; j++)
                {
                    if (secondPreimageBits[offset + j])  // 1
                        sign = "-";                     // invert literal
                    else                            // 0
                        sign = "";                      // invert literal

                    secondPreimageEncoding.Append(String.Format("{0}{1} ", sign, inputMapping[i][j])); // clause
                }

                offset += inputMapping[i].Length; // maintain the correct offset in the one dimensional array secondPreimageBits
            }

            secondPreimageEncoding.Append("0" + Environment.NewLine); // append clause terminating zero

            /* append second preimage encoding to the cnf */
            bool cnfFileExists = File.Exists(outputCnfFilePath);

            if (!cnfFileExists)
            {
                GuiLogMessage(String.Format("Cnf file not found at {0}.", outputCnfFilePath), NotificationLevel.Error);
                return 1;
            }

            using (FileStream fs = File.Open(outputCnfFilePath, FileMode.Append))
            {
                fs.Write(encoding.GetBytes(secondPreimageEncoding.ToString()), 0, secondPreimageEncoding.Length);
            }

            /* write info to output stream */
            outputStringBuilder.Append("successful!" + Environment.NewLine);
            outputStream = new CStreamWriter();
            outputStream.Write(encoding.GetBytes(outputStringBuilder.ToString()));
            outputStream.Close();
            OnPropertyChanged("OutputStream");

            return 0;
        }

        #endregion
    }
}
