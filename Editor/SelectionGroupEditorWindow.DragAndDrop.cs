using Unity.SelectionGroups.Runtime;
using UnityEditor;
using UnityEngine;


namespace Unity.SelectionGroups
{
    public partial class SelectionGroupEditorWindow : EditorWindow
    {
        bool HandleGroupDragEvents(Rect rect, ISelectionGroup group)
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
        bool PerformDrag(Rect rect, ISelectionGroup group, Event evt)
        {
            if (!rect.Contains(evt.mousePosition))
                return false;

            var canDrop = string.IsNullOrEmpty(group.Query);
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
