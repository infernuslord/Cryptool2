using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkspaceManager.View.VisualComponents.StackFrameDijkstra
{
    class NodeState<T> where T:Node<T> {

        public readonly T Node;
        public NodeState<T> previous{get;set;}

        public NodeState(T node, NodeState<T> previous) {
            this.Node = node;
            this.previous = previous;
        }

        public LinkedList<T> makePath()
        {
            LinkedList<T> result = new LinkedList<T>();
            NodeState<T> s = this;
            while (s != null) {
                result.AddFirst(s.Node);
                s = s.previous;
            }

            return result;
        }

    }

}
