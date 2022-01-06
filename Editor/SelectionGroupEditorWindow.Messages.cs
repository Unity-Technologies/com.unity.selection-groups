using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.SelectionGroups;
using Unity.SelectionGroups.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;


namespace Unity.SelectionGroupsEditor
{

    internal partial class SelectionGroupEditorWindow : EditorWindow
    {

        void OnEnable()
        {
            titleContent.text = "Selection Groups";
            wantsMouseMove = false;
            
            //[TODO-sin:2021-12-20] Remove in version 0.7.0             
            //editorHeaderContent = EditorGUIUtility.IconContent("d_Project");
            sceneHeaderContent = EditorGUIUtility.IconContent("SceneAsset Icon");
        }
        
        
        void OnGUI()
        {
            try
            {
                Profiler.BeginSample("Selection Groups Editor Window");
                
                var e = Event.current;
                if (e.type == EventType.Layout) return;
                
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
            }
            finally
            {
                Profiler.EndSample();
            }

            
        }

        void OnExecuteCommand(Event current)
        {
            if (m_activeSelectionGroup != null)
                switch (current.commandName)
                {
                    case "SelectAll":
                        Selection.objects = m_activeSelectionGroup.Members.ToArray();
                        UpdateActiveSelection();
                        current.Use();
                        break;
                    case "DeselectAll":
                        Selection.objects = null;
                        UpdateActiveSelection();
                        current.Use();
                        break;
                    case "InvertSelection":
                        Selection.objects = new HashSet<Object>(m_activeSelectionGroup.Members).Except(Selection.objects).ToArray();
                        UpdateActiveSelection();
                        current.Use();
                        break;
                    case "SoftDelete":
                        m_activeSelectionGroup.Remove(Selection.objects);
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
            // activeSelection.Clear();
            // if (Selection.objects != null)
            //     activeSelection.UnionWith(Selection.objects);
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
                    if (m_activeSelectionGroup != null)
                        current.Use();
                    return;
            }
        }
    }
}
