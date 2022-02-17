using System;
using System.Reflection;
using Unity.FilmInternalUtilities.Editor;
using Unity.SelectionGroups;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
    
namespace Unity.SelectionGroups.Editor 
{
    [CustomEditor(typeof(SelectionGroups.SelectionGroup))]
    internal class SelectionGroupInspector : UnityEditor.Editor 
    {
        private SelectionGroup m_Group;
        private bool m_ShowDebug  = false;
        private SelectionGroupDebugInformation m_DebugInformation;
        
        private void OnEnable() 
        {
            m_Group = target as SelectionGroup;
        }
        
        public override void OnInspectorGUI() 
        {
            serializedObject.Update();

            bool repaintWindow = EditorGUIDrawerUtility.DrawUndoableGUI(m_Group.gameObject, "Group Name",
                () => EditorGUILayout.TextField("Group Name", m_Group.groupName),
                (string groupName) => { m_Group.groupName = groupName; }
            );
            
            repaintWindow |= EditorGUIDrawerUtility.DrawUndoableGUI(m_Group, "Group Color",
                () => EditorGUILayout.ColorField("Color", m_Group.color),
                (Color groupColor) => { m_Group.color = groupColor; }
            );

            repaintWindow |= EditorGUIDrawerUtility.DrawUndoableGUI(m_Group, "Group Query",
                () => EditorGUILayout.TextField("Group Query", m_Group.query),
                (string query) => 
                {
                    {
                        Undo.RegisterCompleteObjectUndo(m_Group, "Query change");
                    }
                    m_Group.SetQuery(query);
                }
            );

            //Query results
            if (m_Group.IsAutoFilled()) 
            {
                GoQL.ParseResult parseResult = m_Group.GetLastQueryParseResult();
                
                string message = (parseResult == GoQL.ParseResult.OK) 
                    ? $"{m_Group.Members.Count} results." 
                    : parseResult.ToString();

                GUILayout.Space(5);
                EditorGUILayout.HelpBox(message, MessageType.Info);
            }
                    
            GUILayout.BeginVertical("box");
            GUILayout.Label("Enabled Toolbar Buttons", EditorStyles.largeLabel);
            foreach (MethodInfo i in TypeCache.GetMethodsWithAttribute<SelectionGroupToolAttribute>()) 
            {
                SelectionGroupToolAttribute attr = i.GetCustomAttribute<SelectionGroupToolAttribute>();            
                repaintWindow |= EditorGUIDrawerUtility.DrawUndoableGUI(m_Group, "Group Toolbar",
                    guiFunc: () => 
                    {
                        bool       isEnabledPrev = m_Group.GetEditorToolState(attr.toolId);
                        GUIContent content       = EditorGUIUtility.IconContent(attr.icon);
                        return EditorGUILayout.ToggleLeft(content, isEnabledPrev, "button");
                    },
                    updateFunc: (bool toolEnabled) => 
                    {
                        m_Group.EnableEditorTool(attr.toolId, toolEnabled);
                    }
                );
            }
            GUILayout.EndVertical();
            
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
            serializedObject.ApplyModifiedProperties();

            if (!repaintWindow)
                return;
            
            SelectionGroupEditorWindow.TryRepaint();
        }
    }
}