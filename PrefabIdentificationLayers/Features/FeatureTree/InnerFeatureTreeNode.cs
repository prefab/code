using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prefab;
using System.Collections.ObjectModel;


namespace PrefabIdentificationLayers.Features.FeatureTree
{
    /// <summary>
    /// A node that is not a leaf node in a FeatureTree
    /// </summary>
    internal class InnerFeatureTreeNode : FeatureTreeNode
    {


		/// <summary>
		/// Constructs a node given the parameters. A shallow copy is
		/// made of each parameter.
		/// </summary>
		/// <param name="offsetToTest">The offset to test at this node.</param>
		/// <param name="childrenByColor">The children bucketed by their pixel value at offsetToTest.</param>
		/// <param name="transparentChild">The child node that should be checked if no other children are matched. </param>
		public InnerFeatureTreeNode(Point offsetToTest, 
			Dictionary<Int32, FeatureTreeNode> childrenByColor, FeatureTreeNode transparentChild)
		{
			m_offsetToTest = offsetToTest;
			m_childrenByColor = childrenByColor;
			m_transparentChild = transparentChild;
		}

		#region FeatureTreeNode Members

		public void GetMatches(Bitmap bitmap, int probeOffsetX, int probeOffsetY, ICollection<Tree> bucket)
		{
			int imageOffsetX, imageOffsetY;
			imageOffsetX = probeOffsetX + m_offsetToTest.X;
			imageOffsetY = probeOffsetY + m_offsetToTest.Y;
			//Point.Add(probeOffsetX, m_offsetToTest.X, probeOffsetY, m_offsetToTest.Y, out imageOffsetX, out imageOffsetY);

			if (imageOffsetX >= 0 && imageOffsetY >= 0
				&& imageOffsetY < bitmap.Height && imageOffsetX < bitmap.Width)
			{
				FeatureTreeNode child = null;
				bool has = m_childrenByColor.TryGetValue(bitmap[imageOffsetY, imageOffsetX], out child);
				if (has)
				{
					child.GetMatches(bitmap, probeOffsetX, probeOffsetY, bucket);
					if(m_transparentChild != null)
						m_transparentChild.GetMatches(bitmap, probeOffsetX, probeOffsetY, bucket);
					return;
				}
			}
			if(m_transparentChild != null)
				m_transparentChild.GetMatches(bitmap, probeOffsetX, probeOffsetY, bucket);
		}

		public bool IsLeaf
		{
			get { return false ; }
		}

		#endregion

		/// <summary>
		/// The offset to check at this node. The offset is relative to the
		/// probe offset.
		/// </summary>
		private Point m_offsetToTest;

		/// <summary>
		/// The child node representing the case where no pixel value matches
		/// any of the children, and thus the probed pixel must be assumed to be
		/// transparent.
		/// </summary>
		private FeatureTreeNode m_transparentChild;

		/// <summary>
		/// The children bucketed by their pixel value at the OffsetToTest.
		/// </summary>
		private Dictionary<Int32, FeatureTreeNode> m_childrenByColor;

//		private FixedNodeDictionary nodesByColor;
//		private FeatureTreeNode transparentNode;
//		private Point offsetToTest;
//
//		public InnerFeatureTreeNode(Point offsetToTest,
//			Dictionary<int, FeatureTreeNode> nodesByColor,
//			FeatureTreeNode transparentNode) {
//
//			this.nodesByColor = new FixedNodeDictionary(nodesByColor);
//			this.transparentNode = transparentNode;
//			this.offsetToTest = offsetToTest;
//
//		}
//
//		public void GetMatches(Bitmap bitmap, int probeOffsetX, int probeOffsetY, ICollection<Tree> bucket)
//		{
//			int imageOffsetX, imageOffsetY;
//			imageOffsetX = probeOffsetX + offsetToTest.X;
//			imageOffsetY = probeOffsetY + offsetToTest.Y;
//
//			if (imageOffsetX >= 0 && imageOffsetY >= 0
//				&& imageOffsetY < bitmap.Height && imageOffsetX < bitmap.Width)
//			{
//				int color = bitmap[imageOffsetY, imageOffsetX];
//				FeatureTreeNode child = nodesByColor.Get(color);
//				if (child != null)
//				{
//					child.GetMatches(bitmap, probeOffsetX, probeOffsetY, bucket);
//				}
//			}
//
//			if(transparentNode != null)
//				transparentNode.GetMatches(bitmap, probeOffsetX, probeOffsetY, bucket);
//		}
//
//
//		public bool IsLeaf
//		{
//			get{
//				return false;
//
//			}
//		}
//
//
//		private sealed class FixedNodeDictionary{
//
//			private GraphUtilities.BMZ bmz;
//			private FeatureTreeNode[] nodesByColor;
//			private int[] keys;
//			public FixedNodeDictionary(Dictionary<int, FeatureTreeNode> nodesByColorMap){
//
//				int size = nodesByColorMap.Count;
//				nodesByColor = new FeatureTreeNode[size];
//
//				bmz = GraphUtilities.GetBmz(nodesByColorMap.Keys);
//
//				keys = new int[size];
//
//				foreach(int key in nodesByColorMap.Keys){
//
//					int index = bmz.DoHash(key);
//					keys[index] = key;				
//					nodesByColor[index] = nodesByColorMap[key];
//				}
//
//			}
//
//			public FeatureTreeNode Get(int color){
//				int index = bmz.DoHash(color);
//				if(keys[index] != color)
//					return null;
//
//				return nodesByColor[index];
//			}
//
//		}
	}

}
