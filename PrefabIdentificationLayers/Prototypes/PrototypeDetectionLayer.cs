using System;
using Prefab;
using System.Collections.Generic;
using PrefabIdentificationLayers.Features.FeatureTree;

namespace PrefabIdentificationLayers.Prototypes
{
    [Serializable]
	public class PrototypeDetectionLayer : Layer
	{
		private List<Ptype> ptypes;
		private List<Tree> found;
		private Dictionary<string, object> shared;
		
		public String Name {
			get{
				return "prototype_detection";
			}
		}


		public void Init(Dictionary<string, object> parameters) {
			shared = (Dictionary<string,object>)parameters["shared"];
			found = new List<Tree>();
		}

		
		public void Interpret(InterpretArgs args) {

			ptypes = (List<Ptype>)shared[Features.FeatureDetectionLayer.SHARED_PTYPES_KEY];
			found.Clear();

			//Finding prototype occurrences
			Bitmap pixels = (Bitmap)args.Tree["capturedpixels"];
			IEnumerable<Tree> features = args.Tree.GetChildren();

			foreach(Ptype p in ptypes){
				p.Model.Finder.FindOccurrences(p, pixels, features, found);
			}


			//Creating the spatial hierarchy
			Tree.AddNodesToCollection(args.Tree, found);
            SetAncestorsByContainment(found, args);

		}

        public static void SetAncestorsByContainment(List<Tree> nodes, InterpretArgs args)
        {
            nodes.Sort(CompareByAreaAndFeaturesFirst);

            for (int i = 0; i < nodes.Count - 1; i++)
            {
                Tree curr = nodes[i];

                for (int j = i + 1; j < nodes.Count; j++)
                {

                    Tree next = nodes[j];
                    if (BoundingBox.IsInsideInclusive(curr, next))
                        args.SetAncestor(curr, next);
                }
            }
        }

        public static int CompareByAreaAndFeaturesFirst(Tree a, Tree b)
        {
            int val =  CompareByArea(a, b);
            if (val == 0)
            {
                if (a.HasTag("type") && a["type"].Equals("feature") && 
                    (!b.HasTag("type") || !b["type"].Equals("feature")))
                    return -1;
                else if (b.HasTag("type") && b["type"].Equals("feature"))
                    return 1;
            }

            return val;
        }

		private static int CompareByArea(Tree a, Tree b){
			return (a.Width * a.Height) - (b.Width * b.Height); 
		}

		public void AfterInterpret(Tree tree) {
			
		}


		public void ProcessAnnotations(AnnotationArgs args) {
			
		}

		
		public IEnumerable<string> AnnotationLibraries() {
			return null;
		}

		
		public void Close(){

		}


		public static List<Tree> FindPrototypeOccurrences(Bitmap bitmap, Ptype.Mutable ptype){
			List<Ptype.Mutable> ptypes = new List<Ptype.Mutable>();
			ptypes.Add(ptype);
			List<Ptype> lib = Ptype.CreatePrototypeLibrary(ptypes);

			List<Tree> foundPtypes = new List<Tree>();

			try {



				FeatureTree tree = FeatureTree.BuildTree(lib[0].Features());
				List<Tree> foundFeatures = new List<Tree>();
				tree.MultiThreadedMatch(bitmap, foundFeatures);

				foreach(Ptype p in lib){
					p.Model.Finder.FindOccurrences(p, bitmap,foundFeatures, foundPtypes);
				}

			} catch{

			}

			return foundPtypes;

		}

	}
}

