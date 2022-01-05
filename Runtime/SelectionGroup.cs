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
    /// A component to group a number of GameObjects under a common name.
    /// </summary>
    [ExecuteAlways]
    public class SelectionGroup : MonoBehaviour, ISelectionGroup, ISerializationCallbackReceiver    
    {
        /// <summary>
        /// A color assigned to this group.
        /// </summary>
        [HideInInspector][SerializeField] Color color;
        /// <summary>
        /// If not empty, this is a GoQL query string used to create the set of matching objects for this group.
        /// </summary>
        [HideInInspector][SerializeField] string query = string.Empty;

        //Obsolete
        [HideInInspector][FormerlySerializedAs("_members")] [SerializeField] Object[] _legacyMembers;
        
        [HideInInspector][SerializeField] List<Object> members = new List<Object>();
        
#pragma warning disable 414    
        [HideInInspector][SerializeField] private int sgVersion = CUR_SG_VERSION; 
#pragma warning restore 414
                

        GoQL.ParseResult parseResult;
        List<object> code;
        GoQL.GoQLExecutor executor;
        HashSet<string> enabledTools = new HashSet<string>();
        
        
        string _name;
        
        
        void OnEnable()
        {
            executor = new GoQL.GoQLExecutor();
            executor.Code = query;

            this.gameObject.hideFlags = HideFlags.HideInHierarchy;

            if (!m_registerOnEnable) 
                return;
            SelectionGroupManager.GetOrCreateInstance().Register(this);
            m_registerOnEnable = false;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public string Query
        {
            get => this.query; 
            set => this.query = value;
        }
        
        /// <inheritdoc/>
        public bool IsAutoFilled() {
            return !string.IsNullOrEmpty(Query);
        }
        
        /// <inheritdoc/>
        public Color Color
        {
            get => this.color; 
            set => this.color = value;
        }

        /// <inheritdoc/>
        public HashSet<string> EnabledTools
        {
            get => enabledTools;
            set => enabledTools = value;
        }

        /// <inheritdoc/>
        public SelectionGroupDataLocation Scope
        {
            get => SelectionGroupDataLocation.Scene;
            set  {
                //[TODO-sin: 2021-10-22] Remove set starting from version 0.6.0
            }
        }

        /// <inheritdoc/>
        public int Count => members.Count;
        /// <inheritdoc/>
        public bool ShowMembers { get; set; }

        /// <inheritdoc/>
        public IList<Object> Members => members;

        /// <inheritdoc/>
        public void Add(IList<Object> objects) 
        {
            foreach (var i in objects) 
            {
                if (i == null)
                    continue;
                
                if(!members.Contains(i))
                    members.Add(i);
            }
            RemoveNullMembers();
        }
        
        /// <inheritdoc/>
        public void SetMembers(IList<Object> objects) 
        {
            members.Clear();
            foreach (var i in objects) 
            {
                if (i == null)
                    continue;
                members.Add(i);
            }
        }

        /// <inheritdoc/>
        public void Remove(IEnumerable<Object> objectReferences) {
            if (IsAutoFilled())
                return;
            
            members.RemoveAll(a=> objectReferences.Contains(a));
            RemoveNullMembers();
        }
        
        /// <inheritdoc/>
        public void Clear()
        {
            members.Clear();
        }

        /// <summary>
        /// Enumerate components from all members of a group that have a certain type T.
        /// </summary>
        /// <typeparam name="T">The type of the component</typeparam>
        /// <returns>The enumerated component</returns>
        internal IEnumerable<T> GetMemberComponents<T>() where T : Component
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

        /// <inheritdoc/>
        public void OnBeforeSerialize() 
        {
            sgVersion = CUR_SG_VERSION;
        }

        /// <inheritdoc/>
        public void OnAfterDeserialize()
        {
            //if we have legacyMembers but no current members
            if (null != _legacyMembers && _legacyMembers.Length > 0 && (null == members || members.Count <= 0)) 
            {
                members = new List<Object>();
                members.AddRange(_legacyMembers);                 
                _legacyMembers = null; //clear
            }

            if (sgVersion < (int) SGVersion.ORDERED_0_6_0) {
                m_registerOnEnable = true;
            }
            
            sgVersion = CUR_SG_VERSION;            
        }        

//----------------------------------------------------------------------------------------------------------------------
        
        private const int  CUR_SG_VERSION     = (int) SGVersion.ORDERED_0_6_0;
        private       bool m_registerOnEnable = false;
        
        
        
        enum SGVersion {
            INITIAL = 1,    //initial
            ORDERED_0_6_0 = 2, //The order of selection groups is maintained by SelectionGroupManager

        }        
    }
} //end namespace
