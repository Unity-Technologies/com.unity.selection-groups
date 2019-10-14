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

        void OnEnable()
        {
            titleContent.text = "Selection Groups";
            editorWindow = this;
        }

        void OnHierarchyChange()
        {
            //This is required to preserve refences when a gameobject is moved between scenes in the editor.
            SanitizeSceneReferences();
        }

        void OnDisable()
        {
            SelectionGroupContainer.onLoaded -= OnContainerLoaded;
            editorWindow = null;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {

        }

        void OnSelectionChange()
        {
            activeSelection.Clear();
            activeSelection.UnionWith(Selection.objects);
            var names = SelectionGroupUtility.GetGroupNames();
            activeNames.Clear();
            foreach (var n in names)
            {
                var members = SelectionGroupUtility.GetGameObjects(n);
                if (activeSelection.Intersect(members).Count() > 0)
                {
                    activeNames.Add(n);
                    continue;
                }
                var queryMembers = SelectionGroupUtility.GetQueryObjects(n);
                if (activeSelection.Intersect(queryMembers).Count() > 0)
                {
                    activeNames.Add(n);
                    continue;
                }
            }
            Repaint();
        }

        void OnGUI()
        {
            SetupStyles();
            DrawGUI();

            //Unlike other drag events, this DragExited should be handled once per frame.
            if (Event.current.type == EventType.DragExited)
            {
                ExitDrag();
                Event.current.Use();
            }

            if (focusedWindow == this)
                Repaint();

            if (Event.current.type == EventType.Repaint)
                EditorApplication.delayCall += PerformSelectionCommands;
        }


    }
}
