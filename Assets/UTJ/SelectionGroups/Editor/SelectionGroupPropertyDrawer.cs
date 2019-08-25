using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Utj.Film
{
    [CustomPropertyDrawer(typeof(SelectionGroup))]
    public class SelectionGroupPropertyDrawer : PropertyDrawer
    {
        bool focus;
        static int activePropertyId = 0;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var showMembersProperty = property.FindPropertyRelative("showMembers");
            var height = base.GetPropertyHeight(property, label);
            if (showMembersProperty.boolValue)
            {
                var count = property.FindPropertyRelative("objects").arraySize;
                count += property.FindPropertyRelative("queryResults").arraySize;
                return height * 1.2f + count * EditorGUIUtility.singleLineHeight;
            }
            return height * 1.2f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var nameProperty = property.FindPropertyRelative("groupName");
            var colorProperty = property.FindPropertyRelative("color");
            var objects = property.FindPropertyRelative("objects");
            var editProperty = property.FindPropertyRelative("edit");
            var isLightGroupProperty = property.FindPropertyRelative("isLightGroup");
            var queryProperty = property.FindPropertyRelative("selectionQuery").FindPropertyRelative("enabled");
            var showMembersProperty = property.FindPropertyRelative("showMembers");
            var ev = Event.current;

            var propertyId = property.propertyPath.GetHashCode();
            if (ev.isMouse && ev.button == 0 && ev.clickCount == 2 && position.Contains(ev.mousePosition))
                activePropertyId = propertyId;
            var rect = position;
            if (activePropertyId == propertyId)
            {
                if (focus)
                {
                    GUI.FocusControl(property.propertyPath);
                    GUI.SetNextControlName(property.propertyPath);
                }
                rect = position;
                rect.width -= 72;
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rect, nameProperty, label);
                rect.x += rect.width;
                rect.width = 48;
                EditorGUI.PropertyField(rect, colorProperty, label);
            }
            else
            {
                rect = position;
                rect.width -= 32;
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(rect, nameProperty.stringValue);
                rect.x += rect.width;
                rect.width = 16;
                rect.y += 1;
                if (isLightGroupProperty.boolValue)
                {
                    rect.x -= 16;
                    EditorGUI.LabelField(rect, EditorGUIUtility.IconContent("Light Icon", "This group contains Light components."));
                    rect.x += 16;
                }
                if (queryProperty.boolValue)
                {
                    rect.x -= 32;
                    EditorGUI.LabelField(rect, EditorGUIUtility.IconContent("d_FilterByType", "This group contains a query."));
                    rect.x += 32;
                }
                EditorGUI.DrawRect(rect, colorProperty.colorValue);
            }
            rect = position;
            rect.x = position.width;
            rect.width = 18;
            rect.height = EditorGUIUtility.singleLineHeight;
            rect.x += 5;
            rect.y += 1;

            if (EditorGUI.DropdownButton(rect, GUIContent.none, FocusType.Passive))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Add Selection to Group"), false, () =>
                {
                    SelectionGroupUtility.AddObjects(property, Selection.objects);
                    Selection.objects = SelectionGroupUtility.FetchObjects(property);
                });
                menu.AddItem(new GUIContent("Remove Selection from Group"), false, () =>
                {
                    SelectionGroupUtility.RemoveObjects(property, Selection.objects);
                    Selection.objects = SelectionGroupUtility.FetchObjects(property);

                });
                menu.AddItem(new GUIContent("Set Selection as Group"), false, () =>
                {
                    objects.ClearArray();
                    SelectionGroupUtility.PackArrayProperty(objects, Selection.objects);
                    Selection.objects = SelectionGroupUtility.FetchObjects(property);

                });
                menu.AddItem(new GUIContent("Clear Group"), false, () =>
                {
                    SelectionGroupUtility.ClearObjects(property);
                    Selection.objects = SelectionGroupUtility.FetchObjects(property);
                });
                menu.AddItem(new GUIContent("Edit"), false, () =>
                {
                    activePropertyId = propertyId;
                    focus = true;
                });
                menu.AddItem(new GUIContent("Configure Dynamic Selection"), false, () => DynamicSelectionDialog.Open(property));
                menu.AddItem(new GUIContent("Show Members"), showMembersProperty.boolValue, () =>
                {
                    showMembersProperty.boolValue = !showMembersProperty.boolValue;
                    property.serializedObject.ApplyModifiedProperties();
                });
                menu.DropDown(position);
            }
            if (showMembersProperty.boolValue)
            {
                rect = position;
                rect.x += 16;
                rect.height = EditorGUIUtility.singleLineHeight;
                SelectionGroupUtility.UpdateQueryResults(property);
                foreach (var i in SelectionGroupUtility.FetchObjects(property))
                {
                    rect.y += rect.height;
                    var content = EditorGUIUtility.IconContent("d_GameObject Icon", i.name);
                    content.text = i.name;
                    if (Selection.activeObject == i)
                    {
                        GUI.Box(rect, "");
                    }
                    if (GUI.Button(rect, content, "label"))
                    {
                        Selection.activeObject = i;
                    }
                }
            }
            if (ev.isKey)
            {
                switch (ev.keyCode)
                {
                    case KeyCode.Escape:
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                    case KeyCode.Tab:
                        if (activePropertyId == propertyId) activePropertyId = 0;
                        break;
                }
            }

            //Disabled for now, it interferes with ReorderableList
            // HandleDragEvents(position, property);

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
            SelectionGroupUtility.AddObjects(property, DragAndDrop.objectReferences);
        }
    }
}
