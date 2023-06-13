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
            treeViewElement.RefreshItems();
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
            Selection.objects = selection.OfType<Object>().ToArray();
            foreach (var i in Selection.objects)
            {
                if (i is GameObject go && go.TryGetComponent<SelectionGroup>(out var selectionGroup))
                    m_activeSelectionGroup = selectionGroup;
            }
        }

        private static void CreateToolColumn(MultiColumnTreeView treeViewElement)
        {
            treeViewElement.columns[2].makeCell = () => new VisualElement() { name = "toolColumn" };
            treeViewElement.columns[2].bindCell = (element, i) =>
            {
                var go = treeViewElement.GetItemDataForIndex<GameObject>(i);
                var container = element.Q<VisualElement>("toolColumn");

                if (go.TryGetComponent<SelectionGroup>(out var selectionGroup))
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
                var go = treeViewElement.GetItemDataForIndex<GameObject>(i);
                var colorField = element.Q<ColorField>();

                if (go.TryGetComponent<SelectionGroup>(out var selectionGroup))
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
            treeViewElement.columns[0].makeCell = () => new Label();
            treeViewElement.columns[0].bindCell = (element, i) =>
            {
                var go = treeViewElement.GetItemDataForIndex<GameObject>(i);
                var label = element.Q<Label>();
                label.text = go.name;
                if (go.TryGetComponent<SelectionGroup>(out var selectionGroup))
                {
                    label.AddManipulator(new GameObjectDropManipulator((objects =>
                    {
                        selectionGroup.AddRange(objects.OfType<GameObject>());
                        treeViewElement.SetRootItems(TreeRoots);
                        treeViewElement.RefreshItems();
                    })));
                }

                var parentIndex = treeViewElement.GetParentIdForIndex(i);
                if (parentIndex > 0)
                {
                    label.AddManipulator(CreateContextMenuManipulator(go, parentIndex));
                }
            };
        }

        private IManipulator CreateContextMenuManipulator(GameObject go, int parentIndex)
        {
            return new ContextualMenuManipulator(ev =>
            {
                var parent = treeViewElement.GetItemDataForIndex<GameObject>(parentIndex);
                if (parent.TryGetComponent<SelectionGroup>(out var clickedGroup))
                {
                    if (clickedGroup.IsAutoFilled())
                    {
                        ev.menu.AppendAction("(Remove from Group)", action => { }, DropdownMenuAction.Status.Disabled);
                    }
                    else
                    {
                        ev.menu.AppendAction("Remove from Group", (dda) =>
                        {
                            m_activeSelectionGroup = clickedGroup;
                            m_selectedGroupMembers.Clear();
                            m_selectedGroupMembers.AddObject(clickedGroup, go);
                            RemoveSelectedMembersFromGroup();
                            Refresh();
                        }, DropdownMenuAction.AlwaysEnabled);
                    }
                }
            });
        }

        private void GroupMemberContextMenu(ContextualMenuPopulateEvent ev)
        {
            var content = new GUIContent("Remove From Group");
            var clickedGroup = m_activeSelectionGroup;
            if (clickedGroup.IsAutoFilled()) {
                ev.menu.AppendAction("Remove from Group", action => { }, DropdownMenuAction.Status.Disabled);
            } else {
                ev.menu.AppendAction("Remove from Group", (dda)=>RemoveSelectedMembersFromGroup(), DropdownMenuAction.AlwaysEnabled);
            }
        }

        private static IList<TreeViewItemData<GameObject>> TreeRoots
        {
            get
            {
                var selectionGroupManager = SelectionGroupManager.GetOrCreateInstance();
                int id = 0;
                var roots = new List<TreeViewItemData<GameObject>>(selectionGroupManager.Groups.Count);
                foreach (var group in selectionGroupManager.Groups)
                {
                    var children = new List<TreeViewItemData<GameObject>>(group.Count);
                    foreach (var groupGameObject in group)
                    {
                        children.Add(new TreeViewItemData<GameObject>(id++, groupGameObject));
                    }
                    roots.Add(new TreeViewItemData<GameObject>(id++, group.gameObject, children));
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


        private void DeleteGroup(SelectionGroup group) {
            m_selectedGroupMembers.RemoveGroup(group);
            SelectionGroupManager.GetOrCreateInstance().DeleteGroup(group);
            UpdateUnityEditorSelectionWithMembers();
            
        }

        private void RemoveSelectedMembersFromGroup() {
            
            foreach (KeyValuePair<SelectionGroup, OrderedSet<GameObject>> kv in m_selectedGroupMembers) {
                SelectionGroup group = kv.Key;
                RegisterUndo(group, "Remove Member");
                Debug.Log($"{group} {kv.Value}");
                group.Except(kv.Value);
            }
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

        
        private Texture2D m_inspectorLockTex;
        private Texture2D m_hiddenInSceneTex;
        private MultiColumnTreeView treeViewElement;
    }
} //end namespace
