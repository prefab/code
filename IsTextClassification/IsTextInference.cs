using Prefab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsTextClassification
{
    [Serializable]
    public class IsTextInference : Layer
    {

        public string Name
        {
            get { return "is_text_inference"; }
        }

        public void Init(Dictionary<string, object> parameters)
        {
            
        }

        public void Close()
        {
            
        }

        public void Interpret(InterpretArgs args)
        {
            InterpretHelper(args, args.Tree);
        }

        private void InterpretHelper(InterpretArgs args, Tree currNode)
        {
            if(currNode.Height > 3 && currNode["type"] != null && currNode["type"].Equals("content"))
            {
                args.Tag(currNode, "is_text", true);
            }

            foreach (Tree child in currNode.GetChildren())
                InterpretHelper(args, child);
        }

        public void AfterInterpret(Tree tree)
        {
            
        }

        public void ProcessAnnotations(AnnotationArgs args)
        {
            
        }

        public IEnumerable<string> AnnotationLibraries()
        {
            return new List<string>();
        }
    }
}
