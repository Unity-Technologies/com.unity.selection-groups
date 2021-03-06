﻿using System.Collections.Generic;
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
            SelectionGroupManager.Create -= RepaintOnCreate;
            SelectionGroupManager.Create += RepaintOnCreate;
            SelectionGroupManager.Delete -= RepaintOnDelete;
            SelectionGroupManager.Delete += RepaintOnDelete;
            
            editorHeaderContent = EditorGUIUtility.IconContent("d_Project");
            sceneHeaderContent = EditorGUIUtility.IconContent("SceneAsset Icon");
        }

        void RepaintOnDelete(ISelectionGroup @group) => 
            Repaint();

        void RepaintOnCreate(SelectionGroupDataLocation scope, string s, string query, Color color, IList<Object> members) =>
            Repaint();

        void OnDisable()
        {
            SelectionGroupManager.Create -= RepaintOnCreate;
            SelectionGroupManager.Delete -= RepaintOnDelete;
        }
        
        void OnGUI()
        {
            try
            {
                Profiler.BeginSample("Selection Groups Editor Window");
                
                var e = Event.current;
                if (e.type == EventType.Layout) return;
                
                isReadOnly = EditorApplication.isPlayingOrWillChangePlaymode;

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
            if (activeSelectionGroup != null)
                switch (current.commandName)
                {
                    case "SelectAll":
                        Selection.objects = activeSelectionGroup.Members.ToArray();
                        UpdateActiveSelection();
                        current.Use();
                        break;
                    case "DeselectAll":
                        Selection.objects = null;
                        UpdateActiveSelection();
                        current.Use();
                        break;
                    case "InvertSelection":
                        Selection.objects = new HashSet<Object>(activeSelectionGroup.Members).Except(Selection.objects).ToArray();
                        UpdateActiveSelection();
                        current.Use();
                        break;
                    case "SoftDelete":
                        if (!isReadOnly)
                        {
                            activeSelectionGroup.Remove(Selection.objects);
                            Selection.objects = null;
                            UpdateActiveSelection();
                            current.Use();
                        }
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
