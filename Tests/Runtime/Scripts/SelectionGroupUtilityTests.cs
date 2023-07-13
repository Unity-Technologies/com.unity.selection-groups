using System.Collections.Generic;
using NUnit.Framework;
using Unity.FilmInternalUtilities;
using UnityEngine;

namespace Unity.SelectionGroups.Tests 
{
internal class SelectionGroupUtilityTests {
    
    [Test]
    public void CheckMovingMembersSelectionToGroup() {
        //Initialize source groups
        SelectionGroupManager groupManager = SelectionGroupManager.GetOrCreateInstance();
        SelectionGroup        firstGroup   = groupManager.CreateSelectionGroup("First", Color.red);
        firstGroup.AddRange(CreateGameObjects("1","2","3","4","5"));

        SelectionGroup secondGroup = groupManager.CreateSelectionGroup("Second", Color.green);
        secondGroup.AddRange(CreateGameObjects("6","7","8"));

        //Configure selection
        GroupMembersSelection selection = new GroupMembersSelection();
        AddGroupMembersToSelection(firstGroup, new []{ 1,2,3}, selection);
        AddGroupMembersToSelection(secondGroup, new []{ 1}, selection);
                
        //Move to dest
        SelectionGroup destGroup = groupManager.CreateSelectionGroup("Dest", Color.blue);
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
        names.Loop((string name) => {
            GameObject go = new GameObject(name);
            list.Add(go);
        });

        return list;
    }
    
    private static void AddGroupMembersToSelection(SelectionGroup group, int[] memberIndexes,
        GroupMembersSelection selection) 
    {
        IList<GameObject> members = group.Members;
        memberIndexes.Loop((int memberIndex) => {
            selection.AddObject(group, members[memberIndex]);
        });
    }
    
    private static bool GroupContainsMembers(SelectionGroup group, HashSet<string> names) {
        IList<GameObject> members = group.Members;
        if (names.Count != members.Count) 
            return false;

        int numMembers = members.Count;
        for (int i=0;i<numMembers;++i) {
            if (!names.Contains(members[i].name))
                return false;
        }

        return true;
    }
    
    private static bool SelectionContainsGroupWithMembers(GroupMembersSelection selection, 
        SelectionGroup group, HashSet<string> names) 
    {
        bool      firstGroupPassed = false;
        using var enumerator       = selection.GetEnumerator();
        while (enumerator.MoveNext()) {
            KeyValuePair<SelectionGroup, OrderedSet<GameObject>> kv = enumerator.Current;
            //must contain only one group
            if (firstGroupPassed)
                return false;

            //Ignore empty sets
            if (kv.Value.Count <= 0)
                continue;
            
            if (group != (SelectionGroup)kv.Key)
                return false;

            using var goEnumerator = kv.Value.GetEnumerator();
            while (goEnumerator.MoveNext()) {
                GameObject selectedObject = goEnumerator.Current;
                if (null == selectedObject)
                    continue;
                if (!names.Contains(selectedObject.name))
                    return false;
            }

            firstGroupPassed = true;
        }

        return true;
    }
    
}

} //end namespace