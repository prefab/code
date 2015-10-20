using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Prefab;

using PrefabIdentificationLayers.Features;
using PrefabIdentificationLayers.Regions;
using PrefabIdentificationLayers.Models;
using PrefabIdentificationLayers.Models.NinePart;

namespace PrefabIdentificationLayers.Prototypes
{
    public class Ptype
    {

	    private readonly Dictionary<string, Feature> features;
		private readonly Dictionary<string, Region> regions;
		private Dictionary<string, Ptype> children;

		public readonly Model Model;
		public readonly string Id;


		public static Ptype LoadDummy(){
			Bitmap bitmap = Bitmap.FromFile("../prefab/AdobePreferences.png");
			Bitmap feature = Bitmap.Crop(bitmap, 606, 614, 2,2 );
			Bitmap feature2 = Bitmap.Crop(bitmap, 681, 614, 2, 2);
			Bitmap feature3 = Bitmap.Crop(bitmap, 606, 637, 2, 2);
			Bitmap feature4 = Bitmap.Crop(bitmap, 681, 637, 2, 2);

			Bitmap top = Bitmap.Crop(bitmap, 608, 614, 1, 1);
			Bitmap bottom = Bitmap.Crop(bitmap, 608, 638, 1, 1);
			Bitmap left = Bitmap.Crop(bitmap, 606, 616, 1,1);
			Bitmap right = Bitmap.Crop(bitmap, 682, 616, 1, 1);
			Bitmap interior = Bitmap.Crop(bitmap, 608, 615, 1, 23);

			Dictionary<String, Bitmap> features = new Dictionary<String, Bitmap>();
			Dictionary<String, Region> regions = new Dictionary<String, Region>();

			features.Add("topleft", feature);
			features.Add("topright", feature2);
			features.Add("bottomleft", feature3);
			features.Add("bottomright", feature4);


			regions.Add("top", new Region("horizontal", top));
			regions.Add("bottom", new Region("horizontal", bottom));
			regions.Add("left", new Region("vertical", left));
			regions.Add("right", new Region("right", right));
			regions.Add("interior", new Region("horizontal", interior));

			try{
				Mutable mutable = new Mutable(features, regions);
				mutable.Model = "ninepart";
				mutable.Id = Guid.NewGuid().ToString();
				List<Mutable> all = new List<Mutable>();
				all.Add(mutable);
				IEnumerable<Ptype> lib = CreatePrototypeLibrary( all);

				return lib.First();


			}catch{

			}

			return null;

		}

		public static List<Mutable> BuildFromExamples(List<BuildPrototypeArgs> args)
		{
			List<Mutable> created = new List<Mutable>();
            List<BuildPrototypeArgs> couldntCreate = new List<BuildPrototypeArgs>();
			foreach (BuildPrototypeArgs arg in args)
			{
				Mutable result = BuildFromExamples(arg);

				if (result != null)
				{
					created.Add(result);
                }
                else
                {

                    couldntCreate.Add(arg);
                }
			}

            if (couldntCreate.Count > 0)
                throw new PtypeBuildException(couldntCreate);

			return created;
		}

		public static Mutable BuildFromExamples(BuildPrototypeArgs arg){
			Mutable result;

			result = arg.Model.Builder.BuildPrototype(arg);

			if (result != null)
			{
				result.Id = arg.Id;
				result.Model = arg.Model.Name;
			}

			return result;
		}



		private Ptype(Dictionary<string, Feature> features, Dictionary<string, Region> regions, Model model) : 
		this(Guid.NewGuid().ToString(), features, regions, model){


		}

		private Ptype(string guid, Dictionary<string, Feature> features, Dictionary<string, Region> regions, Model model)
		{
			this.Model = model;
			this.Id = guid;
			this.features = features;
			this.regions = regions;
		}

