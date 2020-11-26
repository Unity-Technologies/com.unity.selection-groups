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
        bool HandleGroupDragEvents(Rect rect, SelectionGroup group)
        {
            Event evt = Event.current;

            switch (evt.type)
            {
                case EventType.DragExited:
                    ExitDrag();
                    break;
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!PerformDrag(rect, group, evt)) return false;
                    break;
            }
            return false;
        }
        bool PerformDrag(Rect rect, SelectionGroup group, Event evt)
        {
            if (!rect.Contains(evt.mousePosition))
                return false;

            var canDrop = string.IsNullOrEmpty(group.query);
            hotRect = rect;
            if (!canDrop)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    Undo.RegisterCompleteObjectUndo(SelectionGroupManager.instance, "Add to group");
                    group.Add(DragAndDrop.objectReferences);
                    hotRect = null;
                }
            }
            return true;
        }


        void ExitDrag()
        {
            hotRect = null;
        }


    }
}
