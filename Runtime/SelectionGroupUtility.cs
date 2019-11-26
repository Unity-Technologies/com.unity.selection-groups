using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;


namespace Unity.SelectionGroups
{

    public static class SelectionGroupUtility
    {
        /// <summary>
        /// Return the first group instance that matches name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static SelectionGroup GetFirstGroup(string name)
        {
            foreach (var i in SelectionGroupContainer.instanceMap.Values)
            {
                if (i.groups.TryGetValue(name, out SelectionGroup group))
                    return group;
            }
            throw new KeyNotFoundException($"No group named: {name}");
        }

        /// <summary>
        /// Return all group names.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetGroupNames()
        {
            var nameSet = new HashSet<string>();
            foreach (var i in SelectionGroupContainer.instanceMap.Values)
                nameSet.UnionWith(i.groups.Keys);
            var names = nameSet.ToList();
            names.Sort();
            return names.ToArray();
        }

        /// <summary>
        /// Return all components of type(T) that exist on members of a group.
        /// </summary>
        /// <param name="groupName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetComponents<T>(string groupName) where T : Component
        {
            foreach (var i in GetMembers(groupName))
            {
                if (i.TryGetComponent<T>(out T component))
                {
                    yield return component;
                }
            }
        }

        /// <summary>
        /// Get all members, including gameobjects and query objects from a group.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static List<GameObject> GetMembers(string groupName)
        {
            var objectSet = new HashSet<GameObject>();
            objectSet.UnionWith(GetGameObjects(groupName));
            objectSet.UnionWith(GetQueryObjects(groupName));
            var objects = (from o in objectSet where o != null select o).ToList();
            objects.Sort((a, b) => a.name.CompareTo(b.name));
            return objects;
        }

        /// <summary>
        /// Get all gameobjects (excluding query objects) from a group.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static List<GameObject> GetGameObjects(string groupName)
        {
            var objectSet = new HashSet<GameObject>();
            foreach (var i in SelectionGroupContainer.instanceMap.Values)
            {
                if (i.groups.TryGetValue(groupName, out SelectionGroup group))
                {
                    if (group != null && group.objects != null)
                        objectSet.UnionWith(group.objects);
                }
            }
            var objects = (from o in objectSet where o != null select o).ToList();
            objects.Sort((a, b) => a.name.CompareTo(b.name));
            return objects;
        }

        /// <summary>
        /// Get all query objects (excluding manually added gameobjects) from a group.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static List<GameObject> GetQueryObjects(string groupName)
        {
            var objectSet = new HashSet<GameObject>();
            foreach (var i in SelectionGroupContainer.instanceMap.Values)
            {
                if (i.groups.TryGetValue(groupName, out SelectionGroup group))
                {
                    if (group.selectionQuery.enabled)
                    {
                        if (group.queryResults == null || group.queryResults.Count == 0)
                        {
                            group.queryResults = RunSelectionQuery(group.selectionQuery);
                            i.groups[groupName] = group;
                        }
                        //query might be referencing unloaded or deleted gameobjects
                        objectSet.UnionWith(from r in @group.queryResults where r != null select r);
                    }
                }
            }
            var objects = objectSet.ToList();
            objects.Sort((a, b) => a.name.CompareTo(b.name));
            return objects;
        }

        public static HashSet<GameObject> RunSelectionQuery(DynamicSelectionQuery query)
        {
            var results = new HashSet<GameObject>();
            foreach (var i in GameObject.FindObjectsOfType<Transform>())
            {
                if (i == null || i.gameObject == null) continue;
                if (!string.IsNullOrEmpty(query.nameQuery))
                {
                    if (!i.gameObject.name.Contains(query.nameQuery)) continue;
                }
                if (query.requiredTypes.Count > 0)
                {
                    var missingTypes = false;
                    foreach (var typeName in query.requiredTypes)
                    {
                        if (!string.IsNullOrEmpty(typeName))
                        {
                            if (i.gameObject.GetComponent(typeName) == null)
                            {
                                missingTypes = true;
                                break;
                            }
                        }
                    }
                    if (missingTypes) continue;
                }
                if (query.requiredMaterials.Count > 0)
                {
                    var renderer = i.gameObject.GetComponent<Renderer>();
                    if (renderer == null) continue;
                    var requiredMaterialSet = new HashSet<Material>(query.requiredMaterials);
                    if (requiredMaterialSet.IsSubsetOf(renderer.sharedMaterials))
                        continue;
                }
                if (query.requiredShaders.Count > 0)
                {
                    var renderer = i.gameObject.GetComponent<Renderer>();
                    if (renderer == null) continue;
                    var requiredShaderSet = new HashSet<Shader>(query.requiredShaders);
                    if (!requiredShaderSet.IsSubsetOf(from m in renderer.sharedMaterials select m.shader)) continue;
                }
                results.Add(i.gameObject);
            }
            return results;
        }

    }
}
