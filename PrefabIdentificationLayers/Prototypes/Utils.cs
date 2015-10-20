using System;
using Prefab;
using PrefabIdentificationLayers.Features;
using System.Collections.Generic;

namespace PrefabIdentificationLayers
{
	public static class Utils
	{
		public static readonly int DEFAULT_BUFFER_WIDTH = 2000;
		public static readonly int DEFAULT_BUFFER_HEIGHT = 2000;
		public static readonly int BACKGROUND = 0;


		public sealed class RectData
		{

			public RectData(int left, int top, int right, int bottom)
			{
				Left = left;
				Right = right;
				Bottom = bottom;
				Top = top;
			}
			public RectData() { }
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;

			public override bool Equals(object obj)
			{
				RectData rect = obj as RectData;

				if (rect == null)
					return false;

				return rect.Top == Top && rect.Left == Left && rect.Bottom == Bottom && rect.Right == Right;
			}

			public override int GetHashCode()
			{
				int result = 17;

				result = 31 * result + GetType().GetHashCode();
				result = 31 * result + Top.GetHashCode();
				result = 31 * result + Left.GetHashCode();
				result = 31 * result + Right.GetHashCode();
				result = 31 * result + Bottom.GetHashCode();

				return result;
			}
		}




		public static bool MatchesIgnoringTransparentPixels(Bitmap b1, Bitmap b2){
			if (b1.Width != b2.Width || b1.Height != b2.Height)
				return false;

			for (int row = 0; row < b1.Height; row++)
			{
				for (int col = 0; col < b2.Width; col++){
					int b1pixel = b1[row,col];
					int b2pixel = b2[row,col];
					if (b1pixel != Feature.TRANSPARENT_VALUE
						&& b2pixel != Feature.TRANSPARENT_VALUE
						&& b1pixel != b2pixel)
						return false;
				}
			}

			return true;
		}
		public static Bitmap CombineBitmapsAndMakeDifferencesTransparent(Bitmap b1, Bitmap b2)
		{
			Bitmap combined = Bitmap.FromDimensions(b1.Width, b1.Height);

			for (int row = 0; row < b1.Height; row++)
			{
				for (int col = 0; col < b1.Width; col++)
				{
					if (b1[row, col] != b2[row, col])
						combined[row, col] =  Feature.TRANSPARENT_VALUE;
					else
						combined[row, col] =  b1[row, col];
				}
			}

			return combined;
		}

		public static Bitmap CombineBitmapsAndMakeDifferencesTransparent(List<Bitmap> list)
		{
			Bitmap combined = Bitmap.DeepCopy(list[0]);
			for (int i = 1; i < list.Count; i++)
			{
				combined = CombineBitmapsAndMakeDifferencesTransparent(combined, list[i]);
			}

			return combined;

		}
	}
}

