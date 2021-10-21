using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


namespace Unity.SelectionGroups.Tests 
{
internal class GoQLIndexerTests
{
    
    [UnitySetUp]
    public IEnumerator SetUp() {
        Assert.IsTrue(System.IO.File.Exists($"{SelectionGroupsTestsConstants.TestScenePath}.unity"));
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode($"{SelectionGroupsTestsConstants.TestScenePath}.unity", 
            new LoadSceneParameters(LoadSceneMode.Single));
    }
    
    [Test]
    public void FirstChild() {
        GameObject[] results = TestUtility.ExecuteGoQLAndVerify("Head/[0]", 2);
        Assert.AreEqual("Child (0)", results[0].name);
    }
    
    [Test]
    public void IndexedChildren()
    {
        GameObject[] results = TestUtility.ExecuteGoQLAndVerify("Head/[0,1,5]", 5);
        // Assert.AreEqual("Child (0)", results[0].name);
        // Assert.AreEqual("Child (1)", results[1].name);
        // Assert.AreEqual("Child (5)", results[2].name);
    }
    
    [Test]
    public void LastChild()
    {
        GameObject[] results = TestUtility.ExecuteGoQLAndVerify("Head/[-1]", 1);
        Assert.AreEqual("Head", results[0].name);
    }
    
    [Test]
    public void RangedChildren()
    {
        GameObject[] results = TestUtility.ExecuteGoQLAndVerify("Head/[3:5]", 4);
        Assert.AreEqual("Child (3)", results[0].name);
        Assert.AreEqual("Quad 1", results[1].name);
        Assert.AreEqual("Child (4)", results[2].name);
        Assert.AreEqual("Quad 2", results[3].name);
    }
    

}

} //end namespace