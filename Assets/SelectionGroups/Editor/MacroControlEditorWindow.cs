using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class MacroControlEditorWindow : EditorWindow
{
    Vector2 scroll;
    List<MacroEditor> macroEditors = new List<MacroEditor>();

    [MenuItem("Window/Macro Controls")]
    public static void OpenWindow()
    {
        var window = EditorWindow.GetWindow<MacroControlEditorWindow>();
        window.ShowUtility();
    }

    void OnEnable()
    {
        titleContent.text = "Macro Editors";
        macroEditors.Clear();
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(MacroEditor)))
                {
                    macroEditors.Add((MacroEditor)System.Activator.CreateInstance(type));
                }
            }
        }
        foreach (var macroEditor in macroEditors)
            macroEditor.OnEnable();
        foreach (var macroEditor in macroEditors)
            macroEditor.OnSelectionChange();
    }

    void OnSelectionChange()
    {
        foreach (var macroEditor in macroEditors)
            macroEditor.OnSelectionChange();
        Repaint();
    }

    void OnDisable()
    {
        foreach (var macroEditor in macroEditors)
            macroEditor.OnDisable();
    }

    void OnGUI()
    {
        using (var scrollScope = new EditorGUILayout.ScrollViewScope(scroll))
        {
            foreach (var macroEditor in macroEditors)
            {
                if (macroEditor.IsValidForSelection)
                {
                    GUILayout.Label(macroEditor.GetType().Name, EditorStyles.miniBoldLabel);
                    macroEditor.OnGUI();
                    GUILayout.Space(5);
                }
            }
            scroll = scrollScope.scrollPosition;
        }
    }

}
