using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Cryptool.MD5.Presentation.Helpers;

namespace Test.Cryptool.MD5.Presentation.Helpers
{
    [TestFixture]
    class PresentationControlFactoryTest
    {
        [Test]
        void Construction()
        {
            new PresentationControlFactory();
        }
    }
}
