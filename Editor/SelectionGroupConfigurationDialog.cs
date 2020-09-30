using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.SelectionGroups
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

        internal static void Open(SelectionGroup group, SelectionGroupEditorWindow parentWindow)
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
            if (SelectionGroupManager.instance == null) return;
            group = SelectionGroupManager.instance.GetGroup(groupId);
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
                // if (exclusionList == null)
                // {
                //     exclusionList = new ReorderableList(group.exclude, typeof(SelectionGroup), false, false, true, true);
                //     exclusionList.drawElementCallback = DrawSelectionGroupElement;
                // }
                // exclusionList.DoLayoutList();
                GUILayout.BeginVertical("box");
                GUILayout.Label("Enabled Toolbar Buttons", EditorStyles.largeLabel);
                foreach (var i in TypeCache.GetMethodsWithAttribute<SelectionGroupToolAttribute>())
                {
                    var attr = i.GetCustomAttribute<SelectionGroupToolAttribute>();
                    var enabled = group.enabledTools.Contains(attr.toolId);
                    var content = EditorGUIUtility.IconContent(attr.icon);
                    var _enabled = EditorGUILayout.ToggleLeft(content, enabled, "button");
                    if (enabled && !_enabled)
                    {
                        group.enabledTools.Remove(attr.toolId);
                    }
                    if (!enabled && _enabled)
                    {
                        group.enabledTools.Add(attr.toolId);
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
                    EditorGUILayout.TextArea(debugInformation.text);
                }
                else
                {
                    debugInformation = null;
                }
            }
        }

        private void DrawSelectionGroupElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index >= 0 && index < group.exclude.Count)
            {
                group.exclude[index] = SelectionGroupManager.Popup(rect, group.exclude[index]);
            }
        }
    }

    class SelectionGroupDebugInformation
    {

        public string text;

        public SelectionGroupDebugInformation(SelectionGroup group)
        {
            text = EditorJsonUtility.ToJson(group, prettyPrint: true);
        }
    }

}

