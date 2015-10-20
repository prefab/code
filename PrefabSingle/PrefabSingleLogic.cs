using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrefabIdentificationLayers;
using Prefab;
using PrefabIdentificationLayers.Features;
using PrefabIdentificationLayers.Prototypes;
using System.Reflection;
using System.IO;
using Newtonsoft.Json.Linq;
using PrefabUtils;
using PythonHost;
namespace PrefabSingle
{
    public class PrefabSingleLogic : PrefabInterpretationLogic
    {

        private List<LayerWrapper> _layers;
        private PythonSingleLayer _interpreter;
        private static readonly string _singleLayerName = @"..\..\..\single\interpret_tree.py";

        public string LayerDirectory
        {
            get;
            private set;

        }

        public IRuntimeStorage Storage
        {
            get;
            private set;
        }

        public PrefabSingleLogic(string scriptDirectory)
        {
            LayerDirectory = scriptDirectory;
        }

        public Tree Interpret(Tree frame)
        {
            if (_interpreter != null)
            {
                Tree tree = LayerChain.InterpretChain(_layers, frame);
                MutableTree mutable = MutableTree.FromTree(tree);
                PrefabSingleInterpretArgs args = new PrefabSingleInterpretArgs(mutable, Storage);

                try
                {
                    _interpreter.Interpret(args);
                }
                catch (Exception e)
                {

                    mutable["interpretation_exception"] = e;
                }

                return Tree.FromMutable(mutable);    
            }

            return frame;
        }

        private static PythonSingleLayer LoadSingleLayer(string pythonfile)
        {
            string name = System.IO.Path.GetFileName(pythonfile);
            string fullpath = System.IO.Path.GetFullPath(System.IO.Path.GetDirectoryName(pythonfile));
            PythonScriptHost.Instance.AddPath(fullpath);
            PythonSingleLayer layer = new PythonSingleLayer(PythonScriptHost.Instance, File.ReadAllText(pythonfile), name);
            
            return layer;
        }

        public void Load()
        {

            _layers = ChainLoader.LoadChain("prefab_single", LayerDirectory);
            PythonScriptHost.Instance.LoadAssembly(typeof(PrefabSingleLogic).Assembly);
            _interpreter = LoadSingleLayer(_singleLayerName);

            UpdateLogic();
        }

        public string GetPtypeDatabase()
        {
            return (_layers[0].Layer as FeatureDetectionLayer).PtypeLibrary;
        }

        public void UpdateLogic()
        {
            AnnotationUpdateUtility.UpdateInvalidatedLayers(_layers);
            Storage = RuntimeStorage.FromCouchDb("prefab_single_layer");
            var args = new ProcessAnnotationArgs(Storage, GetPtypes());
            
            if (File.Exists(_singleLayerName))
            {
                _interpreter = LoadSingleLayer(_singleLayerName);
                _interpreter.ProcessAnnotations(args);
            }
            
            
        }

        public IEnumerable<Ptype> GetPtypes()
        {
            return (_layers[0].Layer as FeatureDetectionLayer).GetPtypes();
        }

        public Dictionary<string, List<IAnnotation>> GetAnnotationsMatchingNode(Tree node, Tree root, string bitmapid)
        {
            Dictionary<string, List<IAnnotation>> toreturn = new Dictionary<string, List<IAnnotation>>();

            Dictionary<string, JToken> all = Storage.ReadAllData();

            string nodepath = PathDescriptor.GetPath(node, root);
            List<IAnnotation> matches = new List<IAnnotation>();


            foreach (string key in all.Keys)
            {
                if (key.Equals(nodepath))
                {
                    matches.Add(new PathDescriptorAnnotation(key, all[key]));
                }
            }

            toreturn.Add("prefab_single", matches);

            return toreturn;
        }

        public IEnumerable<IRuntimeStorage> GetRuntimeStorages()
        {
            var intents = new List<IRuntimeStorage>();
            foreach (LayerWrapper lw in _layers)
            {
                intents.Add(lw.Intent);
            }
            intents.Add(Storage);

            return intents;
        }

        public Dictionary<string, List<IAnnotation>> GetAllAnnotations()
        {
            Dictionary<string, List<IAnnotation>> toreturn = new Dictionary<string, List<IAnnotation>>();
            Dictionary<string, JToken> all = Storage.ReadAllData();
            List<IAnnotation> matches = new List<IAnnotation>();


            foreach (string key in all.Keys)
            {
                matches.Add(new PathDescriptorAnnotation(key, all[key]));
            }

            toreturn.Add("", matches);

            return toreturn;
        }


        public IEnumerable<string> GetAnnotationLibraries()
        {
            return AnnotationLibrary.GetAnnotationLibraries(_layers);
        }
    }
}
