using Newtonsoft.Json.Linq;
using Prefab;
using PrefabSingle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrefabSingle
{
    public class ImageAnnotationWrapper : IAnnotation
    {
        private ImageAnnotation _annotation;

        public ImageAnnotationWrapper(ImageAnnotation annotation)
        {
            _annotation = annotation;
        }

        public string Id
        {
            get { return _annotation.Id; }
        }

        public JToken Data
        {
            get { return _annotation.Data; }
        }
    }
}
