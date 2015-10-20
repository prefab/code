using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.IO;
using System.Reflection;
using Prefab;
namespace PythonHost
{
	public class ChainLoader
	{
		private readonly static Dictionary<string, object> sharedLayerData = new Dictionary<string, object>();
        private readonly static AssemblyManager  manager = new AssemblyManager();
       
        public ChainLoader() { }



		/// <summary>
		/// Loads a layer chain from the given directory.
		/// The method looks for a chain.txt file with layer names,
		/// and it looks for dll files to create the corresponding layers
		/// 
		/// Subchains can be packaged in directories. Just provide another
		/// chain.txt file in that directory with the corresponding layers,
		/// and include the corresponding dlls.
		/// 
		/// Then, in the parent directory, make sure chain.txt lists the folder name
		/// as a layer.
		/// </summary>
		/// <returns>The chain.</returns>
		/// <param name="dir">Dir.</param>
        public static List<LayerWrapper> LoadChain(string appName, string dir)
        {
            var toreturn = new List<LayerWrapper>();

            //Get the proposed chain from the text file chain.txt,
            //and by also recursively visiting subdirectories
            //Then create seed layers based on those names and available dlls
            //Then use those seed layers to create actual layers for each item in the chain
            PythonScriptHost.Instance.ResetEngine();
            var layerinfos = RecursiveGetLayerInfos(dir);
            var seeds = GetSeedLayers(layerinfos, PythonScriptHost.Instance);

            int index = 0;


            foreach (LayerInfo info in layerinfos)
            {
                LayerSeed seed = GetLayerByName(seeds, info.Name);
                if (seed == null)
                {
                    throw new Exception("Could not find layer " + info.Name + " in directory " + info.Directory);
                }
                LayerWrapper toadd = seed.CreateLayerFromSeed(appName, info, index);
                index++;

                toreturn.Add(toadd);
            }


            return toreturn;
        }


        public static List<LayerWrapper> LoadChainFromSeedLayers(
                       string appName,
            List<Layer> layers,
            List<Dictionary<string, object>> parameters)
        {
            List<LayerSeed> seeds = new List<LayerSeed>();
            foreach (Layer l in layers)
            {
                seeds.Add(new LayerSeed(l, ""));
            }

            return LoadChainFromSeedLayers(appName, seeds, parameters);
        }
        private static List<LayerWrapper> LoadChainFromSeedLayers(
            string appName, 
            List<LayerSeed> layers, 
            List<Dictionary<string,object>> parameters)
        {
            var chain = new List<LayerWrapper>();
            for(int i = 0; i < layers.Count; i++)
            {
                LayerSeed layer = layers[i];
                var param = parameters[i];
                LayerInfo info = new LayerInfo(null, layer.Seed.Name, param);
                LayerWrapper toadd = layer.CreateLayerFromSeed( appName, info, i);
                chain.Add(toadd);
            }

            return chain;
        }


        


		//Utils for loading layer chain from disk -----
        //public static List<string> GetAllLayerNames(IEnumerable<string> dirs)
        //{
            
        //    List<string> names = new List<string>();
        //    try
        //    {
        //        List<LayerSeed> seeds = new List<LayerSeed>();
        //        Dictionary<Layer, string> withDir = new Dictionary<Layer, string>();

        //        GetSeedLayersHelper(dirs, seeds, withDir);

        //        foreach (LayerSeed seed in seeds)
        //        {
        //            names.Add(withDir[seed.Seed] + "/" + seed.Seed.Name);
        //        }
        //    }
        //    catch { }

        //    return names;
        //}


        private static List<string> ReadAllLinesInChain(string dir, bool ignoreComments)
        {
            List<string> allLines = new List<string>();
            string[] lines = System.IO.File.ReadAllLines(dir + @"/chain.txt");

            foreach (string currLine in lines)
            {
                string line = currLine.Trim();

                if ( (!ignoreComments || !line.StartsWith("#")) && line.Length > 0)
                {
                    allLines.Add(line);
                }
            }

            return allLines;
        }


