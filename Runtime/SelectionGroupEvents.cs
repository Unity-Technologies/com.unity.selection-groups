using System.Collections.Generic;
using UnityEngine;

namespace Unity.SelectionGroups.Runtime
{
    public static class SelectionGroupEvents
    {
        public delegate void CreateEvent(SelectionGroupScope sender, int groupId, string name, string query, Color color, IList<Object> members);
        public delegate void UpdateEvent(SelectionGroupScope sender, int groupId, string name, string query, Color color, IList<Object> members);
        public delegate void DeleteEvent(SelectionGroupScope sender, int groupId);
        public static CreateEvent Create;
        public static UpdateEvent Update;
        public static DeleteEvent Delete;

        static SelectionGroupEvents()
        {
            Create += OnCreate;
            Update += OnUpdate;
            Delete += OnDelete;
        }

        private static void OnDelete(SelectionGroupScope sender, int groupId)
        {
            Debug.Log($"Delete: {sender} {groupId}");
        }

        private static void OnUpdate(SelectionGroupScope sender, int groupId, string name, string query, Color color, IList<Object> members)
        {
            Debug.Log($"Update: {sender} {groupId} {name} {query} {color} {members.Count}");
        }

        private static void OnCreate(SelectionGroupScope sender, int groupId, string name, string query, Color color, IList<Object> members)
        {
            Debug.Log($"Create: {sender} {groupId} {name} {query} {color} {members.Count}");
        }
    }
}