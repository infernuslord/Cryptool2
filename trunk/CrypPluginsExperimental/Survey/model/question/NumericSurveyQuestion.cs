using System.Collections.Generic;
using System.Xml;

namespace Survey.model.question 
{
    internal class NumericSurveyQuestion : SurveyQuestion 
    {
        /// <summary>
        ///   Gets or sets from.
        /// </summary>
        public int From { get; set; }

        /// <summary>
        ///   Gets or sets to.
        /// </summary>
        public int To { get; set; }

        /// <summary>
        ///   Gets or sets the answered.
        ///   if no answer had been given, -1 is returned
        /// </summary>
        public int Answered { get; set; }

        /// <summary>
        ///   Initializes a new instance of the <see cref="NumericSurveyQuestion" /> class.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        public NumericSurveyQuestion(string id, SurveyQuestionType type) : base(id, type) 
        {
            Answered = -1;
        }
    }
}
