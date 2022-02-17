using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Unity.SelectionGroups
{
    //A class to hold selected members from selected groups
    internal class GroupMembersSelection : IEnumerable<KeyValuePair<SelectionGroup, OrderedSet<Object>>>
    {
        private readonly Dictionary<SelectionGroup, OrderedSet<Object>> m_SelectedGroupMembers 
            = new Dictionary<SelectionGroup, OrderedSet<Object>>();
        
        internal GroupMembersSelection() { }

        internal GroupMembersSelection(GroupMembersSelection other) 
        {
            foreach (KeyValuePair<SelectionGroup, OrderedSet<Object>> kv in other) 
            {
                OrderedSet<Object> collection = new OrderedSet<Object>() { };
                foreach (Object member in kv.Value) 
                {
                    collection.Add(member);
                }

                m_SelectedGroupMembers[kv.Key] = collection;
            }
        }
    //----------------------------------------------------------------------------------------------------------------------
        
        public IEnumerator<KeyValuePair<SelectionGroup, OrderedSet<Object>>> GetEnumerator() 
        {
            return m_SelectedGroupMembers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() 
        {
            return this.GetEnumerator();
        }
        
    //----------------------------------------------------------------------------------------------------------------------
        
        internal void AddObject(SelectionGroup group, Object member) 
        {
            if (!m_SelectedGroupMembers.ContainsKey(group)) 
            {
                m_SelectedGroupMembers.Add(group, new OrderedSet<Object>(){member});
                return;
            }

            m_SelectedGroupMembers[group].Add(member);
        }

        internal void AddGroupMembers(SelectionGroup group) 
        {
            AddObjects(group, group.Members);
        }

        internal void Add(GroupMembersSelection otherSelection) 
        {
            foreach (KeyValuePair<SelectionGroup, OrderedSet<Object>> kv in otherSelection) 
            {
                SelectionGroup group = kv.Key;
                AddObjects(group, kv.Value);
            }
        }
        
        private void AddObjects(SelectionGroup group, IEnumerable<Object> objects) 
        {
            
            OrderedSet<Object> collection = null;
            if (!m_SelectedGroupMembers.ContainsKey(group)) 
            {
                collection = new OrderedSet<Object>() { };
                m_SelectedGroupMembers.Add(group, collection);
            } else 
            {
                collection = m_SelectedGroupMembers[group];
            }

            foreach (Object m in objects) {
                collection.Add(m);
            }
            
        }

        internal void RemoveObject(SelectionGroup group, Object member) 
        {
            if (!m_SelectedGroupMembers.ContainsKey(group))
            {
                return;
            }

            m_SelectedGroupMembers[group].Remove(member);
        }

        internal void RemoveGroup(SelectionGroup group) 
        {
            if (!m_SelectedGroupMembers.ContainsKey(group)) 
            {
                return;
            }
            m_SelectedGroupMembers.Remove(group);
        }
        
        internal bool Contains(SelectionGroup group, Object member) 
        {
            if (!m_SelectedGroupMembers.ContainsKey(group))
                return false;

            return (m_SelectedGroupMembers[group].Contains(member));
        }
            
        internal void Clear() 
        {
            m_SelectedGroupMembers.Clear();
        }

        internal Object[] ConvertMembersToArray() 
        {
            HashSet<Object> set = ConvertMembersToSet();
            Object[] arr = new Object[set.Count];
            set.CopyTo(arr);
            return arr;
        }

        internal HashSet<Object> ConvertMembersToSet() 
        {
            HashSet<Object> set = new HashSet<Object>();
            foreach (KeyValuePair<SelectionGroup, OrderedSet<Object>> kv in m_SelectedGroupMembers)
            {
                set.UnionWith(kv.Value);
            }

            return set;
        }
    }
} //end namespace
