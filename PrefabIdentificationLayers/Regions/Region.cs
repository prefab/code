using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prefab;

namespace PrefabIdentificationLayers.Regions
{
    public class Region
    {
        public Region(string matcher, Bitmap bitmap)
        {
            Bitmap = Bitmap.DeepCopy(bitmap);
            MatchStrategy = matcher;
        }
        public string MatchStrategy
        {
            get;
            private set;
        }
        public Bitmap Bitmap
        {
            get;
            private set;
        }

        public override bool Equals(object obj)
        {
            Region r = obj as Region;

            if (r == null)
                return false;

            return r.MatchStrategy.Equals(MatchStrategy) && r.Bitmap.Equals(Bitmap);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result = 31 * result + Bitmap.GetHashCode();
            result = 31 * result + MatchStrategy.GetHashCode();

            return result;
        }
    }
}
