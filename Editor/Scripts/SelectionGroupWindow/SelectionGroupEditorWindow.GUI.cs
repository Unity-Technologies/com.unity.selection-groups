using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using NUnit.Framework;
using Unity.FilmInternalUtilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
    

namespace Unity.SelectionGroups.Editor
{

    internal partial class SelectionGroupEditorWindow : EditorWindow
    {
        private const string AddGroup    = "Add Group";
        private const int    RightMargin = 16;
        
        private GUIStyle   Label;
        private GUIContent sceneHeaderContent;
        private GUIContent m_CreateDropdownContent;

        private static readonly Color ProTextColor = new Color(0.824f, 0.824f, 0.824f, 1f);
        

        [MenuItem("Window/General/Selection Groups")]
        static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<SelectionGroupEditorWindow>();
            window.Show();
        }

        internal static void TryRepaint() {
            if (!EditorWindow.HasOpenInstances<SelectionGroupEditorWindow>())
                return;
            
            SelectionGroupEditorWindow window = EditorWindow.GetWindow<SelectionGroupEditorWindow>(
                utility:false, title:"", focus:false);
            window.Repaint();
        }
        

        static float CalculateHeight(IList<SelectionGroup> groups)
        {
            float height = EditorGUIUtility.singleLineHeight;
            groups.Loop((SelectionGroup group) => {
                if (null == @group)
                    return;
                
                height += EditorGUIUtility.singleLineHeight + 3;
                if (@group.AreMembersShownInWindow())
                {
                    height += @group.Count * EditorGUIUtility.singleLineHeight;
                }                
            });
            return height;
        }

