using System.Collections.Generic;
using Unity.FilmInternalUtilities;
using Unity.FilmInternalUtilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Unity.SelectionGroups.Editor {
class SelectionGroupsProjectSettingsProvider : SettingsProvider {
    private class Contents {
		public static readonly GUIContent SHOW_GROUPS_IN_HIERARCHY = EditorGUIUtility.TrTextContent("Show groups in Hierarchy");

        public static readonly GUIContent[] DEFAULT_GROUP_EDITOR_TOOL = new GUIContent[] {
            EditorGUIUtility.TrTextContent("Enable eye toolbar button by default"),
            EditorGUIUtility.TrTextContent("Enable lock toolbar button by default"),
        };

    }


//----------------------------------------------------------------------------------------------------------------------		

    SelectionGroupsProjectSettingsProvider() : base(PROJECT_SETTINGS_MENU_PATH, SettingsScope.Project) {
        //activateHandler is called when the user clicks on the Settings item in the Settings window.
        activateHandler = (string searchContext, VisualElement root) => {
            //Main Tree
            VisualTreeAsset main =
                UIElementsEditorUtility.LoadVisualTreeAsset(SelectionGroupEditorConstants.MAIN_PROJECT_SETTINGS_PATH);
            main.CloneTree(root);

            //Style
            UIElementsEditorUtility.LoadAndAddStyle(root.styleSheets,
                SelectionGroupEditorConstants.PROJECT_SETTINGS_STYLE_PATH);

            //add fields
            VisualElement defaultSectionContainer = root.Query<VisualElement>("DefaultSectionContainer");
            Assert.IsNotNull(defaultSectionContainer);

            SelectionGroupsEditorProjectSettings projSettings =
                SelectionGroupsEditorProjectSettings.GetOrCreateInstance();
            
            
            UIElementsEditorUtility.AddField<Toggle, bool>(defaultSectionContainer, Contents.SHOW_GROUPS_IN_HIERARCHY, 
                projSettings.AreGroupsVisibleInHierarchy(),
                (e) => {
                    projSettings.ShowGroupsInHierarchy(e.newValue);
                    projSettings.SaveInEditor();
                    RefreshGroupHideFlagsInEditor();
                }
            );

            for (int i = 0; i < (int)SelectionGroupToolType.BuiltIn_Max; ++i) {
                int toolID = i;
                UIElementsEditorUtility.AddField<Toggle, bool>(defaultSectionContainer, 
                    Contents.DEFAULT_GROUP_EDITOR_TOOL[i], 
                    projSettings.GetDefaultGroupEditorToolState(toolID),
                    (e) => {
                        projSettings.EnableDefaultGroupEditorTool(toolID, e.newValue);
                        projSettings.SaveInEditor();
                    }
                );
                
            }
            
        };

        deactivateHandler = () => { };


        //keywords
        HashSet<string> sgKeywords = new HashSet<string>(new[] { "Selection Groups", "Selection Group" });
        sgKeywords.UnionWith(GetSearchKeywordsFromGUIContentProperties<Contents>());
        keywords = sgKeywords;
    }


//----------------------------------------------------------------------------------------------------------------------

    [SettingsProvider]
    internal static SettingsProvider CreateSelectionGroupsSettingsProvider() {
        m_projectSettingsProvider = new SelectionGroupsProjectSettingsProvider();
        return m_projectSettingsProvider;
    }


//----------------------------------------------------------------------------------------------------------------------
    
    internal void RefreshGroupHideFlagsInEditor() {
        SelectionGroupManager sgManager = SelectionGroupManager.GetOrCreateInstance();
        sgManager.Groups.Loop((SelectionGroup group) => {
            group.RefreshHideFlagsInEditor();
        });
        SelectionGroupManager.UpdateQueryResults();
        EditorApplication.RepaintHierarchyWindow();
        EditorApplication.DirtyHierarchyWindowSorting();
        
        SelectionGroupEditorWindow.TryRepaint();
    }
    

    private static SelectionGroupsProjectSettingsProvider m_projectSettingsProvider = null;

    private const string PROJECT_SETTINGS_MENU_PATH = "Project/Selection Groups";


//----------------------------------------------------------------------------------------------------------------------
}
}