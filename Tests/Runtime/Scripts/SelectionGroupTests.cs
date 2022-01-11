using NUnit.Framework;
using UnityEngine;

namespace Unity.SelectionGroups.Tests 
{
internal class SelectionGroupTests {
    
//----------------------------------------------------------------------------------------------------------------------
    
    [Test]
    public void DeleteGroupComponent() {
        SelectionGroupManager groupManager = TestUtility.GetAndInitGroupManager();
        SelectionGroup        group        = groupManager.CreateSceneSelectionGroup("TestGroup", Color.green);
        Object.DestroyImmediate(group);
        Assert.AreEqual(0, groupManager.Groups.Count);
    }
    
    [Test]
    public void DeleteGroupGameObject() {
        SelectionGroupManager groupManager = TestUtility.GetAndInitGroupManager();
        SelectionGroup        group        = groupManager.CreateSceneSelectionGroup("TestGroup", Color.green);
        Object.DestroyImmediate(group);
        Assert.AreEqual(0, groupManager.Groups.Count);
    }
    
    [Test]
    public void DeleteGroupByAPI() {
        SelectionGroupManager groupManager = TestUtility.GetAndInitGroupManager();
        SelectionGroup        group        = groupManager.CreateSceneSelectionGroup("TestGroup", Color.green);
        groupManager.DeleteGroup(group);
        Assert.AreEqual(0, groupManager.Groups.Count);
    }
        
//----------------------------------------------------------------------------------------------------------------------

}

} //end namespace