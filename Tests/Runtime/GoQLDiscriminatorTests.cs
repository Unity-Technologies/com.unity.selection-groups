using System.Collections;
using NUnit.Framework;
using Unity.GoQL;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


namespace Unity.SelectionGroups.Tests 
{
internal class GoQLDiscriminatorTests
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
    public void ColliderComponent()
    {
        var e = new GoQLExecutor("Head<t:Collider>");
        var results = e.Execute();
        Assert.AreEqual(ParseResult.OK, e.parseResult);
        Assert.AreEqual(1, results.Length);
        Assert.IsTrue(results[0].GetComponent<Collider>() != null);
    }
    
    [Test]
    public void GlowMaterial()
    {
        var e = new GoQLExecutor("Head<m:Glow>");
        var results = e.Execute();
        Assert.AreEqual(ParseResult.OK, e.parseResult);
        Assert.AreEqual(1, results.Length);
        Assert.IsTrue(results[0].GetComponent<MeshRenderer>() != null);
        Assert.IsTrue(results[0].GetComponent<MeshRenderer>().sharedMaterial.name == "Glow");
    }
    
    [Test]
    public void StandardShader()
    {
        var e = new GoQLExecutor("Head<s:Standard>");
        var results = e.Execute();
        Assert.AreEqual(ParseResult.OK, e.parseResult);
        Assert.AreEqual(1, results.Length);
        Assert.IsTrue(results[0].GetComponent<MeshRenderer>() != null);
        Assert.IsTrue(results[0].GetComponent<MeshRenderer>().sharedMaterial.shader.name == "Standard");
    }
    


}

} //end namespace