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
    /// <summary>
    /// The Editor only manager for selection groups.
    /// </summary>
    public partial class SelectionGroupManager : ScriptableObject, IEnumerable<SelectionGroup>
    {
        Dictionary<int, SelectionGroup> groups = new Dictionary<int, SelectionGroup>();
        [SerializeField] int _groupCounter;
        [SerializeField] bool isDirty = true;
        [SerializeField] internal bool enablePlayModeSelectionGroups = false;

        Dictionary<string, Scene> loadedScenes = new Dictionary<string, Scene>();

        internal SelectionGroup GetGroup(int groupId) => groups[groupId];

        internal void SetIsDirty()
        {
            UpdateSelectionGroupContainersInLoadedScenes();
            isDirty = true;
        }

        void OnEnable()
        {
            Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.Full);
            ReloadGroups();
            // EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            // EditorApplication.hierarchyChanged += OnHierarchyChanged;
            Undo.undoRedoPerformed -= ReloadGroups;
            Undo.undoRedoPerformed += ReloadGroups;
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
            SetIsDirty();
        }

        internal void UpdateSelectionGroupContainers()
        {
            if (enablePlayModeSelectionGroups)
            {
                AddContainersToLoadedScenes();
            }
            else
            {
                RemoveContainersFromLoadedScenes();
            }
        }

        void RemoveContainersFromLoadedScenes()
        {
            for (var i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                var objects = scene.GetRootGameObjects();
                foreach (var j in objects)
                {
                    if (j.TryGetComponent<Runtime.SelectionGroupContainer>(out Runtime.SelectionGroupContainer container))
                    {
                        DestroyImmediate(j.gameObject);
                        break;
                    }
                }
            }
        }

        void AddContainersToLoadedScenes()
        {
            for (var i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                var objects = scene.GetRootGameObjects();
                var hasContainer = false;
                foreach (var j in objects)
                {
                    if (j.TryGetComponent<Runtime.SelectionGroupContainer>(out Runtime.SelectionGroupContainer container))
                    {
                        hasContainer = true;
                        break;
                    }
                }
                if (!hasContainer)
                {
                    var container = new GameObject("Selection Groups").AddComponent<Runtime.SelectionGroupContainer>();
                    EditorSceneManager.MoveGameObjectToScene(container.gameObject, scene);
                }
            }
            UpdateSelectionGroupContainersInLoadedScenes();
        }

        void UpdateSelectionGroupContainersInLoadedScenes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (enablePlayModeSelectionGroups)
            {

            }
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
                    foreach (var i in group)
                    {
                        var go = i as GameObject;
                        if (go == null) continue;
                        var scene = go.scene;
                        if (go.scene == null) continue;
                        if (go != null && go.scene != runtimeGroup.gameObject.scene) continue;
                        runtimeGroup.members.Add(i);
                    }
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

        void ReloadGroups()
        {
            foreach (var g in groups)
            {
                g.Value.Reload();
            }
        }

        void OnHierarchyChanged() => SetIsDirty();

        void Update()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
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
                Save();
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

        bool TryGetGroup(int groupId, out SelectionGroup group)
        {
            return groups.TryGetValue(groupId, out group);
        }

        internal SelectionGroup CreateGroup(string name)
        {
            Undo.RecordObject(instance, "Create Group");
            var g = new SelectionGroup();
            g.groupId = _groupCounter++;
            g.name = name;
            g.color = Color.HSVToRGB(Random.value, Random.Range(0.9f, 1f), Random.Range(0.9f, 1f));
            g.showMembers = true;
            groups.Add(g.groupId, g);
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
            return (from i in groups.Values select i.name).ToArray();
        }

        /// <summary>
        /// Duplicate the group specified by groupId.
        /// </summary>
        /// <param name="groupId"></param>
        public void DuplicateGroup(int groupId)
        {
            if (TryGetGroup(groupId, out SelectionGroup group))
            {
                Undo.RecordObject(instance, "Duplicate Group");
                var newGroup = CreateGroup(group.name);
                newGroup.query = group.query;
                newGroup.color = group.color;
                newGroup.Add(group.ToArray());
                SetIsDirty();
            }
        }

        /// <summary>
        /// Enumerate over all selection groups in this manager.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<SelectionGroup> GetEnumerator()
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
    }
}
