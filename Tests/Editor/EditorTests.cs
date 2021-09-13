using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.GoQL;
using UnityEngine;
using UnityEngine.TestTools;


namespace Tests
{
    public class EditorTests
    {
        public string[] test_queries = new[]
        {
            "Head",
            "*Head",
            "Head*",
            "*Head*",
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
        
        
        [Test]
        public void TestGoQLExamples()
        {
            for (var i = 0; i < test_queries.Length; i++)
            {
                var q = test_queries[i];
                var e = new GoQLExecutor();
                e.Code = q;
                var results = e.Execute();
                Assert.AreEqual(ParseResult.OK, e.parseResult);
            }
        }

        
    }
} //end namespace