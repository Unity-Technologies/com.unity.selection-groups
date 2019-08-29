using System.Collections.Generic;
using UnityEngine;

namespace Unity.SelectionGroups
{
    public class SelectionGroupContainer : MonoBehaviour
    {
        public List<SelectionGroup> groups = new List<SelectionGroup>();

        public SelectionGroup this[int index]
        {
            get => groups[index];
        }
    }
}