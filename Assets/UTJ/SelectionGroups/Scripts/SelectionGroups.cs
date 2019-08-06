using System.Collections.Generic;
using UnityEngine;

namespace Utj.Film
{
    public class SelectionGroups : MonoBehaviour
    {
        public List<SelectionGroup> groups = new List<SelectionGroup>();

        static SelectionGroups selectionGroups;
        public static SelectionGroups Instance
        {
            get
            {
                selectionGroups = GameObject.FindObjectOfType<SelectionGroups>();
                if (selectionGroups == null)
                {
                    var g = new GameObject("Hidden_SelectionGroups");
                    g.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                    selectionGroups = g.AddComponent<SelectionGroups>();
                }
                return selectionGroups;
            }
        }

        public void FetchObjects(int index, List<Object> objects)
        {
            if (groups[index].objects != null)
                objects.AddRange(groups[index].objects);
        }

        public void RemoveObjects(int index, Object[] objects)
        {
            var group = groups[index].objects;
            var uniqueObjects = new HashSet<Object>(group);
            uniqueObjects.ExceptWith(objects);
            group.Clear();
            group.AddRange(uniqueObjects);
        }

        public void AddObjects(int index, Object[] objects)
        {
            var group = groups[index].objects;
            var uniqueObjects = new HashSet<Object>(group);
            uniqueObjects.UnionWith(objects);
            group.Clear();
            group.AddRange(uniqueObjects);
        }
    }
}