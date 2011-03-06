using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkspaceManager.View.Base
{
    public enum BinInternalState
    {
        Warning,
        Error,
        Normal,
    };

    public enum PanelOrientation
    {
        North,
        West,
        East,
        South,
    };

    public enum EditorState
    {
        READY,
        BUSY
    };
}
