using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.SelectionGroups
{
    [System.Serializable]
    class StringArray
    {
        public string[] items;

        public StringArray(string[] v)
        {
            items = v;
        }
    }

    public partial class SelectionGroup : ISerializationCallbackReceiver
    {

        [SerializeField] Object[] _assets;
        [SerializeField] Scene[] _scenes;
        [SerializeField] StringArray[] _objectIds;

        public void DebugGIDS() {
            foreach(var i in _objectIds) {
                Debug.Log($"{i.items.Length} - {string.Join(", ", i.items)}");
            }
        }

        public void OnAfterDeserialize()
        {
            if (assets == null)
                assets = new HashSet<Object>();
            else
                assets.Clear();

            if (_assets != null)
                assets.UnionWith(from i in _assets where i != null select i);

            if (objectIdStrings == null)
                objectIdStrings = new Dictionary<Scene, string[]>();
            else
                objectIdStrings.Clear();

            if (_scenes != null && _objectIds != null)
            {
                for (var i = 0; i < _scenes.Length; i++) {
                    // Debug.Log($"{name} {_scenes[i].handle} {_objectIds[i].items.Length}");
                    objectIdStrings.Add(_scenes[i], _objectIds[i].items);
                }
            }
        }

        public void OnBeforeSerialize()
        {
            _assets = assets == null ? new Object[0] : assets.ToArray();
            _scenes = objectIdStrings == null ? new Scene[0] : objectIdStrings.Keys.ToArray();
            _objectIds = (from i in _scenes select new StringArray(objectIdStrings[i])).ToArray();
        }
    }
}