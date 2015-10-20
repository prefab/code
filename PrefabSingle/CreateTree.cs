using IronPython.Runtime;
using Prefab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrefabSingle
{
    public static class CreateTree
    {
        public static MutableTree from_bounding_box(IBoundingBox box, PythonDictionary dict)
        {
            var csharpDict = new Dictionary<string, object>();
            foreach (string key in dict.Keys)
            {
                csharpDict[key] = dict[key];
            }

            return MutableTree.FromBoundingBox(box, csharpDict);
        }
    }
}
