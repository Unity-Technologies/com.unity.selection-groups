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
    


}

} //end namespace