		public static void GetLayerNamesAndParameters(string dir, List<string> names, 
			List<Dictionary<string,object>> parameterList){

			try {

                List<string> lines = ReadAllLinesInChain(dir, true);

                foreach (string line in lines)
                {

                    string[] items = line.Split(' ');
                    string filename = items[0];
                    Dictionary<string, Object> parameters = new Dictionary<string, Object>();

                    for (int i = 1; i < items.Length; i++)
                    {
                        string[] keyval = items[i].Split('=');
                        string key = keyval[0];
                        string val = keyval[1];
                        string[] vals = val.Split(',');

                        if (vals.Length > 1)
                        {
                           

                            parameters.Add(key, new List<string>(vals));
                        }
                        else
                        {
                            if (key.Equals("libraries"))
                            {
                                vals = new string[] { val };
                                parameters.Add(key, new List<string>(vals));
                            }else

                                parameters.Add(key, val);

                        }
                    }
                    parameterList.Add(parameters);
                    names.Add(filename);

                }
			}catch(IOException e){
				Console.Error.WriteLine (e.StackTrace);
			}
		}


		private class LayerInfo{

			public readonly Dictionary<string, object> Parameters;
			public readonly string Name;
            public readonly string Directory;

			public LayerInfo(string directory, string name, Dictionary<string, object> parameters){
                Directory = directory;
                Name = name;
				Parameters = new Dictionary<string, object>(parameters);
			}
		}

		private static List<LayerInfo> RecursiveGetLayerInfos(string dir){
			var parameterList = new List<Dictionary<string,object>> ();
			List<string> layernames = new List<string> ();

			GetLayerNamesAndParameters (dir, layernames, parameterList);


			List<LayerInfo> infos = new List<LayerInfo> ();
			for (int i = 0; i < layernames.Count; i++) {

				string subchaindir = FindSubchainDirectory (dir + "/" + layernames [i]);
				if (subchaindir  != null) {
					List<LayerInfo> subchain = RecursiveGetLayerInfos (subchaindir);
					infos.AddRange (subchain);
                }
                else
                {
                    string name = System.IO.Path.GetFileName(layernames[i]);
                    string dirname = dir + "\\" + System.IO.Path.GetDirectoryName(layernames[i]);
                    infos.Add(new LayerInfo(dirname, name, parameterList[i]));
                }
					
			}
				
			return infos;
		}

		private static string FindSubchainDirectory(string subchainName)
        {
			
            //Can specify and absolute or relative path.
            if (Directory.Exists(subchainName))
            {
                return subchainName;
            }


            return null;
		}

        private static string GetLayerBuildDate(string filePath)
        {
            return File.GetLastWriteTimeUtc(filePath).ToFileTime().ToString();
        }


		


		private static List<LayerSeed> GetSeedLayers(IEnumerable<LayerInfo> layerinfos, PythonScriptHost pythonhost){
            var seeds = new List<LayerSeed>();
            var dirs = new List<string>();
            foreach (LayerInfo linfo in layerinfos)
            {
                if (linfo.Directory != null && !dirs.Contains(linfo.Directory))
                    dirs.Add(linfo.Directory);
            }

            



			GetSeedLayersHelper (dirs, layerinfos, pythonhost, seeds);

			return seeds;
		}


        /// <summary>
        /// This method needs updating. 
        /// The assembly needs to be loaded without locking the file,
        /// there's an appropriate solution to this, but I'm just hacking it for now.
        /// See this thread.
        /// http://stackoverflow.com/questions/1031431/system-reflection-assembly-loadfile-locks-file
        /// </summary>
        /// <param name="dirs"></param>
        /// <param name="seeds"></param>
        /// <param name="seedDirs"></param>
		private static void GetSeedLayersHelper(IEnumerable<string> dirs, IEnumerable<LayerInfo> infos, 
            PythonScriptHost pythonhost,
            List<LayerSeed> seeds, Dictionary<Layer, string> seedDirs = null){

            
            foreach (string dir in dirs)
            {
                string[] files = Directory.GetFiles(dir, "*.dll");
                GetDllLayers(files, seeds, dir);


               
                var pyinfos = infos.Where(i => i.Directory.Equals(dir) && i.Name.Contains(".py"));
                var pythonfiles = new List<string>();
                foreach (var info in pyinfos)
                {
                    pythonfiles.Add(info.Directory + "/" + info.Name);
                }
                GetPythonLayers(pythonfiles, seeds, pythonhost);

                if (seedDirs != null)
                {
                    foreach (LayerSeed seed in seeds)
                        seedDirs[seed.Seed] = dir;
                }
            }
		}

