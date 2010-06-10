using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Cryptool.PluginBase;

/*
 * EVERYTHING UNTESTED AND NOT READY!!!
 */

namespace Cryptool.P2P.Helper
{
    public static class PAPCertificate
    {
        #region Variables

        /// <summary>
        /// You can only use the P@P-network by using this user name!!!
        /// All certificates, which are necessary for using the P@P-P2P-Network, are 
        /// registered with this user name.
        /// </summary>
        public const string CERTIFIED_PEER_NAME = "CrypTool2"; //"pap0001"; //"CT_PAP_User";
        public const string CERTIFICATE_DIRECTORY = "pap_certificates";

        private static string sCertPath;
        /// <summary>
        /// only set the application path - without certificate 
        /// directory or a specific certificate file name!
        /// </summary>
        public static string CertPath
        {
            get { return sCertPath; }
            private set
            {
                string combindedCertPath = Path.Combine(value, CERTIFICATE_DIRECTORY);
                if (Directory.Exists(combindedCertPath))
                    sCertPath = combindedCertPath;
                else
                    throw (new DirectoryNotFoundException("Path: " + combindedCertPath + " not found"));
            }
        }

        //private const string CERT_USER_SUBJECT = "CN=pap0001@pap.de@pap0001@.*, O=Peers@Play, S=None, C=NA";
        private const string CERT_USER_FILE_NAME = "CrypTool2.pfx"; //"pap0001.p12"; //ct2_user_pap.cer
        private const string CERT_USER_PASSWORD = "ct2"; //"test"; //CT2p@pC3rt1f1cat10n
        private const string CERT_USER_ISSUER = "P@P project";
        private const string CERT_USER_SERIAL = "1E"; //"0D";

        private const string CERT_PAP_FILE_NAME = "pap.cer";
        private const string CERT_PAP_PASSWORD = "test";
        private const string CERT_PAP_ISSUER = "P@P project";
        private const string CERT_PAP_SERIAL = "37cbb8163698c48845e9d5c80e3496b1";

        private const string CERT_OPA_FILE_NAME = "opa.vs.uni-due.de.cer";
        private const string CERT_OPA_PASSWORD = "";
        private const string CERT_OPA_ISSUER = "opa.vs.uni-due.de";
        private const string CERT_OPA_SERIAL = "2FB799C15BAE2FB74FDB72DD0329196A";

        private const string CERT_SERVER_CA_FILE_NAME = "ServerCA.cer";
        private const string CERT_SERVER_CA_PASSWORD = "";
        private const string CERT_SERVER_CA_ISSUER = "PAP Server CA";
        private const string CERT_SERVER_CA_SERIAL = "23440A89B21FA7B84ADDB59DA494F79B";

        private const X509FindType PAP_FIND_TYPE = X509FindType.FindBySerialNumber;
        // here have to be at least two certificate (PAP Server CA, P@P project)
        private const StoreName CERT_STORE_ROOT = StoreName.Root;
        private const StoreLocation CERT_STORE_LOCATION = StoreLocation.CurrentUser;
        // here has to be one certificate (User certificate)
        private const StoreName CERT_STORE_USER = StoreName.My;

        #endregion

        #region VERY PAP-SPECIFIC CERTIFICATE METHODS

        public enum PAP_Certificates
        {
            Server_CA,
            Opa,
            Pap,
            User
        }

        /// <summary>
        /// Checks if all neccessary certificates were installed, otherwise trying to
        /// install all missing certificates. Returns true if everything worked fine.
        /// </summary>
        /// <param name="sPath">only the path of the running application, not the 
        /// certificate direction or a specific filename!!! Everything else will 
        /// be combined internally!!!</param>
        /// <returns></returns>
        public static bool CheckAvailabilityAndInstallMissingCertificates(string sPath)
        {
            List<PAP_Certificates> lstMissingCerts = new List<PAP_Certificates>();
            lstMissingCerts = CheckAvailabilityOfPAPCertificates(sPath);
            return InstallMissingCertificates(lstMissingCerts, sPath);
        }

