using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Unity.SelectionGroups;
using Unity.SelectionGroups.Runtime;
using UnityEngine.SceneManagement;

namespace Unity.SelectionGroupsEditor
{
    /// <summary>
    /// This class is the Editor-only container for selection group information and members.
    /// </summary>
    [System.Serializable]
    internal partial class SelectionGroup : ISelectionGroup
    {
        [SerializeField] string name;
        [SerializeField] Color color;
        [SerializeField] bool showMembers;
        [SerializeField] string query = string.Empty;
        [SerializeField] int groupId;
        [SerializeField] SelectionGroupDataLocation scope = SelectionGroupDataLocation.Editor;
        HashSet<string> enabledTools = new HashSet<string>();
        
        /// <summary>
        /// Creates all references in this group that exist in a loaded scene.
        /// </summary>
        void ReloadReferences() {
            PersistentReferenceCollection.LoadObjects(forceReload:true);
        }

        public IList<Object> Members
        {
            get => PersistentReferenceCollection.Items;
        }

        public IEnumerable<T> GetMemberComponents<T>() where T : Component
        {
            foreach (var i in this.Members)
            {
                if (i is GameObject go)
                {
                    foreach (var j in go.GetComponents<T>())
                        yield return j;
                }
            }
        }
    }
}
