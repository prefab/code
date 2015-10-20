using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prefab;
namespace PrefabIdentificationLayers.Features.FeatureTree
{
    /// <summary>
    /// A node in a feature tree.
    /// </summary>
    internal interface FeatureTreeNode
    {
        /// <summary>
        /// Returns a list of FeatureTreeMatches that were located in the bitmap
        /// at the given offset to probe.
        /// </summary>
        /// <param name="bitmap">The bitmap to probe.</param>
        /// <param name="probeOffset">The offset to probe.</param>
        /// <param name="bucket">The bucket to add the matches to.</param>
        /// <returns>A list of matches at the given offset. This can be null if there are no matches.</returns>
		void GetMatches(Bitmap bitmap, int probeXOffset, int probeYOffset, ICollection<Tree> bucket);
        
        /// <summary>
        /// Returns true if this is a leaf node.
        /// </summary>
        bool IsLeaf { get; }

        
    }
}
