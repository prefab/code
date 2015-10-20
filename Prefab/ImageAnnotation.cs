using Newtonsoft.Json.Linq;
using System;

namespace Prefab
{
	public class ImageAnnotation
	{

        
        public string Id { 
            get {

                return GetAnnotationId(ImageId, Region);
               
            } 
        }

		public readonly string ImageId;

		public readonly JObject Data;

		public readonly IBoundingBox Region;

        public static string GetImageId(Bitmap image)
        {
            return image.GetHashCode().ToString();
        }

        public static string GetAnnotationId(string imageId, IBoundingBox region)
        {
            return imageId + "-" + region.Left + "-" + region.Top + "-" + region.Width + "-" + region.Height;
        }

        public static string GetAnnotationId(Bitmap image, IBoundingBox region)
        {
            string imgid = GetImageId(image);
            return GetAnnotationId(imgid, region);
        }

		public ImageAnnotation (IBoundingBox region, JObject data, string imgid)
		{
			Region = region;
			Data = data;
			ImageId = imgid;

		}
	}
}

