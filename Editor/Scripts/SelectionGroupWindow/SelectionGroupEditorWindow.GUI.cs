using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;


namespace Unity.SelectionGroups.Editor
{
    
    
    internal partial class SelectionGroupEditorWindow : EditorWindow
    {
        const string _uxmlPath = "Packages/com.unity.selection-groups/Editor/UIElements/EditorWindow/SelectionGroupEditorWindow.uxml";
        private const string _stylesheetPath = "Packages/com.unity.selection-groups/Editor/UIElements/EditorWindow/SelectionGroupEditorWindow.uss";
        
        [SerializeField] private VisualTreeAsset _visualTreeAsset = default;
        [SerializeField] private StyleSheet _styleSheet;

        
        private GUIStyle   Label;

        private SerializedObject _serializedObject;

        [MenuItem("Window/General/Selection Groups")]
        static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<SelectionGroupEditorWindow>();
            window.Show();
        }

        private void CreateGUI()
        {
            if(_visualTreeAsset == null)
                _visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_uxmlPath);
            if (_styleSheet == null)
                _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(_stylesheetPath);
            
            rootVisualElement.Add(_visualTreeAsset.Instantiate());
            rootVisualElement.styleSheets.Add(_styleSheet);
            var addGroupButton = rootVisualElement.Q<Button>("addGroup");
            addGroupButton.clicked += () =>
            {
                CreateNewGroup();
                Refresh();
            };
            CreateTreeView(rootVisualElement);
        }

        private void CreateTreeView(VisualElement root)
        {
            treeViewElement = root.Q<MultiColumnTreeView>("treeview");
            
            treeViewElement.selectionType = SelectionType.Multiple;
            
            treeViewElement.selectionChanged += OnSelectionChanged;
            
            treeViewElement.itemsChosen += OnItemsChosen;

            treeViewElement.headerContextMenuPopulateEvent += (menu, c) =>
            {
                menu.menu.AppendAction("Add Group", a => CreateNewGroup(), DropdownMenuAction.AlwaysEnabled);
                EditorApplication.delayCall += Refresh;
            };
            
            treeViewElement.fixedItemHeight = EditorGUIUtility.singleLineHeight;
            
            CreateNameColumn(treeViewElement);

            CreateColorColumn(treeViewElement);
            
            CreateToolColumn(treeViewElement);

            treeViewElement.SetRootItems(TreeRoots);
            
            
            
        }
        
        private void Refresh()
        {
            treeViewElement.SetRootItems(TreeRoots);
            treeViewElement.Rebuild();
        }
        

        private void OnItemsChosen(IEnumerable<object> items)
        {
            foreach (var i in items)
            {
                if (i is GameObject go)
                {
                    if (go.TryGetComponent<SelectionGroup>(out var selectionGroup))
                    {
                        SelectAllGroupMembers(selectionGroup);
                    }
                }
            }
        }

        private void OnSelectionChanged(IEnumerable<object> selection)
        {
            m_selectedGroupMembers.Clear();
            m_selectedGroupMembers.UnionWith(selection.OfType<GroupMembership>());
            
        }

        private static void CreateToolColumn(MultiColumnTreeView treeViewElement)
        {
            treeViewElement.columns[2].makeCell = () => new VisualElement() { name = "toolColumn" };
            treeViewElement.columns[2].bindCell = (element, i) =>
            {
                var groupMembership = treeViewElement.GetItemDataForIndex<GroupMembership>(i);
                var container = element.Q<VisualElement>("toolColumn");

                if (groupMembership.isParent && groupMembership.gameObject.TryGetComponent<SelectionGroup>(out var selectionGroup))
                {
                    container.Clear();
                    container.style.flexDirection = FlexDirection.Row;
                    container.StretchToParentSize();
                    CreateToolButtons(container, selectionGroup);
                }
                else
                {
                    container.RemoveFromHierarchy();
                }
            };
        }

        private static void CreateToolButtons(VisualElement container, SelectionGroup group)
        {
            for (int toolId = (int) SelectionGroupToolType.BuiltIn_Max - 1; toolId >= 0; --toolId)
            {
                bool toolStatus = group.GetEditorToolState(toolId);
                if (false == toolStatus)
                    continue;


                SelectionGroupToolAttribute attr = null;
                MethodInfo methodInfo = null;

                bool found = SelectionGroupToolAttributeCache.TryGetAttribute(toolId, out attr);
                Assert.IsTrue(found);
                found = SelectionGroupToolAttributeCache.TryGetMethodInfo(toolId, out methodInfo);
                Assert.IsTrue(found);

                GUIContent content = EditorGUIUtility.IconContent(attr.icon);
                content.tooltip = attr.description;

                var button = new Button(() =>
                {
                    try
                    {
                        methodInfo.Invoke(null, new object[] {group});
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                });
                container.Add(button);
                button.style.backgroundImage = EditorGUIUtility.Load(attr.icon) as Texture2D;
                button.style.width = container.style.height;
            }
        }

        private static void CreateColorColumn(MultiColumnTreeView treeViewElement)
        {
            treeViewElement.columns[1].makeCell = () => new ColorField();
            treeViewElement.columns[1].bindCell = (element, i) =>
            {
                var groupMembership = treeViewElement.GetItemDataForIndex<GroupMembership>(i);
                var colorField = element.Q<ColorField>();

                if (groupMembership.isParent&& groupMembership.gameObject.TryGetComponent<SelectionGroup>(out var selectionGroup))
                {
                    colorField.value = selectionGroup.Color;
                }
                else
                {
                    colorField.RemoveFromHierarchy();
                }
            };
        }

        
        private void CreateNameColumn(MultiColumnTreeView treeViewElement)
        {
            
            treeViewElement.columns[0].makeCell = () =>
            {
                var element = new Label();
                element.AddManipulator(new ContextualMenuManipulator(OnContextmenu));
                element.AddManipulator(new GameObjectDropManipulator(OnDropObjects));
                return element;
            };

            treeViewElement.columns[0].bindCell = (element, i) =>
            {
                var groupMembership = treeViewElement.GetItemDataForIndex<GroupMembership>(i);
                var label = element.Q<Label>();
                label.text = groupMembership.gameObject.name;
                
                element.userData = groupMembership;
            };
            
            treeViewElement.columns[0].unbindCell = (element, i) =>
            {
                element.userData = null;
            };
            

        }

        private void OnContextmenu(ContextualMenuPopulateEvent ev)
        {
            var groupMembership = (GroupMembership)(ev.target as VisualElement).userData;
            
            if (groupMembership.isParent)
            {
                CreateRootLevelContextMenu(groupMembership, ev);
            }
            else
            {
                CreateMemberLevelContextMenu(groupMembership, ev);
            }
            ev.StopPropagation();
        }

        private void CreateMemberLevelContextMenu(GroupMembership groupMembership, ContextualMenuPopulateEvent ev)
        {
            var clickedGroup = groupMembership.group;
            if (clickedGroup.IsAutoFilled()) {
                ev.menu.AppendAction("Remove from Group", action => { }, DropdownMenuAction.Status.Disabled);
            } else {
                ev.menu.AppendAction("Remove from Group", (dda)=>RemoveSelectedMembersFromGroup(), DropdownMenuAction.AlwaysEnabled);
            }
        }

        private void CreateRootLevelContextMenu(GroupMembership groupMembership, ContextualMenuPopulateEvent ev)
        {
            var menu = ev.menu;
            var group = groupMembership.group;
            menu.AppendAction("Select All Group Members", (dda) => {
                SelectAllGroupMembers(group);
            });
            menu.AppendSeparator();
            if (group.IsAutoFilled())
            {
                menu.AppendAction("Clear Group", null, DropdownMenuAction.Status.Disabled);
            } else {
                menu.AppendAction("Clear Group", (dda) => {
                    m_selectedGroupMembers.RemoveWhere(i=>i.group==group);
                    group.Clear();
                    UpdateUnityEditorSelectionWithMembers();
                    Refresh();
                });
            }

            menu.AppendAction("Delete Group", (dda) => {
                DeleteGroup(group);
                Refresh();
            });
        }

        private void OnDropObjects(VisualElement element, Object[] objects)
        {
            var index = element.userData;

            if (element.userData is GroupMembership groupMembership)
            {
                if (groupMembership.isParent && groupMembership.gameObject.TryGetComponent<SelectionGroup>(out var selectionGroup))
                {
                    selectionGroup.AddRange(objects.OfType<GameObject>());
                    treeViewElement.SetRootItems(TreeRoots);
                    treeViewElement.RefreshItems();
                }
            }
        }
        

        private static IList<TreeViewItemData<GroupMembership>> TreeRoots
        {
            get
            {
                var selectionGroupManager = SelectionGroupManager.GetOrCreateInstance();
                int id = 0;
                var roots = new List<TreeViewItemData<GroupMembership>>(selectionGroupManager.Groups.Count);
                foreach (var group in selectionGroupManager.Groups)
                {
                    var children = new List<TreeViewItemData<GroupMembership>>(group.Count);
                    foreach (var groupGameObject in group)
                    {
                        children.Add(new TreeViewItemData<GroupMembership>(id++, GroupMembership.Child(group, groupGameObject)));
                    }
                    roots.Add(new TreeViewItemData<GroupMembership>(id++, GroupMembership.Parent(group, group.gameObject), children));
                }
                return roots;
            }
        }

        internal static void TryRepaint() {
            if (!EditorWindow.HasOpenInstances<SelectionGroupEditorWindow>())
                return;
            
            SelectionGroupEditorWindow window = EditorWindow.GetWindow<SelectionGroupEditorWindow>(utility:false, title:"", focus:false);
            window.Refresh();
            window.Repaint();
        }

        void ShowGroupMemberContextMenu(SelectionGroup clickedGroup)
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
            
            menu.ShowAsContext();
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
                    m_selectedGroupMembers.RemoveWhere(i=>i.group==group);
                    group.Clear();
                    UpdateUnityEditorSelectionWithMembers();
                });
            }

            menu.AddItem(new GUIContent("Delete Group"), false, () => {
                DeleteGroup(group);
            });
            menu.DropDown(rect);
        }


        private void DeleteGroup(SelectionGroup group) {
            Debug.Log($"Deleting {group}");
            m_selectedGroupMembers.RemoveWhere(i=>i.group==group);
            SelectionGroupManager.GetOrCreateInstance().DeleteGroup(group);
            UpdateUnityEditorSelectionWithMembers();
        }

        private void RemoveSelectedMembersFromGroup() {
            foreach (var membership in m_selectedGroupMembers) {
                RegisterUndo(membership.group, "Remove Member");
                membership.group.Remove(membership.gameObject);
            }
            Refresh();
        }
        

        private void ClearSelectedMembers() {
            m_selectedGroupMembers.Clear();
            UpdateUnityEditorSelectionWithMembers();
        }

        private void SelectAllGroupMembers(SelectionGroup group) {
            foreach (var m in group.Members)
            {
                m_selectedGroupMembers.Add(GroupMembership.Child(group, m));
            }
            UpdateUnityEditorSelectionWithMembers();
        }
        
