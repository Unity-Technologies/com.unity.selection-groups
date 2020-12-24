using Unity.SelectionGroups.Runtime;
using UnityEditor;
using UnityEngine;


namespace Unity.SelectionGroups
{
    public partial class SelectionGroupEditorWindow : EditorWindow
    {
        
        bool HandleDragEvents(Rect rect, ISelectionGroup group)
        {
            Event evt = Event.current;
            if (!rect.Contains(evt.mousePosition))
            {
                return false;
            }
                
            switch (evt.type)
            {
                case EventType.DragExited:
                case EventType.DragPerform:
                case EventType.DragUpdated:
                case EventType.MouseDrag:
                    Debug.Log($"{evt.type} {group.Name}");
                    break;
            }

            switch (evt.type)
            {
                case EventType.MouseDrag:
                    //This event occurs when dragging inside the EditorWindow which contains this OnGUI method.
                    //It would be better named DragStarted.
                    Debug.Log($"Start Drag: {group.Name}");
                    DragAndDrop.PrepareStartDrag();
                    if(hotMember != null)
                        DragAndDrop.objectReferences = new []{ hotMember };
                    else
                        DragAndDrop.objectReferences = Selection.objects;

                    DragAndDrop.StartDrag("Selection Group");
                    evt.Use();
                    break;
                 case EventType.DragExited: 
                     //This event occurs when MouseUp occurs, or the cursor leaves the EditorWindow.
                     ////The cursor may come back into the EditorWindow, however MouseDrag will not be triggered.
                     break;
                case EventType.DragUpdated:
                    //This event can occur ay any time. VisualMode must be assigned a value other than Rejected, else
                    //the DragPerform event will not be triggered.
                    var canDrop = string.IsNullOrEmpty(group.Query);
                    if (!canDrop)
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    else
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    evt.Use();
                    break;
                case EventType.DragPerform:
                    //This will only get called when a valid Drop occurs (determined by the above DragUpdated code)
                    DragAndDrop.AcceptDrag();
                    Undo.RegisterCompleteObjectUndo(SelectionGroupManager.instance, "Add to group");
                    SelectionGroupEvents.Add(SelectionGroupScope.Editor, group.GroupId, DragAndDrop.objectReferences);
                    hotRect = null;
                    evt.Use();
                    break;
            }
            return false;
        }

    }
}
