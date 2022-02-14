using NUnit.Framework;
using Unity.FilmInternalUtilities.Editor;
using Unity.SelectionGroups.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.SelectionGroups.EditorTests {

public class ResourcesTests {

    [Test]
    public void CheckStyleSheets() {
        CheckSingleStyleSheet(SelectionGroupEditorConstants.PROJECT_SETTINGS_STYLE_PATH);
    }

    void CheckSingleStyleSheet(string path) {
        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath(path + ".uss", typeof(StyleSheet)) as StyleSheet;
        Assert.IsNotNull(styleSheet);

    }

//----------------------------------------------------------------------------------------------------------------------
    
    [Test]
    public void CheckUIElements() {
        CheckSingleTemplate(SelectionGroupEditorConstants.MAIN_PROJECT_SETTINGS_PATH);
    }
    
    void CheckSingleTemplate(string path) {
        Assert.IsNotNull(UIElementsEditorUtility.LoadVisualTreeAsset(path));
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
}

} //end namespace
