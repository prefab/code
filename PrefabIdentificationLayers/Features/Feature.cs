
using Prefab;
using System.Collections.Generic;
namespace PrefabIdentificationLayers.Features{

	public sealed class Feature {

		public const int TRANSPARENT_VALUE = 0;

		public readonly int Id;
		public readonly Bitmap Bitmap;


		private Feature(int id, Bitmap bitmap)
		{
			this.Id = id;
			this.Bitmap = bitmap;
		}


		public override bool Equals(object obj)
		{
			if(! (obj is Feature) )
				return false;
			Feature feature = (Feature)obj;

			return feature.Id == Id;
		}


		public override int GetHashCode(){
			return 17 * 31 + Id;
		}

		public class Factory
		{
			private HashSet<Feature> features;
			public Factory()
			{
				features = new HashSet<Feature>();
			}

			public Feature Create(Bitmap bitmap)
			{
				foreach (Feature feature in features)
				{
					if (Bitmap.ExactlyMatches(bitmap, feature.Bitmap))
					{
						return feature;
					}
				}

				Feature f = new Feature(features.Count, bitmap);
				features.Add(f);

				return f;
			}


		}


		public static List<Feature> LoadDummy(){
			List<Feature> features  = new List<Feature>();

			Bitmap bitmap = Bitmap.FromFile("../prefab/AdobePreferences.png");
			Bitmap feature = Bitmap.Crop(bitmap, 606, 614, 2,2 );
			Bitmap feature2 = Bitmap.Crop(bitmap, 681, 614, 2, 2);
			Bitmap feature3 = Bitmap.Crop(bitmap, 606, 637, 2, 2);
			Bitmap feature4 = Bitmap.Crop(bitmap, 681, 637, 2, 2);
			Bitmap feature5 = Bitmap.Crop(bitmap, 219, 318, 12, 12);

			Factory factory = new Factory();
			Feature dummy = factory.Create(feature);
			Feature dummy2 = factory.Create(feature2);
			Feature dummy3 = factory.Create(feature3);
			Feature dummy4 = factory.Create(feature4);
			Feature dummy5  = factory.Create(feature5);
			features.Add(dummy);
			features.Add(dummy2);
			features.Add(dummy3);
			features.Add(dummy4);
			features.Add(dummy5);
			return features;
		}



	}
}
