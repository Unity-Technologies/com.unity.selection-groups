using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
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

        internal static void TryRepaint() {
            if (!EditorWindow.HasOpenInstances<SelectionGroupEditorWindow>())
                return;
            
            SelectionGroupEditorWindow window = EditorWindow.GetWindow<SelectionGroupEditorWindow>(
                utility:false, title:"", focus:false);
            window.Repaint();
        }
        

        static float CalculateHeight(IList<SelectionGroup> groups)
        {
            var height = EditorGUIUtility.singleLineHeight;
            for (var i=0; i<groups.Count; i++)
            {
                var group = groups[i];
                if (null == group)
                    continue;
                
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
            m_groupsToDraw = SelectionGroupManager.GetOrCreateInstance().Groups;
            
            var viewRect = Rect.zero;
            viewRect.width = position.width-16;
            viewRect.height = CalculateHeight(m_groupsToDraw);
            var windowRect = new Rect(0, 0, position.width, position.height);
            scroll = GUI.BeginScrollView(windowRect, scroll, viewRect);
            
            Rect cursor = new Rect(0, 0, position.width-RightMargin, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(cursor, AddGroup)) CreateNewGroup();
            cursor.y += cursor.height;

            for (var i=0; i<m_groupsToDraw.Count; i++)
            {
                var group = m_groupsToDraw[i];
                if (group == null) continue;
                cursor.y += GROUP_HEADER_PADDING; 
                
                //early out if this group yMin is below window rect (not visible).
                if ((cursor.yMin - scroll.y) > position.height) break;
                var dropRect = cursor;
                
                cursor = DrawHeader(cursor, i, out bool showChildren);

                if (showChildren)
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

        Rect DrawAllGroupMembers(Rect rect, ISelectionGroup group)
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
                    DrawGroupMember(rect, group, i);
                rect.y += rect.height;
            }
            return rect;
        }

        void DrawGroupMember(Rect rect, ISelectionGroup group, UnityEngine.Object g) 
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

            if (g.hideFlags.HasFlag(HideFlags.NotEditable)) {
                GUIContent icon  = InspectorLock;
                Rect irect = rect;
                irect.width  = 16;
                irect.height = 14;
                GUI.DrawTexture(irect, icon.image);
            }

            rect.x           += 24;
            bool allowRemove = !group.IsAutoFilled();
            GUI.contentColor =  allowRemove ? Color.white : Color.Lerp(Color.white, Color.yellow, 0.25f);
            GUI.Label(rect, content);
            GUI.contentColor = Color.white;
            
            HandleGroupMemberMouseEvents(rect, group, g, isGroupMemberSelected);            
        }
        
        Rect DrawHeader(Rect cursor, int groupIndex, out bool showChildren) {
            SelectionGroup group                 = m_groupsToDraw[groupIndex];
            bool           isPaint               = Event.current.type == EventType.Repaint;
            Rect           rect                  = new Rect(cursor) {x = 0, };
            bool           isAvailableInEditMode = true;
            GUIContent     content               = sceneHeaderContent;
            
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
            const float COLOR_WIDTH      = 32;
            const float SEPARATOR_WIDTH  = 8;
            float       currentViewWidth = EditorGUIUtility.currentViewWidth;
            
            //background
            Color backgroundColor = ((ISelectionGroup) group == m_activeSelectionGroup) ? Color.white * 0.6f : Color.white * 0.3f;
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
            
            //Draw tools first before handling mouse events
            float toolRightAlignedX = rect.x + rect.width;
            DrawTools(toolRightAlignedX, rect.y, group);
            
            if(isAvailableInEditMode)
                HandleHeaderMouseEvents(rect, groupIndex);
            if (isPaint) 
            {
                Label.normal.textColor = EditorGUIUtility.isProSkin ? ProTextColor: Color.black;
                GUI.Label(rect, content, Label);
            }
            
            rect.x     = toolRightAlignedX + SEPARATOR_WIDTH;
            rect.width = COLOR_WIDTH;

            if(isPaint) EditorGUI.DrawRect(rect, new Color(group.Color.r, group.Color.g, group.Color.b));

            showChildren =  isAvailableInEditMode ? group.ShowMembers : false;
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
            
            for (int toolId = (int)SelectionGroupToolType.MAX-1; toolId >=0; --toolId) {
                bool toolStatus = group.GetEditorToolStatus(toolId);
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

        void ShowGroupMemberContextMenu(Rect rect, ISelectionGroup clickedGroup)
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

        void ShowGroupContextMenu(Rect rect, string groupName, ISelectionGroup group)
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
                    m_selectedGroupMembers.RemoveGroupFromSelection(group);
                    group.Clear();
                    UpdateUnityEditorSelectionWithMembers();
                });
            }
            
            
            //[TODO-sin:2022-01-06] Remove in version 0.7.0 
            //menu.AddItem(new GUIContent("Configure Group"), false, () => SelectionGroupConfigurationDialog.Open(group, this));
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

                        DragDropPos dropPos = DragDropPos.ABOVE;
                        float halfHeight = rect.height * 0.5f;
                        if (evt.mousePosition.y - rect.y > halfHeight) {
                            dropRect.y += rect.height + GROUP_HEADER_PADDING;
                            dropPos    =  DragDropPos.BELOW;
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
                            if (evt.clickCount == 1) {                                
                                SetUnityEditorSelection(group);
                                m_selectedGroupMembers.Clear();
                            }
                            else {
                                SelectAllGroupMembers(group);
                                
                                //[TODO-sin:2022-01-06] Remove in version 0.7.0 
                                //SelectionGroupConfigurationDialog.Open(@group, this);
                            }
                            break;
                    }
                    evt.Use();
                    break;
                case EventType.MouseDrag:
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new[] { @group.gameObject };
                    DragAndDrop.SetGenericData(DRAG_ITEM_TYPE,DragItemType.GROUP);
                    DragAndDrop.SetGenericData(DRAG_GROUP_INDEX,groupIndex);
                    DragAndDrop.StartDrag(@group.Name);
                    evt.Use();
                    break;
                case EventType.DragUpdated: {
                    //This event can occur ay any time. VisualMode must be assigned a value other than Rejected, else
                    //the DragPerform event will not be triggered.
                    DragItemType? dragItemType = DragAndDrop.GetGenericData(DRAG_ITEM_TYPE) as DragItemType?;

                    bool targetGroupIsAuto  = @group.IsAutoFilled();
                    bool draggedItemIsGroup = (dragItemType == DragItemType.GROUP);

                    if (draggedItemIsGroup) {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    }
                    else if (targetGroupIsAuto) { 
                        //copying/moving members to auto group. Invalid 
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    } else {
                        //copying/moving members to normal  group. Valid 
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    }
                    
                    evt.Use();
                    break;
                }
                case EventType.DragPerform: {
                    //DragPerform will only arrive when a valid Drop occurs (determined by the above DragUpdated code)
                    DragAndDrop.AcceptDrag();

                    DragItemType? dragItemType = DragAndDrop.GetGenericData(DRAG_ITEM_TYPE) as DragItemType?;
                    if (!dragItemType.HasValue) {
                        evt.Use();
                        break;
                    }

                    try {
                        switch (dragItemType.Value) {
                            case DragItemType.GROUP_MEMBERS: {
                                RegisterUndo(@group, "Add Members");
                                @group.Add(DragAndDrop.objectReferences);
                                break;
                            }
                            case DragItemType.GROUP: {

                                Object[] draggedObjects = DragAndDrop.objectReferences;
                                if (null == draggedObjects || draggedObjects.Length <= 0 || null == draggedObjects[0])
                                    break;

                                int?         dragGroupIndex = DragAndDrop.GetGenericData(DRAG_GROUP_INDEX) as int?;
                                DragDropPos? dropPos        = DragAndDrop.GetGenericData(DRAG_DROP_POS) as DragDropPos?;
                                if (!dragGroupIndex.HasValue || !dropPos.HasValue) {
                                    break;
                                }

                                if (dragGroupIndex == groupIndex
                                    || dropPos == DragDropPos.ABOVE && dragGroupIndex == groupIndex - 1
                                    || dropPos == DragDropPos.BELOW && dragGroupIndex == groupIndex + 1
                                   ) {
                                    break;
                                }

                                SelectionGroupManager.GetOrCreateInstance().MoveGroup(dragGroupIndex.Value, groupIndex);
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

        
        void HandleGroupMemberMouseEvents(Rect rect, ISelectionGroup group, Object groupMember, bool isGroupMemberSelected)
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
                                m_selectedGroupMembers.AddObjectToSelection(group, groupMember);
                            }
                            else {
                                if (!isGroupMemberSelected) {
                                    m_selectedGroupMembers.AddObjectToSelection(group, groupMember);
                                } else {
                                    m_selectedGroupMembers.Remove(group, groupMember);
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
                    //Prepare the selected objects to be dragged: Convert to array
                    HashSet<Object> uniqueDraggedObjects = new HashSet<Object>();
                    foreach (KeyValuePair<ISelectionGroup, OrderedSet<Object>> members in m_selectedGroupMembers) {
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
                    evt.Use();
                    break;
            }
        }
        
        //Find object between (pivotGroup, pivotGroupMember) and (endGroup,endMember).
        //The order between them is not guaranteed.
        [CanBeNull]
        static GroupMembersSelection SelectMembersInBetween(
            ISelectionGroup pivotGroup, Object pivotMember, 
            ISelectionGroup endGroup, Object endMember, IList<SelectionGroup> allGroups) 
        {
            if (allGroups.Count == 0)
                return null;

            GroupMembersSelection ret = new GroupMembersSelection();
            
            SelectionGroup pivotSG = pivotGroup as SelectionGroup;
            SelectionGroup endSG = endGroup as SelectionGroup;
            
            bool startAdd = (null == pivotSG);

            foreach (SelectionGroup group in allGroups) {
                foreach (Object m in group.Members) {

                    bool shouldToggleState = (group == pivotSG && m == pivotMember)
                        || (group == endSG && m == endMember);
                    
                    if (startAdd) {
                        
                        ret.AddObjectToSelection(group,m);
                        if (shouldToggleState)
                            return ret;
                    } else {
                        if (!shouldToggleState) 
                            continue;
                        
                        startAdd = true;
                        ret.AddObjectToSelection(@group,m);

                    }
                }
                
            }

            return ret;
        }

//----------------------------------------------------------------------------------------------------------------------        

        private void DeleteGroup(ISelectionGroup group) {
            m_selectedGroupMembers.RemoveGroupFromSelection(group);
            SelectionGroupManager.GetOrCreateInstance().DeleteSceneSelectionGroup(group);
            UpdateUnityEditorSelectionWithMembers();
            
        }

        private void RemoveSelectedMembersFromGroup() {
            foreach (KeyValuePair<ISelectionGroup, OrderedSet<Object>> kv in m_selectedGroupMembers) {
                ISelectionGroup group = kv.Key;
                RegisterUndo(group, "Remove Member");
                group.Remove(kv.Value);
            }
        }

        private void ClearSelectedMembers() {
            m_selectedGroupMembers.Clear();
            UpdateUnityEditorSelectionWithMembers();
        }

        private void SelectAllGroupMembers(ISelectionGroup group) {
            m_selectedGroupMembers.AddGroupMembersToSelection(group);
            UpdateUnityEditorSelectionWithMembers();
        }
        
        
//----------------------------------------------------------------------------------------------------------------------        
        
        private void SetUnityEditorSelection(SelectionGroup group) {
            m_activeSelectionGroup  = @group;
            Selection.objects       = new Object[] { null == group ? null : group.gameObject };
        }

        private void UpdateUnityEditorSelectionWithMembers() {
            Selection.objects = m_selectedGroupMembers.ConvertMembersToArray();
        }
        

//----------------------------------------------------------------------------------------------------------------------        

        readonly GroupMembersSelection m_selectedGroupMembers = new GroupMembersSelection();

        private IList<SelectionGroup> m_groupsToDraw = null;
        private Object m_hoveredGroupMember = null;
        
        
        private const string DRAG_ITEM_TYPE   = "SelectionGroupsDragItemType";
        private const string DRAG_DROP_POS    = "SelectionGroupsDragDropPos";
        private const string DRAG_GROUP_INDEX = "SelectionGroupsDragGroup";
        
        private const   int   GROUP_HEADER_PADDING = 3;
        static readonly Color HOVER_COLOR          = new Color32(112, 112, 112, 128);

        private ISelectionGroup m_shiftPivotGroup       = null;
        private Object         m_shiftPivotGroupMember = null;

    }
} //end namespace
