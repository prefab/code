using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prefab;
using System.Collections;
using PrefabIdentificationLayers.Models;
using PrefabIdentificationLayers;

namespace PrefabIdentificationLayers.Models.NinePart
{
	internal class PartGetter : IPartGetter, IConstraintGetter
	{

		private const int c_maxCornerSize = 6;
		private const int c_minCornerSize = 2;


		public static readonly PartGetter Instance = new PartGetter();
		private PartGetter() {}

		#region Parts
		public Dictionary<string, Part> GetParts(IEnumerable<Bitmap> positives, IEnumerable<Bitmap> negatives)
		{
			Dictionary<string, Part> parts = new Dictionary<string, Part>();

			List<string> edgetypes = new List<string>() { "repeating" };
			ArrayList interiortypes = new ArrayList { "horizontal", "vertical", "single" };


			RegionParameters minhoriz = new RegionParameters(null, c_minCornerSize, c_minCornerSize, 1);
			RegionParameters maxhoriz = new RegionParameters(null, c_maxCornerSize, c_maxCornerSize, c_maxCornerSize);
			RegionParameters minvert = new RegionParameters(null, c_minCornerSize, c_minCornerSize, 1);
			RegionParameters maxvert = new RegionParameters(null, c_maxCornerSize, c_maxCornerSize, c_maxCornerSize);


			int smallestWidth = int.MaxValue;
			int smallestHeight = int.MaxValue;

			foreach (Bitmap example in positives)
			{
				if (example.Width < smallestWidth)
					smallestWidth = example.Width;
				if (example.Height < smallestHeight)
					smallestHeight = example.Height;
			}

			smallestHeight -= 2;
			smallestWidth -= 2;



			maxhoriz.Depth = (int)Math.Min(smallestHeight / 2, maxhoriz.Depth);
			maxhoriz.Start = (int)Math.Min(smallestWidth / 2, maxhoriz.Start);
			maxhoriz.End = (int)Math.Min(smallestWidth / 2, maxhoriz.End);
			maxvert.Depth = (int)Math.Min(smallestWidth / 2, maxvert.Depth);
			maxvert.Start = (int)Math.Min(smallestHeight / 2, maxvert.Start);
			maxvert.End = (int)Math.Min(smallestHeight / 2, maxvert.End);




			parts.Add("topleft", new Part(GetCornerValues(smallestWidth, smallestHeight)));
			parts.Add("topright", new Part(GetCornerValues(smallestWidth, smallestHeight)));
			parts.Add("bottomleft", new Part(GetCornerValues(smallestWidth, smallestHeight)));
			parts.Add("bottomright", new Part(GetCornerValues(smallestWidth, smallestHeight)));

			parts.Add("top", new Part(GetEdgeValues(edgetypes, minhoriz, maxhoriz)));
			parts.Add("bottom", new Part(GetEdgeValues(edgetypes, minhoriz, maxhoriz)));
			parts.Add("left", new Part(GetEdgeValues(edgetypes, minvert, maxvert)));
			parts.Add("right", new Part(GetEdgeValues(edgetypes, minvert, maxvert)));

			parts.Add("interior", new Part(interiortypes));

			return parts;
		}

		private ArrayList GetCornerValues(int smallestWidth, int smallestHeight)
		{
			Size max = new Size(c_maxCornerSize, c_maxCornerSize);
			Size min = new Size(c_minCornerSize, c_minCornerSize);

			GetMaxSize(max, smallestWidth, smallestHeight);

			ArrayList values = new ArrayList();
			for (int height = min.Height; height <= max.Height; height++)
			{
				for (int width = min.Width; width <= max.Width; width++)
				{
					object value = new Size(width, height);
					values.Add(value);
				}

			}
			return values;
		}

