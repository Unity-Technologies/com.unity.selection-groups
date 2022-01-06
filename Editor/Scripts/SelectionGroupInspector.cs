using System;
using System.Reflection;
using Unity.SelectionGroups;
using Unity.SelectionGroups.Runtime;
using UnityEditor;
using UnityEngine;
using Unity.FilmInternalUtilities.Editor;
    
namespace Unity.SelectionGroupsEditor {

[CustomEditor(typeof(Unity.SelectionGroups.Runtime.SelectionGroup))]
internal class SelectionGroupInspector : Editor {
    public override void OnInspectorGUI() {
        serializedObject.Update();

        DrawUndoableGUI(m_group, "Group Name",
            () => EditorGUILayout.TextField("Group Name", m_group.Name),
            (string groupName) => { m_group.Name = groupName; }
        );
        
        DrawUndoableGUI(m_group, "Group Color",
            () => EditorGUILayout.ColorField("Color", m_group.Color),
            (Color groupColor) => { m_group.Color = groupColor; }
        );
        serializedObject.ApplyModifiedProperties();        

        DrawUndoableGUI(m_group, "Group Query",
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

        if (m_group.IsAutoFilled()) {
            GoQL.ParseResult parseResult = m_group.GetLastQueryParseResult();
            
            message = (parseResult == GoQL.ParseResult.OK) 
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


        
        using (var cc = new EditorGUI.ChangeCheckScope())
        {                        
            //
            // GUILayout.BeginVertical("box");
            // GUILayout.Label("Enabled Toolbar Buttons", EditorStyles.largeLabel);
            // foreach (var i in TypeCache.GetMethodsWithAttribute<SelectionGroupToolAttribute>())
            // {
            //     var attr = i.GetCustomAttribute<SelectionGroupToolAttribute>();
            //     var enabled = m_group.EnabledTools.Contains(attr.toolId);
            //     var content = EditorGUIUtility.IconContent(attr.icon);
            //     var _enabled = EditorGUILayout.ToggleLeft(content, enabled, "button");
            //     if (enabled && !_enabled)
            //     {
            //         m_group.EnabledTools.Remove(attr.toolId);
            //     }
            //     if (!enabled && _enabled)
            //     {
            //         m_group.EnabledTools.Add(attr.toolId);
            //     }
            // }
            // GUILayout.EndVertical();
            
            // showDebug = GUILayout.Toggle(showDebug, "Show Debug Info", "button");
            // if (showDebug)
            // {
            //     if (debugInformation == null) debugInformation = new SelectionGroupDebugInformation(m_group);
            //     EditorGUILayout.TextArea(debugInformation.text);
            // }
            // else
            // {
            //     debugInformation = null;
            // }
        }
        
        
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    private void OnEnable() {
        m_group = target as SelectionGroup;
        Undo.undoRedoPerformed -= OnUndoRedo;
        Undo.undoRedoPerformed += OnUndoRedo;
    }

    private void OnUndoRedo() {
        refreshQuery = true;
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
    

    SelectionGroup                 m_group;
    
    GoQL.GoQLExecutor              executor = new GoQL.GoQLExecutor();
    string                         message      = string.Empty;
    bool                           refreshQuery = true;
    bool                           showDebug    = false;
    SelectionGroupDebugInformation debugInformation;
    
}

} //end namespace