		public int GetNonGuidBasedHashCode()
		{
			int result = 17;

			result = 31 * result + Model.Name.GetHashCode();

			foreach (string featurename in features.Keys)
			{

				result = 31 * result + featurename.GetHashCode();
				result = 31 * result + features[featurename].GetHashCode();
			}

			foreach (KeyValuePair<string, Region> pair in regions)
			{
				result = 31 * result + pair.Key.GetHashCode();
				result = 31 * result + pair.Value.GetHashCode();
			}


			return result;
		}


		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}


		public override bool Equals(object obj)
		{

			if (obj is Ptype){
				Ptype ptype = (Ptype)obj;

				return ptype.Id.Equals(Id);
			}

			return false;
		}


		public override string ToString()
		{

			string result = "{ model: " + Model.Name + ", id: " + Id;
			foreach(KeyValuePair<string, Feature> pair in features)
				result += ", " + pair.Key + " width:" + pair.Value.Bitmap.Width + " height:" +pair.Value.Bitmap.Height;

			foreach (KeyValuePair<string, Region> pair in regions)
				result += ", " + pair.Key + " type:" + pair.Value.MatchStrategy + " width:" +
				pair.Value.Bitmap.Width + " height:" + pair.Value.Bitmap.Height;

			result += " }";

			return result;
		}

		public Feature Feature(string name)
		{
			return features[name];
		}

		public Region Region(string name)
		{
			return regions[name];
		}

		public IEnumerable<string> FeatureNames()
		{
			return features.Keys;
		}

		public IEnumerable<String> RegionNames()
		{
			return regions.Keys;
		}

		public IEnumerable<Feature> Features()
		{
			return features.Values;
		}

		public IEnumerable<Region> Regions()
		{
			return regions.Values;
		}

		public sealed class Hierarchical : Ptype
		{

			public Hierarchical(string guid, Dictionary<String, Feature> features,
				Dictionary<String, Region> regions, Model model, Dictionary<String, Ptype> children)
				: base(guid, features, regions, model)

			{
				this.children = children;
			}



			public Ptype GetChild(String name)
			{
				return children[name];
			}

			public IEnumerable<Ptype> Children()
			{
				return children.Values;
			}

		
			public sealed class HierarchicalMutable : Mutable
			{
				public HierarchicalMutable() : base()

				{
					children = new List<string>();
					childrenNames = new List<string>();
				}

				public HierarchicalMutable(Dictionary<String, Bitmap> features,
					Dictionary<string, Region> regions, Dictionary<string, string> children) : base(features, regions)

				{

					this.childrenNames = new List<string>(children.Keys);
					this.children = new List<string>(children.Values);
				}

				public HierarchicalMutable(Hierarchical ptype) : base((Ptype)ptype)

				{
					childrenNames = new List<String>(ptype.children.Keys);
					children = new List<string>();
					foreach (Ptype child in ptype.children.Values)
						children.Add(child.Id);
				}
				public List<string> childrenNames;

				public List<string> children;
			}

		}

		public class Mutable
		{

			public Dictionary<string, Bitmap>  Features;

			public Dictionary<string, Region> Regions;

			public String Model;

			public string Id;

			public Mutable()
			{
				Features = new Dictionary<String, Bitmap>();

				Regions = new Dictionary<String, Region>();
			}
			public Mutable(Dictionary<string, Bitmap> features, Dictionary<string, Region> regions)
			{
				List<Bitmap> fcpy = new List<Bitmap>();
				foreach (Bitmap f in features.Values)
				{

					Bitmap cpy = Bitmap.SetTransparentValues(f);
					if (Bitmap.AllTransparent(cpy)) {
						throw new Exception("Cannot create an all transparent Bitmap.");
					}

					fcpy.Add(cpy);
				}



				this.Features = new Dictionary<string,Bitmap>(features);
				this.Regions = new Dictionary<string, Region>(regions);
			}

			public Mutable(Ptype ptype)
			{
				Features = new Dictionary<String,Bitmap>();

				foreach(string fname in ptype.FeatureNames())
					Features.Add(fname, ptype.Feature(fname).Bitmap);

				Regions = new Dictionary<String,Region>();
				foreach(string rname in ptype.RegionNames())
					Regions.Add(rname, ptype.Region(rname));

				Id = ptype.Id;

				Model = ptype.Model.Name;
			}

