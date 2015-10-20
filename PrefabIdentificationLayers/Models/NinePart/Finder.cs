using System;
using Prefab;
using PrefabIdentificationLayers.Regions;
using PrefabIdentificationLayers.Features;
using System.Collections.Generic;
using PrefabIdentificationLayers.Prototypes;
using System.Threading.Tasks;

namespace PrefabIdentificationLayers.Models.NinePart
{
	public class Finder : PtypeFinder
	{

		private Finder() { }
		public static readonly Finder Instance = new Finder();



		/// <summary>
		/// Finds any occurrences given the found features.
		/// </summary>
		/// <param name="features">Feature occurrences that correspond to this model.</param>
		/// <param name="bitmap">Bitmap containing the feature occurrences.</param>
		/// <returns>A list of hypotheses</returns>
		
		public void FindOccurrences(Ptype ptype, Bitmap bitmap, IEnumerable<Tree> features, List<Tree> found)
		{

			Feature bottomleftFeature = ptype.Feature("bottomleft");
			Feature toprightFeature = ptype.Feature("topright");
			Feature bottomrightFeature = ptype.Feature("bottomright");
			Feature topleftFeature = ptype.Feature("topleft");

			Region topregion = ptype.Region("top");
			Region leftregion = ptype.Region("left");
			Region bottomregion = ptype.Region("bottom");
			Region rightregion = ptype.Region("right");
			Region interior = ptype.Region("interior");


			//foreach each corresponding bottom left feature, find the corresponding bottom right and top right features
			foreach (Tree bottomleft in features)
			{
				if(bottomleft["feature"].Equals(bottomleftFeature)){

					//Find each bottom right feature corresponding to the bottom left feature
					IEnumerable<Tree> bottomrights = GetBottomRightsFromBottomLeft(bottomrightFeature, bottomleft, features);

					//foreach each bottom right feature, get the corresponding top right features
					foreach (Tree bottomright in bottomrights)
					{

						//Get the top right feature corresponding to this bottom right feature
						IEnumerable<Tree> toprights = GetTopRightsFromBottomRight(toprightFeature, bottomright, features);

						foreach (Tree topright in toprights)
						{
							Tree topleft = GetTopLeft(topleftFeature, topright, bottomleft, features);

							//Validate the hypothesis by matching edges. If they match, then create occurrence.
							if (topleft != null &&
								InteriorFits(interior, topregion, bottomregion, leftregion, rightregion, topleft, bottomleft, topright, bottomright) &&
								EdgesMatch(ptype, bitmap, topleft, topright, bottomleft, bottomright))
							{
								int top = topleft.Top;
								int left = topleft.Left;
								int height = bottomleft.Top + bottomleft.Height - topleft.Top;
								int width = topright.Left + topright.Width - topleft.Left;

								BoundingBox bb = new BoundingBox(left, top, width, height);
								Dictionary<String,Object> dict = new Dictionary<String,Object>();
								dict.Add("type", "ptype");
								dict.Add("ptype", ptype);
                                dict.Add("ptype_id", ptype.Id);
								Tree prototypeOccurrence = Tree.FromBoundingBox(bb, dict);
								found.Add(prototypeOccurrence);
							}
						}
					}
				}
			}
		}



		private bool InteriorFits(Region interior, Region top, Region bottom, Region left,
			Region right, IBoundingBox topleft, IBoundingBox bottomleft, IBoundingBox topright, IBoundingBox bottomright)
		{

			if (interior == null)
				return true;

			if (interior.Bitmap.Width == 1 && interior.Bitmap.Height == 1 && interior.MatchStrategy.Equals("horizontal"))
				return true;

			if (interior.MatchStrategy.Equals("horizontal"))
				return interior.Bitmap.Height == bottomleft.Top + bottomleft.Height - topleft.Top - top.Bitmap.Height - bottom.Bitmap.Height;

			if (interior.MatchStrategy.Equals("vertical"))
				return interior.Bitmap.Width == topright.Left + topright.Width - topleft.Left - left.Bitmap.Width - right.Bitmap.Width;

			return false;
		}


		private Tree GetCorrespondingBottomLeft(Feature bottomleft, IBoundingBox bottomright, IBoundingBox topleft, IEnumerable<Tree> features)
		{
			foreach(Tree f in features){
				if(bottomleft.Equals(f["feature"]) && IsAbove(topleft, f) && IsHorizontallyAligned(f, bottomright) && IsVerticallyAligned(f, topleft)){
					return f;
				}
			}
			return null;
		}

		private Tree GetTopLeft(Feature topleft, IBoundingBox topright, IBoundingBox bottomleft, IEnumerable<Tree> features)
		{
			foreach(Tree f in features){
				if(topleft.Equals(f["feature"]) && IsAbove(f, bottomleft) && IsHorizontallyAligned(f, topright) && IsVerticallyAligned(bottomleft, f))
					return f;
			}

			return null;
		}

