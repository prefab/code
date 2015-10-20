using System;
using Prefab;
using System.Collections.Generic;
using PrefabIdentificationLayers.Regions;

namespace PrefabIdentificationLayers.Models.NinePart
{
	public class CostFunction : ICostFunction
	{
		private CostFunction() { }


		public readonly static CostFunction Instance = new CostFunction();


		public double Cost( Dictionary<string, Part> assignment, IEnumerable<Bitmap> positives, IEnumerable<Bitmap> negatives, Dictionary<object,object> cache)
		{
			double total = 0;
			foreach (KeyValuePair<string, Part> pair in assignment)
			{
				Part part = pair.Value;
				string name = pair.Key;
				if (part.IsAssigned)
				{
					Object extracted = Extractor.ExtractPart(name, part.AssignedValue, assignment, positives, negatives, cache);


					//if we couldn't extract the part - return null
					if (extracted == null)
						return Double.PositiveInfinity;


					//if we did extract a part - let's return it's values
					double partcost = 0;

					if (extracted is  Bitmap)
						partcost = ((Bitmap)extracted).PixelCount();
					else if (extracted is Region)
						partcost = ((Region)extracted).Bitmap.PixelCount();
					else
						partcost = ((BackgroundResults)extracted).Missed + ((BackgroundResults)extracted).Region.Bitmap.PixelCount();

					total += partcost;
				}
				else
				{
					total += GetHeuristicCost(name, part, assignment, cache);
				}
			}

			return total;
		}

		private double GetHeuristicCost(string name, Part part, Dictionary<string, Part> assignment, Dictionary<object, object> cache)
		{
			switch (name)
			{
			case "topleft":
			case "topright":
			case "bottomleft":
			case "bottomright":
				return GetCornerHeuristic(part);


			case "top":
				if (assignment["topleft"].IsAssigned && assignment["topright"].IsAssigned)
				{
					Region biggest = GetBiggest(cache, ((Size)assignment["topleft"].AssignedValue).Width, ((Size)assignment["topright"].AssignedValue).Width);
					if (biggest != null)
						return biggest.Bitmap.PixelCount() + biggest.Bitmap.Width;
				}
				break;

			case "left":
				if (assignment["topleft"].IsAssigned && assignment["bottomleft"].IsAssigned)
				{
					Region biggest = GetBiggest(cache, ((Size)assignment["topleft"].AssignedValue).Height, ((Size)assignment["bottomleft"].AssignedValue).Height);
					if (biggest != null)
						return biggest.Bitmap.PixelCount() + biggest.Bitmap.Height;
				}
				break;

			case "right":
				if (assignment["topright"].IsAssigned && assignment["bottomright"].IsAssigned)
				{
					Region biggest = GetBiggest(cache, ((Size)assignment["topright"].AssignedValue).Width, ((Size)assignment["bottomright"].AssignedValue).Width);
					if (biggest != null)
						return biggest.Bitmap.PixelCount() + biggest.Bitmap.Height;
				}
				break;

			case "bottom":
				if (assignment["bottomleft"].IsAssigned && assignment["bottomright"].IsAssigned)
				{
					Region biggest = GetBiggest(cache, ((Size)assignment["bottomleft"].AssignedValue).Width, ((Size)assignment["bottomright"].AssignedValue).Width);
					if (biggest != null)
						return biggest.Bitmap.PixelCount() + biggest.Bitmap.Width;
				}
				break;
			}

			return 1;
		}

		private Region GetBiggest(Dictionary<object, object> cache, int start, int end)
		{
			List<object> matches = new List<object>();
			foreach(object key in cache.Keys){
				if(key is RegionParameters){
					RegionParameters r  = (RegionParameters)key;
					if(r.Start == start && r.End == end)
						matches.Add(r);
				}
			}

			RegionParameters max = null;
			foreach (object match in matches)
			{
				RegionParameters m = (RegionParameters)match;
				if (max == null || m.Depth > max.Depth)
					max = m;
			}

			if (max != null)
				return (Region)cache[max];

			return null;
		}



		private static double GetCornerHeuristic(Part part)
		{
			if (part.IsAssigned)
				return 0;

			Size smallest = (Size)part.CurrentValidValues[0];
			return smallest.Width * smallest.Height;
		}
	}
}

