using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronPython.Hosting;
using IronPython.Runtime.Types;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.Reflection;
using System.IO;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;
using IronPython.Compiler;
using IronPython.Compiler.Ast;
namespace PythonHost
{
    public class PythonScriptHost
    {


        public static readonly PythonScriptHost Instance = new PythonScriptHost();
        private ScriptEngine _python;
        private string _workingDirectory;
        private MemoryStream _consoleOutput;

        public string ReadConsoleOutput()
        {
            string output = ReadFromMemoryStream(_consoleOutput);
            return output;
        }

        public Exception GetFormattedException(Exception e, string scriptname)
        {
            string exception = _python.GetService<ExceptionOperations>().FormatException(e);
            exception = exception.Replace("Traceback (most recent call last):", "").Trim();
            exception = exception.Replace("File \"<string>\"", "File \"" + scriptname + "\"");
            exception = exception.Replace("File \"\"", "File \"" + scriptname + "\"");
            return new Exception(exception);
        }

        private static string ReadFromMemoryStream(MemoryStream ms)
        {

            int length = (int)ms.Length;
            if (length > 0)
            {
                Byte[] bytes = new Byte[length];

                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(bytes, 0, length);

                return Encoding.Default.GetString(bytes, 0, (int)ms.Length);
            }

            return "";
        }

        public PythonScriptHost(bool debugmode = false)
        {

            ResetEngine(debugmode);
            //ScriptRuntimeSetup setup = new ScriptRuntimeSetup();
            //setup.DebugMode = debugmode;
            //setup.LanguageSetups.Add(Python.CreateLanguageSetup(null));
            //ScriptRuntime runtime = new ScriptRuntime(setup);


            //_python = runtime.GetEngineByTypeName(typeof(PythonContext).AssemblyQualifiedName);
            List path = _python.Runtime.GetSysModule().GetVariable("path");
            
            string standardlib = @"C:\Program Files\IronPython 2.7\Lib";
            if (!path.Contains(standardlib))
            {
                path.append(standardlib);
            }

            standardlib = standardlib + @"\site-packages";
            if (!path.Contains(standardlib))
            {
                path.append(standardlib);
            }

            standardlib = @"C:\Python27\Lib";

            if (!path.Contains(standardlib))
            {
                path.append(standardlib);
            }

            
            //_python.Runtime.GetClrModule().GetVariable("AddReference")("Prefab");
            

            
        }

        


        public void AddPath(string path)
        {
            List pythonpaths = _python.Runtime.GetSysModule().GetVariable("path");

            if (!pythonpaths.Contains(path))
                pythonpaths.append(path);
        }
        public ScriptScope CreateScriptSource(string code, string name)
        {
            try
            {
                code = code.Replace("\r\n", "\n");
                ScriptSource script = _python.CreateScriptSourceFromString(code);
                string[] removewhitespace = code.Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                CompiledCode compiled = script.Compile();
                compiled.Execute();

                return compiled.DefaultScope;
            }
            catch (Exception e)
            {
                throw GetFormattedException(e, name);
            }
        }

        public void ResetEngine(bool debugmode = false)
        {
            if(_python != null)
                 _python.Runtime.Shutdown();

            ScriptRuntimeSetup setup = new ScriptRuntimeSetup();
            setup.DebugMode = debugmode;
            setup.LanguageSetups.Add(Python.CreateLanguageSetup(null));
            ScriptRuntime runtime = new ScriptRuntime(setup);

            _python = runtime.GetEngineByTypeName(typeof(PythonContext).AssemblyQualifiedName);
            LoadAssembly(typeof(Prefab.Bitmap).Assembly);
            LoadAssembly(typeof(PrefabUtils.PathDescriptor).Assembly);

            if(_consoleOutput == null)
                 _consoleOutput = new MemoryStream();
            _python.Runtime.IO.SetOutput(_consoleOutput, Encoding.Default);
            // _python.Runtime.GetClrModule().GetVariable("AddReference")("Prefab");
            //_python = runtime.GetEngineByTypeName(typeof(PythonContext).AssemblyQualifiedName);
            //Instance = new PythonScriptHost(false);

            
        }

        public void LoadAssembly(Assembly assembly)
        {
            _python.Runtime.LoadAssembly(assembly);
        }
        
        public void SetWorkingDirectory(string workingDirectory = null)
        {

            if (_workingDirectory != null)
            {
                ICollection<string> paths = _python.GetSearchPaths();
                paths.Remove(_workingDirectory);
                _python.SetSearchPaths(paths);
            }

            if (workingDirectory != null)
            {
                ICollection<string> paths = _python.GetSearchPaths();
                paths.Add(workingDirectory);
                _python.SetSearchPaths(paths);
            }

            _workingDirectory = workingDirectory;
        }



        public void ClearConsoleOutput()
        {
            _consoleOutput.Close();
            _consoleOutput = new MemoryStream();
            _python.Runtime.IO.SetOutput(_consoleOutput, Encoding.Default);
        }
    }
}
