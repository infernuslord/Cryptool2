using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkspaceManager.View.VisualComponents.StackFrameDijkstra
{
public class Dijkstra<T > : AbstractPathFinder<T> where T : Node<T> {

    private class State : NodeState<T>, IComparable<State> {

        public double dist {get;set;}

        public State(T node, State parent, double dist) : base(node, parent) {
            this.dist = dist;
        }

        public int CompareTo(State other) {
            return (int)(dist - other.dist);
        }

    }

    public LinkedList<T> findPath(IEnumerable<T> graph, T start, T goal) {
        canceled = false;
        Dictionary<T, State> states = new Dictionary<T, State>();
        HashSet<T> Q = new HashSet<T>(graph);

        foreach(T n in graph) {
            states.Add(n, new State(n, null, Double.PositiveInfinity));
        }

        states[start].dist = 0;
        /*
        Predicate<Map.Entry<T, State>> notVisited = new Predicate<Map.Entry<T, State>>() {

            public boolean apply(Entry<T, State> t) {
                return Q.contains(t.getKey());
            }

        };
        Ordering<Map.Entry<T, State>> orderByEntryValue = Utilities.orderByEntryValue();
        */
        while (!(Q.Count==0 || canceled)) {
            //Collection<Map.Entry<T, State>> inQ = Collections2.filter(states.entrySet(), notVisited);
            var inQ = states.Where(m => Q.Contains(m.Key));
            //TODO prefer in-place sort?
            var uEntry = inQ.OrderBy( (a) => a.Value.dist).First();

            if (uEntry.Value.dist == Double.PositiveInfinity) {
                break;
            }

            T u = uEntry.Key;
            State state = uEntry.Value;
           
            Q.Remove(u);
            if (goal.Equals(u)) {
                return state.makePath();
            }

            foreach (T v in u.neighbors()) {
                double alt = state.dist + u.traverseCost(v);
                State vState = states[v];
                if (alt < vState.dist) {
                    vState.dist = alt;
                    vState.previous = state;
                }
            }
        }

        return null;
    }

}

}