        void DrawGUI()
        {
            m_groupsToDraw = SelectionGroupManager.GetOrCreateInstance().Groups;

            Rect toolbarRect = new Rect()
            {
                width = position.width,
                height = EditorGUIUtility.singleLineHeight
            };
            DrawToolbar(toolbarRect);
            
            var viewRect = Rect.zero;
            viewRect.y = toolbarRect.yMax + 2;
            viewRect.width = position.width-16;
            viewRect.height = CalculateHeight(m_groupsToDraw);
            var windowRect = new Rect(0, toolbarRect.yMax + 2, position.width, position.height - toolbarRect.height - 2);
            scroll = GUI.BeginScrollView(windowRect, scroll, viewRect);
            
            Rect cursor = new Rect(0, toolbarRect.yMax + 2, position.width-RightMargin, EditorGUIUtility.singleLineHeight);

            for (var i=0; i<m_groupsToDraw.Count; i++)
            {
                var group = m_groupsToDraw[i];
                if (group == null) continue;
                cursor.y += GROUP_HEADER_PADDING; 
                
                //early out if this group yMin is below window rect (not visible).
                if ((cursor.yMin - scroll.y) > position.height) break;
                
                cursor = DrawHeader(cursor, i);
                if (m_groupsToDraw[i].AreMembersShownInWindow())
                {
                    // dropRect.yMax = rect.yMax;
                    //early out if this group yMax is above window rect (not visible).
                    // if (rect.yMax - scroll.y < 0)
                        // continue;
                    cursor = DrawAllGroupMembers(cursor, group);
                }
                
                group.SetOnDestroyedInEditorCallback(TryRepaint);
            }
            //Handle clicks on blank areas of window.
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                ClearSelectedMembers();
                SetUnityEditorSelection(null);
                Event.current.Use();
            }
            GUI.EndScrollView();

        }

        void DrawToolbar(Rect rect)
        {
            if (Event.current.type == EventType.Repaint)
                EditorStyles.toolbar.Draw(rect, false, false, false, false);

            rect.width = 35;
            if (EditorGUI.DropdownButton(rect, m_CreateDropdownContent, FocusType.Passive, EditorStyles.toolbarDropDown))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Create Empty Group"), false, CreateNewGroup);
                if (Selection.gameObjects.Length > 0)
                {
                    menu.AddItem(new GUIContent("Create Group from Selection"), false, () => CreateNewGroup(Selection.gameObjects));
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Create Group from Selection"));
                }
                menu.DropDown(rect);
            }
        }

        void SetupStyles()
        {
            if (miniButtonStyle == null)
            {
                miniButtonStyle = EditorStyles.miniButton;
                miniButtonStyle.padding = new RectOffset(0, 0, 0, 0); 
                Label = "label";
            }

            if (null == m_inspectorLockTex) {
                m_inspectorLockTex = (Texture2D)EditorGUIUtility.Load("IN LockButton on act@2x");
            }

            if (null == m_hiddenInSceneTex) {
                m_hiddenInSceneTex = (Texture2D)EditorGUIUtility.Load("d_scenevis_hidden_hover@2x");
            }
            
        }

        Rect DrawAllGroupMembers(Rect rect, SelectionGroup group)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            int numMembers = group.Members.Count;
            for (int i = 0; i < numMembers; ++i) {
                GameObject m = group.Members[i];
                if (m == null)
                    continue;
                
                //if rect is below window, early out.
                if (rect.yMin - scroll.y > position.height) return rect;
                //if rect is in window, draw.
                if (rect.yMax - scroll.y > 0)
                    DrawGroupMember(rect, group, m);
                rect.y += rect.height;
            }
            return rect;
        }

        void DrawGroupMember(Rect rect, SelectionGroup group, GameObject g) 
        {
            Assert.IsNotNull(g);
            Event e = Event.current;
            GUIContent content = EditorGUIUtility.ObjectContent(g, g.GetType());
            bool isMouseOver = rect.Contains(e.mousePosition);

            bool isGroupMemberSelected = m_selectedGroupMembers.Contains(group, g);
            if (isGroupMemberSelected)
                EditorGUI.DrawRect(rect, SELECTION_COLOR);
            
            if (isMouseOver) {
                EditorGUI.DrawRect(rect, HOVER_COLOR);
                if (m_hoveredGroupMember != g) {
                    m_hoveredGroupMember = g;
                    Repaint();
                }
            }

            if (SceneVisibilityManager.instance.IsHidden(g)) {
                DrawIconTexture(0, rect.y, m_hiddenInSceneTex);
            }
            
            if (g.hideFlags.HasFlag(HideFlags.NotEditable)) {
                DrawIconTexture(16, rect.y, m_inspectorLockTex);
            }

            rect.x           += 32;
            bool allowRemove = !group.IsAutoFilled();
            GUI.contentColor =  allowRemove ? Color.white : Color.Lerp(Color.white, Color.yellow, 0.25f);
            GUI.Label(rect, content);
            GUI.contentColor = Color.white;
            
            HandleGroupMemberMouseEvents(rect, group, g, isGroupMemberSelected);            
        }
        
        Rect DrawHeader(Rect cursor, int groupIndex) {
            SelectionGroup group   = m_groupsToDraw[groupIndex];
            bool           isPaint = Event.current.type == EventType.Repaint;
            Rect           rect    = new Rect(cursor) {x = 0, };
            GUIContent     content = sceneHeaderContent;
            
            content.text = $"{group.Name}";

            //
            const float FOLDOUT_WIDTH    = 16;
            const float COLOR_WIDTH      = 8;
            const float SEPARATOR_WIDTH  = 8;
            float       currentViewWidth = EditorGUIUtility.currentViewWidth;
            
            //background
            Color backgroundColor = ((SelectionGroup) group == m_activeSelectionGroup) ? Color.white * 0.6f : Color.white * 0.3f;
            if (isPaint) 
            {
                rect.width = currentViewWidth - RightMargin - COLOR_WIDTH;                
                EditorGUI.DrawRect(rect, backgroundColor);
            } 
            
            //foldout and label
            float labelWidth = currentViewWidth
                             - (COLOR_WIDTH + FOLDOUT_WIDTH + RightMargin + SEPARATOR_WIDTH);
            {
                rect.width =  FOLDOUT_WIDTH;
                group.ShowMembersInWindow(EditorGUI.Toggle(rect, group.AreMembersShownInWindow(), EditorStyles.foldout));
                
                rect.x     += FOLDOUT_WIDTH;
                rect.width =  labelWidth;
                
            }
            
            //Draw tools first before handling mouse events
            float toolRightAlignedX = rect.x + rect.width;
            DrawTools(toolRightAlignedX, rect.y, group);
            
            HandleHeaderMouseEvents(rect, groupIndex);
            
            if (isPaint) 
            {
                Label.normal.textColor = EditorGUIUtility.isProSkin ? ProTextColor: Color.black;
                GUI.Label(rect, content, Label);
            }
            
            rect.x     = toolRightAlignedX + SEPARATOR_WIDTH;
            rect.width = COLOR_WIDTH;

            EditorGUI.DrawRect(rect, group.Color);
            rect.x = cursor.x;
            rect.y += rect.height;
            rect.width = cursor.width;
            return rect;
        }

        
        void DrawTools(float rightAlignedX, float y, SelectionGroup group)
        {
            const int TOOL_X_DIFF     = 18;
            const int TOOL_HEIGHT     = 18;

            Rect rect = new Rect(0, y, TOOL_X_DIFF, TOOL_HEIGHT); 
            int enabledToolCounter = 0;
            
            for (int toolId = (int)SelectionGroupToolType.BuiltIn_Max-1; toolId >=0; --toolId) {
                bool toolStatus = group.GetEditorToolState(toolId);
                if (false == toolStatus)
                    continue;
            
                
                SelectionGroupToolAttribute attr = null;
                MethodInfo methodInfo  = null;
                
                bool found = SelectionGroupToolAttributeCache.TryGetAttribute(toolId, out attr);
                Assert.IsTrue(found);
                found = SelectionGroupToolAttributeCache.TryGetMethodInfo(toolId, out methodInfo);
                Assert.IsTrue(found);

                GUIContent content = EditorGUIUtility.IconContent(attr.icon);
                content.tooltip = attr.description;
                
                rect.x = rightAlignedX - ((enabledToolCounter+1) * TOOL_X_DIFF);
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
                
                ++enabledToolCounter;
            }
        }

        static void DrawIconTexture(float iconX, float iconY, Texture2D tex) {
            Rect rect = new Rect() {
                x = iconX,
                y = iconY,
                width  = 16,
                height = 14,
            };
            GUI.DrawTexture(rect,tex);
        }
        

        void ShowGroupMemberContextMenu(Rect rect, SelectionGroup clickedGroup)
        {
            var menu = new GenericMenu();
            var content = new GUIContent("Remove From Group");
            if (clickedGroup.IsAutoFilled()) {
                menu.AddDisabledItem(content,false);
            } else {
                menu.AddItem(content, false, () => {
                    RemoveSelectedMembersFromGroup();
                });
            }
            
            menu.DropDown(rect);
        }

        void ShowGroupContextMenu(Rect rect, string groupName, SelectionGroup group)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Select All Group Members"), false, () => {
                SelectAllGroupMembers(group);
            });
            menu.AddSeparator(string.Empty);
            if (group.IsAutoFilled()) {
                menu.AddDisabledItem(new GUIContent("Clear Group"), false);
            } else {
                menu.AddItem(new GUIContent("Clear Group"), false, () => {
                    m_selectedGroupMembers.RemoveGroup(group);
                    group.Clear();
                    UpdateUnityEditorSelectionWithMembers();
                });
            }

            menu.AddItem(new GUIContent("Delete Group"), false, () => {
                DeleteGroup(group);
            });
            menu.DropDown(rect);
        }

