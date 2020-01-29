using System.Collections;
using System.Collections.Generic;

namespace Unity.SelectionGroups
{
    public class UniqueList<T> : IList<T>
    {
        List<T> items = new List<T>();
        HashSet<T> uniqueIndex = new HashSet<T>();

        public T this[int index] { get => ((IList<T>)items)[index]; set => ((IList<T>)items)[index] = value; }

        public int Count => ((IList<T>)items).Count;

        public bool IsReadOnly => ((IList<T>)items).IsReadOnly;

        public void Add(T item)
        {
            if(!uniqueIndex.Contains(item)) {
                ((IList<T>)items).Add(item);
                uniqueIndex.Add(item);
            }
        }

        public void Clear()
        {
            ((IList<T>)items).Clear();
            uniqueIndex.Clear();
        }

        public bool Contains(T item)
        {
            return uniqueIndex.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((IList<T>)items).CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IList<T>)items).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return ((IList<T>)items).IndexOf(item);
        }

        public void AddRange(IEnumerable<T> objectReferences)
        {
            foreach(var i in objectReferences) Add(i);
        }

        public void Insert(int index, T item)
        {
            if(!uniqueIndex.Contains(item)) {
                ((IList<T>)items).Insert(index, item);
                uniqueIndex.Add(item);
            }
        }

        public bool Remove(T item)
        {
            uniqueIndex.Remove(item);
            return ((IList<T>)items).Remove(item);
        }

        public void RemoveAt(int index)
        {
            var item = items[index];
            uniqueIndex.Remove(item);
            ((IList<T>)items).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<T>)items).GetEnumerator();
        }
    }
}