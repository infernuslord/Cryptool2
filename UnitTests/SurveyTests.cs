using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Cryptool.Core.Properties;
using LatticeCrypto.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Survey.communication;
using Survey.model;
using Survey.model.question;

namespace Tests 
{
    [TestClass]
    public class SurveyTests 
    {

        [TestMethod]
        public void MessageSerializationTests() {
            //this test should verify the serialization of inbounding answers
            var doc = new XmlDocument {InnerXml = UnitTests.Properties.Resources.surveyTestQuestions};
            Assert.IsTrue(doc.HasChildNodes);

            var questions = new Survey.model.SurveyModel(doc);

            Assert.AreEqual("123", questions.BuildNumber, "BuildNumber should be 123, according to the xml within the resources");
            Assert.AreEqual("456", questions.UserID, "UserID should be 456, according to the xml within the resources");
            Assert.AreEqual(4, questions.Questions.Count(), "There should be 4 questions, according to the xml within the resources");

            var selectSurveyQuestionCounter = 0;
            var numericSurveyQuestionCounter = 0;
            SurveyQuestionType[] qTypes = {
                SurveyQuestionType.Range, SurveyQuestionType.Number, SurveyQuestionType.Checkbox,
                SurveyQuestionType.Combobox
            };

            foreach (var q in questions.Questions) 
            {
                var i = q.ID;
                Assert.AreEqual("question" + i + "DE", q.GetTextByLanguage("DE"));
                Assert.AreEqual("question" + i + "EN", q.GetTextByLanguage("EN"));
                Assert.AreEqual("help" + i + "DE", q.GetHelpByLanguage("DE"));
                Assert.AreEqual("help" + i + "EN", q.GetHelpByLanguage("EN"));
                Assert.AreEqual(qTypes[int.Parse(i) - 1], q.Type);

                var numericQuestion = q as NumericSurveyQuestion;
                if (numericQuestion != null) {
                    Assert.AreEqual(0, numericQuestion.From);
                    Assert.AreEqual(10, numericQuestion.To);
                    Assert.AreEqual(SurveyQuestionType.Range.Equals(q.Type) ? 3 : -1, numericQuestion.Answered);
                    numericSurveyQuestionCounter++;
                    continue; // we are done
                }

                var selectQuestion = q as SelectSurveyQuestion;
                if (selectQuestion != null) {
                    var answer = selectQuestion.Answers.First();
                    Assert.AreEqual("1", answer.ID);
                    Assert.AreEqual("Antwort1", answer.GetTextByLanguage("DE"));
                    Assert.AreEqual("Answer1", answer.GetTextByLanguage("EN"));
                    Assert.AreEqual("help1DE", answer.GetHelpByLanguage("DE"));
                    Assert.AreEqual("help1EN", answer.GetHelpByLanguage("EN"));

                    Assert.AreEqual(SurveyQuestionType.Checkbox.Equals(selectQuestion.Type) ? 2 : 1, selectQuestion.Answers.Count);
                    Assert.AreEqual(SurveyQuestionType.Checkbox.Equals(selectQuestion.Type) ? 2 : 0, selectQuestion.Answers.FindAll(a => a.IsSelected).Count());
                }
                selectSurveyQuestionCounter++;
            }
            Assert.AreEqual(2, selectSurveyQuestionCounter, "there should have been 2 selectSurveyQuestion");
            Assert.AreEqual(2, numericSurveyQuestionCounter,"there should have been 2 numericSurveyQuestion");
        }

        [TestMethod]
        public void FileCommunicatorTest() {
            // asure that the file doesnt exist.
            if (File.Exists("test.xml")) 
                File.Delete("test.xml");
            
            ICommunicator com = new FileCommunicator("test.xml");
            SurveyModel surveyModel = com.FetchSurvey(); // since there is no xml stored it should create one with a random userid*/

            Assert.AreEqual("-1", surveyModel.BuildNumber);
            Assert.IsTrue(surveyModel.UserID != null && !surveyModel.UserID.Equals(""));
            
            surveyModel.BuildNumber = "123";

            com.PushSurvey(surveyModel); // store new data
            
            Assert.IsTrue(File.Exists("test.xml"), "push survey should have saved the file");
            surveyModel = com.FetchSurvey(); // reload form xml file
            Assert.Fail("unit test incomplete");
            Assert.AreEqual("123", surveyModel.BuildNumber, "buildnumber should have been changed");

        }
    }
}
