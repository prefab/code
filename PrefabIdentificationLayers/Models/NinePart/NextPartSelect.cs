using System;
using System.Collections.Generic;
using PrefabIdentificationLayers.Models;
using Prefab;
using System.Linq;

namespace PrefabIdentificationLayers.Models
{
	class NextPartSelecter : IPartOrderSelecter
	{
		public static readonly NextPartSelecter Instance = new NextPartSelecter();
		private NextPartSelecter(){}
		public Part SelectNextPartToAssign(Dictionary<string, Part> parts, IEnumerable<Bitmap> positives, IEnumerable<Bitmap> negatives)
		{
			IEnumerable<Part> excludingInterior = parts.Values.Where((p) => p != parts["interior"]);
			if (SearchPtypeBuilder.CompleteAssignment(excludingInterior))
				return parts["interior"];

			return MRVSelecter.SelectNextPartToAssign(excludingInterior);
		}
	}
}

