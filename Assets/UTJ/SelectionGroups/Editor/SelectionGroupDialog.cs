using UnityEditor;
using UnityEngine;

namespace Utj.Film
{
    public class SelectionGroupDialog : EditorWindow
    {
        SerializedProperty property;
        SerializedProperty queryProperty;
        string groupName;

        public static void Open(SerializedProperty property)
        {
            var dialog = EditorWindow.GetWindow<SelectionGroupDialog>();
            dialog.ShowPopup();
            dialog.Configure(property);
        }

        void Configure(SerializedProperty property)
        {
            this.property = property;
            queryProperty = property.FindPropertyRelative("selectionQuery");
            groupName = property.FindPropertyRelative("groupName").stringValue;
            titleContent.text = "Selection Group Settings";
        }

        void OnGUI()
        {
            if (property == null || queryProperty == null)
            {
                Close();
                return;
            }
            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                GUILayout.BeginVertical("box");

                var nameProperty = property.FindPropertyRelative("groupName");
                var colorProperty = property.FindPropertyRelative("color");
                var showMembersProperty = property.FindPropertyRelative("showMembers");
                EditorGUILayout.PropertyField(nameProperty);
                EditorGUILayout.PropertyField(colorProperty);
                EditorGUILayout.PropertyField(showMembersProperty);
                GUILayout.EndVertical();
                GUILayout.Space(EditorGUIUtility.singleLineHeight);
                GUILayout.BeginVertical("box");
                EditorGUILayout.PropertyField(queryProperty, true);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical("box");
                EditorGUILayout.PropertyField(property.FindPropertyRelative("attachments"), true);
                GUILayout.EndVertical();
                if (cc.changed)
                {
                    // property.serializedObject.ApplyModifiedProperties();
                }
            }
            if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape)
                Close();
        }

    }
}