//----------------------------------------------------------------------------------------------------------------------        
        
        void HandleHeaderMouseEvents(Rect rect, int groupIndex)
        {
            Event evt = Event.current;
            if (!rect.Contains(evt.mousePosition)) 
                return;

            SelectionGroup group = m_groupsToDraw[groupIndex];
            
            switch (evt.type)
            {
                case EventType.Repaint: {
                    if (DragAndDrop.visualMode == DragAndDropVisualMode.Move) {
                        //Show lines to indicate where to drop the dragged group
                        Rect dropRect = rect;
                        dropRect.height = 2;

                        DragDropPos dropPos = DragDropPos.Above;
                        float halfHeight = rect.height * 0.5f;
                        if (evt.mousePosition.y - rect.y > halfHeight) {
                            dropRect.y += rect.height + GROUP_HEADER_PADDING;
                            dropPos    =  DragDropPos.Below;
                        }  
                        DragAndDrop.SetGenericData(DRAG_DROP_POS,dropPos);
                        
                        EditorGUI.DrawRect(dropRect, new Color(0.2f, 0.35f, 0.85f, 1f));
                    }
                    break;
                }
                case EventType.MouseDown:
                    switch (evt.button)
                    {
                        case RIGHT_MOUSE_BUTTON:
                            ShowGroupContextMenu(rect, @group.Name, @group);
                            break;
                        case LEFT_MOUSE_BUTTON:
                            m_leftMouseWasDoubleClicked = evt.clickCount > 1;
                            if (m_leftMouseWasDoubleClicked) {
                                SelectAllGroupMembers(group);
                            }
                            break;
                    }
                    evt.Use();
                    break;
                case EventType.MouseUp:
                    switch (evt.button) {
                        case LEFT_MOUSE_BUTTON:
                            if (!m_leftMouseWasDoubleClicked) {
                                SetUnityEditorSelection(group);
                                m_selectedGroupMembers.Clear();
                            }
                            break;
                    }
                    evt.Use();
                    break;
                
                case EventType.MouseDrag:
                    if ((SelectionGroup) m_activeSelectionGroup != group)
                        break;
                    
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new[] { @group.gameObject };
                    DragAndDrop.SetGenericData(DRAG_ITEM_TYPE,DragItemType.Group);
                    DragAndDrop.SetGenericData(DRAG_GROUP_INDEX,groupIndex);
                    DragAndDrop.StartDrag(@group.Name);
                    evt.Use();
                    break;
                case EventType.DragUpdated: {
                    //This event can occur ay any time. VisualMode must be assigned a value other than Rejected, else
                    //the DragPerform event will not be triggered.
                    DragItemType? dragItemType = DragAndDrop.GetGenericData(DRAG_ITEM_TYPE) as DragItemType?;

                    bool targetGroupIsAuto  = @group.IsAutoFilled();
                    bool draggedItemIsGroup = (dragItemType == DragItemType.Group);

                    if (draggedItemIsGroup) {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    }
                    else if (targetGroupIsAuto) { 
                        //copying/moving members to auto group. Invalid 
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    } else {
                        //moving window members to group.  
                        bool isMovingWindowMembers = (dragItemType == DragItemType.WindowGroupMembers && evt.control);
                        DragAndDrop.visualMode = isMovingWindowMembers
                            ? DragAndDropVisualMode.Move
                            : DragAndDropVisualMode.Copy; 
                        
                    }
                    
                    evt.Use();
                    break;
                }
                case EventType.DragPerform: {
                    //DragPerform will only arrive when a valid Drop occurs (determined by the above DragUpdated code)
                    DragAndDrop.AcceptDrag();

                    DragItemType? dragItemType = DragAndDrop.GetGenericData(DRAG_ITEM_TYPE) as DragItemType?;
                    if (!dragItemType.HasValue) {
                        dragItemType = DragItemType.GameObjects; //receive gameObjects from outside the window
                    } 
                    
                    try {
                        switch (dragItemType.Value) {
                            case DragItemType.WindowGroupMembers: {
                                if (evt.control) {
                                    m_selectedGroupMembers = SelectionGroupUtility.MoveMembersSelectionToGroup(
                                        m_selectedGroupMembers, group
                                    );
                                } else {
                                    RegisterUndo(@group, "Add Members");
                                    HashSet<GameObject> members = m_selectedGroupMembers.ConvertMembersToSet();
                                    @group.AddRange(members);
                                }
                                
                                break;
                            } 
                            case DragItemType.GameObjects: {
                                RegisterUndo(@group, "Add Members");
                                DragAndDrop.objectReferences.Loop((Object obj) => {
                                    if (!(obj is GameObject go))
                                        return;
                                    
                                    @group.Add(go);
                                });
                                break;
                            }
                            case DragItemType.Group: {

                                Object[] draggedObjects = DragAndDrop.objectReferences;
                                if (null == draggedObjects || draggedObjects.Length <= 0 || null == draggedObjects[0])
                                    break;

                                int?         dragGroupIndex = DragAndDrop.GetGenericData(DRAG_GROUP_INDEX) as int?;
                                DragDropPos? dropPos        = DragAndDrop.GetGenericData(DRAG_DROP_POS) as DragDropPos?;
                                if (!dragGroupIndex.HasValue || !dropPos.HasValue) {
                                    break;
                                }

                                int srcIndex = dragGroupIndex.Value;

                                if (dragGroupIndex == groupIndex
                                    || DragDropPos.Above == dropPos && srcIndex == groupIndex - 1
                                    || DragDropPos.Below == dropPos && srcIndex == groupIndex + 1
                                   ) {
                                    break;
                                }

                                //Calculate the target new index for the group correctly
                                int targetIndex = groupIndex;
                                if (DragDropPos.Below == dropPos && targetIndex < srcIndex) {
                                    ++targetIndex;
                                }

                                if (DragDropPos.Above == dropPos && targetIndex > srcIndex) {
                                    --targetIndex;
                                }

                                SelectionGroupManager.GetOrCreateInstance().MoveGroup(srcIndex, targetIndex);
                                Repaint();
                                break;
                            }
                        }
                    } catch (SelectionGroupException e) {
                        ShowNotification(new GUIContent(e.Message));
                    }

                    evt.Use();
                    break;
                }
            }
        }

        
        void HandleGroupMemberMouseEvents(Rect rect, SelectionGroup group, GameObject groupMember, bool isGroupMemberSelected)
        {
            Event evt = Event.current;
            if (!rect.Contains(evt.mousePosition)) 
                return;

            bool isControl         = evt.control;
            bool isShift           = evt.shift;
            bool isRightMouseClick = evt.button == RIGHT_MOUSE_BUTTON;
            bool isLeftMouseClick  = evt.button == LEFT_MOUSE_BUTTON;

            switch (evt.type) {
                case EventType.MouseDown: {
                    if (isLeftMouseClick && !isShift) {
                        m_shiftPivotGroup       = group;
                        m_shiftPivotGroupMember = groupMember;
                    }
                    
                    if (isRightMouseClick && isGroupMemberSelected) {
                        ShowGroupMemberContextMenu(rect, group);
                        evt.Use();
                    }

                    SetUnityEditorSelection(null);
                    
                    evt.Use();
                    break;
                }
                case EventType.MouseUp: {
                    if (isLeftMouseClick) {
                        if (!isShift) {
                            if (!isControl) {
                                m_selectedGroupMembers.Clear();
                                m_selectedGroupMembers.AddObject(group, groupMember);
                            }
                            else {
                                if (!isGroupMemberSelected) {
                                    m_selectedGroupMembers.AddObject(group, groupMember);
                                } else {
                                    m_selectedGroupMembers.RemoveObject(group, groupMember);
                                }
                            }
                        
                        } else {
                            if (!isControl) {
                                m_selectedGroupMembers.Clear();
                            }

                            //add objects from shift pivot
                            GroupMembersSelection selectedMembersByShift = SelectMembersInBetween(
                                m_shiftPivotGroup, m_shiftPivotGroupMember, 
                                group, groupMember, m_groupsToDraw);
                            m_selectedGroupMembers.Add(selectedMembersByShift);
                        } //end shift
                        UpdateUnityEditorSelectionWithMembers();
                    } //end left mouse click

                    evt.Use();

                    break;
                }

                case EventType.MouseDrag:
                    //Prepare the selected objects to be dragged:
                    Object[] objects = m_selectedGroupMembers.ConvertMembersToArray();
                    
                    int numDraggedObjects = objects.Length;
                    if (numDraggedObjects <= 0)
                        break;
                    
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = objects;
                    DragAndDrop.SetGenericData(DRAG_ITEM_TYPE,DragItemType.WindowGroupMembers);
                    string dragText = numDraggedObjects > 1 ? objects[0].name + " ..." : objects[0].name;
                    DragAndDrop.StartDrag(dragText);
                    evt.Use();
                    break;
            }
        }
        
        //Find object between (pivotGroup, pivotGroupMember) and (endGroup,endMember).
        //The order between them is not guaranteed.
        [CanBeNull]
        static GroupMembersSelection SelectMembersInBetween(
            SelectionGroup pivotSG, Object pivotMember, 
            SelectionGroup endSG, Object endMember, IList<SelectionGroup> allGroups) 
        {
            if (allGroups.Count == 0)
                return null;

            GroupMembersSelection ret = new GroupMembersSelection();
            
            bool startAdd = (null == pivotSG);

            int numGroups = allGroups.Count;
            for (int i = 0; i < numGroups; ++i) {
                SelectionGroup group      = allGroups[i];
                int            numMembers = group.Members.Count;
                for (int j = 0; j < numMembers; ++j) {
                    GameObject m = group.Members[j];
                    bool shouldToggleState = (group == pivotSG && m == pivotMember)
                        || (group == endSG && m == endMember);
                    
                    if (startAdd) {
                        
                        ret.AddObject(group,m);
                        if (shouldToggleState)
                            return ret;
                    } else {
                        if (!shouldToggleState) 
                            continue;
                        
                        startAdd = true;
                        ret.AddObject(@group,m);
                    }
                }
            }

            return ret;
        }

