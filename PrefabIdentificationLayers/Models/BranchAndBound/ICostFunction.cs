using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prefab;


namespace PrefabIdentificationLayers.Models
{
    public interface ICostFunction
    {
        double Cost(Dictionary<string, Part> assignment, IEnumerable<Bitmap> positives, IEnumerable<Bitmap> negatives, Dictionary<object,object> cache);
    }
}
