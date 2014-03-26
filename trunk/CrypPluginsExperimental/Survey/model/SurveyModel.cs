using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Survey.model.question;

namespace Survey.model
{
   public class SurveyModel 
   {
       /// <summary>
       /// Gets or sets the build number for which the SurveyModel belongs.
       /// </summary> 
       public string BuildNumber { get; set; }

        /// <summary>
       /// Gets or sets the user identifier.
       /// </summary>
       public string UserID { get; set; }

       /// <summary>
       /// Gets or sets the questions.
       /// </summary> 
       public List<SurveyQuestion> Questions { get; set; }

       /// <summary>
       /// Initializes a new instance of the <see cref="SurveyModel"/> class.
       /// </summary>
       /// <param name="doc">The document.</param>
       /// <exception cref="System.FormatException">XML appears to have no root</exception>
       public SurveyModel(XmlDocument doc) : this()
       {
            CreateFromXML(doc);
       }

       /// <summary>
       /// Initializes a new instance of the <see cref="SurveyModel"/> class.
       /// </summary>
       public SurveyModel() {
           Questions = new List<SurveyQuestion>();
       }

       #region deserilize

       /// <summary>
       /// Creates from XML.
       /// </summary>
       /// <param name="doc">The document.</param>
       /// <exception cref="System.FormatException">XML appears to have no root</exception>
       public void CreateFromXML(XmlDocument doc) 
       {
           if (doc.DocumentElement == null) 
               throw new FormatException("XML appears to have no root");

           BuildNumber = doc.DocumentElement.GetAttribute("version");
           UserID = doc.DocumentElement.GetAttribute("uid");
           
           // extract questions
           Questions = new List<SurveyQuestion>();
           var questionNodes = doc.SelectNodes("//questions//question");
           if (questionNodes == null) 
               return;

           foreach (XmlNode question in questionNodes) 
           { 
               var answerRoot = question.SelectSingleNode("answers");
               if (answerRoot == null) 
                   continue;

               if (answerRoot.Attributes == null || question.Attributes == null) 
                   continue;
               
               // get answers type
               SurveyQuestionType type;
               Enum.TryParse(answerRoot.Attributes["type"].Value, out type);

               //get question id  
               var id = question.Attributes["id"].Value;

               //create and load surveyquestion instance depending on the type
               SurveyQuestion surveyQuestion;
               if (type.Equals(SurveyQuestionType.Number) || type.Equals(SurveyQuestionType.Range)) 
               {
                   surveyQuestion = ExtractNumericQuestion(answerRoot, id, type);
               } 
               else // combobox or checkbox
               {
                   surveyQuestion = ExtractSelectionQuestion(answerRoot, id, type);
               }

               //couldnt extract question
               if (surveyQuestion == null) 
                   continue;

               //fill common properties of the question
               surveyQuestion.Text = ExtractI18NDictionary(question, "text");
               surveyQuestion.Help = ExtractI18NDictionary(question, "help");
               Questions.Add(surveyQuestion);
           }
       }

       /// <summary>
       /// Extracts a selection question from the given answerRoot.
       /// </summary>
       /// <param name="answerRoot">The answer root.</param>
       /// <param name="id">The identifier.</param>
       /// <param name="type">The type.</param>
       /// <returns></returns>
       private static SurveyQuestion ExtractSelectionQuestion(XmlNode answerRoot, string id, SurveyQuestionType type) {
           //extract question answers from xml
           var answerList = new List<SelectSurveyAnswer>();
           var answer = answerRoot.SelectNodes("answer");

           if (answer == null)
               return null;

           // get all possible answers
           foreach (XmlNode answerNode in answer) {
               if (answerNode.Attributes == null) {
                   continue;
               }

               //has the answer been selected previously?
               var isSelected = false;
               var selectedAttribute = answerNode.Attributes["selected"];
               if (selectedAttribute != null) {
                   isSelected = bool.Parse(selectedAttribute.Value);
               }

               //add answer to list
               answerList.Add(new SelectSurveyAnswer {
                   ID = answerNode.Attributes["id"].Value,
                   Text = ExtractI18NDictionary(answerNode, "text"),
                   Help = ExtractI18NDictionary(answerNode, "help"),
                   IsSelected = isSelected
               });
           }

           return new SelectSurveyQuestion(id, type) {
               Answers = answerList
           };
       }

       /// <summary>
       /// Extracts a numeric question from the given answerRoot.
       /// </summary>
       /// <param name="answerRoot">The answer root.</param>
       /// <param name="id">The identifier.</param>
       /// <param name="type">The type.</param>
       /// <returns></returns>
       private static SurveyQuestion ExtractNumericQuestion(XmlNode answerRoot, string id, SurveyQuestionType type) {
           
           //get given answer if pressent
           var answered = -1;
           var answeredNode = answerRoot.SelectSingleNode("answered");
           if (answeredNode != null) {
               answered = int.Parse(answeredNode.InnerText);
           }

           // get question answer range
           if (answerRoot.Attributes == null || answerRoot.Attributes["from"] == null || answerRoot.Attributes["to"] == null) {
               return null;
           }

           return new NumericSurveyQuestion(id, type) {
               From = int.Parse(answerRoot.Attributes["from"].Value),
               To = int.Parse(answerRoot.Attributes["to"].Value),
               Answered = answered
           };
       }

       /// <summary>
       /// Extracts a dictionary of the I18Ned Text.
       /// </summary>
       /// <param name="question">The question.</param>
       /// <param name="nodeName">Name of the node.</param>
       /// <returns></returns>
       private static Dictionary<string, string> ExtractI18NDictionary(XmlNode question, string nodeName) 
       {
           var i18NDictionary = new Dictionary<string, string>();

           var textNodes = question.SelectNodes(nodeName);
           if (textNodes != null) 
           {
               foreach (XmlNode textNode in textNodes) 
               {
                   if (textNode.Attributes == null) 
                       continue; // this only happens if xml format is invalid

                   i18NDictionary.Add(textNode.Attributes["lang"].Value.ToLower(), textNode.InnerText);
               }
           }
           return i18NDictionary;
       }

       #endregion

       #region serilize
       
       /// <summary>
       /// Returns a XMLDocument of this object
       /// </summary>
       /// <returns></returns>
       public XDocument ToXML() {
           return new XDocument(new XElement("survey"));
       }

       #endregion

   }
}
