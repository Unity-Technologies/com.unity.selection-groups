using System.Collections;
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
        SelectionGroup        group        = groupManager.CreateSceneSelectionGroup("TestGroup", Color.green);
        Assert.IsNotNull(group);
        Assert.AreEqual(1, groupManager.Groups.Count);
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
    
    [UnityTest]
    public IEnumerator UndoGoQLQuery() {
        SelectionGroupManager groupManager = GetAndInitGroupManager();
        SelectionGroup        group        = groupManager.CreateSceneSelectionGroup("TestGroup", Color.green);
        Undo.RegisterCompleteObjectUndo(group, "Query change");
        group.SetQuery("/");
        Assert.AreEqual("/", group.Query);
        yield return null;
        Undo.PerformUndo();
        yield return null;
        Assert.AreEqual("", group.Query);
        Object.Destroy(group);
        yield return null;
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