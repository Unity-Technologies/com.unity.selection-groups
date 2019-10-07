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
            window.ShowUtility();
        }



        void DrawGroupMembers(Rect rect, string groupName, List<GameObject> members, bool allowRemove)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            foreach (var i in members)
            {
                DrawGroupMemberWidget(rect, groupName, i, allowRemove);
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        void DrawGroupMemberWidget(Rect rect, string groupName, GameObject g, bool allowRemove)
        {
            var content = EditorGUIUtility.ObjectContent(g, typeof(GameObject));
            if (Selection.activeGameObject == g)
            {
                GUI.Box(rect, string.Empty);
            }
            rect.x += 24;
            if (GUI.Button(rect, content, "label"))
            {
                Selection.activeGameObject = g;
                if (Event.current.button == RIGHT_MOUSE_BUTTON)
                {
                    ShowGameObjectContextMenu(rect, groupName, g, allowRemove);
                }
            }
        }

        bool DrawHeader(Rect rect, string groupName)
        {
            var group = SelectionGroupUtility.GetFirstGroup(groupName);
            var content = EditorGUIUtility.IconContent("LODGroup Icon");
            content.text = $"{groupName}";
            var backgroundColor = groupName == hotGroup ? Color.white * 0.5f : Color.white * 0.4f;
            EditorGUI.DrawRect(rect, backgroundColor);
            if (HandleMouseEvents(rect, groupName, group))
                Event.current.Use();
            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                rect.width = 16;
                group.showMembers = EditorGUI.Toggle(rect, group.showMembers, "foldout");
                rect.x += 16;
                rect.width = EditorGUIUtility.currentViewWidth - 96;
                if (GUI.Button(rect, content, "label"))
                {
                    hotGroup = groupName;
                    Selection.objects = SelectionGroupUtility.GetMembers(groupName).ToArray();
                }
                rect.x += rect.width;
                rect.width = 16;
                if (group.mutability != MutabilityMode.Disabled)
                {
                    if (GUI.Button(rect, EditorGUIUtility.IconContent("InspectorLock", "Toggle Mutability"), miniButtonStyle))
                    {
                        if (SelectionGroupEditorUtility.AreAnyMembersLocked(groupName))
                        {
                            SelectionGroupEditorUtility.UnlockGroup(groupName);
                        }
                        else
                        {
                            SelectionGroupEditorUtility.LockGroup(groupName);
                        }
                    }
                }
                rect.x += 20;

                if (group.visibility != VisibilityMode.Disabled)
                {

                    if (GUI.Button(rect, EditorGUIUtility.IconContent("d_VisibilityOn", "Toggle Visibility"), miniButtonStyle))
                    {
                        if (SelectionGroupEditorUtility.AreAnyMembersHidden(groupName))
                        {
                            SelectionGroupEditorUtility.ShowGroup(groupName);
                        }
                        else
                        {
                            SelectionGroupEditorUtility.HideGroup(groupName);
                        }
                    }
                }
                rect.x += 20;

                EditorGUI.DrawRect(rect, group.color);

                if (cc.changed)
                {
                    //saves the show members flag.
                    SelectionGroupUtility.UpdateGroup(groupName, group);
                    MarkAllContainersDirty();
                }
            }

            return group.showMembers;
        }

        void ShowGameObjectContextMenu(Rect rect, string groupName, GameObject g, bool allowRemove)
        {
            Selection.activeGameObject = g;
            var menu = new GenericMenu();
            var content = new GUIContent("Remove From Group");
            if (allowRemove)
                menu.AddItem(content, false, () =>
                {
                    UndoRecordObject("Remove object from group");
                    SelectionGroupUtility.RemoveObjectFromGroup(g, groupName);
                    MarkAllContainersDirty();
                });
            else
                menu.AddDisabledItem(content);
            menu.DropDown(rect);
        }

        void ShowGroupContextMenu(Rect rect, string groupName, SelectionGroup group)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Enable Mutability Toggle"), group.mutability == MutabilityMode.Enabled, () =>
            {
                if (group.mutability == MutabilityMode.Enabled)
                {
                    SelectionGroupEditorUtility.UnlockGroup(groupName);
                    group.mutability = MutabilityMode.Disabled;
                }
                else
                {
                    group.mutability = MutabilityMode.Enabled;
                }
                SelectionGroupUtility.UpdateGroup(groupName, group);
                MarkAllContainersDirty();
            });

            menu.AddItem(new GUIContent("Enable Visibility Toggle"), group.visibility == VisibilityMode.Enabled, () =>
            {
                if (group.visibility == VisibilityMode.Enabled)
                {
                    SceneVisibilityManager.instance.Show(SelectionGroupUtility.GetMembers(groupName).ToArray(), false);
                    group.visibility = VisibilityMode.Disabled;
                }
                else
                {
                    group.visibility = VisibilityMode.Enabled;
                }
                SelectionGroupUtility.UpdateGroup(groupName, group);
                MarkAllContainersDirty();
            });
            menu.AddSeparator(string.Empty);


            menu.AddItem(new GUIContent("Duplicate Group"), false, () =>
            {
                UndoRecordObject("Duplicate group");
                SelectionGroupUtility.DuplicateGroup(groupName);
                MarkAllContainersDirty();
            });
            menu.AddItem(new GUIContent("Configure Group"), false, () => SelectionGroupDialog.Open(groupName));
            menu.DropDown(rect);
        }

        bool HandleMouseEvents(Rect position, string groupName, SelectionGroup group)
        {
            var e = Event.current;
            if (position.Contains(e.mousePosition))
            {
                switch (e.type)
                {
                    case EventType.MouseDown:
                        if (e.button == RIGHT_MOUSE_BUTTON)
                        {
                            ShowGroupContextMenu(position, groupName, group);
                            return true;
                        }
                        break;
                }
            }
            return false;
        }
    }
}
