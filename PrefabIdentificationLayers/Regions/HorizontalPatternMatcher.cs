using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PrefabIdentificationLayers.Models.NinePart;
using PrefabIdentificationLayers.Models;
using Prefab;

namespace PrefabIdentificationLayers.Regions
{
    public class HorizontalPatternMatcher : IRegionMatchStrategy
    {

        private HorizontalPatternMatcher() { }
        public string Name
        {
            get { return "horizontal"; }
        }

        /// <summary>
        /// Adds a bitmap to extend the pattern on the left hand side. This makes it easy to build the shortest
        /// repeating pattern.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="toConcat"></param>
        /// <returns></returns>
        public static Bitmap Append(Bitmap pattern, Bitmap toConcat)
        {
            Bitmap newPat;
            if (pattern == null)
            {
                newPat = Bitmap.DeepCopy(toConcat);
            }
            else if (toConcat == null)
            {
                newPat = Bitmap.DeepCopy(pattern);
            }
            else if (Bitmap.ExactlyMatches(pattern, toConcat))
            {
                newPat = Bitmap.DeepCopy(pattern);
            }
            else
            {
				newPat = Bitmap.FromDimensions(pattern.Width + toConcat.Width, pattern.Height);

                for (int row = 0; row < toConcat.Height; row++)
                {
                    for (int col = 0; col < pattern.Width; col++)
                    {
						((Bitmap)newPat)[row, col] = pattern[row, col];
                    }
                    for (int col = pattern.Width; col < newPat.Width; col++)
                    {
						((Bitmap)newPat)[row,col] = toConcat[row, col - pattern.Width];
                    }
                }
            }

            return newPat;
        }

        /// <summary>
        /// Returns true if the bitmap matches the pattern in the bitmap.
        /// </summary>
        /// <param name="bitmap">The bitmap to try to match</param>
        /// <param name="startRow">The starting row in the bitmap.</param>
        /// <param name="start">The starting column in the bitmap.</param>
        /// <param name="end">The ending column in the bitmap.</param>
        /// <returns></returns>
        public bool Matches(Bitmap pattern, Bitmap bitmap, int startRow, int start, int end)
        {

            for (int column = start; column <= end; column++)
            {
                if (!ColumnMatches(bitmap, pattern, startRow, startRow + pattern.Height - 1, 0, column, (column - start) % pattern.Width))
                    return false;
            }

            return true;
        }


        /// <summary>
        /// Returns the index of the column where the pattern no longer matches the given bitmap.
        /// </summary>
        /// <param name="bitmap">The bitmap to match.</param>
        /// <param name="startRow">The starting row in the bitmap.</param>
        /// <param name="start">The starting column in the bitmap.</param>
        /// <param name="end">The ending column in the bitmap.</param>
        /// <returns></returns>
        public int MatchesHorizontalUntil(Bitmap pattern, Bitmap bitmap, int startRow, int start, int end)
        {
            for (int column = start; column <= end; column++)
            {
                if (!ColumnMatches(bitmap, pattern, startRow, startRow + pattern.Height - 1, 0, column, (column - start) % pattern.Width))
                    return column;
            }

            return end + 1;
        }

        /// <summary>
        /// Returns true if the column in the bitmap matches the column in the pattern.
        /// </summary>
        /// <param name="bitmap">The bitmap containing the column to match.</param>
        /// <param name="startRow">The starting row in the bitmap.</param>
        /// <param name="column">The index of the column to match in the bitmap.</param>
        /// <param name="columnInPattern">The index of the column in the pattern.</param>
        /// <returns></returns>
        public bool ColumnMatches(Bitmap pattern, Bitmap bitmap, int startRow, int column, int columnInPattern)
        {
            return ColumnMatches(bitmap, pattern, startRow, startRow + pattern.Height - 1, 0, column, columnInPattern);
        }

        /// <summary>
        /// Returns true if the column in the bitmap matches the column in the pattern.
        /// </summary>
        /// <param name="bitmap">The bitmap containing a column to match.</param>
        /// <param name="pattern">A pattern containing a column to match.</param>
        /// <param name="startRow">The starting row in the bitmap.</param>
        /// <param name="endRow">The ending row in the bitmap.</param>
        /// <param name="startRowInPattern">The starting row in the pattern.</param>
        /// <param name="columnInBitmap">The index of the column in the bitmap.</param>
        /// <param name="columnInPattern">The index of the column in the pattern.</param>
        /// <returns></returns>
        private static bool ColumnMatches(Bitmap bitmap, Bitmap pattern, int startRow, int endRow, int startRowInPattern, int columnInBitmap, int columnInPattern)
        {
            for (int row = startRow, rowInPattern = startRowInPattern; row <= endRow; row++, rowInPattern++)
            {
				if (bitmap[row, columnInBitmap] != pattern[rowInPattern, columnInPattern])
                    return false;
            }

            return true;
        }

        public static int Missed(Bitmap pattern, Bitmap bitmap, BackgroundValue bp)
        {
            int min = int.MaxValue;
            int missed = 0;
            int left = Math.Max(bp.TopLeft.Width, bp.BottomLeft.Width);
            int right = bitmap.Width - Math.Max(bp.TopRight.Width, bp.BottomRight.Width);
            for (int row = bp.Top; row < bitmap.Height - bp.Bottom; row++)
            {
                for (int col = left; col < right; col++)
                {
					if (pattern[(row - bp.Top) % pattern.Height, (col - bp.Left) % pattern.Width] != bitmap[row, col])
                        missed++;

                }
            }

            for (int row = bp.TopLeft.Height; row < bitmap.Height - bp.BottomLeft.Height; row++)
            {
                for (int col = bp.Left; col < left; col++)
                {
					if (pattern[(row - bp.Top) % pattern.Height, (col - bp.Left) % pattern.Width] != bitmap[row, col])
                        missed++;
                }
            }


            for (int row = bp.TopRight.Height; row < bitmap.Height - bp.BottomRight.Height; row++)
            {
                for (int col = right - 1; col < bitmap.Width - bp.Right; col++)
                {
					if (pattern[(row - bp.Top) % pattern.Height , (col - bp.Left) % pattern.Width] != bitmap[row, col])
                        missed++;
                }
            }

            if (missed < min)
                min = missed;

            return min;
        }



        /// <summary>
        /// Extracts the shortest repeating pattern from the given bitmap.
        /// </summary>
        /// <param name="toExtract">The bitmap containing the pattern.</param>
        /// <param name="startRow">The index of the starting row to begin extracting</param>
        /// <param name="start">The index of the starting column to begin extracting</param>
        /// <param name="end">The index of the ending column to extract.</param>
        /// <param name="height">The height of the pattern to extract.</param>
        /// <returns></returns>
        public static Bitmap ShortestPattern(Bitmap toExtract, int startRow, int start, int end, int height)
        {
            int patternLength = 1;
            for (int col = start + 1; col <= end; col++)
            {
                if (!ColumnMatches(toExtract, toExtract, startRow, startRow + height - 1, startRow, col, ((col - start) % patternLength) + start))
                    patternLength = col - start + 1;
            }

			Bitmap bmp = (Bitmap)Bitmap.Crop(toExtract, start, startRow,  patternLength, height);

            return bmp;
        }

        public static HorizontalPatternMatcher Instance
        {
            get;
            private set;
        }

        static HorizontalPatternMatcher()
        {
            Instance = new HorizontalPatternMatcher();
        }
    }
}
