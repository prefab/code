using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prefab;


namespace PrefabIdentificationLayers.Models
{
    public interface IPartOrderSelecter
    {
        Part SelectNextPartToAssign(Dictionary<string, Part> parts, IEnumerable<Bitmap> positives, IEnumerable<Bitmap> negatives);
    }
}
