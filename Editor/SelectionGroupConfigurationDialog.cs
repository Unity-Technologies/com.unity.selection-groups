using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.SelectionGroups
{
    public class SelectionGroupConfigurationDialog : EditorWindow
    {
        SelectionGroup group;
        ReorderableList materialList;
        ReorderableList typeList;
        ReorderableList shaderList;
        ReorderableList attachmentList;

        GoQL.GoQLExecutor executor = new GoQL.GoQLExecutor();
        SelectionGroupEditorWindow parentWindow;
        string message = string.Empty;
        bool refreshQuery = true;

        public static void Open(SelectionGroup group, SelectionGroupEditorWindow parentWindow)
        {
            var dialog = EditorWindow.GetWindow<SelectionGroupConfigurationDialog>();
            // var dialog = ScriptableObject.CreateInstance(typeof(SelectionGroupConfigurationDialog)) as SelectionGroupConfigurationDialog;
            dialog.group = group;
            dialog.parentWindow = parentWindow;
            dialog.refreshQuery = true;
            // dialog.ShowModalUtility();
            dialog.titleContent.text = $"Configure {group.name}";
            dialog.ShowPopup();
        }

        void OnGUI()
        {
            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                GUILayout.Label("Selection Group Properties", EditorStyles.largeLabel);
                group.name = EditorGUILayout.TextField("Group Name", group.name);
                group.color = EditorGUILayout.ColorField("Color", group.color);
                EditorGUILayout.LabelField("GameObject Query");
                var q = group.query;
                group.query = EditorGUILayout.TextField(group.query);
                refreshQuery = refreshQuery || (q != group.query);
                if (refreshQuery)
                {
                    var code = GoQL.Parser.Parse(group.query, out GoQL.ParseResult parseResult);
                    if (parseResult == GoQL.ParseResult.OK)
                    {
                        executor.Code = group.query;
                        var objects = executor.Execute();
                        message = $"{objects.Length} results.";
                        group.Clear();
                        group.Add(objects);
                        parentWindow.Repaint();
                    }
                    else
                    {
                        message = parseResult.ToString();
                    }
                    refreshQuery = false;
                }
                if (message != string.Empty)
                {
                    GUILayout.Space(5);
                    EditorGUILayout.HelpBox(message, MessageType.Info);
                }
                if (cc.changed)
                {
                    SelectionGroupManager.instance.SetIsDirty();
                }
            }
        }
    }

}

