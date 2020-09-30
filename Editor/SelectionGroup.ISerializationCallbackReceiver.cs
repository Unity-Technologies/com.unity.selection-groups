using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;

namespace Unity.SelectionGroups
{

    public partial class SelectionGroup : ISerializationCallbackReceiver
    {

        [SerializeField] string[] _enabledTools;
        
        /// <summary>
        /// The deserialization callback.
        /// </summary>
        public void OnAfterDeserialize()
        {
            enabledTools.UnionWith(_enabledTools);
        }
        /// <summary>
        /// The serialization callback.
        /// </summary>
        public void OnBeforeSerialize()
        {
            _enabledTools = enabledTools.ToArray();
        }

    }
}