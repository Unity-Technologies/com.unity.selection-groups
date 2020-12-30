using System.Collections.Generic;
using UnityEngine;

namespace Unity.SelectionGroups.Runtime
{
    
    public interface ISelectionGroupContainer : IEnumerable<ISelectionGroup>
    {
        
    }

    public interface ISelectionGroup: IEnumerable<Object>
    {
        string Name { get; set;  }
        string Query { get; set;  }
        Color Color { get; set; }
        HashSet<string> EnabledTools { get; set; }
        SelectionGroupScope Scope { get; set; }
        int Count { get; }
        bool ShowMembers { get; set; }
        void Add(IList<Object> objectReferences);
        void Remove(IList<Object> objectReferences);
        void Clear();
    }
}