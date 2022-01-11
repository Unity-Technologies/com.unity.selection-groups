using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.SelectionGroups.EditorTests 
{
internal class SelectionGroupEditorTests {
    
    
    [UnityTest]
    public IEnumerator DeleteGroupByAPIAndUndo() {
        SelectionGroupManager groupManager = GetAndInitGroupManager();
        SelectionGroup        group        = groupManager.CreateSceneSelectionGroup("TestGroup", Color.green);
        
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        
        groupManager.DeleteGroup(group);
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        
        Assert.AreEqual(0, groupManager.Groups.Count);
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        
        Undo.PerformUndo();
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        Debug.Log(null == group);
        Assert.AreEqual(1, groupManager.Groups.Count);
        
    }
    
    
//----------------------------------------------------------------------------------------------------------------------

    private SelectionGroupManager GetAndInitGroupManager() {
        SelectionGroupManager groupManager = SelectionGroupManager.GetOrCreateInstance();
        groupManager.ClearGroups();
        return groupManager;
    }
}

} //end namespace