using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class SelectionGroupEditorWindow : EditorWindow
{

    ReorderableList list;
    SerializedObject serializedObject;
    SelectionGroups selectionGroups;
    Vector2 scroll;
    void BuildListWidget()
    {
        selectionGroups = GameObject.FindObjectOfType<SelectionGroups>();
        if (selectionGroups == null)
        {
            var g = new GameObject("Hidden_SelectionGroups");
            g.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            selectionGroups = g.AddComponent<SelectionGroups>();
            EditorUtility.SetDirty(selectionGroups);
        }
        serializedObject = new SerializedObject(selectionGroups);
        var groups = serializedObject.FindProperty("groups");
        list = new ReorderableList(serializedObject, groups);
        list.drawElementCallback = OnDrawElement;
        list.drawHeaderCallback = DoNothing;
        list.onAddCallback = OnAdd;
        list.headerHeight = 0;
        titleContent.text = "Selection Groups";
        list.onSelectCallback += OnSelect;
    }

    void OnSelect(ReorderableList list)
    {
        var objects = new List<Object>();
        selectionGroups.FetchObjects(list.index, objects);
        Selection.objects = objects.ToArray();
        list.serializedProperty.GetArrayElementAtIndex(list.index).FindPropertyRelative("edit").boolValue = false;
    }

    void OnAdd(ReorderableList list)
    {
        list.serializedProperty.InsertArrayElementAtIndex(list.serializedProperty.arraySize);
        var item = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
        item.FindPropertyRelative("groupName").stringValue = "New Group";
        var objectsProperty = item.FindPropertyRelative("objects");
        objectsProperty.ClearArray();
        SelectionGroupPropertyDrawer.PackProperty(objectsProperty, Selection.objects);
        serializedObject.ApplyModifiedProperties();
    }

    void DoNothing(Rect rect)
    {
    }

    void OnSelectionChange()
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
        var window = EditorWindow.GetWindow<SelectionGroupEditorWindow>();
        window.ShowUtility();
    }

    void OnGUI()
    {
        if (selectionGroups == null || list == null) BuildListWidget();
        scroll = EditorGUILayout.BeginScrollView(scroll);
        using (var cc = new EditorGUI.ChangeCheckScope())
        {
            list.DoLayoutList();
            if (cc.changed)
            {
                EditorUtility.SetDirty(selectionGroups);
                serializedObject.ApplyModifiedProperties();

            }
        }
        EditorGUILayout.EndScrollView();
        if (focusedWindow == this)
            Repaint();
        // Repaint();
    }


}
