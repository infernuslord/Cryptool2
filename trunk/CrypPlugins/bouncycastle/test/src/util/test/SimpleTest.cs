using System;
using System.IO;
using System.Reflection;
using System.Text;

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Utilities.Test
{
    public abstract class SimpleTest
        : ITest
    {
		public abstract string Name
		{
			get;
		}

		private ITestResult Success()
        {
            return SimpleTestResult.Successful(this, "Okay");
        }

        internal void Fail(
            string message)
        {
            throw new TestFailedException(SimpleTestResult.Failed(this, message));
        }

        internal void Fail(
            string		message,
            Exception	throwable)
        {
            throw new TestFailedException(SimpleTestResult.Failed(this, message, throwable));
        }

		internal void Fail(
            string message,
            object expected,
            object found)
        {
            throw new TestFailedException(SimpleTestResult.Failed(this, message, expected, found));
        }

		internal bool AreEqual(
            byte[] a,
            byte[] b)
        {
			return Arrays.AreEqual(a, b);
		}

		public virtual ITestResult Perform()
        {
            try
            {
                PerformTest();

				return Success();
            }
            catch (TestFailedException e)
            {
                return e.GetResult();
            }
            catch (Exception e)
            {
                return SimpleTestResult.Failed(this, "Exception: " +  e, e);
            }
        }

		internal static void RunTest(
            ITest test)
        {
            RunTest(test, Console.Out);
        }

		internal static void RunTest(
            ITest		test,
            TextWriter	outStream)
        {
            ITestResult result = test.Perform();

			outStream.WriteLine(result.ToString());
            if (result.GetException() != null)
            {
                outStream.WriteLine(result.GetException().StackTrace);
            }
        }

		internal static Stream GetTestDataAsStream(
			string name)
		{
			string fullName = "crypto.test.data." + name;

			return Assembly.GetExecutingAssembly().GetManifestResourceStream(fullName);
		}

		public abstract void PerformTest();
    }
}
