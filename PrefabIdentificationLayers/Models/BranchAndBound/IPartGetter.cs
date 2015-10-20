using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prefab;


namespace PrefabIdentificationLayers.Models
{
    public interface IPartGetter
    {
        Dictionary<string, Part> GetParts(IEnumerable<Bitmap> positives, IEnumerable<Bitmap> negatives);
    }
}
