using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.Alphabets;
using System.IO;

namespace Cryptool.Hill
{
    [PluginInfo("B387B22B-678A-4f09-A783-F7DE889ED980","Hill","Hill cipher")]
    public class Hill : IEncryptionAlgorithm
    {
        private HillSettings settings;

        public Hill ()
	    {
            this.settings = new HillSettings();
	    }

        public void Add(IEncryptionAlgorithmVisualization visualization)
        {

        }
        public IKey GenerateKey()
        {
            return null;
        }

        public EncryptionAlgorithmType AlgorithmType
        {
            get { return EncryptionAlgorithmType.SymmetricClassic; }
        }
        public Stream Encrypt(IEncryptionAlgorithmSettings settings)
        {
            AlphabetConverter alphConv = new AlphabetConverter();
            int[] inputData = alphConv.StreamToIntArray(((HillSettings)settings).InputData);
            int[] outputData = new int[inputData.Length];

            for (long i = 0; i < inputData.Length/((HillSettings)settings).Dim; i++)
            {
                for (int j = 0; j < ((HillSettings)settings).Dim; j++)
                {
                    long hilf = 0;
                    for (int k = 0; k <((HillSettings)settings).Dim; k++)
                    {
                        //hilf += (enc_mat)(k,j) * inputData[dim*i+k];
                        hilf %= ((HillSettings)settings).Modul;
                    }
                    outputData[((HillSettings)settings).Dim * i + j] = (int)hilf;
                }
            }
            return alphConv.intArrayToStream(outputData);
        }

        public Stream Decrypt(IEncryptionAlgorithmSettings settings)
        {
            AlphabetConverter alphConv = new AlphabetConverter();
            int[] inputData = alphConv.StreamToIntArray(((HillSettings)settings).InputData);
            int[] outputData = new int[inputData.Length];

            for (long i = 0; i < inputData.Length/((HillSettings)settings).Dim; i++)
            {
                for (int j = 0; j < ((HillSettings)settings).Dim; j++)
                {
                    long hilf = 0;
                    for (int k = 0; k < ((HillSettings)settings).Dim; k++)
                    {
                        //hilf += (dec_mat)(k,j) * inputData[dim*i+k];
                        hilf %= ((HillSettings)settings).Modul;
                    }
                    outputData[((HillSettings)settings).Dim * i + j] = (int)hilf;
                }
            }
            return alphConv.intArrayToStream(outputData);
        }

        public IEncryptionAlgorithmSettings GetSettingsObject()
        {
            return this.settings;
        }

        public void Initialize()
        {

        }

        public void Dispose()
        {

        }

        public static Guid Gui(string p)
        {
            return new Guid(p);
        }

    }
}
