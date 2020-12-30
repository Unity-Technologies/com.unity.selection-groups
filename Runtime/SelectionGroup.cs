using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.SelectionGroups.Runtime
{
    /// <summary>
    /// This class is used to provide selection group information during play-mode. It reflects the information in the Editor-only class.
    /// </summary>
    [ExecuteAlways]
    public class SelectionGroup : MonoBehaviour, ISelectionGroup, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Unique ID of this group.
        /// </summary>
        [SerializeField] [HideInInspector] public int groupId;
        /// <summary>
        /// A color assigned to this group.
        /// </summary>
        [SerializeField] Color color;
        /// <summary>
        /// If not empty, this is a GoQL query string used to create the set of matching objects for this group.
        /// </summary>
        [SerializeField] string query = string.Empty;

        [SerializeField] SelectionGroupScope scope = SelectionGroupScope.Scene;

        /// <summary>
        /// The members of this group.
        /// </summary>
        OrderedSet<Object> members = new OrderedSet<Object>();
        
        [SerializeField] [HideInInspector] private Object[] _members;

        GoQL.ParseResult parseResult;
        List<object> code;
        GoQL.GoQLExecutor executor;
        HashSet<string> enabledTools = new HashSet<string>();
        
        
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
            SelectionGroupManager.Register(this);
        }

        void OnDisable()
        {
            SelectionGroupManager.Unregister(this);
        }

        void OnDestroy()
        {
            SelectionGroupManager.Delete(this);
        }

        public string Name
        {
            get => this.name;
            set => this.name = value;
        }

        public string Query
        {
            get => this.query; 
            set => this.query = value;
        }

        public Color Color
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
        
        public void Add(IList<Object> objectReferences)
        {   
            members.AddRange(objectReferences);
        }

        public void Remove(IList<Object> objectReferences)
        {
            members.Remove(objectReferences);
        }

        public void Clear()
        {
            members.Clear();
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

        public void OnBeforeSerialize()
        {
            _members = members.ToArray();
        }

        public void OnAfterDeserialize()
        {
            if(_members != null) members.AddRange(_members);
        }
    }
}
