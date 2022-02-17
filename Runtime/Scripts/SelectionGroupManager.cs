using System;
using System.Collections.Generic;
using System.Linq;
using Unity.FilmInternalUtilities;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.SelectionGroups 
{
    [ExecuteAlways]
    [AddComponentMenu("")]
    internal class SelectionGroupManager : MonoBehaviourSingleton<SelectionGroupManager>, ISerializationCallbackReceiver 
    {
        [FormerlySerializedAs("m_sceneSelectionGroups")] 
        [SerializeField] private List<SelectionGroup> m_SceneSelectionGroups = new List<SelectionGroup>();
        
        internal IList<SelectionGroup> groups => m_SceneSelectionGroups;

        internal IEnumerable<string> groupNames => m_SceneSelectionGroups.Select(g => g.groupName);
        
        private void OnEnable() 
        {
            gameObject.hideFlags = HideFlags.HideInHierarchy;
        }

        public static void UpdateQueryResults() 
        {
            foreach (var i in SelectionGroupManager.GetOrCreateInstance().m_SceneSelectionGroups) 
            {
                if (!string.IsNullOrEmpty(i.query)) 
                {
                    i.UpdateQueryResults();
                }
            }
        }

        [Obsolete]
        internal SelectionGroup CreateSceneSelectionGroup(string groupName, string query, Color color, IList<Object> members) 
        {
            SelectionGroup group = CreateSelectionGroupInternal(groupName, color);        
            group.SetQuery(query);

            if (!group.IsAutoFilled()) 
            {
                group.Add(members);
            }

            m_SceneSelectionGroups.Add(group);
            return group;
        }

        [Obsolete]
        internal SelectionGroup CreateSceneSelectionGroup(string groupName, Color color) 
        {
            return CreateSelectionGroup(groupName, color);
        }
        
        [Obsolete]
        internal SelectionGroup CreateSceneSelectionGroup(string groupName, Color color, string query) 
        {
            return CreateSelectionGroup(groupName, color, query);
        }

        [Obsolete]
        internal SelectionGroup CreateSceneSelectionGroup(string groupName, Color color, IList<Object> members) 
        {
            return CreateSelectionGroup(groupName, color, members);
        }

        //
        internal SelectionGroup CreateSelectionGroup(string groupName, Color color) 
        {
            SelectionGroup group = CreateSelectionGroupInternal(groupName, color);
            m_SceneSelectionGroups.Add(group);
            return group;
        }
        
        internal SelectionGroup CreateSelectionGroup(string groupName, Color color, string query)
        {
            SelectionGroup group = CreateSelectionGroupInternal(groupName, color);        
            group.SetQuery(query);
            m_SceneSelectionGroups.Add(group);
            return group;
        }

        internal SelectionGroup CreateSelectionGroup(string groupName, Color color, IList<Object> members)
        {
            SelectionGroup group = CreateSelectionGroupInternal(groupName, color);
            group.Add(members);
            m_SceneSelectionGroups.Add(group);
            return group;
        }

        private static SelectionGroup CreateSelectionGroupInternal(string groupName, Color color)
        {
            GameObject g = new GameObject(groupName);
    #if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(g, "New Scene Selection Group");
    #endif
            SelectionGroup group = g.AddComponent<SelectionGroup>();
            group.groupName        = groupName;
            group.color       = color;
            
            SelectionGroupsEditorProjectSettings projSettings = SelectionGroupsEditorProjectSettings.GetOrCreateInstance();
            for (int i = 0; i < (int)SelectionGroupToolType.BUILT_IN_MAX; ++i) 
            {
                group.EnableEditorTool(i, projSettings.GetDefaultGroupEditorToolState(i));
            }
            
            return group;
        }

        //----------------------------------------------------------------------------------------------------------------------
        
        internal void ClearGroups() 
        {
            int numGroups = m_SceneSelectionGroups.Count;
            for (int i = numGroups - 1; i >= 0; --i) 
            {
                SelectionGroup g = m_SceneSelectionGroups[i];
                if (null == g)
                    continue;
                
                FilmInternalUtilities.ObjectUtility.Destroy(g.gameObject, forceImmediate: true);
            }
            m_SceneSelectionGroups.Clear();
        }

        internal void DeleteGroup(SelectionGroup group) 
        {
            
    #if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(this, "Delete Group");
            Undo.DestroyObjectImmediate(group.gameObject);
    #else
            DestroyImmediate(group,gameObject);
    #endif
            m_SceneSelectionGroups.Remove(group);
        }
        
        //----------------------------------------------------------------------------------------------------------------------

        internal void Register(SelectionGroup group) 
        {
            Assert.IsNotNull(group);
            m_SceneSelectionGroups.Add(group);
        }

        internal void Unregister(SelectionGroup group) 
        {
            m_SceneSelectionGroups.Remove(group);
        }
        
        internal void MoveGroup(int prevIndex, int newIndex) 
        {
    #if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(this, "Move Group");
    #endif
            m_SceneSelectionGroups.Move(prevIndex, newIndex);
        }

        //----------------------------------------------------------------------------------------------------------------------

        ///<inheritdoc/>
        public void OnBeforeSerialize() 
        {
            m_SceneSelectionGroups.RemoveAll((g) => null == g);
        }

        ///<inheritdoc/>
        public void OnAfterDeserialize() { }
        
        //----------------------------------------------------------------------------------------------------------------------

    }
}