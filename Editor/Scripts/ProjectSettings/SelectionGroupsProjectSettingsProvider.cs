using System.Collections.Generic;
using Unity.FilmInternalUtilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Unity.SelectionGroups.Editor {
	
class SelectionGroupsProjectSettingsProvider : SettingsProvider {
	
	private class Contents {
//		public static readonly GUIContent MaskChannel = EditorGUIUtility.TrTextContent("Default Mask Channel");
	}

	
//----------------------------------------------------------------------------------------------------------------------		
	
	SelectionGroupsProjectSettingsProvider() : base(PROJECT_SETTINGS_MENU_PATH,SettingsScope.Project) {
		
		//activateHandler is called when the user clicks on the Settings item in the Settings window.
		activateHandler = (string searchContext, VisualElement root) => {
			
			//Main Tree
			VisualTreeAsset main = UIElementsEditorUtility.LoadVisualTreeAsset(SelectionGroupEditorConstants.MAIN_PROJECT_SETTINGS_PATH);
			main.CloneTree(root);
					
			//Style
			UIElementsEditorUtility.LoadAndAddStyle(root.styleSheets, SelectionGroupEditorConstants.PROJECT_SETTINGS_STYLE_PATH);
			
			//add fields
			VisualElement defaultSectionContainer = root.Query<VisualElement>("DefaultSectionContainer");
			Assert.IsNotNull(defaultSectionContainer);

			//
			// SelectionGroupsEditorProjectSettings projSettings = SelectionGroupsEditorProjectSettings.GetOrCreateSettings();			
			// UIElementsEditorUtility.AddPopupField<ColorChannel>(defaultSectionContainer, Contents.MaskChannel,
			// 	m_colorChannelEnums, projSettings.GetDefaultMaskChannel(),
			// 	(e) => {
			// 		projSettings.SetDefaultMaskChannel(e.newValue);
			// 		projSettings.Save();
			// 	});
			//
			
		};	
		
		deactivateHandler = () => {
		};
		
		
		//keywords
		HashSet<string> vcKeywords = new HashSet<string>(new[] { "Visual Compositor",});
		vcKeywords.UnionWith(GetSearchKeywordsFromGUIContentProperties<Contents>());		
		keywords = vcKeywords;
		
	}
	
	
//----------------------------------------------------------------------------------------------------------------------

    [SettingsProvider]
    internal static SettingsProvider CreateVisualCompositorSettingsProvider() {
	    m_projectSettingsProvider = new SelectionGroupsProjectSettingsProvider();
	    return m_projectSettingsProvider;
    }
    
	
//----------------------------------------------------------------------------------------------------------------------

	private static SelectionGroupsProjectSettingsProvider m_projectSettingsProvider = null;

	private const string PROJECT_SETTINGS_MENU_PATH = "Project/Visual Compositor";

	
//----------------------------------------------------------------------------------------------------------------------
	
}

	
}
