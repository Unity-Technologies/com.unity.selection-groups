using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.SelectionGroups.Editor
{
    public class GameObjectDropManipulator : PointerManipulator
    {
        public event System.Action<Object[]> OnDropObject;
        
        public GameObjectDropManipulator(System.Action<Object[]> onDropObject)
        {
            OnDropObject = onDropObject;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            target.RegisterCallback<DragPerformEvent>(OnDragPerform);
            target.RegisterCallback<DragEnterEvent>(OnDragEnter);
            target.RegisterCallback<DragExitedEvent>(OnDragExit);
            target.RegisterCallback<DragLeaveEvent>(OnDragLeave);
        }
        
        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<DragUpdatedEvent>(OnDragUpdated);
            target.UnregisterCallback<DragPerformEvent>(OnDragPerform);
            target.UnregisterCallback<DragEnterEvent>(OnDragEnter);
            target.UnregisterCallback<DragExitedEvent>(OnDragExit);
            target.UnregisterCallback<DragLeaveEvent>(OnDragLeave);
        }

        private void OnDragLeave(DragLeaveEvent evt)
        {
            target.RemoveFromClassList("Hover");
        }

        private void OnDragExit(DragExitedEvent evt)
        {
            target.RemoveFromClassList("Hover");
        }

        private void OnDragEnter(DragEnterEvent evt)
        {
            target.AddToClassList("Hover");
        }

        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.StopPropagation();
        }

        private void OnDragPerform(DragPerformEvent evt)
        {
            if (DragAndDrop.objectReferences.Length > 0)
            {
                DragAndDrop.AcceptDrag();
                evt.StopPropagation();
                target.RemoveFromClassList("Hover");
                OnDropObject?.Invoke(DragAndDrop.objectReferences);
            }
        }
    }
}