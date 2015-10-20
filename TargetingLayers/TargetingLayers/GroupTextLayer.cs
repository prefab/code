using Prefab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TargetingLayers
{
    [Serializable]
    public class GroupTextLayer : Layer
    {

        public void AfterInterpret(Tree tree)
        {
            
        }

        public IEnumerable<string> AnnotationLibraries()
        {
            return new List<string>();
        }

        public void Close()
        {
            
        }

        public void Init(Dictionary<string, object> parameters)
        {
            
        }

        public void Interpret(InterpretArgs args)
        {
            InterpretHelper(args, args.Tree);
        }

        private void InterpretHelper(InterpretArgs args, Tree currNode)
        {
            var children = currNode.GetChildren().Where( n => n["is_text"] != null && (bool)n["is_text"]);
            var sorted = GroupNextLayer.SortNodesInReadingOrder(children);

            bool createNew = true;
            List<Tree> togroup = new List<Tree>();
            for (int i = 1; i < sorted.Count; i++ )
            {
                
                Tree currChild = sorted[i];
                Tree prevChild = sorted[i - 1];
                if (prevChild["group_next"] != null && (bool)prevChild["group_next"])
                {
                    if (createNew)
                    {
                        createNew = false;
                        togroup.Add(prevChild);
                    }
                    
                    togroup.Add(currChild);
                }
                else
                {
                    if (togroup.Count > 0)
                    {
                        IBoundingBox bounding = BoundingBox.Union(togroup);
                        var tags = new Dictionary<string, object>();
                        tags["type"] = "grouped_text";
                        tags["is_text"] = true;
                        Tree grouped = Tree.FromBoundingBox(bounding, tags);

                        foreach (Tree t in togroup)
                            args.SetAncestor(t, grouped);

                        args.SetAncestor(grouped, currNode);

                        togroup.Clear();
                    }

                    createNew = true;

                }
            }

            if (togroup.Count > 0)
            {
                IBoundingBox bounding = BoundingBox.Union(togroup);
                var tags = new Dictionary<string, object>();
                tags["type"] = "grouped_text";
                tags["is_text"] = true;
                Tree grouped = Tree.FromBoundingBox(bounding, tags);

                foreach (Tree t in togroup)
                    args.SetAncestor(t, grouped);

                args.SetAncestor(grouped, currNode);

                togroup.Clear();
            }

            foreach (Tree child in currNode.GetChildren())
                InterpretHelper(args, child);
        }

        public string Name
        {
            get { return "group_text"; }
        }

        public void ProcessAnnotations(AnnotationArgs args)
        {
            
        }
    }
}
