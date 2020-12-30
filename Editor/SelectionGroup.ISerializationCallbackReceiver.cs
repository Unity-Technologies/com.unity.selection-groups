using System.Linq;
using Unity.SelectionGroups.Runtime;
using UnityEngine;

namespace Unity.SelectionGroupsEditor
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
            // SelectionGroupManager.Register(this);
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