using System.Collections.Generic;
using UnityEngine;

namespace Unity.SelectionGroups.Runtime
{
    public interface ISelectionGroup: IEnumerable<Object>
    {
        void Reload();
        void RefreshQueryResults();
        string Name { get; set;  }
        string Query { get; set;  }
        Color Color { get; set; }
        HashSet<string> EnabledTools { get; set; }
        SelectionGroupScope Scope { get; set; }
        int Count { get; }
        bool ShowMembers { get; set; }
        int GroupId { get; set; }
        void Clear();
        void Add(IList<Object> objects);
        void Remove(IList<Object> objects);
    }
}