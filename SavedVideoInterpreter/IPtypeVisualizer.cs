using PrefabIdentificationLayers.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace SavedVideoInterpreter
{
    interface IPtypeVisualizer
    {
        Visual Visualize(Ptype ptype, object parameters);
    }
}
