using System.IO;

namespace Unity.SelectionGroups.Editor {

internal class SelectionGroupEditorConstants {

    //USS
    private static readonly  string USS_FOLDER = Path.Combine(SelectionGroupConstants.PackagesPath,"Editor/StyleSheets");
    internal static readonly string PROJECT_SETTINGS_STYLE_PATH = Path.Combine(USS_FOLDER,"ProjectSettings_Style");
    
    // XML
    internal static readonly string MAIN_PROJECT_SETTINGS_PATH = ProjSettingsUIPath("ProjectSettings_Main");

    
//----------------------------------------------------------------------------------------------------------------------
    private static string ProjSettingsUIPath(string uiElementRelativePath) {
        const string UIELEMENTS_PATH = "Editor/UIElements/ProjectSettings";
        return Path.Combine(SelectionGroupConstants.PackagesPath,UIELEMENTS_PATH,uiElementRelativePath);
        
    }

}

} //end namespace