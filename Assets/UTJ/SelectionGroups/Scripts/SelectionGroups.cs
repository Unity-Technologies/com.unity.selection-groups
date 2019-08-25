using System.Collections.Generic;
using UnityEngine;

namespace Utj.Film
{
    public class SelectionGroups : MonoBehaviour
    {
        public List<SelectionGroup> groups = new List<SelectionGroup>();

        static SelectionGroups instance;

        public SelectionGroup this[int index]
        {
            get => groups[index];
        }

        public static SelectionGroups Instance
        {
            get
            {
                instance = GameObject.FindObjectOfType<SelectionGroups>();
                if (instance == null)
                {
                    var g = new GameObject("Hidden_SelectionGroups");
                    g.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                    instance = g.AddComponent<SelectionGroups>();
                }
                return instance;
            }
        }
    }
}