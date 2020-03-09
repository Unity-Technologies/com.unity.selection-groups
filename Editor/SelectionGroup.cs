using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace Unity.SelectionGroups
{

    [System.Serializable]
    public partial class SelectionGroup : IEnumerable<Object>
    {
        public string name;
        public Color color;
        public bool showMembers;

        /// <summary>
        /// A GoQL query string which will populate the group with matching members.
        /// </summary>
        public string query = string.Empty;

        public int groupId;

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
            PersistentReferenceCollection.Add(objects);
        }

        public IEnumerator<Object> GetEnumerator()
        {
            PersistentReferenceCollection.LoadObjects();
            return PersistentReferenceCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            PersistentReferenceCollection.LoadObjects();
            return PersistentReferenceCollection.GetEnumerator();
        }
    }
}