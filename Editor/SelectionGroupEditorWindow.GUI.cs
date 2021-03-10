using System.Linq;
using NUnit.Framework;
using Unity.SelectionGroups;
using Unity.SelectionGroups.Runtime;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Unity.SelectionGroupsEditor
{

    internal partial class SelectionGroupEditorWindow : EditorWindow
    {
        private const string AddGroup    = "Add Group";
        private const int    RightMargin = 16;
        
        private GUIStyle Foldout;
        private GUIStyle Label;
        private GUIContent editorHeaderContent, sceneHeaderContent;
        private GUIContent InspectorLock;
        

        [MenuItem("Window/General/Selection Groups")]
        static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<SelectionGroupEditorWindow>();
            window.Show();
        }

        float CalculateHeight()
        {
            var height = EditorGUIUtility.singleLineHeight;
            var groups = SelectionGroupManager.Groups;
            for (var i=0; i<groups.Count; i++)
            {
                var group = groups[i];
                height += EditorGUIUtility.singleLineHeight + 3;
                if (group.ShowMembers)
                {
                    height += group.Count * EditorGUIUtility.singleLineHeight;
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
            
            Rect cursor = new Rect(0, 0, position.width-RightMargin, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(cursor, AddGroup)) CreateNewGroup();
            cursor.y += cursor.height;

            var groups = SelectionGroupManager.Groups;
            for (var i=0; i<groups.Count; i++)
            {
                var group = groups[i];
                if (group == null) continue;
                cursor.y += 3;
                
                //early out if this group yMin is below window rect (not visible).
                if ((cursor.yMin - scroll.y) > position.height) break;
                var dropRect = cursor;
                cursor = DrawHeader(cursor, group, out bool showChildren);

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

        void SetupStyles()
        {
            if (miniButtonStyle == null)
            {
                miniButtonStyle = EditorStyles.miniButton;
                miniButtonStyle.padding = new RectOffset(0, 0, 0, 0); 
                Foldout = "foldout";
                Label = "label";
                InspectorLock = EditorGUIUtility.IconContent("InspectorLock");
            }
        }

        Rect DrawAllGroupMembers(Rect rect, ISelectionGroup group, bool allowRemove)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            foreach (Object i in group.Members) 
            {
                if (null == i)
                    continue;
                
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
            Assert.IsNotNull(g);
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
            var isPaint = e.type == EventType.Repaint;

            if (isMouseOver && isPaint)
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

            if (isPaint)
            {
                if (isInSelection)
                    EditorGUI.DrawRect(rect, SELECTION_COLOR);

                if (g.hideFlags.HasFlag(HideFlags.NotEditable))
                {
                    var icon = InspectorLock;
                    var irect = rect;
                    irect.width = 16;
                    irect.height = 14;
                    GUI.DrawTexture(irect, icon.image);
                }

                rect.x += 24;
                GUI.contentColor = allowRemove ? Color.white : Color.Lerp(Color.white, Color.yellow, 0.25f);
                GUI.Label(rect, content);
                GUI.contentColor = Color.white;
            }

            if (isRightButton && isMouseOver && isMouseDown && isInSelection)
            {
                ShowGameObjectContextMenu(rect, group, g, allowRemove);
                e.Use();
            }
            
            if (updateSelectionObjects)
                Selection.objects = activeSelection.ToArray();

        }
        
        Rect DrawHeader(Rect cursor, ISelectionGroup group, out bool showChildren) 
        {           
            bool isPaint = Event.current.type == EventType.Repaint;            
            Rect rect = new Rect(cursor) {x = 0, };            
            bool isAvailableInEditMode = true;
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
            
            Color backgroundColor = group == activeSelectionGroup ? Color.white * 0.6f : Color.white * 0.3f;
            if(isPaint) 
                EditorGUI.DrawRect(rect, backgroundColor);
            
            //foldout and label
            const int FOLDOUT_WIDTH   = 16;
            const int COLOR_WIDTH     = 128;
            const int SEPARATOR_WIDTH = 8;
            float labelWidth = EditorGUIUtility.currentViewWidth
                             - (COLOR_WIDTH + FOLDOUT_WIDTH + RightMargin + SEPARATOR_WIDTH);
            {
                rect.width        =  FOLDOUT_WIDTH;
                group.ShowMembers =  EditorGUI.Toggle(rect, group.ShowMembers, Foldout);
                rect.x            += FOLDOUT_WIDTH;
                rect.width        =  labelWidth;
            }
            if(isAvailableInEditMode)
                HandleHeaderMouseEvents(rect, group.Name, group);
            if(isPaint) 
                GUI.Label(rect, content, Label);

            rect.x     += rect.width;
            rect       =  DrawTools(rect, group);
            rect.x     += SEPARATOR_WIDTH;
            rect.width =  COLOR_WIDTH;

            if(isPaint) EditorGUI.DrawRect(rect, new Color(group.Color.r, group.Color.g, group.Color.b));

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
