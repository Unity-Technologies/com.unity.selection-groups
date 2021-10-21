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
    public class GoQLExampleTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            //Debug.Log("Loading Test Scene.");
            Assert.IsTrue(System.IO.File.Exists($"{SelectionGroupsTestsConstants.TestScenePath}.unity"));
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode($"{SelectionGroupsTestsConstants.TestScenePath}.unity", 
                new LoadSceneParameters(LoadSceneMode.Single));
        }
 
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode($"{SelectionGroupsTestsConstants.EmptyScenePath}.unity", 
                new LoadSceneParameters(LoadSceneMode.Single));
        }
        
        [Test]
        public void TestGoQLExamples11()
        {
            var e = new GoQLExecutor("Head<t:Collider>");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(1, results.Length);
            Assert.IsTrue(results[0].GetComponent<Collider>() != null);
        }
        
        [Test]
        public void TestGoQLExamples12()
        {
            var e = new GoQLExecutor("Head<m:Glow>");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(1, results.Length);
            Assert.IsTrue(results[0].GetComponent<MeshRenderer>() != null);
            Assert.IsTrue(results[0].GetComponent<MeshRenderer>().sharedMaterial.name == "Glow");
        }
        
        [Test]
        public void TestGoQLExamples13()
        {
            var e = new GoQLExecutor("Head<s:Standard>");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(1, results.Length);
            Assert.IsTrue(results[0].GetComponent<MeshRenderer>() != null);
            Assert.IsTrue(results[0].GetComponent<MeshRenderer>().sharedMaterial.shader.name == "Standard");
        }
        
        [Test]
        public void TestGoQLExamples14()
        {
            var e = new GoQLExecutor("/");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(7, results.Length);
        }
        
        [Test]
        public void TestGoQLExamples15()
        {
            var e = new GoQLExecutor("Quad*/<t:AudioSource>[1]");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(1, results.Length);
            Assert.IsTrue(results[0].GetComponent<AudioSource>() != null);
            
        }
        
        [Test]
        public void TestGoQLExamples16()
        {
            var e = new GoQLExecutor("<t:Transform, t:AudioSource>");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(8, results.Length);
            Assert.IsTrue(results[0].GetComponent<AudioSource>() != null);
            Assert.IsTrue(results[1].GetComponent<AudioSource>() != null);
            Assert.IsTrue(results[2].GetComponent<AudioSource>() != null);
        }
        
        [Test]
        public void TestGoQLExamples17()
        {
            var e = new GoQLExecutor("<t:Renderer>/*Audio*/[0:3]");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("GameObject", results[0].name);
        }
        
        [Test]
        public void TestGoQLExamples18()
        {
            var e = new GoQLExecutor("Cube/Quad/<t:AudioSource>[-1]");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("ChildWithAudio (2)", results[0].name);
        }
        
        [Test]
        public void TestGoQLExamples19()
        {
            var e = new GoQLExecutor("<m:Skin>");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(3, results.Length);
            
        }
        
        [Test]
        public void TestGoQLExamples20()
        {
            var e = new GoQLExecutor("/Environment/**<t:MeshRenderer>");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(8, results.Length);
        }
        
        [Test]
        public void TestGoQLExamples21()
        {
            var e = new GoQLExecutor("Env*ent");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(2, results.Length);
            Assert.AreEqual("Environment", results[0].name);
            Assert.AreEqual("Environment", results[1].name);
        }
        
        
        [Test]
        public void TestGoQLExamples22()
        {
            var e = new GoQLExecutor("/Head*!*Unit");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("Head", results[0].name);
        }
        
        

    }

} //end namespace