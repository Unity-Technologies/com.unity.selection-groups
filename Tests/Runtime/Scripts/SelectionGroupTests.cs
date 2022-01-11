using NUnit.Framework;
using UnityEngine;

namespace Unity.SelectionGroups.Tests 
{
internal class SelectionGroupTests {
    
    [Test]
    public void CreateEmptyGroup() {
        SelectionGroupManager groupManager = SelectionGroupManager.GetOrCreateInstance();         
        SelectionGroup group = groupManager.CreateSceneSelectionGroup("TestGroup", Color.green);
        Assert.IsNotNull(group);
        Assert.AreEqual(1, groupManager.Groups.Count);        
    }
    
    [Test]
    public void DeleteGroupComponent() {
        SelectionGroupManager groupManager = SelectionGroupManager.GetOrCreateInstance();         
        SelectionGroup        group        = groupManager.CreateSceneSelectionGroup("TestGroup", Color.green);       
        FilmInternalUtilities.ObjectUtility.Destroy(group);
        Assert.AreEqual(0, groupManager.Groups.Count);        
    }
    
    [Test]
    public void DeleteGroupGameObject() {
        SelectionGroupManager groupManager = SelectionGroupManager.GetOrCreateInstance();         
        SelectionGroup        group        = groupManager.CreateSceneSelectionGroup("TestGroup", Color.green);       
        FilmInternalUtilities.ObjectUtility.Destroy(group.gameObject);
        Assert.AreEqual(0, groupManager.Groups.Count);        
    }
}

} //end namespace