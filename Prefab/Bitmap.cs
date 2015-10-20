using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;



using System.Linq;
using System.Threading.Tasks;


namespace Prefab
{
	/// <summary>
	/// This class represents a bitmap object.
	/// A Prefab bitmap is a 2D array of int
	/// objects representing pixels. A pixel
	/// is stored in ARGB format (aa rr gg bb in hex).
	/// </summary>

	public sealed class Bitmap
	{
		public const int TRANSPARENT_VALUE = 0;

		public int Width
		{
			get;
			set;
		}
		public int Height
		{
			get;
			set;
		}

		public int[] Pixels
		{
			get;
			set;
		}



		/// <summary>
		/// Contstructs a bitmap with Width and Height set to 0 and
		/// no internal pixels. This is mainly to support serialization.
		/// </summary>
		public Bitmap() {}

		/// <summary>
		/// Constructs a Bitmap with the given parameters.
		/// Makes a shallow copy of the pixels passed in.
		/// </summary>
		/// <param name="width">Width of the bitmap</param>
		/// <param name="height">Height of the bitmap</param>
		/// <param name="pixels">Array of pixel values</param>
		public static Bitmap FromPixels(int width, int height, int[] pixels){
			return new Bitmap (width, height, pixels);
		}


		/// <summary>
		/// Creates a prefab bitmap given a Ststem.Drawing.Bitmap
		/// </summary>
		public static Bitmap FromSystemDrawingBitmap(System.Drawing.Bitmap inputBitmap){

			int width = inputBitmap.Width;
			int height = inputBitmap.Height;
			int[] pixels = new int[width * height];

			BitmapData bitmapData = inputBitmap.LockBits(
				new Rectangle(0, 0, width, height),
				ImageLockMode.ReadOnly,
				System.Drawing.Imaging.PixelFormat.Format32bppArgb
			);
			System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, pixels, 0, pixels.Length);

			inputBitmap.UnlockBits(bitmapData);

			Bitmap toreturn = Bitmap.FromPixels (width, height, pixels);


			return toreturn;
		}

		private Bitmap(int width, int height, int[] pixels) : this()
		{
			if (width <= 0 || height <= 0)
			{
				throw new Exception("Cannot create a bitmap with a negative width or height parameter.");
			}
			if (pixels.Length < width * height)
			{
				throw new Exception("Cannot create a bitmap with fewer pixels than the width and height are set.");
			}

			Width = width;
			Height = height;
			Pixels = pixels;
		}







		/// <summary>
		/// Cosntructs a bitmap with the given parameters.
		/// A new array of pixels is created.
		/// </summary>
		/// <param name="width">Width of the bitmap</param>
		/// <param name="height">Height of the bitmap</param>
		public static Bitmap FromDimensions(int width, int height){
			return new Bitmap (width, height);
		}


		private Bitmap(int width, int height)
			: this(width, height, new int[width * height])
		{ }

		public static Bitmap DeepCopy(Bitmap toCopy) {

			return DeepCopyUsingBuffer(toCopy, new int[toCopy.Width * toCopy.Height]);

		}

		private static Bitmap DeepCopyUsingBuffer(Bitmap toCopy, int[] bufferToUse){
			int index = 0;
			for (int row = 0; row < toCopy.Height; row++)
			{
				for (int col = 0; col < toCopy.Width; col++)
				{
					bufferToUse[index] = toCopy[row, col];
					index++;
				}
			}

			return new Bitmap(toCopy.Width, toCopy.Height, bufferToUse);
		}



		/// <summary>
		/// Converts an array of integers to an array of bytes.
		/// </summary>
		/// <param name="ints"></param>
		/// <returns></returns>
		public static byte[] BytesFromIntArray(int[] ints)
		{
			List<byte> bytes = new List<byte>();
			foreach (int i in ints)
				bytes.AddRange(BitConverter.GetBytes(i));
			return bytes.ToArray();
		}

