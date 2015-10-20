using IronPython.Runtime;
using Newtonsoft.Json.Linq;
using Prefab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PythonHost
{
    public class PythonRuntimeStorageWrapper : PythonDictionary
    {

        public override void __delitem__(params object[] key)
        {
            foreach (string str in key)
                delete_data(str);
        }

        public override List items()
        {
            List l = new List();
            foreach (var key in RuntimeStorage.Keys)
            {
                l.Add(key);
                
            }

            return l;
        }
        public  override bool __contains__(object key)
        {
            return has_key(key.ToString());
        }


        public override  void __delitem__(object key)
        {
            delete_data(key.ToString());
        }

        public override int __len__()
        {
            return RuntimeStorage.Count();
        }

        public override object __iter__()
        {
            return RuntimeStorage.Keys.GetEnumerator();
        }

        public override List keys()
        {
            List l = new List();
            foreach (var key in RuntimeStorage.Keys)
                l.Add(key);

            return l;
        }

        public override List values()
        {
            List l = new List();
            foreach (var val in RuntimeStorage.Values)
                l.Add(val);

            return l;

        }

        public new void clear()
        {
            RuntimeStorage.Clear();
        }
        

        public override  object this[object key]
        {
            get
            {
                return get_data(key.ToString());
            }
            set
            {
                if (value is PythonDictionary)
                    put_data(key.ToString(), (PythonDictionary)value);
                else
                    throw new Exception("Currently runtime_storage can only store dicts. And those dicts must have string keys and values");
            }
        }

        public IRuntimeStorage RuntimeStorage
        {
            get;
            private set;
        }

        public PythonRuntimeStorageWrapper(IRuntimeStorage storage)
        {

            RuntimeStorage = storage;
            
        }

        public PythonDictionary get_data(string key)
        {

            JToken tok = RuntimeStorage.GetData(key);

            if (tok != null)
            {
                JObject obj = tok as JObject;
                PythonDictionary dict = new PythonDictionary();

                foreach (var item in obj.Properties())
                {
                    dict[item.Name] = item.Value.ToString();
                }
                return dict;
            }

            return null;
        }

        public void put_data(string key, PythonDictionary data)
        {
            JObject obj = new JObject();
            foreach (string itemname in data.Keys)
            {
                obj[itemname] = data[itemname].ToString(); 
            }

            RuntimeStorage.PutData(key, obj);
        }


        public bool has_key(string key)
        {
            return RuntimeStorage.ContainsKey(key);
        }

        public void delete_data(string key)
        {
            RuntimeStorage.DeleteData(key);
            
        }

        public PythonDictionary read_all_data()
        {
            var data = RuntimeStorage.ReadAllData();

            PythonDictionary dict = new PythonDictionary();

            foreach (var d in data)
            {
                PythonDictionary datadict = new PythonDictionary();
                JObject obj = d.Value as JObject;
                foreach (var item in obj.Properties())
                {
                    datadict[item.Name] = item.Value.Value<string>();
                }

                dict[d.Key] = datadict;
            }

            return dict;
        }

        
    }
}