		private IEnumerable<Tree> GetCorrespondingBottomRights(Feature bottomright, IBoundingBox topright, IEnumerable<Tree> features)
		{
			List<Tree> brs = new List<Tree>();
			foreach(Tree f in features){
				if(bottomright.Equals(f["feature"]) && IsAbove(topright, f) && IsVerticallyAligned(f, topright)){
					brs.Add(f);
				}
			}
			return brs;

		}

		private IEnumerable<Tree> GetTopRightsFromBottomRight(Feature topright, IBoundingBox bottomright, IEnumerable<Tree> features)
		{
			List<Tree> brs = new List<Tree>();
			foreach(Tree f in features){
				if(topright.Equals(f["feature"]) && IsAbove(f, bottomright) && IsVerticallyAligned(bottomright, f)){
					brs.Add(f);
				}
			}
			return brs;


		}

		private IEnumerable<Tree> GetBottomRightsFromBottomLeft(Feature bottomright, IBoundingBox bottomleft, IEnumerable<Tree> features)
		{

			List<Tree> brs = new List<Tree>();
			foreach(Tree f in features){
				if(bottomright.Equals(f["feature"]) &&
					IsLeftOf(bottomleft, f) &&
					IsHorizontallyAligned(f, bottomleft)){
					brs.Add(f);
				}
			}
			return brs;
		}

		public static bool IsAbove(IBoundingBox feature1, IBoundingBox feature2)
		{
			return feature1.Top < feature2.Top;
		}

		public static bool IsVerticallyAligned(IBoundingBox feature1, IBoundingBox feature2)
		{
			return feature1.Left == feature2.Left;
		}

		private IEnumerable<Tree> GetCorrespondingTopRights(Feature topright, IBoundingBox topleft, IEnumerable<Tree> features)
		{

			List<Tree> brs = new List<Tree>();
			foreach(Tree f in features){
				if(topright.Equals(f["feature"]) && IsLeftOf(topleft, f) && IsHorizontallyAligned(f, topleft)){
					brs.Add(f);
				}
			}
			return brs;
		}

		private IEnumerable<Tree> GetCorrespondingTopLefts(Feature topleft, IBoundingBox topright, IEnumerable<Tree> features)
		{
			List<Tree> brs = new List<Tree>();
			foreach(Tree f in features){
				if(topleft.Equals(f["feature"]) && IsLeftOf(f, topright) && IsHorizontallyAligned(topright, f)){
					brs.Add(f);
				}
			}
			return brs;
		}

		public static bool IsLeftOf(IBoundingBox feature1, IBoundingBox feature2)
		{
			return feature1.Left < feature2.Left;
		}

		public static bool IsHorizontallyAligned(IBoundingBox feature1, IBoundingBox feature2)
		{
			return feature1.Top == feature2.Top;
		}


		/// <summary>
		/// Returns true if every edge matches the given hypothesis.
		/// </summary>
		/// <param name="features">The features found in the bitmap.</param>
		/// <param name="bitmap">The bitmap containing the hypothesis.</param>
		/// <returns>Returns true if every edge matches the given hypothesis.</returns>
		private bool EdgesMatch(Ptype ptype, Bitmap bitmap, IBoundingBox topleft, IBoundingBox topright, IBoundingBox bottomleft, IBoundingBox bottomright)
		{
			Region right = ptype.Region("right");
			Region top = ptype.Region("top");
			Region bottom = ptype.Region("bottom");
			Region left = ptype.Region("left");

			return HorizontalPatternMatcher.Instance.Matches(top.Bitmap, bitmap, topleft.Top, topleft.Left + topleft.Width, topright.Left - 1)
				&& HorizontalPatternMatcher.Instance.Matches(bottom.Bitmap, bitmap, bottomleft.Top + bottomleft.Height - bottom.Bitmap.Height,
					bottomleft.Left + bottomleft.Width, bottomright.Left - 1)
				&& VerticalPatternMatcher.Instance.Matches(left.Bitmap, bitmap, topleft.Left, topleft.Top + topleft.Height,
					bottomleft.Top - 1)
				&& VerticalPatternMatcher.Instance.Matches(right.Bitmap, bitmap, topright.Left + topright.Width - right.Bitmap.Width, topright.Top + topright.Height,
					bottomright.Top - 1);

		}

		public void SetForeground(Tree node, Bitmap image, Bitmap foreground) {

			Ptype ptype = (Ptype)node["ptype"];
			Region interiorregion = ptype.Region("interior");

			foreground.WriteBlock(Utils.BACKGROUND, node);

			if (interiorregion != null)
			{
				IBoundingBox interior = GetInteriorBox(node, ptype);

				int top = interior.Top;
				int left = interior.Left;
				int leftdepth = ptype.Region("left").Bitmap.Width;

				int bottom = interior.Top + interior.Height;
				int right = interior.Left + interior.Width;

				for(int row = top; row < bottom; row++)
				{
					for (int col = left; col < right; col++)
					{
						int backgroundValue = interiorregion.Bitmap[(row - top) % interiorregion.Bitmap.Height,
							(col - node.Left - leftdepth) % interiorregion.Bitmap.Width];

						if (backgroundValue != image[row, col])
							foreground[row, col] = node.GetHashCode();
					}
				}

				//We looked at the features and might have counted them as foreground.
				EraseFeaturesFromforeground(foreground, node, ptype);
			}
		}

