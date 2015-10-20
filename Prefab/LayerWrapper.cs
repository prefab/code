using System;
using System.Collections.Generic;

namespace Prefab
{
	public class LayerWrapper
	{
		public readonly Dictionary<string, object> Parameters;
		public readonly IRuntimeStorage Intent;
		public readonly Layer Layer;
		public readonly string Id;

		public LayerWrapper (Layer layer, 
			Dictionary<string,object> parameters,
			IRuntimeStorage intent,
			string id)
		{
			this.Layer = layer;
			this.Id = id;
			this.Intent = intent;
			this.Parameters = parameters;
		}
	}
}

