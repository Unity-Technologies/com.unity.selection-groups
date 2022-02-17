using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
    

namespace Unity.SelectionGroups.Editor
{
    internal partial class SelectionGroupEditorWindow : EditorWindow
    {
        private const string kAddGroup  = "Add Group";
        private const int kRightMargin = 16;
        private const string kDragItemType  = "SelectionGroupsDragItemType";
        private const string kDragDropPos = "SelectionGroupsDragDropPos";
        private const string kDragGroupIndex = "SelectionGroupsDragGroup";
        private const int kGroupHeaderPadding = 3;
        
        private static readonly Color s_ProTextColor = new Color(0.824f, 0.824f, 0.824f, 1f);
        private static readonly Color s_HoverColor = new Color32(112, 112, 112, 128);
        
        private GUIStyle m_Label;
        private GUIContent m_SceneHeaderContent;
        private GroupMembersSelection m_SelectedGroupMembers = new GroupMembersSelection();

        private IList<SelectionGroup> m_GroupsToDraw = null;
        private Object m_HoveredGroupMember = null;

        private SelectionGroup m_ShiftPivotGroup = null;
        private Object m_ShiftPivotGroupMember = null;
        
        private bool m_LeftMouseWasDoubleClicked = false;

        private Texture2D m_InspectorLockTex;
        private Texture2D m_HiddenInSceneTex;

        [MenuItem("Window/General/Selection Groups")]
        private static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<SelectionGroupEditorWindow>();
            window.Show();
        }

        internal static void TryRepaint() 
        {
            if (!EditorWindow.HasOpenInstances<SelectionGroupEditorWindow>())
                return;
            
            SelectionGroupEditorWindow window = EditorWindow.GetWindow<SelectionGroupEditorWindow>(
                utility:false, title:"", focus:false);
            window.Repaint();
        }
        

        private static float CalculateHeight(IList<SelectionGroup> groups)
        {
            float height = EditorGUIUtility.singleLineHeight;
            foreach (SelectionGroup @group in groups) 
            {
                if (null == @group)
                    continue;
                
                height += EditorGUIUtility.singleLineHeight + 3;
                if (@group.AreMembersShownInWindow())
                {
                    height += @group.count * EditorGUIUtility.singleLineHeight;
                }
            }
            return height;
        }

        private void DrawGUI()
        {
            m_GroupsToDraw = SelectionGroupManager.GetOrCreateInstance().groups;
            
            var viewRect = Rect.zero;
            viewRect.width = position.width-16;
            viewRect.height = CalculateHeight(m_GroupsToDraw);
            var windowRect = new Rect(0, 0, position.width, position.height);
            m_Scroll = GUI.BeginScrollView(windowRect, m_Scroll, viewRect);
            
            Rect cursor = new Rect(0, 0, position.width-kRightMargin, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(cursor, kAddGroup)) CreateNewGroup();
            cursor.y += cursor.height;

            for (var i=0; i<m_GroupsToDraw.Count; i++)
            {
                var group = m_GroupsToDraw[i];
                if (group == null) continue;
                cursor.y += kGroupHeaderPadding; 
                
                //early out if this group yMin is below window rect (not visible).
                if ((cursor.yMin - m_Scroll.y) > position.height) break;
                
                cursor = DrawHeader(cursor, i);
                if (m_GroupsToDraw[i].AreMembersShownInWindow())
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
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0) 
            {
                ClearSelectedMembers();
                SetUnityEditorSelection(null);
                Event.current.Use();
            }
            GUI.EndScrollView();

        }

        private void SetupStyles()
        {
            if (m_MiniButtonStyle == null)
            {
                m_MiniButtonStyle = EditorStyles.miniButton;
                m_MiniButtonStyle.padding = new RectOffset(0, 0, 0, 0); 
                m_Label = "label";
            }

            if (null == m_InspectorLockTex) 
            {
                m_InspectorLockTex = (Texture2D)EditorGUIUtility.Load("IN LockButton on act@2x");
            }

            if (null == m_HiddenInSceneTex) 
            {
                m_HiddenInSceneTex = (Texture2D)EditorGUIUtility.Load("d_scenevis_hidden_hover@2x");
            }
            
        }