        /// <summary>
        /// Checks if all neccessary certificates were installed, otherwise it returns a 
        /// list with all missing certificates
        /// </summary>
        /// <param name="sPath">only the path of the running application, not the 
        /// certificate direction or a specific filename!!! Everything else will 
        /// be combined internally!!!</param>
        /// <returns></returns>
        public static List<PAP_Certificates> CheckAvailabilityOfPAPCertificates(string sPath)
        {
            CertPath = sPath;
            List<PAP_Certificates> retLst = new List<PAP_Certificates>();
            X509Certificate2Collection certColl;

            /* BEGIN: Checking availablity of the three root certificates */

            // Wacker 27.02.2010: Removed checking for Server and OPA (operator-SSL) certificates; These are only needed if 
            // we want to get a new certificate online - a feature which is not implmented now. When this feature is
            // implemented, just uncommentd the following lines

            //certColl = FindCertificates(CERT_STORE_ROOT, CERT_STORE_LOCATION,
            //    PAP_FIND_TYPE, CERT_SERVER_CA_SERIAL, true);
            //if (certColl.Count == 0)
            //    retLst.Add(PAP_Certificates.Server_CA);

            //certColl = FindCertificates(CERT_STORE_ROOT, CERT_STORE_LOCATION,
            //    PAP_FIND_TYPE, CERT_OPA_SERIAL, true);
            //if (certColl.Count == 0)
            //    retLst.Add(PAP_Certificates.Opa);

            certColl = FindCertificates(CERT_STORE_ROOT, CERT_STORE_LOCATION,
                PAP_FIND_TYPE, CERT_PAP_SERIAL, true);
            if (certColl.Count == 0)
                retLst.Add(PAP_Certificates.Pap);
            /* END: Checking availablity of the three root certificates */

            // Check user certificate availability
            certColl = FindCertificates(CERT_STORE_USER, CERT_STORE_LOCATION,
                PAP_FIND_TYPE, CERT_USER_SERIAL, true);
            if (certColl.Count == 0)
                retLst.Add(PAP_Certificates.User);

            return retLst;
        }

        /// <summary>
        /// Installs all certificates, which are specified in the parameter list. 
        /// Returns true, if everything works fine
        /// </summary>
        /// <param name="installList">a list of all certificates, which you want to install</param>
        /// <param name="sPath">only the path of the running application, not the 
        /// certificate direction or a specific filename!!! Everything else will 
        /// be combined internally!!!</param>
        /// <returns></returns>
        public static bool InstallMissingCertificates(List<PAP_Certificates> installList, string sPath)
        {
            bool intermediateResult = true;
            bool actualResult = true;

            CertPath = sPath;

            foreach (PAP_Certificates papCert in installList)
            {
                switch (papCert)
                {
                    case PAP_Certificates.Server_CA:
                        intermediateResult = InstallCertificate(CERT_STORE_ROOT, CERT_STORE_LOCATION,
                            Path.Combine(CertPath, CERT_SERVER_CA_FILE_NAME), CERT_SERVER_CA_PASSWORD);
                        break;
                    case PAP_Certificates.Opa:
                        intermediateResult = InstallCertificate(CERT_STORE_ROOT, CERT_STORE_LOCATION,
                            Path.Combine(CertPath, CERT_OPA_FILE_NAME), CERT_OPA_PASSWORD);
                        break;
                    case PAP_Certificates.Pap:
                        intermediateResult = InstallCertificate(CERT_STORE_ROOT, CERT_STORE_LOCATION,
                            Path.Combine(CertPath, CERT_PAP_FILE_NAME), CERT_PAP_PASSWORD);
                        break;
                    case PAP_Certificates.User:
                        intermediateResult = InstallCertificate(CERT_STORE_USER, CERT_STORE_LOCATION,
                            Path.Combine(CertPath, CERT_USER_FILE_NAME), CERT_USER_PASSWORD);
                        break;
                    default:
                        break;
                }
                actualResult = actualResult && intermediateResult;
            }
            return actualResult;
        }

        #endregion

        #region Common Certificate Operations (Find and Install Certs)

