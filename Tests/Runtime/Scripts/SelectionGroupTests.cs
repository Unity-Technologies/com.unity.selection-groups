using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.SelectionGroups.Tests 
{
internal class SelectionGroupTests {
    
    [Test]
    public void CreateEmptyGroupAndUndo() {
        SelectionGroupManager groupManager = GetAndInitGroupManager();
        SelectionGroup        group        = groupManager.CreateSceneSelectionGroup("TestGroup", Color.green);
        Assert.IsNotNull(group);
        Assert.AreEqual(1, groupManager.Groups.Count);
        
        Undo.PerformUndo();
        Assert.AreEqual(0, groupManager.Groups.Count);
        
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    [Test]
    public void DeleteGroupComponent() {
        SelectionGroupManager groupManager = GetAndInitGroupManager();
        SelectionGroup        group        = groupManager.CreateSceneSelectionGroup("TestGroup", Color.green);       
        Object.DestroyImmediate(group);
        Assert.AreEqual(0, groupManager.Groups.Count);
    }
    
    [Test]
    public void DeleteGroupGameObject() {
        SelectionGroupManager groupManager = GetAndInitGroupManager();
        SelectionGroup        group        = groupManager.CreateSceneSelectionGroup("TestGroup", Color.green);       
        Object.DestroyImmediate(group);
        Assert.AreEqual(0, groupManager.Groups.Count);
    }
    
    [Test]
    public void DeleteGroupByAPI() {
        SelectionGroupManager groupManager = GetAndInitGroupManager();
        SelectionGroup        group        = groupManager.CreateSceneSelectionGroup("TestGroup", Color.green);
        groupManager.DeleteGroup(group);
        Assert.AreEqual(0, groupManager.Groups.Count);
    }
    
    
//----------------------------------------------------------------------------------------------------------------------

    private SelectionGroupManager GetAndInitGroupManager() {
        SelectionGroupManager groupManager = SelectionGroupManager.GetOrCreateInstance();
        groupManager.ClearGroups();
        return groupManager;
    }
}

} //end namespace