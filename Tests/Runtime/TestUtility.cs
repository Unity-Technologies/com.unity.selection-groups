using System;
using System.Collections.Generic;
using Unity.GoQL;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.SelectionGroups.Tests {

internal static class TestUtility
{

    internal static GameObject[] ExecuteGoQLAndVerify(string goql, int numExpectedResults) {
        GoQLExecutor e       = new GoQLExecutor(goql);
        GameObject[] results = e.Execute();
        Assert.AreEqual(ParseResult.OK, e.parseResult);
        Assert.AreEqual(numExpectedResults, results.Length);
        return results;
    }
    
    internal static List<Transform> ExecuteGoQLAndVerify(string goql, int numExpectedResults, Func<Transform, bool> validFunc) {
        GoQLExecutor e       = new GoQLExecutor(goql);
        GameObject[] results = e.Execute();

        Assert.AreEqual(ParseResult.OK, e.parseResult);
        List<Transform> ret = new List<Transform>(results.Length);
        foreach (GameObject go in results) {
            Transform t = go.transform;
            Assert.IsTrue(validFunc(t));
            ret.Add(t);

        }

        return ret;
            
    }
    
    


}

} //end namespace