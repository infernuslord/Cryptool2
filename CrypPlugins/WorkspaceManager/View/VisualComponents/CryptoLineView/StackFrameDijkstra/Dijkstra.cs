using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wintellect.PowerCollections;

namespace WorkspaceManager.View.VisualComponents.StackFrameDijkstra
{
public class Dijkstra<T>  where T : Node<T> {

    private class State : NodeState<T>, IComparable<State> {

        public double Dist {get;set;}

        public State(T node, State parent, double dist) : base(node, parent) {
            this.Dist = dist;
        }

        public int CompareTo(State other) {
            return Dist.CompareTo(other.Dist);
        }

    }
    
    public LinkedList<T> findPath(IEnumerable<T> graph, T start, T goal) {
    
        Dictionary<T, State> states = new Dictionary<T, State>();
        OrderedSet<State> unvisitedNodes = new OrderedSet<State>((a, b) => a.CompareTo(b));
        //BinaryQueue<State, double> unvisitedNodes = new BinaryQueue<State, double>(m => m.Dist, (a,b) => a.CompareTo(b));

        foreach(T n in graph) {
            var state = new State(n, null, Double.PositiveInfinity);
            if(n.Equals(start))
            {
                state.Dist = 0;
            }
            states.Add(n, state);
            unvisitedNodes.Add(state);
        }

        while (unvisitedNodes.Count!=0 ) {
            var visitingNode = unvisitedNodes.RemoveFirst();

            if (visitingNode.Dist == Double.PositiveInfinity) {
                break;
            }

           
            if (goal.Equals(visitingNode.Node)) {
                return visitingNode.makePath();
            }

            foreach (T v in visitingNode.Node.neighbors()) {
                double altPathCost = visitingNode.Dist + visitingNode.Node.traverseCost(v);
                State vState = states[v];
                if (altPathCost < vState.Dist) {
                    unvisitedNodes.Remove(vState);
                    vState.Dist = altPathCost;
                    vState.previous = visitingNode;
                    unvisitedNodes.Add(vState);
                }
            }
        }

        return null;
    }
}

}
