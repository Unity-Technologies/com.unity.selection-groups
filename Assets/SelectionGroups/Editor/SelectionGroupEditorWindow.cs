using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class SelectionGroupEditorWindow : EditorWindow
{

    ReorderableList list;
    SerializedObject serializedObject;
    SelectionGroups selectionGroups;

    struct DropZone
    {
        public Rect rect;
        public SerializedProperty property;
    }

    List<DropZone> dropZones = new List<DropZone>();

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

    void OnSelectionChanged()
    {
        Debug.Log("osc");
    }

    void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        var item = list.serializedProperty.GetArrayElementAtIndex(index);
        dropZones.Add(new DropZone() { rect = rect, property = item });
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
        dropZones.Clear();
        if (selectionGroups == null || list == null) BuildListWidget();
        using (var cc = new EditorGUI.ChangeCheckScope())
        {
            list.DoLayoutList();
            if (cc.changed)
                EditorUtility.SetDirty(selectionGroups);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.HelpBox("Hold shift to add, alt to remove.", MessageType.Info);

        if (focusedWindow == this)
            Repaint();
        // Repaint();
    }


}
