using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace Unity.SelectionGroups
{

//A class to hold selected members from selected groups
internal class GroupMembersSelection : IEnumerable<KeyValuePair<ISelectionGroup, OrderedSet<Object>>>
{

    internal GroupMembersSelection() { }

    internal GroupMembersSelection(GroupMembersSelection other) {

        foreach (KeyValuePair<ISelectionGroup, OrderedSet<Object>> kv in other) {
            OrderedSet<Object> collection = new OrderedSet<Object>() { };
            foreach (Object member in kv.Value) {
                collection.Add(member);
            }

            m_selectedGroupMembers[kv.Key] = collection;
        }
    }
//----------------------------------------------------------------------------------------------------------------------
    
    public IEnumerator<KeyValuePair<ISelectionGroup, OrderedSet<Object>>> GetEnumerator() {
        return m_selectedGroupMembers.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return this.GetEnumerator();
    }
    
//----------------------------------------------------------------------------------------------------------------------

    [CanBeNull]
    internal Object GetFirstObject() {
        foreach (KeyValuePair<ISelectionGroup, OrderedSet<Object>> kv in m_selectedGroupMembers) {
            foreach (Object member in kv.Value) {
                return member;
            }
        }
        return null;
    }

    internal int FindNumUniqueObjects() {
        HashSet<Object> uniqueDraggedObjects = new HashSet<Object>();
        foreach (KeyValuePair<ISelectionGroup, OrderedSet<Object>> members in m_selectedGroupMembers) {
            uniqueDraggedObjects.UnionWith(members.Value);
        }
        return uniqueDraggedObjects.Count;
    }
    
    internal void AddObject(ISelectionGroup group, Object member) {
        if (!m_selectedGroupMembers.ContainsKey(group)) {
            m_selectedGroupMembers.Add(group, new OrderedSet<Object>(){member});
            return;
        }

        m_selectedGroupMembers[group].Add(member);
    }

    internal void AddGroupMembers(ISelectionGroup group) {
        AddObjects(group, group.Members);
    }
    
    
    internal void Add(GroupMembersSelection otherSelection) {
        foreach (KeyValuePair<ISelectionGroup, OrderedSet<Object>> kv in otherSelection) {
            ISelectionGroup group = kv.Key;
            AddObjects(group, kv.Value);
        }
    }
    

    private void AddObjects(ISelectionGroup group, IEnumerable<Object> objects) {
        
        OrderedSet<Object> collection = null;
        if (!m_selectedGroupMembers.ContainsKey(group)) {
            collection = new OrderedSet<Object>() { };
            m_selectedGroupMembers.Add(group, collection);
        } else {
            collection = m_selectedGroupMembers[group];
        }

        foreach (Object m in objects) {
            collection.Add(m);
        }
        
    }
    

    internal void RemoveObject(ISelectionGroup group, Object member) {
        if (!m_selectedGroupMembers.ContainsKey(group)) {
            return;
        }

        m_selectedGroupMembers[group].Remove(member);
    }

    internal void RemoveGroup(ISelectionGroup group) {
        if (!m_selectedGroupMembers.ContainsKey(group)) {
            return;
        }
        m_selectedGroupMembers.Remove(group);
    }
    
    internal bool Contains(ISelectionGroup group, Object member) {
        if (!m_selectedGroupMembers.ContainsKey(group))
            return false;

        return (m_selectedGroupMembers[group].Contains(member));
    }
        
    internal void Clear() {
        m_selectedGroupMembers.Clear();
    }

    internal Object[] ConvertMembersToArray() {
        HashSet<Object> set = new HashSet<Object>();
        foreach (KeyValuePair<ISelectionGroup, OrderedSet<Object>> kv in m_selectedGroupMembers) {
            set.UnionWith(kv.Value);
        }
        Object[] arr = new Object[set.Count];
        set.CopyTo(arr);
        return arr;
    }

//----------------------------------------------------------------------------------------------------------------------    
    readonly Dictionary<ISelectionGroup, OrderedSet<Object>> m_selectedGroupMembers 
        = new Dictionary<ISelectionGroup, OrderedSet<Object>>();
    
}
} //end namespace
