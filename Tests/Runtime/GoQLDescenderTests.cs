using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Unity.SelectionGroups.Tests 
{
internal class GoQLDescenderTests
{
    
    [UnitySetUp]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public IEnumerator SetUp()
    {
        Assert.IsTrue(System.IO.File.Exists($"{TestScenePath}.unity"));
        //[TODO-sin: 2022-5-17] Reduce code
#if UNITY_EDITOR
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode($"{TestScenePath}.unity", 
            new LoadSceneParameters(LoadSceneMode.Single));
#else
        yield return null;
#endif
    }


    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void Root() {
        TestUtility.ExecuteGoQLAndVerify("/Head", 1, (Transform t) => (t.parent == null) && t.name == "Head");
    }
    
    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void AllChildren() {
        TestUtility.ExecuteGoQLAndVerify("Head/", 11, (Transform t) => t.parent.name == "Head");
    }

//----------------------------------------------------------------------------------------------------------------------
    
    const string TestScenePath = "Packages/com.unity.selection-groups/Tests/Scenes/GoQLDescenderTestScene";
    
    

}

} //end namespace