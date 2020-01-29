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

        const int LEFT_MOUSE_BUTTON = 0;
        const int RIGHT_MOUSE_BUTTON = 1;

        static readonly Color SELECTION_COLOR = new Color32(62, 95, 150, 255);
        static readonly Color HOVER_COLOR = new Color32(112, 112, 112, 128);

        ReorderableList list;
        Vector2 scroll;
        SelectionGroup activeSelectionGroup;
        float width;
        static SelectionGroupEditorWindow editorWindow;
        Rect? hotRect = null;
        GUIStyle miniButtonStyle;
        HashSet<Object> activeSelection = new HashSet<Object>();
        SelectionOperation nextSelectionOperation;
        HashSet<string> activeNames = new HashSet<string>();

        Object hotMember;

        enum SelectionCommand
        {
            Add,
            Remove,
            Set,
            None
        }

        class SelectionOperation
        {
            public SelectionCommand command;
            public Object gameObject;
        }

        static void CreateNewGroup(Object[] objects)
        {
            Undo.RegisterCompleteObjectUndo(SelectionGroupManager.instance, "Create");
            var g = SelectionGroupManager.instance.CreateGroup("New Group");
            g.Add(objects);
        }

        void QueueSelectionOperation(SelectionCommand command, Object gameObject)
        {
            if (nextSelectionOperation == null)
            {
                nextSelectionOperation = new SelectionOperation() { command = command, gameObject = gameObject };
            }
        }

        void PerformSelectionCommands()
        {
            if (nextSelectionOperation != null)
            {
                if (nextSelectionOperation.gameObject != null)
                    switch (nextSelectionOperation.command)
                    {
                        case SelectionCommand.Add:
                            Selection.objects = Selection.objects.Append(nextSelectionOperation.gameObject).ToArray();
                            break;
                        case SelectionCommand.Remove:
                            Selection.objects = (from i in Selection.objects where i != nextSelectionOperation.gameObject select i).ToArray();
                            break;
                        case SelectionCommand.Set:
                            Selection.objects = new Object[] { nextSelectionOperation.gameObject };
                            break;
                    }
                nextSelectionOperation = null;
            }
        }

    }
}
