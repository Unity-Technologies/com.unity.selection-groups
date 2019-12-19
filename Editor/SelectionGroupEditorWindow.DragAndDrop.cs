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
        //This is called once per group, every frame.
        bool HandleGroupDragEvents(Rect position, SelectionGroup group)
        {
            var e = Event.current;
            if (position.Contains(e.mousePosition))
            {
                switch (e.type)
                {
                    case EventType.DragUpdated:
                        UpdateDrag(position, group);
                        return true;
                    case EventType.DragPerform:
                        PerformDrag(position, group);
                        return true;
                }
            }
            return false;
        }

        void ExitDrag()
        {
            hotRect = null;
        }

        void UpdateDrag(Rect rect, SelectionGroup group)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            hotRect = rect;
            DragAndDrop.AcceptDrag();
        }

        void PerformDrag(Rect position, SelectionGroup group)
        {
            if (group == null)
            {
                CreateNewGroup(DragAndDrop.objectReferences);
            }
            else
            {
                Undo.RegisterCompleteObjectUndo(SelectionGroupManager.instance, "Add to group");
                group.AddRange(DragAndDrop.objectReferences);
                hotRect = null;
            }
        }
    }
}
