using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrefabSingle
{
    public class PathDescriptorAnnotation : IAnnotation
    {
        public string PathDescriptor
        {
            get;
            private set;
        }
        public string Id
        {
            get { return PathDescriptor; }
        }
        public JToken Data
        {
            get;
            private set;
        }

        public PathDescriptorAnnotation(string descriptor, JToken data)
        {
            PathDescriptor = descriptor;
            Data = data;
        }
    }
}
