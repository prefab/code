using System;

namespace PrefabIdentificationLayers.Models
{
	public class Model
	{
		public static readonly Model NULL = null;

		public readonly string Name;
		public readonly PtypeFinder Finder;
		public readonly PtypeBuilder Builder;

		public Model(string name, PtypeBuilder builder, PtypeFinder finder){
			this.Name = name;
			this.Finder = finder;
			this.Builder = builder;
		}

	}
}

