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
        Dictionary<string, SelectionGroup> groups = new Dictionary<string, SelectionGroup>();

        void OnEnable()
        {
            ReloadGroups();
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosing -= OnSceneClosing;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            Undo.undoRedoPerformed -= ReloadGroups;
            Undo.undoRedoPerformed += ReloadGroups;
        }

        private void ReloadGroups()
        {
            var allGroups = groups.Values.ToArray();
            for (var i = 0; i < EditorSceneManager.loadedSceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                foreach (var g in allGroups)
                    g.LoadSceneObjects(scene);
            }
        }

        void OnHierarchyChanged()
        {
            using (var bt = new BlockTimer("SelectionGroupManager.OnHierarchyChange"))
            {
                foreach (var i in groups.Values)
                {
                    i.RefreshQueryResults();
                }
            }
        }

        private void OnSceneClosing(Scene scene, bool removingScene)
        {
            foreach (var g in groups.Values)
            {
                g.SaveSceneObjects(scene);
            }
        }

        void OnDisable()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneClosing -= OnSceneClosing;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            Undo.undoRedoPerformed -= ReloadGroups;
        }

        void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            foreach (var g in groups.Values)
            {
                g.LoadSceneObjects(scene);
            }
        }

        public bool TryGetGroup(string name, out SelectionGroup group)
        {
            return groups.TryGetValue(name, out group);
        }

        public SelectionGroup CreateGroup(string name)
        {
            var g = new SelectionGroup();
            name = UniqueName(name);
            g.name = name;
            groups.Add(name, g);
            return g;
        }

        public void RemoveGroup(string name)
        {
            groups.Remove(name);
        }

        public string[] GetGroupNames()
        {
            return groups.Keys.ToArray();
        }

        string UniqueName(string name)
        {
            if (!groups.ContainsKey(name)) return name;
            var i = 1;
            if (Regex.IsMatch(name, @" \(\d\)$"))
            {
                name = name.Substring(0, name.Length - 4);
            }
            while (true)
            {
                var newName = $"{name} ({i})";
                if (!groups.ContainsKey(newName))
                {
                    return newName;
                }
                else
                {
                    i += 1;
                }
            }
        }

        public void DuplicateGroup(string groupName)
        {
            if (TryGetGroup(groupName, out SelectionGroup group))
            {
                var newGroup = CreateGroup(groupName);
                newGroup.AddRange(group);
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