		private ArrayList GetEdgeValues(List<string> types, RegionParameters min, RegionParameters max)
		{
			ArrayList values = new ArrayList();
			foreach (string type in types)
			{
				for (int depth = min.Depth; depth <= max.Depth; depth++)
				{
					for (int left = min.Start; left <= max.Start; left++)
					{
						for (int right = min.End; right <= max.End; right++)
						{
							object value = new RegionParameters(type, left, right, depth);
							values.Add(value);
						}
					}
				}
			}

			return values;
		}

		private Size GetMaxSize(Size currSize, int smallestWidth, int smallestHeight)
		{
			int width = (int)Math.Min(smallestWidth / 2, currSize.Width);
			int height = (int)Math.Min(smallestHeight / 2, currSize.Height);
			return new Size(width, height);
		}


		#endregion

		#region Constraints


		public IEnumerable<Constraint> GetConstraints(Dictionary<string, Part> parts, IEnumerable<Bitmap> positives, IEnumerable<Bitmap> negatives)
		{
			List<Constraint> constraints = new List<Constraint>();

			Constraint topSymmetricToBottom = new Constraint(SameEdgeDepth, parts["top"], parts["bottom"]);
			constraints.Add(topSymmetricToBottom);

			Constraint leftSymmetricToRight = new Constraint(SameEdgeDepth, parts["left"], parts["right"]);
			constraints.Add(leftSymmetricToRight);

			Constraint topLeftAdjacentToTopRegion = new Constraint(RegionStartEqualsWidth, parts["topleft"], parts["top"]);
			constraints.Add(topLeftAdjacentToTopRegion);

			Constraint topRightAdjacentToTopRegion = new Constraint(RegionEndEqualsWidth, parts["top"], parts["topright"]);
			constraints.Add(topRightAdjacentToTopRegion);

			Constraint bottomLeftAdjacentToBottomRegion = new Constraint(RegionStartEqualsWidth,  parts["bottom"], parts["bottomleft"]);
			constraints.Add(bottomLeftAdjacentToBottomRegion);

			Constraint bottomRightAdjacentToBottomRegion = new Constraint(RegionEndEqualsWidth, parts["bottomright"], parts["bottom"]);
			constraints.Add(bottomRightAdjacentToBottomRegion);

			Constraint topLeftAdjacentToLeftRegion = new Constraint(RegionStartEqualsHeight, parts["left"], parts["bottomleft"]);
			constraints.Add(topLeftAdjacentToLeftRegion);

			Constraint bottomLeftAdjacentToLeftRegion = new Constraint(RegionEndEqualsHeight, parts["bottomleft"], parts["left"]);
			constraints.Add(bottomLeftAdjacentToLeftRegion);

			Constraint topRightAdjacentToRightRegion = new Constraint(RegionStartEqualsHeight, parts["topright"], parts["right"]);
			constraints.Add(topRightAdjacentToRightRegion);

			Constraint bottomRightAdjacentToRightRegion = new Constraint(RegionEndEqualsHeight, parts["right"], parts["bottomright"]);
			constraints.Add(bottomRightAdjacentToRightRegion);

			Constraint topLeftIsSquare = new Constraint(FeatureIsSquare, parts["topleft"], parts["topleft"]);
			constraints.Add(topLeftIsSquare);

			Constraint topRightIsSquare = new Constraint(FeatureIsSquare, parts["topright"], parts["topright"]);
			constraints.Add(topRightIsSquare);

			Constraint bottomRightIsSquare = new Constraint(FeatureIsSquare, parts["bottomright"], parts["bottomright"]);
			constraints.Add(bottomRightIsSquare);

			Constraint bottomLeftIsSquare = new Constraint(FeatureIsSquare, parts["bottomleft"], parts["bottomleft"]);
			constraints.Add(bottomLeftIsSquare);

			Constraint topLeftTopRightSameSize = new Constraint(PartsAreSameSizeOrZero, parts["topleft"], parts["topright"]);
			constraints.Add(topLeftTopRightSameSize);

			Constraint topRightBottomRightSameSize = new Constraint(PartsAreSameSizeOrZero, parts["topright"], parts["bottomright"]);
			constraints.Add(topRightBottomRightSameSize);

			Constraint bottomRightBottomLeftSameSize = new Constraint(PartsAreSameSizeOrZero, parts["bottomright"], parts["bottomleft"]);
			constraints.Add(bottomRightBottomLeftSameSize);

			Constraint bottomLeftTopLeftSameSize = new Constraint(PartsAreSameSizeOrZero, parts["bottomleft"], parts["topleft"]);
			constraints.Add(bottomLeftTopLeftSameSize);

			Constraint topIsLessThanOrEqualToHeight = new Constraint(DepthIsSmallHeight, parts["topleft"], parts["top"]);
			constraints.Add(topIsLessThanOrEqualToHeight);

			Constraint bottomIsLessThanOrEqualToHeight = new Constraint(DepthIsSmallHeight, parts["bottomleft"], parts["bottom"]);
			constraints.Add(bottomIsLessThanOrEqualToHeight);

			Constraint leftIsLessThanOrEqualToWidth = new Constraint(DepthIsSmallWidth, parts["left"], parts["topleft"]);
			constraints.Add(leftIsLessThanOrEqualToWidth);

			Constraint rightIsLessThanOrEqualToWidth = new Constraint(DepthIsSmallWidth, parts["right"], parts["topright"]);
			constraints.Add(rightIsLessThanOrEqualToWidth);

			Constraint topStartEqualsEnd = new Constraint(RegionStartEqualsEnd, parts["top"], parts["top"]);
			constraints.Add(topStartEqualsEnd);

			Constraint bottomStartEqualsEnd = new Constraint(RegionStartEqualsEnd, parts["bottom"], parts["bottom"]);
			constraints.Add(bottomStartEqualsEnd);

			Constraint leftStartEqualsEnd = new Constraint(RegionStartEqualsEnd, parts["left"], parts["left"]);
			constraints.Add(leftStartEqualsEnd);

			Constraint rightStartEqualsEnd = new Constraint(RegionStartEqualsEnd, parts["right"], parts["right"]);
			constraints.Add(rightStartEqualsEnd);

			return constraints;
		}

