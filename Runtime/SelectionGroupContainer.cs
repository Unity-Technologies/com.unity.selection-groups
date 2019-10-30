using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Unity.SelectionGroups
{
    [ExecuteInEditMode]
    public class SelectionGroupContainer : MonoBehaviour, ISerializationCallbackReceiver
    {
        public static readonly Dictionary<Scene, SelectionGroupContainer> instanceMap = new Dictionary<Scene, SelectionGroupContainer>();
        public static event System.Action<SelectionGroupContainer> onLoaded, onUnloaded;

        public Dictionary<string, SelectionGroup> groups = new Dictionary<string, SelectionGroup>();

        [SerializeField] string[] _groupsKeys;
        [SerializeField] SelectionGroup[] _groupsValues;

        void OnEnable()
        {
            instanceMap.Add(gameObject.scene, this);
            if (onLoaded != null) onLoaded(this);
        }

        void OnDisable()
        {
            instanceMap.Remove(gameObject.scene);
            if (onUnloaded != null) onUnloaded(this);
        }

        public void OnBeforeSerialize()
        {
            _groupsKeys = groups.Keys.ToArray();
            _groupsValues = (from i in _groupsKeys select groups[i]).ToArray();
        }

        public void OnAfterDeserialize()
        {
            if (_groupsKeys != null && _groupsValues != null)
                for (var i = 0; i < _groupsKeys.Length; i++)
                {
                    groups[_groupsKeys[i]] = _groupsValues[i];
                }
        }
        public static IEnumerable<SelectionGroupContainer> Instances => instanceMap.Values;
        public SelectionGroup this[string index] => groups[index];

        internal SelectionGroup Create(string groupName, Color color, bool showMembers)
        {
            var g = new GameObject(groupName).AddComponent<SelectionGroup>();
            g.color = color;
            g.showMembers = showMembers;
            g.objects = new HashSet<GameObject>();
            groups[groupName] = g;
            g.transform.parent = this.transform;
            return g;
        }

        internal SelectionGroup Create(string groupName)
        {
            var color = Color.HSVToRGB(Random.value, Random.Range(0.7f, 1f), Random.Range(0.7f, 1f));
            return Create(groupName, color, showMembers: true);
        }
    }
}