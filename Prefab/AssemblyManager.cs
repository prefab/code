using Prefab;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prefab
{
    public class AssemblyManager
    {
        private AppDomain _domain;
        private Scanner _scanner;

        public void LoadFrom(string path)
        {
            //if (_domain != null)
            //{
            //    _scanner.Teardown();
            //    Console.WriteLine("unloading " + _domain.FriendlyName);
            //    AppDomain.Unload(_domain);
            //}

            var name = Path.GetFileNameWithoutExtension(path);
            var dirPath = Path.GetFullPath(Path.GetDirectoryName(path));

            var setup = new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                PrivateBinPath = dirPath,
                ShadowCopyFiles = "true",
                ShadowCopyDirectories = dirPath,
                
            };

            _domain = AppDomain.CreateDomain(name + "Domain", AppDomain.CurrentDomain.Evidence, setup);
            Console.WriteLine("loading " + _domain.FriendlyName);
            var scannerType = typeof(Scanner);
            _scanner = (Scanner)_domain.CreateInstanceAndUnwrap(scannerType.Assembly.FullName, scannerType.FullName);
            _scanner.Load(path);
            _scanner.Setup();


            
        }

        public List<Layer> GetLayerSeeds()
        {
            return _scanner.GetSeeds();
        }

        public Layer ShowCrossDomainPollutionExceptions()
        {
            return _scanner.ShowCrossDomainPollutionExceptions();
        }
    }
}
