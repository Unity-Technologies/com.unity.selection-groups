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

        public void OnAfterDeserialize()
        {
            enabledTools.UnionWith(_enabledTools);
        }

        public void OnBeforeSerialize()
        {
            _enabledTools = enabledTools.ToArray();
        }

    }
}