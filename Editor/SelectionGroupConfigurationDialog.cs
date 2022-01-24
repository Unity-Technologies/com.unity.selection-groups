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
        [SerializeField] int groupId;
        ReorderableList exclusionList;

        ISelectionGroup group;
        GoQL.GoQLExecutor executor = new GoQL.GoQLExecutor();
        SelectionGroupEditorWindow parentWindow;
        string message = string.Empty;
        bool refreshQuery = true;
        bool showDebug = false;
        SelectionGroupDebugInformation debugInformation;

        private void OnEnable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            refreshQuery = true;
            Repaint();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        void OnGUI()
        {
            if (group == null)
            {
                Close();
                return;
            }

            //Null for SelectionGroup (scene), which is a MonoBehaviour, must be checked against null using its type
            if (group is Component component && component == null){
                Close();
                return;                
            }


            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                GUILayout.Label("Selection Group Properties", EditorStyles.largeLabel);
                group.Name = EditorGUILayout.TextField("Group Name", group.Name);
                group.Color = EditorGUILayout.ColorField("Color", group.Color);
                EditorGUILayout.LabelField("GameObject Query");
                var q = group.Query;
                var newQuery = EditorGUILayout.TextField(group.Query);
                refreshQuery = refreshQuery || (q != newQuery);
                
                if (refreshQuery)
                {
                    {
                        var obj = group as Object;
                        
                        Undo.RegisterCompleteObjectUndo(obj, "Query change");
                        
                    }
                }
                if (message != string.Empty)
                {
                    GUILayout.Space(5);
                    EditorGUILayout.HelpBox(message, MessageType.Info);
                }
                GUILayout.Space(5);
                
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
                foreach (var i in TypeCache.GetMethodsWithAttribute<SelectionGroupToolAttribute>())
                {
                    //[TODO-sin:2022-01-07] Remove in version 0.7.0                     
                    // var attr = i.GetCustomAttribute<SelectionGroupToolAttribute>();
                    // var enabled = group.EnabledTools.Contains(attr.toolId);
                    // var content = EditorGUIUtility.IconContent(attr.icon);
                    // var _enabled = EditorGUILayout.ToggleLeft(content, enabled, "button");
                    // if (enabled && !_enabled)
                    // {
                    //     group.EnabledTools.Remove(attr.toolId);
                    // }
                    // if (!enabled && _enabled)
                    // {
                    //     group.EnabledTools.Add(attr.toolId);
                    // }
                }
                GUILayout.EndVertical();
                
                showDebug = GUILayout.Toggle(showDebug, "Show Debug Info", "button");
                if (showDebug)
                {
                    if (debugInformation == null) debugInformation = new SelectionGroupDebugInformation(group);
                    EditorGUILayout.TextArea(debugInformation.text);
                }
                else
                {
                    debugInformation = null;
                }
            }
        }

        
    }
}