			public static Mutable DeepCopy(Mutable ptype)
			{
				Mutable copy = null;
				if (ptype is Ptype.Hierarchical.HierarchicalMutable)
				{
					Ptype.Hierarchical.HierarchicalMutable hp = (Ptype.Hierarchical.HierarchicalMutable) ptype;
					copy = new Ptype.Hierarchical.HierarchicalMutable();
					Ptype.Hierarchical.HierarchicalMutable hpcopy = (Ptype.Hierarchical.HierarchicalMutable)copy;
					hpcopy.children.AddRange(hp.children);
					hpcopy.childrenNames.AddRange(hp.childrenNames);
				}

				else
				{
					copy = new Mutable();
				}

				copy.Regions = (Dictionary<String,Region>)ptype.Regions;
				copy.Features = (Dictionary<String,Bitmap>)ptype.Features;
				copy.Model = ptype.Model;
				copy.Id = ptype.Id;

				return copy;
			}



			public override int GetHashCode()
			{

				return Id.GetHashCode();
			}


			public override bool Equals(object obj)
			{

				if (obj is Mutable)
				{
					return ((Mutable)obj).Id.Equals(Id);
				}

				return false;
			}
		}




		public static Mutable ToMutable(Ptype ptype)
		{

			if (!(ptype is Hierarchical))
				return new Hierarchical.HierarchicalMutable((Hierarchical)ptype);

			return new Mutable(ptype);
		}

		private static IRegionMatchStrategy GetRegionMatcher(string type)
		{
			if(type.Equals("horizontal"))
				return HorizontalPatternMatcher.Instance;
			else
				return VerticalPatternMatcher.Instance;

		}

		public static List<Ptype> CreatePrototypeLibrary(IEnumerable<Mutable> buildargs)
		{
			List<Ptype> ptypes = new List<Ptype>();
			Feature.Factory features = new Feature.Factory();

			foreach (Mutable prototypebuildargs in buildargs)
			{
				ptypes.Add(CreatePtypeFromMutable(prototypebuildargs, ModelInstances.All, features, buildargs));
			}


			return ptypes;
		}

		private static Ptype CreatePtypeFromMutable(Mutable prototypebuildargs, Model[] models, Feature.Factory features, IEnumerable<Mutable> allptypes)
		{
			Dictionary<string, Bitmap> ptypefeatureshash = prototypebuildargs.Features;
			Dictionary<string,Feature> ptypefeatures = new Dictionary<string, Feature>();

			foreach (String fname in ptypefeatureshash.Keys)
			{
				Feature feature = features.Create(ptypefeatureshash[fname]);
				ptypefeatures.Add(fname, feature);
			}

			Model model = null;
			foreach(Model m in models)
				if(m.Name.Equals(prototypebuildargs.Model))
				{
					model = m;
					break;
				}


			Dictionary<string, Region> rdict = (Dictionary<string,Region>)new Dictionary<string,Region>(prototypebuildargs.Regions);


//            if (prototypebuildargs instanceof Hierarchical.HierarchicalMutable)
//            {
//                Hierarchical.HierarchicalMutable mhp = (Hierarchical.HierarchicalMutable)prototypebuildargs;
//                Dictionary<String, Ptype> children = new Dictionary<String, Ptype>();
//                for (int c = 0; c < mhp.childrenNames.size(); c++)
//                {
//                	Mutable ptype = null;
//                	for(Mutable p : allptypes){
//                		if(p.id.equals(mhp.children.get(c))){
//                			ptype = p;
//                			break;
//                		}
//                	}
//                    children.put(mhp.childrenNames.get(c),  createPtypeFromMutable( ptype, models, features, allptypes));
//                }
//                return new Hierarchical(mhp.id, fdict, rdict, model, children);
//            }
//            else
			return new Ptype(prototypebuildargs.Id, ptypefeatures, rdict, model);
		}
        
    }
}
