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
using PythonHost;

namespace PrefabSingle
{
    public class PythonSingleLayer
    {

        
        private static readonly string _interpret = "interpret";
        private static readonly string _processannotations = "process_annotations";
        //private static readonly string _annotationlibs = "get_annotation_libraries";
        //private static readonly string _afterinterpret = "after_interpret";
        //private static readonly string _init = "init";

        private PythonDictionary _parameters;
        private ScriptScope _scope;
        
        private dynamic _interpretFunc;
        private dynamic _processAnnotationsFunc;
        //private dynamic _getAnnotationLibsFunc;
        private dynamic _afterInterpretFunc;
        //private dynamic _initFunc;

        private string _id;

        public readonly string Code;


        public PythonScriptHost Host
        {
            get;
            private set;
        }


        public PythonSingleLayer(PythonScriptHost host, string code, string name)
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

            if (_scope.ContainsVariable(_processannotations))
            {
                _processAnnotationsFunc = _scope.GetVariable(_processannotations);
            }

            if (_scope.ContainsVariable(_interpret))
            {
                _interpretFunc = _scope.GetVariable(_interpret);
            }

        }





        public string GetId()
        {
            return _id;
        }


        public void Interpret(PrefabSingleInterpretArgs args)
        {
            try
            {
                
                if (_interpretFunc != null)
                {
                    _scope.Engine.Operations.Invoke(_interpretFunc, args);
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



        public void ProcessAnnotations(ProcessAnnotationArgs args)
        {
            if (_processAnnotationsFunc != null)
            {
                try
                {
                    var runtime_storage = new PythonRuntimeStorageWrapper(args.RuntimeStorage);
                    _processAnnotationsFunc(runtime_storage);
                }
                catch (Exception e)
                {
                    throw PythonScriptHost.Instance.GetFormattedException(e, Name);
                }
            }

        }

        
    }
}
