using System;
using Prefab;
using System.Collections.Generic;

namespace PrefabIdentificationLayers.Prototypes
{

    [Serializable]
	public class ContentDetectionLayer : Layer
	{


		Bitmap foreground;
		/// <summary>
		/// Finds the unpredictable content using the backround regions of elements.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="image"></param>
		/// <param name="background"></param>
		/// <param name="isForeground"></param>
		private static List<Tree> FindAndAddContent(InterpretArgs args, Tree node, Bitmap image, Bitmap isForeground){
			Ptype ptype = (Ptype)node["ptype"];
			List<Tree> allFound = new List<Tree>();
			if(ptype != null){
				ptype.Model.Finder.SetForeground(node, image, isForeground);
			}
			//recurse. if no siblings are overlapping (the majority case) then we can run each child in parallel.
//        if (!anyOverlapping(node.children())){
//            List<Future<ICollection<Tree>>> asyncResults = new List<Future<ICollection<Tree>>>();
//            for(final Tree child : node.children()){
//                Callable<ICollection<Tree>> callable = new Callable<ICollection<Tree>>(){
//                   public ICollection<Tree> call(){
//                        return findAndAddContent(args, ptype, child, image, isForeground);
//                  }
//                };
//                Future<ICollection<Tree>> results =  Utils.service.submit(callable);
//                asyncResults.add(results);
//            }
//
//            for(Future<ICollection<Tree>> res : asyncResults){
//
//                try {
//                    allFound.addAll(res.get());
//                } catch (InterruptedException e) {
//                    e.printStackTrace();  //To change body of catch statement use File | Settings | File Templates.
//                } catch (ExecutionException e) {
//                    e.printStackTrace();  //To change body of catch statement use File | Settings | File Templates.
//                }
//            }
//        }
//        else //we can't run children in parallel if any nodes overlap.
//        //nodes only overlap when there's a false positive. so we might be able
//        //to trigger something here and automatically correct that.
//        {
			foreach (Tree child in node.GetChildren())
				FindAndAddContent(args, child, image, isForeground);


			if(ptype != null){
				ptype.Model.Finder.FindContent(node, image, isForeground, allFound);
				foreach(Tree found  in allFound)
					args.SetAncestor(found, node);

                allFound.AddRange(node.GetChildren());
                PrototypeDetectionLayer.SetAncestorsByContainment(allFound, args);
			}



			// }

			return allFound;
		}


		/// <summary>
		/// Returns true if any two nodes in the list are overlapping.
		/// </summary>
		/// <param name="nodes"></param>
		/// <returns></returns>
		private static bool AnyOverlapping(List<Tree> tocheck)
		{
			List<Tree> nodes = new List<Tree>();
			nodes.AddRange(tocheck);
			nodes.Sort(BoundingBox.CompareByLeft);


			for (int i = 0; i < nodes.Count - 1; i++)
			{
				int right = nodes[i].Left + nodes[i].Width;
				for (int j = i + 1; j < nodes.Count && right > nodes[j].Left; j++)
				{
					if (BoundingBox.Overlap(nodes[i], nodes[j]))
						return true;
				}
			}

			return false;
		}


		public string Name {
			get{
				return "content_detection";
			}
		}

		
		public void Init(Dictionary<String, Object> parameters) {
			Dictionary<String,Object> shared = (Dictionary<String,Object>)parameters["shared"];
			foreground = Bitmap.FromDimensions(Utils.DEFAULT_BUFFER_WIDTH, Utils.DEFAULT_BUFFER_HEIGHT);
		}

		
		public void Interpret(InterpretArgs args) {
			Bitmap bitmap = (Bitmap)args.Tree["capturedpixels"];
			FindAndAddContent(args, args.Tree,bitmap, foreground);
		}

		
		public void AfterInterpret(Tree tree) {
			//To change body of implemented methods use File | Settings | File Templates.
		}

		
		public void ProcessAnnotations(AnnotationArgs args) {
			//To change body of implemented methods use File | Settings | File Templates.
		}

		
		public IEnumerable<string> AnnotationLibraries() {
			return null;  //To change body of implemented methods use File | Settings | File Templates.
		}

		
		public void Close(){}
	}
}