        public IBoundingBox WriteBackgroundOver(Tree node, IBoundingBox regionToFill, 
            Bitmap destination, int destTop, int destLeft)
        {
            Ptype ptype = (Ptype)node["ptype"];
            Region interiorregion = ptype.Region("interior");

            if (interiorregion != null)
            {
                IBoundingBox interior = GetInteriorBox(node, ptype);

                int top = interior.Top;
                int left = interior.Left;

                int bottom = interior.Top + interior.Height;
                int right = interior.Left + interior.Width;


                int startRow = Math.Max(regionToFill.Top, top);
                int startCol = Math.Max(regionToFill.Left, left);
                int endRow = Math.Min(regionToFill.Top + regionToFill.Height, bottom);
                int endCol = Math.Min(regionToFill.Left + regionToFill.Width, right);


                if (startRow > regionToFill.Top)
                    destTop += startRow - regionToFill.Top;

                if (startCol > regionToFill.Left)
                    destLeft += startCol - regionToFill.Left;

                int interiorheight = interiorregion.Bitmap.Height;
                int interiorwidth = interiorregion.Bitmap.Width;
                
                Parallel.For(startRow, endRow, delegate(int row)
                {
                    int bkgTop = destTop + row - startRow;
                    for (int col = startCol, bkgLeft = destLeft; col < endCol; col++, bkgLeft++)
                    {
                        destination[bkgTop, bkgLeft] = 
                            interiorregion.Bitmap[(row - top) % interiorheight, (col - left) % interiorwidth]; //image[/*top +*/ row/*(row - startRow)*/, left];//Interior[row - top, col - left];
                    }
                });

                return regionToFill;
            }

            return null;
        }


		private static void EraseFeaturesFromforeground(Bitmap foreground, Tree occurrence, Ptype ptype)
		{
			//top left
			Feature topleft = ptype.Feature("topleft");
			for(int row = occurrence.Top; row < occurrence.Top + topleft.Bitmap.Height; row++)
			{
				for(int col = occurrence.Left; col < occurrence.Left + topleft.Bitmap.Width; col++)
				{
					foreground[row, col] =  Utils.BACKGROUND;
				}
			}

			//top right
			Feature topright = ptype.Feature("topright");
			for (int row = occurrence.Top; row < occurrence.Top + topleft.Bitmap.Height; row++)
			{
				for(int col = occurrence.Left + occurrence.Width - topright.Bitmap.Width; col < occurrence.Left + occurrence.Width; col++)
				{
					foreground[row, col] =  Utils.BACKGROUND;
				}
			}

			//bottom left
			Feature bottomleft = ptype.Feature("bottomleft");
			for (int row = occurrence.Top + occurrence.Height - bottomleft.Bitmap.Height; row < occurrence.Top + occurrence.Height; row++)
			{
				for (int col = occurrence.Left; col < occurrence.Left + bottomleft.Bitmap.Width; col++)
				{
					foreground[row, col] = Utils.BACKGROUND;
				}
			}

			//bottom right
			Feature bottomright = ptype.Feature("bottomright");
			for (int row = occurrence.Top + occurrence.Height - bottomright.Bitmap.Height; row < occurrence.Top + occurrence.Height; row++)
			{
				for (int col = occurrence.Left + occurrence.Width - bottomright.Bitmap.Width; col < occurrence.Left + occurrence.Width; col++)
				{
					foreground[row, col] = Utils.BACKGROUND;
				}
			}
		}

		private IBoundingBox GetInteriorBox(IBoundingBox toProcess, Ptype ptype)
		{

			Region topregion = ptype.Region("top");
			Region bottomregion = ptype.Region("bottom");
			Region leftregion = ptype.Region("left");
			Region rightregion = ptype.Region("right");

			int top = toProcess.Top + topregion.Bitmap.Height;
			int left = toProcess.Left + leftregion.Bitmap.Width;
			int height = toProcess.Height - topregion.Bitmap.Height - bottomregion.Bitmap.Height;
			int width = toProcess.Width - leftregion.Bitmap.Width - rightregion.Bitmap.Width;

			return new BoundingBox(left, top, width, height);
		}

		public void FindContent(Tree occurrence, Bitmap image, Bitmap foreground, List<Tree> found)
		{
			Ptype ptype = (Ptype)occurrence["ptype"];
			Region interiorregion = ptype.Region("interior");
			if (interiorregion != null)
			{
				ContentFinder.Instance.FindContent(image, foreground, occurrence, found);
			}
		}


	}
}

