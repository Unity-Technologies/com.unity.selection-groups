using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Unity.SelectionGroups.Tests 
{
internal class SelectionGroupUtilityTests {
    
    [Test]
    public void CheckMovingMembersSelectionToGroup() {
        //Initialize source groups
        SelectionGroupManager groupManager = SelectionGroupManager.GetOrCreateInstance();
        SelectionGroup        firstGroup     = groupManager.CreateSceneSelectionGroup("First", Color.red);
        firstGroup.Add(CreateGameObjects("1","2","3","4","5"));

        SelectionGroup secondGroup = groupManager.CreateSceneSelectionGroup("Second", Color.green);
        secondGroup.Add(CreateGameObjects("6","7","8"));

        //Configure selection
        GroupMembersSelection selection = new GroupMembersSelection();
        AddGroupMembersToSelection(firstGroup, new []{ 1,2,3}, selection);
        AddGroupMembersToSelection(secondGroup, new []{ 1}, selection);
                
        //Move to dest
        SelectionGroup destGroup = groupManager.CreateSceneSelectionGroup("Dest", Color.blue);
        selection = SelectionGroupUtility.MoveMembersSelectionToGroup(selection,destGroup);

        Assert.IsTrue(GroupContainsMembers(firstGroup, new HashSet<string>() {"1", "5"}));
        Assert.IsTrue(GroupContainsMembers(secondGroup, new HashSet<string>() {"6", "8"}));


        //Test new selection
        Assert.IsTrue(SelectionContainsGroupWithMembers(selection, 
            destGroup, new HashSet<string>() { "2", "3", "4", "7" })
        );
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    
    private static List<GameObject> CreateGameObjects(params string[] names) {
        List<GameObject> list = new List<GameObject>(); 
        foreach (string name in names) {
            GameObject go = new GameObject(name);
            list.Add(go);
        }

        return list;
    }
    
    private static void AddGroupMembersToSelection(SelectionGroup group, int[] memberIndexes,
        GroupMembersSelection selection) 
    {
        IList<Object> members = group.Members;
        foreach (int memberIndex in memberIndexes) {
            selection.AddObject(group, members[memberIndex]);
        }
    }
    
    private static bool GroupContainsMembers(SelectionGroup group, HashSet<string> names) {
        IList<Object> members = group.Members;
        if (names.Count != members.Count) 
            return false;
            
        foreach (Object member in members) {
            if (!names.Contains(member.name))
                return false;
        }

        return true;
    }
    
    private static bool SelectionContainsGroupWithMembers(GroupMembersSelection selection, 
        SelectionGroup group, HashSet<string> names) 
    {
        bool firstGroupPassed = false;
        foreach (KeyValuePair<SelectionGroup, OrderedSet<Object>> kv in selection) {
            //must contain only one group
            if (firstGroupPassed)
                return false;

            //Ignore empty sets
            if (kv.Value.Count <= 0)
                continue;
            
            if (group != (SelectionGroup)kv.Key)
                return false;
            
            foreach (var selectedObject in kv.Value) {
                if (!names.Contains(selectedObject.name))
                    return false;
            }

            firstGroupPassed = true;
        }

        return true;
    }
    
}

} //end namespace