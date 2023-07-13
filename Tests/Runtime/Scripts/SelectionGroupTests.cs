using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.SelectionGroups.Tests 
{
internal class SelectionGroupTests {
    
    [Test]
    public void CreateEmptyGroup() {
        SelectionGroupManager groupManager = GetAndInitGroupManager();
        SelectionGroup        group        = groupManager.CreateSelectionGroup("TestGroup", Color.green);
        Assert.IsNotNull(group);
        Assert.AreEqual(1, groupManager.Groups.Count);
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    [Test]
    public void DeleteGroupComponent() {
        SelectionGroupManager groupManager = GetAndInitGroupManager();
        SelectionGroup        group        = groupManager.CreateSelectionGroup("TestGroup", Color.green);       
        Object.DestroyImmediate(group);
        Assert.AreEqual(0, groupManager.Groups.Count);
    }
    
    [Test]
    public void DeleteGroupGameObject() {
        SelectionGroupManager groupManager = GetAndInitGroupManager();
        SelectionGroup        group        = groupManager.CreateSelectionGroup("TestGroup", Color.green);       
        Object.DestroyImmediate(group);
        Assert.AreEqual(0, groupManager.Groups.Count);
    }
    
    [Test]
    public void DeleteGroupByAPI() {
        SelectionGroupManager groupManager = GetAndInitGroupManager();
        SelectionGroup        group        = groupManager.CreateSelectionGroup("TestGroup", Color.green);
        groupManager.DeleteGroup(group);
        Assert.AreEqual(0, groupManager.Groups.Count);
    }
    
  
//----------------------------------------------------------------------------------------------------------------------
    [Test]
    public void FindGroupMemberComponents() {
        //Preparation
        SelectionGroupManager groupManager = GetAndInitGroupManager();
        SelectionGroup        group        = groupManager.CreateSelectionGroup("TestGroup", Color.green);

        Transform foo = CreateLightObject("Foo");
        Transform bar = CreateLightObject("Bar", enable: true, foo);
        Transform baz = CreateLightObject("Baz", enable: true, bar);
        Transform boo = CreateLightObject("Boo", enable: false, foo);
        
        //Just add one to the group
        group.Add(foo.gameObject);
        VerifyGroupMemberComponents(group, includeInactiveChildren: false, 3);
        VerifyGroupMemberComponents(group, includeInactiveChildren: true, 4);
        
        //Add all to the group
        group.Clear();
        group.Add(foo.gameObject);
        group.Add(bar.gameObject);
        group.Add(baz.gameObject);
        VerifyGroupMemberComponents(group, includeInactiveChildren: false, 3);
        VerifyGroupMemberComponents(group, includeInactiveChildren: true, 4);
    }
    
    
//----------------------------------------------------------------------------------------------------------------------

    private SelectionGroupManager GetAndInitGroupManager() {
        SelectionGroupManager groupManager = SelectionGroupManager.GetOrCreateInstance();
        groupManager.ClearGroups();
        return groupManager;
    }

    private static Transform CreateLightObject(string objectName, bool enable = true, Transform parent = null) {
        Light light = new GameObject(objectName).AddComponent<Light>();
        light.gameObject.SetActive(enable);

        Transform t = light.transform;
        t.parent = parent;
        return t;
    }

    private static void VerifyGroupMemberComponents(SelectionGroup group, bool includeInactiveChildren, int numExpectedMembers) {
        List<Light>    tempList = new List<Light>();
        HashSet<Light> lights   = new HashSet<Light>();
        group.FindMemberComponents<Light>(includeInactiveChildren, tempList, lights);
        Assert.AreEqual(numExpectedMembers, lights.Count);
        
    }
}

} //end namespace