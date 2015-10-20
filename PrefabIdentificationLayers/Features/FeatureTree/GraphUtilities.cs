using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace PrefabIdentificationLayers.Features.FeatureTree
{
	public static class GraphUtilities
	{
		public static BMZ GetBmz(ICollection<int> keys){
			return BMZ.Create(keys, 100, 1.5 * keys.Count);
		}


		public sealed class BMZ{

			private readonly int seed1, seed2;
			private readonly int[] g;

			BMZ(int seed1, int seed2, int[] g){
				this.seed1 = seed1;
				this.seed2 = seed2;
				this.g = g;
			}



			public int DoHash(int key){
				int n = g.Length;
				int h1 = (key ^ seed1) % n;
				int h2 = (key ^ seed2) % n;
				if(h1 < 0) {h1 += n;}
				if(h2 < 0) {h2 += n;}
				if(h1 == h2){ h2 = (h2 + 1) % n;}
				return g[h1] + g[h2];
			}

			public static BMZ Create(ICollection<int> keys, int maxTries, double c){
				Random r = new Random(17);
				for(int tries = 0; tries < maxTries; tries++){
					int seed1 = r.Next();
					int seed2 = r.Next();
					int[] g = new int[(int)Math.Ceiling(c * keys.Count)];
					Graph graph = Graph.Create(keys, seed1, seed2, g.Length);
					if(graph == null) { continue; } // some duplicates - try again with new seeds
					BitArray criticalNodes = FindCriticalNodes(graph, g.Length);
					BitArray ae = new BitArray(g.Length);
					if(!AssignIntsToCriticalVertices(graph, g, ae, criticalNodes)) continue; // try again from the start with different seeds
					AssignIntsToNonCriticalVertices(graph, g, ae, criticalNodes); // this can't fail
					return new BMZ(seed1, seed2, g);
				}
				return null;
			}
		}

		private static int[] GetTwoHashes(int key, int seed1, int seed2, int n){
			int h1 = (key ^ seed1) % n;
			int h2 = (key ^ seed2) % n;
			if(h1 < 0) {h1 += n;}
			if(h2 < 0) {h2 += n;}
			if(h1 == h2){ h2 = (h2 + 1) % n;}
			return new int[]{h1, h2};
		}


		private class Edge{
			public readonly int a, b;
			public Edge(int[] ab){
				this.a = ab[0];
				this.b = ab[1];
			}
		}

		private sealed class Graph{
			public readonly List<Edge> edges;
			public readonly LinkedList<int>[] adjacencyList;
			Graph(int n, int m){
				this.edges = new List<Edge>();
				this.adjacencyList = new LinkedList<int>[n];
			}

			bool AddEdge(Edge e) {
				edges.Add(e);
				if(GetAdjacencyList(e.a).Contains(e.b)) return true; // linear, but list should be v. small
				GetAdjacencyList(e.a).AddLast(e.b); 
				GetAdjacencyList(e.b).AddLast(e.a); 
				return false;
			}
			private LinkedList<int> GetAdjacencyList(int forVertex) {
				LinkedList<int> ret = adjacencyList[forVertex];
				return ret == null ? (adjacencyList[forVertex] = new LinkedList<int>()) : ret;
			}

			public static Graph Create(ICollection<int> keys, int seed1, int seed2, int n) {
				Graph ret = new Graph(n, keys.Count);
				foreach(int key in keys) { if(ret.AddEdge(new GraphUtilities.Edge(GetTwoHashes(key, seed1, seed2, n)))) return null; }
				return ret;
			}
		}


		private static BitArray FindCriticalNodes(Graph graph, int n) {
			// calculate node degrees...
			int[] degrees = new int[n];
			foreach(Edge edge in graph.edges){ ++degrees[edge.a]; ++degrees[edge.b]; };
			// ...and trim the chains...
			LinkedList<int> degree1 = new LinkedList<int>();
			for(int i=0; i<n; ++i) { if(degrees[i] == 1) degree1.AddLast(i); }
			while(degree1.Count > 0){
				int v = degree1.ElementAt (0);
				degree1.RemoveFirst(); --degrees[v];
				if(graph.adjacencyList[v] != null) 
					foreach(int adjacent in graph.adjacencyList[v] ) 
						if(--degrees[adjacent] == 1 ) degree1.AddLast(adjacent);
			}

			// ...and return a bitmap of critical vertices
			BitArray ret = new BitArray(n); // all non-critical by default - very useful!
			for(int i=0; i<n; ++i) { if(degrees[i] > 1) ret.Set(i, true); }
			return ret;
		}


		private static int NextSetBit(BitArray ba, int startInclusive){
			for (int i = startInclusive; i < ba.Length; i++) {
				if (ba.Get (i))
					return i;
			}

			return -1;
		}

		private static int NextClearBit(BitArray ba, int startInclusive){
			for (int i = startInclusive; i < ba.Length; i++) {
				if (!ba.Get (i))
					return i;
			}

			return -1;
		}

		private static void AndNot(BitArray a, BitArray b){
			for (int i = 0; i < a.Length; i++) {
				if (a [i] && b [i])
					a.Set (i, false);
			}
		}

		private static bool BitArrayEquals(BitArray b1, BitArray b2){
			if (b1.Length != b2.Length)
				return false;

			for (int i = 0; i < b1.Length; i++) {
				if (b1 [i] != b2 [i])
					return false;
			}

			return true;
		}

		/*		* @returns false if we couldn't assign the integers */
		private static bool AssignIntsToCriticalVertices(Graph graph, int[] g, BitArray ae, BitArray criticalNodes) {
			int x = 0;
			LinkedList<int> toProcess = new LinkedList<int>(); 
			BitArray assigned = new BitArray(g.Length);
			while(!BitArrayEquals(assigned, criticalNodes)) {

				BitArray unprocessed = (BitArray)criticalNodes.Clone(); 
				AndNot (unprocessed, assigned);

				toProcess.AddLast(NextSetBit(unprocessed, 0)); // start at the lowest unassigned critical vertex
				// assign another "tree" of vertices - not all critical ones are necessarily connected!
				x = ProcessCriticalNodes(toProcess, graph, ae, g, x, assigned, criticalNodes);
				if(x < 0) return false; // x is overloaded as a failure signal
			}
			return true;
		}
		/*		* process a single "tree" of connected critical nodes, rooted at the vertex in toProcess */
		private static int ProcessCriticalNodes(LinkedList<int> toProcess, Graph graph, BitArray ae, int[] g, int x, BitArray assigned, BitArray criticalNodes) {
			while(toProcess.Count != 0) {
				int v = toProcess.ElementAt (0);
				toProcess.RemoveFirst ();
				if(v < 0 || assigned.Get(v)) continue; // there are no critical nodes || already done this vertex
				if(graph.adjacencyList[v] != null) {
					x = GetXThatSatifies(graph.adjacencyList[v], x, ae, assigned, g);
					foreach(int adjacent in graph.adjacencyList[v]) {
						if(!assigned.Get(adjacent) && criticalNodes.Get(adjacent) && v!= adjacent) { 
							// give this one an integer, & note we shouldn't have loops - except if there is one key 
							toProcess.AddLast(adjacent); 
						} 
						if(assigned.Get(adjacent)) {  
							int edgeXtoAdjacent = x + g[adjacent]; // if x is ok, then this edge is now taken
							if(edgeXtoAdjacent >= graph.edges.Count) return -1; // this edge is too big! we're only AssignIng between 0 & m-1
							ae.Set(edgeXtoAdjacent, true); 
						} 
					}
				}
				g[v] = x; assigned.Set(v, true); // assign candidate x to g
				++x; // next v needs a new candidate x
			} 
			return x; // will use this as a candidate for other "trees" of critical vertices
		}
		private static int GetXThatSatifies(LinkedList<int> adjacencyList, int x, BitArray ae, BitArray assigned, int[] g) {
			foreach(int adjacent in adjacencyList) {
				if(assigned.Get(adjacent) 
					&& ae.Get(g[adjacent] + x)) { 
					// if we assign x to v, then the edge between v & and 'adjacent' will
					// be a duplicate - so our hash code won't be perfect! Try again with a new x:
					return GetXThatSatifies(adjacencyList, x + 1, ae, assigned, g);
				} 
			}
			return x; // this one satisfies all edges
		}

		private static void AssignIntsToNonCriticalVertices(Graph graph, int[] g, BitArray ae, BitArray criticalNodes) {
			LinkedList<int> toProcess = new LinkedList<int>();
			for(int v = NextSetBit(criticalNodes, 0); v != -1; v = NextSetBit(criticalNodes, v+1)) { toProcess.AddLast(v); } // load with the critical vertices
			BitArray visited = (BitArray) criticalNodes.Clone();
			ProcessNonCriticalNodes(toProcess, graph, ae, visited, g); // process the critical nodes
			// we've done everything reachable from the critical nodes - but
			// what about isolated chains?
			for(int v = NextClearBit(visited, 0); v != -1 && v < g.Length; v = NextClearBit(visited, v+1)) { 
				toProcess.AddLast(v);
				ProcessNonCriticalNodes(toProcess, graph, ae, visited, g);
			}    
		}
		/*		* process everything in the list and all vertices reachable from it */
		private static void ProcessNonCriticalNodes(LinkedList<int> toProcess, Graph graph, BitArray ae, BitArray visited, int[] g) {
			int nextEdge = NextClearBit(ae, 0);
			while(toProcess.Count != 0) {
				int v = toProcess.ElementAt (0);
				toProcess.RemoveFirst ();
				if(v < 0) continue; // there are no critical nodes
				if(graph.adjacencyList[v] != null) {
					foreach(int adjacent in graph.adjacencyList[v]) {
						if(!visited.Get(adjacent) && v != adjacent) { // shouldn't have loops - only if one key 
							// we must give it a value
							g[adjacent] = nextEdge - g[v]; // i.e. g[v] + g[a] = edge as needed
							toProcess.AddLast(adjacent);
							ae.Set(nextEdge, true);

							nextEdge = NextClearBit(ae, nextEdge + 1);                    
						}
					}
				}
				visited.Set(v, true);
			} 
		}
	}
}

