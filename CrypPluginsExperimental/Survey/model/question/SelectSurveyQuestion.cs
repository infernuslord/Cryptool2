using System.Collections.Generic;
using System.Xml;

namespace Survey.model.question
{
   class SelectSurveyQuestion : SurveyQuestion
   {
        /// <summary>
        /// Gets or sets the answers of which the user may select some.
        /// </summary>
        public List<SelectSurveyAnswer>  Answers { get; set; }

       /// <summary>
        /// Initializes a new instance of the <see cref="SelectSurveyQuestion"/> class.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        public SelectSurveyQuestion(string id, SurveyQuestionType type) : base(id, type) {}
    }

    public class SelectSurveyAnswer 
    { 
        /// <summary>
        /// Gets or sets the answer identifier.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Gets or sets the answer text.
        /// </summary>
        public Dictionary<string, string> Text { get; set; }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        public Dictionary<string, string> Help { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this answer has been selected
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is selected]; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Gets the text of an answer depending on the given language.
        /// </summary>
        /// <param name="lang">language identifier</param>
        /// <returns>The answer Text in the given language or null if lang not exists</returns>
        public string GetTextByLanguage(string lang) 
        {
            return Text.ContainsKey(lang.ToLower()) ? Text[lang.ToLower()] : null;
        }

        /// <summary>
        /// Gets the help of an answer depending on the given language.
        /// </summary>
        /// <param name="lang">language identifier</param>
        /// <returns>The Help Text in the given language or null if lang not exists</returns>
        public string GetHelpByLanguage(string lang) 
        {
            return Help.ContainsKey(lang.ToLower()) ? Help[lang.ToLower()] : null;
        }
    }
}
