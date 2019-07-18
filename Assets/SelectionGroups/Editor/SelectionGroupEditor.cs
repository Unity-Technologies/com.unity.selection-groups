using System;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class SelectionGroupEditor : EditorWindow
{

    ReorderableList list;
    SerializedObject serializedObject;

    void OnEnable()
    {
        var item = GameObject.FindObjectOfType<SelectionGroups>();
        if (item == null)
        {
            var g = new GameObject("Hidden_SelectionGroups");
            g.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            item = g.AddComponent<SelectionGroups>();
            EditorUtility.SetDirty(item);
        }
        serializedObject = new SerializedObject(item);
        var groups = serializedObject.FindProperty("groups");
        list = new ReorderableList(serializedObject, groups);
        list.drawElementCallback = OnDrawElement;
        list.drawHeaderCallback = DoNothing;
        list.onAddCallback = OnAdd;
        list.headerHeight = 0;
        titleContent.text = "Selection Groups";
    }

    void OnAdd(ReorderableList list)
    {
        list.serializedProperty.InsertArrayElementAtIndex(list.serializedProperty.arraySize);
        var item = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
        item.FindPropertyRelative("groupName").stringValue = "New Group";
        item.FindPropertyRelative("objects").ClearArray();
        serializedObject.ApplyModifiedProperties();
    }

    void DoNothing(Rect rect)
    {
    }

    void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        var item = list.serializedProperty.GetArrayElementAtIndex(index);
        if (isActive)
        {
            EditorGUI.PropertyField(rect, item, GUIContent.none);
        }
        else
        {
            GUI.Label(rect, item.FindPropertyRelative("groupName").stringValue, EditorStyles.label);
        }
    }

    [MenuItem("Window/Selection Groups")]
    static void OpenWindow()
    {
        var window = EditorWindow.GetWindow<SelectionGroupEditor>();
        window.ShowUtility();
    }

    void OnGUI()
    {
        list.DoLayoutList();
        GUILayout.FlexibleSpace();
        EditorGUILayout.HelpBox("Hold shift to add, alt to remove.", MessageType.Info);
        Repaint();
    }

}
