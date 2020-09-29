using System.Collections;
using System.Collections.Generic;

namespace Unity.GoQL
{
    public partial class GoQLExecutor
    {
        class DoubleBuffer<T> : IEnumerable<T>
        {
            List<T> readBuffer = new List<T>(4096);
            List<T> writeBuffer = new List<T>(4096);

            public int Count => readBuffer.Count;

            public T this[int index] => readBuffer[index];

            public void Clear()
            {
                readBuffer.Clear();
                writeBuffer.Clear();
            }

            public void Reverse() => readBuffer.Reverse();

            public void Swap()
            {
                var t = readBuffer;
                readBuffer = writeBuffer;
                writeBuffer = t;
                writeBuffer.Clear();
            }

            public void Add(T item)
            {
                writeBuffer.Add(item);
            }

            public void AddRange(IEnumerable<T> items)
            {
                writeBuffer.AddRange(items);
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