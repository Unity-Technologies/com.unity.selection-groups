using System.Linq;
using Unity.SelectionGroups;
using UnityEngine;

namespace Unity.SelectionGroups.Editor
{

    //[TODO-sin:2021-12-20] Remove in version 0.7.0 
    // internal partial class EditorSelectionGroup : ISerializationCallbackReceiver
    // {
    //
    //     [SerializeField] string[] _enabledTools;
    //
    //     /// <summary>
    //     /// The deserialization callback.
    //     /// </summary>
    //     public void OnAfterDeserialize()
    //     {
    //         enabledTools.Clear();
    //         enabledTools.UnionWith(_enabledTools);
    //     }
    //     /// <summary>
    //     /// The serialization callback.
    //     /// </summary>
    //     public void OnBeforeSerialize()
    //     {
    //         _enabledTools = enabledTools.ToArray();
    //     }
    //     
    // }
}