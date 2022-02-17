using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.GoQL
{
    public partial class GoQLExecutor
    {
        class DoubleBuffer<T> : IEnumerable<T>
        {
            List<T> readBuffer = new List<T>(4096);
            List<T> writeBuffer = new List<T>(4096);
            HashSet<T> uniqueIndex = new HashSet<T>();

            public int Count => readBuffer.Count;

            public T this[int index] => readBuffer[index];

            public void Clear()
            {
                readBuffer.Clear();
                writeBuffer.Clear();
                uniqueIndex.Clear();
            }

            public void Reverse() => readBuffer.Reverse();

            public void Swap()
            {
                (readBuffer, writeBuffer) = (writeBuffer, readBuffer);
                writeBuffer.Clear();
                uniqueIndex.Clear();
            }

            public void Add(T item)
            {
                if (uniqueIndex.Contains(item)) return;
                writeBuffer.Add(item);
                uniqueIndex.Add(item);
            }
            
            public void Remove(T item)
            {
                writeBuffer.Remove(item);
            }

            public void AddRange(IEnumerable<T> items)
            {
                foreach (var item in items)
                {
                    if (uniqueIndex.Contains(item)) continue;
                    writeBuffer.Add(item);    
                }
            }

            public IEnumerator<T> GetEnumerator()
            {
                return readBuffer.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return readBuffer.GetEnumerator();
            }
        }

    }

}