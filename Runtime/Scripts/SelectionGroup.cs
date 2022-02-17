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
        enum SGVersion 
        {
            Initial = 1,        //initial
            Ordered_0_6_0,      //The order of selection groups is maintained by SelectionGroupManager
            EditorState_0_7_2, //The data structure of m_editorToolsStates was changed
        } 
        
        private const int kCurrentSGVersion = (int) SGVersion.EditorState_0_7_2;

        private GoQL.ParseResult m_QueryParseResult = ParseResult.Empty;
        private bool m_RegisterOnEnable = false;

#if UNITY_EDITOR        
        private Action m_OnDestroyedInEditorCB = null;
#endif
        
        /// <summary>
        /// A color assigned to this group.
        /// </summary>
        [FormerlySerializedAs("color")] [HideInInspector]
        [SerializeField] private Color m_Color;
        
        /// <summary>
        /// If not empty, this is a GoQL query string used to create the set of matching objects for this group.
        /// </summary>
        [FormerlySerializedAs("query")] [HideInInspector]
        [SerializeField] private string m_Query = string.Empty;

        //Obsolete
        [Obsolete] [FormerlySerializedAs("_members")] [HideInInspector]
        [SerializeField] private Object[] _legacyMembers;
        [FormerlySerializedAs("members")] [HideInInspector]
        [SerializeField] private List<Object> m_Members = new List<Object>();
        
#pragma warning disable 414    
        [FormerlySerializedAs("sgVersion")] [HideInInspector]
        [SerializeField] private int m_SGVersion = kCurrentSGVersion; 
#pragma warning restore 414
        
        [Obsolete]
        [SerializeField] private List<bool> m_editorToolsStatus = null;
        [FormerlySerializedAs("m_editorToolsStates")] 
        [SerializeField] private EditorToolStates m_EditorToolsStates = new EditorToolStates();
        [FormerlySerializedAs("m_showMembersInWindow")]
        [SerializeField] private bool m_ShowMembersInWindow = true;

        /// <summary>
        /// Sets/gets the name of the SelectionGroup
        /// </summary>
        public string groupName
        {
            get { return gameObject.name; }
            set { gameObject.name = value; }
        }


        /// <summary>
        /// Sets/gets the query which will automatically include GameObjects from the hierarchy that match the query into the group.
        /// </summary>
        public string query
        {
            get { return m_Query; }
        }
        
        /// <summary>
        /// Gets the number of members in this SelectionGroup
        /// </summary>
        public int count
        {
            get { return m_Members.Count; }
        }
        
        /// <summary>
        /// Sets/gets the color of the SelectionGroup 
        /// </summary>
        public Color color
        {
            get { return m_Color; }
            set { m_Color = value; }
        }

        private void OnEnable()
        {
            RefreshHideFlagsInEditor();
            
            if (!m_RegisterOnEnable) 
                return;
            SelectionGroupManager.GetOrCreateInstance().Register(this);
            m_RegisterOnEnable = false;
        }

        private void OnDestroy()
        {
            //Check if the gameObject/component was deleted, instead of scene closure
            if (!gameObject.scene.isLoaded) 
                return;
            SelectionGroupManager.GetOrCreateInstance().Unregister(this);
        }

        /// <summary>
        /// Sets the query which will automatically include GameObjects from the hierarchy that match the query into the group.
        /// Returns early when the query is not valid (GoQL.ParseResult.OK).
        /// </summary>
        /// <param name="q">The query</param>
        /// <returns>The parse result of the query.</returns>
        public void SetQuery(string q)
        {

            if (string.IsNullOrEmpty(q)) {
                this.m_Query = q;
                return;
            }

            this.m_Query = q;
            UpdateQueryResults();
        }

        internal void UpdateQueryResults() 
        {
            GoQL.GoQLExecutor executor = new GoQL.GoQLExecutor();
            GoQL.Parser.Parse(this.m_Query, out m_QueryParseResult);
            if (m_QueryParseResult != GoQL.ParseResult.OK) 
                return;
            
            executor.Code = this.m_Query;
            GameObject[] objects = executor.Execute();
            SetMembersInternal(objects);
        }
        

        public GoQL.ParseResult GetLastQueryParseResult() => m_QueryParseResult;
        
        
         /// <summary>
         /// Gets whether the group is automatically filled
         /// </summary>
         public bool IsAutoFilled()
         {
            return !string.IsNullOrEmpty(query);
         }
        
        

         internal bool GetEditorToolState(int toolID) 
         {
            if (m_EditorToolsStates.TryGetValue(toolID, out bool status))
                return status;

            return false;
         }

        public void EnableEditorTool(int toolID, bool toolEnabled) 
        {
            m_EditorToolsStates[toolID] = toolEnabled; 
        }
        
        

