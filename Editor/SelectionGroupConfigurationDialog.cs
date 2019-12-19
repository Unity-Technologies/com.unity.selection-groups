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

        public static void Open(SelectionGroup group, SelectionGroupEditorWindow parentWindow)
        {
            var dialog = EditorWindow.GetWindow<SelectionGroupConfigurationDialog>();
            dialog.group = group;
            dialog.parentWindow = parentWindow;
            dialog.ShowPopup();
        }

        void OnGUI()
        {
            GUILayout.Label("Selection Group Properties", EditorStyles.largeLabel);
            group.name = EditorGUILayout.TextField("Group Name", group.name);
            group.color = EditorGUILayout.ColorField("Color", group.color);
            EditorGUILayout.LabelField("GameObject Query");
            group.query = EditorGUILayout.TextField(group.query);
            var clock = new System.Diagnostics.Stopwatch();
            clock.Start();
            var code = GoQL.Parser.Parse(group.query, out GoQL.ParseResult parseResult);
            clock.Stop();
            EditorGUILayout.LabelField($"Parse Result: {parseResult} in {clock.ElapsedMilliseconds} milliseconds.");
            if (parseResult == GoQL.ParseResult.OK)
            {
                clock.Reset();
                clock.Start();
                var objects = executor.Execute(code);
                clock.Stop();
                EditorGUILayout.LabelField($"{objects.Length} results in {clock.ElapsedMilliseconds} milliseconds.");
                group.Clear();
                group.AddRange(objects);
                parentWindow.Repaint();
            }
        }
    }

}

