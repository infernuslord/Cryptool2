#region

using System.Collections.Generic;

#endregion

namespace Survey.model.question {

    /// <summary>
    /// Represents a Question within a SurveyModel
    /// </summary>
    public class SurveyQuestion
    {

        #region properties

        /// <summary>
        /// Gets or sets the question identifier.
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public SurveyQuestionType Type { get; set; }

        /// <summary>
        /// Gets or sets the question text.
        /// </summary>
        public Dictionary<string, string> Text { get; set; }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        public Dictionary<string, string> Help { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyQuestion"/> class.
        /// </summary>
        public SurveyQuestion(string id, SurveyQuestionType type) {
            ID = id;
            Type = type;
        }

        /// <summary>
        /// Gets the Question.
        /// </summary>
        /// <param name="lang">language identifier</param>
        /// <returns>The Question Text in the given language or null if lang not exists</returns>
        public string GetTextByLanguage(string lang) {
            return Text.ContainsKey(lang.ToLower()) ? Text[lang.ToLower()] : null;
        }

        /// <summary>
        /// Gets the help.
        /// </summary>
        /// <param name="lang">language identifier</param>
        /// <returns>The Help Text in the given language or null if lang not exists</returns>
        public string GetHelpByLanguage(string lang) {
            return Help.ContainsKey(lang.ToLower()) ? Help[lang.ToLower()] : null;
        }

    }

    /// <summary>
    /// Represents the different Types of Questions within the SurveyModel
    /// </summary>
    public enum SurveyQuestionType {
        Range,
        Number,
        Checkbox,
        Combobox
    }
}
