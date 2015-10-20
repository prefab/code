using Prefab;
using System.Collections.Generic;
using System;

using PrefabIdentificationLayers.Prototypes;
using PrefabIdentificationLayers.Features.FeatureTree;
namespace PrefabIdentificationLayers.Features{
	[Serializable]
    public class FeatureDetectionLayer : Layer {



		private FeatureTree.FeatureTree featureTree;
		private List<string> libraries;
		private Dictionary<string, object> shared;
		private IRuntimeStorage intent;
		private PtypeSerializationUtility ptypeUtil;
        public static readonly string SHARED_PTYPES_KEY = "ptypes";

        private List<Tree> _prevFeatures;

		public string Name
        {
			get{ return "feature_detection"; }
		}


		public void Init(Dictionary<string, object> parameters) {

			intent = (IRuntimeStorage)parameters["intent"];

			shared = (Dictionary<string,object>)parameters["shared"];
			libraries = new List<string>();
			if(parameters.ContainsKey("library"))
				libraries.Add(  (string)parameters["library"] );


			try{
				ptypeUtil = new PtypeSerializationUtility();
				List<Ptype> lib = ptypeUtil.LoadPtypes(intent);
                shared[SHARED_PTYPES_KEY] = lib;

				this.featureTree = FeatureTree.FeatureTree.BuildTree(GetFeatures(lib));
			}catch(Exception e){
				Console.Error.WriteLine(e.StackTrace);
			}
		}

		public static int Main(string[] args){
			FeatureDetectionLayer layer = new FeatureDetectionLayer();
			layer.Init(null);
			Bitmap bmp = Bitmap.FromFile("../prefab/AdobePreferences.png");
			Dictionary<string, object> tags = new Dictionary<string, object>();
			Tree tree = Tree.FromPixels(bmp, tags);

			InterpretArgs iargs = new InterpretArgs(tree, new Tree.BatchTransform(tree), null);

			layer.Interpret(iargs);

			return 0;
		}


		public void Interpret(InterpretArgs args) {
			Bitmap pixels = (Bitmap)args.Tree["capturedpixels"];


            if (featureTree != null)
            {
                List<Tree> found = new List<Tree>();

                if (args.Tree.HasTag("invalidated"))
                {

                    IBoundingBox invalidated = args.Tree["invalidated"] as IBoundingBox;
                    if (invalidated != null && !BoundingBox.Equals(invalidated, args.Tree) 
                        && args.Tree.HasTag("previous") && args.Tree["previous"] != null)
                    {                        
                        featureTree.MatchInvalidatedRegion(pixels, found, _prevFeatures, invalidated);

                    }
                    else
                        featureTree.MultiThreadedMatch(pixels, found);
                }
                else
                {
                    featureTree.MultiThreadedMatch(pixels, found);
                }

                

                foreach (Tree f in found)
                {
                    args.SetAncestor(f, args.Tree);
                }
            }

		}

        public static List<Tree> GetFeaturesFromTree(Tree tree)
        {
            List<Tree> features = new List<Tree>();
            GetFeaturesFromTree(tree, features);
            return features;
        }

        private static void GetFeaturesFromTree(Tree tree, List<Tree> features)
        {
            if (tree.HasTag("type") && tree["type"].Equals("feature"))
                features.Add(tree);

            foreach (Tree child in tree.GetChildren())
                GetFeaturesFromTree(child, features);
        }


		public void AfterInterpret(Tree tree) {

            _prevFeatures = GetFeaturesFromTree(tree);
		}


		public void ProcessAnnotations(AnnotationArgs args) {
			//This is where we are going to look at the annotation library
			//to learn from the examples.

			try {

				List<Ptype> lib = new List<Ptype>();

				bool needsUpdate = ptypeUtil.UpdatePtypes(args.AnnotatedNodes, lib);

				if(needsUpdate){
					shared[SHARED_PTYPES_KEY] = lib;
					featureTree = FeatureTree.FeatureTree.BuildTree( GetFeatures(lib) );
				}

			} catch (Exception e) {
				Console.WriteLine (e.StackTrace);
			}
		}


		public IEnumerable<string> AnnotationLibraries() {
			return libraries;
		}

        public string PtypeLibrary
        {
            get { return libraries[0]; }
        }

		public void Close(){
//			if(!Utils.service.isShutdown())
//				Utils.service.shutdown();
		}

		private static IEnumerable<Feature> GetFeatures(IEnumerable<Ptype> library){
			HashSet<Feature> features = new HashSet<Feature>();


			foreach (Ptype ptype in library)
			{
				IEnumerable<Feature> ptypefeatures = ptype.Features();
				foreach (Feature f in ptypefeatures)
					features.Add(f);
			}

			return features;
		}

        public IEnumerable<Ptype> GetPtypes()
        {
            return shared[SHARED_PTYPES_KEY] as IEnumerable<Ptype>;
        }
    }

}
