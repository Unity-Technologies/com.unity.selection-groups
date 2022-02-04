using System.IO;

namespace Unity.SelectionGroups.Editor {

internal class SelectionGroupsEditorConstants {

    //USS
    private static readonly  string USS_FOLDER = Path.Combine(SelectionGroupConstants.PACKAGES_PATH,"Editor/StyleSheets");
    internal static readonly string PROJECT_SETTINGS_STYLE_PATH = Path.Combine(USS_FOLDER,"ProjectSettings_Style");
    
    // XML
    internal static readonly string MAIN_PROJECT_SETTINGS_PATH = ProjSettingsUIPath("ProjectSettings_Main");

    private static readonly string PROJECT_SETTINGS_UIELEMENTS_PATH = Path.Combine(SelectionGroupConstants.PACKAGES_PATH,"Editor/UIElements/ProjectSettings");
    
//----------------------------------------------------------------------------------------------------------------------
    private static string ProjSettingsUIPath(string uiElementRelativePath) {
        return Path.Combine(PROJECT_SETTINGS_UIELEMENTS_PATH, uiElementRelativePath);
    }

}

} //end namespace