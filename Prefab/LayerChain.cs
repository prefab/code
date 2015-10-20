using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using Prefab;
namespace Prefab
{
	public class LayerChain
	{
		private readonly static Dictionary<string, object> sharedLayerData = new Dictionary<string, object>();
        private readonly static AssemblyManager  manager = new AssemblyManager();


		public LayerChain (){}



		public static Tree InterpretChain(IEnumerable<LayerWrapper> layers, Tree start){
            Tree.BatchTransform updater = new Tree.BatchTransform(start);
            Tree tree = start;
            LayerWrapper currLayer = null;
            try
            {
                
                
                foreach (LayerWrapper wrapper in layers)
                {
                    currLayer = wrapper;
                    InterpretArgs args = new InterpretArgs(tree, updater, wrapper.Intent);

                    wrapper.Layer.Interpret(args);
                    tree = updater.GetUpdatedTree();
                    wrapper.Layer.AfterInterpret(tree);
                    
                }


            }
            catch(Exception e)
            {
                Tree.BatchTransform newupdater = new Tree.BatchTransform(tree);
                LayerException exception = new LayerException(currLayer, e);
                newupdater.Tag(tree, "interpretation_exception", exception);
                tree = newupdater.GetUpdatedTree();
            }


            return tree;
		}




		public static string ConfigurationId(List<LayerWrapper> layers, int index) {

			int count = 0;
			int end;
			if (index < 0)
				end = layers.Count - 1;
			else
				end = index;



			string id = "";
			bool first = true;
			foreach (LayerWrapper layer in layers)
			{
				string annotationids = "";
                IEnumerable<string> libs = layer.Layer.AnnotationLibraries();
              
                
                if (libs != null)
                {
                      List<string> sorted = new List<string>(libs);
                        sorted.Sort();
                    foreach (string lib in sorted)
                        annotationids += AnnotationLibrary.GetLibraryId(lib);
                }

				int parameters = 17;
				string layerid = layer.Layer.Name  + layer.Id + annotationids + parameters;
				if(!first)
					id += ",";

				first = false;
				id += layerid;

				if (count == end)
					break;

				count++;
			}

			return id;

		}

	}
}

