using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
        
        private GUIStyle   Label;
        //private GUIContent editorHeaderContent;         //[TODO-sin:2021-12-20] Remove in version 0.7.0 
        private GUIContent sceneHeaderContent;
        private GUIContent InspectorLock;       

        private static readonly Color ProTextColor = new Color(0.824f, 0.824f, 0.824f, 1f);
        

        [MenuItem("Window/General/Selection Groups")]
        static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<SelectionGroupEditorWindow>();
            window.Show();
        }

        float CalculateHeight()
        {
            var height = EditorGUIUtility.singleLineHeight;
            var groups = SelectionGroupManager.GetOrCreateInstance().Groups;
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

            var groups = SelectionGroupManager.GetOrCreateInstance().Groups;
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
                Label = "label";
                InspectorLock = EditorGUIUtility.IconContent("InspectorLock");
            }
        }

        Rect DrawAllGroupMembers(Rect rect, ISelectionGroup group, bool allowRemove)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            foreach (Object i in group.Members) 
            {
                if (i == null)
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
            Event e = Event.current;
            GUIContent content = EditorGUIUtility.ObjectContent(g, g.GetType());
            bool isMouseOver = rect.Contains(e.mousePosition);
            bool isPaint = e.type == EventType.Repaint;

            if (isMouseOver && isPaint)
                EditorGUI.DrawRect(rect, HOVER_COLOR);

            bool isInSelection = IsGroupMemberSelected(group, g);

            if (isPaint) {
                if (isInSelection)
                    EditorGUI.DrawRect(rect, SELECTION_COLOR);

                if (g.hideFlags.HasFlag(HideFlags.NotEditable)) {
                    GUIContent icon  = InspectorLock;
                    Rect irect = rect;
                    irect.width  = 16;
                    irect.height = 14;
                    GUI.DrawTexture(irect, icon.image);
                }

                rect.x           += 24;
                GUI.contentColor =  allowRemove ? Color.white : Color.Lerp(Color.white, Color.yellow, 0.25f);
                GUI.Label(rect, content);
                GUI.contentColor = Color.white;
            }
            
            //[TODO-sin: 2021-12-24] if the gameobject belongs to two groups then selecting it will select it in both groups
            // var isMouseDrag = e.type == EventType.MouseDrag;
            // var isManySelected = activeSelection.Count > 1;
            // var isAnySelected = activeSelection.Count > 0;
            // var isLeftButton = e.button == LEFT_MOUSE_BUTTON;
            // var isRightButton = e.button == RIGHT_MOUSE_BUTTON;
            // var isMouseDown = e.type == EventType.MouseDown;
            // var isMouseUp = e.type == EventType.MouseUp;
            // var isNoSelection = activeSelection.Count == 0;
            // var isControl = e.control;
            // var isShift = e.shift;
            // var isLeftMouseDown = isMouseOver && isLeftButton && isMouseDown;
            // var isLeftMouseUp = isMouseOver && isLeftButton && isMouseUp;
            // var isHotMember = g == hotMember;
            // var updateSelectionObjects = false;
            HandleGroupMemberMouseEvents(rect);
            
            // if (isLeftMouseDown)
            // {
            //     hotMember = g;
            //     activeSelectionGroup = group;
            //     e.Use();
            // }
            //
            // if (isControl)
            // {
            //     if (isLeftMouseUp && isHotMember && isInSelection)
            //     {
            //         activeSelection.Remove(g);
            //         activeSelectionGroup = group;
            //         updateSelectionObjects = true;
            //         hotMember = null;
            //         e.Use();
            //     }
            //     if (isLeftMouseUp && isHotMember && !isInSelection)
            //     {
            //         activeSelection.Add(g);
            //         activeSelectionGroup = group;
            //         updateSelectionObjects = true;
            //         hotMember = null;
            //         e.Use();
            //     }
            // }
            // else if (isShift)
            // {
            //     if (isLeftMouseUp && isHotMember)
            //     {
            //         activeSelection.Add(g);
            //         int firstIndex = -1, lastIndex = -1;
            //         var objects = group.Members;
            //         for (var i = 0; i < objects.Count; i++)
            //         {
            //             if (activeSelection.Contains(objects[i]))
            //             {
            //                 if (firstIndex < 0)
            //                     firstIndex = i;
            //                 lastIndex = i;
            //             }
            //         }
            //         for (var i = firstIndex; i < lastIndex; i++)
            //             activeSelection.Add(objects[i]);
            //         updateSelectionObjects = true;
            //         hotMember = null;
            //         e.Use();
            //     }
            // }
            // else
            // {
            //     if (isLeftMouseUp && isHotMember)
            //     {
            //         if (isInSelection && isManySelected)
            //         {
            //             activeSelection.Clear();
            //             activeSelection.Add(g);
            //             updateSelectionObjects = true;
            //             e.Use();
            //         }
            //         else if (!isInSelection)
            //         {
            //             activeSelection.Clear();
            //             activeSelection.Add(g);
            //             updateSelectionObjects = true;
            //             e.Use();
            //         }
            //         else
            //         {
            //             //TODO: add a rename overlay
            //         }
            //         hotMember = null;
            //     }
            // }
            //
            // if (isRightButton && isMouseOver && isMouseDown && isInSelection)
            // {
            //     ShowGameObjectContextMenu(rect, group, g, allowRemove);
            //     e.Use();
            // }
            //
            // if (updateSelectionObjects)
            //     Selection.objects = activeSelection.ToArray();

        }
        
        Rect DrawHeader(Rect cursor, SelectionGroup group, out bool showChildren) 
        {           
            bool isPaint = Event.current.type == EventType.Repaint;            
            Rect rect = new Rect(cursor) {x = 0, };            
            bool isAvailableInEditMode = true;
            GUIContent content = sceneHeaderContent;
            
            //[TODO-sin:2021-12-20] Remove in version 0.7.0             
            // if (group.Scope == SelectionGroupDataLocation.Editor)
            //     content = editorHeaderContent;
            // else
            //     content = sceneHeaderContent;
                    
            //Editor groups don't work in play mode, as GetGloBALoBJECTiD does not work in play mode.
            //[TODO-sin:2021-12-20] Remove in version 0.7.0             
            // if (group.Scope == SelectionGroupDataLocation.Editor && EditorApplication.isPlayingOrWillChangePlaymode)
            // {
            //     content.text = $"{group.Name} (Not available in play mode)";
            //     isAvailableInEditMode = false;
            // }
            // else
            // {
            //     content.text = $"{group.Name}";    
            // }
            
            content.text = $"{group.Name}";

            //
            const float FOLDOUT_WIDTH    = 16;
            const float COLOR_WIDTH      = 128;
            const float SEPARATOR_WIDTH  = 8;
            float       currentViewWidth = EditorGUIUtility.currentViewWidth;
            
            //background
            Color backgroundColor = ((ISelectionGroup) group == activeSelectionGroup) ? Color.white * 0.6f : Color.white * 0.3f;
            if (isPaint) 
            {
                rect.width = currentViewWidth - RightMargin - COLOR_WIDTH;                
                EditorGUI.DrawRect(rect, backgroundColor);
            } 
            
            //foldout and label
            float labelWidth = currentViewWidth
                             - (COLOR_WIDTH + FOLDOUT_WIDTH + RightMargin + SEPARATOR_WIDTH);
            {
                rect.width        =  FOLDOUT_WIDTH;
                group.ShowMembers =  EditorGUI.Toggle(rect, group.ShowMembers, EditorStyles.foldout);
                rect.x            += FOLDOUT_WIDTH;
                rect.width        =  labelWidth;
            }
            if(isAvailableInEditMode)
                HandleHeaderMouseEvents(rect, group);
            if (isPaint) 
            {
                Label.normal.textColor = EditorGUIUtility.isProSkin ? ProTextColor: Color.black;
                GUI.Label(rect, content, Label);
            }

            rect.x += rect.width;
            
            if (group.EnabledTools.Count > 0)
                DrawTools(rect.x, rect.y, group);
            
            rect.x     += SEPARATOR_WIDTH;
            rect.width =  COLOR_WIDTH;

            if(isPaint) EditorGUI.DrawRect(rect, new Color(group.Color.r, group.Color.g, group.Color.b));

            showChildren =  isAvailableInEditMode ? group.ShowMembers : false;
            rect.x = cursor.x;
            rect.y += rect.height;
            rect.width = cursor.width;
            return rect;
        }

        
        void DrawTools(float rightAlignedX, float y, ISelectionGroup group)
        {
            Assert.Greater(group.EnabledTools.Count,0);
            const int TOOL_X_DIFF     = 18;
            const int TOOL_HEIGHT     = 18;
            int numEnabledTools = group.EnabledTools.Count;

            Rect rect = new Rect(0, y, TOOL_X_DIFF, TOOL_HEIGHT); 
            int i    = 0;
            
            foreach (string toolId in group.EnabledTools) {
                SelectionGroupToolAttribute attr = null;
                MethodInfo methodInfo  = null;
                
                bool found = SelectionGroupToolAttributeCache.TryGetAttribute(toolId, out attr);
                Assert.IsTrue(found);
                found = SelectionGroupToolAttributeCache.TryGetMethodInfo(toolId, out methodInfo);
                Assert.IsTrue(found);

                GUIContent content = EditorGUIUtility.IconContent(attr.icon);
                content.tooltip = attr.description;
                
                rect.x = rightAlignedX - ((numEnabledTools - i) * TOOL_X_DIFF);
                if (GUI.Button(rect, content, miniButtonStyle))
                {
                    try
                    {
                        methodInfo.Invoke(null, new object[] { group });
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }                
                
                ++i;
            }
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
            if(!string.IsNullOrEmpty(group.Query))
                menu.AddItem(new GUIContent("Update Query Results"), false, () => SelectionGroupManager.ExecuteQuery(group));
            else
                menu.AddDisabledItem(new GUIContent("Update Query Results"), false);
            
            //[TODO-sin:2021-12-20] Remove in version 0.7.0             
            // if (group.Scope == SelectionGroupDataLocation.Editor)
            // {
            //     menu.AddItem(new GUIContent("Move to Scene"), false, () =>
            //     {
            //         SelectionGroupManager.ChangeGroupScope(group, SelectionGroupDataLocation.Scene);
            //     });
            // }
            // else
            // {
            //     menu.AddItem(new GUIContent("Move to Editor"), false, () =>
            //     {
            //         SelectionGroupManager.ChangeGroupScope(group, SelectionGroupDataLocation.Editor);
            //     });
            // }

            menu.AddItem(new GUIContent("Delete Group"), false, () =>
            {
                SelectionGroupManager.GetOrCreateInstance().DeleteSceneSelectionGroup(group);
            });
            menu.DropDown(rect);
        }

//----------------------------------------------------------------------------------------------------------------------        
        
        void HandleHeaderMouseEvents(Rect rect, SelectionGroup group)
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
                                ShowGroupContextMenu(rect, group.Name, group);
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
                        DragAndDrop.objectReferences = new[] { group.gameObject };
                        DragAndDrop.SetGenericData(DRAG_ITEM_TYPE,DragItemType.GROUP);                        
                        DragAndDrop.StartDrag(group.Name);
                        e.Use();
                        break;
                }
            }
        }

        
        void HandleGroupMemberMouseEvents(Rect rect)
        {
            Event e = Event.current;
            if (!rect.Contains(e.mousePosition)) 
                return;
            
            switch (e.type) {
                case EventType.MouseDrag:
                    //Convert the dragged objects
                    HashSet<Object> uniqueDraggedObjects = new HashSet<Object>();
                    foreach (KeyValuePair<ISelectionGroup, HashSet<Object>> members in m_selectedGroupMembers) {
                        uniqueDraggedObjects.UnionWith(members.Value);
                    }
                    int numDraggedObjects  = uniqueDraggedObjects.Count;
                    if (numDraggedObjects <= 0)
                        break;

                    Object[] objects = uniqueDraggedObjects.ToArray();                    
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = objects;
                    DragAndDrop.SetGenericData(DRAG_ITEM_TYPE,DragItemType.GROUP_MEMBERS);
                    string dragText = numDraggedObjects > 1 ? objects[0].name + " ..." : objects[0].name;                        
                    DragAndDrop.StartDrag(dragText);
                    e.Use();
                    break;
            }
        }
        
        bool HandleDragEvents(Rect rect, ISelectionGroup group)
        {
            Event evt = Event.current;
            if (!rect.Contains(evt.mousePosition))
            {
                return false;
            }                

            switch (evt.type)
            {
                case EventType.MouseDrag:
                    //This event occurs when dragging inside the EditorWindow which contains this OnGUI method.
                    //It would be better named DragStarted.
                    // Debug.Log($"Start Drag: {group.Name}");
                    DragAndDrop.PrepareStartDrag();
                    if (hotMember != null)
                        DragAndDrop.objectReferences = new[] { hotMember };
                    else
                        DragAndDrop.objectReferences = Selection.objects;

                    DragAndDrop.StartDrag("Selection Group");
                    evt.Use();
                    break;
                case EventType.DragExited:
                    //This event occurs when MouseUp occurs, or the cursor leaves the EditorWindow.
                    ////The cursor may come back into the EditorWindow, however MouseDrag will not be triggered.
                    break;
                case EventType.DragUpdated:
                    //This event can occur ay any time. VisualMode must be assigned a value other than Rejected, else
                    //the DragPerform event will not be triggered.
                    DragItemType? dragItemType = DragAndDrop.GetGenericData(DRAG_ITEM_TYPE) as DragItemType?;

                    bool targetGroupContainsQuery = string.IsNullOrEmpty(group.Query);
                    bool draggedItemIsGroup       = (null != dragItemType && dragItemType == DragItemType.GROUP);

                    if (!targetGroupContainsQuery || draggedItemIsGroup) 
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    else
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    evt.Use();
                    break;
                case EventType.DragPerform:
                    //This will only get called when a valid Drop occurs (determined by the above DragUpdated code)
                    DragAndDrop.AcceptDrag();
                    RegisterUndo(group, "Add Members");
                    try
                    {
                        group.Add(DragAndDrop.objectReferences);
                    }
                    catch (SelectionGroupException e)
                    {
                        ShowNotification(new GUIContent(e.Message));
                    }
                    evt.Use();

                    break;
            }
            return false;
        }

        bool IsGroupMemberSelected(ISelectionGroup group, Object member) {
            if (!m_selectedGroupMembers.ContainsKey(group))
                return false;

            return (!m_selectedGroupMembers[group].Contains(member));
        }
        
//----------------------------------------------------------------------------------------------------------------------        

        readonly Dictionary<ISelectionGroup, HashSet<Object>> m_selectedGroupMembers 
            = new Dictionary<ISelectionGroup, HashSet<Object>>();
        
        private const string DRAG_ITEM_TYPE = "SelectionGroupsWindows";
    }
} //end namespace
