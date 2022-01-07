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
        string[] names;

        /// <summary>
        /// Implements UI for SelectionGroup drawers.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="property"></param>
        /// <param name="label"></param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (names == null) names = SelectionGroupManager.GetOrCreateInstance().GroupNames.ToArray();
            var name = property.stringValue;
            position = EditorGUI.PrefixLabel(position, label);
            var index = System.Array.IndexOf(names, name);
            var newIndex = EditorGUI.Popup(position, index, names);
            if (newIndex != index)
            {
                property.stringValue = names[newIndex];
            }
        }
    }
}