//----------------------------------------------------------------------------------------------------------------------        

        private void DeleteGroup(SelectionGroup group) {
            m_selectedGroupMembers.RemoveGroup(group);
            SelectionGroupManager.GetOrCreateInstance().DeleteGroup(group);
            UpdateUnityEditorSelectionWithMembers();
            
        }

        private void RemoveSelectedMembersFromGroup() {
            m_selectedGroupMembers.Loop((KeyValuePair<SelectionGroup, OrderedSet<GameObject>> kv) => {
                SelectionGroup group = kv.Key;
                RegisterUndo(group, "Remove Member");
                group.Except(kv.Value);
            });
        }

        private void ClearSelectedMembers() {
            m_selectedGroupMembers.Clear();
            UpdateUnityEditorSelectionWithMembers();
        }

        private void SelectAllGroupMembers(SelectionGroup group) {
            m_selectedGroupMembers.AddGroupMembers(group);
            UpdateUnityEditorSelectionWithMembers();
        }
        
//----------------------------------------------------------------------------------------------------------------------        
        
        private void SetUnityEditorSelection(SelectionGroup group) {
            m_activeSelectionGroup  = @group;
            Selection.objects       = new Object[] { null == group ? null : group.gameObject };
        }

        //Update Editor Selection to show the properties of group members in the inspector
        private void UpdateUnityEditorSelectionWithMembers() {
            Selection.objects = m_selectedGroupMembers.ConvertMembersToArray();
        }
        

//----------------------------------------------------------------------------------------------------------------------        

        GroupMembersSelection m_selectedGroupMembers = new GroupMembersSelection();

        private IList<SelectionGroup> m_groupsToDraw = null;
        private Object m_hoveredGroupMember = null;
        
        
        private const string DRAG_ITEM_TYPE   = "SelectionGroupsDragItemType";
        private const string DRAG_DROP_POS    = "SelectionGroupsDragDropPos";
        private const string DRAG_GROUP_INDEX = "SelectionGroupsDragGroup";
        
        private const   int   GROUP_HEADER_PADDING = 3;
        static readonly Color HOVER_COLOR          = new Color32(112, 112, 112, 128);

        private SelectionGroup m_shiftPivotGroup       = null;
        private Object          m_shiftPivotGroupMember = null;
        
        private bool m_leftMouseWasDoubleClicked = false;

        
        private Texture2D m_inspectorLockTex;
        private Texture2D m_hiddenInSceneTex;
        
        

    }
} //end namespace
