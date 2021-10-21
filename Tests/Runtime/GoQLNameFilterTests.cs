using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.GoQL;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


namespace Unity.SelectionGroups.Tests 
{
public class GoQLNameFilterTests
{
    private static string TestScenePath = "Packages/com.unity.selection-groups/Tests/Scenes/GoQLTestScene";
    private static string EmptyScenePath = "Packages/com.unity.selection-groups/Tests/Scenes/EmptyScene";
    
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        //Debug.Log("Loading Test Scene.");
        Assert.IsTrue(System.IO.File.Exists($"{TestScenePath}.unity"));
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode($"{TestScenePath}.unity", new LoadSceneParameters(LoadSceneMode.Single));
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode($"{EmptyScenePath}.unity", new LoadSceneParameters(LoadSceneMode.Single));
    }

    [Test]
    public void TestGoQLExamples1()
    {
        var e = new GoQLExecutor("Head");
        var results = e.Execute();
        Assert.AreEqual(ParseResult.OK, e.parseResult);
        Assert.AreEqual(2, results.Length);
    }
    
    [Test]
    public void TestGoQLExamples2()
    {
        var e = new GoQLExecutor("*Head");
        var results = e.Execute();
        Assert.AreEqual(ParseResult.OK, e.parseResult);
        Assert.AreEqual(3, results.Length);
    }
    
    [Test]
    public void TestGoQLExamples3()
    {
        var e = new GoQLExecutor("Head*");
        var results = e.Execute();
        Assert.AreEqual(ParseResult.OK, e.parseResult);
        Assert.AreEqual(3, results.Length);
    }
    
    [Test]
    public void TestGoQLExamples4()
    {
        var e = new GoQLExecutor("*Head*");
        var results = e.Execute();
        Assert.AreEqual(ParseResult.OK, e.parseResult);
        Assert.AreEqual(4, results.Length);
    }
    


}

} //end namespace