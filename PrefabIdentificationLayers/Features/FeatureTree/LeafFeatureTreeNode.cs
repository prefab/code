﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prefab;
using System.Diagnostics;
namespace PrefabIdentificationLayers.Features.FeatureTree
{
    /// <summary>
    /// A leaf node in a FeatureTree.
    /// </summary>
    internal class LeafFeatureTreeNode : FeatureTreeNode
    {
        /// <summary>
        /// The features that are matched at this node.
        /// </summary>
        public FeatureWrapper FeatureWithHotspot;

        /// <summary>
        /// Any remaining offsets to match to see if the features should be returned.
        /// </summary>
        public List<Point> OffsetsToTest;

        /// <summary>
        /// Constructs a leaf node from the given parameters.
        /// A reference to the features list is kept, so do not
        /// modify the list after constructing this node.
        /// </summary>
        /// <param name="features">The features at this node.</param>
        /// <param name="offsetsToTest">Any remaining offests to test before returning matches.</param>
        public LeafFeatureTreeNode(FeatureWrapper feature, List<Point> offsetsToTest)
        {
            FeatureWithHotspot = feature;

            OffsetsToTest = GetOffsets(feature, offsetsToTest);

        }

        /// <summary>
        /// Return any offsets that are not transparent.
        /// </summary>
        /// <param name="feature">The feature to check.</param>
        /// <param name="offsetsToTest">The offsets to check.</param>
        /// <returns>Any offets that are not transparent.</returns>
        private List<Point> GetOffsets(FeatureWrapper featurewrapper, List<Point> offsetsToTest)
        {
            List<Point> nonTransparents = new List<Point>();
            foreach (Point offset in offsetsToTest)
            {
				int translatedOffsetToTestX = offset.X +  featurewrapper.HotspotX;
				int translatedOffsetToTestY = offset.Y + featurewrapper.HotspotY;
				Point translatedOffsetToTest = new Point(translatedOffsetToTestX, translatedOffsetToTestY);
				if (Bitmap.OffsetIsInBounds(featurewrapper.Feature.Bitmap, translatedOffsetToTest) 
					&& featurewrapper.Feature.Bitmap[translatedOffsetToTest.Y, translatedOffsetToTest.X] != Features.Feature.TRANSPARENT_VALUE)
                    nonTransparents.Add(offset);
            }

            return nonTransparents;
        }

        #region FeatureTreeNode Members

        /// <summary>
        /// Collects all of the features that match the bitmap at the given probeOffset into the
        /// given feature bucket. Returns true if any matches were found.
        /// </summary>
        /// <param name="bitmap">The bitmap to find features in.</param>
        /// <param name="probeOffset">The offset that was the initial point to check in the bitmap.</param>
        /// <param name="bucket">The bucket where any found features will be added</param>
        /// <returns>True if there were any matches.</returns>
		public void GetMatches(Bitmap bitmap, int probeOffsetX, int probeOffsetY, ICollection<Tree> bucket)
        {
            
            foreach (Point offset in OffsetsToTest)
            {
                int imagex, imagey;

				int featureOffsetX = offset.X + FeatureWithHotspot.HotspotX;
				int featureOffsetY = offset.Y + FeatureWithHotspot.HotspotY;

				imagex = offset.X + probeOffsetX;
				imagey = offset.Y + probeOffsetY;

				if (imagey < 0 || imagex < 0
					|| imagey >= bitmap.Height || imagex >= bitmap.Width)
					return;


				int featurePixel = FeatureWithHotspot.Feature.Bitmap[featureOffsetY, featureOffsetX];
				if (featurePixel != bitmap[imagey, imagex])
					return;


            }

			AddMatch(probeOffsetX, probeOffsetY, bucket);
        }


        /// <summary>
        /// Returns true always. Just adds all the feature matches at this leaf node into the bucket.
        /// </summary>
        /// <param name="probeOffset"></param>
        /// <param name="bucket"></param>
        /// <returns></returns>
		private void AddMatch(int probeXOffset, int probeYOffset, ICollection<Tree> bucket)
        {
			int topleftx = probeXOffset - FeatureWithHotspot.HotspotX;
			int toplefty = probeYOffset - FeatureWithHotspot.HotspotY;

			BoundingBox fo = new BoundingBox(topleftx, toplefty, FeatureWithHotspot.Width, FeatureWithHotspot.Height);

			Dictionary<string, object> attributes = new Dictionary<string, object>();
			attributes.Add("type", "feature");
			attributes.Add("feature", FeatureWithHotspot.Feature);
            attributes.Add("feature_id", FeatureWithHotspot.Feature.Id);
			attributes.Add("hotspotX", FeatureWithHotspot.HotspotX);
			attributes.Add("hotspotY", FeatureWithHotspot.HotspotY);
			Tree node = Tree.FromBoundingBox(fo, attributes);
			bucket.Add(node);
        }

        

        /// <summary>
        /// Returns true.
        /// </summary>
        public bool IsLeaf
        {
            get { return true; }
        }

        #endregion
    }
}
