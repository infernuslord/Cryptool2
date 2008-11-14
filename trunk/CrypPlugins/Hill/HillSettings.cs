using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.IO;

namespace Cryptool.Hill
{
    public class HillSettings : IEncryptionAlgorithmSettings
    {
        private Stream inputData;

        private int dim;
        private int modul;

        [ControlType(ControlType.TextBox, DisplayLevel.Beginner, true, "", "", new string[] { })]
        public Stream InputData
        {
            get { return this.inputData; }
            set { this.inputData = value; }
        }

        [ControlType(ControlType.TextBox, DisplayLevel.Beginner, true, "", "", new string[] { })]
        public int Dim
        {
            get { return this.dim; }
            set { this.dim = value; }
        }

        [ControlType(ControlType.TextBox, DisplayLevel.Beginner, true, "", "", new string[] { })]
        public int Modul
        {
            get { return this.modul; }
            set { this.modul = value; }
        }
    }
}
