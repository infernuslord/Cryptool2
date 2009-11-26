using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media;
using Cryptool.PluginBase.Editor;
using Cryptool.PluginBase.IO;
using Cryptool.Core;
using System.Windows.Threading;
using System.Threading;

/*
 * TODO:
 * - Respect mandatory/optional flag
 * - Execute only when mandatory properties are set
 * - Catch Exceptions
 * - Add support for more types
 * - Re-enable support for files
 * - Support settings
 */
namespace SimpleEditor
{
    [PluginInfo(false, "Simple Editor", "A simple Cryptool editor", "", "SimpleEditor/icon.png")]
    public class SimpleEditor : IEditor
    {
        public PluginManager PluginManager { get; set; }
        private SimpleEditorSettings settings;
        private UserControlSimpleEditor usrCtrlSimpleEditor;
        // this is the currently active plugin
        private IPlugin plugin = null;

        private List<PropertyInfoAttribute> inputProps;
        private List<PropertyInfoAttribute> outputProps;
        
        public SimpleEditor()
        {
            this.settings = new SimpleEditorSettings();
            this.usrCtrlSimpleEditor = new UserControlSimpleEditor();

            this.inputProps = new List<PropertyInfoAttribute>();
            this.outputProps = new List<PropertyInfoAttribute>();
        }

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (SimpleEditorSettings)value; }
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IEditor Members

        public event ChangeDisplayLevelHandler OnChangeDisplayLevel;

        public event SelectedPluginChangedHandler OnSelectedPluginChanged;

        public event ProjectTitleChangedHandler OnProjectTitleChanged;

        public event OpenProjectFileHandler OnOpenProjectFile;

        public event EditorSpecificPluginsChanged OnEditorSpecificPluginsChanged;

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
            if (type == null)
                return;

            plugin = type.CreateObject();

            if (plugin == null)
                return;

            // TODO: projectManager.Add required?

            inputProps.Clear();
            outputProps.Clear();

            foreach (PropertyInfoAttribute pInfo in this.plugin.GetProperties())
            {
                if (pInfo.Direction == Direction.InputData)
                {
                    inputProps.Add(pInfo);
                }
                else if (pInfo.Direction == Direction.OutputData)
                {
                    outputProps.Add(pInfo);
                }
            }

            usrCtrlSimpleEditor.DisplayControls(plugin, inputProps, outputProps);
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

        public void ShowHelp()
        {
        }

        public void ShowSelectedPluginDescription()
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

        public bool CanExecute
        {
            get { return true; }
        }

        public DisplayLevel DisplayLevel
        {
            get;
            set;
        }

        public List<EditorSpecificPluginInfo> EditorSpecificPlugins
        {
            get { return new List<EditorSpecificPluginInfo>(); }
        }

        public bool CanStop
        {
            get { return false; }
        }

        public bool HasChanges
        {
            get { return false; }
        }

        #endregion

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public UserControl Presentation
        {
            get { return usrCtrlSimpleEditor; }
        }

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void Execute()
        {
            if (this.plugin == null)
            {
                System.Windows.MessageBox.Show("Please select a plugin first!");
                return;
            }

            foreach (PropertyInfoAttribute pInfo in inputProps)
            {
                if (usrCtrlSimpleEditor.IsUsingTextBox(pInfo))
                {
                    Type propType = pInfo.PropertyInfo.PropertyType;
                    String text = usrCtrlSimpleEditor.GetBoxText(pInfo);

                    object value = null;

                    // TODO: should use some more generic string to propType parsing/casting

                    if (propType == typeof(Int32))
                        value = int.Parse(text);
                    else if (propType == typeof(String))
                        value = text;
                    else if (propType == typeof(byte[]))
                        value = Encoding.Default.GetBytes(text);

                    plugin.SetPropertyValue(pInfo.PropertyInfo, value);
                }
            }

            plugin.Execute();

            foreach (PropertyInfoAttribute pInfo in outputProps)
            {
                if (usrCtrlSimpleEditor.IsUsingTextBox(pInfo))
                {
                    object obj = pInfo.PropertyInfo.GetValue(plugin, null);

                    usrCtrlSimpleEditor.SetBoxText(pInfo, obj.ToString());
                }
            }

        }

        public void Pause()
        {
        }


        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public void PreExecution()
        {
        }

        public void PostExecution()
        {
        }

        #endregion
    }
}
