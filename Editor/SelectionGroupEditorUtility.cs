using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.SelectionGroups
{
    public static class SelectionGroupEditorUtility
    {
        public static void ShowGroup(string groupName)
        {
            SceneVisibilityManager.instance.Show(SelectionGroupUtility.GetMembers(groupName).ToArray(), false);
        }

        public static void HideGroup(string groupName)
        {
            SceneVisibilityManager.instance.Hide(SelectionGroupUtility.GetMembers(groupName).ToArray(), false);
        }

        public static void LockGroup(string groupName)
        {
            foreach (var go in SelectionGroupUtility.GetMembers(groupName))
                go.hideFlags |= HideFlags.NotEditable;
        }

        public static void UnlockGroup(string groupName)
        {
            foreach (var go in SelectionGroupUtility.GetMembers(groupName))
                go.hideFlags &= ~HideFlags.NotEditable;
        }

        public static void RecordUndo(string msg)
        {
            var objects = SelectionGroupContainer.instanceMap.Values.ToArray();
            foreach (var i in objects)
            {
                Undo.RegisterFullObjectHierarchyUndo(i, msg);
                if (i.groups != null)
                    Undo.RegisterCompleteObjectUndo(i.groups.Values.ToArray(), msg);
            }
        }

        /// <summary>
        /// Delete a group specified by name.
        /// </summary>
        /// <param name="groupName"></param>
        public static void RemoveGroup(string groupName)
        {
            // var undoId = Undo.GetCurrentGroup();
            foreach (var i in SelectionGroupContainer.instanceMap.Values)
            {
                var sg = i.groups[groupName];
                Undo.RegisterCompleteObjectUndo(i, "Destroy Object");
                i.groups.Remove(groupName);
                Undo.DestroyObjectImmediate(sg.gameObject);
            }
            // Undo.CollapseUndoOperations(undoId);
        }

        public static bool AreAnyMembersLocked(string groupName)
        {
            foreach (var go in SelectionGroupUtility.GetMembers(groupName))
            {
                if (go.hideFlags.HasFlag(HideFlags.NotEditable))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool AreAnyMembersHidden(string groupName)
        {
            foreach (var go in SelectionGroupUtility.GetMembers(groupName))
            {
                if (SceneVisibilityManager.instance.IsHidden(go))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Create a new group with specified name. If name is not unique, it will have a numeric suffix appended.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string CreateNewGroup(string name)
        {
            var actualName = SelectionGroupEditorUtility.CreateUniqueName(name);
            var color = Color.HSVToRGB(Random.value, Random.Range(0.7f, 1f), Random.Range(0.7f, 1f));
            foreach (var i in SelectionGroupContainer.instanceMap.Values)
            {
                Create(i, actualName, color, showMembers: true);
            }
            return actualName;
        }

        internal static SelectionGroup Create(SelectionGroupContainer sgc, string groupName, Color color, bool showMembers)
        {
            var g = new GameObject(groupName).AddComponent<SelectionGroup>();
            Undo.RegisterCreatedObjectUndo(g.gameObject, "Create Group");
            g.color = color;
            g.showMembers = showMembers;
            g.objects = new HashSet<GameObject>();
            sgc.groups[groupName] = g;
            g.transform.parent = sgc.transform;
            return g;
        }

        internal static SelectionGroup Create(SelectionGroupContainer sgc, string groupName)
        {
            var color = Color.HSVToRGB(Random.value, Random.Range(0.7f, 1f), Random.Range(0.7f, 1f));
            return Create(sgc, groupName, color, showMembers: true);
        }

        public static string CreateUniqueName(string name)
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
            foreach (var i in SelectionGroupUtility.GetGameObjects(groupName))
                AddObjectToGroup(i, actualName);
            var g = SelectionGroupUtility.GetFirstGroup(groupName);
            var n = SelectionGroupUtility.GetFirstGroup(actualName);
            n.selectionQuery = g.selectionQuery;
            UpdateGroup(actualName, n);
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
                        existingGroup.queryResults = SelectionGroupUtility.RunSelectionQuery(existingGroup.selectionQuery);
                    else
                        existingGroup.queryResults = null;
                    i.groups[group.name] = existingGroup;
                }
            }
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
            var newGroup = SelectionGroupEditorUtility.Create(container, groupName);
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

    }
}
