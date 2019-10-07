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

        void OnEnable()
        {
            titleContent.text = "Selection Groups";
            editorWindow = this;
        }

        void OnHierarchyChange()
        {
            //This is required to preserve refences when a gameobject is moved between scenes in the editor.
            SanitizeSceneReferences();
        }

        void OnDisable()
        {
            SelectionGroupContainer.onLoaded -= OnContainerLoaded;
            editorWindow = null;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {

        }

        void OnSelectionChange()
        {
        }

        void OnGUI()
        {
            if (miniButtonStyle == null)
            {
                miniButtonStyle = EditorStyles.miniButton;
                miniButtonStyle.padding = new RectOffset(0, 0, 0, 0);
            }
            var names = SelectionGroupUtility.GetGroupNames();
            scroll = EditorGUILayout.BeginScrollView(scroll);
            if (hotRect.HasValue)
                EditorGUI.DrawRect(hotRect.Value, Color.white * 0.5f);
            if (GUILayout.Button("Add Group"))
            {
                CreateNewGroup(Selection.objects);
            }
            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                foreach (var n in names)
                {
                    GUILayout.Space(EditorGUIUtility.singleLineHeight);
                    var rect = GUILayoutUtility.GetRect(1, EditorGUIUtility.singleLineHeight);
                    var dropRect = rect;
                    var showChildren = DrawHeader(rect, n);
                    if (showChildren)
                    {
                        var members = SelectionGroupUtility.GetGameObjects(n);
                        rect = GUILayoutUtility.GetRect(1, (EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight) * members.Count);
                        dropRect.yMax = rect.yMax;
                        DrawGroupMembers(rect, n, members, allowRemove: true);
                        var queryMembers = SelectionGroupUtility.GetQueryObjects(n);
                        if (queryMembers.Count > 0)
                        {
                            var bg = GUI.backgroundColor;
                            GUI.backgroundColor = Color.yellow;
                            rect = GUILayoutUtility.GetRect(1, (EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight) * queryMembers.Count);
                            dropRect.yMax = rect.yMax;
                            DrawGroupMembers(rect, n, queryMembers, allowRemove: false);
                            GUI.backgroundColor = bg;
                        }
                    }
                    if (HandleDragEvents(dropRect, n))
                        Event.current.Use();
                }
                GUILayout.FlexibleSpace();
                GUILayout.Space(16);

                // addNewRect.yMax = GUILayoutUtility.GetLastRect().yMax;
                //This handle creating a new group by dragging onto the add button.
                var addNewRect = GUILayoutUtility.GetLastRect();
                addNewRect.yMin -= 16;
                HandleDragEvents(addNewRect, null);

                GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
                var bottom = GUILayoutUtility.GetLastRect();
                if (cc.changed)
                {
                }
            }
            EditorGUILayout.EndScrollView();

            //Unlike other drag events, this DragExited should be handled once per frame.
            if (Event.current.type == EventType.DragExited)
            {
                ExitDrag();
                Event.current.Use();
            }

            if (focusedWindow == this)
                Repaint();
        }
    }
}
