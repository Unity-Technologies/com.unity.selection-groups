using System.Collections.Generic;
using System.Reflection;
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
    
    internal partial class SelectionGroupEditorWindow : EditorWindow
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
        Object hotMember;

        private bool isReadOnly = false;
        private static float? performQueryRefresh = null;

        
        [InitializeOnLoadMethod]
        static void SetupQueryCallbacks()
        {
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        void Update()
        {
            //This should coalesce many consecutive and possibly duplicate or spurious
            //hierarchy change events into a single query update and repaint operation.
            if (performQueryRefresh.HasValue && EditorApplication.timeSinceStartup > performQueryRefresh.Value)
            {
                SelectionGroupManager.ExecuteSelectionGroupQueries();
                performQueryRefresh = null;
                Repaint();
            }
        }
        private static void OnHierarchyChanged()
        {
            performQueryRefresh = (float) (EditorApplication.timeSinceStartup + 0.2f);
        }

        static void CreateNewGroup()
        {
            SelectionGroupManager.Create(SelectionGroupScope.Scene, "New Group", string.Empty, Color.HSVToRGB(Random.value, Random.Range(0.9f, 1f), Random.Range(0.9f, 1f)), new List<Object>());
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
