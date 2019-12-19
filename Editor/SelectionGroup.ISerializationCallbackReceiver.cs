using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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

        public void OnAfterDeserialize()
        {
            if (_assets != null)
                assets = new HashSet<Object>((from i in _assets where i != null select i));
            else
                assets = new HashSet<Object>();

            if (_scenes != null && _objectIds != null)
            {
                objectIdStrings = new Dictionary<Scene, string[]>();
                for (var i = 0; i < _scenes.Length; i++)
                    objectIdStrings.Add(_scenes[i], _objectIds[i].items);
            }
            else
            {
                objectIdStrings = new Dictionary<Scene, string[]>();
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