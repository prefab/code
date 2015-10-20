using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronPython.Runtime;

using Prefab;
using PrefabUtils;

namespace PythonHost
{
    public class PythonInterpretArgs
    {
        public PythonInterpretArgs(InterpretArgs interpretArgs)
        {
            tree = interpretArgs.Tree;
            runtime_storage = new PythonRuntimeStorageWrapper(interpretArgs.RuntimeStorage);
            args = interpretArgs;
        }


        private InterpretArgs args;

        public Tree tree
        {
            get;
            internal set;
        }

        public PythonRuntimeStorageWrapper runtime_storage
        {
            get;
            internal set;
        }


        public PythonInterpretArgs tree_transformer
        {
            get { return this; }

        }


        public void enqueue_set_tag(Tree node, string key, object value)
        {            
            args.Tag(node, key, value);
        }

        public void enqueue_set_ancestor(Tree node, Tree ancestor)
        {
            args.SetAncestor(node, ancestor);
        }


        public string get_path(Tree node)
        {
            return PathDescriptor.GetPath(node, tree);
        }

        public Tree get_parent(Tree node)
        {
            return Tree.GetParent(node, tree);
        }

        public Tree create_node(int left, int top, int width, int height, PythonDictionary tags)
        {
            BoundingBox bb = new BoundingBox(left, top, width, height);
            Dictionary<string, object> cTags = new Dictionary<string,object>();
            foreach(string key in tags.Keys)
                cTags[key] = tags[key];

            return Tree.FromBoundingBox(bb, cTags);
        
       }

        public Tree create_node(IBoundingBox bb, PythonDictionary tags)
        {
            return create_node(bb.Left, bb.Top, bb.Width, bb.Height, tags);
        }
    }
}
