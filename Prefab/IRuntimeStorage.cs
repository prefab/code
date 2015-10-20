using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Prefab
{
	public interface IRuntimeStorage
	{
		void PutData(string key, JToken value);
		JToken GetData(string key);

        void DeleteData(string key);

        Dictionary<string, JToken> ReadAllData();

        bool ContainsKey(string key);

        void Clear();

        int Count();

        IEnumerable<string> Keys { get; }

        IEnumerable<object> Values { get; }
	}
}

