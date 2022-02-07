using System;
using Unity.FilmInternalUtilities;


namespace Unity.SelectionGroups {

[Json("ProjectSettings/SelectionGroupsSettings.asset")]
[Serializable]
internal class SelectionGroupsEditorProjectSettings : BaseJsonSingleton<SelectionGroupsEditorProjectSettings> {

    protected override int GetLatestVersionV() => LATEST_VERSION;

    protected override void UpgradeToLatestVersionV(int prevVersion, int curVersion) {
        
    }
    
//----------------------------------------------------------------------------------------------------------------------
    private const int LATEST_VERSION = (int) Version.INITIAL; 
    enum Version {
        INITIAL = 1,
    };


}

} //end namespace