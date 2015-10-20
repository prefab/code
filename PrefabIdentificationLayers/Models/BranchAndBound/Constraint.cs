using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrefabIdentificationLayers.Models {
    public class Constraint {
        public Part Part1
        {
            get;
            private set;
        }
        public Part Part2
        {
            get;
            private set;
        }
        public delegate bool SatisfiedFunction(object v1, object v2);
        public Func<object,object,bool> Satisfied;

        public Constraint(Func<object, object, bool> satisified, Part p1, Part p2) {
            Satisfied = satisified;
            Part1 = p1;
            Part2 = p2;
        }
    }
}
