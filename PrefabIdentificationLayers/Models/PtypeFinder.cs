using System;
using PrefabIdentificationLayers.Prototypes;
using Prefab;
using System.Collections.Generic;

namespace PrefabIdentificationLayers.Models
{
	public interface PtypeFinder
	{
		void FindOccurrences(Ptype ptype, Bitmap bitmap, IEnumerable<Tree> features, List<Tree> found);

		/// <summary>
		/// Override this to find content using the foreground image.
		/// </summary>
		void FindContent(Tree occurrence, Bitmap image, Bitmap foreground, List<Tree> found);

		/// <summary>
		/// Override this if you'd like to diff an occurrence's background with
		/// an image to locate any content. You can use the common CommonSetForeground
		/// function, which just diffs a rectangle and stores a foreground color (anything but 0)
		/// in the foreground bitmap.
		/// </summary>
		void SetForeground(Tree node, Bitmap image, Bitmap foreground);
	}
}

