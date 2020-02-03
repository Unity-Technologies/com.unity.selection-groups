using System.Collections;
using System.Collections.Generic;

namespace Unity.SelectionGroups
{
    /// <summary>
    /// A HashSet which retains order of items as they are added or inserted.
    /// or
    /// A List which only contains unique references.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class OrderedSet<T> : IList<T>
    {
        List<T> items = new List<T>();
        HashSet<T> uniqueIndex = new HashSet<T>();

        public T this[int index] { get => items[index]; set => items[index] = value; }

        public int Count => items.Count;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            if(!uniqueIndex.Contains(item)) {
                items.Add(item);
                uniqueIndex.Add(item);
            }
        }

        public void Clear()
        {
            items.Clear();
            uniqueIndex.Clear();
        }

        public bool Contains(T item)
        {
            return uniqueIndex.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return items.IndexOf(item);
        }

        public void AddRange(IEnumerable<T> objectReferences)
        {
            foreach(var i in objectReferences) 
                Add(i);
        }

        public void Insert(int index, T item)
        {
            if(!uniqueIndex.Contains(item)) {
                items.Insert(index, item);
                uniqueIndex.Add(item);
            }
        }

        public bool Remove(T item)
        {
            uniqueIndex.Remove(item);
            return items.Remove(item);
        }

        public void Remove(T[] items)
        {
            uniqueIndex.ExceptWith(items);
            foreach(var i in items) {
                this.items.Remove(i);
            }
        }

        public void RemoveAt(int index)
        {
            var item = items[index];
            uniqueIndex.Remove(item);
            items.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
    }
}