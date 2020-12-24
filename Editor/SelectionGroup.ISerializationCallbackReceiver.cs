using System.Linq;
using Unity.SelectionGroups.Runtime;
using UnityEngine;

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
            SelectionGroupEvents.RegisterListener(this);
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