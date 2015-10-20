using System;
using System.Collections.Generic;

namespace Prefab
{
	public class AnnotationArgs
	{
		public readonly List<AnnotatedNode> AnnotatedNodes;
        public readonly IRuntimeStorage RuntimeStorage;

		public AnnotationArgs (List<AnnotatedNode> nodes, IRuntimeStorage runtimeStorage)
		{
			this.AnnotatedNodes = nodes;
            this.RuntimeStorage = runtimeStorage;
		}
	}
}

