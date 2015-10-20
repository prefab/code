using System;

using System.Collections.Generic;
using Prefab;
using PrefabIdentificationLayers.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrefabIdentificationLayers.Regions;
namespace PrefabIdentificationLayers.Prototypes
{
	public class PtypeSerializationUtility
	{
		private Dictionary<string, PtypeMetadata> ptypeData;
		private IRuntimeStorage intent;

        private static readonly string INTENT_PTYPES = "ptypes";

		public PtypeSerializationUtility(){}


		private Dictionary<string, PtypeMetadata> LoadPtypesFromIntent(){

			JToken ptypeStr = intent.GetData(INTENT_PTYPES);
			Dictionary<string, PtypeMetadata> ptypeData = new Dictionary<string, PtypeMetadata>();
			if(ptypeStr != null){

                JArray arr = ptypeStr.Value<JArray>();
				foreach(JObject obj in arr){

					string id = obj.GetValue ("id").Value<string> ();

					PtypeMetadata data = new PtypeMetadata(id);
					ptypeData.Add(id, data);


					JArray examplesJson = obj["examples"].Value<JArray>();

					foreach(JObject exJson in examplesJson){
						Example e = GetExample(exJson);
						data.Examples.Add(e);
					}

					JArray featuresJson = obj["features"].Value<JArray>();
					Dictionary<string, Bitmap> features = GetFeatures(featuresJson);

					JArray regionsJson = obj["regions"].Value<JArray>();
					Dictionary<string, Region> regions = GetRegions(regionsJson);
					string model = obj.Value<string> ("model");


					data.Ptype.Model = model;
					data.Ptype.Features = features;
					data.Ptype.Regions = regions;
				}
			}

			return ptypeData;
		}


		private void SavePtypesToIntent(IRuntimeStorage intent, IEnumerable<PtypeMetadata> ptypeData){
			JArray array = new JArray();
			foreach(PtypeMetadata data in ptypeData){

				JObject item = new JObject();

				item.Add("id", data.Ptype.Id);
				item.Add("model", data.Ptype.Model);

				JArray features = new JArray();
				JArray regions = new JArray();

				AddFeaturesToJArray(features, data);
				AddRegionsToJArray(regions, data);

				item.Add("features", features);
				item.Add("regions", regions);
                
				JArray examples = new JArray();
				foreach(Example e in data.Examples){
					JObject ejson = new JObject();
					ejson.Add("imageId", e.ImageId);
					ejson.Add("positive", e.IsPositive);

					JObject region = new JObject();
					region.Add("top", e.Region.Top);
					region.Add("left", e.Region.Left);
					region.Add("width", e.Region.Width);
					region.Add("height", e.Region.Height);

					ejson.Add("region", region);

					examples.Add(ejson);
				}

				item.Add("examples", examples);
				array.Add(item);
			}
            
			
			intent.PutData(INTENT_PTYPES , array);
		}

		public List<Ptype> LoadPtypes(IRuntimeStorage intent)
        {
			this.intent = intent;
			ptypeData = LoadPtypesFromIntent();

			List<Ptype.Mutable> ptypes = new List<Ptype.Mutable>();
			foreach(PtypeMetadata data in ptypeData.Values)
				ptypes.Add(data.Ptype);

			return Ptype.CreatePrototypeLibrary(ptypes);
		}

		public bool UpdatePtypes(List<AnnotatedNode> annotations, List<Ptype> newLib){

			bool anyRemoved = RemoveDeletedAnnotations(annotations, ptypeData);
			bool needsUpdate = LoadDataFromAnnotations(annotations, ptypeData) || anyRemoved;

			Dictionary<string, Bitmap> images = new Dictionary<string, Bitmap>();
            foreach (AnnotatedNode n in annotations)
            {
                if(!images.ContainsKey(n.ImageId))
                    images.Add(n.ImageId, (Bitmap)n.Root["capturedpixels"]);
            }

			List<BuildPrototypeArgs> buildargs = new List<BuildPrototypeArgs>();
			List<Ptype.Mutable> ptypes = new List<Ptype.Mutable>();
			foreach(PtypeMetadata data in ptypeData.Values){
				if(data.NeedsUpdate){

					List<Bitmap> positives = new List<Bitmap>();
					List<Bitmap> negatives = new List<Bitmap>();

					foreach(Example e in data.Examples){
						Bitmap example =  Bitmap.Crop(images[e.ImageId], e.Region  );
						if(e.IsPositive)
							positives.Add(example );
						else
							negatives.Add(example);
					}

					Examples examples = new Examples(positives,negatives);
					BuildPrototypeArgs args = new BuildPrototypeArgs(examples, ModelInstances.Get(data.Ptype.Model), data.Ptype.Id);

					buildargs.Add(args);
				}else{
					ptypes.Add(data.Ptype);
				}
			}


			ptypes.AddRange(Ptype.BuildFromExamples(buildargs));

			foreach(Ptype.Mutable ptype in ptypes)
			{
				PtypeMetadata data = ptypeData[ptype.Id];
				data.Ptype.Features = ptype.Features;
				data.Ptype.Regions = ptype.Regions;
				data.Ptype.Model = ptype.Model;
			}

			SavePtypesToIntent(intent, ptypeData.Values);

			newLib.AddRange(Ptype.CreatePrototypeLibrary(ptypes));

			return needsUpdate;

		}