		/// <summary>
		/// Converts an array of bytes to an array of ints.
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static int[] IntArrayFromByteArray(byte[] bytes)
		{
			int[] ints = new int[bytes.Length / 4];
			Buffer.BlockCopy(bytes, 0, ints, 0, bytes.Length);
			return ints;
		}


		/// <summary>
		/// Returns the number of pixels in the bitmap.
		/// </summary>
		/// <returns>The number of pixels in the bitmap.</returns>
		public int PixelCount()
		{
			return Width * Height;
		}

		/// <summary>
		/// Gets or sets the pixel value at the given offset.
		/// </summary>
		/// <param name="offset">The offset to get or set.</param>
		/// <returns>The pixel value at offset</returns>
		public int this[int offset]
		{
			get
			{
				return Pixels[offset];
			}
			set
			{
				Pixels[offset] = value;
			}
		}

		/// <summary>
		/// Gets or sets the pixel value at the given coordinates.
		/// </summary>
		/// <param name="column">The column coordinate of the pixel value.</param>
		/// <param name="row">The row coordinate of the pixel value.</param>
		/// <returns>The pixel value at the given coordinates.</returns>
		public int this[int row, int column]
		{
			get
			{
				return Pixels[(row * Width) + column];
			}
			set
			{
				Pixels[(row * Width) + column] = value;
			}
		}


		/// <summary>
		/// Crops a region inside the bitmap starting at (row, column)
		/// that is numRows tall and numColumns wide.
		/// </summary>
		/// <param name="row">The starting row to crop.</param>
		/// <param name="column">The starting column to crop.</param>
		/// <param name="numRows">The number of rows to crop.</param>
		/// <param name="numColumns">The number of columns to crop.</param>
		/// <returns>A cropped region of the bitmap.</returns>
		public static Bitmap Crop(Bitmap src, int x, int y, int width, int height)
		{
			Bitmap cropped = new Bitmap(width, height, new int[width*height]);

			for(int row = 0; row < height; row++){
				for(int col = 0; col < width; col++){
					int pixel = src[row + y, col + x];
					cropped[row, col] =  pixel;
				}
			}

			return cropped;
		}




		/// <summary>
		/// Crops the region defined by the bounding box from the bitmap
		/// </summary>
		public static Bitmap Crop(Bitmap bitmap, IBoundingBox box)
		{
			return Crop(bitmap, box.Left, box.Top, box.Width, box.Height);
		}




		public void WriteBlock(int value, IBoundingBox rect)
		{
			int bottom = rect.Top + rect.Height;
			int right = rect.Left + rect.Width;

			//Parallel.For(rect.Top, bottom, delegate(int row)
			//{
			for(int row = rect.Top; row < bottom; row++){
				for (int col = rect.Left; col < right; col++)
				{
					this[row, col] = value;
				}
			}//);
		}





		/// <summary>
		/// Returns true if the bitmap exactly matches
		/// the bitmap passed in.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (!(obj is Bitmap))
				return false;

