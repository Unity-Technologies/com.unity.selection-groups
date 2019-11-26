using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Unity.SelectionGroups
{
    [ExecuteInEditMode]
    public class SelectionGroupContainer : MonoBehaviour
    {
        public static readonly Dictionary<Scene, SelectionGroupContainer> instanceMap = new Dictionary<Scene, SelectionGroupContainer>();
        public static event System.Action<SelectionGroupContainer> onLoaded, onUnloaded;

        public Dictionary<string, SelectionGroup> groups = new Dictionary<string, SelectionGroup>();

        void OnEnable()
        {
            RebuildIndex();
            instanceMap.Add(gameObject.scene, this);
            if (onLoaded != null) onLoaded(this);
        }

        public void RebuildIndex()
        {
            groups.Clear();
            foreach (var i in GetComponentsInChildren<SelectionGroup>())
            {
                groups[i.name] = i;
            }
        }

        void OnDisable()
        {
            instanceMap.Remove(gameObject.scene);
            if (onUnloaded != null) onUnloaded(this);
        }

        public static IEnumerable<SelectionGroupContainer> Instances => instanceMap.Values;
        public SelectionGroup this[string index] => groups[index];

    }
}