        private static void GetPythonLayers(IEnumerable<string> files, List<LayerSeed> seeds, PythonScriptHost host)
        {
            foreach (string file in files)
            {
                string name = System.IO.Path.GetFileName(file);
                string path = System.IO.Path.GetFullPath(file);
                path = System.IO.Path.GetDirectoryName(path);
                host.AddPath(path);
                
                PythonLayer layer = new PythonLayer(host, File.ReadAllText(file), name);
                seeds.Add(new LayerSeed(layer, file));
            }
        }

        private static void GetDllLayers(IEnumerable<string> files, List<LayerSeed> seeds, string dir, Dictionary<Layer, string> seedDirs = null)
        {
            foreach (string dll in files)
            {


                manager.LoadFrom(dll);

                List<Layer> dllseeds = manager.GetLayerSeeds();

                List<LayerSeed> toreturn = new List<LayerSeed>();
                string fullDllPath = System.IO.Path.GetFullPath(dll);

                foreach (Layer l in dllseeds)
                {
                    seeds.Add(new LayerSeed(l, fullDllPath));

                }

                //AssemblyName assemblyName = AssemblyName.GetAssemblyName(dll);
                //Assembly asm = Assembly.Load(System.IO.File.ReadAllBytes(dll));

                ////Assembly asm = Assembly.Load(assemblyName);



                //Layer layer = null;

                //foreach (Type t in asm.GetTypes())
                //{
                //    if (t.GetInterface("Layer") != null)
                //    {
                //        layer = (Layer)Activator.CreateInstance(t);
                //        seeds.Add(new LayerSeed(layer, dll));


                //    }
                //}
            }
        }

        private class LayerSeed
        {
            public readonly string LayerDllLocation;

            public Layer Seed
            {
                get;
                private set;
            }
            public LayerSeed(Layer seed, string dll)
            {
                Seed = seed;
                LayerDllLocation = dll;
            }


            public LayerWrapper CreateLayerFromSeed(string chainName, LayerInfo info, int layerIndex)
            {

                Layer toAdd = CopyLayer( Seed);
                string buildDate = GetLayerBuildDate(LayerDllLocation);

                try
                {

                    Dictionary<string, object> parameters = info.Parameters;
                    string intentFilename = chainName + "_" + info.Name + "_" + layerIndex;
                    IRuntimeStorage intent = RuntimeStorage.FromCouchDb(intentFilename);
                    parameters.Add("intent", intent);
                    parameters.Add("shared", sharedLayerData);
                    toAdd.Init(parameters);
                    IEnumerable<string> libnames = toAdd.AnnotationLibraries();

                    
                    LayerWrapper wrapper = new LayerWrapper(toAdd, parameters, intent, buildDate);
                    return wrapper;
                }
                catch (IOException e)
                {
                    Console.WriteLine("Could not load layer: " + info.Name);
                    Console.WriteLine("Error Message: " + e.Message);
                    Console.WriteLine("Stack Trace: " + e.StackTrace);
                    return null;
                }

            }
        }




		private static Layer CopyLayer( Layer seed){
			try {
                Layer copy = null;
                if (seed is PythonLayer)
                {
                    copy = new PythonLayer(((PythonLayer)seed).Host, ((PythonLayer)seed).Code, seed.Name);
                }else
				     copy = (Layer)Activator.CreateInstance (seed.GetType());
				return copy;

			} catch (Exception e){
				Console.Error.WriteLine (e.StackTrace);
			}

			return null;
		}


		private static LayerSeed GetLayerByName(IEnumerable<LayerSeed> layers, string name){
			foreach(LayerSeed l in layers){
				if(l.Seed.Name.Equals(name))
					return l;
			}

			return null;
		}



	}
}

