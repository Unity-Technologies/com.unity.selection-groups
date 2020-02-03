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
        public string query = string.Empty;

        public int groupId;

        [SerializeField] internal HashSet<string> enabledTools = new HashSet<string>();

        [SerializeField] PersistentObjectStore _persistentObjectStore;
        PersistentObjectStore PersistentObjectStore
        {
            get
            {
                if (_persistentObjectStore == null)
                {
                    _persistentObjectStore = new PersistentObjectStore();
                    _persistentObjectStore.LoadObjects();
                }
                return _persistentObjectStore;
            }
        }

        GoQL.GoQLExecutor executor = new GoQL.GoQLExecutor();

        public int Count => PersistentObjectStore.LoadedObjectCount;

        public int TotalCount => PersistentObjectStore.TotalObjectCount;

        public Object this[int index] { get => PersistentObjectStore[index]; set => PersistentObjectStore[index] = value; }

        public void RefreshQueryResults()
        {
            if (!string.IsNullOrEmpty(query))
            {
                executor.Code = query;
                var objects = executor.Execute();
                PersistentObjectStore.Clear();
                PersistentObjectStore.Add(objects);
            }
        }

        internal void Clear()
        {
            PersistentObjectStore.Clear();
        }

        internal void Remove(Object[] objects)
        {
            PersistentObjectStore.Remove(objects);
        }

        internal void Add(Object[] objects)
        {
            PersistentObjectStore.Add(objects);
        }

        public IEnumerator<Object> GetEnumerator()
        {
            PersistentObjectStore.LoadObjects();
            return PersistentObjectStore.activeObjects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            PersistentObjectStore.LoadObjects();
            return PersistentObjectStore.activeObjects.GetEnumerator();
        }
    }
}