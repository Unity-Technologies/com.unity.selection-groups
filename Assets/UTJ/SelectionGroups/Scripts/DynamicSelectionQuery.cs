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

        public IEnumerable<GameObject> Filter(IEnumerable<Transform> transforms)
        {
            if (enabled)
                foreach (var i in transforms)
                {
                    if (!string.IsNullOrEmpty(nameQuery))
                    {
                        if (!i.name.Contains(nameQuery)) continue;
                    }
                    if (requiredTypes.Length > 0)
                    {
                        var missingComponents = false;
                        foreach (var c in requiredTypes)
                        {
                            var component = i.GetComponent(c);
                            if (component == null)
                            {
                                missingComponents = true;
                                break;
                            }
                        }
                        if (missingComponents) continue;
                    }
                    if (requiredMaterials.Length > 0)
                    {
                        var renderer = i.GetComponent<Renderer>();
                        if (renderer == null) continue;
                        var missingMaterials = false;
                        foreach (var m in requiredMaterials)
                        {
                            if (System.Array.IndexOf(renderer.sharedMaterials, m) == -1)
                            {
                                missingMaterials = true;
                                break;
                            }
                        }
                        if (missingMaterials) continue;
                    }
                    if (requiredShaders.Length > 0)
                    {
                        var renderer = i.GetComponent<Renderer>();
                        if (renderer == null) continue;
                        var shaders = (from m in renderer.sharedMaterials select m.shader).ToArray();
                        var missingShaders = false;
                        foreach (var s in requiredShaders)
                        {
                            if (System.Array.IndexOf(shaders, s) == -1)
                            {
                                missingShaders = true;
                                break;
                            }
                        }
                        if (missingShaders) continue;
                    }
                    yield return i.gameObject;
                }
        }
    }

}