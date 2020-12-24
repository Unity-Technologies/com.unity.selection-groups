using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.SelectionGroups.Runtime
{
    public static class SelectionGroupEvents
    {
        public delegate void CreateEvent(SelectionGroupScope sender, int groupId, string name, string query, Color color, IList<Object> members);
        public delegate void UpdateEvent(SelectionGroupScope sender, int groupId, string name, string query, Color color, IList<Object> members);
        public delegate void DeleteEvent(SelectionGroupScope sender, int groupId);
        public delegate void AddEvent(SelectionGroupScope sender, int groupId, IList<Object> members);
        public delegate void RemoveEvent(SelectionGroupScope sender, int groupId, IList<Object> members);
        public static CreateEvent Create;
        public static UpdateEvent Update;
        public static DeleteEvent Delete;
        public static AddEvent Add;
        public static RemoveEvent Remove;

        static SelectionGroupEvents()
        {
            Create += OnCreate;
            Update += OnUpdate;
            Delete += OnDelete;
            Add += OnAdd;
            Remove += OnRemove;
        }

        public static void RegisterListener(ISelectionGroup groupEventReceiver)
        {
            UnregisterListener(groupEventReceiver);
            Create += groupEventReceiver.OnCreate;
            Update += groupEventReceiver.OnUpdate;
            Delete += groupEventReceiver.OnDelete;
            Add += groupEventReceiver.OnAdd;
            Remove += groupEventReceiver.OnRemove;
        }
        
        public static void UnregisterListener(ISelectionGroup groupEventReceiver)
        {
            Create -= groupEventReceiver.OnCreate;
            Update -= groupEventReceiver.OnUpdate;
            Delete -= groupEventReceiver.OnDelete;
            Add -= groupEventReceiver.OnAdd;
            Remove -= groupEventReceiver.OnRemove;
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

        public static void OnAdd(SelectionGroupScope sender, int groupId, IList<Object> members)
        {
            Debug.Log($"Add: {sender} {groupId} {members.Count}");
        }
        
        public static void OnRemove(SelectionGroupScope sender, int groupId, IList<Object> members)
        {
            Debug.Log($"Remove: {sender} {groupId} {members.Count}");
        }
    }
}