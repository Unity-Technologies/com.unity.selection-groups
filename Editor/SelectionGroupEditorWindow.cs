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
    /// <summary>
    /// The main editor window for working with selection groups.
    /// </summary>
    public partial class SelectionGroupEditorWindow : EditorWindow
    {

        const int LEFT_MOUSE_BUTTON = 0;
        const int RIGHT_MOUSE_BUTTON = 1;

        static readonly Color SELECTION_COLOR = new Color32(62, 95, 150, 255);
        static readonly Color HOVER_COLOR = new Color32(112, 112, 112, 128);

        ReorderableList list;
        Vector2 scroll;
        SelectionGroup activeSelectionGroup;
        float width;
        static SelectionGroupEditorWindow editorWindow;
        Rect? hotRect = null;
        GUIStyle miniButtonStyle;
        HashSet<Object> activeSelection = new HashSet<Object>();
        HashSet<string> activeNames = new HashSet<string>();

        Object hotMember;

        static void CreateNewGroup(Object[] objects)
        {
            Undo.RegisterCompleteObjectUndo(SelectionGroupManager.instance, "Create");
            var g = SelectionGroupManager.instance.CreateGroup("New Group");
            g.Add(objects);
        }
    }
}
