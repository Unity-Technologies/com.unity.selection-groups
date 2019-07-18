using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SelectionGroup))]
public class SelectionGroupPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var width = position.width / 2;
        var nameProperty = property.FindPropertyRelative("groupName");
        var objects = property.FindPropertyRelative("objects");
        position.width = width;
        EditorGUI.PropertyField(position, nameProperty, label);
        position.x += width;
        var ev = Event.current;
        if (ev.shift)
        {
            if (GUI.Button(position, "Add Selected"))
            {
                var uniqueObjects = new HashSet<Object>(UnpackProperty(objects));
                uniqueObjects.UnionWith(Selection.objects);
                PackProperty(objects, uniqueObjects);
            }
        }
        else if (ev.alt)
        {
            if (GUI.Button(position, "Remove Selected"))
            {
                var uniqueObjects = new HashSet<Object>(UnpackProperty(objects));
                uniqueObjects.ExceptWith(Selection.objects);
                PackProperty(objects, uniqueObjects);
            }
        }
        else
        {
            if (GUI.Button(position, "Select"))
            {
                Selection.objects = UnpackProperty(objects);
            }
        }
    }

    static void PackProperty(SerializedProperty objects, IEnumerable<Object> uniqueObjects)
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
}
