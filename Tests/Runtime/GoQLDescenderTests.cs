using System.Collections;
using NUnit.Framework;
using Unity.GoQL;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


namespace Unity.SelectionGroups.Tests 
{
public class GoQLDescenderTests
{
    
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        //Debug.Log("Loading Test Scene.");
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
    public void RootDescender()
    {
        var e = new GoQLExecutor("/Head");
        var results = e.Execute();
        Assert.AreEqual(ParseResult.OK, e.parseResult);
        Assert.AreEqual(1, results.Length);
    }
    
    [Test]
    public void AllChildrenDescender()
    {
        var e = new GoQLExecutor("Head/");
        var results = e.Execute();
        Assert.AreEqual(ParseResult.OK, e.parseResult);
        Assert.AreEqual(11, results.Length);
    }
    

}

} //end namespace