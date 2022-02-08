using System;
using System.Collections.Generic;
using System.Linq;
using Unity.FilmInternalUtilities;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.SelectionGroups {

[ExecuteAlways]
[AddComponentMenu("")]
internal class SelectionGroupManager : MonoBehaviourSingleton<SelectionGroupManager>, ISerializationCallbackReceiver {
    private void OnEnable() {
        this.gameObject.hideFlags = HideFlags.HideInHierarchy;
    }


    public static void UpdateQueryResults() {
        foreach (var i in SelectionGroupManager.GetOrCreateInstance().m_sceneSelectionGroups) {
            if (!string.IsNullOrEmpty(i.Query)) {
                i.UpdateQueryResults();
            }
        }
    }

    internal IList<SelectionGroup> Groups => m_sceneSelectionGroups;

    internal IEnumerable<string> GroupNames => m_sceneSelectionGroups.Select(g => g.Name);


    [Obsolete]
    internal SelectionGroup CreateSceneSelectionGroup(string groupName, string query, Color color, IList<Object> members) {
        SelectionGroup group = CreateSelectionGroupInternal(groupName, color);        
        group.SetQuery(query);

        if (!group.IsAutoFilled()) {
            group.Add(members);
        }

        m_sceneSelectionGroups.Add(group);
        return group;
    }

    internal SelectionGroup CreateSceneSelectionGroup(string groupName, Color color) {
        SelectionGroup group = CreateSelectionGroupInternal(groupName, color);
        m_sceneSelectionGroups.Add(group);
        return group;
    }
    
    internal SelectionGroup CreateSceneSelectionGroup(string groupName, Color color, string query) {
        SelectionGroup group = CreateSelectionGroupInternal(groupName, color);        
        group.SetQuery(query);
        m_sceneSelectionGroups.Add(group);
        return group;
    }

    internal SelectionGroup CreateSceneSelectionGroup(string groupName, Color color, IList<Object> members) {
        SelectionGroup group = CreateSelectionGroupInternal(groupName, color);
        group.Add(members);
        m_sceneSelectionGroups.Add(group);
        return group;
    }

    private static SelectionGroup CreateSelectionGroupInternal(string groupName, Color color) {
        GameObject g = new GameObject(groupName);
#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(g, "New Scene Selection Group");
#endif
        SelectionGroup group = g.AddComponent<SelectionGroup>();
        group.Name        = groupName;
        group.Color       = color;
        
        SelectionGroupsEditorProjectSettings projSettings = SelectionGroupsEditorProjectSettings.GetOrCreateInstance();
        for (int i = 0; i < (int)SelectionGroupToolType.MAX; ++i) {
            group.EnableEditorTool(i, projSettings.GetDefaultGroupEditorToolStatus(i));
        }
        
        return group;
    }

//----------------------------------------------------------------------------------------------------------------------
    
    internal void ClearGroups() {
        int numGroups = m_sceneSelectionGroups.Count;
        for (int i = numGroups - 1; i >= 0; --i) {
            SelectionGroup g = m_sceneSelectionGroups[i];
            if (null == g)
                continue;
            
            FilmInternalUtilities.ObjectUtility.Destroy(g.gameObject, forceImmediate: true);
        }
        m_sceneSelectionGroups.Clear();
    }
        
    
    
    internal void DeleteGroup(SelectionGroup group) {
        
#if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(this, "Delete Group");
        Undo.DestroyObjectImmediate(group.gameObject);
#else
        DestroyImmediate(group,gameObject);
#endif
        m_sceneSelectionGroups.Remove(group);
    }
    
//----------------------------------------------------------------------------------------------------------------------

    internal void Register(SelectionGroup group) {
        Assert.IsNotNull(group);
        m_sceneSelectionGroups.Add(group);
    }

    internal void Unregister(SelectionGroup group) {
        m_sceneSelectionGroups.Remove(group);
    }
    

    internal void MoveGroup(int prevIndex, int newIndex) {
#if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(this, "Move Group");
#endif
        m_sceneSelectionGroups.Move(prevIndex, newIndex);
    }

#if UNITY_EDITOR
    internal void RefreshGroupHideFlagsInEditor() {
        foreach (SelectionGroup group in m_sceneSelectionGroups) {
            group.RefreshHideFlagsInEditor();
        }
        EditorApplication.RepaintHierarchyWindow();
        EditorApplication.DirtyHierarchyWindowSorting();
    }
#endif

//----------------------------------------------------------------------------------------------------------------------

    ///<inheritdoc/>
    public void OnBeforeSerialize() {
        m_sceneSelectionGroups.RemoveAll((g) => null == g);
    }

    ///<inheritdoc/>
    public void OnAfterDeserialize() { }
    

//----------------------------------------------------------------------------------------------------------------------

    [SerializeField] private List<SelectionGroup> m_sceneSelectionGroups = new List<SelectionGroup>();
}
}