using System;
using LoveSeat;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


namespace Prefab
{
	[Serializable]
	public class CouchDbAnnotation{

		[JsonProperty("_id")]
		public string Id{ get; set; }

		[JsonProperty("_rev")]
		public string Rev{get; set;}

		[JsonProperty]
		public JObject data{ get; set; }

		[JsonProperty]
		public int top{ get; set; }

		[JsonProperty]
		public int left{ get; set; }

		[JsonProperty]
		public int width { get; set; }

		[JsonProperty]
		public int height{ get; set; }


		[JsonProperty]
		public string screenshotId{ get; set; }

		[JsonProperty]
		public string type{ get; set; }


	}


}

