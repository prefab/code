using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prefab;
using IronPython.Runtime;
using PythonHost;
namespace PrefabSingle
{
    public class PrefabSingleInterpretArgs
    {
        public readonly PythonRuntimeStorageWrapper RuntimeStorage;

        public MutableTree Tree;

        public MutableTree tree { get { return Tree; } }

        public PythonRuntimeStorageWrapper runtime_storage
        {
            get { return RuntimeStorage; }
        }

        public PrefabSingleInterpretArgs(MutableTree tree, IRuntimeStorage runtimeStorage)
        {
            Tree = tree;
            RuntimeStorage = new PythonRuntimeStorageWrapper(runtimeStorage);
        }
    }
}
