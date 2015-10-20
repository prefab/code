using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prefab
{

    public sealed class ReadOnlyDictionary<K, V> : IDictionary<K, V>
    {

        public static readonly ReadOnlyDictionary<K, V> Empty = 
            new ReadOnlyDictionary<K, V>(new Dictionary<K, V>());

        private Dictionary<K, V> _dict;


        private ReadOnlyDictionary() { }
        
        public ReadOnlyDictionary(IEnumerable<KeyValuePair<object, object>> keyvalues)
        {
            _dict = new Dictionary<K, V>();
            foreach (KeyValuePair<object, object> pair in keyvalues)
            {
                _dict[(K)pair.Key] = (V)pair.Value;
            }
        }

        public ReadOnlyDictionary(IEnumerable<KeyValuePair<K,V>> keyvalues)
        {
            _dict = new Dictionary<K, V>();
            foreach (KeyValuePair<K, V> pair in keyvalues)
            {
                _dict.Add(pair.Key, pair.Value);
            }
        }



        public ReadOnlyDictionary<K, V> Set(K key, V value)
        {
            Dictionary<K,V> cpy = new Dictionary<K,V>(_dict);
            cpy[key] = value;

            ReadOnlyDictionary<K, V> newdict = new ReadOnlyDictionary<K, V>();
            newdict._dict = cpy;

            return newdict;
        }

        public ReadOnlyDictionary<K, V> Set(IEnumerable<KeyValuePair<K,V>> pairs)
        {
            Dictionary<K, V> cpy = new Dictionary<K, V>(_dict);
            foreach (KeyValuePair<K, V> pair in pairs)
            {
                cpy[(K)pair.Key] = (V)pair.Value;
            }

            ReadOnlyDictionary<K, V> newdict = new ReadOnlyDictionary<K, V>();
            newdict._dict = cpy;

            return newdict;
        }

        public void Add(K key, V value)
        {
            throw new NotSupportedException("This dictionary is read-only");
        }

        public bool ContainsKey(K key)
        {
            return _dict.ContainsKey(key);
        }

        public ICollection<K> Keys
        {
            get { return _dict.Keys; }
        }

        public bool Remove(K key)
        {
            throw new NotSupportedException("This dictionary is read-only");
        }

        public bool TryGetValue(K key, out V value)
        {
            return _dict.TryGetValue(key, out value);
        }

        public ICollection<V> Values
        {
            get { return _dict.Values; }
        }

        public V this[K key]
        {
            get
            {
                return _dict[key];
            }
            set
            {
                throw new NotSupportedException("This dictionary is read-only");
            }
        }

        public void Add(KeyValuePair<K, V> item)
        {
            throw new NotSupportedException("This dictionary is read-only");
        }

        public void Clear()
        {
            throw new NotSupportedException("This dictionary is read-only");
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            return _dict.Contains(item);
        }


        public int Count
        {
            get { return _dict.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            throw new NotSupportedException("This dictionary is read-only");
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _dict.GetEnumerator();
        }


        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            ((IDictionary<K, V>)_dict).CopyTo(array, arrayIndex);
        }
    }

}
