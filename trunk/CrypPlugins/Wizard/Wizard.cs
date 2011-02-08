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
    [TabColor("white")]
    [EditorInfo("wizard")]
    [Author("Simone Sauer", "sauer@cryptool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Wizard.Resources.Attributes", false, "Wizard", "The CrypTool 2.0 wizard", "Wizard/DetailedDescription/Description.xaml",
      "Wizard/wizard.png")]
    class Wizard : IEditor
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
        
        public Wizard()
        {
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
            
        }

        public void Dispose()
        {
            
        }

        public void ShowHelp()
        {

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

        public void Execute()
        {
            
        }

        public void PostExecution()
        {
            
        }

        public void Pause()
        {
            
        }

        public void Stop()
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

        public void ShowSelectedPluginDescription()
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

        public bool CanExecute
        {
            get { return false; }
        }

        public bool CanStop
        {
            get { return false; }
        }

        public bool HasChanges
        { get; set; }

        public bool CanPrint
        {
            get { return false; }
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
    }
}
