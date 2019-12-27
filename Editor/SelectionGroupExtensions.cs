using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.SelectionGroups
{
    public static class SelectionGroupExtensions
    {
        static internal bool ConvertGlobalObjectIdsToSceneObjects(this SelectionGroup group, Scene scene)
        {
            if (group.objectIdStrings.TryGetValue(scene, out string[] ids))
            {
                var objectIds = new GlobalObjectId[ids.Length];
                for (var i = 0; i < ids.Length; i++)
                    GlobalObjectId.TryParse(ids[i], out objectIds[i]);
                var objects = new Object[ids.Length];
                GlobalObjectId.GlobalObjectIdentifiersToObjectsSlow(objectIds, objects);
                var hashset = group.sceneObjects[scene] = new HashSet<Object>();
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
                group.sceneObjects[scene] = new HashSet<Object>();
                return false;
            }
        }

        static internal void ConvertSceneObjectsToGlobalObjectIds(this SelectionGroup group)
        {
            foreach (var kv in group.sceneObjects)
            {
                var scene = kv.Key;
                var hashset = kv.Value;
                if (scene.isLoaded)
                    ConvertSceneObjectsToGlobalObjectIds(group, scene);
            }
        }

        static internal bool ConvertSceneObjectsToGlobalObjectIds(this SelectionGroup group, Scene scene)
        {
            if (group.sceneObjects.TryGetValue(scene, out HashSet<Object> hashset))
            {
                var objectIds = new GlobalObjectId[hashset.Count];
                var objects = hashset.ToArray();
                GlobalObjectId.GetGlobalObjectIdsSlow(objects, objectIds);
                if (group.objectIdStrings == null) group.objectIdStrings = new Dictionary<Scene, string[]>();
                group.objectIdStrings[scene] = (from i in objectIds select i.ToString()).ToArray();
                return true;
            }
            return false;
        }
    }
}
