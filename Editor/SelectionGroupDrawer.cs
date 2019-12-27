using Unity.SelectionGroups.Runtime;
using UnityEditor;
using UnityEngine;


namespace Unity.SelectionGroups
{
    [CustomPropertyDrawer(typeof(SelectionGroupDropDownAttribute))]
    public class SelectionGroupDrawer : PropertyDrawer
    {
        string[] names;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (names == null) names = SelectionGroupManager.instance.GetGroupNames();
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
