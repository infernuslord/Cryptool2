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
    [EditorInfo("startcenter", false, false, false, false, true)]
    [Author("Sven Rech", "rech@cryptool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Startcenter.Properties.Resources", false, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "Startcenter/startcenter.png")]
    public class StartcenterEditor : IEditor
    {
        private string _samplesDir;

        public event PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public delegate void StartupBehaviourChangedHandler(bool showOnStartup);
        public static event StartupBehaviourChangedHandler StartupBehaviourChanged;

        public bool ShowOnStartup
        {
            set { _startcenter.StartupCheckbox.IsChecked = value; }
        }

        public ISettings Settings
        {
            get { return null; }
        }

        private readonly Startcenter.Startcenter _startcenter = new Startcenter.Startcenter();
        public UserControl Presentation
        {
            get { return _startcenter; }
        }
        
        public UserControl QuickWatchPresentation
        {
            get { return _startcenter; }
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
            _startcenter.StartupBehaviourChanged += (showOnStartup) => StartupBehaviourChanged(showOnStartup);
            _startcenter.OnOpenEditor += (content, title, filename) => OnOpenEditor(content, title, filename);
            _startcenter.OnOpenTab += (content, title, parent) => OnOpenTab(content, title, parent);
            _startcenter.TemplatesDir = _samplesDir;

            OnProjectTitleChanged(this, "Startcenter");
            Presentation.ToolTip = "Startcenter";
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

        public string CurrentFile
        {
            get { return null; }
        }

        public string SamplesDir
        {
            set { _samplesDir = value; }
        }

        public List<EditorSpecificPluginInfo> EditorSpecificPlugins
        {
            get { return null; }
        }

        public bool ReadOnly { get; set; }


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
