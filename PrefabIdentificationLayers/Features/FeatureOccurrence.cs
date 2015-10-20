using System;

namespace PrefabIdentificationLayers.Features
{
	public sealed class FeatureOccurrence
	{

		public readonly FeatureWrapper FeatureWrapper;
		private readonly int left, top;




		
		public int Height {
			get{ return FeatureWrapper.Height; }
		}


		
		public int Left {
			get{ return left; }
		}

		
		public int Top {
			get{ return top; }
		}

		
		public int Width {
			get{ return FeatureWrapper.Width; }
		}

		
		public override string ToString(){
			return "x=" + left + ", y=" + top + ", w=" + Width + ", h=" + Height;
		}

		public FeatureOccurrence(FeatureWrapper feature, int left, int top){
			this.left = left;
			this.top = top;
			this.FeatureWrapper = feature;
		}



	}
}

