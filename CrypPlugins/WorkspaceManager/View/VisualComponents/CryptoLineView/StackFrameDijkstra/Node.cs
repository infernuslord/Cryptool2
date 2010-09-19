using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkspaceManager.View.VisualComponents.StackFrameDijkstra
{
    public interface Node<T> where T : Node<T>
    {
        /**
         * Returns the cost to get from this node to the dest node.
         *
         * @return the cost
         */
        double traverseCost(T dest);

        /**
         * Returns the neighbors of this node.
         *
         * @return the neighbors
         */
        IEnumerable<T> neighbors();

    }

}
