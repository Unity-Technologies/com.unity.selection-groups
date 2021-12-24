using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Unity.SelectionGroups.Runtime
{

internal class GroupsMembersSelection    
{
    void Add(ISelectionGroup group, Object member) {
        if (!m_selectedGroupMembers.ContainsKey(group)) {
            m_selectedGroupMembers.Add(group, new OrderedSet<Object>(){member});
            return;
        }

        m_selectedGroupMembers[group].Add(member);
    }

    void Remove(ISelectionGroup group, Object member) {
        if (!m_selectedGroupMembers.ContainsKey(group)) {
            return;
        }

        m_selectedGroupMembers[group].Remove(member);
    }
        
    bool Contains(ISelectionGroup group, Object member) {
        if (!m_selectedGroupMembers.ContainsKey(group))
            return false;

        return (m_selectedGroupMembers[group].Contains(member));
    }
        
    void Clear() {
        m_selectedGroupMembers.Clear();
    }

//----------------------------------------------------------------------------------------------------------------------    
    readonly Dictionary<ISelectionGroup, OrderedSet<Object>> m_selectedGroupMembers 
        = new Dictionary<ISelectionGroup, OrderedSet<Object>>();
    
}
} //end namespace
