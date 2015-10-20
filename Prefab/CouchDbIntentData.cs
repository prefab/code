using System;
using LoveSeat;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Prefab
{
	[Serializable]
	public class CouchDbIntentData
	{

		[JsonProperty("data")]
		public Dictionary<string, JToken> data{ get; set;}

	}

}

