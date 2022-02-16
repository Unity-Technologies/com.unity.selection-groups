using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.FilmInternalUtilities;
using Unity.GoQL;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.SelectionGroups
{
    /// <summary>
    /// A component to group a number of GameObjects under a common name.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("")]
    public class SelectionGroup : MonoBehaviour, ISerializationCallbackReceiver    
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
                
        void OnEnable()
        {
            RefreshHideFlagsInEditor();
            
            if (!m_registerOnEnable) 
                return;
            SelectionGroupManager.GetOrCreateInstance().Register(this);
            m_registerOnEnable = false;
        }

        private void OnDestroy() {
            //Check if the gameObject/component was deleted, instead of scene closure
            if (!gameObject.scene.isLoaded) 
                return;
            SelectionGroupManager.GetOrCreateInstance().Unregister(this);
        }


         /// <summary>
         /// Sets/gets the name of the SelectionGroup
         /// </summary>
        public string Name
        {
            get { return gameObject.name; }
            set { gameObject.name = value; }
        }


         /// <summary>
         /// Sets/gets the query which will automatically include GameObjects from the hierarchy that match the query into the group.
         /// </summary>
        public string Query
        {
            get => this.query; 
        }

        /// <summary>
        /// Sets the query which will automatically include GameObjects from the hierarchy that match the query into the group.
        /// Returns early when the query is not valid (GoQL.ParseResult.OK).
        /// </summary>
        /// <param name="q">The query</param>
        /// <returns>The parse result of the query.</returns>
        public void SetQuery(string q) {

            if (string.IsNullOrEmpty(q)) {
                this.query = q;
                return;
            }

            this.query = q;
            UpdateQueryResults();
        }

        internal void UpdateQueryResults() {
            GoQL.GoQLExecutor executor = new GoQL.GoQLExecutor();
            GoQL.Parser.Parse(this.query, out m_queryParseResult);
            if (m_queryParseResult != GoQL.ParseResult.OK) 
                return;
            
            executor.Code = this.query;
            GameObject[] objects = executor.Execute();
            SetMembersInternal(objects);
        }
        

        public GoQL.ParseResult GetLastQueryParseResult() => m_queryParseResult;
        
        
         /// <summary>
         /// Gets whether the group is automatically filled
         /// </summary>
        public bool IsAutoFilled() {
            return !string.IsNullOrEmpty(Query);
        }
        
         /// <summary>
         /// Sets/gets the color of the SelectionGroup 
         /// </summary>
        public Color Color
        {
            get => this.color; 
            set => this.color = value;
        }

        internal bool GetEditorToolStatus(int toolID) {
            if (m_editorToolsStatus.TryGetValue(toolID, out bool status))
                return status;

            return false;
        }

        public void EnableEditorTool(int toolID, bool toolEnabled) {
            Assert.IsTrue(toolID < (int)SelectionGroupToolType.MAX);
            m_editorToolsStatus[toolID] = toolEnabled;
        }
        
         /// <summary>
         /// Gets the number of members in this SelectionGroup
         /// </summary>
        public int Count => members.Count;
        
#if UNITY_EDITOR
        internal bool AreMembersShownInWindow() => m_showMembersInWindow;

        internal void ShowMembersInWindow(bool show) {
            m_showMembersInWindow = show;
        }
#endif

        /// <summary>
         /// Get the members of the SelectionGroup
         /// </summary>
        public IList<Object> Members => members;

        [NotNull]
        internal List<GameObject> FindGameObjectMembers() {
            List<GameObject> ret = new List<GameObject>();
            foreach (Object m in members) {
                if (m is GameObject go) {
                    ret.Add(go);
                }
            }
            return ret;
        }

//----------------------------------------------------------------------------------------------------------------------
        
         /// <summary>
         /// Adds a list of objects to the SelectionGroup 
         /// </summary>
         /// <param name="objects">A list of objects to be added</param>
        public void Add(IEnumerable<Object> objects) {
            foreach (Object i in objects) {
                Add(i);
            }
            RemoveNullMembers();
        }
        
        /// <summary>
        /// Adds an object to the SelectionGroup 
        /// Does nothing if the group is automatically filled. 
        /// </summary>
        /// <param name="obj">the object to be added</param>
        public void Add(Object obj) {
            if (null == obj)
                return;
            
            if(!members.Contains(obj))
                members.Add(obj);
        }
        
//----------------------------------------------------------------------------------------------------------------------        
        
         /// <summary>
         /// Clears and set the members of the SelectionGroup 
         /// </summary>
         /// <param name="objects">A enumerable collection of objects to be added</param>
        public void SetMembers(IEnumerable<Object> objects) {
            if (IsAutoFilled()) {
                Debug.LogWarning($"[SG] Group {Name} is auto-filled. Can't manually set members");
                return;
            }
                
            SetMembersInternal(objects);
        }

        private void SetMembersInternal(IEnumerable<Object> objects) 
        {
            members.Clear();
            foreach (var i in objects) 
            {
                if (i == null)
                    continue;
                members.Add(i);
            }
        }

         /// <summary>
         /// Removes a list of objects from the SelectionGroup 
         /// </summary>
         /// <param name="objectReferences">A list of objects to be removed</param>
        public void Remove(IEnumerable<Object> objectReferences) {
            if (IsAutoFilled())
                return;
            
            members.RemoveAll(a=> objectReferences.Contains(a));
            RemoveNullMembers();
        }
        
        /// <summary>
        /// Removes an object from the SelectionGroup.
        /// Does nothing if the group is automatically filled. 
        /// </summary>
        /// <param name="obj">The object to be removed</param>
        public void Remove(Object obj) {
            if (IsAutoFilled())
                return;
            
            members.Remove(obj);
        }

         /// <summary>
         /// Clears the members of the SelectionGroup
         /// </summary>
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

#if UNITY_EDITOR        
        internal void SetOnDestroyedInEditorCallback(Action cb) {
            m_onDestroyedInEditorCB = cb;
        }
#endif
        
        internal void RefreshHideFlagsInEditor() {
#if UNITY_EDITOR
            SelectionGroupsEditorProjectSettings settings = SelectionGroupsEditorProjectSettings.GetOrCreateInstance();
            
            HideFlags goHideFlags = HideFlags.None;
            if (!settings.AreGroupsVisibleInHierarchy()) {
                goHideFlags |= HideFlags.HideInHierarchy;
            }
            //Note: SelectionGroup won't be selectable in the select popup window if HideInHierarchy is set 
            this.gameObject.hideFlags = goHideFlags;
            this.transform.hideFlags  = goHideFlags | HideFlags.HideInInspector;
#endif
        }
        

        
//----------------------------------------------------------------------------------------------------------------------

        [SerializeField] SerializedDictionary<int, bool> m_editorToolsStatus = new SerializedDictionary<int,bool>(); 

        [SerializeField] private bool m_showMembersInWindow = true;
        

        private GoQL.ParseResult m_queryParseResult = ParseResult.Empty;       
        
        private const int  CUR_SG_VERSION     = (int) SGVersion.ORDERED_0_6_0;
        private       bool m_registerOnEnable = false;

#if UNITY_EDITOR        
        private Action m_onDestroyedInEditorCB = null;
#endif        
        
        enum SGVersion {
            INITIAL = 1,    //initial
            ORDERED_0_6_0 = 2, //The order of selection groups is maintained by SelectionGroupManager

        }        
    }
} //end namespace
