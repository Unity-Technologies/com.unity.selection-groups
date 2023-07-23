using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Unity.SelectionGroups.Editor
{
    [CustomPropertyDrawer(typeof(SelectionGroup))]
    internal class SelectionGroupPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // We need to get whether a option has been selected in the popup
            // and assign the property if it has.
            if (SelectionGroupsPopup.HasSelection(property.propertyPath))
                property.objectReferenceValue = SelectionGroupsPopup.UseSelectedGroup();

            // We create a rect over the button on the right side of the
            // object field that opens the object selector.
            Rect selectorRect = position;
            selectorRect.xMin = selectorRect.xMax - 14;
            selectorRect.yMax += 1;
            
            // We get and use the mouse input if the object selector would have opened.
            // We do it here before the object field otherwise
            // the object field would get and already use the input event.
            Event evt = Event.current;
            if (evt.type == EventType.MouseDown && evt.button == 0 && 
                selectorRect.Contains(evt.mousePosition))
            {
                // We only want to show the dropdown under the actual field and not the label as well.
                var fieldRect = position;
                fieldRect.xMin += EditorGUIUtility.labelWidth;
                SelectionGroupsPopup.Show(fieldRect, new Vector2(fieldRect.width, 80), property.propertyPath);
                
                evt.Use();
                GUIUtility.ExitGUI();
            }
            
            EditorGUI.ObjectField(position, property);

            EditorGUI.EndProperty();
        }
    }
}
