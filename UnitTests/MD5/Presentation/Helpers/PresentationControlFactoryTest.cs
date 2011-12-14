using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.MD5.Presentation.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Cryptool.MD5.Presentation.Helpers
{
    [TestClass]
    public class PresentationControlFactoryTest
    {
        [TestMethod]
        public void Construction()
        {
            new PresentationControlFactory();
        }
    }
}
