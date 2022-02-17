using System;
using System.Collections.Generic;
using Unity.FilmInternalUtilities;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;


namespace Unity.SelectionGroups 
{
    [Json("ProjectSettings/SelectionGroupsSettings.asset")]
    [Serializable]
    internal class SelectionGroupsEditorProjectSettings : BaseJsonSingleton<SelectionGroupsEditorProjectSettings> 
    {
        private enum SGProjectSettingsVersion 
        {
            Initial = 1,
            EditorState_0_7_2, //The data structure of m_defaultGroupEditorToolStates was changed
        };
        
        private const int kLatestVersion = (int) SGProjectSettingsVersion.EditorState_0_7_2; 
        
        [FormerlySerializedAs("m_groupsVisibleInHierarchy")] 
        [SerializeField] private bool m_GroupsVisibleInHierarchy = true;
        
        [Obsolete]
        [SerializeField] private List<bool> m_defaultGroupEditorToolStatus = null;
        
        [FormerlySerializedAs("m_defaultGroupEditorToolStates")] 
        [SerializeField] private EditorToolStates m_DefaultGroupEditorToolStates = new EditorToolStates(); 
        
        protected override int GetLatestVersionV() => kLatestVersion;

        protected override void UpgradeToLatestVersionV(int prevVersion, int curVersion) 
        {
            if (prevVersion < (int) SGProjectSettingsVersion.EditorState_0_7_2) 
            {
    #pragma warning disable 612 //obsolete
                if (null != m_defaultGroupEditorToolStatus) 
                {
                    int numStates = m_defaultGroupEditorToolStatus.Count;
                    for (int i = 0; i < numStates; ++i) 
                    {
                        m_DefaultGroupEditorToolStates[i] = m_defaultGroupEditorToolStatus[i];
                    }
                    m_defaultGroupEditorToolStatus = null;
    #pragma warning restore 612
                }
            }
        }
        
        internal bool AreGroupsVisibleInHierarchy() => m_GroupsVisibleInHierarchy; 
        
        internal void ShowGroupsInHierarchy(bool visible) 
        {
            m_GroupsVisibleInHierarchy = visible; 
        }

        internal bool GetDefaultGroupEditorToolState(int toolID) 
        {
            if (m_DefaultGroupEditorToolStates.TryGetValue(toolID, out bool status))
                return status;
            return false;
        }

        public void EnableDefaultGroupEditorTool(int toolID, bool toolEnabled) 
        {
            m_DefaultGroupEditorToolStates[toolID] = toolEnabled;
        }
    }
}