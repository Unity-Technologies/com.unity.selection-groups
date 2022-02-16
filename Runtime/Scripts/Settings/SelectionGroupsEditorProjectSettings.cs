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
        
    }
    
//----------------------------------------------------------------------------------------------------------------------

    internal bool AreGroupsVisibleInHierarchy() => m_groupsVisibleInHierarchy; 
    
    internal void ShowGroupsInHierarchy(bool visible) {
        m_groupsVisibleInHierarchy = visible; 
    }

    internal bool GetDefaultGroupEditorToolStatus(int toolID) => m_defaultGroupEditorToolStatus[toolID];

    public void EnableDefaultGroupEditorTool(int toolID, bool toolEnabled) {
        m_defaultGroupEditorToolStatus[toolID] = toolEnabled;
    }
    
    
//----------------------------------------------------------------------------------------------------------------------
    
    [SerializeField] private bool m_groupsVisibleInHierarchy       = true;
    
    [SerializeField] SerializedDictionary<int, bool> m_defaultGroupEditorToolStatus = new SerializedDictionary<int,bool>(); 
    
//----------------------------------------------------------------------------------------------------------------------
    private const int LATEST_VERSION = (int) Version.INITIAL; 
    enum Version {
        INITIAL = 1,
    };


}

} //end namespace