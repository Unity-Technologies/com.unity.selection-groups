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
        List<Light>           tempList     = new List<Light>();
        HashSet<Light>        lights       = new HashSet<Light>();

        Transform foo = CreateLightObject("Foo");
        Transform bar = CreateLightObject("Bar", enable: true, foo);
        Transform baz = CreateLightObject("Baz", enable: true, bar);
        Transform boo = CreateLightObject("Boo", enable: false, foo);
        
        //Just add one to the group
        group.Add(foo.gameObject);
        group.FindMemberComponents<Light>(includeInactiveChildren:false, tempList, lights);
        Assert.AreEqual(3, lights.Count);

        group.FindMemberComponents<Light>(includeInactiveChildren:true, tempList, lights);        
        Assert.AreEqual(4, lights.Count);
        
        //Add all to the group
        group.Clear();
        group.Add(foo.gameObject);
        group.Add(bar.gameObject);
        group.Add(baz.gameObject);
        group.FindMemberComponents<Light>(includeInactiveChildren:false, tempList, lights);
        Assert.AreEqual(3, lights.Count);        

        group.FindMemberComponents<Light>(includeInactiveChildren:true, tempList, lights);
        Assert.AreEqual(4, lights.Count);        
    }
    
    
//----------------------------------------------------------------------------------------------------------------------

    private SelectionGroupManager GetAndInitGroupManager() {
        SelectionGroupManager groupManager = SelectionGroupManager.GetOrCreateInstance();
        groupManager.ClearGroups();
        return groupManager;
    }

    private Transform CreateLightObject(string objectName, bool enable = true, Transform parent = null) {
        Light light = new GameObject(objectName).AddComponent<Light>();
        light.gameObject.SetActive(enable);

        Transform t = light.transform;
        t.parent = parent;
        return t;
    }
}

} //end namespace