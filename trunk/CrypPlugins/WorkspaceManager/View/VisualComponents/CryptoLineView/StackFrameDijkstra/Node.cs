using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkspaceManager.View.VisualComponents.StackFrameDijkstra
{
public interface Node<T> where T:Node<T> {

    /**
     * Returns the estimate of the cost to get from this node to the goal node. If unable to estimate, it is safe to return 0 or
     * underestimate. Overestimates can result in failures to find a path.
     *
     * @return the estimate
     */
    double pathCostEstimate(T goal);

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
