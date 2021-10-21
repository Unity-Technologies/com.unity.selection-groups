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
        TestUtility.ExecuteGoQLAndVerify("Head", 3, (Transform t) => t.name == "Head");
    }
    
    [Test]
    public void BeginningWildcard() {
        TestUtility.ExecuteGoQLAndVerify("*Head", 4, (Transform t) => t.name.EndsWith("Head"));
    }
    
    [Test]
    public void EndingWildcard() {
        TestUtility.ExecuteGoQLAndVerify("Head*", 5, (Transform t) => t.name.StartsWith("Head"));
    }
    
    [Test]
    public void BeginningAndEndingWildcard() {
        TestUtility.ExecuteGoQLAndVerify("Head*", 6, (Transform t) => t.name.Contains("Head"));
    }
    
    [Test]
    public void InnerWildCard() {
        TestUtility.ExecuteGoQLAndVerify("H*d", 4, (Transform t) => t.name.StartsWith("H") && t.name.EndsWith("d"));
    }
    
//----------------------------------------------------------------------------------------------------------------------
    #region Exclusion

    [Test]
    public void ExclusionEndingWildcard() {
        TestUtility.ExecuteGoQLAndVerify("!Head*", 21, (Transform t) => !t.name.StartsWith("Head"));
    }

    [Test]
    public void EndingWildcardAndExclusionBeginningWildcard() {
        TestUtility.ExecuteGoQLAndVerify("Hea*!*d", 3, (Transform t) => t.name.StartsWith("Hea") && !t.name.EndsWith("d"));
    }

    [Test]
    public void EndingWildcardAndSingleExclusion() {
        TestUtility.ExecuteGoQLAndVerify("Hea*!Heat", 6, (Transform t) => t.name.StartsWith("Hea") && t.name!="Heat");
    }

    [Test]
    public void EndingWildcardAndDoubleExclusion() {
        TestUtility.ExecuteGoQLAndVerify("Hea*!Heat!Head", 3, (Transform t) => t.name.StartsWith("H") && t.name!="Heat" && t.name!="Head");
    }

    [Test]
    public void ExclusionInnerWildcard() {
        TestUtility.ExecuteGoQLAndVerify("!H*d", 22, (Transform t) => !t.name.StartsWith("H") || !t.name.EndsWith("d"));
    }
    
    #endregion    

//----------------------------------------------------------------------------------------------------------------------
    
    const string TestScenePath = "Packages/com.unity.selection-groups/Tests/Scenes/GoQLNameFilterTestScene";
    

}

} //end namespace