using Prefab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrefabIdentificationLayers.Features
{
    [Serializable]
    public class Cleanup : Layer
    {
        public string Name
        {
            get { return "feature_cleanup"; }
        }

        public void Init(Dictionary<string, object> parameters)
        {
            
        }

        public void Close()
        {
            
        }

        public void Interpret(InterpretArgs args)
        {
            List<Tree> features = FeatureDetectionLayer.GetFeaturesFromTree(args.Tree);
            foreach (Tree feature in features)
                args.Remove(feature);
        }

        public void AfterInterpret(Tree tree)
        {
            
        }

        public void ProcessAnnotations(AnnotationArgs args)
        {
            
        }

        public IEnumerable<string> AnnotationLibraries()
        {
            return null;
        }
    }
}
