using Prefab;
using PrefabIdentificationLayers.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrefabSingle
{
    public interface PrefabInterpretationLogic
    {

        string LayerDirectory
        {
            get;
        }
        void Load();
        Tree Interpret(Tree frame);

        string GetPtypeDatabase();

        void UpdateLogic();

        IEnumerable<Ptype> GetPtypes();

         Dictionary<string, List<IAnnotation>> GetAnnotationsMatchingNode(Tree node, Tree root, string bitmapid);

        IEnumerable<IRuntimeStorage> GetRuntimeStorages();

        //Dictionary<string, List<IAnnotation>> GetAllAnnotations();

        IEnumerable<string> GetAnnotationLibraries();
    }
}