		public static Tuple<Size, RegionParameters> GetFromValues(object v1, object v2)
		{
			Size size = v1 as Size;
			RegionParameters parameters = v2 as RegionParameters;
			if (size == null)
			{
				size = v2 as Size;
				parameters = v1 as RegionParameters;
			}

			return new Tuple<Size, RegionParameters>(size, parameters);
		}

		private static Tuple<Utils.RectData, Size> GetFomValuesContentEdge(object v1, object v2)
		{
			Utils.RectData content = v1 as Utils.RectData;
			Size feature = v2 as Size;

			if (content == null)
			{
				content = v2 as Utils.RectData;
				feature = v1 as Size;
			}


			return new Tuple<Utils.RectData, Size>(content, feature);
		}

		public static bool DepthIsSmallHeight(object v1, object v2)
		{
			Tuple<Size, RegionParameters> pair = GetFromValues(v1, v2);
			Size size = pair.Item1;
			RegionParameters parameters = pair.Item2;

			return size.Height == 0 || parameters.Depth <= size.Height;
		}

		public static bool SameEdgeDepth(object v1, object v2)
		{
			return ((RegionParameters)v1).Depth == ((RegionParameters)v2).Depth;
		}

		public static bool DepthIsSmallWidth(object v1, object v2)
		{
			Tuple<Size, RegionParameters> pair = GetFromValues(v1, v2);
			Size size = pair.Item1;
			RegionParameters parameters = pair.Item2;

			return size.Width == 0 || parameters.Depth <= size.Width;
		}

		public static bool PartsAreSameSizeOrZero(object v1, object v2)
		{
			Size s1 = (Size)v1;
			Size s2 = (Size)v2;

			if (s1.Height == 0 || s2.Height == 0 || s1.Width == 0 || s2.Width == 0)
				return true;

			return s1.Equals(s2);
		}

