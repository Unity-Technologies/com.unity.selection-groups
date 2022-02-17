using System.Linq;
using Unity.SelectionGroups;
using UnityEditor;
using UnityEngine;


namespace Unity.SelectionGroups.Editor
{
    /// <summary>
    /// The editor draw used in the inspector for SelectionGroup properties.
    /// </summary>
    [CustomPropertyDrawer(typeof(SelectionGroupDropDownAttribute))]
    public class SelectionGroupDrawer : PropertyDrawer
    {
        private string[] m_Names;

        /// <summary>
        /// Implements UI for SelectionGroup drawers.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="property"></param>
        /// <param name="label"></param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (m_Names == null) m_Names = SelectionGroupManager.GetOrCreateInstance().groupNames.ToArray();
            var name = property.stringValue;
            position = EditorGUI.PrefixLabel(position, label);
            var index = System.Array.IndexOf(m_Names, name);
            var newIndex = EditorGUI.Popup(position, index, m_Names);
            if (newIndex != index)
            {
                property.stringValue = m_Names[newIndex];
            }
        }
    }
}
