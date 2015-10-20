using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrefabIdentificationLayers.Regions
{
    public interface IRegionMatchStrategy
    {
        string Name
        {
            get;
        }
    }
}
