using System;

namespace PrefabIdentificationLayers
{
	public class RegionParameters
	{


		/// <summary>
		/// The starting point to extract the region.
		/// </summary>
		public int Start;

		/// <summary>
		/// The ending margin to stop extracting the region.
		/// </summary>
		public int End;

		/// <summary>
		/// The depth (either rows or columns depending on the
		/// region's orientation) of the region.
		/// </summary>
		public int Depth;

		public String PatternType;


		public RegionParameters(String patternType, int start, int end, int depth)
		{
			this.Start = start;
			this.End = end;
			this.Depth = depth;
			this.PatternType = patternType;
		}


		
		public override bool Equals(Object obj)
		{
			if(obj is  RegionParameters){
				RegionParameters r = (RegionParameters)obj;
				return PatternType.Equals(r.PatternType) && Start == r.Start && End == r.End && Depth == r.Depth;
			}

			return false;
		}

		
		public override int GetHashCode()
		{
			int result = 17;
			result = 31 * result + Start;
			result = 31 * result + End;
			result = 31 * result + Depth;
			result = 31 * result + PatternType.GetHashCode();
			return result;
		}

	}
}

