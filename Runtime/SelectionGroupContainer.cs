using System.Collections.Generic;
using UnityEngine;

namespace Unity.SelectionGroups.Runtime
{

    /// <summary>
    /// This class is used to maintain the child set of SelectionGroups in play-mode.
    /// </summary>
    [ExecuteInEditMode]
    public class SelectionGroupContainer : MonoBehaviour
    {
        /// <summary>
        /// The set of instances of this class. There may be more than one instance if multiple scenes are loaded.
        /// </summary>
        public static HashSet<SelectionGroupContainer> instances = new HashSet<SelectionGroupContainer>();

        /// <summary>
        /// A mapping of group Id to SelectionGroup instances.
        /// </summary>
        public Dictionary<int, SelectionGroup> groups = new Dictionary<int, SelectionGroup>();

        /// <summary>
        /// Allows enumeration over all SelectionGroup instances which may be contained in multiple SelectionGroupContainer instances.
        /// </summary>
        public static IEnumerable<SelectionGroup> Groups
        {
            get
            {
                var returnedGroups = new HashSet<int>();
                foreach (var instance in instances)
                {
                    foreach (var kv in instance.groups)
                    {
                        var id = kv.Key;
                        var group = kv.Value;
                        if (!returnedGroups.Contains(id))
                        {
                            returnedGroups.Add(id);
                            yield return group;
                        }
                    }
                }
            }
        }

        void OnEnable()
        {
            instances.Add(this);
            groups.Clear();
            foreach (var i in GetComponentsInChildren<SelectionGroup>())
            {
                groups[i.groupId] = i;
            }
        }

        void OnDisable() => instances.Remove(this);

        /// <summary>
        /// Add a SelectionGroup to this container using a unique ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SelectionGroup AddGroup(int id)
        {
            var g = new GameObject("Selection Group").AddComponent<SelectionGroup>();
            g.transform.parent = transform;
            g.groupId = id;
            groups.Add(id, g);
            return g;
        }
    }
}
