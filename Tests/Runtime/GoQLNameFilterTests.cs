using System.Collections;
using NUnit.Framework;
using Unity.GoQL;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


namespace Unity.SelectionGroups.Tests 
{
internal class GoQLNameFilterTests
{
    
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        Assert.IsTrue(System.IO.File.Exists($"{TestScenePath}.unity"));
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode($"{TestScenePath}.unity", 
            new LoadSceneParameters(LoadSceneMode.Single));
    }

    [Test]
    public void Simple() {
        TestUtility.ExecuteGoQLAndVerify("Head", 2);
    }
    
    [Test]
    public void BeginningWildcard() {
        TestUtility.ExecuteGoQLAndVerify("*Head", 3);
    }
    
    [Test]
    public void EndingWildcard() {
        TestUtility.ExecuteGoQLAndVerify("Head*", 3);
    }
    
    [Test]
    public void BeginningAndEndingWildcard() {
        TestUtility.ExecuteGoQLAndVerify("*Head*", 4);
    }
    

//----------------------------------------------------------------------------------------------------------------------
    
    const string TestScenePath = "Packages/com.unity.selection-groups/Tests/Scenes/GoQLNameFilterTestScene";
    

}

} //end namespace