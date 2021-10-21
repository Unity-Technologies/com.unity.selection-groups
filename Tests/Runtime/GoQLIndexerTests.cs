using System.Collections;
using NUnit.Framework;
using Unity.GoQL;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


namespace Unity.SelectionGroups.Tests 
{
internal class GoQLIndexerTests
{
    
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        Assert.IsTrue(System.IO.File.Exists($"{SelectionGroupsTestsConstants.TestScenePath}.unity"));
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode($"{SelectionGroupsTestsConstants.TestScenePath}.unity", 
            new LoadSceneParameters(LoadSceneMode.Single));
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode($"{SelectionGroupsTestsConstants.EmptyScenePath}.unity", 
            new LoadSceneParameters(LoadSceneMode.Single));
    }
    
    [Test]
    public void FirstChild()
    {
        var e = new GoQLExecutor("Head/[0]");
        var results = e.Execute();
        Assert.AreEqual(ParseResult.OK, e.parseResult);
        Assert.AreEqual(2, results.Length);
        Assert.AreEqual("Child (0)", results[0].name);
    }
    
    [Test]
    public void IndexedChildren()
    {
        var e = new GoQLExecutor("Head/[0,1,5]");
        var results = e.Execute();
        Assert.AreEqual(ParseResult.OK, e.parseResult);
        Assert.AreEqual(5, results.Length);
        // Assert.AreEqual("Child (0)", results[0].name);
        // Assert.AreEqual("Child (1)", results[1].name);
        // Assert.AreEqual("Child (5)", results[2].name);
    }
    
    [Test]
    public void LastChild()
    {
        var e = new GoQLExecutor("Head/[-1]");
        var results = e.Execute();
        Assert.AreEqual(ParseResult.OK, e.parseResult);
        Assert.AreEqual(1, results.Length);
        Assert.AreEqual("Head", results[0].name);
    }
    
    [Test]
    public void RangedChildren()
    {
        var e = new GoQLExecutor("Head/[3:5]");
        var results = e.Execute();
        Assert.AreEqual(ParseResult.OK, e.parseResult);
        Assert.AreEqual(4, results.Length);
        Assert.AreEqual("Child (3)", results[0].name);
        Assert.AreEqual("Quad 1", results[1].name);
        Assert.AreEqual("Child (4)", results[2].name);
        Assert.AreEqual("Quad 2", results[3].name);
    }
    

}

} //end namespace