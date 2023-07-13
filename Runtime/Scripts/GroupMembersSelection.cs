using System.Collections;
using System.Collections.Generic;
using Unity.FilmInternalUtilities;
using UnityEngine;

namespace Unity.SelectionGroups
{

//A class to hold selected members from selected groups
internal class GroupMembersSelection : IEnumerable<KeyValuePair<SelectionGroup, OrderedSet<GameObject>>>
{

    internal GroupMembersSelection() { }

    internal GroupMembersSelection(GroupMembersSelection other) {

        other.Loop((KeyValuePair<SelectionGroup, OrderedSet<GameObject>> kv) => {
            OrderedSet<GameObject> collection = new OrderedSet<GameObject>() { };
            kv.Value.Loop((GameObject member) => {
                collection.Add(member);
            });

            m_selectedGroupMembers[kv.Key] = collection;
        });
    }
//----------------------------------------------------------------------------------------------------------------------
    
    public IEnumerator<KeyValuePair<SelectionGroup, OrderedSet<GameObject>>> GetEnumerator() {
        return m_selectedGroupMembers.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return this.GetEnumerator();
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    internal void AddObject(SelectionGroup group, GameObject member) {
        if (!m_selectedGroupMembers.ContainsKey(group)) {
            m_selectedGroupMembers.Add(group, new OrderedSet<GameObject>(){member});
            return;
        }

        m_selectedGroupMembers[group].Add(member);
    }

    internal void AddGroupMembers(SelectionGroup group) {
        AddObjects(group, group.Members);
    }
    
    
    internal void Add(GroupMembersSelection otherSelection) {
        otherSelection.Loop((KeyValuePair<SelectionGroup, OrderedSet<GameObject>> kv) => {
            SelectionGroup group = kv.Key;
            AddObjects(group, kv.Value);
        });
    }
    

    private void AddObjects(SelectionGroup group, IEnumerable<GameObject> objects) {
        
        OrderedSet<GameObject> collection = null;
        if (!m_selectedGroupMembers.ContainsKey(group)) {
            collection = new OrderedSet<GameObject>() { };
            m_selectedGroupMembers.Add(group, collection);
        } else {
            collection = m_selectedGroupMembers[group];
        }

        objects.Loop((GameObject m) => {            
            collection.Add(m);
        });
        
    }
    

    internal void RemoveObject(SelectionGroup group, GameObject member) {
        if (!m_selectedGroupMembers.ContainsKey(group)) {
            return;
        }

        m_selectedGroupMembers[group].Remove(member);
    }

    internal void RemoveGroup(SelectionGroup group) {
        if (!m_selectedGroupMembers.ContainsKey(group)) {
            return;
        }
        m_selectedGroupMembers.Remove(group);
    }
    
    internal bool Contains(SelectionGroup group, GameObject member) {
        if (!m_selectedGroupMembers.ContainsKey(group))
            return false;

        return (m_selectedGroupMembers[group].Contains(member));
    }
        
    internal void Clear() {
        m_selectedGroupMembers.Clear();
    }

    internal GameObject[] ConvertMembersToArray() {
        HashSet<GameObject> set = ConvertMembersToSet();
        GameObject[]        arr = new GameObject[set.Count];
        set.CopyTo(arr);
        return arr;
    }

    internal HashSet<GameObject> ConvertMembersToSet() {
        HashSet<GameObject> set = new HashSet<GameObject>();
        m_selectedGroupMembers.Loop((KeyValuePair<SelectionGroup, OrderedSet<GameObject>> kv) => {
            set.UnionWith(kv.Value);
        });

        return set;
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    readonly Dictionary<SelectionGroup, OrderedSet<GameObject>> m_selectedGroupMembers 
        = new Dictionary<SelectionGroup, OrderedSet<GameObject>>();
    
}
} //end namespace
