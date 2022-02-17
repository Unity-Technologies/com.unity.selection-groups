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
        private const int kLeftMouseButton = 0;
        private const int kRightMouseButton = 1;

        private static readonly Color s_SelectionColor = new Color32(62, 95, 150, 255);
        private static float? s_PerformQueryRefresh = null;

        private ReorderableList m_List;
        private Vector2 m_Scroll;
        private SelectionGroup m_ActiveSelectionGroup;
        private float m_Width;
        private GUIStyle m_MiniButtonStyle;
        private Object m_HotMember;

        [InitializeOnLoadMethod]
        private static void SetupQueryCallbacks()
        {
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        private void Update()
        {
            //This should coalesce many consecutive and possibly duplicate or spurious
            //hierarchy change events into a single query update and repaint operation.
            if (s_PerformQueryRefresh.HasValue && EditorApplication.timeSinceStartup > s_PerformQueryRefresh.Value)
            {
                SelectionGroupManager.UpdateQueryResults();
                s_PerformQueryRefresh = null;
                Repaint();
            }
        }
        
        private static void OnHierarchyChanged()
        {
            s_PerformQueryRefresh = (float) (EditorApplication.timeSinceStartup + 0.2f);
        }

        private static void CreateNewGroup() 
        {
            SelectionGroupManager sgManager = SelectionGroupManager.GetOrCreateInstance();
            
            int numGroups = sgManager.groups.Count;
            sgManager.CreateSelectionGroup($"SG_New Group {numGroups}",
                Color.HSVToRGB(Random.value, Random.Range(0.9f, 1f), Random.Range(0.9f, 1f)));
        }

        private static void RegisterUndo(SelectionGroup group, string msg)
        {
            Undo.RegisterCompleteObjectUndo(group, msg);
            EditorUtility.SetDirty(group);
        }
    }
}
