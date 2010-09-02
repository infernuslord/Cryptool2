using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Documents;
using System.Windows.Controls;

namespace WorkspaceManager.Model
{
    /// <summary>
    /// This class wraps a text which can be put to the workspace
    /// </summary>
    [Serializable]
    public class TextModel : VisualElementModel
    {
        private byte[] data = null;

        /// <summary>
        /// Instantiate a new TextModel
        /// </summary>  
        public TextModel()
        {
            
        }

        /// <summary>
        /// Instantiate a new TextModel
        /// </summary>
        /// <param name="text"></param>
        public TextModel(byte[] data)
        {
            this.data = data;
        }

        /// <summary>
        /// The WorkspaceModel of this TextModel
        /// </summary>
        public WorkspaceModel WorkspaceModel { get; set; }

        /// <summary>
        /// Loads the Content of this TextModel to the given RichtTextBox
        /// </summary>
        /// <param name="rtb"></param>
        public void loadRTB(RichTextBox rtb){
            
            if (data == null)
            {
                return;
            }

            MemoryStream memoryStream = new MemoryStream(data);
            FlowDocument flowDocument = new FlowDocument();
            TextRange textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
            textRange.Load(memoryStream, System.Windows.DataFormats.XamlPackage);
            rtb.Document = flowDocument;            
        }

        /// <summary>
        /// Save the Content of the given RichTextBox to this TextModel
        /// </summary>
        /// <param name="rtb"></param>
        public void saveRTB(RichTextBox rtb){

            if (rtb.Document.Blocks.Count == 0)
            {
                return;
            }

            MemoryStream memoryStream = new MemoryStream();
            TextRange textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            textRange.Save(memoryStream, System.Windows.DataFormats.XamlPackage);

            data = new byte[memoryStream.Length];
            data = memoryStream.ToArray();
            memoryStream.Close();
            
        }

        /// <summary>
        /// is the image enabled ?
        /// </summary>
        private bool isEnabled = true;
        public bool IsEnabled { get { return isEnabled; } set { isEnabled = value; } }
    }
}
