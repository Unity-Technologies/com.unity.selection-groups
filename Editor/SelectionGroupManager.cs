using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public SelectionGroup GetGroup(int groupId) => groups[groupId];

        internal void SetIsDirty()
        {
            isDirty = true;
        }

        void OnEnable()
        {
            ReloadGroups();
            // EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            // EditorApplication.hierarchyChanged += OnHierarchyChanged;
            Undo.undoRedoPerformed -= ReloadGroups;
            Undo.undoRedoPerformed += ReloadGroups;
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
            SetIsDirty();
        }

        void UpdateSelectionGroupContainersInLoadedScenes()
        {
            foreach (var container in Runtime.SelectionGroupContainer.instances)
            {
                //track all groups so we can delete dead groups.
                var allContainedGroups = new HashSet<Runtime.SelectionGroup>(container.GetComponentsInChildren<Runtime.SelectionGroup>());

                foreach (var kv in groups)
                {
                    var id = kv.Key;
                    var group = kv.Value;
                    if (!container.groups.TryGetValue(id, out Runtime.SelectionGroup runtimeGroup))
                    {
                        runtimeGroup = container.AddGroup(id);
                        allContainedGroups.Add(runtimeGroup);
                    }
                    runtimeGroup.name = group.name;
                    runtimeGroup.color = group.color;
                    runtimeGroup.query = group.query;
                    if (runtimeGroup.members == null)
                        runtimeGroup.members = new List<UnityEngine.Object>();
                    else
                        runtimeGroup.members.Clear();
                    runtimeGroup.members.AddRange(group);
                    allContainedGroups.Remove(runtimeGroup);
                }
                foreach (var deadGroup in allContainedGroups)
                {
                    if (container.groups.ContainsKey(deadGroup.groupId))
                        container.groups.Remove(deadGroup.groupId);
                    DestroyImmediate(deadGroup.gameObject);
                }
            }
        }

        private void ReloadGroups()
        {
            
        }

        void OnHierarchyChanged() => SetIsDirty();

        void Update()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
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
                    UpdateSelectionGroupContainersInLoadedScenes();
                    // bt.SendEvent("USGCIS");
                }
            }
            Profiler.EndSample();
        }


        void OnDisable()
        {
            Save();
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            Undo.undoRedoPerformed -= ReloadGroups;
            EditorApplication.update -= Update;
        }

        public bool TryGetGroup(int groupId, out SelectionGroup group)
        {
            return groups.TryGetValue(groupId, out group);
        }

        public SelectionGroup CreateGroup(string name)
        {
            Undo.RecordObject(instance, "Create Group");
            var g = new SelectionGroup();
            g.groupId = _groupCounter++;
            g.name = name;
            g.color = Color.HSVToRGB(Random.value, Random.Range(0.9f, 1f), Random.Range(0.9f, 1f));
            g.showMembers = true;
            groups.Add(g.groupId, g);
            return g;
        }

        public void RemoveGroup(int groupId)
        {
            Undo.RecordObject(instance, "Remove Group");
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
                Undo.RecordObject(instance, "Duplicate Group");
                var newGroup = CreateGroup(group.name);
                newGroup.query = group.query;
                newGroup.color = group.color;
                newGroup.Add(group.ToArray());
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
