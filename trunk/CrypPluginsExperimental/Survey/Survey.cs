#region

using System;
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.Core;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.Editor;
using Survey;
using Survey.communication;

#endregion

namespace Survey {
    [TabColor("royalblue")]
    [EditorInfo("SurveyModel", true, false, false, false)]
    [Author("Christopher Konze", "konze@cryptool.org", "Universität Kassel", "http://www.ais.uni-kassel.de")]
    [PluginInfo("Survey.Properties.Resources", "PluginCaption", "PluginTooltip", "Survey/DetailedDescription/doc.xml", "Survey/survey.png")]
    public class Survey : IEditor {
        
        #region events

        public event PropertyChangedEventHandler PropertyChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event SelectedPluginChangedHandler OnSelectedPluginChanged;
        public event ProjectTitleChangedHandler OnProjectTitleChanged;
        public event OpenProjectFileHandler OnOpenProjectFile;
        public event OpenTabHandler OnOpenTab;
        public event OpenEditorHandler OnOpenEditor;
        public event FileLoadedHandler OnFileLoaded;

        #endregion

        #region private member

        private const string LocalSurveyPath = @"Data\Survey.xml";
        private readonly FileCommunicator fileCommunicator = new FileCommunicator(LocalSurveyPath);

        private SurveyControl surveyControl = new SurveyControl();

        #endregion

        #region properties
        // see also section "things we cant do"

        /// <summary>
        /// Gets or sets the plugin manager.
        /// </summary>
        /// <value>
        /// The plugin manager.
        /// </value>
        public PluginManager PluginManager { get; set; }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public ISettings Settings { get { return null; }}

        /// <summary>
        /// Gets the presentation.
        /// </summary>
        /// <value>
        /// The presentation.
        /// </value>
        public UserControl Presentation {
            set { surveyControl = value as SurveyControl; }
            get { return surveyControl; }}

        #endregion

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize() {
            surveyControl.Initialize();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose() {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Is called if this instances should be reused.
        /// </summary>
        public void New() {
            surveyControl.Clear();
            var localSurvey = fileCommunicator.FetchSurvey();
            surveyControl.DisplaySurvey(localSurvey);
        }

        #region things we cant do

        public bool CanUndo {get { return false; }} 
        public void Undo() {}

        public bool CanRedo {get { return false; }}
        public void Redo() {}
        
        public bool CanCut {get { return false; }}
        public void Cut() {}

        public bool CanCopy {get { return false; }}
        public void Copy() {}

        public bool CanPaste {get { return false; }}
        public void Paste() {}

        public bool CanRemove {get { return false; }}
        public void Remove() {}

        public bool CanExecute {get { return false; }}
        public void Execute() {}

        public bool CanStop {get { return false; }}
        public void Stop() {}

        public bool HasChanges {get { return false; }}

        public bool CanPrint {get { return false; }}
        public void Print() {}

        public bool CanSave {get { return false; }}
        public void Save(string fileName) {}
        
        #endregion

        #region i dont know what to do with these :)
          
        public string CurrentFile { get; private set; }
        public string SamplesDir { set; private get; }
        public bool ReadOnly { get; set; }
        
        public void Open(string fileName) {
            throw new NotImplementedException();
        }
        
        public void Add(Type type) {
            throw new NotImplementedException();
        }

        public void AddText() {
            throw new NotImplementedException();
        }

        public void AddImage() {
            throw new NotImplementedException();
        }

        public void ShowSelectedEntityHelp() {
            throw new NotImplementedException();
        }

        #endregion
    }
}
