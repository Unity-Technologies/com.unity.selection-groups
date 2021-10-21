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
    public IEnumerator SetUp() {
        Assert.IsTrue(System.IO.File.Exists($"{SelectionGroupsTestsConstants.TestScenePath}.unity"));
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode($"{SelectionGroupsTestsConstants.TestScenePath}.unity", 
            new LoadSceneParameters(LoadSceneMode.Single));
    }
    
    [Test]
    public void ColliderComponent() {
        GameObject[] results = TestUtility.ExecuteGoQLAndVerify("Head<t:Collider>", 1);
        Assert.IsTrue(results[0].GetComponent<Collider>() != null);
    }
    
    [Test]
    public void GlowMaterial() {
        GameObject[] results = TestUtility.ExecuteGoQLAndVerify("Head<m:Glow>", 1);
        Assert.IsTrue(results[0].GetComponent<MeshRenderer>() != null);
        Assert.IsTrue(results[0].GetComponent<MeshRenderer>().sharedMaterial.name == "Glow");
    }
    
    [Test]
    public void StandardShader() {
        GameObject[] results = TestUtility.ExecuteGoQLAndVerify("Head<s:Standard>", 1);
        Assert.IsTrue(results[0].GetComponent<MeshRenderer>() != null);
        Assert.IsTrue(results[0].GetComponent<MeshRenderer>().sharedMaterial.shader.name == "Standard");
    }
    


}

} //end namespace