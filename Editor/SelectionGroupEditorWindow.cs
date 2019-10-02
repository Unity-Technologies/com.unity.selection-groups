using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Unity.SelectionGroups
{

    public class SelectionGroupEditorWindow : EditorWindow
    {
        private const int RIGHT_MOUSE_BUTTON = 1;

        ReorderableList list;
        SerializedObject serializedObject;
        SelectionGroupContainer selectionGroups;
        Vector2 scroll;
        SerializedProperty activeSelectionGroup;
        float width;
        static SelectionGroupEditorWindow editorWindow;
        Rect? hotRect = null;
        string hotGroup = null;

        static SelectionGroupEditorWindow()
        {
            SelectionGroupContainer.onLoaded -= OnContainerLoaded;
            SelectionGroupContainer.onLoaded += OnContainerLoaded;
        }

        void OnEnable()
        {
            titleContent.text = "Selection Groups";
            editorWindow = this;
        }

        void OnHierarchyChange()
        {
            //This is required to preserve refences when a gameobject is moved between scenes in the editor.
            SanitizeSceneReferences();
        }

        private static void SanitizeSceneReferences()
        {
            foreach (var i in SelectionGroupContainer.instanceMap.ToArray())
            {
                var scene = i.Key;
                var container = i.Value;
                foreach (var g in container.groups)
                {
                    var name = g.Key;
                    var group = g.Value;
                    foreach (var o in group.objects.ToArray())
                    {
                        if (o != null && o.scene != scene)
                        {
                            group.objects.Remove(o);
                            SelectionGroupUtility.AddObjectToGroup(o, name);
                            EditorUtility.SetDirty(container);
                        }
                    }
                }
            }
        }

        static void OnContainerLoaded(SelectionGroupContainer container)
        {
            foreach (var name in container.groups.Keys.ToArray())
            {
                var mainGroup = SelectionGroupUtility.GetFirstGroup(name);
                var importedGroup = container.groups[name];
                importedGroup.color = mainGroup.color;
                importedGroup.selectionQuery = mainGroup.selectionQuery;
                importedGroup.showMembers = mainGroup.showMembers;
                container.groups[name] = importedGroup;
            }
            foreach (var i in SelectionGroupContainer.instanceMap.Values)
            {
                //clear all results so the queries can be refreshed with items from the new scene.
                foreach (var g in i.groups.Values)
                {
                    g.ClearQueryResults();
                }
                container.gameObject.hideFlags = HideFlags.None;
                EditorUtility.SetDirty(i);
            }
            if (editorWindow != null) editorWindow.Repaint();
        }

        void OnDisable()
        {
            SelectionGroupContainer.onLoaded -= OnContainerLoaded;
            editorWindow = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {

        }

        void OnSelectionChange()
        {
        }

        [MenuItem("Window/General/Selection Groups")]
        static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<SelectionGroupEditorWindow>();
            window.ShowUtility();
        }

        internal static void MarkAllContainersDirty()
        {
            foreach (var container in SelectionGroupContainer.instanceMap.Values)
                EditorUtility.SetDirty(container);
        }

        internal static void UndoRecordObject(string msg)
        {
            foreach (var i in SelectionGroupContainer.instanceMap.Values)
                Undo.RecordObject(i, msg);
        }

        void OnGUI()
        {
            var names = SelectionGroupUtility.GetGroupNames();
            scroll = EditorGUILayout.BeginScrollView(scroll);
            if (hotRect.HasValue)
                EditorGUI.DrawRect(hotRect.Value, Color.white * 0.5f);
            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                foreach (var n in names)
                {
                    GUILayout.Space(EditorGUIUtility.singleLineHeight);
                    var rect = GUILayoutUtility.GetRect(1, EditorGUIUtility.singleLineHeight);
                    var dropRect = rect;
                    var showChildren = DrawGroupWidget(rect, n);
                    if (showChildren)
                    {
                        var members = SelectionGroupUtility.GetGameObjects(n);
                        rect = GUILayoutUtility.GetRect(1, (EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight) * members.Count);
                        dropRect.yMax = rect.yMax;
                        DrawGroupMembers(rect, n, members, allowRemove: true);
                        var queryMembers = SelectionGroupUtility.GetQueryObjects(n);
                        if (queryMembers.Count > 0)
                        {
                            var bg = GUI.backgroundColor;
                            GUI.backgroundColor = Color.yellow;
                            rect = GUILayoutUtility.GetRect(1, (EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight) * queryMembers.Count);
                            dropRect.yMax = rect.yMax;
                            DrawGroupMembers(rect, n, queryMembers, allowRemove: false);
                            GUI.backgroundColor = bg;
                        }
                    }
                    if (HandleDragEvents(dropRect, n))
                        Event.current.Use();
                }
                GUILayout.FlexibleSpace();
                GUILayout.Space(16);
                if (GUILayout.Button("Add Group"))
                {
                    CreateNewGroup(Selection.objects);
                }
                // addNewRect.yMax = GUILayoutUtility.GetLastRect().yMax;
                //This handle creating a new group by dragging onto the add button.
                var addNewRect = GUILayoutUtility.GetLastRect();
                addNewRect.yMin -= 16;
                HandleDragEvents(addNewRect, null);

                GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
                var bottom = GUILayoutUtility.GetLastRect();
                if (cc.changed)
                {
                }
            }
            EditorGUILayout.EndScrollView();

            //Unlike other drag events, this DragExited should be handled once per frame.
            if (Event.current.type == EventType.DragExited)
            {
                ExitDrag();
                Event.current.Use();
            }

            if (focusedWindow == this)
                Repaint();
        }

        private static void CreateNewGroup(Object[] objects)
        {
            UndoRecordObject("New Selection Group");
            var actualName = SelectionGroupUtility.CreateNewGroup("New Group");
            SelectionGroupUtility.AddObjectToGroup(objects, actualName);
            MarkAllContainersDirty();
        }

        void DrawGroupMembers(Rect rect, string groupName, List<GameObject> members, bool allowRemove)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            foreach (var i in members)
            {
                DrawGroupMemberWidget(rect, groupName, i, allowRemove);
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        private void DrawGroupMemberWidget(Rect rect, string groupName, GameObject g, bool allowRemove)
        {
            var content = EditorGUIUtility.ObjectContent(g, typeof(GameObject));
            if (Selection.activeGameObject == g)
            {
                GUI.Box(rect, string.Empty);
            }
            rect.x += 24;
            if (GUI.Button(rect, content, "label"))
            {
                Selection.activeGameObject = g;
                if (Event.current.button == RIGHT_MOUSE_BUTTON)
                {
                    ShowGameObjectContextMenu(rect, groupName, g, allowRemove);
                }
            }
        }

        bool DrawGroupWidget(Rect rect, string groupName)
        {
            var group = SelectionGroupUtility.GetFirstGroup(groupName);
            var content = EditorGUIUtility.IconContent("LODGroup Icon");
            content.text = $"{groupName}";
            var backgroundColor = groupName == hotGroup ? Color.white * 0.5f : Color.white * 0.4f;
            EditorGUI.DrawRect(rect, backgroundColor);
            if (HandleMouseEvents(rect, groupName))
                Event.current.Use();
            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                rect.width = 16;
                group.showMembers = EditorGUI.Toggle(rect, group.showMembers, "foldout");
                rect.x += 16;
                rect.width = EditorGUIUtility.currentViewWidth - 16;
                if (GUI.Button(rect, content, "label"))
                {
                    hotGroup = groupName;
                    Selection.objects = SelectionGroupUtility.GetMembers(groupName).ToArray();
                }
                var colorRect = rect;
                colorRect.x = colorRect.width - colorRect.height - 4;
                colorRect.width = colorRect.height;
                EditorGUI.DrawRect(colorRect, group.color);
                if (cc.changed)
                {
                    //saves the show members flag.
                    SelectionGroupUtility.UpdateGroup(groupName, group);
                    MarkAllContainersDirty();
                }
            }

            return group.showMembers;
        }

        void ShowGameObjectContextMenu(Rect rect, string groupName, GameObject g, bool allowRemove)
        {
            Selection.activeGameObject = g;
            var menu = new GenericMenu();
            var content = new GUIContent("Remove From Group");
            if (allowRemove)
                menu.AddItem(content, false, () =>
                {
                    UndoRecordObject("Remove object from group");
                    SelectionGroupUtility.RemoveObjectFromGroup(g, groupName);
                    MarkAllContainersDirty();
                });
            else
                menu.AddDisabledItem(content);
            menu.DropDown(rect);
        }

        void ShowGroupContextMenu(Rect rect, string groupName)
        {
            var menu = new GenericMenu();
           
            menu.AddItem(new GUIContent("Enable Changes"), false, () =>
            {
                UndoRecordObject("Unlock group");
                SelectionGroupUtility.UnlockGroup(groupName);
                MarkAllContainersDirty();
            });
            menu.AddItem(new GUIContent("Disable Changes"), false, () =>
            {
                UndoRecordObject("Lock group");
                SelectionGroupUtility.LockGroup(groupName);
                MarkAllContainersDirty();
            });
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Hide"), false, () =>
            {
                SceneVisibilityManager.instance.Hide(SelectionGroupUtility.GetMembers(groupName).ToArray(), false);
            });
            menu.AddItem(new GUIContent("Show"), false, () =>
            {
                SceneVisibilityManager.instance.Show(SelectionGroupUtility.GetMembers(groupName).ToArray(), false);
            });
            menu.AddSeparator(string.Empty);

            menu.AddItem(new GUIContent("Duplicate Group"), false, () =>
            {
                UndoRecordObject("Duplicate group");
                SelectionGroupUtility.DuplicateGroup(groupName);
                MarkAllContainersDirty();
            });
            menu.AddItem(new GUIContent("Configure Group"), false, () => SelectionGroupDialog.Open(groupName));
            menu.DropDown(rect);
        }

        bool HandleMouseEvents(Rect position, string groupName)
        {
            var e = Event.current;
            if (position.Contains(e.mousePosition))
            {
                switch (e.type)
                {
                    case EventType.MouseDown:
                        if (e.button == RIGHT_MOUSE_BUTTON)
                        {
                            ShowGroupContextMenu(position, groupName);
                            return true;
                        }
                        break;
                }
            }
            return false;
        }

        //This is called once per group, every frame.
        bool HandleDragEvents(Rect position, string groupName)
        {
            var e = Event.current;
            if (position.Contains(e.mousePosition))
            {
                switch (e.type)
                {
                    case EventType.DragUpdated:
                        UpdateDrag(position, groupName);
                        return true;
                    case EventType.DragPerform:
                        PerformDrag(position, groupName);
                        return true;
                }
            }
            return false;
        }

        void ExitDrag()
        {
            hotRect = null;
        }

        void UpdateDrag(Rect rect, string groupName)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            hotRect = rect;
            DragAndDrop.AcceptDrag();
        }

        void PerformDrag(Rect position, string groupName)
        {
            if (groupName == null)
            {
                CreateNewGroup(DragAndDrop.objectReferences);
            }
            else
            {
                UndoRecordObject("Drop objects into group");
                SelectionGroupUtility.AddObjectToGroup(DragAndDrop.objectReferences, groupName);
                MarkAllContainersDirty();
                hotRect = null;
            }
        }
    }
}
