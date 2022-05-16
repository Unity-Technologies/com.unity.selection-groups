using System.Collections;
using NUnit.Framework;
using Unity.SelectionGroups.Tests;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


namespace Unity.SelectionGroups.EditorTests 
{
internal class GoQLDescenderTests
{
    
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        Assert.IsTrue(System.IO.File.Exists($"{TestScenePath}.unity"));
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode($"{TestScenePath}.unity", 
            new LoadSceneParameters(LoadSceneMode.Single));
    }


    [Test]
    public void Root() {
        TestUtility.ExecuteGoQLAndVerify("/Head", 1, (Transform t) => (t.parent == null) && t.name == "Head");
    }
    
    [Test]
    public void AllChildren() {
        TestUtility.ExecuteGoQLAndVerify("Head/", 11, (Transform t) => t.parent.name == "Head");
    }

//----------------------------------------------------------------------------------------------------------------------
    
    const string TestScenePath = "Packages/com.unity.selection-groups/Tests/Scenes/GoQLDescenderTestScene";
    
    

}

} //end namespace