using System.Collections.Generic;
using UnityEngine;

namespace Unity.SelectionGroups
{
    public class SelectionGroupContainer : MonoBehaviour
    {
        public List<SelectionGroup> groups = new List<SelectionGroup>();

        static SelectionGroupContainer instance;

        public SelectionGroup this[int index]
        {
            get => groups[index];
        }

        public static SelectionGroupContainer Instance
        {
            get
            {
                instance = GameObject.FindObjectOfType<SelectionGroupContainer>();
                if (instance == null)
                {
                    var g = new GameObject("Hidden_SelectionGroups");
                    g.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                    instance = g.AddComponent<SelectionGroupContainer>();
                }
                return instance;
            }
        }
    }
}