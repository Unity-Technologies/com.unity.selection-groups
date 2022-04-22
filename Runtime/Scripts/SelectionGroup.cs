using System;
using System.Collections;
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
    public class SelectionGroup : MonoBehaviour, IList<Object>, ISerializationCallbackReceiver    
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

        /// <summary>
        /// Gets the number of objects in the <see cref="SelectionGroup"/>
        /// </summary>
        public int Count
        {
            get { return members.Count; }
        }

        bool ICollection<Object>.IsReadOnly
        {
            get { return ((ICollection<Object>) members).IsReadOnly; }
        }
        
        public Object this[int index]
        {
            get { return members[index]; }
            set
            {
                if (IsAutoFilled())
                    return;
                
                members[index] = value;
            }
        }

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

        internal bool GetEditorToolState(int toolID) {
            if (m_editorToolsStates.TryGetValue(toolID, out bool status))
                return status;

            return false;
        }

        public void EnableEditorTool(int toolID, bool toolEnabled) {
            m_editorToolsStates[toolID] = toolEnabled;
        }


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

        /// <summary>
        /// Adds the objects in the specified collection to the end
        /// of the <see cref="SelectionGroup"/> if they are not already present.
        /// Does nothing if the <see cref="SelectionGroup"/> is automatically filled.
        /// </summary>
        /// <param name="objects">The collection of objects to add.</param>
        public void AddRange(IEnumerable<Object> objects) 
        {
             if (IsAutoFilled())
                 return; 
             
             foreach (Object obj in objects) 
             {
                 Add(obj);
             }
             RemoveNullMembers();
        }
        
        /// <summary>
        /// Adds the specified object to the <see cref="SelectionGroup"/>. 
        /// Does nothing if the <see cref="SelectionGroup"/> is automatically filled.
        /// </summary>
        /// <param name="obj">The object to be added. Cannot be <c>null</c>.</param>
        public void Add(Object obj) 
        {
            if (null == obj)
                return;
            
            if(!members.Contains(obj))
                members.Add(obj);
        }

        /// <summary>
        /// Inserts the specified object into the <see cref="SelectionGroup"/> at the specified index.
        /// Does nothing if the <see cref="SelectionGroup"/> is automatically filled. 
        /// </summary>
        /// <param name="index">The zero-based index the object should be inserted at.</param>
        /// <param name="obj">The object to be inserted.</param>
        public void Insert(int index, Object obj)
        {
            if (IsAutoFilled())
                return;
            
            members.Insert(index, obj);
        }
         
        /// <summary>
        /// Removes an object at the specified index.
        /// Does nothing if the <see cref="SelectionGroup"/> is automatically filled. 
        /// </summary>
        /// <param name="index">The zero based index of the object to remove.</param>
        public void RemoveAt(int index)
        { 
            if (IsAutoFilled())
                return;
            
            members.RemoveAt(index);
        }

         /// <summary>
         /// Removes the specified object from the <see cref="SelectionGroup"/>.
         /// Does nothing if the <see cref="SelectionGroup"/> is automatically filled. 
         /// </summary>
         /// <param name="obj">The object to be removed.</param>
         /// <returns>
         /// <c>true</c> if <paramref name="obj"/> is successfully removed; otherwise, <c>false</c>.
         /// </returns>
        public bool Remove(Object obj) 
        {
            if (IsAutoFilled())
                return false;
            
            return members.Remove(obj);
        }
         
         /// <summary>
         /// Removes all objects from the <see cref="SelectionGroup"/> that are in the specified collection.
         /// Does nothing if the <see cref="SelectionGroup"/> is automatically filled. 
         /// </summary>
         /// <param name="objectReferences">The collection of objects to be removed.</param>
         public void Except(IEnumerable<Object> objectReferences) 
         {
             if (IsAutoFilled())
                 return;
            
             members.RemoveAll(objectReferences.Contains);
             RemoveNullMembers();
         }

         /// <summary>
         /// Removes all objects from the <see cref="SelectionGroup"/>.
         /// </summary>
         public void Clear()
         {
             members.Clear();
         }

         /// <summary>
         /// Determines whether the specified object is in the <see cref="SelectionGroup"/>.
         /// </summary>
         /// <param name="obj">The object to locate in the <see cref="SelectionGroup"/>.</param>
         /// <returns>
         /// <c>true</c> if <paramref name="obj"/> was found
         /// in the <see cref="SelectionGroup"/>; otherwise, <c>false</c>.
         /// </returns>
         public bool Contains(Object obj)
         {
             return members.Contains(obj);
         }
         
         /// <summary>
         /// Searches for the specified object and returns the zero-based index of it.
         /// </summary>
         /// <param name="obj">The object to locate in the <see cref="SelectionGroup"/>.</param>
         /// <returns>
         /// The zero-based index of <paramref name="obj"/> within
         /// the <see cref="SelectionGroup"/> if found; otherwise, <c>-</c>.
         /// </returns>
         public int IndexOf(Object obj)
         {
             return members.IndexOf(obj);
         }
         
         /// <summary>
         /// Copies the entire <see cref="SelectionGroup"/> to a compatible one-dimensional array,
         /// starting at the specified index of the target array.
         /// </summary>
         /// <param name="array">
         /// The one-dimensional <see cref="Array"/> that is the destination of the objects
         /// copied from the <see cref="SelectionGroup"/>. The <see cref="Array"/> must have zero-based indexing.</param>
         /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
         public void CopyTo(Object[] array, int arrayIndex)
         {
             members.CopyTo(array, arrayIndex);
         }
         
         /// <inheritdoc cref="IList{T}.GetEnumerator"/>
         public IEnumerator<Object> GetEnumerator()
         {
             return members.GetEnumerator();
         }

         IEnumerator IEnumerable.GetEnumerator()
         {
             return GetEnumerator();
         }
         
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

            if (sgVersion < (int) SGVersion.Ordered_0_6_0) {
                m_registerOnEnable = true;
            }

            if (sgVersion < (int) SGVersion.EditorState_0_7_2) {
#pragma warning disable 612 //obsolete
                if (null != m_editorToolsStatus) {
                    int numStates = m_editorToolsStatus.Count;
                    for (int i = 0; i < numStates; ++i) {
                        m_editorToolsStates[i] = m_editorToolsStatus[i];
                    }
                    m_editorToolsStatus = null;
#pragma warning restore 612
                }
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

        [Obsolete]
        [SerializeField] List<bool> m_editorToolsStatus = null;
        
        [SerializeField] EditorToolStates m_editorToolsStates = new EditorToolStates(); 

        [SerializeField] private bool m_showMembersInWindow = true;
        

        private GoQL.ParseResult m_queryParseResult = ParseResult.Empty;       
        
        private const int  CUR_SG_VERSION     = (int) SGVersion.EditorState_0_7_2;
        private       bool m_registerOnEnable = false;

#if UNITY_EDITOR        
        private Action m_onDestroyedInEditorCB = null;
#endif        
        
        private enum SGVersion {
            Initial = 1,        //initial
            Ordered_0_6_0,      //The order of selection groups is maintained by SelectionGroupManager
            EditorState_0_7_2, //The data structure of m_editorToolsStates was changed
        }
    }
} //end namespace