		public static bool RegionStartEqualsEnd(object v1, object v2)
		{
			RegionParameters r1 = (RegionParameters)v1;

			return r1.Start == 0 || r1.End == 0 || r1.Start == r1.End;
		}

		public static bool RegionStartEqualsWidth(object v1, object v2)
		{
			Tuple<Size, RegionParameters> pair = GetFromValues(v1, v2);
			Size size = pair.Item1;
			RegionParameters parameters = pair.Item2;

			return parameters.Start == size.Width;
		}

		public static bool RegionEndEqualsWidth(object v1, object v2)
		{
			Tuple<Size, RegionParameters> pair = GetFromValues(v1, v2);
			Size size = pair.Item1;
			RegionParameters parameters = pair.Item2;

			return parameters.End == size.Width;
		}

		public static bool RegionStartEqualsHeight(object v1, object v2)
		{
			Tuple<Size, RegionParameters> pair = GetFromValues(v1, v2);
			Size rect = (Size)pair.Item1;
			RegionParameters parameters = pair.Item2;

			return rect.Height == parameters.Start;
		}

		public static bool RegionEndEqualsHeight(object v1, object v2)
		{
			Tuple<Size, RegionParameters> pair = GetFromValues(v1, v2);
			Size size = pair.Item1;
			RegionParameters parameters = pair.Item2;

			return size.Height == parameters.End;
		}

		public static bool RegionDepthEqualsHeight(object v1, object v2)
		{
			Tuple<Size, RegionParameters> pair = GetFromValues(v1, v2);
			Size size = pair.Item1;
			RegionParameters parameters = pair.Item2;

			return size.Height == parameters.Depth;

		}

		public static bool RegionDepthEqualsWidth(object v1, object v2)
		{
			Tuple<Size, RegionParameters> pair = GetFromValues(v1, v2);
			Size size = pair.Item1;
			RegionParameters parameters = pair.Item2;

			return size.Width == parameters.Depth;
		}

		public static bool RegionDepthLessThanHeight(object v1, object v2)
		{
			Tuple<Size, RegionParameters> pair = GetFromValues(v1, v2);
			Size size = pair.Item1;
			RegionParameters parameters = pair.Item2;

			return parameters.Depth < size.Height;
		}

		public static bool RegionDepthLessThanWidth(object v1, object v2)
		{
			Tuple<Size, RegionParameters> pair = GetFromValues(v1, v2);
			Size size = pair.Item1;
			RegionParameters parameters = pair.Item2;

			return parameters.Depth < size.Width;
		}

		public static bool FeatureIsSquare(object v1, object v2)
		{
			Size size = (Size)v1;
			return size.Width == size.Height;
		}

		private static bool ContentLeftAligned(object v1, object v2)
		{
			Tuple<Utils.RectData, Size> pair = GetFomValuesContentEdge(v1, v2);
			Utils.RectData contentParam = pair.Item1;
			Size featureParam = pair.Item2;

			return featureParam.Width == contentParam.Left;
		}

		private bool ContentTopAligned(object v1, object v2)
		{
			Tuple<Utils.RectData, Size> pair = GetFomValuesContentEdge(v1, v2);
			Utils.RectData contentParam = pair.Item1;
			Size featureParam = pair.Item2;

			return featureParam.Height == contentParam.Top;
		}

		private bool ContentBottomAligned(object v1, object v2)
		{
			Tuple<Utils.RectData, Size> pair = GetFomValuesContentEdge(v1, v2);
			Utils.RectData contentParam = pair.Item1;
			Size featureParam = pair.Item2;

			return featureParam.Height == contentParam.Bottom;
		}

		private bool ContentRightAligned(object v1, object v2)
		{
			Tuple<Utils.RectData, Size> pair = GetFomValuesContentEdge(v1, v2);
			Utils.RectData contentParam = pair.Item1;
			Size featureParam = pair.Item2;

			return featureParam.Width == contentParam.Right;
		}

		#endregion
	}
}