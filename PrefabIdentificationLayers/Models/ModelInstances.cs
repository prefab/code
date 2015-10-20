using System;
using PrefabIdentificationLayers.Models.NinePart;

namespace PrefabIdentificationLayers.Models
{
	public class ModelInstances
	{      
		private ModelInstances(){}

		public static readonly Model NinePart = new Model( "ninepart",
			new SearchPtypeBuilder("ninepart",
				CostFunction.Instance,
				PartGetter.Instance, //parts
				PartGetter.Instance, //constraints
				NextPartSelecter.Instance, //how to select parts in search
				Builder.Instance), //how to build a ptype from full assignment
			Finder.Instance); //how to find ptype occurrences


		public static readonly Model OnePart = new Model("onepart",
			OnePartLogic.Instance,
			OnePartLogic.Instance);

		public static readonly Model[] All = {NinePart, OnePart};

		public static Model Get(string name){
			foreach(Model m in All)
				if(m.Name.Equals(name))
					return m;

			return null;
		}
	}
}

