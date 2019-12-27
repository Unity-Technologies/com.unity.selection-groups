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
        }


        void OnHierarchyChanged()
        {
            //This is required to preserve refences when a gameobject is moved between scenes in the editor.
            SanitizeSceneReferences();
            Repaint();
        }

        void OnUndoRedoPerformed()
        {
            SelectionGroupManager.Reload();
            Repaint();
        }

        void OnDisable()
        {
            editorWindow = null;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            SelectionGroupManager.instance.Save();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Repaint();
        }

        void OnGUI()
        {
            // Profiler.BeginSample("SelectionGroupEditorWindow");
            SetupStyles();
            DrawGUI();

            switch (Event.current.type)
            {
                //Unlike other drag events, this DragExited should be handled once per frame.
                case EventType.DragExited:
                    ExitDrag();
                    Event.current.Use();
                    break;
                case EventType.Repaint:
                    EditorApplication.delayCall += PerformSelectionCommands;
                    break;
                case EventType.ValidateCommand:
                    OnValidateCommand(Event.current);
                    break;
                case EventType.ExecuteCommand:
                    OnExecuteCommand(Event.current);
                    break;
            }

            //Make sure window repaints after any user action.
            if (focusedWindow == this && (Event.current.isMouse || Event.current.isKey))
                Repaint();

            // Profiler.EndSample();
        }

        private void OnExecuteCommand(Event current)
        {
            switch (current.commandName)
            {
                case "SelectAll":
                    Selection.objects = activeSelectionGroup.ToArray();
                    current.Use();
                    break;
                case "DeselectAll":
                    Selection.objects = null;
                    current.Use();
                    break;
                case "InvertSelection":
                    Selection.objects = new HashSet<Object>(activeSelectionGroup).Except(Selection.objects).ToArray();
                    current.Use();
                    break;
                case "SoftDelete":
                    Undo.RegisterCompleteObjectUndo(SelectionGroupManager.instance, "Remove");
                    activeSelectionGroup.Remove(Selection.objects);
                    current.Use();
                    return;
            }
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
            activeSelection.Clear();
            activeSelection.UnionWith(Selection.objects);

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
            Repaint();
        }


    }
}
