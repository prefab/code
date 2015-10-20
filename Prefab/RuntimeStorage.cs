using System;
using System.Collections.Generic;
using LoveSeat;
using Newtonsoft.Json;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Prefab
{
	public class RuntimeStorage
	{
		public static readonly string DATABASE = "prefab-runtime-intents";

		private Dictionary<string, JToken> data;
		private string filename;



		private RuntimeStorage (){}

		public static IRuntimeStorage FromCouchDb(string name)
		{
			try {
				return new CouchDbIntent (name);
			} catch (HttpException e) {
				Console.WriteLine ("\nError Connecting to CouchDB. Make sure CouchDB is running.\n");
				throw e;
			}
		}

		private void putData(string key, JToken value)
        {
			data[key] = value;
		}

        private bool ContainsKey(string key)
        {
            return data.ContainsKey(key);
        }

		private JToken getData(string key)
        {
			JToken value = null;
			data.TryGetValue (key, out value);

			return value;

		}

        public void deleteData(string key)
        {
            data.Remove(key);
        }

		public static void ToCouchDb(RuntimeStorage intent){

			CouchClient client = new CouchClient ();
			if (!client.HasDatabase (DATABASE))
				client.CreateDatabase (DATABASE);

			CouchDatabase db = client.GetDatabase(DATABASE);
			CouchDbIntentData dbIntent;
			try{

				Document doc = db.GetDocument(intent.filename);
				dbIntent = new CouchDbIntentData();
				dbIntent.data = intent.data;

				Document<CouchDbIntentData> tosave = new Document<CouchDbIntentData>(dbIntent);
				tosave.Id = doc.Id;
				tosave.Rev = doc.Rev;
				db.SaveDocument(tosave);

			}catch{

				dbIntent = new CouchDbIntentData ();
				dbIntent.data = intent.data;
				Document<CouchDbIntentData> tosave = new Document<CouchDbIntentData>(dbIntent);
				tosave.Id = intent.filename;

				db.CreateDocument (tosave);
			}
		}


		private class CouchDbIntent : IRuntimeStorage
		{
			private RuntimeStorage _intent;


			public CouchDbIntent(string name){
				_intent = new RuntimeStorage();
				_intent.filename = name;
				_intent.data = new Dictionary<string, JToken>();

				CouchClient client = new CouchClient();

				if(!client.HasDatabase(DATABASE))
					client.CreateDatabase(DATABASE);

				CouchDatabase database = client.GetDatabase(DATABASE);

				CouchDbIntentData data = null;
				try{
					Document d = database.GetDocument(name);
					data = JsonConvert.DeserializeObject<CouchDbIntentData>(d.ToString());
				}catch{

				}

				if(data == null)
				{
					data = new CouchDbIntentData();

					data.data = new Dictionary<string, JToken>();
					Document<CouchDbIntentData> tosave = new Document<CouchDbIntentData>(data);
					tosave.Id = name;
					database.CreateDocument( tosave );
				}

                if (data.data != null)
                {
                    foreach (var entry in data.data)
                    {
                        _intent.data.Add(entry.Key, entry.Value);
                    }
                }
			}

			public void PutData(string key, JToken value) {
				_intent.putData(key, value);
				RuntimeStorage.ToCouchDb(_intent);
			}


			public JToken GetData(string key) {
				return _intent.getData(key);
			}

            public void DeleteData(string key)
            {
                _intent.deleteData(key);
                RuntimeStorage.ToCouchDb(_intent);
            }

            public Dictionary<string, JToken> ReadAllData()
            {
                return new Dictionary<string, JToken>(_intent.data);
            }

            public int Count()
            {
                return _intent.data.Count;
            }


            public IEnumerable<string> Keys
            {
                get { return _intent.data.Keys; }
            }

            public IEnumerable<object> Values
            {
                get { return _intent.data.Values; }
            }

            public bool ContainsKey(string key)
            {
                return _intent.ContainsKey(key);
            }

            public void Clear()
            {
                _intent.data.Clear();
                RuntimeStorage.ToCouchDb(_intent);
            }
        }




	}




}
