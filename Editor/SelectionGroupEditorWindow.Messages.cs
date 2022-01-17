﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.SelectionGroups;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;


namespace Unity.SelectionGroups.Editor
{

    internal partial class SelectionGroupEditorWindow : EditorWindow
    {

        void OnEnable()
        {
            titleContent.text = "Selection Groups";
            wantsMouseMove = true;
            
            //[TODO-sin:2021-12-20] Remove in version 0.7.0             
            //editorHeaderContent = EditorGUIUtility.IconContent("d_Project");
            sceneHeaderContent     =  EditorGUIUtility.IconContent("SceneAsset Icon");
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private void OnDisable() {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }


        void OnGUI()
        {
            try
            {
                Profiler.BeginSample("Selection Groups Editor Window");
                
                Event evt = Event.current;
                if (evt.type == EventType.Layout) return;
                
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
                    case EventType.KeyDown: {
                        if (Event.current.keyCode == (KeyCode.Delete)) {
                            if (null != m_activeSelectionGroup) {
                                DeleteGroup(m_activeSelectionGroup);
                            } else {
                                RemoveSelectedMembersFromGroup();  
                            }
                            evt.Use();
                        }
                        break;
                    }                
                }
            }
            finally
            {
                Profiler.EndSample();
            }

            
        }

        void OnExecuteCommand(Event current)
        {
            switch (current.commandName) {
                case "SelectAll":
                    foreach (SelectionGroup group in SelectionGroupManager.GetOrCreateInstance().Groups) {
                        m_selectedGroupMembers.AddGroupMembers(group);
                    }
                    UpdateUnityEditorSelectionWithMembers();
                    current.Use();
                    break;
                case "DeselectAll":
                    ClearSelectedMembers();
                    current.Use();
                    break;
                case "InvertSelection":
                    GroupMembersSelection prevSelectedMembers = new GroupMembersSelection(m_selectedGroupMembers);
                    m_selectedGroupMembers.Clear();
                    
                    foreach (SelectionGroup group in SelectionGroupManager.GetOrCreateInstance().Groups) {
                        foreach (Object m in group.Members) {
                            if (prevSelectedMembers.Contains(group, m))
                                continue;
                            m_selectedGroupMembers.AddObject(group,m);
                        }
                    }
                    current.Use();
                    break;
            }
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
            }
        }
        
        private void OnUndoRedoPerformed() {
            Repaint();
        }
        
    }
}