        /// <summary>
        /// Searches for certificates in the given Store with the given FindType and value
        /// </summary>
        /// <param name="storeName">Name of the store</param>
        /// <param name="storeLocation">Location of the store</param>
        /// <param name="findType">choose which attribute of the installed cert's should be compare with the given value</param>
        /// <param name="findValue">value, which should be exactly in the chosen FindType of an installed certificate</param>
        /// <param name="onlyValidCerts">only valid certificates (not outdated, revocated, etc.) will be considered.</param>
        /// <returns>a list of all certificates, who satisfy the search attributes</returns>
        private static X509Certificate2Collection FindCertificates(StoreName storeName, StoreLocation storeLocation, X509FindType findType, object findValue, bool onlyValidCerts)
        {
            X509Certificate2Collection findedCertCol = null;
            X509Store store = new X509Store(storeName, storeLocation);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                findedCertCol = store.Certificates.Find(findType, findValue, onlyValidCerts);
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                store.Close();
            }

            return findedCertCol;
        }

        /// <summary>
        /// Installs a given certificate if it is already valid and not installed yet
        /// </summary>
        /// <param name="storeName">Name of the store</param>
        /// <param name="storeLocation">Location of the store</param>
        /// <param name="sCertPath">Whole certification path and filename</param>
        /// <param name="sCertPassword">if necessary, you have to declare a password. Otherwise use ""</param>
        private static bool InstallCertificate(StoreName storeName, StoreLocation storeLocation, string sCertPath, string sCertPassword)
        {
            bool ret = false;

            if (File.Exists(sCertPath))
            {
                X509Store store = new X509Store(storeName, storeLocation);
                try
                {
                    /* Verification of certifates failed every time - no idea why */
                    //if (cert.Verify())
                    //{
                    X509Certificate2 cert = new X509Certificate2(sCertPath, sCertPassword);
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(cert);
                    store.Close();
                    ret = true;
                    //}
                    //else
                    //{
                    //    throw (new Exception("Installing Certificate " + cert.SubjectName.Name + " wasn't possible, because Certificate isn't valid anymore"));
                    //}
                }
                catch (Exception ex)
                {
                    throw (new Exception("Installation of " + sCertPath + " certificate wasn't possible", ex));
                }
                finally
                {
                    store.Close();
                }
            }
            return ret;
        }

        #endregion

        #region (currently not used) PAP specific stuff (eMail Address registered certificates, etc.)

        /// <summary>
        /// Will search for the root certificate in the windows certificate store.
        /// </summary>
        /// <returns>The root certificate or null on error</returns>
        public static X509Certificate2Collection getRootCertificate()
        {
            X509Certificate2Collection root = FindCertificates(CERT_STORE_ROOT, CERT_STORE_LOCATION, X509FindType.FindBySerialNumber, CERT_PAP_SERIAL, true);
            return root;
        }

