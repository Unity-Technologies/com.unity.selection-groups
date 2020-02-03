using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.SelectionGroups
{
    /// <summary>
    /// This class contains a set of global object id values. When a scene is loaded, they are
    /// converted (where they exist) into scene objects. When a scene is unloaded, a global object id
    /// is generated for any new gameobjects added to the set.
    /// </summary>
    [System.Serializable]
    public class PersistentReferenceCollection : ISerializationCallbackReceiver, System.IDisposable, IEnumerable<Object>
    {
        OrderedSet<UnityEngine.Object> activeObjects = new OrderedSet<UnityEngine.Object>();
        Dictionary<int, GlobalObjectId> instanceIdMap = new Dictionary<int, GlobalObjectId>();
        HashSet<GlobalObjectId> globalObjectIdSet = new HashSet<GlobalObjectId>();

        [SerializeField] string[] _objectIds;

        public int LoadedObjectCount => activeObjects.Count;
        public int TotalObjectCount => globalObjectIdSet.Count;

        public Object this[int index] { get => activeObjects[index]; set => activeObjects[index] = value; }

        /// <summary>
        /// Load references to objects that currently exist in a scene.
        /// </summary>
        /// <param name="forceReload"></param>
        public void LoadObjects(bool forceReload = false)
        {
            if (LoadedObjectCount == 0 || forceReload)
                ConvertGlobalObjectIdsToSceneObjects();
        }

        public void Add(Object obj)
        {
            activeObjects.Add(obj);
            ConvertSceneObjectsToGlobalObjectIds();
        }

        public void Add(Object[] objects)
        {
            activeObjects.AddRange(objects);
            ConvertSceneObjectsToGlobalObjectIds();
        }

        public void Remove(Object obj)
        {
            activeObjects.Remove(obj);
            var id = obj.GetInstanceID();
            var gid = instanceIdMap[id];
            instanceIdMap.Remove(id);
            globalObjectIdSet.Remove(gid);
        }

        public void Remove(Object[] objects)
        {
            activeObjects.Remove(objects);
            foreach (var obj in objects)
            {
                var id = obj.GetInstanceID();
                var gid = instanceIdMap[id];
                instanceIdMap.Remove(id);
                globalObjectIdSet.Remove(gid);
            }
        }

        public PersistentReferenceCollection()
        {
            EditorSceneManager.sceneLoaded -= OnSceneLoaded;
            EditorSceneManager.sceneLoaded += OnSceneLoaded;
            EditorSceneManager.sceneClosing -= OnSceneClosing;
            EditorSceneManager.sceneClosing += OnSceneClosing;
        }

        void OnSceneClosing(Scene scene, bool removingScene)
        {
            ConvertSceneObjectsToGlobalObjectIds();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ConvertGlobalObjectIdsToSceneObjects();
        }

        public void Dispose()
        {
            EditorSceneManager.sceneClosing -= OnSceneClosing;
            EditorSceneManager.sceneLoaded -= OnSceneLoaded;
        }

        internal void ConvertSceneObjectsToGlobalObjectIds()
        {
            var objects = activeObjects.ToArray();
            var gids = GetGlobalObjectIds(objects);
            globalObjectIdSet.UnionWith(gids);
            for (var i = 0; i < objects.Length; i++)
                instanceIdMap[objects[i].GetInstanceID()] = gids[i];
        }

        internal void Clear()
        {
            globalObjectIdSet.Clear();
            activeObjects.Clear();
            instanceIdMap.Clear();
        }

        internal void ConvertGlobalObjectIdsToSceneObjects()
        {
            activeObjects.Clear();
            instanceIdMap.Clear();
            var outputObjects = new UnityEngine.Object[globalObjectIdSet.Count];
            var gids = globalObjectIdSet.ToArray();
            GlobalObjectId.GlobalObjectIdentifiersToObjectsSlow(gids, outputObjects);
            for (var i = 0; i < outputObjects.Length; i++)
            {
                var obj = outputObjects[i];
                if (obj != null)
                {
                    activeObjects.Add(obj);
                    instanceIdMap[obj.GetInstanceID()] = gids[i];
                }
            }
        }

        internal GlobalObjectId[] GetGlobalObjectIds(params UnityEngine.Object[] gameObjects)
        {
            var gids = new GlobalObjectId[gameObjects.Length];
            GlobalObjectId.GetGlobalObjectIdsSlow(gameObjects, gids);
            return gids;
        }

        public void OnAfterDeserialize()
        {
            var ids = new GlobalObjectId[_objectIds.Length];
            for (var i = 0; i < _objectIds.Length; i++)
                if (GlobalObjectId.TryParse(_objectIds[i], out ids[i]))
                    globalObjectIdSet.Add(ids[i]);
        }

        public void OnBeforeSerialize()
        {
            _objectIds = (from i in globalObjectIdSet select i.ToString()).ToArray();
        }

        public IEnumerator<Object> GetEnumerator()
        {
            return activeObjects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return activeObjects.GetEnumerator();
        }
    }
}
