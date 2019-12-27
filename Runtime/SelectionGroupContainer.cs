using System.Collections.Generic;
using UnityEngine;

namespace Unity.SelectionGroups.Runtime
{
    [ExecuteInEditMode]
    public class SelectionGroupContainer : MonoBehaviour
    {
        public static HashSet<SelectionGroupContainer> instances = new HashSet<SelectionGroupContainer>();

        public Dictionary<int, SelectionGroup> groups = new Dictionary<int, SelectionGroup>();

        void OnEnable()
        {
            instances.Add(this);
            groups.Clear();
            foreach(var i in GetComponentsInChildren<SelectionGroup>()) {
                groups.Add(i.groupId, i);
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
