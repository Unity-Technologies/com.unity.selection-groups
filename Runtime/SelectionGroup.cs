using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.SelectionGroups.Runtime
{
    /// <summary>
    /// This class is used to provide selection group information during play-mode. It reflects the information in the Editor-only class.
    /// </summary>
    [ExecuteAlways]
    public class SelectionGroup : MonoBehaviour, ISelectionGroup
    {
        /// <summary>
        /// Unique ID of this group.
        /// </summary>
        [SerializeField] [HideInInspector] public int groupId;
        /// <summary>
        /// A color assigned to this group.
        /// </summary>
        public Color color;
        /// <summary>
        /// If not empty, this is a GoQL query string used to create the set of matching objects for this group.
        /// </summary>
        public string query = string.Empty;

        public SelectionGroupScope scope = SelectionGroupScope.Scene;

        /// <summary>
        /// The members of this group.
        /// </summary>
        public OrderedSet<Object> members = new OrderedSet<Object>();

        GoQL.ParseResult parseResult;
        List<object> code;
        GoQL.GoQLExecutor executor;
        [SerializeField] private HashSet<string> enabledTools = new HashSet<string>();
        

        /// <summary>
        /// An enumerator that matches only the GameObject members of this group.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GameObject> EnumerateGameObjects()
        {
            foreach (var i in members)
                if (i is GameObject)
                    yield return i as GameObject;
        }

        void OnEnable()
        {
            executor = new GoQL.GoQLExecutor();
            executor.Code = query;
            SelectionGroupEvents.RegisterListener(this);
        }

        void OnDisable()
        {
            SelectionGroupEvents.UnregisterListener(this);
        }

        public string Name
        {
            get => this.name;
            set => this.name = value;
        }

        string ISelectionGroup.Query
        {
            get => this.query; 
            set => this.query = value;
        }

        Color ISelectionGroup.Color
        {
            get => this.color; 
            set => this.color = value;
        }

        public HashSet<string> EnabledTools
        {
            get => enabledTools;
            set => enabledTools = value;
        }

        public SelectionGroupScope Scope
        {
            get => scope; 
            set => scope = value;
        }

        public int Count => members.Count;
        public bool ShowMembers { get; set; }
        public int GroupId { 
            get => groupId; 
            set => groupId = value; 
        }

        public void OnCreate(SelectionGroupScope sender, int groupId, string name, string query, Color color, IList<Object> members)
        {
            if (groupId != this.groupId) return;
        }

        public void OnUpdate(SelectionGroupScope sender, int groupId, string name, string query, Color color, IList<Object> members)
        {
            if (groupId != this.groupId) return;
            this.name = name;
            this.query = query;
            this.color = color;
            this.members.Clear();
            this.members.AddRange(members);
        }

        public void OnDelete(SelectionGroupScope sender, int groupId)
        {
            if (groupId != this.groupId) return;   
        }

        public void OnAdd(SelectionGroupScope sender, int groupId, IList<Object> members)
        {
            if (groupId != this.groupId) return;
            this.members.AddRange(members);   
        }

        public void OnRemove(SelectionGroupScope sender, int groupId, IList<Object> members)
        {
            if (groupId != this.groupId) return;
            this.members.Remove(members);
        }

        /// <summary>
        /// Get components from all members of a group that are GameObjects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetMemberComponents<T>() where T : Component
        {
            foreach (var member in members)
            {
                var go = member as GameObject;
                if (go != null)
                {
                    foreach (var component in go.GetComponentsInChildren<T>())
                    {
                        yield return component;
                    }
                }
            }
        }

        public IEnumerator<Object> GetEnumerator() => members.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => members.GetEnumerator();

    }
}
