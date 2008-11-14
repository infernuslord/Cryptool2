using System;
using System.IO;

using NUnit.Framework;

using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Test;

namespace Org.BouncyCastle.OpenSsl.Tests
{
	/**
	* basic class for reading test.pem - the password is "secret"
	*/
	[TestFixture]
	public class ReaderTest
		: SimpleTest
	{
		private static readonly string TestPemData =
			  "-----BEGIN UNRECOGNISED TYPE-----\n"
			+ "blahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblah\n"
			+ "blahblahblahblahblahblahblahblahblahblahblahblahblahblahblahblah\n"
			+ "-----END UNRECOGNISED TYPE-----\n"
			+ "-----BEGIN DSA PRIVATE KEY-----\n"
			+ "Proc-Type: 4,ENCRYPTED\n"
			+ "DEK-Info: DES-EDE3-CBC,079DD7CA8A9BAA19\n"
			+ "\n"
			+ "jIqIT0DIvUlwvkREv6gnCTVFdOsoVwUS9nxcVuQ8JOwg+TB42GnhgZ5x6MFczgNd\n"
			+ "Dw9L60zono+ethYfYB91CdIcVULzvg61/DCAFDjIFPgOIYTVNmteUq75Dmt6wDTV\n"
			+ "4A07iMwjwBk+0YHaeVwcr0AkdvXEcVySOGdGrwy10K8Eq/kSndzm1Sm6tUCJ45+v\n"
			+ "zSUelLlT9fgRTmAbeowT1/tlt/q52bGTcXJzIWBxDuOxK5ASCTjk28kri9WeRUb/\n"
			+ "iCnXn+cqR3BvkDCdlhk2A6A6Q8U9vo0m1MPtwwsolz76jnxbtBNr6Sc+zcny0brP\n"
			+ "jCPMP1qF+IIk01N3YsAZ9mbJmXYoFf9B0VwNUjPudWlSqhvmzzanatevgZ9TID8G\n"
			+ "Mnltd2XsqIgdvu0JhhEJRC6n7hBvn+l7iwYKtBoK+rJmEWWttkoP82UGgjzjzqKa\n"
			+ "rEtdaZPUCEiBLqHPyiIaDWm5Pe4sZZqrV8Ix8vDLKK59ZUkgmYXyCul2yL/cfO5T\n"
			+ "gjMqh9EeQCrNsu3hJGuEEE3MlskQkCpEspm9qDGbKPkMno7CE37plVopx0o0oovE\n"
			+ "S8MZn0v2lIzV8yZ6krtcEA==\n"
			+ "-----END DSA PRIVATE KEY-----\n"
			+ "-----BEGIN RSA PRIVATE KEY-----\n"
			+ "Proc-Type: 4,ENCRYPTED\n"
			+ "DEK-Info: DES-EDE3-CBC,9AC3CD3B4ED42426\n"
			+ "\n"
			+ "AzAvQsDkxXGdPMlFjqO0lYXSlypkfGJWIMqij00cZI1B6K7pMnBHo3wIYStlKDg2\n"
			+ "SMMTYwJfsbFA8+cyCyzsqVBQC63ZgEnStMrDOt/U0SGXNb/C4mFsf1ksnAI6y/3J\n"
			+ "3lhuPAZbMKCmANslivbj9gp4hSe828btXDhvIPKKJgKHhwHveeW9JDulySAN5KYn\n"
			+ "s4aeasvSWVBOYtefFaS+NzrXjDnPtOMX6TQDxd9CE8es1RQbU9Ze2CEa9S7l4coj\n"
			+ "0abWVTjxu55foDCDzbukkQm+eU0aaI4OoydoOCz6bcOfADgRx3/fJ9Z9IuS5w73O\n"
			+ "BiF8Z4S8e7GYpgsPVEqj06PuNH7t6bwzKC9U2FTiNcejZIYPqz21d9iA4hsEfFDd\n"
			+ "0DlXqYUKMazE+TjU8goss5cEqyLNDQmWLYLKUE2p8QmSZC/UqEsZ9pu1VZz3xpFT\n"
			+ "k2sju26HCV14XPG5oi9LpxDQaVRmXBMFxz5b+RmUwPfZuWntTzHQXuNyH2J3B9FN\n"
			+ "el8gaskt4hxHI28hB/eF0h7pOIBUDiR9n/BB+feOrNW8y7/ThSkdI3OK0+dyOzLg\n"
			+ "go9s7SGz5WLyy8FWevw0k2Is4IpCueTwclDrOAPixW+paHhDGIasnvvCsz5jGqQz\n"
			+ "4F4JPmA2ZYg/Z8dYob/ZeF/3KuJqFZAm9gseGS19sT7UzVvvmSf5IXOFfpb7VdRn\n"
			+ "ScFMRVr8PY9ANGh7zYg/8w+G3JJj4C7djA3RB/yOhK4KwbTz/2cnEDI26VREjKs0\n"
			+ "7/WPQXxr/oQ+XEBTnYuJZfa7o7orGL+6wA0Vk/GJqjA=\n"
			+ "-----END RSA PRIVATE KEY-----\n"
			+ "-----BEGIN CERTIFICATE REQUEST-----\n"
			+ "MIIBkDCB+gIBADAlMSMwIQYDVQQDExpSZXF1ZXN0ZWQgVGVzdCBDZXJ0aWZpY2F0\n"
			+ "ZTCBnzANBgkqhkiG9w0BAQEFAAOBjQAwgYkCgYEAsO4slP/KdZQsZyYn3asTWDtX\n"
			+ "E1YN+QQbbHELK7boPQa91YHv5DV/SgucThoXXCtSA45d3dQhrEbZ2+HRBarZIylk\n"
			+ "Nc+VmcV1qFX5KsD9wCYPMtdAYYog6jz259yCOKPDXPm787Q5t9h2zV3Ml1i0eWhC\n"
			+ "cdRYiWHQ5g20W4Bq3GsCAwEAAaAsMCoGCSqGSIb3DQEJDjEdMBswGQYDVR0RBBIw\n"
			+ "EIEOdGVzdEB0ZXN0LnRlc3QwDQYJKoZIhvcNAQELBQADgYEAZGPA0Jyw49cHPJjG\n"
			+ "bloqKAPNlBO200AiRFsHnkOQ1DopJff3mW+FdszAc9g6rTB4/YAiM4r0E314e0vm\n"
			+ "XSlW2q8sp+c2XJO7PUIdJIuAUnvSmMb/uwXFP2SzLdjLcmymMsnFfjvwkht0K2it\n"
			+ "O5HuUDuhLnxEimGlUEBrfkdrsH0=\n"
			+ "-----END CERTIFICATE REQUEST-----\n"
			+ "-----BEGIN NEW CERTIFICATE REQUEST-----\n"
			+ "MIIBkDCB+gIBADAlMSMwIQYDVQQDExpSZXF1ZXN0ZWQgVGVzdCBDZXJ0aWZpY2F0\n"
			+ "ZTCBnzANBgkqhkiG9w0BAQEFAAOBjQAwgYkCgYEAsO4slP/KdZQsZyYn3asTWDtX\n"
			+ "E1YN+QQbbHELK7boPQa91YHv5DV/SgucThoXXCtSA45d3dQhrEbZ2+HRBarZIylk\n"
			+ "Nc+VmcV1qFX5KsD9wCYPMtdAYYog6jz259yCOKPDXPm787Q5t9h2zV3Ml1i0eWhC\n"
			+ "cdRYiWHQ5g20W4Bq3GsCAwEAAaAsMCoGCSqGSIb3DQEJDjEdMBswGQYDVR0RBBIw\n"
			+ "EIEOdGVzdEB0ZXN0LnRlc3QwDQYJKoZIhvcNAQELBQADgYEAZGPA0Jyw49cHPJjG\n"
			+ "bloqKAPNlBO200AiRFsHnkOQ1DopJff3mW+FdszAc9g6rTB4/YAiM4r0E314e0vm\n"
			+ "XSlW2q8sp+c2XJO7PUIdJIuAUnvSmMb/uwXFP2SzLdjLcmymMsnFfjvwkht0K2it\n"
			+ "O5HuUDuhLnxEimGlUEBrfkdrsH0=\n"
			+ "-----END NEW CERTIFICATE REQUEST-----\n"
			+ "-----BEGIN CERTIFICATE-----\n"
			+ "MIIDXjCCAsegAwIBAgIBBzANBgkqhkiG9w0BAQQFADCBtzELMAkGA1UEBhMCQVUx\n"
			+ "ETAPBgNVBAgTCFZpY3RvcmlhMRgwFgYDVQQHEw9Tb3V0aCBNZWxib3VybmUxGjAY\n"
			+ "BgNVBAoTEUNvbm5lY3QgNCBQdHkgTHRkMR4wHAYDVQQLExVDZXJ0aWZpY2F0ZSBB\n"
			+ "dXRob3JpdHkxFTATBgNVBAMTDENvbm5lY3QgNCBDQTEoMCYGCSqGSIb3DQEJARYZ\n"
			+ "d2VibWFzdGVyQGNvbm5lY3Q0LmNvbS5hdTAeFw0wMDA2MDIwNzU2MjFaFw0wMTA2\n"
			+ "MDIwNzU2MjFaMIG4MQswCQYDVQQGEwJBVTERMA8GA1UECBMIVmljdG9yaWExGDAW\n"
			+ "BgNVBAcTD1NvdXRoIE1lbGJvdXJuZTEaMBgGA1UEChMRQ29ubmVjdCA0IFB0eSBM\n"
			+ "dGQxFzAVBgNVBAsTDldlYnNlcnZlciBUZWFtMR0wGwYDVQQDExR3d3cyLmNvbm5l\n"
			+ "Y3Q0LmNvbS5hdTEoMCYGCSqGSIb3DQEJARYZd2VibWFzdGVyQGNvbm5lY3Q0LmNv\n"
			+ "bS5hdTCBnzANBgkqhkiG9w0BAQEFAAOBjQAwgYkCgYEArvDxclKAhyv7Q/Wmr2re\n"
			+ "Gw4XL9Cnh9e+6VgWy2AWNy/MVeXdlxzd7QAuc1eOWQkGQEiLPy5XQtTY+sBUJ3AO\n"
			+ "Rvd2fEVJIcjf29ey7bYua9J/vz5MG2KYo9/WCHIwqD9mmG9g0xLcfwq/s8ZJBswE\n"
			+ "7sb85VU+h94PTvsWOsWuKaECAwEAAaN3MHUwJAYDVR0RBB0wG4EZd2VibWFzdGVy\n"
			+ "QGNvbm5lY3Q0LmNvbS5hdTA6BglghkgBhvhCAQ0ELRYrbW9kX3NzbCBnZW5lcmF0\n"
			+ "ZWQgY3VzdG9tIHNlcnZlciBjZXJ0aWZpY2F0ZTARBglghkgBhvhCAQEEBAMCBkAw\n"
			+ "DQYJKoZIhvcNAQEEBQADgYEAotccfKpwSsIxM1Hae8DR7M/Rw8dg/RqOWx45HNVL\n"
			+ "iBS4/3N/TO195yeQKbfmzbAA2jbPVvIvGgTxPgO1MP4ZgvgRhasaa0qCJCkWvpM4\n"
			+ "yQf33vOiYQbpv4rTwzU8AmRlBG45WdjyNIigGV+oRc61aKCTnLq7zB8N3z1TF/bF\n"
			+ "5/8=\n"
			+ "-----END CERTIFICATE-----\n"
			+ "-----BEGIN X509 CERTIFICATE-----\n"
			+ "MIIDXjCCAsegAwIBAgIBBzANBgkqhkiG9w0BAQQFADCBtzELMAkGA1UEBhMCQVUx\n"
			+ "ETAPBgNVBAgTCFZpY3RvcmlhMRgwFgYDVQQHEw9Tb3V0aCBNZWxib3VybmUxGjAY\n"
			+ "BgNVBAoTEUNvbm5lY3QgNCBQdHkgTHRkMR4wHAYDVQQLExVDZXJ0aWZpY2F0ZSBB\n"
			+ "dXRob3JpdHkxFTATBgNVBAMTDENvbm5lY3QgNCBDQTEoMCYGCSqGSIb3DQEJARYZ\n"
			+ "d2VibWFzdGVyQGNvbm5lY3Q0LmNvbS5hdTAeFw0wMDA2MDIwNzU2MjFaFw0wMTA2\n"
			+ "MDIwNzU2MjFaMIG4MQswCQYDVQQGEwJBVTERMA8GA1UECBMIVmljdG9yaWExGDAW\n"
			+ "BgNVBAcTD1NvdXRoIE1lbGJvdXJuZTEaMBgGA1UEChMRQ29ubmVjdCA0IFB0eSBM\n"
			+ "dGQxFzAVBgNVBAsTDldlYnNlcnZlciBUZWFtMR0wGwYDVQQDExR3d3cyLmNvbm5l\n"
			+ "Y3Q0LmNvbS5hdTEoMCYGCSqGSIb3DQEJARYZd2VibWFzdGVyQGNvbm5lY3Q0LmNv\n"
			+ "bS5hdTCBnzANBgkqhkiG9w0BAQEFAAOBjQAwgYkCgYEArvDxclKAhyv7Q/Wmr2re\n"
			+ "Gw4XL9Cnh9e+6VgWy2AWNy/MVeXdlxzd7QAuc1eOWQkGQEiLPy5XQtTY+sBUJ3AO\n"
			+ "Rvd2fEVJIcjf29ey7bYua9J/vz5MG2KYo9/WCHIwqD9mmG9g0xLcfwq/s8ZJBswE\n"
			+ "7sb85VU+h94PTvsWOsWuKaECAwEAAaN3MHUwJAYDVR0RBB0wG4EZd2VibWFzdGVy\n"
			+ "QGNvbm5lY3Q0LmNvbS5hdTA6BglghkgBhvhCAQ0ELRYrbW9kX3NzbCBnZW5lcmF0\n"
			+ "ZWQgY3VzdG9tIHNlcnZlciBjZXJ0aWZpY2F0ZTARBglghkgBhvhCAQEEBAMCBkAw\n"
			+ "DQYJKoZIhvcNAQEEBQADgYEAotccfKpwSsIxM1Hae8DR7M/Rw8dg/RqOWx45HNVL\n"
			+ "iBS4/3N/TO195yeQKbfmzbAA2jbPVvIvGgTxPgO1MP4ZgvgRhasaa0qCJCkWvpM4\n"
			+ "yQf33vOiYQbpv4rTwzU8AmRlBG45WdjyNIigGV+oRc61aKCTnLq7zB8N3z1TF/bF\n"
			+ "5/8=\n"
			+ "-----END X509 CERTIFICATE-----\n"
			+ "-----BEGIN ATTRIBUTE CERTIFICATE-----\n"
			+ "MIIBuDCCASECAQEwZ6BlMGCkXjBcMQswCQYDVQQGEwJBVTEoMCYGA1UEChMfVGhl\n"
			+ "IExlZ2lvbiBvZiB0aGUgQm91bmN5IENhc3RsZTEjMCEGA1UECxMaQm91bmN5IFBy\n"
			+ "aW1hcnkgQ2VydGlmaWNhdGUCARSgYjBgpF4wXDELMAkGA1UEBhMCQVUxKDAmBgNV\n"
			+ "BAoTH1RoZSBMZWdpb24gb2YgdGhlIEJvdW5jeSBDYXN0bGUxIzAhBgNVBAsTGkJv\n"
			+ "dW5jeSBQcmltYXJ5IENlcnRpZmljYXRlMA0GCSqGSIb3DQEBBQUAAgEBMCIYDzIw\n"
			+ "MDUwNjEwMDI0MTMzWhgPMjAwNTA2MTAwMjQzMTNaMBkwFwYDVRhIMRAwDoEMREFV\n"
			+ "MTIzNDU2Nzg5MA0GCSqGSIb3DQEBBQUAA4GBALAYXT9zdxSR5zdPLAon1xIPehgI\n"
			+ "NZhjM7w0uu3OdzSV5sC31X1Kx9vi5RIWiM9VimRTwbQIod9POttD5QMXCwQb/fm7\n"
			+ "eiJqL2YBIXOeClB19VrQe8xQtMFbyuFpDiM7QdvIam9ShZZMEMGjv9QHI64M4b0G\n"
			+ "odUBlSsJwPPQjZSU\n"
			+ "-----END ATTRIBUTE CERTIFICATE-----\n"
			+ "-----BEGIN X509 CRL-----\n"
			+ "MIICjTCCAfowDQYJKoZIhvcNAQECBQAwXzELMAkGA1UEBhMCVVMxIDAeBgNVBAoT\n"
			+ "F1JTQSBEYXRhIFNlY3VyaXR5LCBJbmMuMS4wLAYDVQQLEyVTZWN1cmUgU2VydmVy\n"
			+ "IENlcnRpZmljYXRpb24gQXV0aG9yaXR5Fw05NTA1MDIwMjEyMjZaFw05NTA2MDEw\n"
			+ "MDAxNDlaMIIBaDAWAgUCQQAABBcNOTUwMjAxMTcyNDI2WjAWAgUCQQAACRcNOTUw\n"
			+ "MjEwMDIxNjM5WjAWAgUCQQAADxcNOTUwMjI0MDAxMjQ5WjAWAgUCQQAADBcNOTUw\n"
			+ "MjI1MDA0NjQ0WjAWAgUCQQAAGxcNOTUwMzEzMTg0MDQ5WjAWAgUCQQAAFhcNOTUw\n"
			+ "MzE1MTkxNjU0WjAWAgUCQQAAGhcNOTUwMzE1MTk0MDQxWjAWAgUCQQAAHxcNOTUw\n"
			+ "MzI0MTk0NDMzWjAWAgUCcgAABRcNOTUwMzI5MjAwNzExWjAWAgUCcgAAERcNOTUw\n"
			+ "MzMwMDIzNDI2WjAWAgUCQQAAIBcNOTUwNDA3MDExMzIxWjAWAgUCcgAAHhcNOTUw\n"
			+ "NDA4MDAwMjU5WjAWAgUCcgAAQRcNOTUwNDI4MTcxNzI0WjAWAgUCcgAAOBcNOTUw\n"
			+ "NDI4MTcyNzIxWjAWAgUCcgAATBcNOTUwNTAyMDIxMjI2WjANBgkqhkiG9w0BAQIF\n"
			+ "AAN+AHqOEJXSDejYy0UwxxrH/9+N2z5xu/if0J6qQmK92W0hW158wpJg+ovV3+wQ\n"
			+ "wvIEPRL2rocL0tKfAsVq1IawSJzSNgxG0lrcla3MrJBnZ4GaZDu4FutZh72MR3Gt\n"
			+ "JaAL3iTJHJD55kK2D/VoyY1djlsPuNh6AEgdVwFAyp0v\n"
			+ "-----END X509 CRL-----";

		private static readonly string Pkcs7PemData =
			  "-----BEGIN PKCS7-----\n"
			+ "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\n"
			+ "4oX2oNPE9Vr3H9Y8YaVoeUU+Kiqo5g==\n"
			+ "-----END PKCS7-----";

		private static readonly string ECPemData =
			  "-----BEGIN EC PARAMETERS-----\n"
			+ "BgUrgQQAIg==\n"
			+ "-----END EC PARAMETERS-----\n"
			+ "-----BEGIN EC PRIVATE KEY-----\n"
			+ "MIGkAgEBBDCSBU3vo7ieeKs0ABQamy/ynxlde7Ylr8HmyfLaNnMrjAwPp9R+KMUE\n"
			+ "hB7zxSAXv9KgBwYFK4EEACKhZANiAQQyyolMpg+TyB4o9kPWqafHIOe8o9K1glus\n"
			+ "+w2sY8OIPQQWGb5i5LdAyi/SscwU24rZM0yiL3BHodp9ccwyhLrFYgXJUOQcCN2d\n"
			+ "no1GMols5497in5gL5+zn0yMsRtyv5o=\n"
			+  "-----END EC PRIVATE KEY-----\n";



		private class Password
			: IPasswordFinder
		{
			private readonly char[] password;

			public Password(
				char[] word)
			{
				this.password = (char[]) word.Clone();
			}

			public char[] GetPassword()
			{
				return (char[]) password.Clone();
			}
		}

		public override string Name
		{
			get { return "PEMReaderTest"; }
		}

		public override void PerformTest()
		{
			TextReader fRd = new StringReader(TestPemData);
			IPasswordFinder pGet = new Password("secret".ToCharArray());
			PemReader pemRd = new PemReader(fRd, pGet);
			AsymmetricCipherKeyPair pair;

			object o;
			while ((o = pemRd.ReadObject()) != null)
			{
//				if (o is AsymmetricCipherKeyPair)
//				{
//					ackp = (AsymmetricCipherKeyPair)o;
//
//					Console.WriteLine(ackp.Public);
//					Console.WriteLine(ackp.Private);
//				}
//				else
//				{
//					Console.WriteLine(o.ToString());
//				}
			}

			//
			// pkcs 7 data
			//
//			fRd =new BufferedReader(new StreamReader(this.getClass().getResourceAsStream("pkcs7.pem")));
			fRd = new StringReader(Pkcs7PemData);
			pemRd = new PemReader(fRd);

			ContentInfo d = (ContentInfo)pemRd.ReadObject();    
	
			if (!d.ContentType.Equals(CmsObjectIdentifiers.EnvelopedData))
			{
				Fail("failed envelopedData check");
			}

			//
			// ECKey
			//
//			fRd = new BufferedReader(new InputStreamReader(this.getClass().getResourceAsStream("eckey.pem")));
			fRd = new StringReader(ECPemData);
			pemRd = new PemReader(fRd);

			// TODO Resolve return type issue with EC keys and fix PemReader to return parameters
//			ECNamedCurveParameterSpec spec = (ECNamedCurveParameterSpec)pemRd.ReadObject();

			pair = (AsymmetricCipherKeyPair)pemRd.ReadObject();
			ISigner sgr = SignerUtilities.GetSigner("ECDSA");

			sgr.Init(true, pair.Private);

			byte[] message = new byte[] { (byte)'a', (byte)'b', (byte)'c' };

			sgr.BlockUpdate(message, 0, message.Length);

			byte[] sigBytes = sgr.GenerateSignature();

			sgr.Init(false, pair.Public);

			sgr.BlockUpdate(message, 0, message.Length);

			if (!sgr.VerifySignature(sigBytes))
			{
				Fail("EC verification failed");
			}

			// TODO Resolve this issue with the algorithm name, study Java version
//			if (!((ECPublicKeyParameters) pair.Public).AlgorithmName.Equals("ECDSA"))
//			{
//				Fail("wrong algorithm name on public");
//			}
//
//			if (!((ECPrivateKeyParameters) pair.Private).AlgorithmName.Equals("ECDSA"))
//			{
//				Fail("wrong algorithm name on private");
//			}

			//
			// writer/parser test
			//
			IAsymmetricCipherKeyPairGenerator kpGen = GeneratorUtilities.GetKeyPairGenerator("RSA");
			kpGen.Init(
				new RsaKeyGenerationParameters(
				BigInteger.ValueOf(0x10001),
				new SecureRandom(),
				768,
				25));

			pair = kpGen.GenerateKeyPair();

			keyPairTest("RSA", pair);

//			kpGen = KeyPairGenerator.getInstance("DSA");
//			kpGen.initialize(512, new SecureRandom());
			DsaParametersGenerator pGen = new DsaParametersGenerator();
			pGen.Init(512, 80, new SecureRandom());

			kpGen = GeneratorUtilities.GetKeyPairGenerator("DSA");
			kpGen.Init(
				new DsaKeyGenerationParameters(
					new SecureRandom(),
					pGen.GenerateParameters()));

			pair = kpGen.GenerateKeyPair();

			keyPairTest("DSA", pair);

			//
			// PKCS7
			//
			MemoryStream bOut = new MemoryStream();
			PemWriter pWrt = new PemWriter(new StreamWriter(bOut));

			pWrt.WriteObject(d);
			pWrt.Writer.Close();

			pemRd = new PemReader(new StreamReader(new MemoryStream(bOut.ToArray(), false)));
			d = (ContentInfo)pemRd.ReadObject();    

			if (!d.ContentType.Equals(CmsObjectIdentifiers.EnvelopedData))
			{
				Fail("failed envelopedData recode check");
			}


#if BC_BUILD_MONODEVELOP
			// TODO Can get MonoDevelop to put embedded resources at a path?
#else
			// OpenSSL test cases (as embedded resources)
			doOpenSslDsaTest("unencrypted");
			doOpenSslRsaTest("unencrypted");

			doOpenSslTests("aes128");
			doOpenSslTests("aes192");
			doOpenSslTests("aes256");
			doOpenSslTests("blowfish");
			doOpenSslTests("des1");
			doOpenSslTests("des2");
			doOpenSslTests("des3");
			doOpenSslTests("rc2_128");

			doOpenSslDsaTest("rc2_40_cbc");
			doOpenSslRsaTest("rc2_40_cbc");
			doOpenSslDsaTest("rc2_64_cbc");
			doOpenSslRsaTest("rc2_64_cbc");
#endif
		}

		private void keyPairTest(
			string					name,
			AsymmetricCipherKeyPair	pair) 
		{
			MemoryStream bOut = new MemoryStream();
			PemWriter pWrt = new PemWriter(new StreamWriter(bOut));

			pWrt.WriteObject(pair.Public);
			pWrt.Writer.Close();

			PemReader pemRd = new PemReader(new StreamReader(new MemoryStream(bOut.ToArray(), false)));

			AsymmetricKeyParameter pubK = (AsymmetricKeyParameter) pemRd.ReadObject();
			if (!pubK.Equals(pair.Public))
			{
				Fail("Failed public key read: " + name);
			}

			bOut = new MemoryStream();
			pWrt = new PemWriter(new StreamWriter(bOut));

			pWrt.WriteObject(pair.Private);
			pWrt.Writer.Close();

			pemRd = new PemReader(new StreamReader(new MemoryStream(bOut.ToArray(), false)));

			AsymmetricCipherKeyPair kPair = (AsymmetricCipherKeyPair) pemRd.ReadObject();
			if (!kPair.Private.Equals(pair.Private))
			{
				Fail("Failed private key read: " + name);
			}
	        
			if (!kPair.Public.Equals(pair.Public))
			{
				Fail("Failed private key public read: " + name);
			}
		}

		private void doOpenSslTests(
			string baseName)
		{
			doOpenSslDsaModesTest(baseName);
			doOpenSslRsaModesTest(baseName);
		}

		private void doOpenSslDsaModesTest(
			string baseName)
		{
			doOpenSslDsaTest(baseName + "_cbc");
			doOpenSslDsaTest(baseName + "_cfb");
			doOpenSslDsaTest(baseName + "_ecb");
			doOpenSslDsaTest(baseName + "_ofb");
		}

		private void doOpenSslRsaModesTest(
			string baseName)
		{
			doOpenSslRsaTest(baseName + "_cbc");
			doOpenSslRsaTest(baseName + "_cfb");
			doOpenSslRsaTest(baseName + "_ecb");
			doOpenSslRsaTest(baseName + "_ofb");
		}

		private void doOpenSslDsaTest(
			string name)
		{
			string fileName = "dsa.openssl_dsa_" + name + ".pem";

			doOpenSslTestFile(fileName, typeof(DsaPrivateKeyParameters));
		}

		private void doOpenSslRsaTest(
			string name)
		{
			string fileName = "rsa.openssl_rsa_" + name + ".pem";

			doOpenSslTestFile(fileName, typeof(RsaPrivateCrtKeyParameters));
		}

		private void doOpenSslTestFile(
			string	fileName,
			Type	expectedPrivKeyType)
		{
			Stream data = GetTestDataAsStream("openssl." + fileName);
			TextReader tr = new StreamReader(data);
			PemReader pr = new PemReader(tr, new Password("changeit".ToCharArray()));
			AsymmetricCipherKeyPair kp = pr.ReadObject() as AsymmetricCipherKeyPair;
			data.Close();

			if (kp == null)
			{
				Fail("Didn't find OpenSSL key");
			}

			if (!expectedPrivKeyType.IsInstanceOfType(kp.Private))
			{
				Fail("Returned key not of correct type");
			}
		}

		public static void Main(
			string[] args)
		{
			RunTest(new ReaderTest());
		}

		[Test]
		public void TestFunction()
		{
			string resultText = Perform().ToString();

			Assert.AreEqual(Name + ": Okay", resultText);
		}
	}
}
