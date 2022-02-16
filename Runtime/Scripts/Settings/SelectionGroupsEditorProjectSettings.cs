using System;
using System.Collections.Generic;
using Unity.FilmInternalUtilities;
using UnityEngine;
using UnityEngine.Assertions;


namespace Unity.SelectionGroups {

[Json("ProjectSettings/SelectionGroupsSettings.asset")]
[Serializable]
internal class SelectionGroupsEditorProjectSettings : BaseJsonSingleton<SelectionGroupsEditorProjectSettings> {

    protected override int GetLatestVersionV() => LATEST_VERSION;

    protected override void UpgradeToLatestVersionV(int prevVersion, int curVersion) {
        
        if (prevVersion < (int) SGProjectSettingsVersion.EDITOR_STATE_0_7_2) {
#pragma warning disable 612 //obsolete
            if (null != m_defaultGroupEditorToolStatus) {
                int numStates = m_defaultGroupEditorToolStatus.Count;
                for (int i = 0; i < numStates; ++i) {
                    m_defaultGroupEditorToolStates[i] = m_defaultGroupEditorToolStatus[i];
                }
                m_defaultGroupEditorToolStatus = null;
#pragma warning restore 612
            }
        }
        
    }
    
//----------------------------------------------------------------------------------------------------------------------

    internal bool AreGroupsVisibleInHierarchy() => m_groupsVisibleInHierarchy; 
    
    internal void ShowGroupsInHierarchy(bool visible) {
        m_groupsVisibleInHierarchy = visible; 
    }

    internal bool GetDefaultGroupEditorToolState(int toolID) {
        if (m_defaultGroupEditorToolStates.TryGetValue(toolID, out bool status))
            return status;
        return false;
    }

    public void EnableDefaultGroupEditorTool(int toolID, bool toolEnabled) {
        m_defaultGroupEditorToolStates[toolID] = toolEnabled;
    }
    
    
//----------------------------------------------------------------------------------------------------------------------
    
    [SerializeField] private bool m_groupsVisibleInHierarchy       = true;
    
    [Obsolete]
    [SerializeField] List<bool> m_defaultGroupEditorToolStatus = null;
    
    [SerializeField] EditorToolStates m_defaultGroupEditorToolStates = new EditorToolStates(); 
    
//----------------------------------------------------------------------------------------------------------------------
    private const int LATEST_VERSION = (int) SGProjectSettingsVersion.INITIAL; 
    enum SGProjectSettingsVersion {
        INITIAL = 1,
        EDITOR_STATE_0_7_2, //The data structure of m_defaultGroupEditorToolStates was changed
        
    };


}

} //end namespace