using System;
using System.Collections.Generic;
using LoveSeat;

namespace Prefab
{
	public class Run
	{
		public static int Main(string[] args)
		{
			if(args.Length == 1)
				Interpret ("", args [0]);

			Console.WriteLine ("Done.");
			return 0;
		}

		private static void Interpret(string appname, string filename){

			try{
				Bitmap bitmap = Bitmap.FromFile(filename);
				Tree start = Tree.FromPixels(bitmap, new Dictionary<string, object>());



                //List<LayerWrapper> layers = LayerChain.LoadChain("prefab", @"../../../layers");
                //AnnotationUpdateUtility.UpdateInvalidatedLayers(layers);


                //Tree output = LayerChain.InterpretChain(layers, start);


                ////Print the output
                //string json = Tree.ToJson(output);
                //Console.WriteLine(json);

                //foreach(LayerWrapper l in layers)
                //    l.Layer.Close();
			

			}catch(Exception e){
				FailedToLoad (filename, e);
			}
		}

		private static void FailedToLoad(string filename, Exception e){
			Console.WriteLine (e.StackTrace);
		}
	}
}

