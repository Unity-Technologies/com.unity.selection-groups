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
    internal class GoQLOtherExamplesTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Assert.IsTrue(System.IO.File.Exists($"{SelectionGroupsTestsConstants.TestScenePath}.unity"));
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode($"{SelectionGroupsTestsConstants.TestScenePath}.unity", 
                new LoadSceneParameters(LoadSceneMode.Single));
        }
         
        [Test]
        public void RootGameObjects() {
            TestUtility.ExecuteGoQLAndVerify("/", 7);
        }
        
        [Test]
        public void FromQuadWildcardGetSecondChildWithAudioSource()
        {
            GameObject[] results = TestUtility.ExecuteGoQLAndVerify("Quad*/<t:AudioSource>[1]", 1);
            Assert.IsTrue(results[0].GetComponent<AudioSource>() != null);
            
        }
        
        [Test]
        public void GameObjectsHavingTransformAndAudioSource()
        {
            GameObject[] results = TestUtility.ExecuteGoQLAndVerify("<t:Transform, t:AudioSource>", 8);
            Assert.IsTrue(results[0].GetComponent<AudioSource>() != null);
            Assert.IsTrue(results[1].GetComponent<AudioSource>() != null);
            Assert.IsTrue(results[2].GetComponent<AudioSource>() != null);
        }
        
        [Test]
        public void FromRendererGetAudioWildcardThenGetRangedChildren()
        {
            GameObject[] results = TestUtility.ExecuteGoQLAndVerify("<t:Renderer>/*Audio*/[0:3]", 1);
            Assert.AreEqual("GameObject", results[0].name);
        }
        
        [Test]
        public void FromCubeGetQuadThenGetLastAudioSource()
        {
            GameObject[] results = TestUtility.ExecuteGoQLAndVerify("Cube/Quad/<t:AudioSource>[-1]", 1);
            Assert.AreEqual("ChildWithAudio (2)", results[0].name);
        }
        
        [Test]
        public void SkinMaterial()
        {
            GameObject[] results = TestUtility.ExecuteGoQLAndVerify("<m:Skin>", 3);
            Assert.AreEqual(3, results.Length);
            
        }
        
        [Test]
        public void FromEnvironmentGetMeshRenderer()
        {
            TestUtility.ExecuteGoQLAndVerify("/Environment/**<t:MeshRenderer>", 8);
        }
        
        [Test]
        public void InnerWildcard()
        {
            GameObject[] results = TestUtility.ExecuteGoQLAndVerify("Env*ent", 2);
            Assert.AreEqual("Environment", results[0].name);
            Assert.AreEqual("Environment", results[1].name);
        }
        
        
        [Test]
        public void WildcardsWithExclusion()
        {
            GameObject[] results = TestUtility.ExecuteGoQLAndVerify("/Head*!*Unit", 1);
            Assert.AreEqual("Head", results[0].name);
        }
        
        

    }

} //end namespace