        /// <summary>
        /// Gives back an X509Certificate2 from an given email address (hashed value)
        /// </summary>
        /// <param name="email">the email</param>
        /// <returns>the searched certificate</returns> 
        public static X509Certificate2 GetCertificateWithMail(String email)
        {
            try
            {
                String fileName = GetHash(email);
                X509Certificate2Collection col = FindCertificates(StoreName.My, StoreLocation.CurrentUser, X509FindType.FindByIssuerName, CERT_PAP_ISSUER, true);
                foreach (X509Certificate2 cert in col)
                {
                    if (cert.Subject.Contains(fileName))
                    {
                        return cert;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Creates an crypted string from an ordinary
        /// </summary>
        /// <param name="str">the ordinary string</param>
        /// <returns>an coded string</returns>
        public static String GetHash(String str)
        {
            str = str.ToLower();
            System.Security.Cryptography.SHA1 sec = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            Encoding encoding = Encoding.Unicode;
            byte[] insertion = encoding.GetBytes(str);
            byte[] hash = sec.ComputeHash(insertion);
            String result = BitConverter.ToString(hash);//Convert.ToBase64String(hash);
            result = result.Substring(result.Length / 2 + 1);
            return result;
        }

        #endregion

        #region PickCertificate (currently not used)
        /// <summary>
        /// Let the user choose a certificate from given location and store name
        /// </summary>
        /// <param name="location">The location</param>
        /// <param name="name">The store name</param>
        public static void PickCertificate(StoreLocation location, StoreName name)
        {
            X509Certificate2 MyCertificate;
            X509Store store = new X509Store(name, location);
            try
            {
                store.Open(OpenFlags.ReadOnly);

                // Pick a certificate from the store
                MyCertificate = X509Certificate2UI.SelectFromCollection(store.Certificates, "P@P certificates", "Please select your certificate", X509SelectionFlag.SingleSelection)[0];

                // Comment next line to enable selection of an invalid certificate
                ValidateCert(MyCertificate);

                //MyCertificateSerialNumber = MyCertificate.SerialNumber;
            }
            catch (Exception ex)
            {
                MyCertificate = null;
                throw (new Exception("Certificate not valid", ex));
            }
            finally { store.Close(); }
        }
        #endregion

        #region ValidateCert (currently not used)
        /// <summary>
        /// Validates a certificate according to P@P-rules
        /// </summary>
        /// <exception cref="SNALCertificateNotValidException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <param name="cert">Certificate to validate</param>
        public static void ValidateCert(X509Certificate2 cert)
        {
            if (cert == null)
            {
                throw new ArgumentNullException("cert");
            }

            X509Chain chain = new X509Chain();

            // check entire chain for revocation
            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;

            // TODO: Check Online
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            // timeout for online revocation list
            chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 0, 30);

            // TODO: Revocation unknown allowed
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;

            // Check chain
            chain.Build(cert);

            // If there was an error or no root CA is given throw exception
            if (chain.ChainStatus.Length != 0 || chain.ChainElements.Count != 2)
            {
                throw new Exception("Certificates chain not valid!");
            }

            // Check root certificate
            if (chain.ChainElements[1].Certificate.SerialNumber.ToLower() != CERT_PAP_SERIAL.ToLower())
            {
                throw new Exception("Certificates root CA not valid!");
            }
        }
        #endregion

        /// <summary>
        /// Checks if all certificates for using the pap p2p system are installed.
        /// Otherwise it tries to install the missing certificates. If all operations
        /// succeed, return value is true. Only when value is true, you can try
        /// to initialize the PAP System.
        /// </summary>
        /// <returns>If all operations succeed, return value is true. Only when value 
        /// is true, you can try to initialize the PAP System.</returns>
        public static bool CheckAndInstallPAPCertificates()
        {
            bool retValue = false;

            // get exe directory, because there resides the certificate directory
            System.Reflection.Assembly assemb = System.Reflection.Assembly.GetEntryAssembly();
            string applicationDir = System.IO.Path.GetDirectoryName(assemb.Location);
            // check if all necessary certs are installed
            P2PManager.GuiLogMessage("Validating installation of P2P certificates.", NotificationLevel.Info);
            List<PAPCertificate.PAP_Certificates> lstMissingCerts = PAPCertificate.CheckAvailabilityOfPAPCertificates(applicationDir);
            if (lstMissingCerts.Count == 0)
            {
                //GuiLogMessage("All neccessary p2p certificates are installed.", NotificationLevel.Info);
                retValue = true;
            }
            else
            {
                StringBuilder sbMissingCerts = new StringBuilder();
                for (int i = 0; i < lstMissingCerts.Count; i++)
                {
                    sbMissingCerts.AppendLine(Enum.GetName(typeof(PAPCertificate.PAP_Certificates), lstMissingCerts[i]));
                }
                P2PManager.GuiLogMessage("Following certificates are missing. They will be installed now.\n" + sbMissingCerts.ToString(), NotificationLevel.Info);

                // try/catch neccessary because the CT-Editor doesn't support the whole exception display process (e.g. shows only "unknown error.")
                try
                {
                    if (PAPCertificate.InstallMissingCertificates(lstMissingCerts, applicationDir))
                    {
                        P2PManager.GuiLogMessage("Installation of all missing certificates was successful.", NotificationLevel.Info);
                        retValue = true;
                    }
                    else
                    {
                        P2PManager.GuiLogMessage("No/not all missing certificates were installed successful.", NotificationLevel.Error);
                    }
                }
                catch (Exception ex)
                {
                    P2PManager.GuiLogMessage("Error occured while installing certificates. Exception: " + ex.ToString(), NotificationLevel.Error);
                }
            }
            return retValue;
        }
    }
}
