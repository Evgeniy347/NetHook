using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetHook.Cores.Handlers
{
    public class ConcurentList<T> : IList<T>
    {
        private List<T> _list = new List<T>();
        private object _lock = new object();

        public T this[int index]
        {
            get
            {
                lock (_lock) return _list[index];
            }
            set
            {
                lock (_lock)
                    _list[index] = value;
            }
        }

        public int Count => _list.Count;

        public bool IsReadOnly => ((IList)_list).IsReadOnly;

        public void Add(T item)
        {
            lock (_lock)
                _list.Add(item);
        }

        public void Clear()
        {
            lock (_lock)
                _list.Clear();
        }

        public bool Contains(T item)
        {
            lock (_lock)
                return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_lock)
                _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_lock)
                return _list.ToList().GetEnumerator();
        }

        public int IndexOf(T item)
        {
            lock (_lock)
                return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            lock (_lock)
                _list.Insert(index, item);
        }

        public bool Remove(T item)
        {
            lock (_lock)
                return _list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            lock (_lock)
                _list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_lock)
                return _list.ToArray().GetEnumerator();
        }
    }
}
