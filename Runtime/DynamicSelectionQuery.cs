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
        public List<string> requiredTypes;
        public List<Material> requiredMaterials;
        public List<Shader> requiredShaders;
    }
}