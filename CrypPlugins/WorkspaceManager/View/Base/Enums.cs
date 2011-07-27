using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkspaceManager.View.Base
{

    public enum BinComponentAction
    {
        LastState
    };

    public enum PanelOrientation
    {
        North,
        West,
        East,
        South,
    };

    public enum ConversionLevel
    {
        Red,
        Yellow,
        Green
    };

    public enum BinEditorState
    {
        READY,
        BUSY
    };
}
