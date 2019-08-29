using UnityEditor;
using UnityEngine;

namespace Unity.SelectionGroups
{
    [CustomPropertyDrawer(typeof(SelectionGroup))]
    public class SelectionGroupPropertyDrawer : PropertyDrawer
    {

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
            switch (Event.current.type)
            {
                case EventType.Repaint:
                case EventType.MouseDown:
                case EventType.MouseUp:
                    HandleRepaintEvent(position, property);
                    break;
                case EventType.DragUpdated:
                case EventType.DragExited:
                case EventType.DragPerform:
                    if (HandleDragEvents(position, property))
                        Event.current.Use();
                    break;
            }

        }

        private void HandleRepaintEvent(Rect position, SerializedProperty property)
        {
            var nameProperty = property.FindPropertyRelative("groupName");
            var colorProperty = property.FindPropertyRelative("color");
            var editProperty = property.FindPropertyRelative("edit");
            var isLightGroupProperty = property.FindPropertyRelative("isLightGroup");
            var queryProperty = property.FindPropertyRelative("selectionQuery").FindPropertyRelative("enabled");
            var showMembersProperty = property.FindPropertyRelative("showMembers");
            var ev = Event.current;
            var rect = position;
            var propertyId = property.propertyPath.GetHashCode();

            if (ev.isMouse && ev.button == 0 && ev.clickCount == 2 && position.Contains(ev.mousePosition))
            {
                SelectionGroupDialog.Open(property);
            }
            property.FindPropertyRelative("rect").rectValue = position;

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
            rect = position;
            rect.x = position.width;
            rect.width = 18;
            rect.height = EditorGUIUtility.singleLineHeight;
            rect.x += 5;
            rect.y += 1;

            if (EditorGUI.DropdownButton(rect, GUIContent.none, FocusType.Passive))
            {
                ShowMenu(property, rect);
            }
            if (showMembersProperty.boolValue)
            {
                rect = position;
                rect.x += 16;
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorSelectionGroupUtility.UpdateQueryResults(property);
                foreach (var i in EditorSelectionGroupUtility.FetchObjects(property))
                {
                    if (i == null) continue;
                    rect.y += rect.height;
                    DrawGameObjectWidget(rect, i);
                }
            }
        }

        private void ShowMenu(SerializedProperty property, Rect rect)
        {
            var menu = new GenericMenu();
            var showMembersProperty = property.FindPropertyRelative("showMembers");

            menu.AddItem(new GUIContent("Add Selection to Group"), false, () =>
            {
                EditorSelectionGroupUtility.AddObjects(property, Selection.objects);
                Selection.objects = EditorSelectionGroupUtility.FetchObjects(property);
            });
            menu.AddItem(new GUIContent("Remove Selection from Group"), false, () =>
            {
                EditorSelectionGroupUtility.RemoveObjects(property, Selection.objects);
                Selection.objects = EditorSelectionGroupUtility.FetchObjects(property);

            });
            menu.AddItem(new GUIContent("Set Selection as Group"), false, () =>
            {
                EditorSelectionGroupUtility.ClearObjects(property);
                EditorSelectionGroupUtility.AddObjects(property, Selection.objects);
                Selection.objects = EditorSelectionGroupUtility.FetchObjects(property);

            });
            menu.AddItem(new GUIContent("Clear Group"), false, () =>
            {
                EditorSelectionGroupUtility.ClearObjects(property);
                Selection.objects = EditorSelectionGroupUtility.FetchObjects(property);
            });
            menu.AddItem(new GUIContent("Settings"), false, () =>
            {
                SelectionGroupDialog.Open(property);
            });
            menu.DropDown(rect);
        }

        static void DrawGameObjectWidget(Rect rect, Object o)
        {
            GUIContent content;
            var g = o as GameObject;
            if (g != null && PrefabUtility.GetPrefabInstanceHandle(g) != null)
                content = EditorGUIUtility.IconContent("d_Prefab Icon", o.name);
            else
                content = EditorGUIUtility.IconContent("d_GameObject Icon", o.name);
            content.text = o.name;
            if (Selection.activeObject == o)
            {
                GUI.Box(rect, "");

            }
            if (GUI.Button(rect, content, "label"))
            {
                Selection.activeObject = o;
            }
            if (Event.current.isMouse && Event.current.button == 2)
            {
                ShowGameObjectContextMenu(rect);
            }
        }

        static void ShowGameObjectContextMenu(Rect rect)
        {
        }

        bool HandleDragEvents(Rect position, SerializedProperty property)
        {
            var e = Event.current;
            if (position.Contains(e.mousePosition))
            {
                switch (e.type)
                {
                    case EventType.DragUpdated:
                        UpdateDrag(position);
                        return true;
                    case EventType.DragExited:
                        ExitDrag();
                        return true;
                    case EventType.DragPerform:
                        PerformDrag(position, property);
                        return true;
                }
            }
            return false;
        }


        void UpdateDrag(Rect rect)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            DragAndDrop.AcceptDrag();
        }

        void ExitDrag()
        {
        }

        void PerformDrag(Rect position, SerializedProperty property)
        {
            EditorSelectionGroupUtility.AddObjects(property, DragAndDrop.objectReferences);
        }
    }
}
