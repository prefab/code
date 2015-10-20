using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PrefabIdentificationLayers.Models.NinePart;
using PrefabIdentificationLayers.Models;
using Prefab;

namespace PrefabIdentificationLayers.Regions
{
    public class VerticalPatternMatcher : IRegionMatchStrategy
    {


        static VerticalPatternMatcher()
        {
            Instance = new VerticalPatternMatcher();
        }
        public static VerticalPatternMatcher Instance
        {
            get;
            private set;
        }
        /// <summary>
        /// Returns true if the pattern matches the bitmap.
        /// </summary>
        /// <param name="bitmap">The bitmap to check against.</param>
        /// <param name="startCol">The starting column in the bitmap where the pattern will be checked.</param>
        /// <param name="start">The starting row in the bitmap where the pattern will be checked.</param>
        /// <param name="end">The ending column in the bitmap where the pattern will be checked.</param>
        /// <returns></returns>
        public  bool Matches(Bitmap pattern, Bitmap bitmap, int startCol, int start, int end)
        {
            for (int row = start; row <= end; row++)
            {
                if (!RowMatches(bitmap, pattern, startCol, startCol + pattern.Width - 1, 0, row, (row - start) % pattern.Height))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the location where the pattern no longer matches the bitmap.
        /// </summary>
        /// <param name="bitmap">The bitmap to match against.</param>
        /// <param name="startCol">The starting column to match in the bitmap.</param>
        /// <param name="start">The starting row in the bitmap to match.</param>
        /// <param name="end">The ending row to match in the bitmap.</param>
        /// <returns>The index of the row where the pattern no longer matches.</returns>
        public  int MatchesVerticalUntil(Bitmap pattern, Bitmap bitmap, int startCol, int start, int end)
        {
            for (int row = start; row <= end; row++)
            {
                if (!RowMatches(bitmap, pattern, startCol, startCol + pattern.Width - 1, 0, row, (row - start) % pattern.Height))
                    return row;
            }

            return end + 1;
        }

        /// <summary>
        /// Returns true if the given pattern matches the row specified by the parameters.
        /// </summary>
        /// <param name="bitmap">The bimap to match against.</param>
        /// <param name="pattern">The pattern to match in the bitmap.</param>
        /// <param name="startCol">The start column in the bitmap.</param>
        /// <param name="endCol">The end column in the bitmap.</param>
        /// <param name="startColInPattern">The start column in the pattern.</param>
        /// <param name="rowInBitmap">The start row in the bitmap.</param>
        /// <param name="rowInPattern">The start row in the pattern.</param>
        /// <returns></returns>
        private static bool RowMatches(Bitmap bitmap, Bitmap pattern, int startCol, int endCol, int startColInPattern, int rowInBitmap, int rowInPattern)
        {
            for (int col = startCol, colInPattern = startColInPattern; col <= endCol; col++, colInPattern++)
            {
				if (bitmap[rowInBitmap, col] != pattern[rowInPattern, colInPattern])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the shortest vertical repeating pattern that can be extracted.
        /// </summary>
        /// <param name="toExtract">The source bitmap of the pattern to extract.</param>
        /// <param name="startCol">The starting column to begin extracting.</param>
        /// <param name="start">The starting row to begin extrcting.</param>
        /// <param name="end">The last row to extract.</param>
        /// <param name="width">The thickness of the pattern to extract.</param>
        /// <returns></returns>
        public static Bitmap ShortestPattern(Bitmap toExtract, int startCol, int start, int end, int width)
        {
            int patternlength = 1;

            for (int row = start; row <= end; row++)
            {
                if (!RowMatches(toExtract, toExtract, startCol, startCol + width - 1, startCol, row, ((row - start) % patternlength) + start))
                    patternlength = row - start + 1;
            }

			Bitmap bmp = Bitmap.Crop(toExtract, startCol, start, width, patternlength );
            return bmp;
        }

        public static int Missed(Bitmap pattern, Bitmap bitmap, BackgroundValue bp)
        {
            int min = int.MaxValue;
            int missed = 0;
            int left = bp.Left;
            int right = bitmap.Width - bp.Right;
            int top = Math.Max(bp.TopLeft.Height, bp.TopRight.Height);
            int bottom = Math.Max(bp.BottomLeft.Height, bp.BottomRight.Height);

            //Center
            for (int row = top; row < bitmap.Height - bottom; row++)
            {
                for (int col = left; col < right; col++)
                {
					if (pattern[(row - bp.Top) % pattern.Height, (col - bp.Left) % pattern.Width] != bitmap[row, col])
                        missed++;
                }
            }

            //Top
            for (int row = bp.Top; row < top; row++)
            {
                for (int col = bp.TopLeft.Width; col < bitmap.Width - bp.TopRight.Width; col++)
                {
					if (pattern[ (row - bp.Top) % pattern.Height, (col - bp.Left) % pattern.Width] != bitmap[row, col])
                        missed++;
                }
            }

            //Bottom
            for (int row = bitmap.Height - bottom; row < bitmap.Height - bp.Bottom; row++)
            {
                for (int col = bp.BottomLeft.Width; col < bitmap.Width - bp.BottomRight.Width; col++)
                {
					if (pattern[(row - bp.Top) % pattern.Height, (col - bp.Left) % pattern.Width] != bitmap[ row , col])
                        missed++;
                }
            }

            if (missed < min)
                min = missed;

            return min;
        }


        public string Name
        {
            get { return "vertical"; }
        }
    }
}
