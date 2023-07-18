using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.FilmInternalUtilities;
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
            
            sceneHeaderContent     =  EditorGUIUtility.IconContent("SceneAsset Icon");
            m_CreateDropdownContent = EditorGUIUtility.IconContent("CreateAddNew");
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
                        OnValidateCommand(evt);
                        break;
                    case EventType.ExecuteCommand:
                        OnExecuteCommand(evt);
                        break;
                    case EventType.KeyDown:
                        OnKeyDown(evt);
                        break;
                    case EventType.DragUpdated:
                        OnDragUpdated(evt);
                        break;
                    case EventType.DragPerform:
                        OnDragPerform(evt);
                        break;
                }
            }
            finally
            {
                Profiler.EndSample();
            }

            
        }

        private void OnDragPerform(Event evt)
        {
            CreateNewGroup(DragAndDrop.objectReferences);
        }

        private void OnDragUpdated(Event evt)
        {
            var      allowDropOp = true;
            Object[] objects     = DragAndDrop.objectReferences;
            int      numObjects  = objects.Length;
            for (int i = 0; i < numObjects; ++i) {
                GameObject go = objects[i] as GameObject;
                if (null != go)
                    continue;
                allowDropOp = false;
                break;
            }

            if (allowDropOp)
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            else
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;

        }

        private void OnKeyDown(Event evt)
        {
            if (Event.current.keyCode == (KeyCode.Delete))
            {
                if (null != m_activeSelectionGroup)
                {
                    DeleteGroup(m_activeSelectionGroup);
                }
                else
                {
                    RemoveSelectedMembersFromGroup();
                }

                evt.Use();
            }
        }

        void OnExecuteCommand(Event current)
        {
            switch (current.commandName) {
                case "SelectAll":
                    SelectionGroupManager.GetOrCreateInstance().Groups.Loop((SelectionGroup group) => {
                        m_selectedGroupMembers.AddGroupMembers(group);
                    });
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

                    SelectionGroupManager.GetOrCreateInstance().Groups.Loop((SelectionGroup group) => {
                        group.Members.Loop((GameObject m) => {
                            if (prevSelectedMembers.Contains(group, m))
                                return;
                            m_selectedGroupMembers.AddObject(group,m);
                        });
                    });
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
