using Newtonsoft.Json.Linq;
using Prefab;
using System;
using System.Collections.Generic;

namespace Prefab
{
	public static class AnnotationUpdateUtility
	{

		

		//Some annotations might be updated with an old library and others might be updated with a new one.
		public static void UpdateInvalidatedLayers(List<LayerWrapper> layers)
		{
            
			//Initialize the intermediate interpretations
			Dictionary<string, InterpretationFromImage> interps = InitializeInterpretations(layers);
			string configId = LayerChain.ConfigurationId(layers, -1);
            LayerWrapper currLayer = null;
			//For each layer, re-interpret the captured images for its annotations
			//and let the layer recompute any data for them
            try
            {
                for (int i = 0; i < layers.Count; i++)
                {
                    currLayer = layers[i];

                    //Check if the layer is invalidated
                    string layerConfigId = GetPreviousConfigId(currLayer);
                    if (!configId.StartsWith(layerConfigId))
                    {
                        List<AnnotatedNode> nodes = NodesFromImageAnnotations(layers, i - 1, interps);
                        AnnotationArgs args = new AnnotationArgs(nodes, currLayer.Intent);

                        //process those nodes
                        currLayer.Layer.ProcessAnnotations(args);

                        //save the configuration id so we know if this layer becomes invalidated.
                        SetConfigId(layers, i);
                    }
                }
            }
            catch (Exception e)
            {
                throw new LayerException(currLayer, e);
            }
		}

		private static string GetPreviousConfigId(LayerWrapper layer)
		{
			try
			{
				return layer.Intent.GetData("config")["id"].Value<string>();
			}
			catch
			{
				return Guid.NewGuid ().ToString ();
			}
		}

		private static void SetConfigId(List<LayerWrapper> layers, int layerIndex)
		{
			LayerWrapper layer = layers[layerIndex];
			string configid = LayerChain.ConfigurationId(layers, layerIndex);
			IRuntimeStorage intent = layer.Intent;

            JObject jobj = new JObject();
            jobj["id"] = configid;
			intent.PutData("config", jobj);
		}


		private static Dictionary<string, InterpretationFromImage> InitializeInterpretations(List<LayerWrapper> layers)
		{

			var interpretationsFromObservations = new Dictionary<String, InterpretationFromImage>();
			foreach (LayerWrapper wrapper in layers)
			{
                IEnumerable<string> libs = wrapper.Layer.AnnotationLibraries();
                if (libs != null)
                {
                    foreach (string lib in libs)
                    {
                        IEnumerable<string> imageids = AnnotationLibrary.GetAllImageIds(lib);
                        foreach (string imageid in imageids)
                        {
                            if (!interpretationsFromObservations.ContainsKey(imageid))
                            {
                                interpretationsFromObservations.Add(imageid, new InterpretationFromImage(AnnotationLibrary.GetImage(lib, imageid)));
                            }
                        }
                    }
                }
			}


			//IEnumerable<string> images = _prefab.GetImages();
			//foreach (JsonData obs in observations)
			//{
			//    interpretationsFromObservations[obs["_id"].ToString()] = new InterpretationFromObservation(obs);
			//}

			return interpretationsFromObservations;
		}

		private class InterpretationFromImage
		{
			public Tree interpretation;

			public LayerWrapper layerWhereInterpretationEnded;

			public InterpretationFromImage(Bitmap image)
			{
				Dictionary<String,Object> attrs = new Dictionary<String,Object>();
				attrs.Add("is_migrating",  "true");
				interpretation = Tree.FromPixels(image, attrs);
			}
		}

