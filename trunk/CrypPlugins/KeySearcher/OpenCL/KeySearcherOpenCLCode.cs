using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Control;
using OpenCLNet;

namespace KeySearcher
{
    class KeySearcherOpenCLCode
    {
        private readonly KeySearcher keySearcher;
        private byte[] encryptedData;
        private IControlCost controlCost;
        private IControlEncryption encryptionController;
        private int approximateNumberOfKeys;

        private IKeyTranslator keyTranslatorOfCode = null;
        private string openCLCode = null;
        private Kernel openCLKernel = null;

        public KeySearcherOpenCLCode(KeySearcher keySearcher, byte[] encryptedData, IControlEncryption encryptionController, IControlCost controlCost, int approximateNumberOfKeys)
        {
            this.keySearcher = keySearcher;
            this.encryptedData = encryptedData;
            this.encryptionController = encryptionController;
            this.controlCost = controlCost;
            this.approximateNumberOfKeys = approximateNumberOfKeys;
        }

        private string CreateOpenCLBruteForceCode(IKeyTranslator keyTranslator)
        {
            if (keyTranslatorOfCode == keyTranslator)
            {
                return openCLCode;
            }

            int bytesUsed = controlCost.getBytesToUse();
            if (encryptedData.Length < bytesUsed)
                bytesUsed = encryptedData.Length;

            string code = encryptionController.GetOpenCLCode(bytesUsed);
            if (code == null)
                throw new Exception("OpenCL not supported in this configuration!");

            //put cost function stuff into code:
            code = controlCost.ModifyOpenCLCode(code);

            //put input to be bruteforced into code:
            string inputarray = string.Format("__constant unsigned char inn[{0}] = {{ \n", bytesUsed);
            for (int i = 0; i < bytesUsed; i++)
            {
                inputarray += String.Format("0x{0:X2}, ", this.encryptedData[i]);
            }
            inputarray = inputarray.Substring(0, inputarray.Length - 2);
            inputarray += "}; \n";
            code = code.Replace("$$INPUTARRAY$$", inputarray);

            //put key movement of pattern into code:
            code = keyTranslator.ModifyOpenCLCode(code, approximateNumberOfKeys);

            keyTranslatorOfCode = keyTranslator;
            this.openCLCode = code;
            
            return code;
        }


        public Kernel GetBruteforceKernel(OpenCLManager oclManager, IKeyTranslator keyTranslator)
        {
            if (keyTranslatorOfCode == keyTranslator)
            {
                return openCLKernel;
            }

            try
            {
                var program = oclManager.CompileSource(CreateOpenCLBruteForceCode(keyTranslator));
                keySearcher.GuiLogMessage(string.Format("Using OpenCL with {0} threads.", keyTranslator.GetOpenCLBatchSize()), NotificationLevel.Info);
                openCLKernel = program.CreateKernel("bruteforceKernel");
                return openCLKernel;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured when trying to compile OpenCL code: " + ex.Message);
            }
        }
    }
}
