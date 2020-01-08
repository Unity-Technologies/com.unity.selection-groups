using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace Unity.SelectionGroups
{
    [System.Serializable]
    public partial class SelectionGroup
    {
        public string name;
        public Color color;
        public bool showMembers;
        public string query = string.Empty;
        public bool sort = false;

        public int groupId;

        [System.NonSerialized] public List<UnityEngine.Object> members = new List<UnityEngine.Object>();
        
        HashSet<GlobalObjectId> globalObjectIdSet = new HashSet<GlobalObjectId>();

        GoQL.GoQLExecutor executor = new GoQL.GoQLExecutor();

        public void RefreshQueryResults()
        {
            if (query != string.Empty)
            {
                executor.Code = query;
                var objects = executor.Execute();
                members.Clear();
                if (sort)
                    System.Array.Sort(objects, (a, b) => a.name.CompareTo(b.name));
                members.AddRange(objects);
            }
        }

        internal void Add(ICollection<UnityEngine.Object> objectReferences)
        {
            members.AddRange(objectReferences);
        }

        internal void ConvertSceneObjectsToGlobalObjectIds()
        {
            var gids = new GlobalObjectId[members.Count];
            GlobalObjectId.GetGlobalObjectIdsSlow(members.ToArray(), gids);
            globalObjectIdSet.UnionWith(gids);
        }

        internal void Clear()
        {
            members.Clear();
            query = string.Empty;
            globalObjectIdSet.Clear();
        }

        internal void ConvertGlobalObjectIdsToSceneObjects()
        {
            var outputObjects = new Object[globalObjectIdSet.Count];
            GlobalObjectId.GlobalObjectIdentifiersToObjectsSlow(globalObjectIdSet.ToArray(), outputObjects);
            var objectSet = new HashSet<Object>(members);
            foreach(var i in outputObjects) {
                if(i != null) objectSet.Add(i);
            }
            members.Clear();
            members.AddRange(objectSet);
        }

        internal void Remove(UnityEngine.Object[] objects)
        {
            foreach(var o in objects) members.Remove(o);
        }
    }
}