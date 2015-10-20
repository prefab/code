using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace SavedVideoInterpreter
{
    public class VirtualizingCollection<T> : IList<T>, IList, INotifyCollectionChanged
    {


        private Func<int, int, IList<T>> _getter;
        private int _count;
        public int PageSize
        {
            get;
            private set;
        }

        public int PageTimeout
        {
            get;
            private set;
        }

        private readonly Dictionary<int, IList<T>> _pages =
        new Dictionary<int, IList<T>>();

        private readonly Dictionary<int, DateTime> _pageTouchTimes =
                new Dictionary<int, DateTime>();

        protected virtual void RequestPage(int pageIndex)
        {
            if (!_pages.ContainsKey(pageIndex))
            {
                _pages.Add(pageIndex, null);
                _pageTouchTimes.Add(pageIndex, DateTime.Now);
                LoadPage(pageIndex);
            }
            else
            {
                _pageTouchTimes[pageIndex] = DateTime.Now;
            }
        }

        protected virtual void PopulatePage(int pageIndex, IList<T> page)
        {
            if (_pages.ContainsKey(pageIndex))
                _pages[pageIndex] = page;
        }

        public void CleanUpPages()
        {
            List<int> keys = new List<int>(_pageTouchTimes.Keys);
            foreach (int key in keys)
            {
                // page 0 is a special case, since the WPF ItemsControl
                // accesses the first item frequently
                if (key != 0 && (DateTime.Now -
                     _pageTouchTimes[key]).TotalMilliseconds > PageTimeout)
                {
                    _pages.Remove(key);
                    _pageTouchTimes.Remove(key);
                }
            }
        }

        public VirtualizingCollection(int count, Func<int,int, IList<T>> getter)
        {
            _getter = getter;
            _count = count;
            PageSize = 30;
            PageTimeout = 10000;
        }


        public T this[int index]
        {
            get
            {
                // determine which page and offset within page
                int pageIndex = index / PageSize;
                int pageOffset = index % PageSize;

                // request primary page
                RequestPage(pageIndex);

                // if accessing upper 50% then request next page
                if (pageOffset > PageSize / 2 && pageIndex < Count / PageSize)
                    RequestPage(pageIndex + 1);

                // if accessing lower 50% then request prev page
                if (pageOffset < PageSize / 2 && pageIndex > 0)
                    RequestPage(pageIndex - 1);

                // remove stale pages
                CleanUpPages();

                // defensive check in case of async load
                if (_pages[pageIndex] == null)
                    return default(T);

                // return requested item
                return _pages[pageIndex][pageOffset];
            }
            set { throw new NotSupportedException(); }
        }

        protected virtual void LoadPage(int pageIndex)
        {
            PopulatePage(pageIndex, FetchPage(pageIndex));
        }

        protected IList<T> FetchPage(int pageIndex)
        {
            return _getter(pageIndex * PageSize, PageSize);
        }

        public int Count
        {
            get
            {
                return _count;
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            return false;
        }

        public int IndexOf(object value)
        {

            for (int i = 0; i < Count; i++)
            {

                if (this[i] != null && this[i].Equals(value))
                    return i;
            }

            return -1;
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public bool IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public bool IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        public object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerator GetEnumerator()
        {
            return new FrameEnumerator(this);
        }

        private class FrameEnumerator : IEnumerator<T>
        {
            private int _index;
            private VirtualizingCollection<T> _collection;

            public FrameEnumerator(VirtualizingCollection<T> collection)
            {
                _index = 0;
                _collection = collection;
            }

            public T Current
            {
                get { return _collection[_index]; }
            }

            public void Dispose()
            {
                
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                _index++;
                if (_index >= _collection.Count)
                    return false;

                return true;
            }

            public void Reset()
            {
                _index = 0;
            }
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new FrameEnumerator(this);
        }


        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
