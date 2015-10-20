using System;

using Prefab;

namespace PrefabIdentificationLayers.Features
{
	public class FeatureWrapper
	{

			public readonly Point Hotspot;

			public readonly Feature Feature;

			public int HotspotX{
				get{ return Hotspot.X; }
			}

			public int HotspotY{
				get{ return Hotspot.Y;}
			}

			public int Height{
				get{
					return Feature.Bitmap.Height;
				}
			}
			public int Width{
				get{ return Feature.Bitmap.Width; }
			}
			public FeatureWrapper(Point hotspot, Feature feature){
				this.Hotspot = hotspot;
				this.Feature = feature;
			}


	}
}

