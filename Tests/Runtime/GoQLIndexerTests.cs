﻿using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif


namespace Unity.SelectionGroups.Tests 
{
internal class GoQLIndexerTests
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
    public void FirstChild() {
        TestUtility.ExecuteGoQLAndVerify("Head/[0]", 2, 
            (Transform t) => null!=t.parent && t.parent.name == "Head" && t.name.EndsWith("Child (0)")
        );
    }
    
    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void IndexedChildren() {
        int[] indexes = new[] { 0, 1, 5 };       
        List<Transform> results = TestUtility.ExecuteGoQLAndVerify($"Head/[{string.Join(",", indexes)}]", 6, 
            (Transform t) => null!=t.parent && t.parent.name == "Head"
        );

        int numIndexes = indexes.Length;
        foreach (Transform t in results) {
            bool found = false;

            for (int i = 0; !found && i < numIndexes; ++i) {
                if (t.name.EndsWith($"Child ({indexes[i]})"))
                    found = true;
            }
            Assert.IsTrue(found);
        } 
    }
    
    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void NoDuplicateIndexedChildren()
    {
        TestUtility.ExecuteGoQLAndVerify("/[0,1,0]", 2);
    }
    
    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void ExclusionOperator()
    {
        TestUtility.ExecuteGoQLAndVerify("/[0:3]", 3);
        TestUtility.ExecuteGoQLAndVerify("/[0:3,!1,!0]", 1,(t) => t.name == "Head");
    }
    
    
    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void LastChild()
    {
        TestUtility.ExecuteGoQLAndVerify("Head/[-1]", 2, (Transform t) => null!=t.parent && t.parent.name == "Head");
    }
    
    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]    
    public void RangedChildren() {
        int startIndex = 3;
        int endIndex   = 5;
            
        List<Transform> results = TestUtility.ExecuteGoQLAndVerify($"Head/[{startIndex}:{endIndex}]", 4, 
            (Transform t) => null!=t.parent && t.parent.name == "Head"
        );
        foreach (Transform t in results) {
            bool found = false;

            for (int i = startIndex; !found && i < endIndex; ++i) {
                if (t.name.EndsWith($"Child ({i})"))
                    found = true;
            }
            Assert.IsTrue(found);
        }         
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    const string TestScenePath = "Packages/com.unity.selection-groups/Tests/Scenes/GoQLIndexerTestScene";

}

} //end namespace