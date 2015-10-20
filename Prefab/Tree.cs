using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace Prefab
{
	public sealed class Tree : IBoundingBox
	{


		private List<Tree> _children;
		private Dictionary<string, object> _tags;
        

		public static Tree DeepCopy(Tree toCopy)
		{
			Tree copy = new Tree(toCopy);
			return copy;
		}

		private Tree(Tree toCopy)
		{
			_tags = new Dictionary<string, object>();
			foreach (var item in toCopy.GetTags())
				_tags.Add (item.Key, item.Value);

			Occurrence = toCopy.Occurrence;

			List<Tree> children = new List<Tree>();
			foreach (Tree child in toCopy.GetChildren())
			{
				children.Add(new Tree(child));
			}

			_children = children;

		}

		private Tree()
		{

		}

		public static Tree FromBoundingBox(IBoundingBox bb, Dictionary<string,object> tags){
			Tree tree = new Tree();
			tree.Occurrence = bb;

			if(tags != null)
				tree._tags = new Dictionary<string, object>(tags);
			else
				tree._tags = new Dictionary<string, object>();

			tree._children = new List<Tree>();

			return tree;
		}

		public static Tree FromPixels(Bitmap image, Dictionary<string, object> tags){
			BoundingBox frame = new BoundingBox(0,0, image.Width, image.Height);
			Tree tree = new Tree();
			tree.Occurrence = frame;
			tree._tags = new Dictionary<string, object>(tags);
			tree._children = new List<Tree>();
			tree._tags.Add("capturedpixels", image);
			tree._tags.Add("type", "frame");

			return tree;
		}



		public IEnumerable<Tree> GetChildren()
		{
			return _children;
		}

        public IEnumerable<Tree> get_children()
        {
            return _children;
        }

		public bool HasTag(string key)
		{
			return _tags.ContainsKey(key);
		}

        public bool has_tag(string key)
        {
            return HasTag(key);
        }

		public IEnumerable<KeyValuePair<string, object>> GetTags()
		{
			return _tags;
		}

		public object this[string attributeName]
		{
			get
			{
				object value = null;
				_tags.TryGetValue(attributeName, out value);
				return value;
			}
		}


		public int Top
		{
			get { return Occurrence.Top; }
		}

        public int top
        {
            get { return Occurrence.Top; }
        }


		public int Left
		{
			get { return Occurrence.Left; }
		}

        public int left
        {
            get { return Occurrence.Left; }
        }

		public int Height
		{
			get { return Occurrence.Height; }
		}

        public int height
        {
            get { return Occurrence.Height; }
        }

		public int Width
		{
			get { return Occurrence.Width; }
		}

        public int width
        {
            get { return Occurrence.Width; }
        }

		public IBoundingBox Occurrence
		{
			get;
			private set;
		}

        public static bool Contains(Tree root, Tree node)
        {
            if(node == root)
                return true;

            Tree parent = GetParent(node, root);
            return parent != null;
        }

		public static void AddNodesToCollection(Tree tree, ICollection<Tree> toadd)
		{

			toadd.Add(tree);
			foreach (Tree child in tree.GetChildren())
				AddNodesToCollection(child, toadd);
		}


        public override string ToString()
        {
            return ToJson(this);
        }

		public static string ToJson(Tree tree){
			JObject jobject = new JObject ();
			ToJsonHelper (tree, jobject);

			return jobject.ToString ();
		}

		private static void ToJsonHelper(Tree currnode, JObject jo){
			foreach(string tagname in currnode._tags.Keys ){
                if (currnode._tags[tagname] != null)
                    jo.Add(tagname, currnode._tags[tagname].ToString());
                else
                    jo.Add(tagname, null);
			}

			jo.Add("top", currnode.Top);
			jo.Add("left",currnode.Left);
			jo.Add("width", currnode.Width);
			jo.Add("height", currnode.Height);

			JArray array = new JArray();

			foreach(Tree child in currnode._children){
				JObject childjo = new JObject();
				ToJsonHelper(child, childjo);
				array.Add(childjo);
			}

			jo.Add("children", array);
		}

		public static Tree GetParent(Tree node, Tree root)
		{
			if (root == null || node == null)
				return null;

			if (root == node)
				return null;

			if (root.GetChildren().Contains(node))
				return root;


			foreach (Tree child in root.GetChildren())
			{
				Tree found = GetParent(node, child);
				if (found != null)
					return found;
			}

			return null;
		}

		public static IEnumerable<Tree> GetSiblings(Tree node, Tree root)
		{
			Tree parent = GetParent(node, root);
			List<Tree> siblings = new List<Tree>();
			if (parent != null)
			{
				foreach (Tree child in parent.GetChildren())
				{
					if (child != node)
						siblings.Add(child);
				}
			}

			return siblings;
		}

		public sealed class BatchTransform
		{
			private Tree _treeToUpdate;
			private Dictionary<Tree, UpdateRequest> _updates;

			private class UpdateRequest
			{
				public Dictionary<string, object> Annotations
				{
					get;
					set;
				}


				public List<Tree> Descendents
				{
					get;
					set;
				}

				public UpdateRequest()
				{
					Descendents = new List<Tree>();
				}

                public bool Delete;

			}



			public BatchTransform(Tree currentTree)
			{
				_treeToUpdate = currentTree;
				_updates = new Dictionary<Tree, UpdateRequest>();


			}

			public void Tag(Tree node, string key, object value)
			{
				UpdateRequest ur = null;
				if (!_updates.TryGetValue(node, out ur))
				{
					ur = new UpdateRequest();
					_updates[node] = ur;
				}

				if (ur.Annotations == null)
					ur.Annotations = new Dictionary<string, object>();

				ur.Annotations[key] = value;
			}

            public void Remove(Tree node)
            {
                UpdateRequest ur = null;
                if (!_updates.TryGetValue(node, out ur))
                {
                    ur = new UpdateRequest();
                    _updates[node] = ur;
                }

                ur.Delete = true;
            }


			public void SetAncestor(Tree node, Tree ancestor)
			{

				UpdateRequest ur = null;
				if (!_updates.TryGetValue(ancestor, out ur))
				{
					ur = new UpdateRequest();
					_updates[ancestor] = ur;
				}

                if(!ur.Descendents.Contains(node))
				    ur.Descendents.Add(node);
			}




			private GraphNode GetAncestryGraph()
			{

				//Construct the ancestry graph

				Dictionary<Tree, GraphNode> graphNodesByCorrespondingTree = new Dictionary<Tree, GraphNode>();
				GraphNode nodeForRoot = GetGraphCorrespondingToTree(_treeToUpdate, graphNodesByCorrespondingTree);
                
				AddDescendentsFromRequests(nodeForRoot, graphNodesByCorrespondingTree);




				return nodeForRoot;


			}



			private Tree CreateTreeFromProcessedAncestryGraph(GraphNode node)
			{
				Tree copy = new Tree();
				copy._tags = new Dictionary<string,object>(node.CorrespondingTreeNode._tags);
				copy.Occurrence = node.CorrespondingTreeNode.Occurrence;
				copy._children = new List<Tree>();

				UpdateRequest ur = null;
				if (_updates.TryGetValue(node.CorrespondingTreeNode, out ur))
				{
					if (ur.Annotations != null) 
                    {
						foreach (var tag in ur.Annotations)
							copy._tags [tag.Key] = tag.Value;
					}
				}

				foreach (GraphNode child in node.Neighbors)
				{
					Tree childTreeNode = CreateTreeFromProcessedAncestryGraph(child);
					copy._children.Add(childTreeNode);
				}

				return copy;
			}


			private GraphNode ReverseAncestryGraph(GraphNode rootToflip)
			{
				Dictionary<Tree, GraphNode> reverseNodesByTreeNode = new Dictionary<Tree, GraphNode>();
				GraphNode sourceNode = new GraphNode();

				ReverseAncestryGraphHelper(rootToflip, sourceNode, reverseNodesByTreeNode);
				return sourceNode;
			}

			private GraphNode ReverseAncestryGraphHelper(GraphNode toFlip, GraphNode sourceNode, Dictionary<Tree, GraphNode> reverseNodesByTree)
			{
				GraphNode flippedNode = null;
				if (!reverseNodesByTree.TryGetValue(toFlip.CorrespondingTreeNode, out flippedNode))
				{
					flippedNode = new GraphNode();
					flippedNode.CorrespondingTreeNode = toFlip.CorrespondingTreeNode;
					reverseNodesByTree[toFlip.CorrespondingTreeNode] = flippedNode;
				}


				foreach (GraphNode neighbor in toFlip.Neighbors)
				{
					GraphNode flippedNeighbor = ReverseAncestryGraphHelper(neighbor, sourceNode, reverseNodesByTree);
					flippedNeighbor.Neighbors.Add(flippedNode);
				}

				//This node doesn't have any neighbors, so we'll reach it with the source node
				if (toFlip.Neighbors.Count == 0)
					sourceNode.Neighbors.Add(flippedNode);

				return flippedNode;
			}

			private void AddDescendentsFromRequests(GraphNode nodeForRoot, Dictionary<Tree, GraphNode> graphNodesByCorrespondingTree)
			{
				foreach (Tree toUpdate in _updates.Keys)
				{

					GraphNode correspondingNode = GetGraphCorrespondingToTree(toUpdate, graphNodesByCorrespondingTree);

					foreach (Tree descendent in _updates[toUpdate].Descendents)
					{
                        
						GraphNode correspondingDescendent = GetGraphCorrespondingToTree(descendent, graphNodesByCorrespondingTree);
						if (!correspondingNode.Neighbors.Contains(correspondingDescendent))
						{
							correspondingNode.Neighbors.Add(correspondingDescendent);
						}
					}
				}
			}

			private GraphNode GetGraphCorrespondingToTree(Tree treeNode, Dictionary<Tree, GraphNode> graphNodesByCorrespondingTree)
			{
				GraphNode correspondingNode = null;
				if (!graphNodesByCorrespondingTree.TryGetValue(treeNode, out correspondingNode))
				{
					correspondingNode = new GraphNode();
					correspondingNode.CorrespondingTreeNode = treeNode;
					graphNodesByCorrespondingTree[treeNode] = correspondingNode;
					foreach (Tree child in treeNode.GetChildren())
					{
						GraphNode childGraphNode = GetGraphCorrespondingToTree(child, graphNodesByCorrespondingTree);
                        if (IsToDelete(child))
                        {
                            correspondingNode.Neighbors.AddRange(childGraphNode.Neighbors);
                        }else
						    correspondingNode.Neighbors.Add(childGraphNode);
					}
				}

				return correspondingNode;
			}

            private bool IsToDelete(Tree node)
            {
                UpdateRequest ur = null;
                if (_updates.TryGetValue(node, out ur))
                {
                    return ur.Delete;
                }

                return false;
            }

			private void SetDescendantsUsingTree(GraphNode node, Dictionary<Tree, GraphNode> graphNodesByCorrespondingTree)
			{

				foreach (Tree treenode in node.CorrespondingTreeNode.GetChildren())
				{
					GraphNode descendant = GetGraphCorrespondingToTree(treenode, graphNodesByCorrespondingTree);
					SetDescendantsUsingTree(descendant, graphNodesByCorrespondingTree);

					node.Neighbors.Add(descendant);
				}
			}

			private class GraphNode
			{
				public Tree CorrespondingTreeNode;

				public List<GraphNode> Neighbors;

				public GraphNode()
				{
					Neighbors = new List<GraphNode>();
				}

                public override string ToString()
                {
                    string str = "GraphNode: {" + CorrespondingTreeNode.ToString() + "}\n";
                    str += "Neighbors: [";
                    foreach (var nb in Neighbors)
                    {
                        str += nb.ToString();
                        str += ",\n";
                    }

                    str += "]\n";

                    return str;
                }
			}


			private void ConstructSubtreeUsingBacklinks(Dictionary<GraphNode, GraphNode> backlinks)
			{
				foreach (GraphNode child in backlinks.Keys)
				{
					foreach (GraphNode parent in backlinks.Values)
					{
						if (backlinks[child] != parent)
							parent.Neighbors.Remove(child);
					}
				}
			}

			public Tree GetUpdatedTree()
			{
				if (_updates.Count == 0)
					return _treeToUpdate;


				//Get the ancestry graph
				GraphNode ancestryGraph = GetAncestryGraph();


				//Check if the graph is over-constrained by detecting cycles.
				if (OverConstrained(ancestryGraph))
				{
					throw new Exception("You've set ancestors in such a way that you've created a cycle.\nA node cannot be an ancestor and a descendant of another node.");
				}


				//Find the longest paths in the graph
				Dictionary<GraphNode, GraphNode> backlinks = Dijkstras(ancestryGraph);


				//Check if the graph is under-constrained by making sure longest paths include all ancestors.
				if (UnderConstrained(ancestryGraph, backlinks))
				{
					throw new Exception("You've set ancestors in such a way that it's ambiguous how to construct a tree.\nIt is impossible to visit all of a node's ancestors in a single path to the root.");
				}

				//Create the tree by removing any nodes that aren't in the longest paths.
				ConstructSubtreeUsingBacklinks(backlinks);
				Tree updated = CreateTreeFromProcessedAncestryGraph(ancestryGraph);


				_treeToUpdate = updated;
				_updates.Clear();
				return _treeToUpdate;
			}

			private bool UnderConstrained(GraphNode ancestryGraph, Dictionary<GraphNode, GraphNode> backlinks)
			{
				GraphNode reverseGraph = ReverseAncestryGraph(ancestryGraph);

				List<GraphNode> longestPathNodes = new List<GraphNode>();
				List<GraphNode> allAncestors = new List<GraphNode>();

				return UnderConstrainedHelper(ancestryGraph, reverseGraph, backlinks, longestPathNodes, allAncestors);
			}

			private bool UnderConstrainedHelper(GraphNode currNode, GraphNode reverseAncestry, Dictionary<GraphNode, GraphNode> backlinks, List<GraphNode> longestPathNodes, List<GraphNode> allAncestors)
			{
				allAncestors.Clear();
				longestPathNodes.Clear();
				GetNodesInLongestPath(currNode, backlinks, longestPathNodes);
				GraphNode currNodeInReverseAncestry = GetCorrespondingNodeInReverseGraph( currNode, reverseAncestry);

				foreach(GraphNode neighbor in currNodeInReverseAncestry.Neighbors)
					GetAncestors(neighbor, allAncestors);

				if (!ListsEqual(longestPathNodes, allAncestors))
				{
					return true;
				}

				foreach (GraphNode neighbor in currNode.Neighbors)
				{
					if (UnderConstrainedHelper(neighbor, reverseAncestry, backlinks, longestPathNodes, allAncestors))
						return true;
				}

				return false;
			}

			private bool ListsEqual(List<GraphNode> list1, List<GraphNode> list2)
			{
				if (list1.Count != list2.Count)
					return false;

				foreach (GraphNode elmnt in list1)
				{

					GraphNode match = list2.FirstOrDefault(n => n.CorrespondingTreeNode == elmnt.CorrespondingTreeNode);
					if (match == null)
						return false;
				}

				return true;
			}

			private GraphNode GetCorrespondingNodeInReverseGraph(GraphNode currNode, GraphNode reverseAncestry)
			{
				if (reverseAncestry.CorrespondingTreeNode == currNode.CorrespondingTreeNode)
					return reverseAncestry;

				foreach (GraphNode neighbor in reverseAncestry.Neighbors)
				{
					GraphNode corresponding = GetCorrespondingNodeInReverseGraph(currNode, neighbor);
					if (corresponding != null)
						return corresponding;
				}

				return null;
			}

			private void GetAncestors(GraphNode nodeInReverseAncestry, List<GraphNode> toAdd)
			{
				if(!toAdd.Contains(nodeInReverseAncestry))
					toAdd.Add(nodeInReverseAncestry);

				foreach (GraphNode ancestor in nodeInReverseAncestry.Neighbors)
					GetAncestors(ancestor, toAdd);
			}

			private bool OverConstrained(GraphNode ancestryGraph)
			{
				HashSet<GraphNode> visited = new HashSet<GraphNode>();

				return ContainsCycle(ancestryGraph, visited);
			}

			private bool ContainsCycle(GraphNode ancestryGraph, HashSet<GraphNode> visited)
			{
				if (visited.Contains(ancestryGraph))
					return true;

				visited.Add(ancestryGraph);

				foreach (GraphNode neighbor in ancestryGraph.Neighbors)
				{
					if (ContainsCycle(neighbor, visited))
						return true;
				}

				visited.Remove(ancestryGraph);

				return false;
			}

			private void AllNodesToCollection(GraphNode current, ICollection<GraphNode> toAdd)
			{
				toAdd.Add(current);
				foreach (GraphNode neighbor in current.Neighbors)
					AllNodesToCollection(neighbor, toAdd);
			}

			private void GetNodesInLongestPath(GraphNode node, Dictionary<GraphNode, GraphNode> backlinks, List<GraphNode> longestPathNodes)
			{
				if (backlinks.ContainsKey(node))
				{
					GraphNode parent = backlinks[node];
					longestPathNodes.Add(parent);
					GetNodesInLongestPath(parent, backlinks, longestPathNodes);
				}
			}

			private Dictionary<GraphNode,GraphNode> Dijkstras(GraphNode root)
			{

				Dictionary<GraphNode, int> dists = new Dictionary<GraphNode, int>();
				Dictionary<GraphNode, GraphNode> previous = new Dictionary<GraphNode, GraphNode>();

				//TODO: make this a priority queue if performance is slow
				List<GraphNode> queue = new List<GraphNode>();
				AllNodesToCollection(root, queue);

				foreach(GraphNode node in queue)
				{
					dists[node] = int.MaxValue;
				}

				dists[root] = 0;



				while (queue.Count > 0)
				{
					GraphNode u = GetNodeWithSmallestDist(queue, dists);
					queue.Remove(u);

					if (dists[u] == int.MaxValue)
						break;

					IEnumerable<GraphNode> neighbors = u.Neighbors;
					foreach (GraphNode neighbor in neighbors)
					{
						int alt = dists[u] - 1;
						if (alt < dists[neighbor])
						{
							dists[neighbor] = alt;
							previous[neighbor] = u;
						}
					}
				}

				return previous;
			}

			private GraphNode GetNodeWithSmallestDist(List<GraphNode> queue, Dictionary<GraphNode, int> dists)
			{
				int min = int.MaxValue;
				GraphNode best = null;
				foreach (GraphNode node in queue)
				{
					if (dists[node] <= min)
					{
						best = node;
						min = dists[node];
					}
				}

				return best;
			}


        }


        public static Tree FromMutable(MutableTree mutable)
        {
            Tree node = Tree.FromBoundingBox(mutable, mutable.Tags);

            foreach (MutableTree child in mutable.GetChildren())
            {
                node._children.Add(FromMutable(child));
            }

            return node;
        }

        
    }
}
