using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkspaceManager.Model
{
    /// <summary>
    /// This class wraps a text which can be put to the workspace
    /// </summary>
    [Serializable]
    public class TextModel : VisualElementModel
    {
        private string text = null;

        /// <summary>
        /// Instantiate a new TextModel
        /// </summary>  
        public TextModel()
        {
            this.text = "";
        }

        /// <summary>
        /// Instantiate a new TextModel
        /// </summary>
        /// <param name="text"></param>
        public TextModel(string text)
        {
            this.text = text;
        }

        /// <summary>
        /// Get/Set the text
        /// </summary>
        public string Text
        {
            get { return this.text; }
            set { this.text = value; }
        }
    }
}
