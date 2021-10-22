using System.Collections.Generic;
using Unity.SelectionGroups;
using Unity.SelectionGroups.Runtime;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.SelectionGroupsEditor
{
    /// <summary>
    /// The Editor-only manager for selection groups.
    /// </summary>
    internal partial class SelectionGroupPersistenceManager : ScriptableObject
    {
        HashSet<GameObject> toDestroy = new HashSet<GameObject>();

        HashSet<EditorSelectionGroup> editorGroups = new HashSet<EditorSelectionGroup>();
        
        void OnEnable()
        {
            Instance = this;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;;
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
            SelectionGroupManager.Delete += OnDeleteSelectionGroup;
            SelectionGroupManager.Create += OnCreateSelectionGroup;
        }

        private void OnCreateSelectionGroup(SelectionGroupDataLocation scope, string name, string query, Color color, IList<Object> members)
        {
            switch (scope)
            {
                case SelectionGroupDataLocation.Editor:
                    CreateEditorSelectionGroup(name, query, color, members);
                    break;
                case SelectionGroupDataLocation.Scene:
                    CreateSceneSelectionGroup(name, query, color, members);
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(scope), scope, null);
            }
        }

        void CreateSceneSelectionGroup(string name, string query, Color color, IList<Object> members)
        {
            var g = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(g,"New Scene Selection Group");
            var group = g.AddComponent<Unity.SelectionGroups.Runtime.SelectionGroup>();
            group.Name = name;
            group.Query = query;
            group.Color = color;
            group.Scope = SelectionGroupDataLocation.Scene;
            group.ShowMembers = true;
            group.Add(members);
            SelectionGroupManager.Register(group);
        }

        void CreateEditorSelectionGroup(string name, string query, Color color, IList<Object> members)
        {
            var g = new EditorSelectionGroup
            {
                Name = name,
                Color = color,
                ShowMembers = true,
                Query = query
            };
            g.Add(members);
            Undo.RegisterCompleteObjectUndo(this, "New Editor Selection Group");
            editorGroups.Add(g);
            Save();
            SelectionGroupManager.Register(g);
        }

        static void OnUndoRedoPerformed()
        {
            
        }

        void OnDeleteSelectionGroup(ISelectionGroup group)
        {
            
            if (group is Unity.SelectionGroups.Runtime.SelectionGroup runtimeGroup)
                toDestroy.Add(runtimeGroup.gameObject);
            if (group is EditorSelectionGroup editorGroup)
            {
                Undo.RegisterCompleteObjectUndo(this, "Delete Editor Selection Group");
                editorGroups.Remove(editorGroup);
            }
            EditorApplication.delayCall += Save;
        }
        
        static void OnHierarchyChanged()
        {    
        } 

        static void OnUpdate()
        {
            foreach (var i in Instance.toDestroy)
            {
                if(i != null) Undo.DestroyObjectImmediate(i);
            }
            Instance.toDestroy.Clear();
        }

        void OnDisable()
        {
            Save();
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            EditorApplication.update -= OnUpdate;
        }

        public static void RegisterUndo(string msg)
        {
            Undo.RegisterCompleteObjectUndo(Instance, msg);
            EditorUtility.SetDirty(Instance);
        }

    }
}
