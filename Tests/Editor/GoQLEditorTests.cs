using System.Collections;
using NUnit.Framework;
using Unity.SelectionGroups;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class GoQLEditorTests
    {
        [UnityTest]
        public IEnumerator UndoGoQLQuery()
        {
            var groupManager = SelectionGroupManager.GetOrCreateInstance();
            groupManager.ClearGroups();
            var group = groupManager.CreateSelectionGroup("TestGroup", Color.green);
            Undo.RegisterCompleteObjectUndo(group, "Query change");
            group.SetQuery("/");
            Assert.AreEqual("/", group.Query);
            yield return null;
            Undo.PerformUndo();
            yield return null;
            Assert.AreEqual("", group.Query);
        }
    }
}