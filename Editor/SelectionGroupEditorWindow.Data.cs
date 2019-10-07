using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Unity.SelectionGroups
{

    public partial class SelectionGroupEditorWindow : EditorWindow
    {
        const int RIGHT_MOUSE_BUTTON = 1;

        ReorderableList list;
        SerializedObject serializedObject;
        SelectionGroupContainer selectionGroups;
        Vector2 scroll;
        SerializedProperty activeSelectionGroup;
        float width;
        static SelectionGroupEditorWindow editorWindow;
        Rect? hotRect = null;
        string hotGroup = null;
        GUIStyle miniButtonStyle;
    }
}
