using System.Collections.Generic;
using UnityEngine;

namespace Unity.SelectionGroups.Runtime
{
    public interface ISelectionGroup: IEnumerable<Object>
    {
        string Name { get; set;  }
        string Query { get; set;  }
        Color Color { get; set; }
        HashSet<string> EnabledTools { get; set; }
        SelectionGroupScope Scope { get; set; }
        int Count { get; }
        bool ShowMembers { get; set; }
        int GroupId { get; set; }
        void OnCreate(SelectionGroupScope sender, int groupId, string name, string query, Color color, IList<Object> members);
        void OnUpdate(SelectionGroupScope sender, int groupId, string name, string query, Color color, IList<Object> members);
        void OnDelete(SelectionGroupScope sender, int groupId);
        void OnAdd(SelectionGroupScope sender, int groupId, IList<Object> members);
        void OnRemove(SelectionGroupScope sender, int groupId, IList<Object> members);
    }
}