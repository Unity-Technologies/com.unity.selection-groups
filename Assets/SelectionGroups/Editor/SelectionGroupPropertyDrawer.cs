using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SelectionGroup))]
public class SelectionGroupPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var rect = position;
        var width = position.width / 2;
        var nameProperty = property.FindPropertyRelative("groupName");
        var objects = property.FindPropertyRelative("objects");
        position.width = position.width - 24;
        EditorGUI.PropertyField(position, nameProperty, label);
        position.x += position.width;
        position.width = 20;
        position.height = 16;
        position.x += 4;
        var ev = Event.current;
        if (ev.shift)
        {
            if (GUI.Button(position, "+"))
            {
                AddObjects(property, objects, Selection.objects);
            }
        }
        else if (ev.alt)
        {
            if (GUI.Button(position, "-"))
            {
                RemoveObjects(property, objects, Selection.objects);
            }
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
