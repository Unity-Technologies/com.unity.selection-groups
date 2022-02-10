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
        Assert.IsTrue(System.IO.File.Exists($"{TestScenePath}.unity"));
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode($"{TestScenePath}.unity", 
            new LoadSceneParameters(LoadSceneMode.Single));
    }
    
    [Test]
    public void ColliderComponent() {
        TestUtility.ExecuteGoQLAndVerify("Head<t:Collider>", 3,
            (Transform t) => t.name == "Head" && t.GetComponent<Collider>() != null);
    }
    
    [Test]
    public void GlowMaterial() {
        TestUtility.ExecuteGoQLAndVerify("Head<m:Glow>", 1, (Transform t) => {
            MeshRenderer mr = t.GetComponent<MeshRenderer>();
            return t.name == "Head" && null!=mr && mr.sharedMaterial.name == "Glow";
        });
    }
    
    [Test]
    public void StandardShader() {
        TestUtility.ExecuteGoQLAndVerify("Head<s:Standard>", 2,(Transform t) => {
            MeshRenderer mr = t.GetComponent<MeshRenderer>();
            return t.name == "Head" && null!=mr && mr.sharedMaterial.shader.name == "Standard";
        });
    }
    
    [Test]
    public void BadDiscriminator() {
        GoQLExecutor e = new GoQLExecutor("/<Collider/*");
        Assert.AreEqual(ParseResult.UnexpectedEndOfInput, e.parseResult);
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    const string TestScenePath = "Packages/com.unity.selection-groups/Tests/Scenes/GoQLDiscriminatorTestScene";
    


}

} //end namespace