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
        /// Copy the fields from group into the Selection Group specified by name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="group"></param>
        public static void UpdateGroup(string name, SelectionGroup group)
        {
            foreach (var i in SelectionGroupContainer.instanceMap.Values)
            {
                if (i.groups.TryGetValue(name, out SelectionGroup existingGroup))
                {
                    i.groups.Remove(name);
                    existingGroup.name = group.name;
                    existingGroup.color = group.color;
                    existingGroup.selectionQuery = group.selectionQuery;
                    existingGroup.showMembers = group.showMembers;
                    existingGroup.mutability = group.mutability;
                    existingGroup.visibility = group.visibility;
                    if (existingGroup.selectionQuery.enabled)
                        existingGroup.queryResults = RunSelectionQuery(existingGroup.selectionQuery);
                    else
                        existingGroup.queryResults = null;
                    i.groups[group.name] = existingGroup;
                }
            }
        }

        /// <summary>
        /// Create a new group with specified name. If name is not unique, it will have a numeric suffix appended.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string CreateNewGroup(string name)
        {
            var actualName = CreateUniqueName(name);
            var color = Color.HSVToRGB(Random.value, Random.Range(0.7f, 1f), Random.Range(0.7f, 1f));
            foreach (var i in SelectionGroupContainer.instanceMap.Values)
            {
                i.Create(actualName, color, showMembers: true);
            }
            return actualName;
        }

        static string CreateUniqueName(string name)
        {
            var actualName = name;
            var count = 1;
            while (true)
            {
                if (IsUniqueGroupName(actualName))
                {
                    break;
                }
                else
                {
                    actualName = $"{name} {count}";
                    count += 1;
                }
            }
            return actualName;
        }

        static bool IsUniqueGroupName(string groupName)
        {
            foreach (var i in SelectionGroupContainer.instanceMap.Values)
            {
                if (i.groups.ContainsKey(groupName))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Duplicate and a new group, giving the new group a new unique name.
        /// </summary>
        /// <param name="groupName"></param>
        public static void DuplicateGroup(string groupName)
        {
            var actualName = CreateNewGroup(groupName);
            foreach (var i in GetGameObjects(groupName))
                AddObjectToGroup(i, actualName);
            var g = GetFirstGroup(groupName);
            var n = GetFirstGroup(actualName);
            n.selectionQuery = g.selectionQuery;
            UpdateGroup(actualName, n);
        }

        /// <summary>
        /// Change the name of a group.
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="newName"></param>
        public static void RenameGroup(string groupName, string newName)
        {
            foreach (var i in SelectionGroupContainer.instanceMap.Values)
            {
                if (i.groups.TryGetValue(groupName, out SelectionGroup group))
                {
                    i.groups.Remove(groupName);
                    group.name = newName;
                    i.groups.Add(newName, group);
                }
            }
        }

        /// <summary>
        /// Delete a group specified by name.
        /// </summary>
        /// <param name="groupName"></param>
        public static void RemoveGroup(string groupName)
        {
            foreach (var i in SelectionGroupContainer.instanceMap.Values)
            {
                i.groups.Remove(groupName);
            }
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
        /// Add gameobjects to a group.
        /// </summary>
        /// <param name="gameObjects"></param>
        /// <param name="groupName"></param>
        public static void AddObjectToGroup(IList<Object> gameObjects, string groupName)
        {
            foreach (var g in gameObjects)
                if (g is GameObject && g != null)
                    AddObjectToGroup((GameObject)g, groupName);
        }

        /// <summary>
        /// Add a single gameobject to a group.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="groupName"></param>
        public static void AddObjectToGroup(GameObject gameObject, string groupName)
        {
            //Get the container from the appropriate scene
            if (!SelectionGroupContainer.instanceMap.TryGetValue(gameObject.scene, out SelectionGroupContainer container))
            {
                //if it doesn't exist, create it in the appropriate scene.
                var scene = SceneManager.GetActiveScene();
                SceneManager.SetActiveScene(gameObject.scene);
                container = new GameObject("Hidden_SelectionGroups").AddComponent<SelectionGroupContainer>();
                // container.gameObject.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                SceneManager.SetActiveScene(scene);
            }
            //at this point, container references the correct instance in the correct scene.
            if (container.groups.TryGetValue(groupName, out SelectionGroup group))
            {
                group.objects.Add(gameObject);
                return;
            }

            //at this point, it appears the group does not exist in this particular container, so we
            //create it and add the gameObject into it.
            var newGroup = container.Create(groupName);
            newGroup.objects.Add(gameObject);
        }

        /// <summary>
        /// Remove a single object from a group.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="groupName"></param>
        public static void RemoveObjectFromGroup(GameObject gameObject, string groupName)
        {
            if (!SelectionGroupContainer.instanceMap.TryGetValue(gameObject.scene, out SelectionGroupContainer container))
                return;

            foreach (var i in SelectionGroupContainer.instanceMap.Values)
            {
                if (i.groups.TryGetValue(groupName, out SelectionGroup group))
                {
                    group.objects.Remove(gameObject);
                }
            }
        }

        /// <summary>
        /// Remove gameobjects from a group.
        /// </summary>
        /// <param name="gameObjects"></param>
        /// <param name="groupName"></param>
        public static void RemoveObjectsFromGroup(IEnumerable<GameObject> gameObjects, string groupName)
        {
            foreach (var i in SelectionGroupContainer.instanceMap.Values)
            {
                if (i.groups.TryGetValue(groupName, out SelectionGroup group))
                {
                    group.objects.ExceptWith(gameObjects);
                }
            }
        }

        /// <summary>
        /// Remove all gameobjects from a group.
        /// </summary>
        /// <param name="groupName"></param>
        public static void ClearGroup(string groupName)
        {
            foreach (var i in SelectionGroupContainer.instanceMap.Values)
            {
                if (i.groups.TryGetValue(groupName, out SelectionGroup group))
                {
                    group.objects.Clear();
                }
            }
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

        static HashSet<GameObject> RunSelectionQuery(DynamicSelectionQuery query)
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
