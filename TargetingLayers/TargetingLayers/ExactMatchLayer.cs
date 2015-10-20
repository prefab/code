using Newtonsoft.Json.Linq;
using Prefab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrefabUtils;
namespace TargetingLayers
{
    [Serializable]
    public class ExactMatchLayer : MarshalByRefObject, Layer
    {
        private string _annotationLibrary;
        private IRuntimeStorage _runtimeIntent;

        private enum DescritptorType
        {
            PtypeId,
            Default
        }

        public void AfterInterpret(Tree tree)
        {
            
        }

        public IEnumerable<string> AnnotationLibraries()
        {
            return new List<string>() { _annotationLibrary };
        }

        public void Close(){ }

        public void Init(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("library"))
            {
                _annotationLibrary = parameters["library"].ToString();
                _runtimeIntent = parameters["intent"] as IRuntimeStorage;
            }
        }

        public void Interpret(InterpretArgs args)
        {
            InterpretHelper(args, args.Tree);
        }

        private void InterpretHelper(InterpretArgs args, Tree node)
        {
            string path = PathDescriptor.GetPath(node, args.Tree);

            JObject data = (JObject)_runtimeIntent.GetData(path);

            if(data != null)
            {
                foreach (var key in data.Properties())
                {
                    bool parsedbool = false;

                    if (bool.TryParse(data[key.Name].Value<string>(), out parsedbool))
                    {
                        args.Tag(node, key.Name, parsedbool);
                    }else{
                        args.Tag(node, key.Name, data[key.Name].Value<string>());
                    }
                    
                }
            }

            foreach (Tree child in node.GetChildren())
            {
                InterpretHelper(args, child);
            }
        }

        public string Name
        {
            get { return "exact_match"; }
        }

        public void ProcessAnnotations(AnnotationArgs args)
        {
            _runtimeIntent.Clear();
            foreach (AnnotatedNode anode in args.AnnotatedNodes)
            {
                if (anode.MatchingNode != null)
                {
                    string path = PathDescriptor.GetPath(anode.MatchingNode, anode.Root);

                    _runtimeIntent.PutData(path, anode.Data);
                }
            }   
        }
    }
}
