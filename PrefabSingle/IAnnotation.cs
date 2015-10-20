using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrefabSingle
{
    public interface IAnnotation
    {
         string Id { get; }

         JToken Data { get; }
    }
}
