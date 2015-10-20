using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prefab
{
	public class BoundingBox : IBoundingBox
	{
		public BoundingBox(int leftOffset, int topOffset, int width, int height)
		{
			Top = topOffset;
			Left = leftOffset;
			Height = height;
			Width = width;
		}

		private BoundingBox() { }

		public override bool Equals(object obj)
		{
			BoundingBox oBox = obj as BoundingBox;
			return oBox != null &&
				oBox.Left == this.Left &&
				oBox.Top == this.Top &&
				oBox.Height == this.Height &&
				oBox.Width == this.Width;
		}

		public override int GetHashCode()
		{
			return GetHashCode(this);
		}

		private static int GetHashCode(IBoundingBox bb)
		{
			int result = 17;
			result = 31 * result + bb.Top;
			result = 31 * result + bb.Left;
			result = 31 * result + bb.Width;
			result = 31 * result + bb.Height;
			return result;
		}

		public override string ToString()
		{
			return "Top=" + Top.ToString() + ", Left=" + Left.ToString() + ", Width=" + Width.ToString() + ", Height=" + Height.ToString();
		}

		public static IBoundingBox ClipOccluded(IBoundingBox element, IBoundingBox occluder)
		{
			int top = element.Top;
			int bottom = element.Top + element.Height;
			int left = element.Left;
			int right = element.Left + element.Width;

			int occluderRight = occluder.Left + occluder.Width;
			int occluderBottom = occluder.Top + occluder.Height;

			if (top < occluderBottom && bottom > occluderBottom)
				top = occluder.Top + occluder.Height;

			if (left < occluderRight && right > occluderRight)
				left = occluder.Left + occluder.Width;

			if (right > occluder.Left && occluder.Left > left)
				right = occluder.Left;

			if (bottom > occluder.Top && occluder.Top > top)
				bottom = occluder.Top;

			BoundingBox clipped = new BoundingBox(left, top, right - left, bottom - top);

			return clipped;
		}

		public static bool Contains(IBoundingBox rect, int column, int row)
		{
			return row >= rect.Top && row < rect.Top + rect.Height
				&& column >= rect.Left && column < rect.Left + rect.Width;
		}

		public static bool IsAlignedHorizontally(IBoundingBox a, IBoundingBox b)
		{
			IBoundingBox furtherUp = a;
			IBoundingBox furtherDown = b;
			if (b.Top < a.Top)
			{
				furtherUp = b;
				furtherDown = a;
			}

			if (furtherDown.Top <= furtherUp.Top + furtherUp.Height)
				return true;

			return false;
		}

		public static bool IsAlignedVertically(IBoundingBox a, IBoundingBox b)
		{
			IBoundingBox furtherLeft = a;
			IBoundingBox furtherRight = b;
			if (b.Left < a.Left)
			{
				furtherLeft = b;
				furtherRight = a;
			}

			if (furtherLeft.Left + furtherLeft.Width >= furtherRight.Left)
				return true;

			return false;
		}

		public static bool VerticallyShareEdge(IBoundingBox a, IBoundingBox b)
		{
			IBoundingBox top = a;
			IBoundingBox bottom = b;
			if (a.Top > b.Top)
			{
				top = b;
				bottom = a;
			}

			return top.Top + top.Height - 1 == bottom.Top;
		}

		public static bool VerticallyAdjacent(IBoundingBox a, IBoundingBox b)
		{
			IBoundingBox top = a;
			IBoundingBox bottom = b;
			if (a.Top > b.Top)
			{
				top = b;
				bottom = a;
			}

			return top.Top + top.Height - 1 == bottom.Top && IsAlignedVertically(a, b) ;
		}

		/// <summary>
		/// Returns true if a is to the left of b and a horizontal line would intersect
		/// both boxes.
		/// </summary>
		/// <param name="distance">The distance between the items if a is to the left of b or -1 otherwise</param>
		public static bool IsToTheLeft(IBoundingBox a, IBoundingBox b, out int distance)
		{
			bool toRet = a.Left < b.Left &&
			             a.Left + a.Width <= b.Left &&
			             ((a.Top <= b.Top &&
				             b.Top <= a.Top + a.Height) ||
				             (a.Top <= b.Top + b.Height &&
					             b.Top + b.Height <= a.Top + a.Height) ||
				             (b.Top <= a.Top &&
					             a.Top <= b.Top + b.Height));
			distance = -1;
			if (toRet)
			{
				distance = b.Left - (a.Left + a.Width);
			}
			return toRet;
		}

		/// <summary>
		/// Returns true if a is to the right of b and a horizontal line would intersect
		/// both boxes.
		/// </summary>
		/// <param name="distance">The distance between the items if a is to the right of b or -1 otherwise</param>
		public static bool IsToTheRight(IBoundingBox a, IBoundingBox b, out int distance)
		{
			return IsToTheLeft(b, a, out distance);
		}

		/// <summary>
		/// Returns true if a is above b and a horizontal line would intersect
		/// both boxes.
		/// </summary>
		/// <param name="distance">The distance between the items if a is above b or -1 otherwise</param>
		public static bool IsAbove(IBoundingBox a, IBoundingBox b, out int distance)
		{
			bool toRet = a.Top < b.Top &&
			             a.Top + a.Height <= b.Top &&
			             ((a.Left <= b.Left &&
				             b.Left <= a.Left + a.Width) ||
				             (a.Left <= b.Left + b.Width &&
					             b.Left + b.Width <= a.Left + a.Width) ||
				             (b.Left <= a.Left &&
					             a.Left <= b.Left + b.Width));
			distance = -1;
			if (toRet)
			{
				distance = b.Top - (a.Top + a.Height);
			}
			return toRet;
		}

		/// <summary>
		/// Returns true if a is below b and a horizontal line would intersect
		/// both boxes.
		/// </summary>
		/// <param name="distance">The distance between the items if a is below b or -1 otherwise</param>
		public static bool IsBelow(IBoundingBox a, IBoundingBox b, out int distance)
		{
			return IsAbove(b, a, out distance);
		}

		/// <summary>
		/// Returns true if a is entirely inside b
		/// </summary>
		public static bool IsInside(IBoundingBox a, IBoundingBox b)
		{
			return a.Left > b.Left &&
				a.Top > b.Top &&
				b.Left + b.Width > a.Left + a.Width &&
				b.Top + b.Height > a.Top + a.Height;
		}

		/// <summary>
		/// Returns true if a is inside b, where edges of a can be touching edges of b. (i.e. IsInsideInclusive(a,a) returns true)
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool IsInsideInclusive(IBoundingBox a, IBoundingBox b)
		{
			return a.Left >= b.Left &&
				a.Top >= b.Top &&
				b.Left + b.Width >= a.Left + a.Width &&
				b.Top + b.Height >= a.Top + a.Height;
		}

		/// <summary>
		/// Returns true if a entirely surrounds b
		/// </summary>
		public static bool IsOutside(IBoundingBox a, IBoundingBox b)
		{
			return IsInside(b, a);
		}

		/// <summary>
		/// Returns true if a has the same location and size as b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool Equals(IBoundingBox a, IBoundingBox b)
		{
			if (a == null && b != null || a != null && b == null)
				return false;

			return a.Left == b.Left && a.Top == b.Top && a.Width == b.Width && a.Height == b.Height;
		}

		public static readonly IEqualityComparer<IBoundingBox> EqualityComparer = new EqualityComparerObj();

		/// <summary>
		/// Returns a bounding box that is the union of the two bounding boxes.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static IBoundingBox Union(IBoundingBox a, IBoundingBox b)
		{
			if (a == null)
				return b;

			if (b == null)
				return a;



			int left = a.Left;
			int top = a.Top;
			int right = a.Left + a.Width;
			int bottom = a.Top + a.Height;

			if (left > b.Left)
				left = b.Left;

			if (top > b.Top)
				top = b.Top;

			int bright = b.Left + b.Width;
			if (right < bright)
				right = bright;

			int bbottom = b.Top + b.Height;
			if (bottom < bbottom)
				bottom = bbottom;

			return new BoundingBox(left, top, right - left, bottom - top);
		}



		/// <summary>
		/// Returns a bounding box that is the union of all of the bounding boxes in the list.
		/// </summary>
		/// <param name="boundingBoxes"></param>
		/// <returns></returns>
		public static IBoundingBox Union(IEnumerable<IBoundingBox> boundingBoxes)
		{
			IBoundingBox total = null;

			foreach (IBoundingBox bb in boundingBoxes)
			{
				total = Union(total, bb);
			}

			return total;
		}



		/// <summary>
		/// Returns the distance between two points.
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns></returns>
		public static double DistanceBetweenTwoPoints(double x1, double y1, double x2, double y2)
		{
			double deltx = x2 - x1;
			double delty = y2 - y1;
			return (double)Math.Sqrt(deltx * deltx + delty * delty);
		}




		/// <summary>
		/// Partial ordering on boxes. Outer boxes are less than the boxes they contain
		/// </summary>
		public static int OutsideFirstComparer(IBoundingBox x, IBoundingBox y)
		{
			if (IsInside(x, y))
			{
				return 1;
			}
			else if (IsOutside(x, y))
			{
				return -1;
			}
			return 0;
		}

		public static int BiggerAreaFirstComparer(IBoundingBox x, IBoundingBox y)
		{
			if (x.Width * x.Height > y.Width * y.Height)
				return -1;
			else if (x.Width * x.Height < y.Width * y.Height)
				return 1;

			return 0;
		}

		/// <summary>
		/// Comparer to sort items by their left offset.
		/// </summary>
		public static int CompareByLeft(IBoundingBox a, IBoundingBox b)
		{
			return a.Left.CompareTo(b.Left);
		}

		/// <summary>
		/// Comparer to sort items in reading order.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static int CompareByTopThenLeft(IBoundingBox a, IBoundingBox b)
		{
			int comparey = a.Top.CompareTo(b.Top);
			if (comparey == 0)
				return CompareByLeft(a, b);

			return comparey;
		}

		/// <summary>
		/// Creates and returns a new bounding box the tightly encloses the given boxes
		/// </summary>
		public static IBoundingBox CombineBoundingBoxes(IEnumerable<IBoundingBox> boxes)
		{
			IBoundingBox toRet = null;
			foreach (IBoundingBox box in boxes)
			{
				if (toRet == null)
				{
					toRet = box;
				}
				else
				{
					toRet = CombineBoundingBoxes(toRet, box);
				}
			}
			return toRet;
		}

		/// <summary>
		/// Returns true if possibleRight's x coordinate is greater than of possibleLeft's right edge.
		/// </summary>
		/// <param name="possibleLeft"></param>
		/// <param name="possibleRight"></param>
		/// <returns></returns>
		public static bool IsRight(IBoundingBox possibleLeft, IBoundingBox possibleRight)
		{
			return possibleLeft.Left + possibleLeft.Width < possibleRight.Left;
		}

		/// <summary>
		/// Creates and returns a new bounding box the tightly encloses the given boxes
		/// </summary>
		public static BoundingBox CombineBoundingBoxes(IBoundingBox a, IBoundingBox b)
		{
			BoundingBox toRet = new BoundingBox();
			toRet.Left = Math.Min(a.Left, b.Left);
			toRet.Top = Math.Min(a.Top, b.Top);
			toRet.Width = Math.Max(a.Left + a.Width, b.Left + b.Width) - toRet.Left;
			toRet.Height = Math.Max(a.Top + a.Height, b.Top + b.Height) - toRet.Top;
			return toRet;
		}

		/// <summary>
		/// Returns true if a and b overlap.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool Overlap(IBoundingBox a, IBoundingBox b)
		{
			int bright = b.Left + b.Width - 1;
			int bbottom = b.Top + b.Height - 1;
			int abottom = a.Top + a.Height - 1;
			int aright = a.Left + a.Width - 1;

			return a.Left <= bright &&
				aright >= b.Left &&
				a.Top <= bbottom &&
				abottom >= b.Top;


		}

		/// <summary>
		/// The top of the bounding box.
		/// </summary>
		public int Top
		{
			get;
			private set;
		}

		/// <summary>
		/// The left coordinate of the bounding box.
		/// </summary>
		public int Left
		{
			get;
			private set;
		}

		/// <summary>
		/// The bounding box's height parameter.
		/// </summary>
		public int Height
		{
			get;
			private set;
		}

		/// <summary>
		/// The bounding box's width parameter.
		/// </summary>
		public int Width
		{
			get;
			private set;
		}

		/// <summary>
		/// Returns true if the bounding box is the Null instance.
		/// </summary>
		public bool IsNull
		{
			get { return false; }
		}

		/// <summary>
		/// A null instance of BoundingBox. 
		/// </summary>
		public static IBoundingBox Null
		{
			get { return NullBoundingBox.Instance; }
		}

		/// <summary>
		/// The null bounding box class.
		/// </summary>
		private class NullBoundingBox : IBoundingBox
		{
			private NullBoundingBox() { }

			public static IBoundingBox Instance
			{
				get { return s_instance; }
			}
			private static readonly NullBoundingBox s_instance = new NullBoundingBox();

			#region IBoundingBox Members

			public bool IsNull
			{
				get { return true; }
			}

			public int Top
			{
				get { return 0; }
			}

			public int Left
			{
				get { return 0; }
			}

			public int Height
			{
				get { return 0; }
			}

			public int Width
			{
				get { return 0; }
			}

			#endregion
		}


		private class EqualityComparerObj : IEqualityComparer<IBoundingBox>
		{
			public EqualityComparerObj()
			{

			}

			public bool Equals(IBoundingBox a, IBoundingBox b)
			{
				return BoundingBox.Equals(a, b);
			}

			#region IEqualityComparer<IBoundingBox> Members


			public int GetHashCode(IBoundingBox obj)
			{
				return BoundingBox.GetHashCode(obj);
			}

			#endregion
		}
	}
}
