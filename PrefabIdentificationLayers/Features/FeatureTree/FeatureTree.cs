using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;

using System.Threading.Tasks;
using System.Collections;
using PrefabIdentificationLayers.Features;
using Prefab;
using System.Collections.ObjectModel;


namespace PrefabIdentificationLayers.Features.FeatureTree
{

	public class FeatureTree{

		/// <summary>
		/// The root node of the tree.
		/// </summary>
		private FeatureTreeNode root;

		/// <summary>
		/// max over feature.hotspot.x
		/// </summary>
		private int farthestRightHotspotX;

		/// <summary>
		/// max over feature.hotspot.y
		/// </summary>
		private int farthestDownHotspotY;

		/*	* <summary>
	    /// max over (feature.Height - feature.hotspot.y - 1)
	    /// </summary>
	     * 
	     */
		private int farthestUpHotspotYFromBottom;

		/// <summary>
		/// max over (feature.Width - feature.hotspot.x - 1)
		/// </summary>
		private int farthestLeftHotspotXFromLeft;




		/// <summary>
		/// Constructs a feature tree with the given parameters.
		/// </summary>
		/// <param name="root">The root node of the tree</param>
		/// <param name="features">The features that the tree can locate</param>
		private FeatureTree(FeatureTreeNode root, int farthestUpFromBottom, int farthestLeftFromRight, int farthestDown, int farthestRight)
		{
			this.root = root;
			farthestUpHotspotYFromBottom = farthestUpFromBottom;
			farthestLeftHotspotXFromLeft = farthestLeftFromRight;
			farthestDownHotspotY = farthestDown;
			farthestRightHotspotX = farthestRight;


		}


		/// <summary>
		/// Returns all of the features found within a bitmap.
		/// </summary>
		/// <param name="bitmap">The bitmap to search through.</param>
		/// <returns>All of the features found within the bitmap.</returns>
		public void Match(Prefab.Bitmap bitmap, ICollection<Tree> found)
		{
			for (int row = 0; row < bitmap.Height; row++)
			{
				for (int col = 0; col < bitmap.Width; col++)
				{
					root.GetMatches(bitmap, col, row, found);
				}
			}
		}




		private ICollection<Tree> Match(Prefab.Bitmap bitmap,
			int top, int left, int bottomExclusive, int rightExclusive){

			List<Tree> found = new List<Tree>();
			for (int row = top; row < bottomExclusive; row++)
			{
				for (int col = left; col < rightExclusive; col++)
				{
					root.GetMatches(bitmap, col, row, found);
				}
			}

			return found;

		}

		void MultiThreadedMatch (Prefab.Bitmap bitmap, ICollection<Tree> found, int top, int left, int bottom, int right)
		{
			Parallel.For<List<Tree>>(top, bottom - 1, () => new List<Tree>(),
				(row, loop, list) =>
				{
					for (int col = left; col <= right; col++)
					{
						root.GetMatches(bitmap, col, row, list);
					}
					return list;
				}
				,
				(elmnts) =>
				{
					lock (((ICollection)found).SyncRoot)
					{
						foreach (Tree o in elmnts)
							found.Add(o);
					}
				}
			);
		}

		public void MultiThreadedMatch (Bitmap bitmap, List<Tree> found)
		{
			MultiThreadedMatch(bitmap,found, 0, 0, bitmap.Height, bitmap.Width );
		}

		/// <summary>
		/// Uses the invalidated region between two frames to find feature occurrences
		/// more efficiently.
		/// </summary>
		/// <param name="bitmap">The current frame.</param>
		/// <param name="found">The set to add the found occurrences. All occurrences within the entire bitmap will be added.</param>
		/// <param name="previous">The set of occurrences found in the previous frame.</param>
		/// <param name="invalidatedRegion">The invalidated region in the new frame.</param>
			public void MatchInvalidatedRegion(Bitmap bitmap, ICollection<Tree> found, ICollection<Tree> previous, IBoundingBox region)
		{
			int top = Math.Max (0, region.Top - farthestUpHotspotYFromBottom + 1);
			int left = Math.Max (0, region.Left - farthestLeftHotspotXFromLeft + 1);
			int bottom = Math.Min (bitmap.Height, region.Top + region.Height + farthestDownHotspotY + 1);
			int right = Math.Min (bitmap.Width, region.Left + region.Width + farthestRightHotspotX + 1);

			MultiThreadedMatch (bitmap, found, top, left, bottom, right);


			BoundingBox bb = new BoundingBox (left, top, (right - left), (bottom - top));


			foreach (Tree o in previous) {
				int hotspotX = (int)o ["hotspotX"];
				int hotspotY = (int)o ["hotspotY"];
				if (!BoundingBox.Contains (bb, o.Left + hotspotX, o.Top + hotspotY)) {
                   
                    Dictionary<string,object> tags = new Dictionary<string,object>();
                    foreach(var tag in o.GetTags())
                        tags.Add(tag.Key, tag.Value);

					found.Add (Tree.FromBoundingBox(o, tags));
				}
			}
		}