		private static List<AnnotatedNode> NodesFromImageAnnotations(List<LayerWrapper> layers, 
			int precedingLayerIndex,  
			Dictionary<string, InterpretationFromImage> interps)
		{
			LayerWrapper currentLayer = layers[precedingLayerIndex + 1];

			List<ImageAnnotation> annotations = new List<ImageAnnotation>();

            IEnumerable<string> layerlibs = currentLayer.Layer.AnnotationLibraries();
            if (layerlibs != null)
            {
                foreach (string lib in layerlibs)
                    annotations.AddRange(AnnotationLibrary.GetAnnotations(lib));
            }

			List<AnnotatedNode> nodesToProcess = new List<AnnotatedNode>();

			//for each one, interpret the observation
			foreach (ImageAnnotation annotation in annotations)
			{
				InterpretationFromImage interp = interps[annotation.ImageId];
				Interpret(layers, precedingLayerIndex, interp);
				Tree root = interp.interpretation;
				Tree matchingnode = MatchingNode(root, annotation.Region);

				AnnotatedNode annoatednode = new AnnotatedNode(matchingnode, root, annotation.Data, annotation.Region, annotation.ImageId);
				nodesToProcess.Add(annoatednode);
			}

			//MergeNodesWithSameRegion(nodesToProcess);

			return nodesToProcess;
		}


        //private static void MergeNodesWithSameRegion(List<AnnotatedNode> nodes){

        //    List<AnnotatedNode> processed = new List<AnnotatedNode>();
        //    for(int i = 0; i < nodes.Count; i++){
        //        AnnotatedNode currNode = nodes[i];
        //        if(!processed.Contains(currNode)){
        //            Dictionary<string, string> merged = new Dictionary<string, string>();

        //            foreach(string key in currNode.Data.Keys)
        //                merged.Add(key, currNode.Data[key]);

        //            for(int j = i+1; j < nodes.Count; j++){

        //                AnnotatedNode possibleMerge = nodes[j];

        //                if(BoundingBox.Equals(possibleMerge.Region, currNode.Region) && possibleMerge.ImageId.Equals(currNode.ImageId))
        //                {
        //                    foreach(string key in possibleMerge.Data.Keys)
        //                        merged.Add(key, possibleMerge.Data[key]);
        //                }

        //                processed.Add(possibleMerge);
        //            }

        //            AnnotatedNode replacement = new AnnotatedNode(currNode.MatchingNode, currNode.Root, merged, currNode.Region, currNode.ImageId );
        //            nodes [i] = replacement;
        //        }

        //    }

        //    foreach(AnnotatedNode toRemove in processed){
        //        nodes.Remove(toRemove);
        //    }
        //}

		private static void Interpret(List<LayerWrapper> layers, int precedingLayerIndex, InterpretationFromImage interp)
		{

			if (precedingLayerIndex >= 0)
			{
				if (interp.layerWhereInterpretationEnded != layers[precedingLayerIndex])
				{
					int start = 0;
					if (interp.layerWhereInterpretationEnded != null)
						start = layers.IndexOf(interp.layerWhereInterpretationEnded);

					interp.interpretation = InterpretSubchain(layers, start, precedingLayerIndex, interp.interpretation);
					interp.layerWhereInterpretationEnded = layers[precedingLayerIndex];
				}
			}
		}

		private static Tree InterpretSubchain(List<LayerWrapper> layers, int startInclusive, int endInclusive, Tree tree){
			
			LayerWrapper currLayer = null;
			try
			{
				for (int i = startInclusive; i <= endInclusive; i++)
				{
					currLayer = layers[i];
					Tree.BatchTransform updater = new Tree.BatchTransform(tree);
					InterpretArgs args = new InterpretArgs(tree, updater, currLayer.Intent);
					currLayer.Layer.Interpret(args);
					tree = updater.GetUpdatedTree();
					currLayer.Layer.AfterInterpret(tree);
				}

			}
			catch (Exception e)
			{
                Tree.BatchTransform newupdater = new Tree.BatchTransform(tree);
                LayerException exception = new LayerException(currLayer, e);
                newupdater.Tag(tree, "interpretation_exception", exception);
                tree = newupdater.GetUpdatedTree();
			}

			return tree;
		}


		private static Tree MatchingNode(Tree tree, IBoundingBox region)
		{
			if (BoundingBox.Equals(region, tree))
				return tree;


			foreach (Tree child in tree.GetChildren())
			{
				if (BoundingBox.IsInsideInclusive(region, child))
					return MatchingNode(child, region);
			}

			return null;
		}
	}
}
