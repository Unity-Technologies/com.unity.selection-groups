using System.Collections;
using NUnit.Framework;
using Unity.GoQL;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif


namespace Unity.SelectionGroups.Tests 
{
internal class GoQLDiscriminatorTests
{
    [UnitySetUp]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public IEnumerator SetUp() {
        Assert.IsTrue(System.IO.File.Exists($"{TestScenePath}.unity"));
#if UNITY_EDITOR
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode($"{TestScenePath}.unity", 
            new LoadSceneParameters(LoadSceneMode.Single));
#else
        yield return null;
#endif
    }
    
    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void ColliderComponent() {
        TestUtility.ExecuteGoQLAndVerify("Head<t:Collider>", 3,
            (Transform t) => t.name == "Head" && t.GetComponent<Collider>() != null);
    }
    
    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void ExclusionOperator() {
        TestUtility.ExecuteGoQLAndVerify("Head<t:Collider><!t:MeshFilter>", 2,
            (Transform t) => t.name == "Head" && t.GetComponent<Collider>() != null && t.GetComponent<MeshFilter>() == null);
    }
    
    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void GlowMaterial() {
        TestUtility.ExecuteGoQLAndVerify("Head<m:Glow>", 1, (Transform t) => {
            MeshRenderer mr = t.GetComponent<MeshRenderer>();
            return t.name == "Head" && null!=mr && mr.sharedMaterial.name == "Glow";
        });
    }
    
    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void StandardShader() {
        TestUtility.ExecuteGoQLAndVerify("Head<s:Standard>", 2,(Transform t) => {
            MeshRenderer mr = t.GetComponent<MeshRenderer>();
            return t.name == "Head" && null!=mr && mr.sharedMaterial.shader.name == "Standard";
        });
    }
    
    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void BadDiscriminator() {
        GoQLExecutor e = new GoQLExecutor("/<Collider/*");
        Assert.AreEqual(ParseResult.UnexpectedEndOfInput, e.parseResult);
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    const string TestScenePath = "Packages/com.unity.selection-groups/Tests/Scenes/GoQLDiscriminatorTestScene";
    


}

} //end namespace