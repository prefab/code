using Prefab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Prefab
{
    [Serializable]
    internal class Scanner : MarshalByRefObject
    {
        private readonly List<Layer> _plugins;

        public List<Layer> GetSeeds()
        {
            return new List<Layer>(_plugins);
        }

        public Scanner()
        {
            _plugins = new List<Layer>();
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }


        private void GetAllPlugins(AppDomain domain)
        {
            var pluginType = typeof(Layer);

            var types = domain.GetAssemblies()
                              .SelectMany(a => a.GetTypes())
                              .Where(t => t.GetInterface(pluginType.Name) != null);

            var ctors = types.Select(t => t.GetConstructor(new Type[] { }))
                             .Where(c => c != null);

            _plugins.Clear();
            _plugins.AddRange(ctors.Select(c => c.Invoke(null))
                                   .Cast<Layer>());
        }

        public void Setup()
        {
            GetAllPlugins(AppDomain.CurrentDomain);
        }

        public void DoWork()
        {

        }

        public void Teardown()
        {

        }

        public void Load(string name)
        {
            AssemblyName assemblyname = AssemblyName.GetAssemblyName(name);
            Assembly.Load(assemblyname);
        }

        public Layer ShowCrossDomainPollutionExceptions()
        {
            return _plugins.First();
        }
    }
}
