using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorkspaceManager.Model;

namespace Tests.KeySearcherTests
{
    [TestClass]
    public class OpenCLTest
    {
        [TestMethod]
        public void TestOpenCL()
        {
            KeySearcher.KeySearcher keysearcher = null;
            WorkspaceManager.WorkspaceManager newEditor = LoadAndInitKeySearcher(ref keysearcher);

            Assert.IsNotNull(keysearcher);  //keysearcher exists
            Assert.AreNotEqual(((KeySearcher.KeySearcherSettings)keysearcher.Settings).OpenCLDevice, -1);   //OpenCL device exists

            //first, calculate with CPU:
            double top1value;
            string top1key;
            CalculateKeySearcher(keysearcher, newEditor, out top1value, out top1key);

            //now, calculate with OpenCL:
            ((KeySearcher.KeySearcherSettings) keysearcher.Settings).CoresUsed = 0;
            ((KeySearcher.KeySearcherSettings) keysearcher.Settings).UseOpenCL = true;
            double top1valueOpenCL;
            string top1keyOpenCL;
            CalculateKeySearcher(keysearcher, newEditor, out top1valueOpenCL, out top1keyOpenCL);

            //now compare:
            Assert.AreEqual(top1value, top1valueOpenCL);
            Assert.AreEqual(top1key, top1keyOpenCL);
        }

        private void CalculateKeySearcher(KeySearcher.KeySearcher keysearcher, WorkspaceManager.WorkspaceManager newEditor, out double top1value, out string top1key)
        {
            newEditor.Execute();
            
            while (!keysearcher.IsKeySearcherFinished)
                Thread.Sleep(1000);

            top1value = keysearcher.Top1.value;
            top1key = keysearcher.Top1.key;

            newEditor.Stop();
        }

        private WorkspaceManager.WorkspaceManager LoadAndInitKeySearcher(ref KeySearcher.KeySearcher keysearcher)
        {
            var file = "..\\..\\..\\ProjectSamples\\WorkspaceManagerSamples\\AES_Analyzer_Entropy.cwm";
            file = Path.Combine(Directory.GetCurrentDirectory(), file);

            var newEditor = new WorkspaceManager.WorkspaceManager();
            var model = ModelPersistance.loadModel(file);

            foreach (PluginModel pluginModel in model.GetAllPluginModels())
            {
                pluginModel.Plugin.Initialize();
                if (pluginModel.GetName() == "KeySearcher")
                    keysearcher = (KeySearcher.KeySearcher) pluginModel.Plugin;
            }

            newEditor.Open(model);
            return newEditor;
        }
    }
}
