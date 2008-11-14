using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media;
using CrypUiPluginBase;
using Cryptool.Core;

namespace SimpleEditor
{

    [PluginInfo("F7A18919-BEF7-4489-8A40-FCA6792F17BE", "Simple Editor", "A simple Cryptool editor", "detailed description", "SimpleEditor.icon.png")]
    public class SimpleEditor : IEditor
    {
        public IPluginManager PluginManager { get; set; }
        private SimpleEditorSettings settings;
        private UserControlSimpleEditor usrCtrlSimpleEditor;
        // this is the plugin that was last selected
        private IPlugin selectedPlugin = null;

        private List<UserControl> inputControl = null;
        private List<UserControl> outputControl = null;

        private FileStream inputFileStream;
        private FileStream outputFileStream;
        private StreamReader inputStreamReader;
        private StreamWriter ouputStreamWriter;
        
        
        public SimpleEditor()
        {
            this.settings = new SimpleEditorSettings();
            this.usrCtrlSimpleEditor = new UserControlSimpleEditor(this.selectedPlugin, this.inputControl, this.outputControl);
        }

        public IEditorSettings Settings
        {
            get { return this.settings; }
            set { this.settings = (SimpleEditorSettings)value; }
        }

        #region IEditor Members

        public void AddPlugin(IPlugin plugin)
        {
            this.SelectedPlugin = plugin;

            this.inputControl = new List<UserControl>();
            this.outputControl = new List<UserControl>();

            this.usrCtrlSimpleEditor = new UserControlSimpleEditor(this.selectedPlugin, this.inputControl, this.outputControl);
        }

        public string LoadProject(string path)
        { return null; }
        public bool CanRun
        {
          get { return true; }
        }

