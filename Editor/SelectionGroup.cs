using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Unity.SelectionGroups.Runtime;
using UnityEngine.SceneManagement;

namespace Unity.SelectionGroups
{
    /// <summary>
    /// This class is the Editor-only container for selection group information and members.
    /// </summary>
    [System.Serializable]
    public partial class SelectionGroup : ISelectionGroup
    {
        [SerializeField] string name;
        [SerializeField] Color color;
        [SerializeField] bool showMembers;
        [SerializeField] string query = string.Empty;
        [SerializeField] int groupId;
        [SerializeField] SelectionGroupScope scope = SelectionGroupScope.Editor;
        HashSet<string> enabledTools = new HashSet<string>();

        public SelectionGroup()
        {
            SelectionGroupEvents.RegisterListener(this);
        }

        /// <summary>
        /// Creates all references in this group that exist in a loaded scene.
        /// </summary>
        void ReloadReferences() {
            PersistentReferenceCollection.LoadObjects(forceReload:true);
        }
        
    }
}
