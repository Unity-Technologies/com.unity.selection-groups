using System;
using Unity.FilmInternalUtilities;
using UnityEngine;


namespace Unity.SelectionGroups {

[Serializable]
internal class SelectionGroupsEditorProjectSettings : BaseJsonSettings {

    internal static SelectionGroupsEditorProjectSettings GetOrCreateSettings() {
        
        if (null != m_instance) {
            return m_instance;
        }

        lock (m_lock) {
        
#if UNITY_EDITOR
            const string PATH = SELECTION_GROUPS_EDITOR_PROJECT_SETTINGS_PATH;
            m_instance = DeserializeFromJson<SelectionGroupsEditorProjectSettings>(PATH);
            if (null != m_instance) {
                return m_instance;
            }
#endif
            
            m_instance = new SelectionGroupsEditorProjectSettings();
#if UNITY_EDITOR
            m_instance.Save();
#endif
        }        

        return m_instance;
        
    }

    internal SelectionGroupsEditorProjectSettings() : base(SELECTION_GROUPS_EDITOR_PROJECT_SETTINGS_PATH) {
        
    }
   
//----------------------------------------------------------------------------------------------------------------------
    protected override object GetLockV()        { return m_lock; }
    
//----------------------------------------------------------------------------------------------------------------------

    protected override void OnDeserializeV() {
        if (m_selectionGroupsProjectSettingsVersion == LATEST_VERSION) {
            return;
        }
        
        m_selectionGroupsProjectSettingsVersion = LATEST_VERSION;
        Save();
    }
    
    
//----------------------------------------------------------------------------------------------------------------------
    

    [SerializeField] private int m_selectionGroupsProjectSettingsVersion = LATEST_VERSION;   
    
//----------------------------------------------------------------------------------------------------------------------

    private static SelectionGroupsEditorProjectSettings m_instance = null;
    private static readonly object m_lock = new object();

    private const string SELECTION_GROUPS_EDITOR_PROJECT_SETTINGS_PATH = "ProjectSettings/SelectionGroupsSettings.asset";

    private const int LATEST_VERSION = (int) Version.INITIAL; 
    enum Version {
        INITIAL = 1,
    };


}

} //end namespace