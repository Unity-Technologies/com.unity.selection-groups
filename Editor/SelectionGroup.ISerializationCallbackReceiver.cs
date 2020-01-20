using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace Unity.SelectionGroups
{

    public partial class SelectionGroup : ISerializationCallbackReceiver
    {

        [SerializeField] string[] _objectIds;

        public void OnAfterDeserialize()
        {
            var ids = new GlobalObjectId[_objectIds.Length];
            for (var i = 0; i < _objectIds.Length; i++)
                if(GlobalObjectId.TryParse(_objectIds[i], out ids[i]))
                    globalObjectIdSet.Add(ids[i]);
            
        }

        public void OnBeforeSerialize()
        {
            _objectIds = (from i in globalObjectIdSet select i.ToString()).ToArray();
        }
    }
}