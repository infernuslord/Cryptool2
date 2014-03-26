using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using System.Xml.Linq;
using Survey.model;

namespace Survey.communication
{
    class FileCommunicator : ICommunicator
    {
      
        /// <summary>
        /// Valid characters for an random created userID
        /// </summary>
        private const string UserIDChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        /// <summary>
        /// The user identifier length
        /// 62^11 = 2^64
        /// (actually its 62^10.75 but who's counting)
        /// </summary>
        private const int UserIDLength = 11;
        private readonly Random rng = new Random();

        /// <summary>
        /// Gets or sets the path to the xml file
        /// </summary>
        public string FilePath { get; private set; }

        public FileCommunicator(string filePath) {
            FilePath = filePath;
        }

        /// <summary>
        /// Fetches the survey.
        /// if none server presend ill create a new with a 11 digit long random userID and a buildNumber of -1
        /// </summary>
        /// <returns></returns>
        public SurveyModel FetchSurvey() {
            if (!File.Exists(FilePath))
            {
                return new SurveyModel() {BuildNumber = "-1", UserID = CreateRandomUserID() };
            }

            var doc = new XmlDocument();
            doc.Load(FilePath);
            return new SurveyModel(doc);
        }

        public void PushSurvey(SurveyModel survey) {
            new XDocument(survey.ToXML()).Save(FilePath);
        }

        /// <summary>
        /// Creates the random user identifier.
        /// </summary>
        private string CreateRandomUserID() {
            var buffer = new char[UserIDLength];

            for (var i = 0; i < UserIDLength; i++) {
                buffer[i] = UserIDChars[rng.Next(UserIDChars.Length)];
            }
            return new string(buffer);
        }
    }
}
