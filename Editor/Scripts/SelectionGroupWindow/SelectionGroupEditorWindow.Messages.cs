using System;
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
            
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private void OnDisable() {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }

        private void OnDragPerform(Event evt)
        {
            CreateNewGroup(DragAndDrop.objectReferences);
        }

        private void OnDragUpdated(Event evt)
        {
            var allowDropOp = true;
            foreach (var o in DragAndDrop.objectReferences)
            {
                if (!(o is GameObject))
                {
                    allowDropOp = false;
                    break;
                }
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
                    foreach (SelectionGroup group in SelectionGroupManager.GetOrCreateInstance().Groups) {
                        m_selectedGroupMembers.UnionWith(group.Members.Select(m=> GroupMembership.Child(group, m)));
                    }
                    UpdateUnityEditorSelectionWithMembers();
                    current.Use();
                    break;
                case "DeselectAll":
                    ClearSelectedMembers();
                    current.Use();
                    break;
                case "InvertSelection":
                    var prevSelectedMembers = new HashSet<GroupMembership>(m_selectedGroupMembers);
                    m_selectedGroupMembers.Clear();
                    
                    foreach (SelectionGroup group in SelectionGroupManager.GetOrCreateInstance().Groups) {
                        foreach (GameObject m in group.Members) {
                            m_selectedGroupMembers.Add(GroupMembership.Child(group,m));
                        }
                    }
                    m_selectedGroupMembers.ExceptWith(prevSelectedMembers);
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
            Refresh();
            Repaint();
        }
        
    }
}
