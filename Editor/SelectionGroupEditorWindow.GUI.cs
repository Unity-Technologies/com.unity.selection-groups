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

    public partial class SelectionGroupEditorWindow : EditorWindow
    {

        [MenuItem("Window/General/Selection Groups")]
        static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<SelectionGroupEditorWindow>();
            window.ShowUtility();
        }

        void DrawGUI()
        {
            var names = SelectionGroupManager.instance.GetGroupNames();
            scroll = EditorGUILayout.BeginScrollView(scroll);
            if (hotRect.HasValue)
                EditorGUI.DrawRect(hotRect.Value, Color.white * 0.5f);
            if (GUILayout.Button("Add Group"))
            {
                CreateNewGroup(Selection.objects);
            }
            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                foreach (var group in SelectionGroupManager.instance)
                {

                    var isActive = activeNames.Contains(group.name);
                    GUILayout.Space(3);
                    var rect = GUILayoutUtility.GetRect(1, EditorGUIUtility.singleLineHeight);
                    var dropRect = rect;
                    //early out if this group yMin is below window rect (not visible).
                    if ((rect.yMin - scroll.y) > position.height) break;
                    var showChildren = DrawHeader(rect, group, isActive: isActive);
                    if (showChildren)
                    {
                        rect = GUILayoutUtility.GetRect(1, (EditorGUIUtility.singleLineHeight) * group.Count);

                        dropRect.yMax = rect.yMax;
                        //early out if this group yMax is above window rect (not visible).
                        if (rect.yMax - scroll.y < 0)
                            continue;
                        DrawAllGroupMembers(rect, group, allowRemove: true);
                    }

                    if (HandleGroupDragEvents(dropRect, group))
                        Event.current.Use();
                }
                GUILayout.FlexibleSpace();
                GUILayout.Space(4);

                //This handles creating a new group by dragging onto the add button.
                var addNewRect = GUILayoutUtility.GetLastRect();
                addNewRect.yMin -= 16;
                HandleGroupDragEvents(addNewRect, null);

                GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
                var bottom = GUILayoutUtility.GetLastRect();
                if (cc.changed)
                {
                }
            }
            EditorGUILayout.EndScrollView();
        }

        void SetupStyles()
        {
            if (miniButtonStyle == null)
            {
                miniButtonStyle = EditorStyles.miniButton;
                miniButtonStyle.padding = new RectOffset(0, 0, 0, 0);
            }
        }

        void DrawAllGroupMembers(Rect rect, SelectionGroup group, bool allowRemove)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            foreach (var i in group)
            {
                //if rect is below window, early out.
                if (rect.yMin - scroll.y > position.height) return;
                //if rect is in window, draw.
                if (rect.yMax - scroll.y > 0)
                    DrawGroupMember(rect, group, i, allowRemove);
                rect.y += rect.height;
            }
        }

        void DrawGroupMember(Rect rect, SelectionGroup group, Object g, bool allowRemove)
        {
            var content = EditorGUIUtility.ObjectContent(g, g.GetType());
            var isThisMemberSelected = activeSelection.Contains(g);
            var isMouseOver = rect.Contains(Event.current.mousePosition);
            var isManySelected = activeSelection.Count > 1;
            var isAnySelected = activeSelection.Count > 0;
            var isLeftButton = Event.current.button == LEFT_MOUSE_BUTTON;
            var isRightButton = Event.current.button == RIGHT_MOUSE_BUTTON;
            var isMouseDown = Event.current.type == EventType.MouseDown;
            var isMouseUp = Event.current.type == EventType.MouseUp;
            var isNoSelection = activeSelection.Count == 0;
            var isControl = Event.current.control;
            var isLeftMouseDown = isMouseOver && isLeftButton && isMouseDown;
            var isLeftMouseUp = isMouseOver && isLeftButton && isMouseUp;

            if (isThisMemberSelected || isLeftMouseDown)
                EditorGUI.DrawRect(rect, SELECTION_COLOR);

            rect.x += 24;
            GUI.contentColor = allowRemove ? Color.white : Color.Lerp(Color.white, Color.yellow, 0.25f);
            GUI.Label(rect, content);
            GUI.contentColor = Color.white;


            //left click down = set active
                //if drag then begin drag op
                //if left click up 
                    //if ctrl then add selecttion
                    //else if shift then add selection between first and current
                    //else set selection to object
                
            

            if (isLeftMouseDown)
            {
                activeSelectionGroup = group;
                if (isNoSelection)
                    QueueSelectionOperation(SelectionCommand.Set, g);
                else
                {
                    if (isThisMemberSelected)
                    {
                        if (isControl)
                            QueueSelectionOperation(SelectionCommand.Remove, g);
                        else if (isManySelected)
                            QueueSelectionOperation(SelectionCommand.Set, g);
                    }
                    else
                    {
                        if (isControl)
                            QueueSelectionOperation(SelectionCommand.Add, g);
                        else
                            QueueSelectionOperation(SelectionCommand.Set, g);
                    }
                }
            }
            else if (isRightButton && isMouseOver && isMouseDown && isThisMemberSelected)
            {
                ShowGameObjectContextMenu(rect, group, g, allowRemove);
            }


            if (Event.current.type == EventType.MouseDrag && rect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = Selection.objects;
                DragAndDrop.StartDrag(g.name);
            }
        }

        bool DrawHeader(Rect rect, SelectionGroup group, bool isActive)
        {
            if (group == null) return false;
            var content = EditorGUIUtility.IconContent("LODGroup Icon");
            content.text = $"{group.name}";
            var backgroundColor = group == activeSelectionGroup ? Color.white * 0.6f : Color.white * 0.3f;
            EditorGUI.DrawRect(rect, backgroundColor);
            if (HandleMouseEvents(rect, group.name, group))
                Event.current.Use();

            rect.width = 16;
            group.showMembers = EditorGUI.Toggle(rect, group.showMembers, "foldout");
            rect.x += 16;
            rect.width = EditorGUIUtility.currentViewWidth - 96;

            if (GUI.Button(rect, content, "label"))
            {
                group.showMembers = !group.showMembers;
                if (group.showMembers)
                {
                    activeSelectionGroup = group;
                }
            }
            rect.x += rect.width;
            rect.width = 16;
            // if(GUI.Button(rect,"")) {
            //     group.DebugGIDS();
            // }
            
            rect.xMax = position.xMax;
            
            EditorGUI.DrawRect(rect, new Color(group.color.r, group.color.g, group.color.b));




            return group.showMembers;
        }

        void ShowGameObjectContextMenu(Rect rect, SelectionGroup group, Object g, bool allowRemove)
        {
            // Selection.activeObject = g;
            var menu = new GenericMenu();
            var content = new GUIContent("Remove From Group");
            if (allowRemove)
                menu.AddItem(content, false, () =>
                {
                    Undo.RegisterCompleteObjectUndo(SelectionGroupManager.instance, "Remove");
                    group.Remove(Selection.objects);
                });
            else
                menu.AddDisabledItem(content);
            menu.DropDown(rect);
        }

        void ShowGroupContextMenu(Rect rect, string groupName, SelectionGroup group)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Duplicate Group"), false, () =>
            {
                Undo.RegisterCompleteObjectUndo(SelectionGroupManager.instance, "Duplicate");
                SelectionGroupManager.instance.DuplicateGroup(group.groupId);
            });
            menu.AddItem(new GUIContent("Clear Group"), false, () =>
            {
                Undo.RegisterCompleteObjectUndo(SelectionGroupManager.instance, "Clear");
                group.Clear();
            });
            menu.AddItem(new GUIContent("Configure Group"), false, () => SelectionGroupConfigurationDialog.Open(group, this));
            menu.AddItem(new GUIContent("Delete Group"), false, () =>
            {
                Undo.RegisterCompleteObjectUndo(SelectionGroupManager.instance, "Delete");
                SelectionGroupManager.instance.RemoveGroup(group.groupId);
            });
            menu.DropDown(rect);
        }

        bool HandleMouseEvents(Rect position, string groupName, SelectionGroup group)
        {
            var e = Event.current;
            if (position.Contains(e.mousePosition))
            {
                switch (e.type)
                {
                    case EventType.MouseDown:
                        if (e.button == RIGHT_MOUSE_BUTTON)
                        {
                            ShowGroupContextMenu(position, groupName, group);
                            return true;
                        }
                        break;
                }
            }
            return false;
        }
    }
}
