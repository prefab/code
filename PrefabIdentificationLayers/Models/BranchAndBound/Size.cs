using System;

namespace PrefabIdentificationLayers.Models
{
	public class Size
	{

			public int Width;
			public int Height;

		public Size(int width, int height){
			Width = width;
			Height = height;
		}

		public override bool Equals (object obj)
		{
			if (obj is Size) {
				Size s = (Size)obj;
				return s.Width == Width && s.Height == Height;
			}

			return false;
		}

		public override int GetHashCode ()
		{
			int result = 17;

			result = 31 * result + GetType().GetHashCode();
			result = 31 * result + Width.GetHashCode();
			result = 31 * result + Height.GetHashCode();

			return result;
		}
	}
}

