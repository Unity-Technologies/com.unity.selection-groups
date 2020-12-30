using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.SelectionGroups;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Unity.SelectionGroupsEditor
{
    /// <summary>
    /// This class contains a set of global object id values. When a scene is loaded, they are
    /// converted (where they exist) into scene objects. When a scene is unloaded, a global object id
    /// is generated for any new gameobjects added to the set.
    /// </summary>
    [System.Serializable]
    internal class PersistentReferenceCollection : ISerializationCallbackReceiver, IDisposable, IEnumerable<Object>
    {
        OrderedSet<UnityEngine.Object> activeObjects = new OrderedSet<Object>();
        HashSet<GlobalObjectId> globalObjectIdSet = new HashSet<GlobalObjectId>();

        [SerializeField] string[] _objectIds;

        public int LoadedObjectCount => activeObjects.Count;
        public int TotalObjectCount => globalObjectIdSet.Count;

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

        public void Add(IList<Object> objects)
        {
            activeObjects.AddRange(objects);
            ConvertSceneObjectsToGlobalObjectIds();
        }

        public bool Update(UnityEngine.Object[] objects)
        {
            var newObjects = new HashSet<UnityEngine.Object>(objects);
            newObjects.ExceptWith(activeObjects);
            if (newObjects.Count > 0)
            {
                Clear();
                activeObjects.AddRange(objects);
                return true;
            }
            return false;
        }

        public void Remove(UnityEngine.Object obj)
        {
            activeObjects.Remove(obj);
            var gid = GlobalObjectId.GetGlobalObjectIdSlow(obj);
            globalObjectIdSet.Remove(gid);
        }

        public void Remove(IList<Object> objects)
        {
            activeObjects.Remove(objects);
            foreach (var gid in GetGlobalObjectIds(objects))
            {
                globalObjectIdSet.Remove(gid);
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
        }

        internal void Clear()
        {
            globalObjectIdSet.Clear();
            activeObjects.Clear();
        }

        internal void ConvertGlobalObjectIdsToSceneObjects()
        {
            activeObjects.Clear();
            var gids = globalObjectIdSet.ToArray();
            var outputObjects = new UnityEngine.Object[gids.Length];
            GlobalObjectId.GlobalObjectIdentifiersToObjectsSlow(gids, outputObjects);
            for (var i = 0; i < outputObjects.Length; i++)
            {
                var obj = outputObjects[i];
                //Debug.Log($"GID: {gids[i]}, OBJ: {obj} -> GUID: {GlobalObjectId.GetGlobalObjectIdSlow(obj)}");
                //Sometimes invalid objects are returned. https://fogbugz.unity3d.com/f/cases/1291291/
                //This is the workaround.
                if (obj != null)
                {
                    //if(gids[i] == GlobalObjectId.GetGlobalObjectIdSlow(obj))
                    var x = gids[i];
                    var y = GlobalObjectId.GetGlobalObjectIdSlow(obj);
                    if(y.identifierType == x.identifierType && y.targetObjectId == x.targetObjectId && y.targetPrefabId == x.targetPrefabId && y.assetGUID == x.assetGUID)
                        activeObjects.Add(obj);
                }
            }
        }

        internal GlobalObjectId[] GetGlobalObjectIds(IList<Object> objects)
        {
            var gids = new GlobalObjectId[objects.Count];
            GlobalObjectId.GetGlobalObjectIdsSlow(objects.ToArray(), gids);
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
