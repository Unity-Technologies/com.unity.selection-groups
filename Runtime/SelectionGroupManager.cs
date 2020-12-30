using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.SelectionGroups.Runtime
{
    public static class SelectionGroupManager
    {
        public delegate void CreateEvent(SelectionGroupScope scope, string name, string query, Color color, IList<Object> members);
        public delegate void DeleteEvent(ISelectionGroup group);

        public delegate void GenericSelectionGroupEvent(SelectionGroupScope sender, int groupId);
        
        public static CreateEvent Create;
        public static DeleteEvent Delete;

        private static HashSet<ISelectionGroup> groups;
        
        static SelectionGroupManager()
        {
            groups = new HashSet<ISelectionGroup>();
            Create += OnCreate;
            Delete += OnDelete;
        }

        public static IEnumerable<ISelectionGroup> Groups => groups.OrderBy(i => i.Name);
        public static IEnumerable<string> GroupNames => groups.OrderBy(i => i.Name).Select(g => g.Name);

        public static void Register(ISelectionGroup @group)
        {
            Unregister(@group);
            groups.Add(group);
        }
        
        public static void Unregister(ISelectionGroup @group)
        {
            groups.Remove(group);
        }

        static void OnDelete(ISelectionGroup group)
        {
            Unregister(group);
        }
        
        static void OnCreate(SelectionGroupScope scope, string name, string query, Color color, IList<Object> members)
        {
        }
        
        public static void ClearEditorGroups()
        {
            foreach(var i in groups.ToArray())
                if(i.Scope == SelectionGroupScope.Editor) Unregister(i);
        }
    }
}