			Bitmap bmp = (Bitmap)obj;
			return Bitmap.ExactlyMatches(this, bmp);
		}

		public static bool ExactlyMatches (Bitmap bitmap1, Bitmap bitmap2)
		{
			if (bitmap1.Width != bitmap2.Width || bitmap1.Height != bitmap2.Height)
				return false;

			int total = bitmap1.Width * bitmap1.Height;

			for(int i = 0; i < total; i++ )
				if(bitmap1.Pixels[i] != bitmap2.Pixels[i])
					return false;


			return true;
		}

		public static bool AllOneValue (Bitmap bitmap)
		{
			int initial = bitmap[0,0];
			for (int row = 0; row < bitmap.Height; row++)
			{
				for (int col = 0; col < bitmap.Width; col++)
				{
					int value = bitmap[row, col];
					if (value != initial)
						return false;
				}
			}

			return true;
		}

		public static int Alpha (int pixel)
		{
			return (pixel >> 24) & 0xff;
		}

        public static int Red(int pixel)
        {
            return (pixel >> 16) & 0xff;
        }

        public static int Green(int pixel){
            return (pixel >> 8) & 0xff;
        }

        public static int Blue(int pixel){
            return pixel & 0xff;
        }

		public static bool AllTransparent (Bitmap bitmap)
		{
			for (int row = 0; row < bitmap.Height; row++)
			{
				for (int col = 0; col < bitmap.Width; col++)
				{
					if (Alpha(bitmap[row, col]) == 255)
						return false;
				}
			}
			return true;
		}

		public static Bitmap SetTransparentValues (Bitmap bitmap)
		{
			Bitmap setTransparent = DeepCopy(bitmap);
			for (int row = 0; row < bitmap.Height; row++)
			{
				for (int col = 0; col < bitmap.Width; col++)
				{
					if (Alpha(setTransparent[row, col]) != 255)
						setTransparent[row, col] =  TRANSPARENT_VALUE;
				}
			}

			return setTransparent;
		}

		/// <summary>
		/// Returns the hash code which iterates through each pixel to return a unique value.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			Int32 result = 17;
			result =  31 * result + Width.GetHashCode();
			result = 31 * result + Height.GetHashCode();

			for (int row = 0; row < Height; row++)
				for (int col = 0; col < Width; col++)
					result = 31 * result + this[row, col];

			return result;
		}




		/// <summary>
		/// Loads a bitmap from the given file location.
		/// </summary>
		/// <param name="filename">The location of the bitmap file</param>
		/// <returns>The Prefab.Utils.Bitmap object representing the image</returns>
		public static Bitmap FromFile(string filename)
		{
			System.Drawing.Bitmap img = new System.Drawing.Bitmap (filename);

			return FromSystemDrawingBitmap (img);

		}





		public static void SetFullAlpha(Bitmap bitmap)
		{
			int length = bitmap.Width * bitmap.Height;
			for (int i = 0; i < length; i++)
			{
				bitmap[i] = (0xff << 24) | bitmap[i];  
			}
		}




		/// <summary>
		/// Saves a bitmap to a given file location.
		/// </summary>
		/// <param name="bitmap">The bitmap to save.</param>
		/// <param name="filename">The location to save the bitmap.</param>
		public static void SaveTofile(Bitmap bitmap, string filename)
		{
			System.Drawing.Bitmap bmp = ToSystemDrawingBitmap (bitmap);
			bmp.Save (filename);
		}




		/// <summary>
		/// Creates a System.Drawing.Bitmap from the infromation in this bitmap
		/// </summary>
		/// <returns></returns>
		public static System.Drawing.Bitmap ToSystemDrawingBitmap(Bitmap source)
		{
			System.Drawing.Bitmap dest = new System.Drawing.Bitmap (source.Width, source.Height);
			ToSystemDrawingBitmap (source, dest);

			return dest;
		}

		public static void ToSystemDrawingBitmap(Bitmap source, System.Drawing.Bitmap dest)
		{
			BitmapData bitmapData = dest.LockBits(
				new Rectangle(0, 0, source.Width, source.Height),
				ImageLockMode.ReadWrite,
				System.Drawing.Imaging.PixelFormat.Format32bppArgb
			);

			System.Runtime.InteropServices.Marshal.Copy(source.Pixels, 0, bitmapData.Scan0, dest.Width * dest.Height);

			dest.UnlockBits(bitmapData);
		}


		/// <summary>
		/// Returns true if the given offset is within the bounds of the bitmap.
		/// </summary>
		/// <param name="rowOffset">Row position of the offset.</param>
		/// <param name="columnOffset">Column position of the offset.</param>
		/// <returns>True if the given offset is within the bounds of the bitmap.</returns>
		public static bool OffsetIsInBounds(Bitmap bitmap, Point offset)
		{
			if (offset.Y >= 0 && offset.X >= 0 && offset.Y < bitmap.Height && offset.X < bitmap.Width)
				return true;

			return false;

		}





	}
}
