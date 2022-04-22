using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace Unity.SelectionGroups.Editor
{
    /// <summary>
    /// The main editor window for working with selection groups.
    /// </summary>
    
    internal partial class SelectionGroupEditorWindow : EditorWindow
    {

        const int LEFT_MOUSE_BUTTON = 0;
        const int RIGHT_MOUSE_BUTTON = 1;

        static readonly Color SELECTION_COLOR = new Color32(62, 95, 150, 255);

        ReorderableList list;
        Vector2 scroll;
        SelectionGroup m_activeSelectionGroup;
        float width;
        GUIStyle miniButtonStyle;
        Object hotMember;

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
                SelectionGroupManager.UpdateQueryResults();
                performQueryRefresh = null;
                Repaint();
            }
        }
        
        private static void OnHierarchyChanged()
        {
            performQueryRefresh = (float) (EditorApplication.timeSinceStartup + 0.2f);
        }

        static void CreateNewGroup() {
            SelectionGroupManager sgManager = SelectionGroupManager.GetOrCreateInstance();
            
            int numGroups = sgManager.Groups.Count;
            sgManager.CreateSelectionGroup($"SG_New Group {numGroups}",
                Color.HSVToRGB(Random.value, Random.Range(0.9f, 1f), Random.Range(0.9f, 1f)));
        }

        static void CreateNewGroupFromSelection()
        {
            SelectionGroupManager sgManager = SelectionGroupManager.GetOrCreateInstance();
            
            int numGroups = sgManager.Groups.Count;
            SelectionGroup newGroup =sgManager.CreateSelectionGroup($"SG_New Group {numGroups}",
                Color.HSVToRGB(Random.value, Random.Range(0.9f, 1f), Random.Range(0.9f, 1f)));

            newGroup.AddRange(Selection.gameObjects);
        }

        static void RegisterUndo(SelectionGroup group, string msg)
        {
            Undo.RegisterCompleteObjectUndo(group, msg);
            EditorUtility.SetDirty(group);
        }
    }
}
