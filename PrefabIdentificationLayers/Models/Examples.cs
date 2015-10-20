

/**
 * Created with IntelliJ IDEA.
 * User: mdixon
 * Date: 9/23/13
 * Time: 11:55 AM
 * To change this template use File | Settings | File Templates.
 */
using Prefab;
using System.Collections.Generic;


namespace PrefabIdentificationLayers.Models{
	public sealed class Examples {


		public Examples(List<Bitmap> positives, List<Bitmap> negatives)
		{
			this.Positives = positives;
			this.Negatives = negatives;
		}

		public readonly List<Bitmap> Positives;

		public readonly List<Bitmap> Negatives;
	}

}