        private Rect DrawAllGroupMembers(Rect rect, SelectionGroup group)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            foreach (Object i in group.Members) 
            {
                if (i == null)
                    continue;
                
                //if rect is below window, early out.
                if (rect.yMin - m_Scroll.y > position.height) return rect;
                //if rect is in window, draw.
                if (rect.yMax - m_Scroll.y > 0)
                    DrawGroupMember(rect, group, i);
                rect.y += rect.height;
            }
            return rect;
        }

        private void DrawGroupMember(Rect rect, SelectionGroup group, UnityEngine.Object g) 
        {
            Assert.IsNotNull(g);
            Event e = Event.current;
            GUIContent content = EditorGUIUtility.ObjectContent(g, g.GetType());
            bool isMouseOver = rect.Contains(e.mousePosition);

            bool isGroupMemberSelected = m_SelectedGroupMembers.Contains(group, g);
            if (isGroupMemberSelected)
                EditorGUI.DrawRect(rect, s_SelectionColor);
            
            if (isMouseOver) 
            {
                EditorGUI.DrawRect(rect, s_HoverColor);
                if (m_HoveredGroupMember != g) {
                    m_HoveredGroupMember = g;
                    Repaint();
                }
            }

            if (g is GameObject gameObject) 
            {
                if (SceneVisibilityManager.instance.IsHidden(gameObject)) 
                {
                    DrawIconTexture(0, rect.y, m_HiddenInSceneTex);
                }
            }
            
            if (g.hideFlags.HasFlag(HideFlags.NotEditable)) 
            {
                DrawIconTexture(16, rect.y, m_InspectorLockTex);
            }

            rect.x += 32;
            bool allowRemove = !group.IsAutoFilled();
            GUI.contentColor = allowRemove ? Color.white : Color.Lerp(Color.white, Color.yellow, 0.25f);
            GUI.Label(rect, content);
            GUI.contentColor = Color.white;
            
            HandleGroupMemberMouseEvents(rect, group, g, isGroupMemberSelected);            
        }
        
        private Rect DrawHeader(Rect cursor, int groupIndex) 
        {
            const float kFoldoutWidth = 16;
            const float kColorWidth = 32;
            const float kSeparatorWidth = 8;
            
            SelectionGroup group = m_GroupsToDraw[groupIndex];
            bool isPaint = Event.current.type == EventType.Repaint;
            Rect rect = new Rect(cursor) {x = 0, };
            GUIContent content = m_SceneHeaderContent;
            
            content.text = $"{group.groupName}";
            
            float currentViewWidth = EditorGUIUtility.currentViewWidth;
            
            //background
            Color backgroundColor = ((SelectionGroup) group == m_ActiveSelectionGroup) ? Color.white * 0.6f : Color.white * 0.3f;
            if (isPaint) 
            {
                rect.width = currentViewWidth - kRightMargin - kColorWidth;                
                EditorGUI.DrawRect(rect, backgroundColor);
            } 
            
            //foldout and label
            float labelWidth = currentViewWidth
                             - (kColorWidth + kFoldoutWidth + kRightMargin + kSeparatorWidth);
            {
                rect.width =  kFoldoutWidth;
                group.ShowMembersInWindow(EditorGUI.Toggle(rect, group.AreMembersShownInWindow(), EditorStyles.foldout));
                
                rect.x     += kFoldoutWidth;
                rect.width =  labelWidth;
                
            }
            
            //Draw tools first before handling mouse events
            float toolRightAlignedX = rect.x + rect.width;
            DrawTools(toolRightAlignedX, rect.y, group);
            
            HandleHeaderMouseEvents(rect, groupIndex);
            
            if (isPaint) 
            {
                m_Label.normal.textColor = EditorGUIUtility.isProSkin ? s_ProTextColor: Color.black;
                GUI.Label(rect, content, m_Label);
            }
            
            rect.x = toolRightAlignedX + kSeparatorWidth;
            rect.width = kColorWidth;

            EditorGUI.DrawRect(rect, group.color);
            rect.x = cursor.x;
            rect.y += rect.height;
            rect.width = cursor.width;
            return rect;
        }

        
        private void DrawTools(float rightAlignedX, float y, SelectionGroup group)
        {
            const int kToolXDiff = 18;
            const int kToolHeight = 18;

            Rect rect = new Rect(0, y, kToolXDiff, kToolHeight); 
            int enabledToolCounter = 0;
            
            for (int toolId = (int)SelectionGroupToolType.BuiltInMAX-1; toolId >=0; --toolId) 
            {
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
                
                rect.x = rightAlignedX - ((enabledToolCounter+1) * kToolXDiff);
                if (GUI.Button(rect, content, m_MiniButtonStyle))
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

        private static void DrawIconTexture(float iconX, float iconY, Texture2D tex) 
        {
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
            if (clickedGroup.IsAutoFilled()) 
            {
                menu.AddDisabledItem(content,false);
            }
            else 
            {
                menu.AddItem(content, false, () => 
                {
                    RemoveSelectedMembersFromGroup();
                });
            }
            
            menu.DropDown(rect);
        }

        private void ShowGroupContextMenu(Rect rect, string groupName, SelectionGroup group)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Select All Group Members"), false, () => 
            {
                SelectAllGroupMembers(group);
            });
            menu.AddSeparator(string.Empty);
            if (group.IsAutoFilled()) 
            {
                menu.AddDisabledItem(new GUIContent("Clear Group"), false);
            }
            else
            {
                menu.AddItem(new GUIContent("Clear Group"), false, () =>
                {
                    m_SelectedGroupMembers.RemoveGroup(group);
                    group.Clear();
                    UpdateUnityEditorSelectionWithMembers();
                });
            }

            menu.AddItem(new GUIContent("Delete Group"), false, () => 
            {
                DeleteGroup(group);
            });
            menu.DropDown(rect);
        }

