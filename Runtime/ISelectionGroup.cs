using System.Collections.Generic;
using UnityEngine;

namespace Unity.SelectionGroups.Runtime
{
    
    internal interface ISelectionGroupContainer : IEnumerable<ISelectionGroup>
    {
        
    }

    internal interface ISelectionGroup
    {
        string Name { get; set;  }
        string Query { get; set;  }
        Color Color { get; set; }
        HashSet<string> EnabledTools { get; set; }
        SelectionGroupDataLocation Scope { get; set; }
        int Count { get; }
        bool ShowMembers { get; set; }
        IList<Object> Members { get; }
        void Add(IList<Object> objectReferences);
        void Remove(IList<Object> objectReferences);
        void Clear();
        IEnumerable<T> GetMemberComponents<T>() where T:Component;
        void SetMembers(IList<Object> objects);
    }
}