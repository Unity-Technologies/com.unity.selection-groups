using System;
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
            window.Show();
        }

        void DrawGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawDebugTools();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Group"))
            {
                CreateNewGroup(Selection.objects);
            }
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Settings"), "label", GUILayout.ExpandWidth(false)))
            {
                ShowSettings();
            }
            GUILayout.EndHorizontal();

            foreach (var group in SelectionGroupManager.instance)
            {
                // Debug.Log(group);
                var isActive = activeNames.Contains(group.name);
                GUILayout.Space(3);
                var rect = GUILayoutUtility.GetRect(1, EditorGUIUtility.singleLineHeight);
                //early out if this group yMin is below window rect (not visible).
                if ((rect.yMin - scroll.y) > position.height) break;
                var showChildren = DrawHeader(rect, group, isActive: isActive);
                var dropRect = rect;

                if (showChildren)
                {
                    rect = GUILayoutUtility.GetRect(1, (EditorGUIUtility.singleLineHeight) * group.Count);

                    dropRect.yMax = rect.yMax;
                    //early out if this group yMax is above window rect (not visible).
                    if (rect.yMax - scroll.y < 0)
                        continue;
                    DrawAllGroupMembers(rect, group, allowRemove: true);
                }
                try
                {

                    HandleGroupDragEvents(dropRect, group);
                }
                catch (SelectionGroupException ex)
                {
                    ShowNotification(new GUIContent(ex.Message));
                }
            }
            EditorGUILayout.EndScrollView();

        }

        void ShowSettings()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Enable Runtime Groups"), SelectionGroupManager.instance.enablePlayModeSelectionGroups, () =>
            {
                SelectionGroupManager.instance.enablePlayModeSelectionGroups = !SelectionGroupManager.instance.enablePlayModeSelectionGroups;
                SelectionGroupManager.instance.UpdateSelectionGroupContainers();
            });
            menu.ShowAsContext();
        }

        void DrawDebugTools()
        {
            // GUILayout.BeginHorizontal();
            // if (GUILayout.Button("DMP"))
            // {

            // }
            // if (GUILayout.Button("SAV"))
            // {
            //     SelectionGroupManager.instance.Save();
            // }

            // GUILayout.EndHorizontal();
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

        void DrawGroupMember(Rect rect, SelectionGroup group, UnityEngine.Object g, bool allowRemove)
        {
            if (g == null) return;
            var e = Event.current;
            var content = EditorGUIUtility.ObjectContent(g, g.GetType());
            var isInSelection = activeSelection.Contains(g);
            var isMouseOver = rect.Contains(e.mousePosition);
            var isMouseDrag = e.type == EventType.MouseDrag;
            var isManySelected = activeSelection.Count > 1;
            var isAnySelected = activeSelection.Count > 0;
            var isLeftButton = e.button == LEFT_MOUSE_BUTTON;
            var isRightButton = e.button == RIGHT_MOUSE_BUTTON;
            var isMouseDown = e.type == EventType.MouseDown;
            var isMouseUp = e.type == EventType.MouseUp;
            var isNoSelection = activeSelection.Count == 0;
            var isControl = e.control;
            var isShift = e.shift;
            var isLeftMouseDown = isMouseOver && isLeftButton && isMouseDown;
            var isLeftMouseUp = isMouseOver && isLeftButton && isMouseUp;
            var isHotMember = g == hotMember;
            var updateSelectionObjects = false;

            if (isMouseOver)
                EditorGUI.DrawRect(rect, HOVER_COLOR);

            if (isLeftMouseDown)
                hotMember = g;


            if (isControl)
            {
                if (isLeftMouseUp && isHotMember && isInSelection)
                {
                    activeSelection.Remove(g);
                    activeSelectionGroup = group;
                    updateSelectionObjects = true;
                }
                if (isLeftMouseUp && isHotMember && !isInSelection)
                {
                    activeSelection.Add(g);
                    activeSelectionGroup = group;
                    updateSelectionObjects = true;
                }
            }
            else if (isShift)
            {
                if (isLeftMouseUp && isHotMember)
                {
                    activeSelection.Add(g);
                    int firstIndex = -1, lastIndex = -1;
                    for (var i = 0; i < group.Count; i++)
                    {
                        if (activeSelection.Contains(group[i]))
                        {
                            if (firstIndex < 0)
                                firstIndex = i;
                            lastIndex = i;
                        }
                    }
                    for (var i = firstIndex; i < lastIndex; i++)
                        activeSelection.Add(group[i]);
                    updateSelectionObjects = true;
                }
            }
            else
            {
                if (isLeftMouseUp && isHotMember)
                {
                    if (isInSelection && isManySelected)
                    {
                        activeSelection.Clear();
                        activeSelection.Add(g);
                        updateSelectionObjects = true;
                    }
                    else if (!isInSelection)
                    {
                        activeSelection.Clear();
                        activeSelection.Add(g);
                        updateSelectionObjects = true;
                    }
                    else
                    {
                        //TODO: add a rename overlay
                    }
                }
            }

            if (isInSelection)
                EditorGUI.DrawRect(rect, SELECTION_COLOR);

            if (g.hideFlags.HasFlag(HideFlags.NotEditable))
            {
                var icon = EditorGUIUtility.IconContent("InspectorLock");
                var irect = rect;
                irect.width = 16;
                irect.height = 14;
                GUI.DrawTexture(irect, icon.image);
            }
            rect.x += 24;
            GUI.contentColor = allowRemove ? Color.white : Color.Lerp(Color.white, Color.yellow, 0.25f);
            GUI.Label(rect, content);
            GUI.contentColor = Color.white;

            if (isRightButton && isMouseOver && isMouseDown && isInSelection)
            {
                ShowGameObjectContextMenu(rect, group, g, allowRemove);
            }


            if (isMouseOver && isMouseDrag)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = Selection.objects;
                DragAndDrop.StartDrag(g.name);
            }

            if (updateSelectionObjects)
                Selection.objects = activeSelection.ToArray();

        }

        bool DrawHeader(Rect rect, SelectionGroup group, bool isActive)
        {
            // if (group == null) return false;
            var content = EditorGUIUtility.IconContent("LODGroup Icon");
            content.text = $"{group.name} ({group.Count}/{group.TotalCount})";
            var backgroundColor = group == activeSelectionGroup ? Color.white * 0.6f : Color.white * 0.3f;
            EditorGUI.DrawRect(rect, backgroundColor);


            rect.width = 16;
            group.showMembers = EditorGUI.Toggle(rect, group.showMembers, "foldout");
            rect.x += 16;
            rect.width = EditorGUIUtility.currentViewWidth - 128;

            HandleHeaderMouseEvents(rect, group.name, group);
            GUI.Label(rect, content, "label");

            rect.x += rect.width;
            rect = DrawTools(rect, group);
            rect.x += 8;
            rect.xMax = position.xMax;

            EditorGUI.DrawRect(rect, new Color(group.color.r, group.color.g, group.color.b));

            return group.showMembers;
        }

        Rect DrawTools(Rect rect, SelectionGroup group)
        {
            rect.width = 18;
            rect.height = 18;
            
            foreach (var i in TypeCache.GetMethodsWithAttribute<SelectionGroupToolAttribute>())
            {
                var attr = AttributeCache.GetCustomAttribute<SelectionGroupToolAttribute>(i);
                if (!group.enabledTools.Contains(attr.toolId))
                    continue;
                var content = EditorGUIUtility.IconContent(attr.icon);
                content.tooltip = attr.description;
                if (GUI.Button(rect, content, miniButtonStyle))
                {
                    try
                    {
                        i.Invoke(null, new object[] { group });
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                rect.x += rect.width;
            }

            return rect;
        }

        void ShowGameObjectContextMenu(Rect rect, SelectionGroup group, UnityEngine.Object g, bool allowRemove)
        {
            // Selection.activeObject = g;
            var menu = new GenericMenu();
            var content = new GUIContent("Remove From Group");
            if (allowRemove)
                menu.AddItem(content, false, () =>
                {
                    Undo.RegisterCompleteObjectUndo(SelectionGroupManager.instance, "Remove");
                    group.query = "";
                    group.Remove(Selection.objects);
                    SelectionGroupManager.instance.SetIsDirty();

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
                SelectionGroupManager.instance.DuplicateGroup(group.groupId);
                SelectionGroupManager.instance.SetIsDirty();

            });
            menu.AddItem(new GUIContent("Clear Group"), false, () =>
            {
                Undo.RegisterCompleteObjectUndo(SelectionGroupManager.instance, "Clear");
                group.Clear();
                SelectionGroupManager.instance.SetIsDirty();
            });
            menu.AddItem(new GUIContent("Configure Group"), false, () => SelectionGroupConfigurationDialog.Open(group, this));
            menu.AddItem(new GUIContent("Delete Group"), false, () =>
            {
                SelectionGroupManager.instance.RemoveGroup(group.groupId);
                SelectionGroupManager.instance.SetIsDirty();

            });
            menu.DropDown(rect);
        }

        void HandleHeaderMouseEvents(Rect rect, string groupName, SelectionGroup group)
        {
            var e = Event.current;
            if (rect.Contains(e.mousePosition))
            {
                switch (e.type)
                {
                    case EventType.MouseDown:
                        switch (e.button)
                        {
                            case RIGHT_MOUSE_BUTTON:
                                ShowGroupContextMenu(rect, groupName, group);
                                break;
                            case LEFT_MOUSE_BUTTON:
                                if (e.clickCount == 1)
                                    activeSelectionGroup = group;
                                else
                                    SelectionGroupConfigurationDialog.Open(group, this);
                                break;
                        }

                        break;
                    case EventType.MouseDrag:
                        DragAndDrop.PrepareStartDrag();
                        DragAndDrop.StartDrag(groupName);
                        DragAndDrop.objectReferences = group.ToArray();
                        e.Use();
                        break;
                }
            }
        }
    }
}
