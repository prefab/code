using System;
using PrefabIdentificationLayers.Prototypes;
using System.Collections.Generic;
using Prefab;
using PrefabIdentificationLayers.Regions;

namespace PrefabIdentificationLayers.Models.NinePart
{
	public class Builder : IPtypeFromAssignment
	{
		private Builder() { }
		public static readonly Builder Instance = new Builder();


		public Ptype.Mutable ConstructPtype(Dictionary<string, Part> assignment, IEnumerable<Bitmap> positives, IEnumerable<Bitmap> negatives, Dictionary<object,object> cache)
		{
			Dictionary<string, Bitmap> features = new Dictionary<string, Bitmap>();
			Dictionary<string, Region> regions = new Dictionary<string, Region>();
			foreach (KeyValuePair<string, Part> pair in assignment)
			{
				Part part = pair.Value;
				string name = pair.Key;
				object extracted = Extractor.ExtractPart(name, part.AssignedValue, assignment, positives, negatives, cache);

				if(extracted is Bitmap){
					Bitmap value = (Bitmap)extracted;

					features.Add(name, value);
				}
				else if (extracted is Region)
					regions.Add(name, (Region) extracted);
				else
					regions.Add(name, ((BackgroundResults)extracted).Region);
			}

			try{
				return new Ptype.Mutable(features, regions);

			}catch{
				return null;
			}

		}
	}
}

