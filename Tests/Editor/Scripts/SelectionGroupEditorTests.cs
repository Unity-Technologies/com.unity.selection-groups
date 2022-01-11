using System.Collections;
using NUnit.Framework;
using Unity.FilmInternalUtilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.SelectionGroups.Tests;

namespace Unity.SelectionGroups.EditorTests 
{
internal class SelectionGroupEditorTests {
    
    [UnityTest]
    public IEnumerator CreateEmptyGroupAndUndo() {
        SelectionGroupManager groupManager = SelectionGroupTestsUtility.GetAndInitGroupManager();
        SelectionGroup        group        = groupManager.CreateSceneSelectionGroup("TestGroup", Color.green);
        Assert.IsNotNull(group);
        Assert.AreEqual(1, groupManager.Groups.Count);
        yield return EditorTestsUtility.WaitForFrames(3);
        
        Undo.PerformUndo();
        Assert.AreEqual(0, groupManager.Groups.Count);        
        yield return null;
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
}

} //end namespace