        public void Run()
        {
            if (this.selectedPlugin == null)
            {
                System.Windows.MessageBox.Show("Please select a plugin first!");
            }
            else
            {
                int inputControlIndex = 0;
                foreach (PropertyInformation pInfo in this.selectedPlugin.GetInputProperties())
                {
                        RadioButton radioButtonString = ((InputUsrCtrl)this.inputControl[inputControlIndex]).radioButtonString;
                        RadioButton radioButtonFile = ((InputUsrCtrl)this.inputControl[inputControlIndex]).radioButtonFile;
                        TextBox textBoxString = ((InputUsrCtrl)this.inputControl[inputControlIndex]).textBoxString;
                        TextBox textBoxFile = ((InputUsrCtrl)this.inputControl[inputControlIndex]).textBoxFile;

                        if ((bool)radioButtonFile.IsChecked)
                        {
                            try
                            {
                                this.inputFileStream = new FileStream(textBoxFile.Text, FileMode.Open);
                                this.inputStreamReader = new StreamReader(this.inputFileStream);
                                if (pInfo.Property.GetType() == typeof(Stream))
                                    this.selectedPlugin.SetProperty(pInfo.Property, this.inputStreamReader.BaseStream);
                                else if (pInfo.Property.GetType() == typeof(String))
                                    this.selectedPlugin.SetProperty(pInfo.Property, this.inputStreamReader.ReadToEnd());
                                else if(pInfo.Property.GetType() == typeof(byte[]))
                                {
                                    byte[] buffer  = new byte[(int)this.inputStreamReader.BaseStream.Length];
                                    BinaryReader reader = new BinaryReader(this.inputStreamReader.BaseStream);
                                    buffer = reader.ReadBytes((int)this.inputStreamReader.BaseStream.Length);
                                    reader.Close();
                                    this.selectedPlugin.SetProperty(pInfo.Property, buffer);
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Windows.MessageBox.Show(ex.ToString());
                                ((InputUsrCtrl)this.inputControl[inputControlIndex]).textBoxFile.Background = Brushes.Red;
                            }
                        }
                        else if ((bool)radioButtonString.IsChecked)
                        {
                            if (pInfo.Property.GetType() == typeof(Stream))
                                this.selectedPlugin.SetProperty(pInfo.Property, new System.IO.MemoryStream(Encoding.Default.GetBytes(textBoxString.Text)));
                            else if (pInfo.Property.GetType() == typeof(String))
                                this.selectedPlugin.SetProperty(pInfo.Property, textBoxString.Text);
                            else if (pInfo.Property.GetType() == typeof(byte[]))
                                this.selectedPlugin.SetProperty(pInfo.Property, Encoding.Default.GetBytes(textBoxString.Text));
                        }
                        inputControlIndex++;
                }
                
                if (this.selectedPlugin is IHashAlgorithm)
                {
                    IHashAlgorithm hashAlgo = this.selectedPlugin as IHashAlgorithm;
                    hashAlgo.Hash();
                }
                if (this.selectedPlugin is IEncryptionAlgorithm)
                {
                    IEncryptionAlgorithm encAlgo = this.selectedPlugin as IEncryptionAlgorithm;

                    switch ((EncryptionAlgorithmAction)encAlgo.Settings.Action)
                    {
                        case EncryptionAlgorithmAction.Decrypt:
                            encAlgo.Decrypt();
                            break;
                        case EncryptionAlgorithmAction.Encrypt:
                            encAlgo.Encrypt();
                            break;
                    }
                }


                if (this.selectedPlugin is IHashAlgorithm)
                {
                    int outputControlIndex = 0;
                    foreach (PropertyInformation pInfo in this.selectedPlugin.GetOutputProperties())
                    {

                            RadioButton radioButtonString = ((OutputUsrCtrl)this.outputControl[outputControlIndex]).radioButtonString;
                            RadioButton radioButtonFile = ((OutputUsrCtrl)this.outputControl[outputControlIndex]).radioButtonFile;
                            TextBox textBoxString = ((OutputUsrCtrl)this.outputControl[outputControlIndex]).textBoxString;
                            TextBox textBoxFile = ((OutputUsrCtrl)this.outputControl[outputControlIndex]).textBoxFile;

                            byte[] data = (byte[])this.selectedPlugin.GetProperty(pInfo.Property);

                            StringBuilder strHash = new StringBuilder();
                            for (int i = 0; i < data.Length; i++)
                            {
                                strHash.Append(data[i].ToString("X2") + " ");
                            }
                            if ((bool)radioButtonFile.IsChecked)
                            {

                                try
                                {
                                    this.outputFileStream = new FileStream(textBoxFile.Text, FileMode.Create);
                                    this.ouputStreamWriter = new StreamWriter(this.outputFileStream);
                                    this.ouputStreamWriter.Write(strHash.ToString());
                                }
                                catch
                                {
                                    textBoxFile.Background = Brushes.Red;
                                }
                            }
                            else if ((bool)radioButtonString.IsChecked)
                            {
                                textBoxString.Text = strHash.ToString();
                            } 
                            outputControlIndex++;
                        }
                        
                }
                if (this.selectedPlugin is IEncryptionAlgorithm)
                {
                    int outputControlIndex = 0;
                    foreach (PropertyInformation pInfo in this.selectedPlugin.GetOutputProperties())
                    {
                        System.IO.MemoryStream ms = (System.IO.MemoryStream)this.selectedPlugin.GetProperty(pInfo.Property);
                        ((OutputUsrCtrl)this.outputControl[outputControlIndex]).textBoxString.Text = Encoding.Default.GetString(ms.GetBuffer());
                        outputControlIndex++;
                    }
                }
                if (this.inputStreamReader != null)
                    this.inputStreamReader.Close();
                if (this.ouputStreamWriter != null)
                    this.ouputStreamWriter.Close();
                if (this.inputFileStream != null)
                    this.inputFileStream.Close();
                if (this.outputFileStream != null)
                    this.outputFileStream.Close();
            }
        }

        public void RunSelected()
        {
        }

        public string SaveProject() { return null; }

        public string SaveProject(string path) { return null; }

        public void NewProject() { }

        public IPlugin SelectedPlugin
        {
            get
            {
                return this.selectedPlugin;
            }
            set
            {
                this.selectedPlugin = (IPlugin)(value);
            }
        }

        public event SelectedPluginChangedHandler SelectedPluginChanged;

        public bool CanStop
        {
          get { return true; }
        }

        public void Stop()
        {
        }

        #endregion

        #region IPlugin Members

        public void Dispose()
        {
        }

        public void Initialize()
        {
        }

        #endregion

        #region IPlugin Members

        public string Title { get; set; }

        public bool HasChanges { get; set; }

        public event StatusBarProgressbarValueChangedHandler OnStatusBarProgressbarValueChanged;

        public event StatusBarTextChangedHandler OnStatusBarTextChanged;

        public event RegisterPluginHandler OnRegisterPlugin;

        public System.Windows.Controls.UserControl PresentationControl
        {
            get
            {
                return this.usrCtrlSimpleEditor;
            }
        }

        public void PostExecution()
        {
          
        }

        public void PreExecution()
        {
          
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
