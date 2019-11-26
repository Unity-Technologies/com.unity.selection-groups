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
        bool HandleGroupDragEvents(Rect position, string groupName)
        {
            var e = Event.current;
            if (position.Contains(e.mousePosition))
            {
                switch (e.type)
                {
                    case EventType.DragUpdated:
                        UpdateDrag(position, groupName);
                        return true;
                    case EventType.DragPerform:
                        PerformDrag(position, groupName);
                        return true;
                }
            }
            return false;
        }

        void ExitDrag()
        {
            hotRect = null;
        }

        void UpdateDrag(Rect rect, string groupName)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            hotRect = rect;
            DragAndDrop.AcceptDrag();
        }

        void PerformDrag(Rect position, string groupName)
        {
            if (groupName == null)
            {
                CreateNewGroup(DragAndDrop.objectReferences);
            }
            else
            {
                SelectionGroupEditorUtility.RecordUndo("Add member to Group");
                SelectionGroupEditorUtility.AddObjectToGroup(DragAndDrop.objectReferences, groupName);
                MarkAllContainersDirty();
                hotRect = null;
            }
        }
    }
}
