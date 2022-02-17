using System.Collections.Generic;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.SelectionGroups
{

internal static class SelectionGroupUtility
{
    //move prevMembersSelection to targetGroup, and return the new members selection
    internal static GroupMembersSelection MoveMembersSelectionToGroup(GroupMembersSelection prevMembersSelection, 
        SelectionGroup targetGroup) 
    {
        GroupMembersSelection newMembersSelection = new GroupMembersSelection(prevMembersSelection);
        RegisterUndo(targetGroup, "Move Members");
            
        foreach (KeyValuePair<SelectionGroup, OrderedSet<Object>> kv in prevMembersSelection) {
            SelectionGroup prevGroup = kv.Key;
            if (null == prevGroup)
                continue;
                
            if (targetGroup == prevGroup)
                continue;

            RegisterUndo(prevGroup, "Move Members");
                                            
            foreach (Object obj in kv.Value) {
                newMembersSelection.AddObject(targetGroup, obj);
                targetGroup.Add(obj);
                prevGroup.Remove(obj);
                newMembersSelection.RemoveObject(prevGroup, obj);
            }
        }

        return newMembersSelection;
    }
    
    
    static void RegisterUndo(SelectionGroup group, string msg)
    {
#if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(group, msg);
        EditorUtility.SetDirty(group);
#endif
    }
    

}
} //end namespace
