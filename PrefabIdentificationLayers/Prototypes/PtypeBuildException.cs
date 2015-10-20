using PrefabIdentificationLayers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrefabIdentificationLayers.Prototypes
{
    public class PtypeBuildException : Exception
    {
        public List<BuildPrototypeArgs> BuildArgs
        {
            get;
            private set;
        }

        public PtypeBuildException(List<BuildPrototypeArgs> args) 
            : base("Could not build prototype(s)")
        {
            BuildArgs = args;
        }


    }
}
