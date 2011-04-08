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
using Cryptool.PluginBase.Miscellaneous;

namespace StartCenter
{
    [TabColor("LightSeaGreen")]
    [EditorInfo("startcenter", true, false, false, false)]
    [Author("Sven Rech", "rech@cryptool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Startcenter.Properties.Resources", false, "Startcenter", "The CrypTool 2.0 startcenter", null, "Startcenter/startcenter.png")]
    class StartcenterEditor : IEditor
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings
        {
            get { return null; }
        }

        readonly Startcenter.Startcenter startcenter = new Startcenter.Startcenter();
        public UserControl Presentation
        {
            get { return startcenter; }
        }
        
        public UserControl QuickWatchPresentation
        {
            get { return startcenter; }
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

        public void Initialize()
        {
            startcenter.OnOpenEditor += (content, title) => OnOpenEditor(content, title);
        }

        public void Dispose()
        {
            
        }

        public PluginManager PluginManager { get; set; }

        public event SelectedPluginChangedHandler OnSelectedPluginChanged;
        public event ProjectTitleChangedHandler OnProjectTitleChanged;
        public event OpenProjectFileHandler OnOpenProjectFile;
        public event EditorSpecificPluginsChanged OnEditorSpecificPluginsChanged;
        public event OpenTabHandler OnOpenTab;
        public event OpenEditorHandler OnOpenEditor;
        public event EventHandler<ZoomChanged> OnZoomChanged;
        public void New()
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

        public double GetZoom()
        {
            return 1.0;
        }

        public void Zoom(double value)
        {
        }

        public void FitToScreen()
        {
        }

        public void ShowHelp()
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
        {
            get { return false; }
        }

        public bool CanPrint
        {
            get { return false; }
        }

        public string SamplesDir
        {
            set { }
        }

        public List<EditorSpecificPluginInfo> EditorSpecificPlugins
        {
            get { return null; }
        }

        public bool ReadOnly { get; set; }
    }
}
