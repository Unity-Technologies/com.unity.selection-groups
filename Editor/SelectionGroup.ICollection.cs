using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;
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


        Dictionary<Scene, string[]> objectIdStrings;
        Dictionary<Scene, HashSet<Object>> sceneObjects = new Dictionary<Scene, HashSet<Object>>();

        HashSet<Object> assets = new HashSet<Object>();

        public void RefreshQueryResults()
        {
            if (query != string.Empty)
            {
                var executor = new GoQL.GoQLExecutor();
                var code = GoQL.Parser.Parse(query, out GoQL.ParseResult parseResult);
                if (parseResult == GoQL.ParseResult.OK)
                {
                    var objects = executor.Execute(code);
                    Clear();
                    System.Array.Sort(objects, (a, b) => a.name.CompareTo(b.name));
                    AddRange(objects);
                }
            }
        }

        internal bool ConvertGlobalObjectIdsToSceneObjects(Scene scene)
        {
            if (objectIdStrings.TryGetValue(scene, out string[] ids))
            {
                var objectIds = new GlobalObjectId[ids.Length];
                for (var i = 0; i < ids.Length; i++)
                    GlobalObjectId.TryParse(ids[i], out objectIds[i]);
                var objects = new Object[ids.Length];
                GlobalObjectId.GlobalObjectIdentifiersToObjectsSlow(objectIds, objects);
                var hashset = sceneObjects[scene] = new HashSet<Object>();
                for (var i = 0; i < objects.Length; i++)
                {
                    var go = objects[i];
                    //a dead global object id will result in a null object
                    if (go != null)
                    {
                        hashset.Add(go);
                    }
                }
                return true;
            }
            else
            {
                sceneObjects[scene] = new HashSet<Object>();
                return false;
            }
        }

        internal void ConvertSceneObjectsToGlobalObjectIds()
        {
            foreach (var kv in sceneObjects)
            {
                var scene = kv.Key;
                var hashset = kv.Value;
                if (scene.isLoaded)
                    ConvertSceneObjectsToGlobalObjectIds(scene);
            }
        }

        internal bool ConvertSceneObjectsToGlobalObjectIds(Scene scene)
        {
            if (sceneObjects.TryGetValue(scene, out HashSet<Object> hashset))
            {
                var objectIds = new GlobalObjectId[hashset.Count];
                var objects = hashset.ToArray();
                GlobalObjectId.GetGlobalObjectIdsSlow(objects, objectIds);
                if (objectIdStrings == null) objectIdStrings = new Dictionary<Scene, string[]>();
                objectIdStrings[scene] = (from i in objectIds select i.ToString()).ToArray();
                return true;
            }
            return false;
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