using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.GoQL;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;


namespace Tests
{
    public class EditorTests
    {

        public string[] test_queries = new[]
        {
            "/Head",
            "Head/",
            "Head/[0]",
            "Head/[0,1,5]",
            "Head/[-1]",
            "Head/[3:5]",
            "Head<t:Collider>",
            "Head<m:Glow>",
            "Head<s:Standard>",
            "/",
            "Quad*/<t:AudioSource>[1]",
            "<t:Transform, t:AudioSource>",
            "<t:Renderer>/*Audio*/[0:3]",
            "Cube/Quad/<t:AudioSource>[-1]",
            "<m:Skin>",
            "/Environment/**<t:MeshRenderer>",
        };

        // public object[] expected_test_results = new[]
        // {
        //     Head | FilterName
        //     *Head | FilterName
        //     Head* | FilterName
        //     *Head* | FilterName
        //     EnterChildren | Head | FilterName
        //     Head | FilterName | EnterChildren
        //     Head | FilterName | EnterChildren | 0 | 1 | FilterIndex
        //     Head | FilterName | EnterChildren | 0 | 1 | 5 | 3 | FilterIndex
        //     Head | FilterName | EnterChildren | -1 | 1 | FilterIndex
        //     Head | FilterName | EnterChildren | Unity.GoQL.Range | 1 | FilterIndex
        //     Head | FilterName | t:Collider | 1 | FilterByDiscriminators
        //     Head | FilterName | m:Glow | 1 | FilterByDiscriminators
        //     Head | FilterName | s:Standard | 1 | FilterByDiscriminators
        //     EnterChildren
        //     Quad* | FilterName | EnterChildren | t:AudioSource | 1 | FilterByDiscriminators | 1 | 1 | FilterIndex
        //     t:Transform | t:AudioSource | 2 | FilterByDiscriminators
        //     t:Renderer | 1 | FilterByDiscriminators | EnterChildren | *Audio* | FilterName | EnterChildren | Unity.GoQL.Range | 1 | FilterIndex
        //     Cube | FilterName | EnterChildren | Quad | FilterName | EnterChildren | t:AudioSource | 1 | FilterByDiscriminators | -1 | 1 | FilterIndex
        //     m:Skin | 1 | FilterByDiscriminators
        //     EnterChildren | Environment | FilterName | EnterChildren | CollectAllAncestors | t:MeshRenderer | 1 | FilterByDiscriminators
        // };


        [Test]
        public void TestGoQLExamples1()
        {
            var e = new GoQLExecutor("Head");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(2, results.Length);
        }
        
        [Test]
        public void TestGoQLExamples2()
        {
            var e = new GoQLExecutor("*Head");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(3, results.Length);
        }
        
        [Test]
        public void TestGoQLExamples3()
        {
            var e = new GoQLExecutor("Head*");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(3, results.Length);
        }
        
        [Test]
        public void TestGoQLExamples4()
        {
            var e = new GoQLExecutor("*Head*");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(4, results.Length);
        }
        
        [Test]
        public void TestGoQLExamples5()
        {
            var e = new GoQLExecutor("/Head");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(1, results.Length);
        }
        
        [Test]
        public void TestGoQLExamples6()
        {
            var e = new GoQLExecutor("Head/");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(11, results.Length);
        }
        
        [Test]
        public void TestGoQLExamples7()
        {
            var e = new GoQLExecutor("Head/[0]");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(2, results.Length);
            Assert.AreEqual("Child (0)", results[0].name);
        }
        
        [Test]
        public void TestGoQLExamples8()
        {
            var e = new GoQLExecutor("Head/[0,1,5]");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(5, results.Length);
            // Assert.AreEqual("Child (0)", results[0].name);
            // Assert.AreEqual("Child (1)", results[1].name);
            // Assert.AreEqual("Child (5)", results[2].name);
        }
        
        [Test]
        public void TestGoQLExamples9()
        {
            var e = new GoQLExecutor("Head/[-1]");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("Head", results[0].name);
        }
        
        [Test]
        public void TestGoQLExamples10()
        {
            var e = new GoQLExecutor("Head/[3:5]");
            var results = e.Execute();
            Assert.AreEqual(ParseResult.OK, e.parseResult);
            Assert.AreEqual(4, results.Length);
            Assert.AreEqual("Child (3)", results[0].name);
            Assert.AreEqual("Quad 1", results[1].name);
            Assert.AreEqual("Child (4)", results[2].name);
            Assert.AreEqual("Quad 2", results[3].name);
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

    }

} //end namespace