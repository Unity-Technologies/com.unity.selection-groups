using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SelectionGroup))]
public class SelectionGroupPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var rect = position;
        var nameProperty = property.FindPropertyRelative("groupName");
        var objects = property.FindPropertyRelative("objects");
        position.width = position.width - 24;
        EditorGUI.PropertyField(position, nameProperty, label);
        position.x += position.width;
        position.width = 18;
        position.height = 16;
        position.x += 5;
        position.y += 1;
        var ev = Event.current;

        if (EditorGUI.DropdownButton(position, GUIContent.none, FocusType.Passive))
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add Selection to Group"), false, () => AddObjects(property, objects, Selection.objects));
            menu.AddItem(new GUIContent("Remove Selection from Group"), false, () => RemoveObjects(property, objects, Selection.objects));
            menu.AddItem(new GUIContent("Set Selection as Group"), false, () =>
            {
                objects.ClearArray();
                AddObjects(property, objects, Selection.objects);
            });
            menu.AddItem(new GUIContent("Clear Group"), false, () =>
            {
                objects.ClearArray();
                property.serializedObject.ApplyModifiedProperties();
                SelectObjects(objects);
            });
            menu.DropDown(position);
        }


        //Disabled for now, it interferes with ReorderableList
        //HandleDragEvents(rect, property);

    }

    static void SelectObjects(SerializedProperty objects)
    {
        Selection.objects = UnpackProperty(objects);
    }

    static void RemoveObjects(SerializedProperty property, SerializedProperty objectsProperty, Object[] objects)
    {
        var uniqueObjects = new HashSet<Object>(UnpackProperty(objectsProperty));
        uniqueObjects.ExceptWith(objects);
        PackProperty(objectsProperty, uniqueObjects);
        property.serializedObject.ApplyModifiedProperties();
        SelectObjects(objectsProperty);
    }

    static void AddObjects(SerializedProperty property, SerializedProperty objectsProperty, Object[] objects)
    {
        var uniqueObjects = new HashSet<Object>(UnpackProperty(objectsProperty));
        uniqueObjects.UnionWith(objects);
        PackProperty(objectsProperty, uniqueObjects);
        property.serializedObject.ApplyModifiedProperties();
        SelectObjects(objectsProperty);
    }

    internal static void PackProperty(SerializedProperty objects, IEnumerable<Object> uniqueObjects)
    {
        objects.ClearArray();
        foreach (var g in uniqueObjects)
        {
            objects.InsertArrayElementAtIndex(0);
            objects.GetArrayElementAtIndex(0).objectReferenceValue = g;
        }
    }

    static Object[] UnpackProperty(SerializedProperty objects)
    {
        var count = objects.arraySize;
        var list = new Object[count];
        for (int i = 0; i < count; i++)
        {
            list[i] = objects.GetArrayElementAtIndex(i).objectReferenceValue;
        }
        return list;
    }

    void HandleDragEvents(Rect rect, SerializedProperty property)
    {
        var e = Event.current;
        switch (e.type)
        {
            case EventType.MouseDown:
                StartDrag();
                e.Use();
                break;
            case EventType.DragUpdated:
                UpdateDrag(rect);
                e.Use();
                break;
            case EventType.DragExited:
                ExitDrag();
                e.Use();
                break;
            case EventType.DragPerform:
                PerformDrag(property);
                e.Use();
                break;
        }
    }

    void StartDrag()
    {
        DragAndDrop.PrepareStartDrag();
        DragAndDrop.paths = new string[] { "woot." };
        DragAndDrop.objectReferences = Selection.objects;
        DragAndDrop.visualMode = DragAndDropVisualMode.Move;
        DragAndDrop.StartDrag("Groups");
    }

    void UpdateDrag(Rect rect)
    {
        var pos = Event.current.mousePosition;
        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
        if (rect.Contains(pos))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            DragAndDrop.AcceptDrag();
            return;
        }
    }

    void ExitDrag()
    {
    }

    void PerformDrag(SerializedProperty property)
    {
        AddObjects(property, property.FindPropertyRelative("objects"), DragAndDrop.objectReferences);
    }
}
