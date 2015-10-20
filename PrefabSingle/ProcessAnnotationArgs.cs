using Prefab;
using PrefabIdentificationLayers.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrefabSingle
{
    public class ProcessAnnotationArgs
    {

        public IRuntimeStorage RuntimeStorage
        {
            get;
            private set;
        }


        public IEnumerable<Ptype> Prototypes
        {
            get;
            private set;
        }


        public ProcessAnnotationArgs(IRuntimeStorage intent, IEnumerable<Ptype> ptypes)
        {
            RuntimeStorage = intent;
            Prototypes = ptypes;
        }


    }
}
