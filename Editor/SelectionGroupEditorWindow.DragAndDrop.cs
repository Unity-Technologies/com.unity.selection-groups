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
        public bool HandleGroupDragEvents(Rect rect, SelectionGroup group)
        {
            Event evt = Event.current;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!rect.Contains(evt.mousePosition))
                        return false;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    hotRect = rect;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        // Debug.Log($"Adding refs to group {position} {position.Contains(Event.current.mousePosition)}");
                        Undo.RegisterCompleteObjectUndo(SelectionGroupManager.instance, "Add to group");
                        group.Add(DragAndDrop.objectReferences);
                        hotRect = null;

                    }
                    break;
            }
            return false;
        }



        void ExitDrag()
        {
            hotRect = null;
        }


    }
}
