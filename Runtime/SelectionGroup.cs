using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Unity.SelectionGroups.Runtime
{
    /// <summary>
    /// This class is used to provide selection group information during play-mode. It reflects the information in the Editor-only class.
    /// </summary>
    [ExecuteAlways]
    internal class SelectionGroup : MonoBehaviour, ISelectionGroup, ISerializationCallbackReceiver    
    {
        /// <summary>
        /// A color assigned to this group.
        /// </summary>
        [SerializeField] Color color;
        /// <summary>
        /// If not empty, this is a GoQL query string used to create the set of matching objects for this group.
        /// </summary>
        [SerializeField] string query = string.Empty;

        [SerializeField] SelectionGroupScope scope = SelectionGroupScope.Scene;

        //Obsolete
        [FormerlySerializedAs("_members")] [SerializeField] Object[] _legacyMembers;
        
        [SerializeField] List<Object> members = new List<Object>();
        
        [HideInInspector][SerializeField] private int sgVersion      = CUR_SG_VERSION; 
        private const                             int CUR_SG_VERSION = (int) SGVersion.INITIAL;        
        

        GoQL.ParseResult parseResult;
        List<object> code;
        GoQL.GoQLExecutor executor;
        HashSet<string> enabledTools = new HashSet<string>();
        
        
        string _name;
        
        
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

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                    _name = this.name;
                return _name;
            }
            set
            {
                this.name = value;
                _name = value;
            }
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

        public IList<Object> Members => members;

        public void Add(IList<Object> objectReferences) 
        {
            var myScene = gameObject.scene;
            foreach (var i in objectReferences) 
            {
                if (null == i)
                    continue;
                
                if (i is GameObject go && go.scene != myScene)
                    continue;
                if(!members.Contains(i))
                    members.Add(i);
            }
            RemoveNullMembers();
            
        }

        public void Remove(IList<Object> objectReferences)
        {
            members.RemoveAll(a=> objectReferences.Contains(a));
            RemoveNullMembers();
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

        private void RemoveNullMembers() {
            for (int i = members.Count-1; i >= 0 ; --i) {
                if (null != members[i])
                    continue;
                
                members.RemoveAt(i);
            }
        }

        public void OnBeforeSerialize() 
        {
            sgVersion = CUR_SG_VERSION;
        }

        public void OnAfterDeserialize()
        {
            //if we have legacyMembers but no current members
            if (null != _legacyMembers && _legacyMembers.Length > 0 && (null == members || members.Count <= 0)) 
            {
                members = new List<Object>();
                members.AddRange(_legacyMembers);                 
                _legacyMembers = null; //clear
            }
            
            sgVersion = CUR_SG_VERSION;            
        }        
        
        
        enum SGVersion {
            INITIAL = 1,    //initial

        }        
    }
} //end namespace
