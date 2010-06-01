using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace WorkspaceManager.View.Interface
{
    public interface IDraggable
    {
        bool CanDrag { get; }
        void SetPosition(Point value);
        Point GetPosition();
    }
}
