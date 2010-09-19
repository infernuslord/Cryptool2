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

        private static int uniqueCounter;
        protected readonly int uniqueIndex;

        public State(T node, State parent, double dist) : base(node, parent) {
            this.Dist = dist;
            uniqueIndex = ++uniqueCounter;
        }
        
        public int CompareTo(State other) {
            int res =  Dist.CompareTo(other.Dist);
            //if res and other is equal then apply different CompareTo() value (OrderedSet deletes any State if 

            if (res == 0)
                return uniqueIndex.CompareTo(other.uniqueIndex);
             return res;
        }


    }
    
    public LinkedList<T> findPath(IEnumerable<T> graph, T start, T goal) {
    
        Dictionary<T, State> states = new Dictionary<T, State>();
        OrderedSet<State> unvisitedNodes = new OrderedSet<State>();

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
                State vState = states[v];
                double altPathCost = visitingNode.Dist + visitingNode.Node.traverseCost(v);
                
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
