using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PrefabIdentificationLayers.Prototypes;
using Prefab;


namespace PrefabIdentificationLayers.Models
{
    public interface IPtypeFromAssignment
    {
        Ptype.Mutable ConstructPtype(Dictionary<string, Part> assignment, 
                                              IEnumerable<Bitmap> positives, 
                                              IEnumerable<Bitmap> negatives, 
                                              Dictionary<object,object> cache);
    }
}