        public class PtypeTrainingExamples
        {

            private List<ImageAnnotation> _positives;
            private List<ImageAnnotation> _negatives;

            public IEnumerable<ImageAnnotation> Positives
            {
                get { return _positives; }
            }


            public IEnumerable<ImageAnnotation> Negatives
            {
                get { return _negatives; }
            }

            public PtypeTrainingExamples(List<ImageAnnotation> positives, List<ImageAnnotation> negatives) 
            {

                _negatives = new List<ImageAnnotation>(negatives);
                _positives = new List<ImageAnnotation>(positives);

            }
           

        }


        public static PtypeTrainingExamples GetTrainingExamples(string libraryname, string ptypeId)
        {
            // Get all ptype annotations.

            IEnumerable<ImageAnnotation> annotations= AnnotationLibrary.GetAnnotations(libraryname);

            List<ImageAnnotation> positives = new List<ImageAnnotation>();
            List<ImageAnnotation> negatives = new List<ImageAnnotation>();
            foreach (ImageAnnotation a in annotations)
            {
                foreach (JObject ptype in a.Data["ptypes"])
                {
                    if (ptype["ptypeId"].Value<string>().Equals(ptypeId))
                    {
                        if (ptype["positive"].Value<bool>())
                            positives.Add(a);
                        else
                            negatives.Add(a);
                    }
                }
            }

            return new PtypeTrainingExamples(positives, negatives);
        }

		private static bool RemoveDeletedAnnotations(List<AnnotatedNode> annotations, Dictionary<string, PtypeMetadata> ptypeData){
			bool anyRemoved = false;

			List<PtypeMetadata> currPtypes = new List<PtypeMetadata>();
			currPtypes.AddRange(ptypeData.Values);
			foreach(PtypeMetadata data in currPtypes){
				bool wasDeleted = true;
                foreach (AnnotatedNode a in annotations)
                {

                    foreach (JObject ptypeJson in a.Data["ptypes"])
                    {
                        if (data.Ptype.Id.Equals(ptypeJson["ptypeId"].Value<string>()))
                        {
                            wasDeleted = false;
                            break;
                        }
                    }
                    if (!wasDeleted)
                        break;
                }

				if(wasDeleted){
					anyRemoved = true;
					ptypeData.Remove(data.Ptype.Id);
				}
			}

			return anyRemoved;
		}

		private static bool LoadDataFromAnnotations(List<AnnotatedNode> annotations, Dictionary<string, PtypeMetadata> ptypeData) 
        {

			bool needsUpdate = false;
			foreach(AnnotatedNode a in annotations){


				string id;

                JObject alldata = a.Data;
                JArray ptypes = alldata["ptypes"].Value<JArray>();

                foreach (JObject dataJson in ptypes)
                {

                    id = dataJson["ptypeId"].Value<string>();
                    PtypeMetadata data;
                    if (ptypeData.ContainsKey(id))
                        data = ptypeData[id];
                    else
                    {
                        data = new PtypeMetadata(id);
                        ptypeData.Add(id, data);
                        data.NeedsUpdate = true;
                    }

                    bool isPos = dataJson["positive"].Value<bool>();
                    Example ex = new Example(a.Region, isPos, a.ImageId);

                    if (!data.Examples.Contains(ex))
                    {
                        data.Examples.Add(ex);
                        data.NeedsUpdate = true;
                    }
                    string model = dataJson["model"].Value<string>();


                    if (data.Ptype.Model == null || !data.Ptype.Model.Equals(model))
                    {
                        data.Ptype.Model = model;
                        data.NeedsUpdate = true;
                        needsUpdate = true;
                    }
                }

			}

			return needsUpdate;
		}

