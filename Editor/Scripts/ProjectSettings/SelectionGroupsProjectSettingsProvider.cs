using System.Collections.Generic;
using Unity.FilmInternalUtilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Unity.SelectionGroups.Editor {
    internal class SelectionGroupsProjectSettingsProvider : SettingsProvider 
    {
        private class Contents 
        {
		    public static readonly GUIContent ShowGroupsInHierarchy = EditorGUIUtility.TrTextContent("Show groups in Hierarchy");

            public static readonly GUIContent[] DefaultGroupEditorTool = new GUIContent[] 
            {
                EditorGUIUtility.TrTextContent("Enable eye toolbar button by default"),
                EditorGUIUtility.TrTextContent("Enable lock toolbar button by default"),
            };
        }
        
        private const string kProjectSettingsMenuPath = "Project/Selection Groups";
        
        private static SelectionGroupsProjectSettingsProvider s_ProjectSettingsProvider = null;

        private SelectionGroupsProjectSettingsProvider() : base(kProjectSettingsMenuPath, SettingsScope.Project) 
        {
            //activateHandler is called when the user clicks on the Settings item in the Settings window.
            activateHandler = (string searchContext, VisualElement root) => 
            {
                //Main Tree
                VisualTreeAsset main =
                    UIElementsEditorUtility.LoadVisualTreeAsset(SelectionGroupEditorConstants.MainProjectSettingsPath);
                main.CloneTree(root);

                //Style
                UIElementsEditorUtility.LoadAndAddStyle(root.styleSheets,
                    SelectionGroupEditorConstants.ProjectSettingsStylePath);

                //add fields
                VisualElement defaultSectionContainer = root.Query<VisualElement>("DefaultSectionContainer");
                Assert.IsNotNull(defaultSectionContainer);

                SelectionGroupsEditorProjectSettings projSettings =
                    SelectionGroupsEditorProjectSettings.GetOrCreateInstance();
                
                
                UIElementsEditorUtility.AddField<Toggle, bool>(defaultSectionContainer, Contents.ShowGroupsInHierarchy, 
                    projSettings.AreGroupsVisibleInHierarchy(),
                    (e) => 
                    {
                        projSettings.ShowGroupsInHierarchy(e.newValue);
                        projSettings.SaveInEditor();
                        RefreshGroupHideFlagsInEditor();
                    }
                );

                for (int i = 0; i < (int)SelectionGroupToolType.BuiltInMAX; ++i) 
                {
                    int toolID = i;
                    UIElementsEditorUtility.AddField<Toggle, bool>(defaultSectionContainer, 
                        Contents.DefaultGroupEditorTool[i], 
                        projSettings.GetDefaultGroupEditorToolState(toolID),
                        (e) => 
                        {
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

        
        [SettingsProvider]
        internal static SettingsProvider CreateSelectionGroupsSettingsProvider() 
        {
            s_ProjectSettingsProvider = new SelectionGroupsProjectSettingsProvider();
            return s_ProjectSettingsProvider;
        }
        
        internal void RefreshGroupHideFlagsInEditor() 
        {
            SelectionGroupManager sgManager = SelectionGroupManager.GetOrCreateInstance();
            foreach (SelectionGroup group in sgManager.groups) 
            {
                group.RefreshHideFlagsInEditor();
            }
            SelectionGroupManager.UpdateQueryResults();
            EditorApplication.RepaintHierarchyWindow();
            EditorApplication.DirtyHierarchyWindowSorting();
            
            SelectionGroupEditorWindow.TryRepaint();
        }
    }
}