		/// <summary>
		/// Builds a tree using the given features and the
		/// default optimizations. The default optimizations are that
		/// the hotspots are Assigned as the least common pixels out of
		/// any pixels in the corpus, and the offset chosen at a given node
		/// is the offset that maximizes information gain.
		/// </summary>
		/// <param name="features">The features that will be used to build the tree.</param>
		/// <returns>A FeatureTree that can locate the given features.</returns>
		public static FeatureTree BuildTree(IEnumerable<Feature> features)
		{
			if (features == null || features.Count() == 0)
				return null;

			List<FeatureWrapper> featuresWithHotspots = AssignHotspotsByPixelFrequency (features);

			System.Drawing.Rectangle hotspotCoordinates = GetFeatureHotspotCoordinates (featuresWithHotspots);
			List<Point> validOffsets = GetAllPossibleOffsetsFromHotspotCoords (hotspotCoordinates);

			int farthestup = GetFarthestUpHotspotXFromItsBottom (featuresWithHotspots);
			int farthestleft = GetFarthestLeftHotspotXFromItsRight (featuresWithHotspots);
			int farthestdown = GetFarthestDownHotspotY (featuresWithHotspots);
			int farthestright = GetFarthestRightHotspotX (featuresWithHotspots);

			FeatureTree tree = new FeatureTree (BuildTreeHelper (featuresWithHotspots, validOffsets, true), farthestup, farthestleft, farthestdown, farthestright);

			return tree;
		}



		/// <summary>
		/// Recursively builds a tree.
		/// </summary>
		/// <param name="features">The current set of features to discriminate.</param>
		/// <param name="validOffsets">The list of offsets that can be used to discriminate features.</param>
		/// <param name="thisIsRoot">Set this to true if the current node to be built is the root node. This will
		/// make sure that the hotspot is chosen as the offset to use.</param>
		/// <returns>A feature tree node where every valid offset is checked and the leaf nodes of this indicate features.</returns>
		private static FeatureTreeNode BuildTreeHelper(List<FeatureWrapper> features, List<Point> validOffsets, bool thisIsRoot)
		{

			//Check if we're at a leaf node and return it if we are
			//If there's no features, then just make a null leaf.
			if (features.Count == 0)
				return null;

			//If there's only one feature then create a leaf node.
			if (features.Count == 1)
				return new LeafFeatureTreeNode(features[0], validOffsets);

			if (!AnyOffsetsThatDiscriminate(features, validOffsets))
				throw new Exception("No two offests discriminate these features.");
			//--------------------------------------------


			//Pick the offset (offsetToTest) that will be used to bucket the features and bucket the features by that offset
			//Pick the offset to use to build the current node's children and remove from the list of valid offsets.----------
			//And bucket the features by that offset.
			Point offsetToTest = new Point(0,0);
			Dictionary<int , List<FeatureWrapper>> featuresByColor = ChooseOffsetToTeset(features, validOffsets, thisIsRoot, offsetToTest);
			List<Point> remainingValidOffsets = new List<Point>(validOffsets);
			remainingValidOffsets.Remove(offsetToTest);
			//----------------------------------------------------------------------------------------


			//Build the transparent child node
			//Build the transparent child-------------------------------------------------------------------

			FeatureTreeNode transparentNode = null;

			if(featuresByColor.ContainsKey(Feature.TRANSPARENT_VALUE)){
				transparentNode = BuildTreeHelper(featuresByColor[Feature.TRANSPARENT_VALUE], remainingValidOffsets, false);
				featuresByColor.Remove(Feature.TRANSPARENT_VALUE);
			}

			//----------------------------------------------------------------------------------------



			//Build the rest of the children------------------------------------------------------------------
			Dictionary<int, FeatureTreeNode> nodesByColor = new Dictionary<int, FeatureTreeNode>();
			foreach (int key in featuresByColor.Keys)
			{
				nodesByColor.Add(key, BuildTreeHelper(featuresByColor[key], remainingValidOffsets, false));
			}
			//-----------------------------------------------------------------------------------------



			return new InnerFeatureTreeNode(offsetToTest, nodesByColor, transparentNode);
		}