		private static void AddRegionsToJArray(JArray array, PtypeMetadata data){

			foreach(string key in data.Ptype.Regions.Keys){
				Region region = data.Ptype.Regions[key];
				JObject regionJson = new JObject();
				BitmapToJson(regionJson, region.Bitmap);
				regionJson.Add("name", key);
				regionJson.Add("matchstrategy", region.MatchStrategy);
				array.Add(regionJson);
			}
		}

		private static void AddFeaturesToJArray(JArray array, PtypeMetadata data){

			foreach(string key in data.Ptype.Features.Keys){
				Bitmap feature = data.Ptype.Features[key];
				JObject featureJson = new JObject ();
				BitmapToJson(featureJson, feature);
				featureJson.Add("name", key);

				array.Add(featureJson);
			}
		}


		private static Dictionary<string, Region> GetRegions(JArray regionsJson){
			Dictionary<string, Region> regions = new Dictionary<string, Region>();
			foreach(JObject robj in regionsJson){


				string name = robj["name"].Value<string>();
				string strategy = robj["matchstrategy"].Value<string>();
				Bitmap pixels =  BitmapFromJson(robj);

				Region region = new Region(strategy, pixels);

				regions.Add(name, region);
			}

			return regions;
		}

		private static void BitmapToJson(JObject dest, Bitmap bitmap){
			dest.Add("width", bitmap.Width );
			dest.Add("height", bitmap.Height);
			int[] pixels = bitmap.Pixels;

			byte[] bytes = new byte[pixels.Length * sizeof(int)];
			Buffer.BlockCopy (pixels, 0, bytes, 0, bytes.Length);

			string pixelstring = Convert.ToBase64String (bytes);


			dest.Add("pixels", pixelstring);

		}
		private static Bitmap BitmapFromJson(JObject obj){
			int width = obj ["width"].Value<int>();
		    int height = obj["height"].Value<int>();
			string pixelstring = obj ["pixels"].Value<string> ();


			byte[] pixelBytes = Convert.FromBase64String(pixelstring);



			int[] pixels = Bitmap.IntArrayFromByteArray (pixelBytes);

			Bitmap bmp = Bitmap.FromPixels(width, height, pixels);

			return  bmp;
		}
		private static Dictionary<string, Bitmap> GetFeatures(JArray featuresJson){
			Dictionary<string, Bitmap> features = new Dictionary<string, Bitmap>();
			foreach(JObject fobj in featuresJson){


				string name = fobj ["name"].Value<string> ();
				Bitmap bitmap = BitmapFromJson(fobj);


				features.Add(name, bitmap);
			}

			return features;
		}

		private static Example GetExample(JObject exobj){

			JToken region = exobj["region"];

			int left = region["left"].Value<int>();
			int top = region ["top"].Value<int>();
			int width = region["width"].Value<int>();
			int height = region["height"].Value<int>();

			BoundingBox bb = new BoundingBox(left, top, width, height);
			string imgId = exobj["imageId"].Value<string>();
			bool isPos = exobj ["positive"].Value<bool> ();

			Example e = new Example(bb, isPos, imgId);

			return e;
		}


		private class PtypeMetadata{
			public readonly List<Example> Examples;

			public readonly Ptype.Mutable Ptype;
			public bool NeedsUpdate;

			public PtypeMetadata(string id){
				Examples = new List<Example>();

				Ptype = new Ptype.Mutable();
				Ptype.Id = id;
				NeedsUpdate = false;
			}



		}


		private class Example{
			public readonly IBoundingBox Region;
			public readonly bool IsPositive;
			public readonly string ImageId;

			public Example(IBoundingBox region, bool isPositive, string imageId){
				this.Region = region;
				this.IsPositive = isPositive;
				this.ImageId = imageId;
			}

			
			public override bool Equals(Object o){
				if(o is  Example){
					Example e = (Example)o;

					return BoundingBox.Equals(Region, e.Region) &&
						IsPositive == e.IsPositive &&
						ImageId.Equals(e.ImageId);
				}

				return false;
			}


			
			public override int GetHashCode(){
				int result = 17;
				result = 31 * result + Region.Top;
				result = 31 * result + Region.Left;
				result = 31 * result + Region.Width;
				result = 31 * result + Region.Height;
				result = 31 * result + (IsPositive ? 1 : 0);
				result = 31 * result + ImageId.GetHashCode();

				return result;
			}
		}

	}
}

