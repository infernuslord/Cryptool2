#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.Editor;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.Properties;
using KeyTextBox;
using Survey.model;
using Survey.model.question;
using WorkspaceManager;
using WorkspaceManager.Model;
using Path = System.IO.Path;
using ValidationType = System.Xml.ValidationType;

#endregion

namespace Survey {
    /// <summary>
    ///   Interaction logic for SurveyControl.xaml
    /// </summary>
    [Localization("SurveyModel.Properties.Resources")]
    public partial class SurveyControl : UserControl {

        private readonly Dictionary<SurveyQuestionType, Action<SurveyQuestion, StackPanel>> answerPrinter =
            new Dictionary<SurveyQuestionType, Action<SurveyQuestion, StackPanel>>();
        
        public void Initialize() {   
            InitializeComponent();
            OuterScrollViewer.Focus();

            answerPrinter.Add(SurveyQuestionType.Checkbox, PrintCheckboxAnswers);
            answerPrinter.Add(SurveyQuestionType.Combobox, PrintComboboxAnswers);
            answerPrinter.Add(SurveyQuestionType.Range, PrintRangeAnswers);
            answerPrinter.Add(SurveyQuestionType.Number, PrintNumberAnswers);
        }
        
        private void OnSubmitButtonClicked(object sender, RoutedEventArgs e) {
           
        }

        /// <summary>
        /// Clears the Questions stackpanel
        /// </summary>
        public void Clear() {
            inputStack.Children.Clear();
        }

        /// <summary>
        /// Displays the given survey.
        /// </summary>
        /// <param name="localSurvey">The local survey.</param>
        public void DisplaySurvey(SurveyModel localSurvey) {
            try 
            {
                foreach (var question in localSurvey.Questions) 
                {
                    if(!answerPrinter.ContainsKey(question.Type))
                        continue;
                    
                    CreateQuestionText(question, inputStack);
                    answerPrinter[question.Type](question, inputStack);
                }
            } 
            catch (Exception e) 
            {
                Console.Out.WriteLine(e.Message + e.StackTrace);
            }
        }
        /// <summary>
        /// Common method to create the question text.
        /// </summary>
        private void CreateQuestionText(SurveyQuestion question, Panel view) 
        {
            var printTextBlock = new TextBlock {Text = question.GetTextByLanguage("en")};
            view.Children.Add(printTextBlock);
        }

        #region answer Printer

        private void PrintCheckboxAnswers(SurveyQuestion question, StackPanel view) 
        {
            var selectQuestion = (SelectSurveyQuestion) question;
            foreach (var answer in selectQuestion.Answers) 
            {
                view.Children.Add(new CheckBox 
                {
                    Content = answer.GetTextByLanguage("en"),
                    IsChecked = answer.IsSelected
                });
            }
        }

        private void PrintComboboxAnswers(SurveyQuestion question, StackPanel view) 
        {
            var selectQuestion = (SelectSurveyQuestion) question;
            var comboBox = new ComboBox();
            foreach (var answer in selectQuestion.Answers) 
            {
                comboBox.Items.Add(new ComboBoxItem {Content = answer.GetTextByLanguage("en")});
            }
            view.Children.Add(comboBox);
        }


        private void PrintNumberAnswers(SurveyQuestion question, StackPanel view) 
        {
            var numericQuestion = (NumericSurveyQuestion) question;
            var slider = new TextBox();
            view.Children.Add(slider);
        }

        private void PrintRangeAnswers(SurveyQuestion question, StackPanel view) {
            var numericQuestion = (NumericSurveyQuestion) question;
            var slider = new Slider 
            {
                SelectionStart =  numericQuestion.From, 
                SelectionEnd = numericQuestion.To, 
                TickFrequency = 1,
                TickPlacement = TickPlacement.Both
            };
            view.Children.Add(slider);
        }

        #endregion

    }
}
