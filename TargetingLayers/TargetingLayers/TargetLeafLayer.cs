using Prefab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TargetingLayers
{
    [Serializable]
    public class TargetLeafLayer : Layer
    {
        public void AfterInterpret(Tree tree)
        {
            
        }

        public IEnumerable<string> AnnotationLibraries()
        {
            return null;
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

        private void InterpretHelper(InterpretArgs args, Tree node)
        {
            if (node.ContainsTag("type") && node["type"].Equals("ptype"))
            {
                args.Tag(node, "is_target", "true");
            }
            else if (node.ContainsTag("type") && !node["type"].Equals("ptype"))
            {
                foreach (Tree child in node.GetChildren())
                {
                    if (child.ContainsTag("type") && !child["type"].Equals("feature") && child.GetChildren().Count() == 0)
                    {
                        args.Tag(child, "is_target", "true");
                    }
                }
            }

            foreach (Tree child in node.GetChildren())
                InterpretHelper(args, child);
        }

        public string Name
        {
            get { return "target_leaf"; }
        }

        public void ProcessAnnotations(AnnotationArgs args)
        {
            
        }
    }
}
