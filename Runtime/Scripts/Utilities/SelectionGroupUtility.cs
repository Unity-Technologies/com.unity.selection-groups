using System.Collections.Generic;
using Unity.FilmInternalUtilities;
using UnityEngine;

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
            
        prevMembersSelection.Loop((KeyValuePair<SelectionGroup, OrderedSet<GameObject>> kv) =>{
            SelectionGroup prevGroup = kv.Key;
            if (null == prevGroup)
                return;
                
            if (targetGroup == prevGroup)
                return;

            RegisterUndo(prevGroup, "Move Members");
                                       
            kv.Value.Loop((GameObject obj) => {
                newMembersSelection.AddObject(targetGroup, obj);
                targetGroup.Add(obj);
                prevGroup.Remove(obj);
                newMembersSelection.RemoveObject(prevGroup, obj);
            });
        });

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
