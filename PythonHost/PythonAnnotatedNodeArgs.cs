using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prefab;
using PrefabUtils;
using IronPython.Runtime;

namespace PythonHost
{
    public class PythonAnnotatedNodeArgs
    {
        internal AnnotationArgs Args;



        public PythonRuntimeStorageWrapper runtime_storage;

        public List annotated_nodes
        {
            get {
                List list = new List();
                foreach (var an in Args.AnnotatedNodes)
                {
                    list.append(new PythonAnnotatedNodeWrapper(an));
                }

                return list;
            }
        }

        public string get_path(PythonAnnotatedNodeWrapper node)
        {
            return PathDescriptor.GetPath(node.node, node.root);
        }

        
    }
}
