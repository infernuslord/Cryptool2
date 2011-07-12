/*
   Copyright 2008 Thomas Schmid, University of Siegen

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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Be.Windows.Forms;
using FileInput.Delegates;
using FileInput.Helper;
using Cryptool.PluginBase;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using System.Windows.Threading;


namespace FileInput.WindowsFormsUserControl
{

    public partial class HexBoxContainer : UserControl
    {
        public event PluginProgressChangedEventHandler OnStatusBarProgressbarValueChanged;
        public event ExceptionOccured OnExceptionOccured;
        public event InformationOccured OnInformationOccured;
        public event SelectionChanged OnSelectionChanged;
        public event FileOpened OnFileOpened;
        public event FileClosed OnFileClosed;

        byte[] _findBuffer = new byte[0];
        string _fileName;

        public HexBoxContainer()
        {
            InitializeComponent();
            Init();
        }

        public void SetSize(System.Windows.Size size)
        {
            this.Width = System.Convert.ToInt32(size.Width);
            this.Height = System.Convert.ToInt32(size.Height);
        }

        void hexBox_SelectionLengthChanged(object sender, EventArgs e)
        {
            if (OnSelectionChanged != null) OnSelectionChanged(this);
            UpdateFileSizeStatus();
        }

        /// <summary>
        /// Initializes the hex editor´s main form
        /// </summary>
        void Init()
        {
            hexBox.SelectionLengthChanged += new EventHandler(hexBox_SelectionLengthChanged);
            hexBox.OnStatusBarProgressbarValueChanged += hexBox_OnStatusBarProgressbarValueChanged;
            DisplayText(null);
            newFileName =
              Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
              @"\CryptoolAnotherEditor.cte";
        }

        void hexBox_OnStatusBarProgressbarValueChanged(object sender, PluginProgressEventArgs args)
        {
            if (this.OnStatusBarProgressbarValueChanged != null)
                OnStatusBarProgressbarValueChanged(null, args);
        }

        /// <summary>
        /// Updates the File size status label
        /// </summary>
        void UpdateFileSizeStatus()
        {
            if (this.hexBox.ByteProvider == null)
                this.toolStripStatusLblFileSieze.Text = string.Empty;
            else
                this.toolStripStatusLblFileSieze.Text = Util.GetDisplayBytes(this.hexBox.ByteProvider.Length);

            if (this._fileName != null)
                this.toolStripStatusLblFile.Text = _fileName;
            else
                this.toolStripStatusLblFile.Text = string.Empty;

            if (_fileName != null && IsReadOnly) toolStripStatusLblFile.Text += " (readonly)";
        }

        /// <summary>
        /// Displays the file name in the Form´s text property
        /// </summary>
        /// <param name="fileName">the file name to display</param>
        void DisplayText(string fileName)
        {
            if (fileName != null && fileName.Length > 0)
            {
                string text = Path.GetFileName(fileName);
                this.Text = string.Format("{0} - Be.HexEditor", text);
            }
            else
            {
                this.Text = "Be.HexEditor";
            }
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public bool CanSave
        {
            get { return true; }
        }

        public bool CanClose
        {
            get { return _fileName != null && _fileName != string.Empty; }
        }

        public bool CanCopy
        {
            get { return hexBox.CanCopy(); }
        }

        public bool CanCut
        {
            get { return hexBox.CanCut(); }
        }

        public bool CanPaste
        {
            get { return hexBox.CanPaste(); }
        }

        public bool CanFind
        {
            get { return CanClose; }
        }

        private bool canCancel;
        public bool CanCancel
        {
            get { return canCancel; }
        }

        private string newFileName;
        public string NewFile()
        {
            if (CanClose) CloseFile();
            FileStream fs = null;
            try
            {
                fs = new FileStream(newFileName, FileMode.Create);
                fs.Close();
                OpenFile(newFileName, false);
                SaveFile();
                return _fileName;
            }
            catch (Exception exception)
            {
                if (OnExceptionOccured != null)
                    OnExceptionOccured(this, exception);
                return null;
            }
        }

        delegate void VoidDelegate();

        /// <summary>
        /// Opens a file.
        /// </summary>
        /// <param name="fileName">the file name of the file to open</param>
        public void OpenFile(string fileName, bool openReadOnly)
        {
            hexBox.Invoke(new VoidDelegate(delegate
            {
                OpenFileImpl(fileName, openReadOnly);
            }), null);
        }

        public void OpenFileImpl(string fileName, bool openReadOnly)
        {
            if (fileName == null || !File.Exists(fileName))
            {
                ExceptionOccured(new Exception("File not found."));
            }

            if (hexBox.ByteProvider != null)
            {
                if (CloseFile() == DialogResult.Cancel)
                    return;
            }

            try
            {
                DynamicFileByteProvider dynamicFileByteProvider = null;
                try
                {
                    // dummy exception to force read-only mode
                    if (openReadOnly) throw new IOException();
                    // try to open in write mode
                    dynamicFileByteProvider = new DynamicFileByteProvider(fileName);
                    IsReadOnly = false;
                    // dynamicFileByteProvider.Changed += new EventHandler(byteProvider_Changed);
                    // dynamicFileByteProvider.LengthChanged += new EventHandler(byteProvider_LengthChanged);
                }
                catch (IOException) // write mode failed
                {
                    try
                    {
                        // try to open in read-only mode
                        dynamicFileByteProvider = new DynamicFileByteProvider(fileName, true);
                        IsReadOnly = true;
                        // dynamicFileByteProvider.Dispose();
                        // return;
                    }
                    catch (IOException ioexception) // read-only also failed
                    {
                        // IsReadOnly = false;
                        if (OnExceptionOccured != null)
                        {
                            // OnExceptionOccured(this, ioexception);
                            OnExceptionOccured(this, ioexception);
                        }
                    }
                }

                hexBox.ByteProvider = dynamicFileByteProvider;
                _fileName = fileName;

                DisplayText(null);

                UpdateFileSizeStatus();
                if (OnFileOpened != null) OnFileOpened(this, new FileOpendedEventArgs(fileName));
            }
            catch (Exception exception)
            {
                if (OnExceptionOccured != null)
                {
                    OnExceptionOccured(this, exception);
                }
            }
            finally
            {

            }
        }

        private bool isReadOnly;
        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set { isReadOnly = value; }
        }


        /// <summary>
        /// Saves the current file.
        /// </summary>
        public void SaveFile()
        {
            if (hexBox.ByteProvider == null)
                return;

            try
            {
                FileByteProvider fileByteProvider = hexBox.ByteProvider as FileByteProvider;
                DynamicByteProvider dynamicByteProvider = hexBox.ByteProvider as DynamicByteProvider;
                DynamicFileByteProvider dynamicFileByteProvider = hexBox.ByteProvider as DynamicFileByteProvider;
                if (fileByteProvider != null)
                {
                    fileByteProvider.ApplyChanges();
                }
                else if (dynamicFileByteProvider != null)
                {
                    dynamicFileByteProvider.ApplyChanges();
                }
                else if (dynamicByteProvider != null)
                {
                    byte[] data = dynamicByteProvider.Bytes.ToArray();
                    using (FileStream stream = File.Open(_fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        stream.Write(data, 0, data.Length);
                    }
                    dynamicByteProvider.ApplyChanges();
                }
            }
            catch (Exception ex1)
            {
                if (OnExceptionOccured != null)
                {
                    OnExceptionOccured(this, ex1);
                }
            }
            finally
            {
                if (_fileName == newFileName)
                {
                    string target = FileHelper.SaveFile();
                    if (target != null && target != string.Empty)
                    {
                        CleanUp();
                        try
                        {
                            if (File.Exists(target)) File.Delete(target);
                            File.Move(newFileName, target);
                            OpenFile(target, false);
                        }
                        catch (Exception exception)
                        {
                            if (OnExceptionOccured != null)
                            {
                                OnExceptionOccured(this, exception);
                            }
                        }
                        finally
                        {
                            if (File.Exists(newFileName)) File.Delete(newFileName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Closes the current file
        /// </summary>
        /// <returns>OK, if the current file was closed.</returns>
        public DialogResult CloseFile()
        {
            if (hexBox.ByteProvider == null)
                return DialogResult.OK;

            try
            {
                if (CanCancel) hexBox.AbortFind();
                if (hexBox.ByteProvider != null && hexBox.ByteProvider.HasChanges() && _fileName != newFileName)
                {
                    DialogResult res = MessageBox.Show("Do you want to save changes?",
                      "FileInput",
                      MessageBoxButtons.YesNoCancel,
                      MessageBoxIcon.Warning);

                    if (res == DialogResult.Yes)
                    {
                        SaveFile();
                        CleanUp();
                    }
                    else if (res == DialogResult.No)
                    {
                        CleanUp();
                    }
                    else if (res == DialogResult.Cancel)
                    {
                        return res;
                    }
                    UpdateFileSizeStatus();
                    if (res != DialogResult.Cancel && OnFileClosed != null) OnFileClosed(this);

                    return res;
                }
                else
                {
                    CleanUp();
                    return DialogResult.OK;
                }
            }
            catch (Exception)
            {
                return DialogResult.Cancel;
            }
        }

        void CleanUp()
        {
            if (hexBox.ByteProvider != null)
            {
                IDisposable byteProvider = hexBox.ByteProvider as IDisposable;
                if (byteProvider != null)
                    byteProvider.Dispose();
                hexBox.ByteProvider = null;
            }
            _fileName = null;
            DisplayText(null);
        }

        public void AbortFind()
        {
            // sould result in quit search => bgworker returns
            hexBox.AbortFind();

            // wait for abort      
            while (CanCancel)
            {
                // Console.WriteLine("Sleeping 10");        
                Thread.Sleep(10);
            }
        }

        public void Find(string value)
        {
            if (value != null)
            {
                if (OnInformationOccured != null) OnInformationOccured(this, new Exception("Searching for: " + value));
                _findBuffer = System.Text.ASCIIEncoding.ASCII.GetBytes(value);
                if (CanCancel) AbortFind();
                FindNext();
            }
        }

        private BackgroundWorker bgWorker = null;

        /// <summary>
        /// Find next match
        /// </summary>
        void FindNext()
        {
            if (_findBuffer.Length == 0)
            {
                // Find();
                return;
            }

            canCancel = true;
            findResult = long.MinValue;
            FindDelegate fd = new FindDelegate(FindInvoke);
            AsyncCallback ac = new AsyncCallback(FindCallbackMethod);
            IAsyncResult ar = fd.BeginInvoke(_findBuffer, hexBox.SelectionStart + hexBox.SelectionLength, ac, null);
        }


        private delegate void HexBoxFocusDelegate();
        private delegate object FindDelegate(byte[] findBuffer, long range);
        private long findResult;

        private object FindInvoke(byte[] findBuffer, long range)
        {
            // return hexBox.Find(findBuffer, range);
            findResult = hexBox.Find(findBuffer, range);
            return findResult;
        }

        private void FindCallbackMethod(IAsyncResult result)
        {
            canCancel = false;
            try
            {
                long res = findResult;
                // res = (long)e.Result;

                if (res == -1) // -1 = no match
                {
                    if (OnInformationOccured != null) OnInformationOccured(this, new Exception("Search reached end of file."));
                }
                else if (res == -2) // -2 = find was aborted
                {
                    if (OnInformationOccured != null) OnInformationOccured(this, new Exception("Search aborted."));
                    return;
                }
                else // something was found
                {
                    // if (!hexBox.Focused)
                    // hexBox.Focus();
                    HexBoxFocusDelegate hexBoxFocusDelegate = delegate()
                    {
                        if (!hexBox.Focused) hexBox.Focus();
                    };
                    hexBox.Invoke(hexBoxFocusDelegate);
                    if (OnInformationOccured != null) OnInformationOccured(this, new Exception("Found on position: " + res.ToString()));
                }
            }
            catch (Exception exception)
            {
                if (OnExceptionOccured != null) OnExceptionOccured(this, exception);
            }
        }



        /// <summary>
        /// Aborts the current find process
        /// </summary>
        void FormFindCancel_Closed(object sender, System.EventArgs e)
        {
            hexBox.AbortFind();
        }

        /// <summary>
        /// Put focus back to the cancel form.
        /// </summary>
        void FocusToFormFindCancel(object sender, System.EventArgs e)
        {
            // _formFindCancel.Focus();
        }

        ///<summary>
        ///Displays the goto byte dialog.
        ///</summary>
        void Goto()
        {
            //_formGoto.SetMaxByteIndex(hexBox.ByteProvider.Length);
            //_formGoto.SetDefaultValue(hexBox.SelectionStart);
            //if (_formGoto.ShowDialog() == DialogResult.OK)
            //{
            //  hexBox.SelectionStart = _formGoto.GetByteIndex();
            //  hexBox.SelectionLength = 1;
            //  hexBox.Focus();
            //}
        }

        /// <summary>
        /// Enables drag&drop
        /// </summary>
        void hexBox_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        /// <summary>
        /// Processes a file drop
        /// </summary>
        void hexBox_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            string[] formats = e.Data.GetFormats();
            object oFileNames = e.Data.GetData(DataFormats.FileDrop);
            string[] fileNames = (string[])oFileNames;
            if (fileNames != null && fileNames.Length == 1)
            {
                OpenFile(fileNames[0], false);
            }
        }

        void Position_Changed(object sender, System.EventArgs e)
        {
            // this.sbCharacterPosition.Text = 
            toolStripLblCharPosition.Text = string.Format("Ln {0}    Col {1}",
              hexBox.CurrentLine, hexBox.CurrentPositionInLine);
        }

        void FormHexEditor_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CloseFile() == DialogResult.Cancel)
                e.Cancel = true;
        }


        public void Cut()
        {
            if (hexBox.CanCut()) hexBox.Cut();
        }

        public void Copy()
        {
            hexBox.Copy();
        }

        public void Paste()
        {
            hexBox.Paste();
        }

        private void ExceptionOccured(Exception exception)
        {
            if (OnExceptionOccured != null)
                OnExceptionOccured(this, exception);
        }

    }
}
