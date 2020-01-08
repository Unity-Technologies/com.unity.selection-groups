using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace Unity.SelectionGroups
{
    //NOTE: This class should eventually use ScriptableObjectSingleton when it becomes usable in a production version.
    public partial class SelectionGroupManager : ScriptableObject, IEnumerable<SelectionGroup>
    {
        Dictionary<int, SelectionGroup> groups = new Dictionary<int, SelectionGroup>();
        public int _groupCounter;
        public bool isDirty = true;

        public Dictionary<string, Scene> loadedScenes = new Dictionary<string, Scene>();

        internal void SetIsDirty()
        {
            isDirty = true;
        }

        void OnEnable()
        {
            ReloadGroups();
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosed -= OnSceneClosed;
            EditorSceneManager.sceneClosed += OnSceneClosed;
            EditorSceneManager.sceneClosing -= OnSceneClosing;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            // EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            // EditorApplication.hierarchyChanged += OnHierarchyChanged;
            Undo.undoRedoPerformed -= ReloadGroups;
            Undo.undoRedoPerformed += ReloadGroups;
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
            SetIsDirty();
        }

        void UpdateSelectionGroupContainersInScene()
        {
            foreach (var container in Runtime.SelectionGroupContainer.instances)
            {
                foreach (var kv in groups)
                {
                    var id = kv.Key;
                    var group = kv.Value;
                    if (!container.groups.TryGetValue(id, out Runtime.SelectionGroup runtimeGroup))
                        runtimeGroup = container.AddGroup(id);
                    runtimeGroup.name = group.name;
                    runtimeGroup.color = group.color;
                    if (runtimeGroup.members == null)
                        runtimeGroup.members = new List<UnityEngine.Object>();
                    else
                        runtimeGroup.members.Clear();
                    runtimeGroup.members.AddRange(group.members);
                }
            }
        }

        private void ReloadGroups()
        {
            var allGroups = groups.Values.ToArray();
            for (var i = 0; i < EditorSceneManager.loadedSceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                foreach (var g in allGroups)
                    g.ConvertGlobalObjectIdsToSceneObjects();
            }
        }

        void OnHierarchyChanged() => SetIsDirty();

        void Update()
        {
            Profiler.BeginSample("SelectionGroupManager.Update()");
            if (isDirty)
            {
                isDirty = false;
                // using (var bt = new AnalyticsTimer("SelectionGroupManager.Update"))
                {
                    foreach (var i in groups.Values)
                    {
                        i.RefreshQueryResults();
                    }
                    // bt.SendEvent("RFQR");
                    UpdateSelectionGroupContainersInScene();
                    // bt.SendEvent("USGCIS");
                }
            }
            Profiler.EndSample();
        }

        void OnSceneClosing(Scene scene, bool removingScene)
        {
            foreach (var g in groups.Values)
            {
                g.ConvertSceneObjectsToGlobalObjectIds();
            }
        }

        void OnSceneClosed(Scene scene)
        {
            SetIsDirty();
        }


        void OnDisable()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneClosing -= OnSceneClosing;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            Undo.undoRedoPerformed -= ReloadGroups;
            EditorApplication.update -= Update;
        }

        void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            foreach (var g in groups.Values)
            {
                g.ConvertGlobalObjectIdsToSceneObjects();
            }
            SetIsDirty();
        }

        public bool TryGetGroup(int groupId, out SelectionGroup group)
        {
            return groups.TryGetValue(groupId, out group);
        }

        public SelectionGroup CreateGroup(string name)
        {
            var g = new SelectionGroup();
            g.groupId = _groupCounter++;
            g.name = name;
            groups.Add(g.groupId, g);
            return g;
        }

        public void RemoveGroup(int groupId)
        {
            groups.Remove(groupId);
        }

        public string[] GetGroupNames()
        {
            return (from i in groups.Values select i.name).ToArray();
        }

        public void DuplicateGroup(int groupId)
        {
            if (TryGetGroup(groupId, out SelectionGroup group))
            {
                var newGroup = CreateGroup(group.name);
                newGroup.query = group.query;
                newGroup.color = group.color;
                newGroup.Add(group.members);
            }
        }

        public IEnumerator<SelectionGroup> GetEnumerator()
        {
            foreach (var kv in groups)
            {
                var group = kv.Value;
                yield return group;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator<SelectionGroup>)(this).GetEnumerator();
        }
    }
}
