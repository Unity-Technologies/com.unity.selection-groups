using System.Collections.Generic;
using UnityEngine;

namespace Unity.SelectionGroups.Runtime
{
    [ExecuteInEditMode]
    public class SelectionGroupContainer : MonoBehaviour
    {
        public static HashSet<SelectionGroupContainer> instances = new HashSet<SelectionGroupContainer>();
        public Dictionary<int, SelectionGroup> groups = new Dictionary<int, SelectionGroup>();

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
