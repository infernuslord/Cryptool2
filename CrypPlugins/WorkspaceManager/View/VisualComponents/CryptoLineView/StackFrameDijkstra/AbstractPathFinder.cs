using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkspaceManager.View.VisualComponents.StackFrameDijkstra
{
public abstract class AbstractPathFinder<T>  where T :Node<T> {
    protected volatile bool canceled;

    public void cancel() {
        canceled = true;
    }

}

}
