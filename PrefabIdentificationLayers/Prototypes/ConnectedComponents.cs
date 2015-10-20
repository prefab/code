using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;

using Prefab;


namespace PrefabIdentificationLayers.Prototypes {

    public class ConnectedComponents
    {

		public static Utils.RectData[] TwoPass(Bitmap labels, Bitmap srcBitmap, Tree root, int background)
		{
			DisjointSets linked = new DisjointSets(root.Width * root.Height);
			int bottom = root.Top + root.Height;
			int right = root.Left + root.Width;

			int numelements = IdentifyComponents(labels, srcBitmap, linked, root, root.GetHashCode(), bottom, right, background);

			//Run the second pass through the image to label each component
			//and create bounding rectangles
			LabelComponentsAndSetBoundingBoxes(labels, linked, root.Top, root.Left, bottom, right, background);


			return GetRectsFromLabeledImage(labels, srcBitmap, numelements, root.Top, root.Left, bottom, right, background);
		}

		private static int IdentifyComponents(Bitmap labels, Bitmap srcBitmap, DisjointSets linked, Tree toProcess, int treeNodeIndex, int bottom, int right, int background)
		{
			int numElements = 0;

			//Run the first pass through the image to find any foreground content
			int[] neighbors = new int[8];
			for (int row = toProcess.Top; row < bottom; row++)
			{
				for (int col = toProcess.Left; col < right; col++)
				{
					int sourcepixel = srcBitmap[row, col];

					if (sourcepixel == treeNodeIndex)
					{
						int neighborCount = NeighborsConnected8(neighbors, toProcess, row, col, labels, background);
						if (neighborCount == 0)
						{

							labels[row, col] = numElements+1;
							numElements++;

						}
						else
						{
							int min = Min(neighbors, neighborCount);
							labels[row, col] =  min;
							for(int i = 0; i < neighborCount; i++)
							{
								linked.union(min, neighbors[i]);

							}
						}

					}
					else
						labels[row, col] = background;
				}
			}

			return numElements;

		}

		private static void LabelComponentsAndSetBoundingBoxes(Bitmap labels, DisjointSets linked, int top, int left, int bottom, int right, int background)
		{
			//Run the second pass through the image to label each component
			//and create bounding rectangles
			for (int row = top; row < bottom; row++)
			{
				for (int col = left; col < right; col++)
				{
					if (labels[row, col] != background)
					{
						int id = linked.Find(labels[row, col]);
						labels[row, col] = id;
					}
				}
			}
		}

		public static Utils.RectData[] GetRectsFromLabeledImage(Bitmap labels, Bitmap src, int numelements,
			int top, int left, int bottom, int right, int background)
		{
			Utils.RectData[] occurrences = new Utils.RectData[numelements];

			for (int row = top; row < bottom; row++)
			{
				for (int col = left; col < right; col++)
				{
					Utils.RectData rect;
					int label = labels[row, col];
					if (label != background)
					{
						rect = occurrences[label-1];
						if (rect != null)
						{
							rect.Right = Math.Max(col, rect.Right);
							rect.Left = Math.Min(col, rect.Left);
							rect.Bottom = row;
						}
						else
						{
							rect = new Utils.RectData();
							rect.Left = col;
							rect.Right = col;
							rect.Bottom = row;
							rect.Top = row;
							occurrences[label-1] = rect;
						}
					}
				}
			}

			return occurrences;
		}

		private static int Min(int[] list, int count)
		{
			int min = int.MaxValue;
			for(int i= 0; i < count; i++){
				if(list[i] < min)
					min = list[i];
			}

			return min;
		}

		private static int NeighborsConnected8(int[] neighbors, IBoundingBox root,
			int rowOffset, int colOffset, Bitmap labels, int background)
		{
			int neighborCount = 0;
			//Find the neighbors above me.
			if (rowOffset > root.Top)
			{
				int row = rowOffset - 1;
				int maxCol = colOffset;
				if (maxCol < root.Left + root.Width - 1)
					maxCol++;

				for (int col = colOffset; col <= maxCol; col++)
				{
					int label = labels[row, col];
					if (label != background){
						neighbors[neighborCount] = label;
						neighborCount++;
					}

				}
			}


			//Find the neighbors to the left
			if (colOffset > root.Left)
			{
				int col = colOffset - 1;

				if (rowOffset > root.Top)
				{

					if (labels[rowOffset - 1, col] != 0){


						neighbors[neighborCount] = labels[rowOffset - 1, col];
						neighborCount++;
					}
				}

				if (labels[rowOffset, col] != 0){
					neighbors[neighborCount] = labels[rowOffset, col];
					neighborCount++;
				}
			}


			return neighborCount;
		}


    }
}