#if UNITY_EDITOR
        internal bool AreMembersShownInWindow() => m_ShowMembersInWindow;

        internal void ShowMembersInWindow(bool show) {
            m_ShowMembersInWindow = show;
        }
#endif

        /// <summary>
         /// Get the members of the SelectionGroup
         /// </summary>
        public IList<Object> Members
        {
            get { return m_Members; }
        }

        [NotNull]
        internal List<GameObject> FindGameObjectMembers() 
        {
            List<GameObject> ret = new List<GameObject>();
            foreach (Object m in m_Members) {
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
         public void Add(IEnumerable<Object> objects) 
         {
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
        public void Add(Object obj) 
        {
            if (null == obj)
                return;
            
            if(!m_Members.Contains(obj))
                m_Members.Add(obj);
        }
        
//----------------------------------------------------------------------------------------------------------------------        
        
         /// <summary>
         /// Clears and set the members of the SelectionGroup 
         /// </summary>
         /// <param name="objects">A enumerable collection of objects to be added</param>
         public void SetMembers(IEnumerable<Object> objects) 
         {
            if (IsAutoFilled()) {
                Debug.LogWarning($"[SG] Group {groupName} is auto-filled. Can't manually set members");
                return;
            }
                
            SetMembersInternal(objects);
         }

        private void SetMembersInternal(IEnumerable<Object> objects) 
        {
            m_Members.Clear();
            foreach (var i in objects) 
            {
                if (i == null)
                    continue;
                m_Members.Add(i);
            }
        }

         /// <summary>
         /// Removes a list of objects from the SelectionGroup 
         /// </summary>
         /// <param name="objectReferences">A list of objects to be removed</param>
        public void Remove(IEnumerable<Object> objectReferences) 
         {
            if (IsAutoFilled())
                return;
            
            m_Members.RemoveAll(a=> objectReferences.Contains(a));
            RemoveNullMembers();
        }
        
        /// <summary>
        /// Removes an object from the SelectionGroup.
        /// Does nothing if the group is automatically filled. 
        /// </summary>
        /// <param name="obj">The object to be removed</param>
        public void Remove(Object obj) 
        {
            if (IsAutoFilled())
                return;
            
            m_Members.Remove(obj);
        }

         /// <summary>
         /// Clears the members of the SelectionGroup
         /// </summary>
        public void Clear()
        {
            m_Members.Clear();
        }

        /// <summary>
        /// Enumerate components from all members of a group that have a certain type T.
        /// </summary>
        /// <typeparam name="T">The type of the component</typeparam>
        /// <returns>The enumerated component</returns>
        internal IEnumerable<T> GetMemberComponents<T>() where T : Component
        {
            foreach (var member in m_Members)
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
            for (int i = m_Members.Count-1; i >= 0 ; --i) {
                if (null != m_Members[i])
                    continue;
                
                m_Members.RemoveAt(i);
            }
        }

        /// <inheritdoc/>
        public void OnBeforeSerialize() 
        {
            m_SGVersion = kCurrentSGVersion;
        }

        /// <inheritdoc/>
        public void OnAfterDeserialize()
        {
            //if we have legacyMembers but no current members
            if (null != _legacyMembers && _legacyMembers.Length > 0 && (null == m_Members || m_Members.Count <= 0)) 
            {
                m_Members = new List<Object>();
                m_Members.AddRange(_legacyMembers);                 
                _legacyMembers = null; //clear
            }

            if (m_SGVersion < (int) SGVersion.Ordered_0_6_0) {
                m_RegisterOnEnable = true;
            }

            if (m_SGVersion < (int) SGVersion.EditorState_0_7_2) {
#pragma warning disable 612 //obsolete
                if (null != m_editorToolsStatus) {
                    int numStates = m_editorToolsStatus.Count;
                    for (int i = 0; i < numStates; ++i) {
                        m_EditorToolsStates[i] = m_editorToolsStatus[i];
                    }
                    m_editorToolsStatus = null;
#pragma warning restore 612
                }
            }
            
            m_SGVersion = kCurrentSGVersion;
        }

#if UNITY_EDITOR        
        internal void SetOnDestroyedInEditorCallback(Action cb) {
            m_OnDestroyedInEditorCB = cb;
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
    }
} //end namespace
