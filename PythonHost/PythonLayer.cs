using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using IronPython.Hosting;
using IronPython.Runtime;
using System.Reflection;
using IronPython.Runtime.Types;
using System.Threading.Tasks;
using Prefab;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace PythonHost
{
    public class PythonLayer : Layer
    {       
        private static readonly string _interpret = "interpret";
        private static readonly string _processannotations = "generalize_annotations";
        //private static readonly string _annotationlibs = "get_annotation_libraries";
        private static readonly string _afterinterpret = "after_interpret";
        //private static readonly string _init = "init";

        private PythonDictionary _parameters;
        private ScriptScope _scope;
        private PythonAnnotatedNodeArgs _annotionArgs;
        private dynamic _interpretFunc;
        private dynamic _processAnnotationsFunc;
        //private dynamic _getAnnotationLibsFunc;
        private dynamic _afterInterpretFunc;
        //private dynamic _initFunc;

        private string _id;

        public readonly string Code;

        public void AfterInterpret(Tree tree)
        {
            if (_afterInterpretFunc != null)
            {
                _afterInterpretFunc(tree);
            }
        }

        public PythonScriptHost Host
        {
            get;
            private set;
        }


        public PythonLayer(PythonScriptHost host, string code, string name)
        {
            Name = name;
            Code = code;
            Host = host;

            string[] codeWithoutWhitespace = code.Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string alltokens = "";

            foreach (string token in codeWithoutWhitespace)
            {
                alltokens += token;
            }

            _id = alltokens.GetHashCode().ToString();

                _scope = host.CreateScriptSource(code, name);

                _annotionArgs = new PythonAnnotatedNodeArgs();

                if (_scope.ContainsVariable(_processannotations))
                {
                    _processAnnotationsFunc = _scope.GetVariable(_processannotations);
                }

                if (_scope.ContainsVariable(_interpret))
                {
                    _interpretFunc = _scope.GetVariable(_interpret);
                }

                //if (_scope.ContainsVariable(_annotationlibs))
                //{
                //    _getAnnotationLibsFunc = _scope.GetVariable(_annotationlibs);
                //}

                if (_scope.ContainsVariable(_afterinterpret))
                {
                    _afterInterpretFunc = _scope.GetVariable(_afterinterpret);
                }

                //if (_scope.ContainsVariable(_init))
                //{
                //    _initFunc = _scope.GetVariable(_init);
                //}


        }





        public string GetId()
        {
            return _id;
        }


        public IEnumerable<string> AnnotationLibraries()
        {
            //if (_getAnnotationLibsFunc != null)
            //{
            //    List list = _getAnnotationLibsFunc();
            //    List<string> csharplist = new List<string>();
            //    foreach (string lib in list)
            //        csharplist.Add(lib);
            //    return csharplist;
            //}
            List<string> libraries = new List<string>();
            if (_parameters.Contains("libraries"))
                foreach (string lib in (IEnumerable<string>)_parameters["libraries"])
                    libraries.Add(lib);

            return libraries;
        }

        public void Close()
        {
            //TODO: Support this in python
        }

        public void Init(Dictionary<string, object> parameters)
        {
            PythonDictionary pydict = new PythonDictionary();
            foreach (string key in parameters.Keys)
                pydict[key] = parameters[key];

            _parameters = pydict;
            //if (_initFunc != null)
            //    _initFunc(pydict);
        }


        public void Interpret(InterpretArgs args)
        {
            try
            {
                PythonInterpretArgs pargs = new PythonInterpretArgs(args);
                if (_interpretFunc != null)
                {
                    _scope.Engine.Operations.Invoke(_interpretFunc, pargs);
                }

            }
            catch(Exception e)
            {
                throw PythonScriptHost.Instance.GetFormattedException(e, Name);
            }
            
        }

        public string Name
        {
            get;
            private set;
        }



        public void ProcessAnnotations(AnnotationArgs args)
        {
            if (_processAnnotationsFunc != null)
            {
                try
                {
                    _annotionArgs.Args = args;
                    _annotionArgs.runtime_storage = new PythonRuntimeStorageWrapper(args.RuntimeStorage);
                    _processAnnotationsFunc(_annotionArgs);
                }
                catch (Exception e)
                {
                    throw PythonScriptHost.Instance.GetFormattedException(e, Name);
                }
            }

        }
    }
    
}
