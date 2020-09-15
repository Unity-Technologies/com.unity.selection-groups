using System;
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
    internal class PersistentReferenceCollection : ISerializationCallbackReceiver, System.IDisposable, IEnumerable<UnityEngine.Object>
    {
        OrderedSet<UnityEngine.Object> activeObjects = new OrderedSet<UnityEngine.Object>();
        Dictionary<int, GlobalObjectId> instanceIdMap = new Dictionary<int, GlobalObjectId>();
        HashSet<GlobalObjectId> globalObjectIdSet = new HashSet<GlobalObjectId>();

        [SerializeField] string[] _objectIds;

        public int LoadedObjectCount => activeObjects.Count;
        public int TotalObjectCount => globalObjectIdSet.Count;

        public UnityEngine.Object this[int index] { get => activeObjects[index]; set => activeObjects[index] = value; }

        /// <summary>
        /// Load references to objects that currently exist in a scene.
        /// </summary>
        /// <param name="forceReload"></param>
        public void LoadObjects(bool forceReload = false)
        {
            if (LoadedObjectCount == 0 || forceReload)
                ConvertGlobalObjectIdsToSceneObjects();
        }

        public void Add(UnityEngine.Object obj)
        {
            activeObjects.Add(obj);
            ConvertSceneObjectsToGlobalObjectIds();
        }

        public void Add(UnityEngine.Object[] objects)
        {
            activeObjects.AddRange(objects);
            ConvertSceneObjectsToGlobalObjectIds();
        }

        public void Remove(UnityEngine.Object obj)
        {
            activeObjects.Remove(obj);
            var id = obj.GetInstanceID();
            var gid = instanceIdMap[id];
            instanceIdMap.Remove(id);
            globalObjectIdSet.Remove(gid);
        }

        public void Remove(UnityEngine.Object[] objects)
        {
            activeObjects.Remove(objects);
            foreach (var obj in objects)
            {
                var id = obj.GetInstanceID();
                if (instanceIdMap.TryGetValue(id, out var gid))
                {
                    instanceIdMap.Remove(id);
                    globalObjectIdSet.Remove(gid);
                }
            }
        }

        public PersistentReferenceCollection()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosing -= OnSceneClosing;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            EditorSceneManager.sceneClosed -= OnSceneClosed;
            EditorSceneManager.sceneClosed += OnSceneClosed;
            // EditorSceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void OnSceneClosed(Scene scene)
        {
            EditorApplication.delayCall += ConvertGlobalObjectIdsToSceneObjects;
        }

        void OnSceneClosing(Scene scene, bool removingScene)
        {
            ConvertSceneObjectsToGlobalObjectIds();
        }

        void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            ConvertGlobalObjectIdsToSceneObjects();
        }

        public void Dispose()
        {
            EditorSceneManager.sceneClosing -= OnSceneClosing;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
        }

        internal void ConvertSceneObjectsToGlobalObjectIds()
        {
            var objects = activeObjects.ToArray();
            var gids = GetGlobalObjectIds(objects);
            globalObjectIdSet.UnionWith(gids);
            for (var i = 0; i < objects.Length; i++)
                if (objects[i] != null)
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

        public IEnumerator<UnityEngine.Object> GetEnumerator()
        {
            return activeObjects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return activeObjects.GetEnumerator();
        }
    }
}
