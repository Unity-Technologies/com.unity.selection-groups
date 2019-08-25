using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Utj.Film
{
    public class SelectionGroupEditorWindow : EditorWindow
    {

        ReorderableList list;
        SerializedObject serializedObject;
        SelectionGroups selectionGroups;
        Vector2 scroll;
        SerializedProperty activeSelectionGroup;

        void BuildListWidget()
        {
            selectionGroups = SelectionGroups.Instance;
            EditorUtility.SetDirty(selectionGroups);
            serializedObject = new SerializedObject(selectionGroups);
            var groups = serializedObject.FindProperty("groups");
            list = new ReorderableList(serializedObject, groups);
            list.drawElementCallback = OnDrawElement;
            list.drawHeaderCallback = DoNothing;
            list.elementHeightCallback += ElementHeightCallback;
            list.onAddCallback = OnAdd;
            list.onRemoveCallback += OnRemove;
            list.headerHeight = 0;
            titleContent.text = "Selection Groups";
            list.onSelectCallback += OnSelect;
        }

        float ElementHeightCallback(int index)
        {
            var p = list.serializedProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(p);
        }

        void OnRemove(ReorderableList list)
        {
            activeSelectionGroup = null;
            list.serializedProperty.DeleteArrayElementAtIndex(list.index);
        }

        void OnSelect(ReorderableList list)
        {
            activeSelectionGroup = list.serializedProperty.GetArrayElementAtIndex(list.index);
            activeSelectionGroup.FindPropertyRelative("edit").boolValue = false;
            SelectionGroupUtility.UpdateQueryResults(activeSelectionGroup);
            Selection.objects = SelectionGroupUtility.FetchObjects(activeSelectionGroup);
        }

        void OnEnable()
        {
        }

        void OnAdd(ReorderableList list)
        {
            list.serializedProperty.InsertArrayElementAtIndex(list.serializedProperty.arraySize);
            var item = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            item.FindPropertyRelative("groupName").stringValue = "New Group";
            item.FindPropertyRelative("color").colorValue = Color.HSVToRGB(Random.value, 1, 1);
            var objectsProperty = item.FindPropertyRelative("objects");
            objectsProperty.ClearArray();
            SelectionGroupUtility.PackArrayProperty(objectsProperty, Selection.objects);
            SelectionGroupUtility.UpdateUsageFlags(item);
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
            EditorGUI.PropertyField(rect, item, GUIContent.none);
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

                EditorGUILayout.EndScrollView();

                if (activeSelectionGroup != null)
                {
                    GUILayout.BeginVertical("box");
                    EditorGUILayout.PropertyField(activeSelectionGroup.FindPropertyRelative("attachments"), true);
                    GUILayout.EndVertical();
                }
                if (cc.changed)
                {
                    EditorUtility.SetDirty(selectionGroups);
                    serializedObject.ApplyModifiedProperties();

                }
            }
            if (focusedWindow == this)
                Repaint();
        }


    }
}