//----------------------------------------------------------------------------------------------------------------------        
        
        private void HandleHeaderMouseEvents(Rect rect, int groupIndex)
        {
            Event evt = Event.current;
            if (!rect.Contains(evt.mousePosition)) 
                return;

            SelectionGroup group = m_GroupsToDraw[groupIndex];
            
            switch (evt.type)
            {
                case EventType.Repaint: 
                {
                    if (DragAndDrop.visualMode == DragAndDropVisualMode.Move) 
                    {
                        //Show lines to indicate where to drop the dragged group
                        Rect dropRect = rect;
                        dropRect.height = 2;

                        DragDropPos dropPos = Editor.DragDropPos.ABOVE;
                        float halfHeight = rect.height * 0.5f;
                        if (evt.mousePosition.y - rect.y > halfHeight) {
                            dropRect.y += rect.height + kGroupHeaderPadding;
                            dropPos    =  Editor.DragDropPos.BELOW;
                        }  
                        DragAndDrop.SetGenericData(kDragDropPos,dropPos);
                        
                        EditorGUI.DrawRect(dropRect, new Color(0.2f, 0.35f, 0.85f, 1f));
                    }
                    break;
                }
                case EventType.MouseDown:
                    switch (evt.button)
                    {
                        case kRightMouseButton:
                            ShowGroupContextMenu(rect, @group.groupName, @group);
                            break;
                        case kLeftMouseButton:
                            m_LeftMouseWasDoubleClicked = evt.clickCount > 1;
                            if (m_LeftMouseWasDoubleClicked) 
                            {
                                SelectAllGroupMembers(group);
                            }
                            break;
                    }
                    evt.Use();
                    break;
                case EventType.MouseUp:
                    switch (evt.button) {
                        case kLeftMouseButton:
                            if (!m_LeftMouseWasDoubleClicked) 
                            {
                                SetUnityEditorSelection(group);
                                m_SelectedGroupMembers.Clear();
                            }
                            break;
                    }
                    evt.Use();
                    break;
                
                case EventType.MouseDrag:
                    if ((SelectionGroup) m_ActiveSelectionGroup != group)
                        break;
                    
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new[] { @group.gameObject };
                    DragAndDrop.SetGenericData(kDragItemType,Editor.DragItemType.GROUP);
                    DragAndDrop.SetGenericData(kDragGroupIndex,groupIndex);
                    DragAndDrop.StartDrag(@group.groupName);
                    evt.Use();
                    break;
                case EventType.DragUpdated: 
                {
                    //This event can occur ay any time. VisualMode must be assigned a value other than Rejected, else
                    //the DragPerform event will not be triggered.
                    DragItemType? dragItemType = DragAndDrop.GetGenericData(kDragItemType) as DragItemType?;

                    bool targetGroupIsAuto  = @group.IsAutoFilled();
                    bool draggedItemIsGroup = (dragItemType == Editor.DragItemType.GROUP);

                    if (draggedItemIsGroup) 
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    }
                    else if (targetGroupIsAuto)
                    { 
                        //copying/moving members to auto group. Invalid 
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    } 
                    else 
                    {
                        //moving window members to group.  
                        bool isMovingWindowMembers = (dragItemType == Editor.DragItemType.WINDOW_GROUP_MEMBERS && evt.control);
                        DragAndDrop.visualMode = isMovingWindowMembers
                            ? DragAndDropVisualMode.Move
                            : DragAndDropVisualMode.Copy; 
                        
                    }
                    
                    evt.Use();
                    break;
                }
                case EventType.DragPerform: 
                {
                    //DragPerform will only arrive when a valid Drop occurs (determined by the above DragUpdated code)
                    DragAndDrop.AcceptDrag();

                    DragItemType? dragItemType = DragAndDrop.GetGenericData(kDragItemType) as DragItemType?;
                    if (!dragItemType.HasValue) {
                        dragItemType = Editor.DragItemType.GAMEOBJECTS; //receive gameObjects from outside the window
                    } 
                    
                    try {
                        switch (dragItemType.Value) 
                        {
                            case Editor.DragItemType.WINDOW_GROUP_MEMBERS: 
                            {
                                if (evt.control) 
                                {
                                    m_SelectedGroupMembers = SelectionGroupUtility.MoveMembersSelectionToGroup(
                                        m_SelectedGroupMembers, group
                                    );
                                } 
                                else 
                                {
                                    RegisterUndo(@group, "Add Members");
                                    HashSet<Object> members = m_SelectedGroupMembers.ConvertMembersToSet();
                                    @group.Add(members);
                                }
                                
                                break;
                            } 
                            case Editor.DragItemType.GAMEOBJECTS: 
                            {
                                RegisterUndo(@group, "Add Members");
                                @group.Add(DragAndDrop.objectReferences);
                                break;
                            }
                            case Editor.DragItemType.GROUP: 
                            {
                                Object[] draggedObjects = DragAndDrop.objectReferences;
                                if (null == draggedObjects || draggedObjects.Length <= 0 || null == draggedObjects[0])
                                    break;

                                int? dragGroupIndex = DragAndDrop.GetGenericData(kDragGroupIndex) as int?;
                                DragDropPos? dropPos = DragAndDrop.GetGenericData(kDragDropPos) as DragDropPos?;
                                if (!dragGroupIndex.HasValue || !dropPos.HasValue) {
                                    break;
                                }

                                int srcIndex = dragGroupIndex.Value;

                                if (dragGroupIndex == groupIndex
                                    || Editor.DragDropPos.ABOVE == dropPos && srcIndex == groupIndex - 1
                                    || Editor.DragDropPos.BELOW == dropPos && srcIndex == groupIndex + 1
                                   ) 
                                {
                                    break;
                                }

                                //Calculate the target new index for the group correctly
                                int targetIndex = groupIndex;
                                if (Editor.DragDropPos.BELOW == dropPos && targetIndex < srcIndex) 
                                {
                                    ++targetIndex;
                                }

                                if (Editor.DragDropPos.ABOVE == dropPos && targetIndex > srcIndex) 
                                {
                                    --targetIndex;
                                }

                                SelectionGroupManager.GetOrCreateInstance().MoveGroup(srcIndex, targetIndex);
                                Repaint();
                                break;
                            }
                        }
                    } 
                    catch (SelectionGroupException e) 
                    {
                        ShowNotification(new GUIContent(e.Message));
                    }

                    evt.Use();
                    break;
                }
            }
        }

        
        private void HandleGroupMemberMouseEvents(Rect rect, SelectionGroup group, Object groupMember, bool isGroupMemberSelected)
        {
            Event evt = Event.current;
            if (!rect.Contains(evt.mousePosition)) 
                return;

            bool isControl = evt.control;
            bool isShift = evt.shift;
            bool isRightMouseClick = evt.button == kRightMouseButton;
            bool isLeftMouseClick = evt.button == kLeftMouseButton;

            switch (evt.type) 
            {
                case EventType.MouseDown: 
                {
                    if (isLeftMouseClick && !isShift) 
                    {
                        m_ShiftPivotGroup       = group;
                        m_ShiftPivotGroupMember = groupMember;
                    }
                    
                    if (isRightMouseClick && isGroupMemberSelected) 
                    {
                        ShowGroupMemberContextMenu(rect, group);
                        evt.Use();
                    }

                    SetUnityEditorSelection(null);
                    
                    evt.Use();
                    break;
                }
                case EventType.MouseUp: 
                {
                    if (isLeftMouseClick) 
                    {
                        if (!isShift) 
                        {
                            if (!isControl)
                            {
                                m_SelectedGroupMembers.Clear();
                                m_SelectedGroupMembers.AddObject(group, groupMember);
                            }
                            else 
                            {
                                if (!isGroupMemberSelected)
                                {
                                    m_SelectedGroupMembers.AddObject(group, groupMember);
                                }
                                else
                                {
                                    m_SelectedGroupMembers.RemoveObject(group, groupMember);
                                }
                            }
                        
                        } 
                        else 
                        {
                            if (!isControl) 
                            {
                                m_SelectedGroupMembers.Clear();
                            }

                            //add objects from shift pivot
                            GroupMembersSelection selectedMembersByShift = SelectMembersInBetween(
                                m_ShiftPivotGroup, m_ShiftPivotGroupMember, 
                                group, groupMember, m_GroupsToDraw);
                            m_SelectedGroupMembers.Add(selectedMembersByShift);
                        } //end shift
                        UpdateUnityEditorSelectionWithMembers();
                    } //end left mouse click

                    evt.Use();

                    break;
                }

                case EventType.MouseDrag:
                    //Prepare the selected objects to be dragged:
                    Object[] objects = m_SelectedGroupMembers.ConvertMembersToArray();
                    
                    int numDraggedObjects = objects.Length;
                    if (numDraggedObjects <= 0)
                        break;
                    
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = objects;
                    DragAndDrop.SetGenericData(kDragItemType,Editor.DragItemType.WINDOW_GROUP_MEMBERS);
                    string dragText = numDraggedObjects > 1 ? objects[0].name + " ..." : objects[0].name;
                    DragAndDrop.StartDrag(dragText);
                    evt.Use();
                    break;
            }
        }
        
        //Find object between (pivotGroup, pivotGroupMember) and (endGroup,endMember).
        //The order between them is not guaranteed.
        [CanBeNull]
        private static GroupMembersSelection SelectMembersInBetween(
            SelectionGroup pivotSG, Object pivotMember, 
            SelectionGroup endSG, Object endMember, IList<SelectionGroup> allGroups) 
        {
            if (allGroups.Count == 0)
                return null;

            GroupMembersSelection ret = new GroupMembersSelection();
            
            bool startAdd = (null == pivotSG);

            foreach (SelectionGroup group in allGroups) 
            {
                foreach (Object m in group.Members) 
                {
                    bool shouldToggleState = (group == pivotSG && m == pivotMember)
                                             || (group == endSG && m == endMember);
                    
                    if (startAdd) 
                    {
                        ret.AddObject(group,m);
                        if (shouldToggleState)
                            return ret;
                    } 
                    else 
                    {
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

        private void DeleteGroup(SelectionGroup group) 
        {
            m_SelectedGroupMembers.RemoveGroup(group);
            SelectionGroupManager.GetOrCreateInstance().DeleteGroup(group);
            UpdateUnityEditorSelectionWithMembers();
            
        }

        private void RemoveSelectedMembersFromGroup() 
        {
            foreach (KeyValuePair<SelectionGroup, OrderedSet<Object>> kv in m_SelectedGroupMembers) 
            {
                SelectionGroup group = kv.Key;
                RegisterUndo(group, "Remove Member");
                group.Remove(kv.Value);
            }
        }

        private void ClearSelectedMembers() 
        {
            m_SelectedGroupMembers.Clear();
            UpdateUnityEditorSelectionWithMembers();
        }

        private void SelectAllGroupMembers(SelectionGroup group) 
        {
            m_SelectedGroupMembers.AddGroupMembers(group);
            UpdateUnityEditorSelectionWithMembers();
        }
        
//----------------------------------------------------------------------------------------------------------------------        
        
        private void SetUnityEditorSelection(SelectionGroup group) 
        {
            m_ActiveSelectionGroup = @group;
            Selection.objects = new Object[] { null == group ? null : group.gameObject };
        }

        //Update Editor Selection to show the properties of group members in the inspector
        private void UpdateUnityEditorSelectionWithMembers() 
        {
            Selection.objects = m_SelectedGroupMembers.ConvertMembersToArray();
        }
    }
} //end namespace
