using UnityEditor;
using UnityEngine;

namespace Utj.Film
{
    public class DynamicSelectionDialog : EditorWindow
    {
        SerializedProperty property;
        SerializedProperty queryProperty;
        string groupName;

        public static void Open(SerializedProperty property)
        {
            var dialog = EditorWindow.GetWindow<DynamicSelectionDialog>();
            dialog.ShowPopup();
            dialog.Configure(property);
        }

        void Configure(SerializedProperty property)
        {
            this.property = property;
            queryProperty = property.FindPropertyRelative("selectionQuery");
            groupName = property.FindPropertyRelative("groupName").stringValue;
            titleContent.text = "Dynamic Selection";
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
                GUILayout.Label($"Dynamic Selection: {groupName}");
                EditorGUILayout.PropertyField(queryProperty, true);
                if (cc.changed)
                {
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }

    }
}
