using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Unity.SelectionGroups
{

    public partial class SelectionGroupEditorWindow : EditorWindow
    {

        static SelectionGroupEditorWindow()
        {
            SelectionGroupContainer.onLoaded -= OnContainerLoaded;
            SelectionGroupContainer.onLoaded += OnContainerLoaded;
        }

        static void SanitizeSceneReferences()
        {
            foreach (var i in SelectionGroupContainer.instanceMap.ToArray())
            {
                var scene = i.Key;
                var container = i.Value;
                foreach (var g in container.groups)
                {
                    var name = g.Key;
                    var group = g.Value;
                    foreach (var o in group.objects.ToArray())
                    {
                        if (o != null && o.scene != scene)
                        {
                            group.objects.Remove(o);
                            SelectionGroupUtility.AddObjectToGroup(o, name);
                            EditorUtility.SetDirty(container);
                        }
                    }
                }
            }
        }

        static void OnContainerLoaded(SelectionGroupContainer container)
        {
            foreach (var name in container.groups.Keys.ToArray())
            {
                var mainGroup = SelectionGroupUtility.GetFirstGroup(name);
                var importedGroup = container.groups[name];
                importedGroup.color = mainGroup.color;
                importedGroup.selectionQuery = mainGroup.selectionQuery;
                importedGroup.showMembers = mainGroup.showMembers;
                container.groups[name] = importedGroup;
            }
            foreach (var i in SelectionGroupContainer.instanceMap.Values)
            {
                //clear all results so the queries can be refreshed with items from the new scene.
                foreach (var g in i.groups.Values)
                {
                    g.ClearQueryResults();
                }
                container.gameObject.hideFlags = HideFlags.None;
                EditorUtility.SetDirty(i);
            }
            if (editorWindow != null) editorWindow.Repaint();
        }

        internal static void MarkAllContainersDirty()
        {
            foreach (var container in SelectionGroupContainer.instanceMap.Values)
                EditorUtility.SetDirty(container);
        }

        internal static void UndoRecordObject(string msg)
        {
            foreach (var i in SelectionGroupContainer.instanceMap.Values)
                Undo.RecordObject(i, msg);
        }

        static void CreateNewGroup(Object[] objects)
        {
            UndoRecordObject("New Selection Group");
            var actualName = SelectionGroupUtility.CreateNewGroup("New Group");
            SelectionGroupUtility.AddObjectToGroup(objects, actualName);
            MarkAllContainersDirty();
        }


    }
}
