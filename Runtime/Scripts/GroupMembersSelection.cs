﻿using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Unity.SelectionGroups.Runtime
{

internal class GroupMembersSelection : IEnumerable<KeyValuePair<ISelectionGroup, OrderedSet<Object>>>
{

    public IEnumerator<KeyValuePair<ISelectionGroup, OrderedSet<Object>>> GetEnumerator() {
        return m_selectedGroupMembers.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return this.GetEnumerator();
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    internal void AddObjectToSelection(ISelectionGroup group, Object member) {
        if (!m_selectedGroupMembers.ContainsKey(group)) {
            m_selectedGroupMembers.Add(group, new OrderedSet<Object>(){member});
            return;
        }

        m_selectedGroupMembers[group].Add(member);
    }

    internal void AddGroupMembersToSelection(ISelectionGroup group) {
        AddObjectsToSelection(group, group.Members);
    }
    
    
    internal void Add(GroupMembersSelection otherSelection) {
        foreach (KeyValuePair<ISelectionGroup, OrderedSet<Object>> kv in otherSelection) {
            ISelectionGroup group = kv.Key;
            AddObjectsToSelection(group, kv.Value);
        }
    }
    

    private void AddObjectsToSelection(ISelectionGroup group, IEnumerable<Object> objects) {
        
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
    

    internal void Remove(ISelectionGroup group, Object member) {
        if (!m_selectedGroupMembers.ContainsKey(group)) {
            return;
        }

        m_selectedGroupMembers[group].Remove(member);
    }
        
    internal bool Contains(ISelectionGroup group, Object member) {
        if (!m_selectedGroupMembers.ContainsKey(group))
            return false;

        return (m_selectedGroupMembers[group].Contains(member));
    }
        
    internal void Clear() {
        m_selectedGroupMembers.Clear();
    }

//----------------------------------------------------------------------------------------------------------------------    
    readonly Dictionary<ISelectionGroup, OrderedSet<Object>> m_selectedGroupMembers 
        = new Dictionary<ISelectionGroup, OrderedSet<Object>>();
    
}
} //end namespace
