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
        public List<UnityEngine.Object> members = new List<Object>();

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
            SelectionGroupEvents.Update -= SelectionGroupEventsOnUpdate;
            SelectionGroupEvents.Update += SelectionGroupEventsOnUpdate;
        }

        private void SelectionGroupEventsOnUpdate(SelectionGroupScope sender, int groupId, string name, string query, Color color, IList<Object> objects)
        {
            //Ignore events coming from scene, we only listen to the editor.
            if (sender == SelectionGroupScope.Scene) return;
            
            if (groupId == this.groupId)
            {
                this.name = name;
                this.query = query;
                this.color = color;
                this.members.Clear();
                this.members.AddRange(objects);
            }
        }

        public void Reload()
        {
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
        
        public void Clear()
        {
            members.Clear();
        }
        public void Add(IList<Object> objects)
        {
            members.AddRange(objects);
        }

        public void Remove(IList<Object> objects)
        {
            var map = new HashSet<Object>(objects);
            members.RemoveAll(o => map.Contains(o));
        }

        /// <summary>
        /// Run the GoQL query attached to this group, adding any new members that are discovered.
        /// </summary>
        public void RefreshQueryResults()
        {
            executor.Code = query;
            members.Clear();
            members.AddRange(executor.Execute());
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