//----------------------------------------------------------------------------------------------------------------------        
        
        private void SetUnityEditorSelection(SelectionGroup group) {
            m_activeSelectionGroup  = @group;
            Selection.objects       = new Object[] { null == group ? null : group.gameObject };
        }

        //Update Editor Selection to show the properties of group members in the inspector
        private void UpdateUnityEditorSelectionWithMembers() {
            Selection.objects = m_selectedGroupMembers.Where(g=>!g.isParent).Select(i=>i.gameObject).ToArray();
        }
        

//----------------------------------------------------------------------------------------------------------------------        

        private HashSet<GroupMembership> m_selectedGroupMembers = new();

        
        private Texture2D m_inspectorLockTex;
        private Texture2D m_hiddenInSceneTex;
        private MultiColumnTreeView treeViewElement;
        
        // This struct is used to store membership information about group items and groups
        // while being used inside the context of the TreeView
        private struct GroupMembership : IEquatable<GroupMembership>
        {
            public bool Equals(GroupMembership other)
            {
                return Equals(gameObject, other.gameObject) && Equals(group, other.group);
            }

            public override bool Equals(object obj)
            {
                return obj is GroupMembership other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(gameObject, group);
            }

            public static bool operator ==(GroupMembership left, GroupMembership right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(GroupMembership left, GroupMembership right)
            {
                return !left.Equals(right);
            }

            public bool isParent;
            public GameObject gameObject;
            public SelectionGroup group;

            public static GroupMembership Child(SelectionGroup group, GameObject gameObject) => new() { group=group, isParent = false, gameObject = gameObject };

            public static GroupMembership Parent(SelectionGroup group, GameObject gameObject) => new() { group = group, isParent = true, gameObject = gameObject };
        }
    }
} //end namespace
