using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.SelectionGroups
{
    [System.Serializable]
    public class SelectionGroupList : ISerializationCallbackReceiver, IEnumerable<SelectionGroup>, IList<SelectionGroup>
    {
        internal OrderedSet<int> groupIds = new OrderedSet<int>();

        int[] _ids;

        public int Count => groupIds.Count;

        public bool IsReadOnly => false;

        public SelectionGroup this[int index]
        {
            get => SelectionGroupManager.instance.GetGroup(groupIds[index]);
            set => groupIds[index] = value.groupId;
        }

        public void Add(SelectionGroup group)
        {
            groupIds.Add(group.groupId);
        }

        public bool Remove(SelectionGroup group)
        {
            return groupIds.Remove(group.groupId);
        }

        public void OnAfterDeserialize()
        {
            if (_ids != null)
            {
                groupIds.AddRange(_ids);
            }
        }

        public void OnBeforeSerialize()
        {
            _ids = groupIds.ToArray();
        }

        public IEnumerator<SelectionGroup> GetEnumerator()
        {
            foreach (var i in groupIds)
            {
                yield return SelectionGroupManager.instance.GetGroup(i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(SelectionGroup item)
        {
            return groupIds.IndexOf(item.groupId);
        }

        public void Insert(int index, SelectionGroup item)
        {
            groupIds.Insert(index, item.groupId);
        }

        public void RemoveAt(int index)
        {
            groupIds.RemoveAt(index);
        }

        public void Clear()
        {
            groupIds.Clear();
        }

        public bool Contains(SelectionGroup item)
        {
            return groupIds.Contains(item.groupId);
        }

        public void CopyTo(SelectionGroup[] array, int arrayIndex)
        {
            for(var i=arrayIndex; i<groupIds.Count; i++)
                array[i] = this[i];
        }

        bool ICollection<SelectionGroup>.Remove(SelectionGroup item)
        {
            return this.Remove(item);
        }
    }
}