using System.IO;

namespace Unity.SelectionGroups.Editor 
{
    internal class SelectionGroupEditorConstants 
    {
        //USS
        private static readonly  string s_USSFolder = Path.Combine(SelectionGroupConstants.PackagesPath,"Editor/StyleSheets");
        internal static readonly string ProjectSettingsStylePath = Path.Combine(s_USSFolder,"ProjectSettings_Style");
        
        // XML
        internal static readonly string MainProjectSettingsPath = ProjSettingsUIPath("ProjectSettings_Main");
        
        private static string ProjSettingsUIPath(string uiElementRelativePath) 
        {
            const string uielementsPath = "Editor/UIElements/ProjectSettings";
            return Path.Combine(SelectionGroupConstants.PackagesPath, uielementsPath,uiElementRelativePath);
        }
    }
}