using System;
using PrefabIdentificationLayers.Prototypes;
using Prefab;
using System.Collections.Generic;
using PrefabIdentificationLayers.Features;
using PrefabIdentificationLayers.Regions;

namespace PrefabIdentificationLayers.Models
{
	public class OnePartLogic : PtypeBuilder, PtypeFinder
	{

		private OnePartLogic() { }

		public static readonly OnePartLogic Instance =  new OnePartLogic();


		public Ptype.Mutable BuildPrototype(IBuildPrototypeArgs args)
		{
			Examples eargs = args.Examples;
			List<Bitmap> positives = eargs.Positives;
			List<Bitmap> negatives = eargs.Negatives;

			Bitmap feature = Utils.CombineBitmapsAndMakeDifferencesTransparent(positives);
			foreach (Bitmap neg in negatives)
			{
				if (Utils.MatchesIgnoringTransparentPixels(feature, neg))
					return null;
			}

			Dictionary<string, Bitmap> dict = new Dictionary<string,Bitmap>();
			dict.Add("part", feature);


			try {
				return new Ptype.Mutable(dict, new Dictionary<String, Region>());
			} catch{
				return null;
			}

		}




		public void FindOccurrences(Ptype ptype, Bitmap bitmap, IEnumerable<Tree> features, List<Tree> found)
		{
			Feature part = ptype.Feature("part");


			foreach (Tree feature in features)
			{
				if(part.Equals(feature["feature"])){
					Dictionary<string,object> dict = new Dictionary<string,object>();
					dict.Add("type", "ptype");
					dict.Add("ptype", ptype);
                    dict.Add("ptype_id", ptype.Id);
					Tree occurrence = Tree.FromBoundingBox(feature, dict);
					found.Add(occurrence);
				}
			}
		}


		public void FindContent(Tree occurrence, Bitmap image, Bitmap foreground, List<Tree> found)
		{

		}

		public void SetForeground(Tree node, Bitmap image, Bitmap foreground)
		{
			foreground.WriteBlock(Utils.BACKGROUND, node);
		}
	}
}

