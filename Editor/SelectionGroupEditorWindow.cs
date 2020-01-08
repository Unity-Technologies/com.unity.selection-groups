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

        static SelectionGroupEditorWindow()
        {
        }

        static void SanitizeSceneReferences()
        {
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
