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

        SelectionGroup group;
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
                group.groupName = EditorGUILayout.TextField("Group Name", group.groupName);
                group.color = EditorGUILayout.ColorField("Color", group.color);
                EditorGUILayout.LabelField("GameObject Query");
                var q = group.query;
                var newQuery = EditorGUILayout.TextField(group.query);
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

