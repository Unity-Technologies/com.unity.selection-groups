using System;
using Unity.FilmInternalUtilities;
using UnityEngine;


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

//----------------------------------------------------------------------------------------------------------------------
    
    [SerializeField] private bool m_groupsVisibleInHierarchy = true;

//----------------------------------------------------------------------------------------------------------------------
    private const int LATEST_VERSION = (int) Version.INITIAL; 
    enum Version {
        INITIAL = 1,
    };


}

} //end namespace