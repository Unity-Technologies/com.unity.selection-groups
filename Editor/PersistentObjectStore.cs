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
    public class PersistentObjectStore : ISerializationCallbackReceiver, System.IDisposable
    {
        internal OrderedSet<UnityEngine.Object> activeObjects = new OrderedSet<UnityEngine.Object>();
        Dictionary<int, GlobalObjectId> instanceIdMap = new Dictionary<int, GlobalObjectId>();
        HashSet<GlobalObjectId> globalObjectIdSet = new HashSet<GlobalObjectId>();

        [SerializeField] internal string[] _objectIds;

        bool isDirty = false;

        public int LoadedObjectCount => activeObjects.Count;
        public int TotalObjectCount
        {
            get
            {
                if (isDirty) ConvertSceneObjectsToGlobalObjectIds();
                return globalObjectIdSet.Count;
            }
        }

        public Object this[int index] { get => activeObjects[index]; set => activeObjects[index] = value; }

        public void LoadObjects(bool forceReload = false)
        {
            if (LoadedObjectCount == 0 || forceReload)
                ConvertGlobalObjectIdsToSceneObjects();
        }

        public void Add(Object obj)
        {
            activeObjects.Add(obj);
            isDirty = true;
        }

        public void Add(Object[] objects)
        {
            activeObjects.AddRange(objects);
            isDirty = true;
        }

        public void Remove(Object obj)
        {
            if (isDirty)
                ConvertSceneObjectsToGlobalObjectIds();
            activeObjects.Remove(obj);
            var id = obj.GetInstanceID();
            var gid = instanceIdMap[id];
            instanceIdMap.Remove(id);
            globalObjectIdSet.Remove(gid);
        }

        public void Remove(Object[] objects)
        {
            if (isDirty)
                ConvertSceneObjectsToGlobalObjectIds();
            activeObjects.Remove(objects);
            foreach (var obj in objects)
            {
                var id = obj.GetInstanceID();
                var gid = instanceIdMap[id];
                instanceIdMap.Remove(id);
                globalObjectIdSet.Remove(gid);
            }
        }


        public PersistentObjectStore()
        {
            EditorSceneManager.sceneLoaded -= OnSceneLoaded;
            EditorSceneManager.sceneLoaded += OnSceneLoaded;
            EditorSceneManager.sceneClosing -= OnSceneClosing;
            EditorSceneManager.sceneClosing += OnSceneClosing;
        }

        void OnSceneClosing(Scene scene, bool removingScene)
        {
            if (isDirty)
                ConvertSceneObjectsToGlobalObjectIds();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ConvertGlobalObjectIdsToSceneObjects();
        }

        public void Dispose()
        {
            if (isDirty)
                ConvertSceneObjectsToGlobalObjectIds();
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
            isDirty = false;
        }

        internal void Clear()
        {
            globalObjectIdSet.Clear();
            activeObjects.Clear();
            instanceIdMap.Clear();
        }

        internal void ConvertGlobalObjectIdsToSceneObjects()
        {
            var outputObjects = new UnityEngine.Object[globalObjectIdSet.Count];
            var gids = globalObjectIdSet.ToArray();
            GlobalObjectId.GlobalObjectIdentifiersToObjectsSlow(gids, outputObjects);
            activeObjects.Clear();
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


    }
}
