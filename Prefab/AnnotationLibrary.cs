using System;
using System.Collections.Generic;
using LoveSeat;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Drawing;
using Newtonsoft.Json;

namespace Prefab
{
	public static class AnnotationLibrary
	{

        private static readonly string _annotationViewsFile = @"../../../database/views/annotations/annotationViews.json";
        private static readonly string _screenshotViewsFile = @"../../../database/views/screenshots/screenshotViews.json";

		public static IEnumerable<ImageAnnotation> GetAnnotations(string library)
        {
            List<ImageAnnotation> annotations = new List<ImageAnnotation>();

            if (library != null)
            {
                CouchDatabase db = GetDatabase(library);
                var options = new ViewOptions();
                options.IncludeDocs = true;

                var cannotations = db.View<CouchDbAnnotation>("all", options, "annotations");

                foreach (CouchDbAnnotation ca in cannotations.Items)
                {
                    BoundingBox bb = new BoundingBox(ca.left, ca.top, ca.width, ca.height);
                    ImageAnnotation ia = new ImageAnnotation(bb, ca.data, ca.screenshotId);
                    annotations.Add(ia);
                }
            }
            return annotations;
        }

		public static List<string> ImageIds(string library) {
            IEnumerable<ImageAnnotation> annotations = GetAnnotations(library);
			List<string> ids = new List<string>();
			foreach(ImageAnnotation a in annotations)
				ids.Add(a.ImageId);

			return ids;
		}

        public static ImageAnnotation GetAnnotation(string library, Bitmap image, IBoundingBox region)
        {

            CouchDatabase db = GetDatabase(library);
            string imgid = ImageAnnotation.GetImageId(image);
            ImageAnnotation ia = new ImageAnnotation(region, null, imgid);
            
            try
            {
                CouchDbAnnotation ca = db.GetDocument<CouchDbAnnotation>(ia.Id);
                return new ImageAnnotation(region, ca.data, imgid);
            }
            catch
            {
                return null;
            }
        }

        public static ImageAnnotation AddAnnotation(string library, Bitmap image, IBoundingBox region, JObject data)
        {

            CouchDatabase db = GetDatabase(library);
            string imgid = AddImage(db, image);
            ImageAnnotation toAdd = new ImageAnnotation(region, data, imgid);

            if (HasDocument(db, toAdd.Id))
                throw new InvalidOperationException("There already exists an annotation for this image with this location. Retreive that annotation and update its data accordingly.");

            CouchDbAnnotation ca = new CouchDbAnnotation();

            ca.data = toAdd.Data;
            ca.top = toAdd.Region.Top;
            ca.left = toAdd.Region.Left;
            ca.width = toAdd.Region.Width;
            ca.height = toAdd.Region.Height;
            ca.screenshotId = toAdd.ImageId;
            ca.type = "annotation";
            Document<CouchDbAnnotation> document = new Document<CouchDbAnnotation>(ca);
            document.Id = toAdd.Id;
            db.CreateDocument(document);


            return toAdd;
        }

        private static string AddImage(CouchDatabase db, Bitmap image)
        {
            string imgid = ImageAnnotation.GetImageId(image);
            if (!HasDocument(db, imgid))
            {
                JObject imgJson = new JObject();
                imgJson["_id"] = imgid;
                imgJson["type"] = "screenshot";
                Document doc = new Document(imgJson);
                db.CreateDocument(doc);
                System.Drawing.Bitmap bmp = Bitmap.ToSystemDrawingBitmap(image);
                ImageConverter converter = new ImageConverter();
                byte[] imgbytes = (byte[])converter.ConvertTo(bmp, typeof(byte[]));
                
                db.AddAttachment(imgid, imgbytes, "image", "image/png");
            }
            return imgid;
        }

        private static bool HasDocument(CouchDatabase db, string id)
        {
            Document d = null;
            try
            {
                d = db.GetDocument(id);
            }
            catch { }

            if (d == null)
                return false;

            return true;
        }


        public static Dictionary<string, List<ImageAnnotation>> GetAllAnnotationsForImageUsingAllLayers(IEnumerable<LayerWrapper> layers, string imageid, IBoundingBox region)
        {
            var annotations = new Dictionary<string, List<ImageAnnotation>>();
            
            var libs = GetAnnotationLibraries(layers);
            foreach (string lib in libs)
            {
                List<ImageAnnotation> forlib = new List<ImageAnnotation>();
                forlib.AddRange(GetAllAnnotationsForImage(lib, imageid, region));
                annotations[lib] = forlib;

            }

            return annotations;
        }


        public static IEnumerable<string> GetAnnotationLibraries(IEnumerable<LayerWrapper> layerChain)
        {
            
            HashSet<string> libs = new HashSet<string>();
            if (layerChain != null)
            {
                foreach (LayerWrapper lw in layerChain)
                {
                    IEnumerable<string> layerlibs = lw.Layer.AnnotationLibraries();
                    if (layerlibs != null)
                    {
                        foreach (string lib in layerlibs)
                            libs.Add(lib);
                    }
                }
            }

            return libs;
        }
        
