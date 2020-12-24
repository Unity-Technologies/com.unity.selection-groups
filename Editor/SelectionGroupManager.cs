using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.SelectionGroups.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Unity.SelectionGroups
{

    //NOTE: This class should eventually use ScriptableObjectSingleton when it becomes usable in a production version.
    /// <summary>
    /// The Editor-only manager for selection groups.
    /// </summary>
    public partial class SelectionGroupManager : ScriptableObject, IEnumerable<ISelectionGroup>
    {
        Dictionary<int, ISelectionGroup> groups = new Dictionary<int, ISelectionGroup>();
        [SerializeField] int _groupCounter;
        [SerializeField] bool isDirty = true;
        [SerializeField] internal bool enablePlayModeSelectionGroups = false;

        Dictionary<string, Scene> loadedScenes = new Dictionary<string, Scene>();
        

        internal ISelectionGroup GetGroup(int groupId) => groups[groupId];

        void SetIsDirty()
        {
            isDirty = true;
        }

        void OnEnable()
        {
            Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.Full);
            ReloadGroups();
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            Undo.undoRedoPerformed -= ReloadGroups;
            Undo.undoRedoPerformed += ReloadGroups;
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
            SetIsDirty();
        }
        
        void ReloadGroups()
        {
            foreach (var g in groups.Values)
            {
                // if(g is SelectionGroup sg)
                //     sg.ReloadReferences();
            }

            foreach (var g in Resources.FindObjectsOfTypeAll<Runtime.SelectionGroup>())
            {
                groups.Add(g.groupId, g);
            }
        }

        void OnHierarchyChanged()
        {    
            RefreshQueryResults();
        } 

        void Update()
        {
            if (isDirty)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                Profiler.BeginSample("SelectionGroupManager.Update()");
                Save();
                isDirty = false;
                Profiler.EndSample();
            }
        }

        private void RefreshQueryResults()
        {
            // foreach (var i in groups.Values)
            //     i.RefreshQueryResults();
        }

        void OnDisable()
        {
            Save();
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            Undo.undoRedoPerformed -= ReloadGroups;
            EditorApplication.update -= Update;
        }

        bool TryGetGroup(int groupId, out ISelectionGroup group)
        {
            return groups.TryGetValue(groupId, out group);
        }

        internal SelectionGroup CreateGroup(string name)
        {
            Undo.RecordObject(instance, "Create Group");
            var g = new SelectionGroup
            {
                GroupId = _groupCounter++,
                Name = name,
                Color = Color.HSVToRGB(Random.value, Random.Range(0.9f, 1f), Random.Range(0.9f, 1f)),
                ShowMembers = true
            };
            groups.Add(g.GroupId, g);
            SetIsDirty();
            return g;
        }

        /// <summary>
        /// Remove a selection group.
        /// </summary>
        /// <param name="groupId"></param>
        public void RemoveGroup(int groupId)
        {
            Undo.RecordObject(instance, "Remove Group");
            SetIsDirty();
            groups.Remove(groupId);
        }

        /// <summary>
        /// Fetch an array of selection group names.
        /// </summary>
        /// <returns></returns>
        public string[] GetGroupNames()
        {
            return (from i in groups.Values select i.Name).ToArray();
        }

        /// <summary>
        /// Duplicate the group specified by groupId.
        /// </summary>
        /// <param name="groupId"></param>
        public void DuplicateGroup(int groupId)
        {
            if (TryGetGroup(groupId, out ISelectionGroup group))
            {
                Undo.RecordObject(instance, "Duplicate Group");
                var newGroup = CreateGroup(group.Name);
                newGroup.Query = group.Query;
                newGroup.Color = group.Color;
                // newGroup.Add(group.ToArray());
                SetIsDirty();
            }
        }

        /// <summary>
        /// Enumerate over all selection groups in this manager.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ISelectionGroup> GetEnumerator()
        {
            foreach (var kv in groups)
            {
                var group = kv.Value;
                yield return group;
            }
        }
        /// <summary>
        /// The enumerable for all items this manager.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator<SelectionGroup>)(this).GetEnumerator();
        }
        
        internal static void ChangeScope(ISelectionGroup group, SelectionGroupScope prevScope, SelectionGroupScope newScope)
        {
            var groups = s_Instance.groups;
            
            //Remove from old scope
            switch(prevScope)
            {
                case SelectionGroupScope.Editor:
                    groups.Remove(group.GroupId);
                    break;
                case SelectionGroupScope.Scene:
                    break;
                case SelectionGroupScope.Asset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(prevScope), prevScope, null);
            }
            
            //Add to new scope
            switch (newScope)
            {
                case SelectionGroupScope.Editor:
                    break;
                case SelectionGroupScope.Scene:
                    var g = new GameObject(group.Name);
                    var sg = g.AddComponent<Runtime.SelectionGroup>();
                    // group.CopyToRuntimeGroup(sg);
                    groups[group.GroupId] = sg;
                    break;                    
                case SelectionGroupScope.Asset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newScope), newScope, null);
            }
        }
    }
}
