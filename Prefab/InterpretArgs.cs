using System;
using Prefab;
namespace Prefab
{
	public class InterpretArgs
	{
		private readonly Tree.BatchTransform _updater;

		public readonly Tree Tree;

        public readonly IRuntimeStorage RuntimeStorage;

		public InterpretArgs (Tree tree, Tree.BatchTransform updater, IRuntimeStorage runtimeStorage)
		{
			this._updater = updater;
			Tree = tree;
            RuntimeStorage = runtimeStorage;
		}


		public void Tag(Tree node, string key, object value)
        {
			_updater.Tag (node, key, value);
		}

		public void SetAncestor(Tree node, Tree ancestor)
        {
			_updater.SetAncestor (node, ancestor);
		}



        public void Remove(Tree node)
        {
            _updater.Remove(node);
        }
    }
}