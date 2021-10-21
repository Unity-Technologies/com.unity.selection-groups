using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


namespace Unity.SelectionGroups.Tests 
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
        TestUtility.ExecuteGoQLAndVerify("/Head", 1);
    }
    
    [Test]
    public void AllChildren() {
        TestUtility.ExecuteGoQLAndVerify("Head/", 11);
    }

//----------------------------------------------------------------------------------------------------------------------
    
    const string TestScenePath = "Packages/com.unity.selection-groups/Tests/Scenes/GoQLDescenderTestScene";
    
    

}

} //end namespace