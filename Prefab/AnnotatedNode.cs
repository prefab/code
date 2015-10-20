using Newtonsoft.Json.Linq;
using Prefab;
using System.Collections;
using System.Collections.Generic;

namespace Prefab{
	public class AnnotatedNode {

		public readonly Tree MatchingNode, Root;
		public readonly JObject Data;
		public readonly IBoundingBox Region;
		public readonly string ImageId;
		public AnnotatedNode(Tree matchingNode, Tree root, JObject data, IBoundingBox region, string imageId){
			this.MatchingNode = matchingNode;
			this.Root = root;
			this.Data = data;
			this.Region = region;
			this.ImageId = imageId;
		}
	}
}


