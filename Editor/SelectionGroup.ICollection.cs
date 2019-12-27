using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.SelectionGroups
{
    [System.Serializable]
    public partial class SelectionGroup : ICollection<Object>
    {
        public string name;
        public Color color;
        public bool showMembers;
        public string query = string.Empty;
        public bool sort = false;

        public Dictionary<Scene, string[]> objectIdStrings;
        public Dictionary<Scene, HashSet<Object>> sceneObjects = new Dictionary<Scene, HashSet<Object>>();
        public HashSet<Object> assets = new HashSet<Object>();
        public int groupId;

        GoQL.GoQLExecutor executor = new GoQL.GoQLExecutor();

        public void RefreshQueryResults()
        {
            if (query != string.Empty)
            {
                executor.Code = query;
                var objects = executor.Execute();
                Clear();
                if (sort)
                    System.Array.Sort(objects, (a, b) => a.name.CompareTo(b.name));
                AddRange(objects);
            }
        }

        internal bool AreSceneObjectsLoaded(Scene scene)
        {
            return sceneObjects.ContainsKey(scene);
        }

        public int Count
        {
            get
            {
                var count = assets == null ? 0 : assets.Count;
                foreach (var kv in sceneObjects)
                {
                    var scene = kv.Key;
                    var objects = kv.Value;
                    if (scene.isLoaded)
                        count += objects.Count;
                }
                return count;
            }
        }

        public void AddRange(ICollection<Object> objects)
        {
            foreach (var i in objects) Add(i);
        }

        public bool IsReadOnly => false;

        public void Add(Object obj)
        {
            if (obj is GameObject)
            {
                var gameObject = (GameObject)obj;
                var scene = gameObject.scene;
                AddSceneObject(scene, obj);
            }
            else if (obj is Component)
            {
                var component = (Component)obj;
                var scene = component.gameObject.scene;
                AddSceneObject(scene, obj);
            }
            else
                AddAsset(obj);
        }

        public IEnumerable<Object> EnumerateObjects()
        {
            foreach (var kv in sceneObjects)
            {
                var scene = kv.Key;
                var objects = kv.Value;
                if (scene.isLoaded)
                {
                    if (objects != null)
                        foreach (var i in objects) yield return i;
                }
                if (assets != null)
                    foreach (var i in assets)
                        yield return i;
            }
        }

        void AddAsset(Object obj)
        {
            if (assets == null) assets = new HashSet<Object>();
            assets.Add(obj);
        }

        void AddSceneObject(Scene scene, Object obj)
        {
            if (scene != null)
            {
                if (!sceneObjects.TryGetValue(scene, out HashSet<Object> items))
                    items = sceneObjects[scene] = new HashSet<Object>();
                items.Add(obj);
            }
        }

        bool RemoveAsset(Object obj)
        {
            return assets.Remove(obj);
        }

        bool RemoveSceneObject(Scene scene, Object obj)
        {
            if (sceneObjects.TryGetValue(scene, out HashSet<Object> items))
            {
                return items.Remove(obj);
            }
            return false;
        }

        public void Clear()
        {
            assets.Clear();
            foreach (var kv in sceneObjects)
            {
                var scene = kv.Key;
                var objects = kv.Value;
                if (scene.isLoaded)
                    objects.Clear();
            }
        }

        public bool Contains(Object item)
        {
            if (assets.Contains(item)) return true;
            foreach (var kv in sceneObjects)
            {
                var scene = kv.Key;
                var objects = kv.Value;
                if (scene.isLoaded && objects.Contains(item))
                    return true;
            }
            return false;
        }

        public void CopyTo(Object[] array, int arrayIndex)
        {
            var index = arrayIndex;
            foreach (var kv in sceneObjects)
            {
                var scene = kv.Key;
                var objects = kv.Value;
                if (scene.isLoaded)
                {
                    objects.CopyTo(array, index);
                    index += objects.Count;
                }
            }
            assets.CopyTo(array, index);
        }

        public bool Remove(Object obj)
        {
            if (obj is GameObject)
            {
                var gameObject = (GameObject)obj;
                var scene = gameObject.scene;
                return RemoveSceneObject(scene, obj);
            }
            else if (obj is Component)
            {
                var component = (Component)obj;
                var scene = component.gameObject.scene;
                return RemoveSceneObject(scene, obj);
            }
            else
                return RemoveAsset(obj);
        }

        public void Remove(Object[] objects)
        {
            foreach (var obj in objects)
            {
                Remove(obj);
            }
        }

        public IEnumerator<Object> GetEnumerator()
        {
            foreach (var i in EnumerateObjects())
                yield return i;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var i in EnumerateObjects())
                yield return i;
        }
    }
}