using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            this.wantsMouseMove = true;
        }


        void OnHierarchyChanged()
        {

        }

        void OnUndoRedoPerformed()
        {
            SelectionGroupManager.Reload();
        }

        void OnDisable()
        {
            editorWindow = null;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Repaint();
        }

        void OnFocus()
        {
        }

        void OnLostFocus()
        {
        }

        void OnGUI()
        {
            //Debug.Log(Event.current.type);
            // Profiler.BeginSample("SelectionGroupEditorWindow");
            SetupStyles();
            DrawGUI();

            switch (Event.current.type)
            {
                case EventType.ValidateCommand:
                    OnValidateCommand(Event.current);
                    break;
                case EventType.ExecuteCommand:
                    // Debug.Log($"Command: {Event.current.commandName}");
                    OnExecuteCommand(Event.current);
                    break;
            }
            Repaint();
            // Profiler.EndSample();
        }

        private void OnExecuteCommand(Event current)
        {
            switch (current.commandName)
            {
                case "SelectAll":
                    Selection.objects = activeSelectionGroup.ToArray();
                    UpdateActiveSelection();
                    current.Use();
                    break;
                case "DeselectAll":
                    Selection.objects = null;
                    UpdateActiveSelection();
                    current.Use();
                    break;
                case "InvertSelection":
                    Selection.objects = new HashSet<Object>(activeSelectionGroup).Except(Selection.objects).ToArray();
                    UpdateActiveSelection();
                    current.Use();
                    break;
                case "SoftDelete":
                    Undo.RegisterCompleteObjectUndo(SelectionGroupManager.instance, "Remove");
                    activeSelectionGroup.Remove(Selection.objects);
                    Selection.objects = null;
                    UpdateActiveSelection();
                    current.Use();
                    return;
            }
        }

        void UpdateActiveSelection()
        {
            activeSelection.Clear();
            if (Selection.objects != null)
                activeSelection.UnionWith(Selection.objects);
        }

        private void OnValidateCommand(Event current)
        {
            switch (current.commandName)
            {
                case "SelectAll":
                    current.Use();
                    return;
                case "DeselectAll":
                    current.Use();
                    return;
                case "InvertSelection":
                    current.Use();
                    return;
                case "SoftDelete":
                    current.Use();
                    return;
            }
            // Debug.Log(current.commandName);
        }

        void OnSelectionChange()
        {
            // activeSelection.Clear();
            // activeSelection.UnionWith(Selection.objects);

            // Below code will enable selection groups to be highlighted when one of their members is selected.
            // Need a more performant way to do this.
            // foreach(var g in SelectionGroupManager.instance)
            // {
            //     if (activeSelection.Intersect(g).Count() > 0)
            //     {
            //         activeNames.Add(g.name);
            //         continue;
            //     }
            // }
            // Repaint();
        }


    }
}
