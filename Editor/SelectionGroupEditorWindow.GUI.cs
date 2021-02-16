using System.Linq;
using Unity.SelectionGroups;
using Unity.SelectionGroups.Runtime;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;


namespace Unity.SelectionGroupsEditor
{

    public partial class SelectionGroupEditorWindow : EditorWindow
    {
        private const string AddGroup = "Add Group";
        private GUIContent editorHeaderContent, sceneHeaderContent;

        [MenuItem("Window/General/Selection Groups")]
        static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<SelectionGroupEditorWindow>();
            window.Show();
        }

        float CalculateHeight()
        {
            var height = EditorGUIUtility.singleLineHeight;
            foreach (var i in SelectionGroupManager.Groups)
            {
                height += EditorGUIUtility.singleLineHeight + 3;
                if (i.ShowMembers)
                {
                    height += i.Count * EditorGUIUtility.singleLineHeight;
                }
            }

            return height;
        }

        void DrawGUI()
        {
            var viewRect = Rect.zero;
            viewRect.width = position.width-16;
            viewRect.height = CalculateHeight();
            var windowRect = new Rect(0, 0, position.width, position.height);
            scroll = GUI.BeginScrollView(windowRect, scroll, viewRect);
            
            var cursor = new Rect(0, 0, position.width-16, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(cursor, AddGroup)) CreateNewGroup();
            cursor.y += cursor.height;
            
            foreach (var group in SelectionGroupManager.Groups)
            {
                if (group == null) continue;
                var isActive = activeNames.Contains(group.Name);
                cursor.y += 3;
                
                //early out if this group yMin is below window rect (not visible).
                if ((cursor.yMin - scroll.y) > position.height) break;
                cursor = DrawHeader(cursor, group, out bool showChildren, isActive: isActive);
                var dropRect = cursor;

                if (showChildren)
                {
                    // dropRect.yMax = rect.yMax;
                    //early out if this group yMax is above window rect (not visible).
                    // if (rect.yMax - scroll.y < 0)
                        // continue;
                    cursor = DrawAllGroupMembers(cursor, group, allowRemove: true);
                }
                try
                {
                    HandleDragEvents(dropRect, group);
                }
                catch (SelectionGroupException ex)
                {
                    ShowNotification(new GUIContent(ex.Message));
                }
            }
            //Handle clicks on blank areas of window.
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Selection.objects = new Object[] { };
                UpdateActiveSelection();
                Event.current.Use();
            }
            GUI.EndScrollView();

        }

        void ShowSettings()
        {
            var menu = new GenericMenu();
            menu.ShowAsContext();
        }
        
        void SetupStyles()
        {
            if (miniButtonStyle == null)
            {
                miniButtonStyle = EditorStyles.miniButton;
                miniButtonStyle.padding = new RectOffset(0, 0, 0, 0);
            }
        }

        Rect DrawAllGroupMembers(Rect rect, ISelectionGroup group, bool allowRemove)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            foreach (var i in group.Members)
            {
                //if rect is below window, early out.
                if (rect.yMin - scroll.y > position.height) return rect;
                //if rect is in window, draw.
                if (rect.yMax - scroll.y > 0)
                    DrawGroupMember(rect, group, i, allowRemove);
                rect.y += rect.height;
            }

            return rect;
        }

        void DrawGroupMember(Rect rect, ISelectionGroup group, UnityEngine.Object g, bool allowRemove)
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
            {
                hotMember = g;
                activeSelectionGroup = group;
                e.Use();
            }

            if (isControl)
            {
                if (isLeftMouseUp && isHotMember && isInSelection)
                {
                    activeSelection.Remove(g);
                    activeSelectionGroup = group;
                    updateSelectionObjects = true;
                    hotMember = null;
                    e.Use();
                }
                if (isLeftMouseUp && isHotMember && !isInSelection)
                {
                    activeSelection.Add(g);
                    activeSelectionGroup = group;
                    updateSelectionObjects = true;
                    hotMember = null;
                    e.Use();
                }
            }
            else if (isShift)
            {
                if (isLeftMouseUp && isHotMember)
                {
                    activeSelection.Add(g);
                    int firstIndex = -1, lastIndex = -1;
                    var objects = group.Members;
                    for (var i = 0; i < objects.Count; i++)
                    {
                        if (activeSelection.Contains(objects[i]))
                        {
                            if (firstIndex < 0)
                                firstIndex = i;
                            lastIndex = i;
                        }
                    }
                    for (var i = firstIndex; i < lastIndex; i++)
                        activeSelection.Add(objects[i]);
                    updateSelectionObjects = true;
                    hotMember = null;
                    e.Use();
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
                        e.Use();
                    }
                    else if (!isInSelection)
                    {
                        activeSelection.Clear();
                        activeSelection.Add(g);
                        updateSelectionObjects = true;
                        e.Use();
                    }
                    else
                    {
                        //TODO: add a rename overlay
                    }
                    hotMember = null;
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
                e.Use();
            }
            
            if (updateSelectionObjects)
                Selection.objects = activeSelection.ToArray();

        }

        Rect DrawHeader(Rect cursor, ISelectionGroup group, out bool showChildren, bool isActive)
        {
            var rect = cursor;
            var isAvailableInEditMode = true;
            GUIContent content;
            if (group.Scope == SelectionGroupScope.Editor)
                content = editorHeaderContent;
            else
                content = sceneHeaderContent;
                    
            //Editor groups don't work in play mode, as GetGloBALoBJECTiD does not work in play mode.
            if (group.Scope == SelectionGroupScope.Editor && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                content.text = $"{group.Name} (Not available in play mode)";
                isAvailableInEditMode = false;
            }
            else
            {
                content.text = $"{group.Name}";    
            }
            
            var backgroundColor = group == activeSelectionGroup ? Color.white * 0.6f : Color.white * 0.3f;
            EditorGUI.DrawRect(rect, backgroundColor);
            {
                rect.width = 16;
                group.ShowMembers = EditorGUI.Toggle(rect, group.ShowMembers, "foldout");
                rect.x += 16;
                rect.width = EditorGUIUtility.currentViewWidth - 128;
            }
            if(isAvailableInEditMode)
                HandleHeaderMouseEvents(rect, group.Name, group);
            GUI.Label(rect, content, "label");

            rect.x += rect.width;
            rect = DrawTools(rect, group);
            rect.x += 8;
            rect.xMax = position.xMax;

            EditorGUI.DrawRect(rect, new Color(group.Color.r, group.Color.g, group.Color.b));

            showChildren =  isAvailableInEditMode ? group.ShowMembers : false;
            rect.x = cursor.x;
            rect.y += rect.height;
            return rect;
        }

        Rect DrawTools(Rect rect, ISelectionGroup group)
        {
            rect.width = 18;
            rect.height = 18;
            if (group.EnabledTools.Count == 0) return rect;
            
            foreach (var i in TypeCache.GetMethodsWithAttribute<SelectionGroupToolAttribute>())
            {
                var attr = AttributeCache.GetCustomAttribute<SelectionGroupToolAttribute>(i);
                if (!group.EnabledTools.Contains(attr.toolId))
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

        void ShowGameObjectContextMenu(Rect rect, ISelectionGroup group, UnityEngine.Object g, bool allowRemove)
        {
            var menu = new GenericMenu();
            var content = new GUIContent("Remove From Group");
            if (allowRemove)
                menu.AddItem(content, false, () =>
                {
                    RegisterUndo(group, "Remove Member");
                    group.Query = "";
                    group.Remove(Selection.objects);
                });
            else
                menu.AddDisabledItem(content);
            menu.DropDown(rect);
        }

        void ShowGroupContextMenu(Rect rect, string groupName, ISelectionGroup group)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Select All"), false, () =>
            {
                Selection.objects = activeSelectionGroup.Members.ToArray();
                UpdateActiveSelection();
            });
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Clear Group"), false, () =>
            {
                group.Clear();
            });
            menu.AddItem(new GUIContent("Configure Group"), false, () => SelectionGroupConfigurationDialog.Open(group, this));
            if (group.Scope == SelectionGroupScope.Editor)
            {
                menu.AddItem(new GUIContent("Move to Scene"), false, () =>
                {
                    SelectionGroupManager.ChangeGroupScope(group, SelectionGroupScope.Scene);
                });
            }
            else
            {
                menu.AddItem(new GUIContent("Move to Editor"), false, () =>
                {
                    SelectionGroupManager.ChangeGroupScope(group, SelectionGroupScope.Editor);
                });
            }

            menu.AddItem(new GUIContent("Delete Group"), false, () =>
            {
                SelectionGroupManager.Delete(group);
            });
            menu.DropDown(rect);
        }

        void HandleHeaderMouseEvents(Rect rect, string groupName, ISelectionGroup group)
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
                        DragAndDrop.objectReferences = group.Members.ToArray();
                        e.Use();
                        break;
                }
            }
        }
    }
}
