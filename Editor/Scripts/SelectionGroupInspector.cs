using System;
using System.Reflection;
using Unity.SelectionGroups;
using Unity.SelectionGroups.Runtime;
using UnityEditor;
using UnityEngine;
    
namespace Unity.SelectionGroupsEditor {

[CustomEditor(typeof(Unity.SelectionGroups.Runtime.SelectionGroup))]
internal class SelectionGroupInspector : Editor {
    public override void OnInspectorGUI() {
        serializedObject.Update();

        bool changed = DrawUndoableGUI(m_group, "Group Name",
            () => EditorGUILayout.TextField("Group Name", m_group.Name),
            (string groupName) => { m_group.Name = groupName; }
        );
        
        changed = changed || DrawUndoableGUI(m_group, "Group Color",
            () => EditorGUILayout.ColorField("Color", m_group.Color),
            (Color groupColor) => { m_group.Color = groupColor; }
        );

        changed = changed || DrawUndoableGUI(m_group, "Group Query",
            () => EditorGUILayout.TextField("Group Query", m_group.Query),
            (string query) => {
                {
                    //[TODO-sin:2021-12-20] Remove in version 0.7.0 
                    //var obj = m_group as Object;
                    // if(obj == null)
                    //     Undo.RegisterCompleteObjectUndo(SelectionGroupPersistenceManager.Instance, "Query change");
                    // else
                    //    Undo.RegisterCompleteObjectUndo(obj, "Query change");
                
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
        
        //[TODO-sin:2021-12-20] Remove in version 0.7.0 
        // SelectionGroupDataLocation scope = @group.Scope;
        // scope = (SelectionGroupDataLocation) EditorGUILayout.EnumPopup(@group.Scope);
        // if (scope != @group.Scope)
        // {
        //     SelectionGroupManager.ChangeGroupScope(group, scope);
        //     Close();
        // }

        
        GUILayout.BeginVertical("box");
        GUILayout.Label("Enabled Toolbar Buttons", EditorStyles.largeLabel);
        foreach (MethodInfo i in TypeCache.GetMethodsWithAttribute<SelectionGroupToolAttribute>()) {
            SelectionGroupToolAttribute attr = i.GetCustomAttribute<SelectionGroupToolAttribute>();
            bool isEnabledPrev = m_group.EnabledTools.Contains(attr.toolId);
            GUIContent content = EditorGUIUtility.IconContent(attr.icon);
            var isEnabledNow = EditorGUILayout.ToggleLeft(content, isEnabledPrev, "button");
            if (isEnabledPrev && !isEnabledNow) {
                m_group.EnabledTools.Remove(attr.toolId);
                changed = true;
            }
            if (!isEnabledPrev && isEnabledNow) {
                m_group.EnabledTools.Add(attr.toolId);
                changed = true;
            }
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

        if (!changed)
            return;

        // Repaint SelectionGroupEditorWindow
        if (EditorWindow.HasOpenInstances<SelectionGroupEditorWindow>()) {
            SelectionGroupEditorWindow window = EditorWindow.GetWindow<SelectionGroupEditorWindow>(
                utility:false, title:"", focus:false);
            window.Repaint();
        }        
        
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    private void OnEnable() {
        m_group = target as SelectionGroup;
        Undo.undoRedoPerformed -= OnUndoRedo;
        Undo.undoRedoPerformed += OnUndoRedo;
    }

    private void OnUndoRedo() {
        Repaint();
    }

    private void OnDisable() {
        Undo.undoRedoPerformed -= OnUndoRedo;
    }

//----------------------------------------------------------------------------------------------------------------------
    //[TODO: 2022-1-6]: Use FilmInternalUtilities
    private static bool DrawUndoableGUI<V>(UnityEngine.Object target, string undoText,  
        Func<V> guiFunc, 
        Action<V> updateFunc)   
    {
        EditorGUI.BeginChangeCheck();
        V newValue = guiFunc();
        if (!EditorGUI.EndChangeCheck()) 
            return false;
        
        Undo.RecordObject(target, undoText);
        updateFunc(newValue);
        return true;
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    

    SelectionGroup m_group;
    
    bool                           m_showDebug  = false;
    SelectionGroupDebugInformation m_debugInformation;
}

} //end namespace