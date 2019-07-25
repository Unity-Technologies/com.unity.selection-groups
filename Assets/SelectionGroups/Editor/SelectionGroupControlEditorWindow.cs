using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class SelectionGroupControlEditorWindow : EditorWindow
{

    SerializedObject serializedObject;
    SelectionGroups selectionGroups;
    bool enabled = false;

    [MenuItem("Window/Selection Group Controls")]
    static void OpenWindow()
    {
        var window = EditorWindow.GetWindow<SelectionGroupControlEditorWindow>();
        window.ShowUtility();
    }

    void OnEnable()
    {
        titleContent.text = "Selection Group Controls";
        OnSelectionChange();
    }

    void OnSelectionChange()
    {
        enabled = Selection.objects.Length > 0;
        if (enabled)
            serializedObject = new SerializedObject(Selection.objects);
        else
            serializedObject = null;
    }

    void OnGUI()
    {
        if (!enabled || serializedObject == null) return;
        DrawProperties();
        GUILayout.FlexibleSpace();
        // Repaint();
    }

    private void DrawProperties()
    {
        var property = serializedObject.GetIterator();
        while (true)
        {
            GUILayout.Label(property.name);
            if (!property.NextVisible(true)) break;
        }
    }
}
