using System.Collections.Generic;
using System.Linq;
using Unity.GoQL;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.SelectionGroups.Runtime
{
    internal static class SelectionGroupManager
    {
        public delegate void CreateEvent(SelectionGroupDataLocation scope, string name, string query, Color color, IList<Object> members);
        public delegate void DeleteEvent(ISelectionGroup group);

        //[TODO-sin: 2021-12-20] Check these functions if we still need them
        public static CreateEvent Create;
        public static DeleteEvent Delete;

        private static OrderedSet<ISelectionGroup> groups;
        
        static SelectionGroupManager()
        {
            groups = new OrderedSet<ISelectionGroup>();
            Create += OnCreate;
            Delete += OnDelete;
        }
        
        public static void ExecuteSelectionGroupQueries()
        {
            foreach (var i in groups)
            {
                if(!string.IsNullOrEmpty(i.Query)) ExecuteQuery(i);
            }
        }

        public static IList<ISelectionGroup> Groups => groups.List;
        
        public static IEnumerable<string> GroupNames => groups.OrderBy(i => i.Name).Select(g => g.Name);

        
        internal static SelectionGroup CreateSceneSelectionGroup(string name, string query, Color color, IList<Object> members)
        {
            GameObject g = new GameObject(name);
#if UNITY_EDITOR        
            Undo.RegisterCreatedObjectUndo(g,"New Scene Selection Group");
#endif        
            SelectionGroup group = g.AddComponent<SelectionGroup>();
            group.Name        = name;
            group.Query       = query;
            group.Color       = color;
            group.ShowMembers = true;
            group.Add(members);
            SelectionGroupManager.Register(group);
            return group;
        }

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
        
        static void OnCreate(SelectionGroupDataLocation scope, string name, string query, Color color, IList<Object> members)
        {
        }

        //[TODO-sin:2021-12-20] Remove in version 0.7.0 
        // public static void ClearEditorGroups()
        // {
        //     foreach(var i in groups.ToArray())
        //         if(i.Scope == SelectionGroupDataLocation.Editor) Unregister(i);
        // }

        //[TODO-sin:2021-12-20] Remove in version 0.7.0 
        // public static void ChangeGroupScope(ISelectionGroup @group, SelectionGroupDataLocation scope)
        // {
        //     Create(scope, @group.Name, @group.Query, @group.Color, @group.Members);
        //     Delete(@group);
        // }

        public static void ExecuteQuery(ISelectionGroup group)
        {
            var executor = new GoQLExecutor();
            var code = GoQL.Parser.Parse(group.Query, out GoQL.ParseResult parseResult);
            if (parseResult == GoQL.ParseResult.OK)
            {
                executor.Code = group.Query;
                var objects = executor.Execute();
                group.SetMembers(objects);
            }
        }
    }
}