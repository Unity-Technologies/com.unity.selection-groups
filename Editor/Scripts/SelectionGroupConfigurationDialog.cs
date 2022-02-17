using System;
using System.Linq;
using System.Reflection;
using Unity.SelectionGroups;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.SelectionGroups.Editor
{
    /// <summary>
    /// Implements the configuration dialog in the editor for a selection group. 
    /// </summary>
    public class SelectionGroupConfigurationDialog : EditorWindow
    {
        [SerializeField] private int m_GroupId;
        
        private ReorderableList m_ExclusionList;
        private SelectionGroup m_Group;
        private GoQL.GoQLExecutor m_Executor = new GoQL.GoQLExecutor();
        private SelectionGroupEditorWindow m_ParentWindow;
        private string m_Message = string.Empty;
        private bool m_RefreshQuery = true;
        private bool m_ShowDebug = false;
        private SelectionGroupDebugInformation m_DebugInformation;

        private void OnEnable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            m_RefreshQuery = true;
            Repaint();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void OnGUI()
        {
            if (m_Group == null)
            {
                Close();
                return;
            }

            //Null for SelectionGroup (scene), which is a MonoBehaviour, must be checked against null using its type
            if (m_Group is Component component && component == null){
                Close();
                return;                
            }


            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                GUILayout.Label("Selection Group Properties", EditorStyles.largeLabel);
                m_Group.groupName = EditorGUILayout.TextField("Group Name", m_Group.groupName);
                m_Group.color = EditorGUILayout.ColorField("Color", m_Group.color);
                EditorGUILayout.LabelField("GameObject Query");
                var q = m_Group.query;
                var newQuery = EditorGUILayout.TextField(m_Group.query);
                m_RefreshQuery = m_RefreshQuery || (q != newQuery);
                
                if (m_RefreshQuery)
                {
                    {
                        var obj = m_Group as Object;
                        
                        Undo.RegisterCompleteObjectUndo(obj, "Query change");
                        
                    }
                }
                if (m_Message != string.Empty)
                {
                    GUILayout.Space(5);
                    EditorGUILayout.HelpBox(m_Message, MessageType.Info);
                }
                GUILayout.Space(5);
                
                m_ShowDebug = GUILayout.Toggle(m_ShowDebug, "Show Debug Info", "button");
                if (m_ShowDebug)
                {
                    if (m_DebugInformation == null) m_DebugInformation = new SelectionGroupDebugInformation(m_Group);
                    EditorGUILayout.TextArea(m_DebugInformation.text);
                }
                else
                {
                    m_DebugInformation = null;
                }
            }
        }
    }
}

