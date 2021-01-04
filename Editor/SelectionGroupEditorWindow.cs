using System.Collections.Generic;
using Unity.SelectionGroups;
using Unity.SelectionGroups.Runtime;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace Unity.SelectionGroupsEditor
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
        ISelectionGroup activeSelectionGroup;
        float width;
        GUIStyle miniButtonStyle;
        HashSet<Object> activeSelection = new HashSet<Object>();
        HashSet<string> activeNames = new HashSet<string>();

        Object hotMember;

        [InitializeOnLoadMethod]
        static void SetupQueryCallbacks()
        {
            EditorApplication.hierarchyChanged += SelectionGroupManager.ExecuteSelectionGroupQueries;
        }

        static void CreateNewGroup()
        {
            SelectionGroupManager.Create(SelectionGroupScope.Editor, "New Group", string.Empty, Color.HSVToRGB(Random.value, Random.Range(0.9f, 1f), Random.Range(0.9f, 1f)), new List<Object>());
        }

        void RegisterUndo(ISelectionGroup @group, string msg)
        {
            if (group is SelectionGroups.Runtime.SelectionGroup runtimeGroup)
            {
                Undo.RegisterCompleteObjectUndo(runtimeGroup, msg);
                EditorUtility.SetDirty(runtimeGroup);
            }

            if (group is SelectionGroup editorGroup)
            {
                SelectionGroupPersistenceManager.RegisterUndo(msg);
            }
        }
    }
}
