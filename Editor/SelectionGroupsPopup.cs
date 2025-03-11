using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.SelectionGroups.Editor
{
    // Implementation note: You cannot pass a SerializedProperty from a
    // PropertyDrawer to another window because after the drawer is finished 
    // the property is disposed of, so one way to get around this is
    // to set a static property to the selected value
    // and then have the property drawer check each draw if there is a selected value.
    // There are other ways to do it too.
    
    internal class SelectionGroupsPopup : EditorWindow
    {
        static SelectionGroup selectedGroup;
        static string selectionId;
        static bool hasSelection;

        List<SelectionGroup> groups;

        public static bool HasSelection(string id)
        {
            return hasSelection && selectionId == id;
        }

        public static SelectionGroup UseSelectedGroup()
        {
            hasSelection = false;
            return selectedGroup;
        }

        public static void Show(Rect buttonRect, Vector2 size, string id)
        {
            selectionId = id;
            hasSelection = false;
            var popup = CreateInstance<SelectionGroupsPopup>();

            popup.ShowAsDropDown(GUIUtility.GUIToScreenRect(buttonRect), size);
        }

        private void OnEnable()
        {
            // We create a new list so we can have a "none" option in the window.
            groups = new List<SelectionGroup>(SelectionGroupManager.GetOrCreateInstance().Groups);
            groups.Insert(0, null);
        }

        private void CreateGUI()
        {
            var list = new ListView(groups, 21, MakeItem, BindItem);
            list.style.flexGrow = 1;
            list.style.borderLeftColor = Color.black;
            list.style.borderRightColor = Color.black;
            list.style.borderTopColor = Color.black;
            list.style.borderBottomColor = Color.black;

            list.style.borderLeftWidth = 1;
            list.style.borderRightWidth = 1;
            list.style.borderTopWidth = 1;
            list.style.borderBottomWidth = 1;

            rootVisualElement.Add(list);
        }
        
        private VisualElement MakeItem()
        {
            return new Label();
        }

        private void BindItem(VisualElement element, int index)
        {
            var group = groups[index];
            ((Label) element).text = group != null? group.Name : "None";
            element.userData = group;
            element.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            selectedGroup = (SelectionGroup) ((VisualElement) evt.currentTarget).userData;
            hasSelection = true;
            Close();
        }
    }
}
