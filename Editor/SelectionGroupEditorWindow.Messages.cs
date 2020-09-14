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
            this.wantsMouseMove = true;
        }

        void OnDisable()
        {
            editorWindow = null;
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
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                GUILayout.Label("Selection Groups are not available in Play Mode.");
                return;
            }
            if (SelectionGroupManager.instance == null)
            {
                GUILayout.Label("Waiting for SelectionGroupManager to load.");
                return;
            }
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

        void OnExecuteCommand(Event current)
        {
            if (activeSelectionGroup != null)
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

        void Update()
        {
            if (SelectionGroupManager.instance == null)
            {
                SelectionGroupManager.CreateAndLoad();
                Repaint();
            }

        }

        void OnSelectionChange()
        {
            UpdateActiveSelection();
        }

        void UpdateActiveSelection()
        {
            activeSelection.Clear();
            if (Selection.objects != null)
                activeSelection.UnionWith(Selection.objects);
        }

        void OnValidateCommand(Event current)
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
                    if (activeSelectionGroup != null)
                        current.Use();
                    return;
            }
            // Debug.Log(current.commandName);
        }

    }
}
