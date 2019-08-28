using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Unity.SelectionGroups
{
    [System.Serializable]
    public struct DynamicSelectionQuery
    {
        public bool enabled;
        public string nameQuery;
        public string[] requiredTypes;
        public Material[] requiredMaterials;
        public Shader[] requiredShaders;
    }
}