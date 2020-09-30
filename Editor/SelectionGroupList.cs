using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.SelectionGroups
{
    /// <summary>
    /// A serializable list of SelectionGroups.
    /// </summary>
    [System.Serializable]
    public class SelectionGroupList : ISerializationCallbackReceiver, IEnumerable<SelectionGroup>, IList<SelectionGroup>
    {
        internal OrderedSet<int> groupIds = new OrderedSet<int>();

        int[] _ids;

        /// <summary>
        /// The number of instances in this list.
        /// </summary>
        public int Count => groupIds.Count;

        /// <summary>
        /// Is this list readonly? Always false.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Fetch a selectiongroup by group id.
        /// </summary>
        /// <value></value>
        public SelectionGroup this[int index]
        {
            get => SelectionGroupManager.instance.GetGroup(groupIds[index]);
            set => groupIds[index] = value.groupId;
        }

        /// <summary>
        /// Add a selection group to this list.
        /// </summary>
        /// <param name="group"></param>
        public void Add(SelectionGroup group)
        {
            groupIds.Add(group.groupId);
        }

        /// <summary>
        /// Remove a selection group from this list.
        /// </summary>
        /// <param name="group"></param>
        /// <returns>True if the group was found, otherwise false.</returns>
        public bool Remove(SelectionGroup group)
        {
            return groupIds.Remove(group.groupId);
        }

        /// <summary>
        /// The deserialization method for this instance.
        /// </summary>
        public void OnAfterDeserialize()
        {
            if (_ids != null)
            {
                groupIds.AddRange(_ids);
            }
        }

        /// <summary>
        /// The serialization method for this instance.
        /// </summary>
        public void OnBeforeSerialize()
        {
            _ids = groupIds.ToArray();
        }

        /// <summary>
        /// Enumerate through all groups in this list.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<SelectionGroup> GetEnumerator()
        {
            foreach (var i in groupIds)
            {
                yield return SelectionGroupManager.instance.GetGroup(i);
            }
        }

        /// <summary>
        /// Enumerable for all groups in this list.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Fetch index of a selection group in this list.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(SelectionGroup item)
        {
            return groupIds.IndexOf(item.groupId);
        }

        /// <summary>
        /// Inser a selection group at an index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, SelectionGroup item)
        {
            groupIds.Insert(index, item.groupId);
        }

        /// <summary>
        /// Remove selection group at an index.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            groupIds.RemoveAt(index);
        }

        /// <summary>
        /// Clear this list.
        /// </summary>
        public void Clear()
        {
            groupIds.Clear();
        }

        /// <summary>
        /// Checks if this list contains a selection group.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>True if found, otherwise false.</returns>
        public bool Contains(SelectionGroup item)
        {
            return groupIds.Contains(item.groupId);
        }

        /// <summary>
        /// Copies this list into an array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(SelectionGroup[] array, int arrayIndex)
        {
            for(var i=arrayIndex; i<groupIds.Count; i++)
                array[i] = this[i];
        }

        /// <summary>
        /// Remove an item from this list.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>True if item was found otherwise false.</returns>
        bool ICollection<SelectionGroup>.Remove(SelectionGroup item)
        {
            return this.Remove(item);
        }
    }
}