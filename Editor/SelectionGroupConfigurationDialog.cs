using System.Linq;
using System.Reflection;
using Unity.SelectionGroups;
using Unity.SelectionGroups.Runtime;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.SelectionGroupsEditor
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

        internal static void Open(ISelectionGroup group, SelectionGroupEditorWindow parentWindow)
        {
            var dialog = EditorWindow.GetWindow<SelectionGroupConfigurationDialog>();
            dialog.group = group;
            dialog.parentWindow = parentWindow;
            dialog.refreshQuery = true;
            dialog.titleContent.text = $"Configure {group.Name}";
            dialog.ShowPopup();
            dialog.debugInformation = null;
        }

        void OnGUI()
        {
            if (group == null)
            {
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
                group.Query = EditorGUILayout.TextField(group.Query);
                refreshQuery = refreshQuery || (q != group.Query);
                if (refreshQuery)
                {
                    var code = GoQL.Parser.Parse(group.Query, out GoQL.ParseResult parseResult);
                    if (parseResult == GoQL.ParseResult.OK)
                    {
                        executor.Code = group.Query;
                        var objects = executor.Execute();
                        message = $"{objects.Length} results.";
                        @group.SetMembers(objects);
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
                var scope = @group.Scope;
                scope = (SelectionGroupDataLocation) EditorGUILayout.EnumPopup(@group.Scope);
                if (scope != @group.Scope)
                {
                    SelectionGroupManager.ChangeGroupScope(group, scope);
                    Close();
                }
                GUILayout.BeginVertical("box");
                GUILayout.Label("Enabled Toolbar Buttons", EditorStyles.largeLabel);
                foreach (var i in TypeCache.GetMethodsWithAttribute<SelectionGroupToolAttribute>())
                {
                    var attr = i.GetCustomAttribute<SelectionGroupToolAttribute>();
                    var enabled = group.EnabledTools.Contains(attr.toolId);
                    var content = EditorGUIUtility.IconContent(attr.icon);
                    var _enabled = EditorGUILayout.ToggleLeft(content, enabled, "button");
                    if (enabled && !_enabled)
                    {
                        group.EnabledTools.Remove(attr.toolId);
                    }
                    if (!enabled && _enabled)
                    {
                        group.EnabledTools.Add(attr.toolId);
                    }
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

