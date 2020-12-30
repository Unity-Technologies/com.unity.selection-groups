using System.Collections.Generic;
using System.Linq;
using Unity.SelectionGroups;
using Unity.SelectionGroups.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Unity.SelectionGroupsEditor
{

    public partial class SelectionGroupEditorWindow : EditorWindow
    {

        void OnEnable()
        {
            titleContent.text = "Selection Groups";
            editorWindow = this;
            wantsMouseMove = true;
            SelectionGroupManager.Create -= RepaintOnCreate;
            SelectionGroupManager.Create += RepaintOnCreate;
            SelectionGroupManager.Delete -= RepaintOnDelete;
            SelectionGroupManager.Delete += RepaintOnDelete;
        }

        void RepaintOnDelete(ISelectionGroup @group) => 
            Repaint();

        void RepaintOnCreate(SelectionGroupScope scope, string s, string query, Color color, IList<Object> members) =>
            Repaint();

        void OnDisable()
        {
            SelectionGroupManager.Create -= RepaintOnCreate;
            SelectionGroupManager.Delete -= RepaintOnDelete;
            editorWindow = null;
        }
        
        void OnGUI()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                GUILayout.Label("Selection Groups are not available in Play Mode.");
                return;
            }
            
            SetupStyles();
            DrawGUI();

            switch (Event.current.type)
            {
                case EventType.ValidateCommand:
                    OnValidateCommand(Event.current);
                    break;
                case EventType.ExecuteCommand:
                    OnExecuteCommand(Event.current);
                    break;
            }
            Repaint();
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
                        activeSelectionGroup.Remove(Selection.objects);
                        Selection.objects = null;
                        UpdateActiveSelection();
                        current.Use();
                        return;
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
        }
    }
}
