using System;
using System.Reflection;
using Unity.FilmInternalUtilities.Editor;
using Unity.SelectionGroups;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
    
namespace Unity.SelectionGroups.Editor {

[CustomEditor(typeof(SelectionGroups.SelectionGroup))]
internal class SelectionGroupInspector : UnityEditor.Editor {
    public override void OnInspectorGUI() {
        serializedObject.Update();

        bool repaintWindow = EditorGUIDrawerUtility.DrawUndoableGUI(m_group.gameObject, "Group Name",
            () => EditorGUILayout.TextField("Group Name", m_group.groupName),
            (string groupName) => { m_group.groupName = groupName; }
        );
        
        repaintWindow |= EditorGUIDrawerUtility.DrawUndoableGUI(m_group, "Group Color",
            () => EditorGUILayout.ColorField("Color", m_group.color),
            (Color groupColor) => { m_group.color = groupColor; }
        );

        repaintWindow |= EditorGUIDrawerUtility.DrawUndoableGUI(m_group, "Group Query",
            () => EditorGUILayout.TextField("Group Query", m_group.query),
            (string query) => {
                {
                    Undo.RegisterCompleteObjectUndo(m_group, "Query change");
                }
                m_group.SetQuery(query);
            }
        );

        //Query results
        if (m_group.IsAutoFilled()) {
            GoQL.ParseResult parseResult = m_group.GetLastQueryParseResult();
            
            string message = (parseResult == GoQL.ParseResult.OK) 
                ? $"{m_group.Members.Count} results." 
                : parseResult.ToString();

            GUILayout.Space(5);
            EditorGUILayout.HelpBox(message, MessageType.Info);
        }
                
        GUILayout.BeginVertical("box");
        GUILayout.Label("Enabled Toolbar Buttons", EditorStyles.largeLabel);
        foreach (MethodInfo i in TypeCache.GetMethodsWithAttribute<SelectionGroupToolAttribute>()) {
            SelectionGroupToolAttribute attr = i.GetCustomAttribute<SelectionGroupToolAttribute>();            
            repaintWindow |= EditorGUIDrawerUtility.DrawUndoableGUI(m_group, "Group Toolbar",
                guiFunc: () => {
                    bool       isEnabledPrev = m_group.GetEditorToolState(attr.toolId);
                    GUIContent content       = EditorGUIUtility.IconContent(attr.icon);
                    return EditorGUILayout.ToggleLeft(content, isEnabledPrev, "button");
                },
                updateFunc: (bool toolEnabled) => {
                    m_group.EnableEditorTool(attr.toolId, toolEnabled);
                }
            );
        }
        GUILayout.EndVertical();
        
        m_showDebug = GUILayout.Toggle(m_showDebug, "Show Debug Info", "button");
        if (m_showDebug) {
            if (m_debugInformation == null) m_debugInformation = new SelectionGroupDebugInformation(m_group);
            EditorGUILayout.TextArea(m_debugInformation.text);
        } else {
            m_debugInformation = null;
        }
        serializedObject.ApplyModifiedProperties();

        if (!repaintWindow)
            return;
        
        SelectionGroupEditorWindow.TryRepaint();

    }
    
//----------------------------------------------------------------------------------------------------------------------    
    private void OnEnable() {
        m_group = target as SelectionGroup;
    }
   
//----------------------------------------------------------------------------------------------------------------------    
    

    SelectionGroup m_group;
    
    bool                           m_showDebug  = false;
    SelectionGroupDebugInformation m_debugInformation;
}

} //end namespace