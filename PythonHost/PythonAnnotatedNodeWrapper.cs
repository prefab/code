using IronPython.Runtime;
using Newtonsoft.Json.Linq;
using Prefab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PythonHost
{
    public class PythonAnnotatedNodeWrapper
    {
        private AnnotatedNode _node;
        public PythonDictionary data;
        public Tree node
        {
            get { return _node.MatchingNode; }
        }

        public Tree root
        {
            get { return _node.Root; }
        }

        public string image_id
        {
            get { return _node.ImageId; }
        }

        public IBoundingBox region
        {
            get { return _node.Region; }
        }

        public PythonAnnotatedNodeWrapper(AnnotatedNode node)
        {
            _node = node;

            data = new PythonDictionary();
            JObject jdata = node.Data;
            foreach (var item in jdata.Properties())
            {
                data[item.Name] = item.Value.ToString();
            }
            
           
        }


        

    }
}
