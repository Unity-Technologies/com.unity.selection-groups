using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Utj.Film
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