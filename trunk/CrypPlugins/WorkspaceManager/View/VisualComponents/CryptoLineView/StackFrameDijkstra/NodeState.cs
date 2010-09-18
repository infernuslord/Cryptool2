using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkspaceManager.View.VisualComponents.StackFrameDijkstra
{
    class NodeState<T> where T:Node<T> {

        readonly T node;
        public NodeState<T> previous{get;set;}

        public NodeState(T node, NodeState<T> previous) {
            this.node = node;
            this.previous = previous;
        }

        public LinkedList<T> makePath()
        {
            LinkedList<T> result = new LinkedList<T>();
            NodeState<T> s = this;
            while (s != null) {
                result.AddFirst(s.node);
                s = s.previous;
            }

            return result;
        }

    }

}
