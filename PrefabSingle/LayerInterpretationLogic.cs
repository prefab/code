using Prefab;
using PrefabSingle;
using PrefabIdentificationLayers.Features;
using PrefabIdentificationLayers.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PythonHost;
namespace PrefabSingle
{
    public class LayerInterpretationLogic : PrefabInterpretationLogic
    {
        public string LayerDirectory
        {
            get;
            private set;

        }

        private List<LayerWrapper> _layers;

        public LayerInterpretationLogic(string layerRootDirectory)
        {
            LayerDirectory = layerRootDirectory;
        }
        public IEnumerable<LayerWrapper> Layers
        {
            get { return _layers; }
        }
        public void Load()
        {
            
            _layers = ChainLoader.LoadChain("prefab_layers", LayerDirectory);

        }

        public Tree Interpret(Tree frame)
        {
            return LayerChain.InterpretChain(_layers, frame);
        }


        public string GetPtypeDatabase()
        {
            var featureLayer = _layers.Find(l => l.Layer.Name.Equals("feature_detection")).Layer as FeatureDetectionLayer;
            return featureLayer.PtypeLibrary;
        }


        public void UpdateLogic()
        {
            AnnotationUpdateUtility.UpdateInvalidatedLayers(_layers);
        }


        public IEnumerable<PrefabIdentificationLayers.Prototypes.Ptype> GetPtypes()
        {
            var featureLayer = _layers.Find(l => l.Layer.Name.Equals("feature_detection"));

            var ptypes = ((Dictionary<string, object>)featureLayer.Parameters["shared"])[FeatureDetectionLayer.SHARED_PTYPES_KEY] as IEnumerable<Ptype>;

            return ptypes;
        }

        public Dictionary<string, List<IAnnotation>> GetAnnotationsMatchingNode(Tree node, Tree root, string bitmapid)
        {
           var anns =  AnnotationLibrary.GetAllAnnotationsForImageUsingAllLayers(_layers, bitmapid, node);

           var annotationsByLib = new Dictionary<string, List<IAnnotation>>();

           foreach (var pair in anns)
           {
               List<IAnnotation> wrappers = new List<IAnnotation>();
               foreach (var annotation in pair.Value)
               {
                       wrappers.Add(new ImageAnnotationWrapper(annotation));
               }

               annotationsByLib[pair.Key] = wrappers;

           }

           return annotationsByLib;
        }

        public IEnumerable<IRuntimeStorage> GetRuntimeStorages()
        {
            var intents = new List<IRuntimeStorage>();
            foreach(LayerWrapper lw in _layers)
            {
                intents.Add(lw.Intent);
            }

            return intents;
        }


        //public Dictionary<string, List<IAnnotation>> GetAllAnnotations()
        //{
            
        //}


        public IEnumerable<string> GetAnnotationLibraries()
        {
            return AnnotationLibrary.GetAnnotationLibraries(_layers);
        }
    }
}
