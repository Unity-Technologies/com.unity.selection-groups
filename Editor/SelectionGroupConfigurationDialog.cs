using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.SelectionGroups
{
    public class SelectionGroupConfigurationDialog : EditorWindow
    {
        public int groupId;
        ReorderableList materialList;
        ReorderableList typeList;
        ReorderableList shaderList;
        ReorderableList attachmentList;

        GoQL.GoQLExecutor executor = new GoQL.GoQLExecutor();
        SelectionGroupEditorWindow parentWindow;
        string message = string.Empty;
        bool refreshQuery = true;
        bool showDebug = false;
        SelectionGroupDebugInformation debugInformation;

        public static void Open(SelectionGroup group, SelectionGroupEditorWindow parentWindow)
        {
            var dialog = EditorWindow.GetWindow<SelectionGroupConfigurationDialog>();
            // var dialog = ScriptableObject.CreateInstance(typeof(SelectionGroupConfigurationDialog)) as SelectionGroupConfigurationDialog;
            dialog.groupId = group.groupId;
            dialog.parentWindow = parentWindow;
            dialog.refreshQuery = true;
            // dialog.ShowModalUtility();
            dialog.titleContent.text = $"Configure {group.name}";
            dialog.ShowPopup();
            dialog.debugInformation = null;
        }

        void OnGUI()
        {
            var group = SelectionGroupManager.instance.GetGroup(groupId);
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
                GUILayout.Space(5);

                GUILayout.BeginVertical("box");
                GUILayout.Label("Enabled Toolbar Buttons", EditorStyles.largeLabel);
                foreach (var i in TypeCache.GetMethodsWithAttribute<SelectionGroupToolAttribute>())
                {
                    var attr = i.GetCustomAttribute<SelectionGroupToolAttribute>();
                    var enabled = group.enabledTools.Contains(attr.ToolID);
                    var content = EditorGUIUtility.IconContent(attr.icon);
                    var _enabled = EditorGUILayout.ToggleLeft(content, enabled, "button");
                    if (enabled && !_enabled)
                    {
                        group.enabledTools.Remove(attr.ToolID);
                    }
                    if (!enabled && _enabled)
                    {
                        group.enabledTools.Add(attr.ToolID);
                    }
                }
                GUILayout.EndVertical();
                if (cc.changed)
                {
                    SelectionGroupManager.instance.SetIsDirty();
                }
                showDebug = GUILayout.Toggle(showDebug, "Show Debug Info", "button");
                if (showDebug)
                {
                    if (debugInformation == null) debugInformation = new SelectionGroupDebugInformation(group);
                    foreach (var kv in debugInformation.idObjectMap)
                    {
                        var gid = kv.Key;
                        var obj = kv.Value;

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(gid.ToString());
                        EditorGUILayout.ObjectField(obj, typeof(Object), true);
                        EditorGUILayout.LabelField(obj == null ? "NULL" : obj.GetInstanceID().ToString());
                        GUILayout.EndHorizontal();
                    }
                }
                else
                {
                    debugInformation = null;
                }
            }
        }
    }

    class SelectionGroupDebugInformation
    {

        public Dictionary<GlobalObjectId, Object> idObjectMap = new Dictionary<GlobalObjectId, Object>();

        public SelectionGroupDebugInformation(SelectionGroup group)
        {
            
        }
    }

}

