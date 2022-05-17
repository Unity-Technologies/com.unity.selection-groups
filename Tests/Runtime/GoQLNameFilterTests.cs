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
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public IEnumerator SetUp()
    {
        Assert.IsTrue(System.IO.File.Exists($"{TestScenePath}.unity"));
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode($"{TestScenePath}.unity", 
            new LoadSceneParameters(LoadSceneMode.Single));
    }

    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void Simple() {
        TestUtility.ExecuteGoQLAndVerify("Head", 3, (Transform t) => t.name == "Head");
    }
    
    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void BeginningWildcard() {
        TestUtility.ExecuteGoQLAndVerify("*Head", 4, (Transform t) => t.name.EndsWith("Head"));
    }
    
    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void EndingWildcard() {
        TestUtility.ExecuteGoQLAndVerify("Head*", 5, (Transform t) => t.name.StartsWith("Head"));
    }
    
    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void BeginningAndEndingWildcard() {
        TestUtility.ExecuteGoQLAndVerify("*Head*", 6, (Transform t) => t.name.Contains("Head"));
    }
    
    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void InnerWildCard() {
        TestUtility.ExecuteGoQLAndVerify("H*d", 4, (Transform t) => t.name.StartsWith("H") && t.name.EndsWith("d"));
    }
    
//----------------------------------------------------------------------------------------------------------------------
    #region Exclusion

    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void ExclusionEndingWildcard() {
        TestUtility.ExecuteGoQLAndVerify("!Head*", 24, (Transform t) => !t.name.StartsWith("Head"));
    }

    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void EndingWildcardAndExclusionBeginningWildcard() {
        TestUtility.ExecuteGoQLAndVerify("Hea*!*d", 3, (Transform t) => t.name.StartsWith("Hea") && !t.name.EndsWith("d"));
    }

    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void EndingWildcardAndSingleExclusion() {
        TestUtility.ExecuteGoQLAndVerify("Hea*!Heat", 6, (Transform t) => t.name.StartsWith("Hea") && t.name!="Heat");
    }

    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void EndingWildcardAndDoubleExclusion() {
        TestUtility.ExecuteGoQLAndVerify("Hea*!Heat!Head", 3, (Transform t) => t.name.StartsWith("H") && t.name!="Heat" && t.name!="Head");
    }

    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void ExclusionInnerWildcard() {
        TestUtility.ExecuteGoQLAndVerify("!H*d", 25, (Transform t) => !t.name.StartsWith("H") || !t.name.EndsWith("d"));
    }
    
    #endregion    

//----------------------------------------------------------------------------------------------------------------------
    
    const string TestScenePath = "Packages/com.unity.selection-groups/Tests/Scenes/GoQLNameFilterTestScene";
    

}

} //end namespace