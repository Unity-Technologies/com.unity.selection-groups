﻿using System.Collections.Generic;
using System.Linq;
using Unity.GoQL;
using UnityEngine;

namespace Unity.SelectionGroups.Runtime
{
    internal static class SelectionGroupManager
    {
        public delegate void CreateEvent(SelectionGroupDataLocation scope, string name, string query, Color color, IList<Object> members);
        public delegate void DeleteEvent(ISelectionGroup group);

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
        
        public static void ClearEditorGroups()
        {
            foreach(var i in groups.ToArray())
                if(i.Scope == SelectionGroupDataLocation.Editor) Unregister(i);
        }

        public static void ChangeGroupScope(ISelectionGroup @group, SelectionGroupDataLocation scope)
        {
            Create(scope, @group.Name, @group.Query, @group.Color, @group.Members);
            Delete(@group);
        }

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