		/// <summary>
		/// Chooses the offset to test. This is the default function that looks for the orgin if
		/// the root node is being constructed, or the offset that maximizes information gain.
		/// </summary>
		/// <param name="features">The features to look through.</param>
		/// <param name="validOffsets">The list of possible offsets to choose from.</param>
		/// <param name="isRoot">True if the root node is being made and the origin offset should be returned.</param>
		/// <returns>The default bitmap offset that should be used to discriminate the given features.</returns>
		private static Dictionary<int, List<FeatureWrapper>> ChooseOffsetToTeset(List<FeatureWrapper> features, List<Point> validOffsets, bool isRoot, Point testedOffset)
		{
			if (isRoot)
			{
				testedOffset.X = 0;
				testedOffset.Y = 0;
				Dictionary<int, List<FeatureWrapper>> toReturn = bucketFeatures(testedOffset, features);
				return toReturn;
			}

			return offsetThatProvidesMostInformationGain(features, validOffsets, testedOffset);
		}

		/// <summary>
		/// Returns the offset that distinguishes the given features the best.
		/// </summary>
		/// <param name="features">The features to disinguish.</param>
		/// <param name="validOffsets">The possible offsets to choose from.</param>
		/// <returns>The offset that distinguishes the given features the best.</returns>
		private static Dictionary<int, List<FeatureWrapper>> offsetThatProvidesMostInformationGain(List<FeatureWrapper> features, List<Point> validOffsets,  Point testedOffset)
		{
			int maxVariance = -1;

			Point offsetToReturn = null;
			Dictionary<int, List<FeatureWrapper>> bestBucketedFeatures = new Dictionary<int, List<FeatureWrapper>>();
			Dictionary<int, List<FeatureWrapper>> currBucketedFeatures;
			foreach (Point offset in validOffsets)
			{
				currBucketedFeatures = bucketFeatures(offset, features);

				if (currBucketedFeatures.Count > maxVariance)
				{
					maxVariance = currBucketedFeatures.Count;
					bestBucketedFeatures = currBucketedFeatures;
					offsetToReturn = offset;
				}
			}

			testedOffset.X = offsetToReturn.X;
			testedOffset.Y = offsetToReturn.Y;

			return bestBucketedFeatures;

		}

		/// <summary>
		/// Buckets each feature by its pixel value at the given offset.
		/// </summary>
		/// <param name="offset">The offset, relative to a feature's hotspot to probe.</param>
		/// <param name="features">The set of features to bucket.</param>
		/// <returns>A dictionary of features bucketed by their pixel value.</returns>
		private static Dictionary<int, List<FeatureWrapper>> bucketFeatures(Point offset, List<FeatureWrapper> features)
		{
			Dictionary<int, List<FeatureWrapper>> featuresByColor = new Dictionary<int, List<FeatureWrapper>>();
			foreach (FeatureWrapper featurewrapper in features)
			{
				int pixelValue = Feature.TRANSPARENT_VALUE;
				Point featureOffset = new Point(offset.X + featurewrapper.HotspotX, offset.Y + featurewrapper.HotspotY);
				if (Bitmap.OffsetIsInBounds(featurewrapper.Feature.Bitmap, featureOffset))
					pixelValue = featurewrapper.Feature.Bitmap[featureOffset.Y, featureOffset.X];

				if (featuresByColor.ContainsKey(pixelValue))
					featuresByColor[pixelValue].Add(featurewrapper);
				else
				{
					List<FeatureWrapper> list = new List<FeatureWrapper>();
					list.Add(featurewrapper);
					featuresByColor.Add(pixelValue , list);
				}
			}
			return featuresByColor;
		}

