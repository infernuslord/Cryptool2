using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.CrypWin.Helper
{
    [Serializable()]
    public abstract class StoredTab
    {
        public string Title { get; private set; }

        protected StoredTab(string title)
        {
            Title = title;
        }
    }

    [Serializable()]
    class EditorTypeStoredTab : StoredTab
    {
        public Type EditorType { get; private set; }

        public EditorTypeStoredTab(string title, Type editorType) : base(title)
        {
            EditorType = editorType;
        }
    }

    [Serializable()]
    class CommonTypeStoredTab : StoredTab
    {
        public Type Type { get; private set; }

        public CommonTypeStoredTab(string title, Type type) : base(title)
        {
            Type = type;
        }
    }

    [Serializable()]
    class EditorFileStoredTab : StoredTab
    {
        public string Filename { get; private set; }

        public EditorFileStoredTab(string title, string filename) : base(title)
        {
            Filename = filename;
        }
    }
}
