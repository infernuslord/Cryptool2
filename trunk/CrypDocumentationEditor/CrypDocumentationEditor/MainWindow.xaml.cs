/*                              
   Copyright 2011 Nils Kopal, Uni Duisburg-Essen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualBasic;

namespace CrypDocumentationEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool hasChanges = false;
        private Documentation docu;
        private string[] languages;
        private List<Reference> _references = new List<Reference>();

        private string language1 = Documentation.DEFAULT_LANGUAGE;
        private string language2 = Documentation.DEFAULT_LANGUAGE;

        public bool HasChanges { 
            get{
                return hasChanges;
            }
            set
            {
                if (value)
                {
                    this.Title = "CrypDocumentationEditor*";
                }
                else
                {
                    this.Title = "CrypDocumentationEditor";
                }
                hasChanges = value;
            }
        }

        public MainWindow()
        {            
            InitializeComponent();            
            NewButton_Click(null, null);
        }

        private void GenerateLanguageSelector()
        {
            languages = docu.GetLanguages();            
            this.DocuLanguage.ItemsSource = languages;
            this.DocuLanguage.SelectedIndex = 0;
            this.DocuLanguage2.ItemsSource = languages;
            this.DocuLanguage2.SelectedIndex = 0;
        }

        private void BoldButton_Click(object sender, RoutedEventArgs e)
        {
            if (Introduction.IsFocused)
            {
                Introduction.Selection.Start.InsertTextInRun("<b>");
                Introduction.Selection.End.InsertTextInRun("</b>");                
            }
            if (Usage.IsFocused)
            {
                Usage.Selection.Start.InsertTextInRun("<b>");
                Usage.Selection.End.InsertTextInRun("</b>");
            }
            if (Presentation.IsFocused)
            {
                Presentation.Selection.Start.InsertTextInRun("<b>");
                Presentation.Selection.End.InsertTextInRun("</b>");
            }
            if (Introduction2.IsFocused)
            {
                Introduction2.Selection.Start.InsertTextInRun("<b>");
                Introduction2.Selection.End.InsertTextInRun("</b>");
            }
            if (Usage2.IsFocused)
            {
                Usage2.Selection.Start.InsertTextInRun("<b>");
                Usage2.Selection.End.InsertTextInRun("</b>");
            }
            if (Presentation2.IsFocused)
            {
                Presentation2.Selection.Start.InsertTextInRun("<b>");
                Presentation2.Selection.End.InsertTextInRun("</b>");
            }
        }

        private void ItalicButton_Click(object sender, RoutedEventArgs e)
        {
            if (Introduction.IsFocused)
            {
                Introduction.Selection.Start.InsertTextInRun("<i>");
                Introduction.Selection.End.InsertTextInRun("</i>");
            }
            if (Usage.IsFocused)
            {
                Usage.Selection.Start.InsertTextInRun("<i>");
                Usage.Selection.End.InsertTextInRun("</i>");
            }
            if (Presentation.IsFocused)
            {
                Presentation.Selection.Start.InsertTextInRun("<i>");
                Presentation.Selection.End.InsertTextInRun("</i>");
            }
            if (Introduction2.IsFocused)
            {
                Introduction2.Selection.Start.InsertTextInRun("<i>");
                Introduction2.Selection.End.InsertTextInRun("</i>");
            }
            if (Usage2.IsFocused)
            {
                Usage2.Selection.Start.InsertTextInRun("<i>");
                Usage2.Selection.End.InsertTextInRun("</i>");
            }
            if (Presentation2.IsFocused)
            {
                Presentation2.Selection.Start.InsertTextInRun("<i>");
                Presentation2.Selection.End.InsertTextInRun("</i>");
            }
        }

        private void UnderlineButton_Click(object sender, RoutedEventArgs e)
        {
            if (Introduction.IsFocused)
            {
                Introduction.Selection.Start.InsertTextInRun("<u>");
                Introduction.Selection.End.InsertTextInRun("</u>");
            }
            if (Usage.IsFocused)
            {
                Usage.Selection.Start.InsertTextInRun("<u>");
                Usage.Selection.End.InsertTextInRun("</u>");
            }
            if (Presentation.IsFocused)
            {
                Presentation.Selection.Start.InsertTextInRun("<u>");
                Presentation.Selection.End.InsertTextInRun("</u>");
            }
            if (Introduction2.IsFocused)
            {
                Introduction2.Selection.Start.InsertTextInRun("<u>");
                Introduction2.Selection.End.InsertTextInRun("</u>");
            }
            if (Usage2.IsFocused)
            {
                Usage2.Selection.Start.InsertTextInRun("<u>");
                Usage2.Selection.End.InsertTextInRun("</u>");
            }
            if (Presentation2.IsFocused)
            {
                Presentation2.Selection.Start.InsertTextInRun("<u>");
                Presentation2.Selection.End.InsertTextInRun("</u>");
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double height = (e.NewSize.Height - 190) / 4;
            double width = (e.NewSize.Width - 20) / 2;
           
            Introduction.Height = height;
            Introduction2.Height = height;
            Introduction.Width = width;
            Introduction2.Width = width;

            Usage.Height = height;
            Usage2.Height = height;
            Usage.Width = width;
            Usage2.Width = width;

            Presentation.Height = height;
            Presentation2.Height = height;
            Presentation.Width = width;
            Presentation2.Width = width;
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            if (HasChanges)
            {
                string messageBoxText = "You have unsaved changes. Do you want to save changes? Click Yes to save and create a new documentation, No to create a new documentation without saving, or Cancel to not create a new documentation.";
                string caption = "CrypDocumentationEditor";
                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Microsoft.Win32.SaveFileDialog dlg2 = new Microsoft.Win32.SaveFileDialog();
                        dlg2.FileName = "Documentation";
                        dlg2.DefaultExt = ".xml";
                        dlg2.Filter = "XML docu files (.xml)|*.xml";

                        Nullable<bool> result2 = dlg2.ShowDialog();

                        if (result2 == true)
                        {
                            string filename = dlg2.FileName;
                            docu.Introduction = Introduction.Document;
                            docu.Usage = Usage.Document;
                            docu.Presentation = Presentation.Document;
                            docu.Save(filename);
                            HasChanges = false;
                        }
                        else
                        {
                            return;
                        }
                        break;
                    case MessageBoxResult.No:
                        // User pressed No button
                        // ...
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }                                
            }
            
            docu = new Documentation();            

            FlowDocument document = new FlowDocument();
            Paragraph para = new Paragraph();
            para.Inlines.Add(new Run(""));
            document.Blocks.Add(para);
            Introduction.Document = document;

            document = new FlowDocument();
            para = new Paragraph();
            para.Inlines.Add(new Run(""));
            document.Blocks.Add(para);
            Usage.Document = document;

            document = new FlowDocument();
            para = new Paragraph();
            para.Inlines.Add(new Run(""));
            document.Blocks.Add(para);
            Presentation.Document = document;

            document = new FlowDocument();
            para = new Paragraph();
            para.Inlines.Add(new Run(""));
            document.Blocks.Add(para);
            Introduction2.Document = document;

            document = new FlowDocument();
            para = new Paragraph();
            para.Inlines.Add(new Run(""));
            document.Blocks.Add(para);
            Usage2.Document = document;

            document = new FlowDocument();
            para = new Paragraph();
            para.Inlines.Add(new Run(""));
            document.Blocks.Add(para);
            Presentation2.Document = document;

            GenerateLanguageSelector();

            HasChanges = false;
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {

            if (HasChanges)
            {
                string messageBoxText = "You have unsaved changes. Do you want to save changes? Click Yes to save and open a documentation, No to open a documentation without saving, or Cancel to not open a documentation.";
                string caption = "CrypDocumentationEditor";
                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Microsoft.Win32.SaveFileDialog dlg2 = new Microsoft.Win32.SaveFileDialog();
                        dlg2.FileName = "Documentation";
                        dlg2.DefaultExt = ".xml";
                        dlg2.Filter = "XML docu files (.xml)|*.xml";

                        Nullable<bool> result2 = dlg2.ShowDialog();

                        if (result2 == true)
                        {
                            string filename = dlg2.FileName;
                            docu.Introduction = Introduction.Document;
                            docu.Usage = Usage.Document;
                            docu.Presentation = Presentation.Document;
                            docu.Save(filename);
                            HasChanges = false;
                        }
                        else
                        {
                            return;
                        }
                        break;
                    case MessageBoxResult.No:
                        // User pressed No button
                        // ...
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }

            Microsoft.Win32.OpenFileDialog dlg3 = new Microsoft.Win32.OpenFileDialog();
            dlg3.FileName = "Documentation"; 
            dlg3.DefaultExt = ".xml";
            dlg3.Filter = "XML docu files (.xml)|*.xml";

            Nullable<bool> result3 = dlg3.ShowDialog();

            if (result3 == true)
            {                                
                string filename = dlg3.FileName;
                docu = new Documentation();                                       
                docu.Load(filename);

                _references = docu.GetReferences();

                Introduction.Document = docu.Introduction;
                Usage.Document = docu.Usage;
                Presentation.Document = docu.Presentation;
                Introduction2.Document = docu.Introduction;
                Usage2.Document = docu.Usage;
                Presentation2.Document = docu.Presentation;                
                References.ItemsSource = _references;
            }

            GenerateLanguageSelector();
            
            HasChanges = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {         
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Documentation";
            dlg.DefaultExt = ".xml";
            dlg.Filter = "XML docu files (.xml)|*.xml";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {               
                string filename = dlg.FileName;
                docu.Language = language1;
                docu.Introduction = Introduction.Document;
                docu.Usage = Usage.Document;
                docu.Presentation = Presentation.Document;
                if (language2 != null && !language1.Equals(language2))
                {
                    docu.Language = language2;
                    docu.Introduction = Introduction2.Document;
                    docu.Usage = Usage2.Document;
                    docu.Presentation = Presentation2.Document;
                }
                docu.Save(filename);
                HasChanges = false;
            }
        }

        private void Introduction_TextChanged(object sender, TextChangedEventArgs e)
        {
            HasChanges = true;
        }

        private void Usage_TextChanged(object sender, TextChangedEventArgs e)
        {
            HasChanges = true;
        }

        private void Presentation_TextChanged(object sender, TextChangedEventArgs e)
        {
            HasChanges = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (HasChanges)
            {
                string messageBoxText = "You have unsaved changes. Do you want to save changes? Click Yes to save and close, No to close without saving, or Cancel to not close.";
                string caption = "CrypDocumentationEditor";
                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNoCancel,  MessageBoxImage.Warning);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                        dlg.FileName = "Documentation";
                        dlg.DefaultExt = ".xml";
                        dlg.Filter = "XML docu files (.xml)|*.xml";

                        Nullable<bool> result2 = dlg.ShowDialog();

                        if (result2 == true)
                        {
                            string filename = dlg.FileName;
                            docu.Introduction = Introduction.Document;
                            docu.Usage = Usage.Document;
                            docu.Presentation = Presentation.Document;
                            docu.Save(filename);
                            HasChanges = false;
                        }
                        else
                        {
                            return;
                        }
                        break;
                    case MessageBoxResult.No:
                        // User pressed No button
                        // ...
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }

        private void AddLanguage_Click(object sender, RoutedEventArgs e)
        {
            string value = Interaction.InputBox("Please enter new language", "Enter new language", "", 100, 100);
            if (value == "" || languages.Contains(value))
            {
                return;
            }
            this.docu.AddLanguage(value);
            string[] newlanguages = new string[languages.Length + 1];
            Array.Copy(languages, newlanguages, languages.Length);
            newlanguages[newlanguages.Length - 1] = value;
            languages = newlanguages;
            this.DocuLanguage.ItemsSource = languages;
            this.DocuLanguage2.ItemsSource = languages;
            this.DocuLanguage2.SelectedValue = value;
            language2 = value;
            this.HasChanges = true;
        }

        private void AddPictureButton_Click(object sender, RoutedEventArgs e)
        {
            string uri = Interaction.InputBox("Please enter the uri to the picture", "Enter picture uri", "", 100, 100);
            if (uri == "")
            {
                return;
            }

            if (Introduction.IsFocused)
            {
                Introduction.Selection.Start.InsertTextInRun("<img src=\"" + uri + "\"/>");
            }
            if (Usage.IsFocused)
            {
                Usage.Selection.Start.InsertTextInRun("<img src=\"" + uri + "\"/>");
            }
            if (Presentation.IsFocused)
            {
                Presentation.Selection.Start.InsertTextInRun("<img src=\"" + uri + "\"/>");
            }
            if (Introduction2.IsFocused)
            {
                Introduction2.Selection.Start.InsertTextInRun("<img src=\"" + uri + "\"/>");
            }
            if (Usage2.IsFocused)
            {
                Usage2.Selection.Start.InsertTextInRun("<img src=\"" + uri + "\"/>");
            }
            if (Presentation2.IsFocused)
            {
                Presentation2.Selection.Start.InsertTextInRun("<img src=\"" + uri + "\"/>");
            }
        }

        private void AddSectionButton_Click(object sender, RoutedEventArgs e)
        {
            string sectionname = Interaction.InputBox("Please enter the new section name", "Enter section name", "", 100, 100);
            if (sectionname == "")
            {
                return;
            }

            if (Introduction.IsFocused)
            {
                Introduction.Selection.Start.InsertTextInRun("<section headline=\"" + sectionname + "\"/>");
            }
            if (Usage.IsFocused)
            {
                Usage.Selection.Start.InsertTextInRun("<section headline=\"" + sectionname + "\"/>");
            }
            if (Presentation.IsFocused)
            {
                Presentation.Selection.Start.InsertTextInRun("<section headline=\"" + sectionname + "\"/>");
            }
            if (Introduction2.IsFocused)
            {
                Introduction2.Selection.Start.InsertTextInRun("<section headline=\"" + sectionname + "\"/>");
            }
            if (Usage2.IsFocused)
            {
                Usage2.Selection.Start.InsertTextInRun("<section headline=\"" + sectionname + "\"/>");
            }
            if (Presentation2.IsFocused)
            {
                Presentation2.Selection.Start.InsertTextInRun("<section headline=\"" + sectionname + "\"/>");
            }
        }

        private void DocuLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DocuLanguage.SelectedValue == null)
            {
                return;
            }            

            //Save old has changes state
            bool hasChanges = HasChanges;
            docu.Language = language1;

            //save old language entries to xml
            docu.Introduction = Introduction.Document;
            docu.Usage = Usage.Document;
            docu.Presentation = Presentation.Document;
            
            //change to new selected language
            docu.Language = (string)DocuLanguage.SelectedValue;
            language1 = (string)DocuLanguage.SelectedValue;

            //put new selected language entries from xml to view
            Introduction.Document = docu.Introduction;
            Usage.Document = docu.Usage;
            Presentation.Document = docu.Presentation;

            //restore old HasChanges state
            HasChanges = hasChanges;
        }

        private void DocuLanguage2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DocuLanguage2.SelectedValue == null)
            {
                return;
            }

            //Save old has changes state
            bool hasChanges = HasChanges;            

            if (language2 != null)
            {
                docu.Language = language2;

                //save old language entries to xml
                docu.Introduction = Introduction2.Document;
                docu.Usage = Usage2.Document;
                docu.Presentation = Presentation2.Document;
            }
            //change to new selected language
            docu.Language = (string)DocuLanguage2.SelectedValue;
            language2 = (string)DocuLanguage2.SelectedValue;

            //put new selected language entries from xml to view
            Introduction2.Document = docu.Introduction;
            Usage2.Document = docu.Usage;
            Presentation2.Document = docu.Presentation;

            //restore old HasChanges state
            HasChanges = hasChanges;
        }

        private void NewlineButton_Click(object sender, RoutedEventArgs e)
        {
            if (Introduction.IsFocused)
            {
                Introduction.Selection.Start.InsertTextInRun("<newline />");
            }
            if (Usage.IsFocused)
            {
                Usage.Selection.Start.InsertTextInRun("<newline />");
            }
            if (Presentation.IsFocused)
            {
                Presentation.Selection.Start.InsertTextInRun("<newline />");
            }
            if (Introduction2.IsFocused)
            {
                Introduction2.Selection.Start.InsertTextInRun("<newline />");
            }
            if (Usage2.IsFocused)
            {
                Usage2.Selection.Start.InsertTextInRun("<newline />");
            }
            if (Presentation2.IsFocused)
            {
                Presentation2.Selection.Start.InsertTextInRun("<newline />");
            }
        }

        private void AddReferenceButton_Click(object sender, RoutedEventArgs e)
        {
            string link = Interaction.InputBox("Link", "Enter link", "", 100, 100);
            if (link != "")
            {
                string caption = Interaction.InputBox("Caption", "Enter caption", "", 100, 100);
                if (caption == "")
                {
                    return;
                }
                _references.Add(new LinkReference() { Caption = caption, Link = link });
            }
            else
            {
                string author = Interaction.InputBox("Author", "Enter author", "", 100, 100);
                if (author == "")
                {
                    return;
                }
                string publisher = Interaction.InputBox("Publisher", "Enter publisher", "", 100, 100);
                if (publisher == "")
                {
                    return;
                }
                string name = Interaction.InputBox("Name", "Enter name", "", 100, 100);
                if (name == "")
                {
                    return;
                }
                _references.Add(new BookReference() { Author = author, Name = name, Publisher = publisher });
            }

            docu.AddReferences(_references);      
            References.ItemsSource = new List<Reference>(_references);
        }

        private void References_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                string messageBoxText = string.Format("Do you really want to delete the reference '{0}'?",References.SelectedItem is LinkReference ? ((LinkReference)References.SelectedItem).Caption : ((BookReference)References.SelectedItem).Name);
                string caption = "Delete Reference?";
                
                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo,  MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    docu.RemoveReference((Reference)References.SelectedItem);
                    _references.Remove((Reference)References.SelectedItem);
                    References.ItemsSource = new List<Reference>(_references);
                }
            }
        }     
    }
}
