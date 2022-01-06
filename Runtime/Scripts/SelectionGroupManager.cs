using System;
using System.Collections.Generic;
using System.Linq;
using Unity.FilmInternalUtilities;
using Unity.GoQL;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.SelectionGroups.Runtime {

[ExecuteAlways]
[AddComponentMenu("")]
internal class SelectionGroupManager : MonoBehaviourSingleton<SelectionGroupManager>, ISerializationCallbackReceiver {
    private void OnEnable() {
        this.gameObject.hideFlags = HideFlags.HideInHierarchy;
    }


    public static void ExecuteSelectionGroupQueries() {
        foreach (var i in SelectionGroupManager.GetOrCreateInstance().m_sceneSelectionGroups) {
            if (!string.IsNullOrEmpty(i.Query)) ExecuteQuery(i);
        }
    }

    internal IList<SelectionGroup> Groups => m_sceneSelectionGroups;

    internal IEnumerable<string> GroupNames => m_sceneSelectionGroups.Select(g => g.Name);


    [Obsolete]
    internal SelectionGroup CreateSceneSelectionGroup(string groupName, string query, Color color, IList<Object> members) {
        SelectionGroup group = CreateSceneSelectionGroupInternal(groupName, color);        
        group.SetQuery(query);

        if (!group.IsAutoFilled()) {
            group.Add(members);
        }

        m_sceneSelectionGroups.Add(group);
        return group;
    }

    internal SelectionGroup CreateSceneSelectionGroup(string groupName, Color color, string query) {
        SelectionGroup group = CreateSceneSelectionGroupInternal(groupName, color);        
        group.SetQuery(query);
        m_sceneSelectionGroups.Add(group);
        return group;
    }

    internal SelectionGroup CreateSceneSelectionGroup(string groupName, Color color, IList<Object> members) {
        SelectionGroup group = CreateSceneSelectionGroupInternal(groupName, color);
        group.Add(members);
        m_sceneSelectionGroups.Add(group);
        return group;
    }

    private static SelectionGroup CreateSceneSelectionGroupInternal(string groupName, Color color) {
        GameObject g = new GameObject(groupName);
#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(g, "New Scene Selection Group");
#endif
        SelectionGroup group = g.AddComponent<SelectionGroup>();
        group.Name        = groupName;
        group.Color       = color;
        group.ShowMembers = true; //[TODO-sin: 2022-1-6] This is always true. Maybe we can move it in the constructor?
        return group;
    }
        
    
    
    internal void DeleteSceneSelectionGroup(ISelectionGroup group) {
        //[TODO-sin: 2021-12-24] Simplify this by removing ISelectionGroup interface
        SelectionGroup sceneSelectionGroup = group as SelectionGroup;
        if (null == sceneSelectionGroup)
            return;

        FilmInternalUtilities.ObjectUtility.Destroy(sceneSelectionGroup.gameObject, forceImmediate: true);

        m_sceneSelectionGroups.Remove(sceneSelectionGroup);
    }

    internal void Register(SelectionGroup group) {
        Assert.IsNotNull(group);
        m_sceneSelectionGroups.Add(group);
    }


    internal void MoveGroup(int prevIndex, int newIndex) {
        m_sceneSelectionGroups.Move(prevIndex, newIndex);
    }

    //[TODO-sin:2021-12-20] Remove in version 0.7.0 
    // public static void ClearEditorGroups()
    // {
    //     foreach(var i in groups.ToArray())
    //         if(i.Scope == SelectionGroupDataLocation.Editor) Unregister(i);
    // }

    //[TODO-sin:2021-12-20] Remove in version 0.7.0 
    // public static void ChangeGroupScope(ISelectionGroup @group, SelectionGroupDataLocation scope)
    // {
    //     Create(scope, @group.Name, @group.Query, @group.Color, @group.Members);
    //     Delete(@group);
    // }
    
    public static void ExecuteQuery(ISelectionGroup group) {
        //[TODO-sin: 2022-1-6] Execute the method inside the group
        
        var executor = new GoQLExecutor();
        var code     = GoQL.Parser.Parse(group.Query, out GoQL.ParseResult parseResult);
        if (parseResult == GoQL.ParseResult.OK) {
            executor.Code = group.Query;
            var objects = executor.Execute();
            group.SetMembers(objects);
        }
    }

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