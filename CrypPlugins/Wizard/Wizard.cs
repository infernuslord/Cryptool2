using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Cryptool.Core;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.Editor;

namespace Wizard
{
    [TabColor("royalblue")]
    [EditorInfo("wizard", true, false, false, false)]
    [Author("Simone Sauer", "sauer@cryptool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Wizard.Properties.Resources", false, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "Wizard/wizard.png")]
    public class Wizard : IEditor
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event SelectedPluginChangedHandler OnSelectedPluginChanged;
        public event ProjectTitleChangedHandler OnProjectTitleChanged;
        public event OpenProjectFileHandler OnOpenProjectFile;
        public event EditorSpecificPluginsChanged OnEditorSpecificPluginsChanged;
        public event OpenTabHandler OnOpenTab;
        public event OpenEditorHandler OnOpenEditor;

        public Wizard()
        {
            wizardControl.OnOpenEditor += (editor, title, filename) => OnOpenEditor(editor, title, filename);
            wizardControl.OnOpenTab += (content, title, parent) => OnOpenTab(content, title, parent);
            wizardControl.OnGuiLogNotificationOccured += (sender, args) => OnGuiLogNotificationOccured(this, new GuiLogEventArgs(args.Message, this, args.NotificationLevel));
        }

        private WizardSettings wizardSettings = new WizardSettings();
        public ISettings Settings
        {
            get { return wizardSettings; }
        }

        private WizardControl wizardControl = new WizardControl();
        public UserControl Presentation
        {
            get { return wizardControl; }
        }

        public void Initialize()
        {
            wizardControl.Initialize();
        }

        public void Dispose()
        {
            wizardControl.StopCurrentWorkspaceManager();
        }

        public void ShowHelp()
        {
            OnOpenTab(this.GetDescriptionDocument(), "Wizard Description", this);
        }

        public void ShowSelectedPluginDescription()
        {
            ShowHelp();
        }

        public void Execute()
        {
            wizardControl.ExecuteCurrentWorkspaceManager();
        }

        public void Stop()
        {
            wizardControl.StopCurrentWorkspaceManager();
        }

        public bool CanExecute
        {
            get { return wizardControl.WizardCanExecute(); }
        }

        public bool CanStop
        {
            get { return wizardControl.WizardCanStop(); }
        }

        #region unused methods

        public void New()
        {
            
        }

        public PluginManager PluginManager
        { get; set; }

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            
        }

        public void PostExecution()
        {
            
        }

        public void Pause()
        {
            
        }

        public void Open(string fileName)
        {
            
        }

        public void Save(string fileName)
        {
            
        }

        public void Add(Type type)
        {
            
        }

        public void AddEditorSpecific(EditorSpecificPluginInfo espi)
        {
            
        }

        public void DeleteEditorSpecific(EditorSpecificPluginInfo espi)
        {
            
        }

        public void Undo()
        {
            
        }

        public void Redo()
        {
            
        }

        public void Cut()
        {
            
        }

        public void Copy()
        {
            
        }

        public void Paste()
        {
            
        }

        public void Remove()
        {
            
        }

        public void Print()
        {
            
        }
        
        public void Active()
        {
            
        }

        public bool CanUndo
        {
            get { return false; }
        }

        public bool CanRedo
        {
            get { return false; }
        }

        public bool CanCut
        {
            get { return false; }
        }

        public bool CanCopy
        {
            get { return false; }
        }

        public bool CanPaste
        {
            get { return false; }
        }

        public bool CanRemove
        {
            get { return false; }
        }

        public bool HasChanges
        { get; set; }

        public bool CanPrint
        {
            get { return false; }
        }

        public bool CanSave
        {
            get { return false; }
        }

        public string CurrentFile
        {
            get { return null; }
        }

        public string SamplesDir
        {
            set { wizardControl.SamplesDir = value; }
        }

        public List<EditorSpecificPluginInfo> EditorSpecificPlugins { get; set; }

        public bool ReadOnly
        {
            get { return false; }
            set {  }
        }

        #endregion


        public double GetZoom()
        {
            return double.NaN;
        }

        public void Zoom(double value)
        {
            
        }

        public void FitToScreen()
        {
           
        }


        public event EventHandler<Cryptool.PluginBase.Miscellaneous.ZoomChanged> OnZoomChanged;


        public void AddText()
        {
            throw new NotImplementedException();
        }

        public void AddImage()
        {
            throw new NotImplementedException();
        }
    }
}