        public static List<ImageAnnotation> GetAllAnnotationsForImage(string library, string imageId, IBoundingBox region)
        {
            CouchClient client = new CouchClient();
            
            CouchDatabase db = GetDatabase(library);
            var options = new ViewOptions();
            options.Key.Add(imageId);
            options.IncludeDocs = true;

            var cannotations = db.View<CouchDbAnnotation>("by_screenshot", options, "annotations");
            List<ImageAnnotation> annotations = new List<ImageAnnotation>();
            foreach (CouchDbAnnotation ca in cannotations.Items)
            {
                BoundingBox bb = new BoundingBox(ca.left, ca.top, ca.width, ca.height);
                if (BoundingBox.Equals(region, bb))
                {
                    ImageAnnotation ia = new ImageAnnotation(bb, ca.data, ca.screenshotId);
                    annotations.Add(ia);
                }
            }

            return annotations;
        }


        public static IEnumerable<string> GetAllImageIds(string library)
        {
            List<string> images = new List<string>();

            if (library != null)
            {
                CouchDatabase db = GetDatabase(library);
                var options = new ViewOptions();
                options.IncludeDocs = true;

                var imgDocs = db.View<JObject>("all", options, "screenshots");

                foreach (JObject doc in imgDocs.Items)
                {
                    images.Add(doc["_id"].Value<string>());
                }
            }
            return images;
        }

        public static ImageAnnotation UpdateExisting(string library, string id, JObject data)
        {
            CouchDbAnnotation toUpdate = null;
            CouchDatabase db = GetDatabase(library);
            try
            {
                toUpdate = db.GetDocument<CouchDbAnnotation>(id);
            }
            catch {
                return null; 
            }

            toUpdate.data = data;

            Document<CouchDbAnnotation> document = new Document<CouchDbAnnotation>(toUpdate);
            db.SaveDocument(document);

            return CouchDbAnnotationToImageAnnotation(toUpdate);
        }

        private static ImageAnnotation CouchDbAnnotationToImageAnnotation( CouchDbAnnotation toUpdate)
        {
            BoundingBox region = new BoundingBox(toUpdate.left, toUpdate.top, toUpdate.width, toUpdate.height);
            return new ImageAnnotation( region, toUpdate.data, toUpdate.screenshotId);
        }

		public static void Delete(string library, string id){
            CouchDatabase db = GetDatabase(library);
            Document ca = db.GetDocument(id);
            db.DeleteDocument(ca.Id, ca.Rev);
		}


		public static int GetLibraryId(string library) {
			int id = 17;
            List<ImageAnnotation> annotations = new List<ImageAnnotation>(GetAnnotations(library));
            annotations.Sort((a, b) => string.Compare(a.Id, b.Id));
			foreach(ImageAnnotation a in annotations){
				id = 31 * id + a.Id.GetHashCode();
                id = 31 * id + GetDataHash(a.Data);
			}

			return id;
		}

        private static int GetDataHash(JObject data)
        {
            int hash = 17;
            List<string> keys = new List<string>();
            foreach (var prop in data.Properties())
            {
                keys.Add(prop.Name);
            }

            keys.Sort();

            foreach (string key in keys)
            {
                hash = 31 * hash + key.GetHashCode();
                hash = 31 * hash + JsonConvert.SerializeObject(data[key]).GetHashCode();
            }

            return hash;
        }


        private static CouchDatabase GetDatabase(string library)
        {
            CouchClient client = new CouchClient();
            CouchDatabase db = null;
            if (!client.HasDatabase(library))
            {
                client.CreateDatabase(library);
                JObject annotationViewsJson = JObject.Parse(File.ReadAllText(_annotationViewsFile));
                JObject annotationDesignDoc = new JObject();
                annotationDesignDoc["language"] = "javascript";
                annotationDesignDoc["views"] = annotationViewsJson;

                JObject screenshotViewsJson = JObject.Parse(File.ReadAllText(_screenshotViewsFile));
                JObject screenshotDesignDoc = new JObject();
                screenshotDesignDoc["language"] = "javascript";
                screenshotDesignDoc["views"] = screenshotViewsJson;

                
                db = client.GetDatabase(library);

                Document aView = new Document(annotationDesignDoc);
                aView.Id = "_design/annotations";
                

                Document sView = new Document(screenshotDesignDoc);
                sView.Id = "_design/screenshots";


                db.CreateDocument(aView);
                db.CreateDocument(sView);
            }

            db = client.GetDatabase(library);

            return db;
        }

        public static Bitmap GetImage(string library, string id)
        {
            CouchDatabase db = GetDatabase(library);
            Stream stream = db.GetAttachmentStream(id, "image");
            try
            {
                System.Drawing.Bitmap image = new System.Drawing.Bitmap(stream);
                return Bitmap.FromSystemDrawingBitmap(image);
            }
            catch (IOException e)
            {

                Console.Error.WriteLine(e.StackTrace);
            }

            return null;
        }

        public static void AddAnnotation(string libraryname, Tree node, JObject metadata)
        {
            Bitmap bmp = node["capturedpixels"] as Bitmap;
            AddAnnotation(libraryname, bmp, node, metadata);
        }

	}
}