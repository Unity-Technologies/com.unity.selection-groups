using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.SelectionGroups.Tests;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;


namespace Unity.SelectionGroups.EditorTests 
{
internal class GoQLIndexerTests
{
    
    [UnitySetUp]
    public IEnumerator SetUp() {
        Assert.IsTrue(System.IO.File.Exists($"{TestScenePath}.unity"));
        EditorSceneManager.OpenScene($"{TestScenePath}.unity");
        yield return null;
    }
    
    [Test]
    public void FirstChild() {
        TestUtility.ExecuteGoQLAndVerify("Head/[0]", 2, 
            (Transform t) => null!=t.parent && t.parent.name == "Head" && t.name.EndsWith("Child (0)")
        );
    }
    
    [Test]
    public void IndexedChildren() {
        int[] indexes = new[] { 0, 1, 5 };       
        List<Transform> results = TestUtility.ExecuteGoQLAndVerify($"Head/[{string.Join(",", indexes)}]", 6, 
            (Transform t) => null!=t.parent && t.parent.name == "Head"
        );

        int numIndexes = indexes.Length;
        foreach (Transform t in results) {
            bool found = false;

            for (int i = 0; !found && i < numIndexes; ++i) {
                if (t.name.EndsWith($"Child ({indexes[i]})"))
                    found = true;
            }
            Assert.IsTrue(found);
        } 
    }
    
    [Test]
    public void NoDuplicateIndexedChildren()
    {
        TestUtility.ExecuteGoQLAndVerify("/[0,1,0]", 2);
    }
    
    [Test]
    public void ExclusionOperator()
    {
        TestUtility.ExecuteGoQLAndVerify("/[0:3]", 3);
        TestUtility.ExecuteGoQLAndVerify("/[0:3,!1,!0]", 1,(t) => t.name == "Head");
    }
    
    
    [Test]
    public void LastChild()
    {
        TestUtility.ExecuteGoQLAndVerify("Head/[-1]", 2, (Transform t) => null!=t.parent && t.parent.name == "Head");
    }
    
    [Test]
    public void RangedChildren() {
        int startIndex = 3;
        int endIndex   = 5;
            
        List<Transform> results = TestUtility.ExecuteGoQLAndVerify($"Head/[{startIndex}:{endIndex}]", 4, 
            (Transform t) => null!=t.parent && t.parent.name == "Head"
        );
        foreach (Transform t in results) {
            bool found = false;

            for (int i = startIndex; !found && i < endIndex; ++i) {
                if (t.name.EndsWith($"Child ({i})"))
                    found = true;
            }
            Assert.IsTrue(found);
        }         
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    const string TestScenePath = "Packages/com.unity.selection-groups/Tests/Scenes/GoQLIndexerTestScene";

}

} //end namespace