using System;
using Prefab;
using System.Collections.Generic;

namespace PrefabIdentificationLayers.Prototypes
{
	public class ContentFinder
	{
		private Bitmap ccLabelingBitmap = Bitmap.FromDimensions(Utils.DEFAULT_BUFFER_WIDTH, Utils.DEFAULT_BUFFER_HEIGHT);
		public static readonly ContentFinder Instance = new ContentFinder();

		private ContentFinder() { }

		public void FindContent(Bitmap image, Bitmap foregroundImage, Tree currentNode, List<Tree> foundContent)
		{
			Utils.RectData[] found = ConnectedComponents.TwoPass(ccLabelingBitmap, foregroundImage, currentNode, Utils.BACKGROUND);

			foreach (Utils.RectData rect in found)
			{
				if(rect != null){
					int hash = 17;
					for (int row = rect.Top; row <= rect.Bottom; row++)
					{
						for (int col = rect.Left; col <= rect.Right; col++)
						{
							hash = 31 * hash + image[row, col];
						}
					}

					BoundingBox bb = new BoundingBox(rect.Left, rect.Top, (rect.Right - rect.Left) + 1, (rect.Bottom - rect.Top) + 1);

					Dictionary<string,object> tags = new Dictionary<string, object>();
					tags.Add("type", "content");
					tags.Add("pixel_hash", hash);

					Tree node = Tree.FromBoundingBox(bb, tags);

					foundContent.Add(node);
				}
			}
		}
	}
}

