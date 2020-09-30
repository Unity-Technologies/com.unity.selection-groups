using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace Unity.SelectionGroups
{

    /// <summary>
    /// This class is the Editor-only container for selection group information and members.
    /// </summary>
    [System.Serializable]
    public partial class SelectionGroup : IEnumerable<Object>
    {
        /// <summary>
        /// The name of the group.
        /// </summary>
        public string name;
        /// <summary>
        /// A color assigned to the group.
        /// </summary>
        public Color color;
        /// <summary>
        /// Should this group expand the list of members in the UI.
        /// </summary>
        public bool showMembers;

        /// <summary>
        /// A GoQL query string which will populate the group with matching members.
        /// </summary>
        public string query = string.Empty;

        /// <summary>
        /// Unique ID of the group.
        /// </summary>
        public int groupId;

        /// <summary>
        /// Allows this group to exclude the members of other selection group.
        /// </summary>
        /// <returns></returns>
        public SelectionGroupList exclude = new SelectionGroupList();

        /// <summary>
        /// Number of objects in this group that are available to be referenced. (Ie. they exist in a loaded scene)
        /// </summary>
        internal int Count => PersistentReferenceCollection.LoadedObjectCount;

        /// <summary>
        /// Number of objects that exist in this group, including objects that cannot be loaded. (Ie. The containing scene has not been loaded)
        /// </summary>
        internal int TotalCount => PersistentReferenceCollection.TotalObjectCount;

        /// <summary>
        /// Access by index the loaded objects in this group.
        /// </summary>
        /// <value></value>
        internal Object this[int index] { get => PersistentReferenceCollection[index]; set => PersistentReferenceCollection[index] = value; }

        [SerializeField] internal HashSet<string> enabledTools = new HashSet<string>();

        [SerializeField] PersistentReferenceCollection _persistentReferenceCollection;
        PersistentReferenceCollection PersistentReferenceCollection
        {
            get
            {
                if (_persistentReferenceCollection == null)
                {
                    _persistentReferenceCollection = new PersistentReferenceCollection();
                    _persistentReferenceCollection.LoadObjects();
                }
                return _persistentReferenceCollection;
            }
        }

        GoQL.GoQLExecutor executor = new GoQL.GoQLExecutor();

        
        internal void RefreshQueryResults()
        {
            if (!string.IsNullOrEmpty(query))
            {
                executor.Code = query;
                var objects = executor.Execute();
                PersistentReferenceCollection.Clear();
                PersistentReferenceCollection.Add(objects);
            }
        }

        /// <summary>
        /// Creates all references in this group that exist in a loaded scene.
        /// </summary>
        internal void Reload() {
            PersistentReferenceCollection.LoadObjects(forceReload:true);
        }

        internal void Clear()
        {
            PersistentReferenceCollection.Clear();
        }

        internal void Remove(Object[] objects)
        {
            PersistentReferenceCollection.Remove(objects);
        }

        internal void Add(Object[] objects)
        {
            foreach(var i in objects) {
                var go = i as GameObject;
                if(go != null && string.IsNullOrEmpty(go.scene.path)) {
                    //GameObject is not saved into a scene, therefore it cannot be stored in a selection group.
                    throw new SelectionGroupException("Cannot add a gameobject from an unsaved scene.");
                }
            }
            PersistentReferenceCollection.Add(objects);
        }

        /// <summary>
        /// Enumerator for all members of this group.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Object> GetEnumerator()
        {
            PersistentReferenceCollection.LoadObjects();
            return PersistentReferenceCollection.GetEnumerator();
        }
        /// <summary>
        /// Enumerable for all members of this group.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            PersistentReferenceCollection.LoadObjects();
            return PersistentReferenceCollection.GetEnumerator();
        }
    }
}