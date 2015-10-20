using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prefab;


namespace PrefabIdentificationLayers.Models
{
    internal class MRVSelecter : IPartOrderSelecter
    {

        private MRVSelecter() { }
        public static readonly MRVSelecter Instance = new MRVSelecter();

        public Part SelectNextPartToAssign(Dictionary<string, Part> parts, IEnumerable<Bitmap> positives, IEnumerable<Bitmap> negatives)
        {
            return SelectNextPartToAssign(parts.Values);
        }

        /// <summary>
        /// Picks the unassigned variable with the smallest
        /// number of values. (i.e. the most restricted variable "MRV")
        /// </summary>
        /// <param name="vars"></param>
        /// <returns></returns>
        public static Part SelectNextPartToAssign(IEnumerable<Part> parts)
        {

            Part mrv = null;
            int minValues = 0;
            foreach (Part part in parts)
            {
                if (mrv == null && !part.IsAssigned)
                {
                    mrv = part;
                    minValues = part.CurrentValidValues.Count;
                }
                else if (!part.IsAssigned && part.CurrentValidValues.Count < minValues)
                {
                    mrv = part;
                    minValues = part.CurrentValidValues.Count;
                }
            }

            return mrv;
        }
    }
}