		/// <summary>
		/// Returns true if there are any offsets at all that can discriminate at least one feature from every other feature.
		/// </summary>
		/// <param name="features">The features to check.</param>
		/// <param name="validOffsets">The possible offsets to search through.</param>
		/// <returns>True if there are any offsets at all that can discriminate at least one feature from every other feature.</returns>
		private static bool AnyOffsetsThatDiscriminate(List<FeatureWrapper> features, List<Point> validOffsets)
		{
			foreach (Point offset in validOffsets)
			{
				if (!EveryFeatureIsTheSameAtOffset(features, offset))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Returns true if every feature has the same value at the given offset relative to their hotspot.
		/// </summary>
		/// <param name="features">The features to check.</param>
		/// <param name="offset">The offsets to try.</param>
		/// <returns>True if every feature has the same value at the given offset relative to their hotspot.</returns>
		private static bool EveryFeatureIsTheSameAtOffset(List<FeatureWrapper> features, Point offset)
		{
			FeatureWrapper representative = features[0];

			int pixelValue = Feature.TRANSPARENT_VALUE;
			Point offsetToCheck = new Point(offset.X + representative.HotspotX,
				offset.Y + representative.HotspotY);


			if (Bitmap.OffsetIsInBounds(representative.Feature.Bitmap, offsetToCheck))
				pixelValue = representative.Feature.Bitmap[offsetToCheck.Y, offsetToCheck.X];


			for (int i = 1; i < features.Count; i++)
			{
				int nextFeaturePixelValue = Feature.TRANSPARENT_VALUE;
				Point nextFeatureOffsetToCheck = new Point(offset.X + features[i].HotspotX,
					offset.Y + features[i].HotspotY);

				if (Bitmap.OffsetIsInBounds(features[i].Feature.Bitmap, nextFeatureOffsetToCheck))
					nextFeaturePixelValue = features[i].Feature.Bitmap[nextFeatureOffsetToCheck.Y, nextFeatureOffsetToCheck.X];

				if (pixelValue != nextFeaturePixelValue)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Returns a list of every possible offset that be checked given the hotspot mask.
		/// </summary>
		/// <param name="coords">The hotspot mask used to Get the offsets.</param>
		/// <returns> A list of every possible offset that be checked given the hotspot mask.</returns>
		private static List<Point> GetAllPossibleOffsetsFromHotspotCoords(System.Drawing.Rectangle coords)
		{
			List<Point> offsets = new List<Point>();
			for (int row = coords.Y; row < coords.Height + coords.Y; row++)
			{
				for (int col = coords.X; col < coords.Width + coords.Y; col++)
				{
					offsets.Add(new Point(col, row));
				}
			}
			return offsets;
		}

		/// <summary>
		/// Returns a mask where every feature is aligned by its hotspot. The X and Y coordinates
		/// represent the upper left most offset that can be checked relative to all of the feature's hotspots.
		/// Some features may not have a value in this spot, but at least one does. The width and height
		/// of the rectangle represent how many pixels can be used horizontally and vertically.
		/// </summary>
		/// <param name="features">The features used to build the mask.</param>
		/// <returns>A mask where every feature is aligned by its hotspot.</returns>
		private static System.Drawing.Rectangle GetFeatureHotspotCoordinates(List<FeatureWrapper> features)
		{
			int minX = 0;
			int minY = 0;
			int maxX = 0;
			int maxY = 0;
			foreach (FeatureWrapper wrapper in features)
			{
				if (-wrapper.HotspotX < minX)
					minX = -wrapper.HotspotX;
				if (-wrapper.HotspotY < minY)
					minY = -wrapper.HotspotY;
				if ((wrapper.Feature.Bitmap.Width - wrapper.HotspotX) > maxX)
					maxX = wrapper.Feature.Bitmap.Width - wrapper.HotspotX;
				if ((wrapper.Feature.Bitmap.Height - wrapper.HotspotY) > maxY)
					maxY = wrapper.Feature.Bitmap.Height - wrapper.HotspotY;
			}

			return new System.Drawing.Rectangle(minX, minY, (maxX - minX), (maxY - minY));
		}

		private static int GetFarthestRightHotspotX(IEnumerable<FeatureWrapper> features)
		{
			int max = -1;
			foreach (FeatureWrapper f in features)
			{
				if (f.HotspotX > max)
					max = f.HotspotX;
			}

			return max;
		}

		private static int GetFarthestDownHotspotY(IEnumerable<FeatureWrapper> features)
		{
			int max = -1;
			foreach (FeatureWrapper f in features)
			{
				if (f.HotspotY > max)
					max = f.HotspotY;
			}

			return max;
		}

		private static int GetFarthestLeftHotspotXFromItsRight(IEnumerable<FeatureWrapper> features)
		{
			int max = -1;
			foreach (FeatureWrapper f in features)
			{
				if ( (f.Feature.Bitmap.Width - 1) - f.HotspotX  > max)
					max = (f.Feature.Bitmap.Width - 1) - f.HotspotX;
			}

			return max;
		}

		private static int GetFarthestUpHotspotXFromItsBottom(IEnumerable<FeatureWrapper> features)
		{
			int max = -1;
			foreach (FeatureWrapper f in features)
			{
				if ( (f.Feature.Bitmap.Height - 1 - f.HotspotY) > max)
					max = (f.Feature.Bitmap.Height - 1 - f.HotspotY);
			}

			return max;
		}

		/// <summary>
		/// Sets the hotspot of each feature to be the pixel in that feature
		/// that is the least common out of every pixel in the whole corpus of features.
		/// </summary>
		/// <param name="features">The features that will be Assigned hotspots.</param>
		private static List<FeatureWrapper> AssignHotspotsByPixelFrequency(IEnumerable<Feature> features)
		{
			List<FeatureWrapper> withHotspots = new List<FeatureWrapper>();
			Dictionary<int, int> pixelFrequencies = GetPixelFrequencies(features);

			foreach (Feature feature in features)
			{
				Point hotspot = GetLeastCommonPixel(feature, pixelFrequencies);
				withHotspots.Add(new FeatureWrapper(hotspot, feature));
			}

			return withHotspots;
		}

		/// <summary>
		/// Returns the bitmap offset that points to the least occurring pixel in this feature
		/// as specified by the pixel frequencies. Transparent pixels are ignored.
		/// </summary>
		/// <param name="feature">The feature used to find the offset.</param>
		/// <param name="pixelFrequencies">The frequencies of each pixel.</param>
		/// <returns>The bitmap offset that points to the least occurring pixel in this feature
		/// as specified by the pixel frequencies.</returns>
		private static Point GetLeastCommonPixel(Feature feature, Dictionary<int, int> pixelFrequencies)
		{
			int freqency = int.MaxValue;
			Point leastCommon = null;
			for (int row = 0; row < feature.Bitmap.Height; row++)
			{
				for (int column = 0; column < feature.Bitmap.Width; column++)
				{
					if (feature.Bitmap[row, column] != Feature.TRANSPARENT_VALUE &&
						pixelFrequencies[feature.Bitmap[row, column]] < freqency)
					{
						leastCommon = new Point(column, row);
						freqency = pixelFrequencies[feature.Bitmap[row, column]];
					}
				}
			}

			return leastCommon;
		}

		/// <summary>
		/// Buckets each pixel value in the corpus of features by its frequency.
		/// </summary>
		/// <param name="features">The corpus of features.</param>
		/// <returns>Pixel values and their cooresponding frequencies.</returns>
		private static Dictionary<int, int> GetPixelFrequencies(IEnumerable<Feature> features)
		{
			Dictionary<int, int> pixelFrequencies = new Dictionary<int, int>();
			foreach (Feature feature in features)
			{
				for (int row = 0; row < feature.Bitmap.Height; row++)
				{
					for (int column = 0; column < feature.Bitmap.Width; column++)
					{
						if (pixelFrequencies.ContainsKey(feature.Bitmap[row, column])){
							int pixel = feature.Bitmap[row, column];
							int val =  pixelFrequencies[pixel];
							pixelFrequencies[pixel] = val + 1;
						}
						else
							pixelFrequencies.Add(feature.Bitmap[row, column], 1);
					}
				}
			}

			return pixelFrequencies;
		}



}

}
