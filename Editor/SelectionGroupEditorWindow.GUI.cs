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
                    cursor = DrawAllGroupMembers(cursor, group);
                }
                
                group.SetOnDestroyedCallback(TryRepaint);
            }
            //Handle clicks on blank areas of window.
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                m_selectedGroupMembers.Clear();
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
            bool isPaint = e.type == EventType.Repaint;

            if (isMouseOver && isPaint)
                EditorGUI.DrawRect(rect, HOVER_COLOR);

            bool isGroupMemberSelected = m_selectedGroupMembers.Contains(group, g);

            if (isPaint) {
                if (isGroupMemberSelected)
                    EditorGUI.DrawRect(rect, SELECTION_COLOR);

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
            }
            
            HandleGroupMemberMouseEvents(rect, group, g, isGroupMemberSelected);            
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

        void ShowGroupMemberContextMenu(Rect rect)
        {
            var menu = new GenericMenu();
            var content = new GUIContent("Remove From Group");
            menu.AddItem(content, false, () => {
                foreach (KeyValuePair<ISelectionGroup, OrderedSet<Object>> kv in m_selectedGroupMembers) {
                    ISelectionGroup group = kv.Key;
                    RegisterUndo(group, "Remove Member");
                    group.Remove(kv.Value);
                }
            });
            
            menu.DropDown(rect);
        }

        void ShowGroupContextMenu(Rect rect, string groupName, ISelectionGroup group)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Select All"), false, () => {
                m_selectedGroupMembers.AddGroupMembersToSelection(group);
            });
            menu.AddSeparator(string.Empty);
            if (group.IsAutoFilled()) {
                menu.AddDisabledItem(new GUIContent("Clear Group"), false);
            } else {
                menu.AddItem(new GUIContent("Clear Group"), false, () => {
                    m_selectedGroupMembers.RemoveGroupFromSelection(group);
                    group.Clear();
                });
            }
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
                m_selectedGroupMembers.RemoveGroupFromSelection(group);
                SelectionGroupManager.GetOrCreateInstance().DeleteSceneSelectionGroup(group);
            });
            menu.DropDown(rect);
        }

//----------------------------------------------------------------------------------------------------------------------        
        
        void HandleHeaderMouseEvents(Rect rect, SelectionGroup group)
        {
            Event evt = Event.current;
            if (!rect.Contains(evt.mousePosition)) 
                return;
                
            switch (evt.type)
            {
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
                            } else
                                SelectionGroupConfigurationDialog.Open(@group, this);
                            break;
                    }
                    evt.Use();
                    break;
                case EventType.MouseDrag:
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new[] { @group.gameObject };
                    DragAndDrop.SetGenericData(DRAG_ITEM_TYPE,DragItemType.GROUP);
                    DragAndDrop.StartDrag(@group.Name);
                    evt.Use();
                    break;
                case EventType.DragUpdated:
                    //This event can occur ay any time. VisualMode must be assigned a value other than Rejected, else
                    //the DragPerform event will not be triggered.
                    DragItemType? dragItemType = DragAndDrop.GetGenericData(DRAG_ITEM_TYPE) as DragItemType?;

                    bool targetGroupIsAuto  = @group.IsAutoFilled();
                    bool draggedItemIsGroup = (null != dragItemType && dragItemType == DragItemType.GROUP);

                    if (targetGroupIsAuto || draggedItemIsGroup)
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    else
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    evt.Use();
                    break;
                case EventType.DragPerform:
                    //This will only get called when a valid Drop occurs (determined by the above DragUpdated code)
                    DragAndDrop.AcceptDrag();
                    RegisterUndo(@group, "Add Members");
                    try
                    {
                        @group.Add(DragAndDrop.objectReferences);
                    }
                    catch (SelectionGroupException e)
                    {
                        ShowNotification(new GUIContent(e.Message));
                    }
                    evt.Use();
                    break;
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
                    
                    if (isRightMouseClick && isGroupMemberSelected)                    {
                        ShowGroupMemberContextMenu(rect);
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

        
        private void SetUnityEditorSelection(SelectionGroup group) {
            m_activeSelectionGroup  = @group;
            Selection.objects       = new Object[] { null == group ? null : group.gameObject };
        }

//----------------------------------------------------------------------------------------------------------------------        

        readonly GroupMembersSelection m_selectedGroupMembers = new GroupMembersSelection();

        private IList<SelectionGroup> m_groupsToDraw = null;
        
        private const string DRAG_ITEM_TYPE = "SelectionGroupsWindows";

        private ISelectionGroup m_shiftPivotGroup       = null;
        private Object         m_shiftPivotGroupMember = null;

    }
} //end namespace
